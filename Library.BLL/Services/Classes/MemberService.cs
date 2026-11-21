using Library.BLL.Common;
using Library.BLL.Services.Interfaces;
using Library.DAL.Entities;
using Library.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Library.BLL.Services.Classes
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository memberRepo;
        private readonly UserManager<ApplicationUser> userManager;

        public MemberService(IMemberRepository _memberRepo, UserManager<ApplicationUser> _userManager)
        {
            memberRepo = _memberRepo;
            userManager = _userManager;
        }

        public async Task<ServiceResult> CreateMemberAsync(string fullName, string email, string password, string? phone, string? address)
        {
            if (await userManager.FindByEmailAsync(email) != null)
                return ServiceResult.Fail("This email is already registered.");

            // 1) الحساب
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                return ServiceResult.Fail(string.Join(" | ", createResult.Errors.Select(e => e.Description)));

            // 2) الدور
            await userManager.AddToRoleAsync(user, "Member");

            // 3) سجل العضوية
            try
            {
                var member = new Member
                {
                    MembershipNumber = memberRepo.GetNextMembershipNumber(),
                    FullName = fullName,
                    PhoneNumber = phone,
                    Address = address,
                    ApplicationUserId = user.Id
                };
                memberRepo.Add(member);
                memberRepo.Save();
            }
            catch
            {
                // لو حصل خطأ نمسح الحساب عشان ما يفضلش حساب من غير عضوية
                await userManager.DeleteAsync(user);
                return ServiceResult.Fail("Could not create the member. Please try again.");
            }

            return ServiceResult.Ok("Member created successfully.");
        }

        public List<Member> GetAll(string? search)
        {
            var members = memberRepo.GetAllWithUser();
            if (!string.IsNullOrWhiteSpace(search))
            {
                members = members.Where(m =>
                    m.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    m.MembershipNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (m.ApplicationUser.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return members;
        }

        public Member? GetById(int id) => memberRepo.GetByIdWithUser(id);

        public Member? GetByUserId(string userId) => memberRepo.GetByUserId(userId);

        public ServiceResult Update(int id, string fullName, string? phone, string? address, bool isActive)
        {
            var member = memberRepo.GetById(id);
            if (member == null) return ServiceResult.Fail("Member not found.");

            member.FullName = fullName;
            member.PhoneNumber = phone;
            member.Address = address;
            member.IsActive = isActive;
            memberRepo.Save();
            return ServiceResult.Ok("Member updated.");
        }
    }
}
