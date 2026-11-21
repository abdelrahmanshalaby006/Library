using Library.BLL.Common;
using Library.DAL.Entities;

namespace Library.BLL.Services.Interfaces
{
    public interface IMemberService
    {
        // بتعمل ApplicationUser + Role "Member" + سجل Member وتربطهم ببعض
        Task<ServiceResult> CreateMemberAsync(string fullName, string email, string password, string? phone, string? address);

        List<Member> GetAll(string? search);
        Member? GetById(int id);
        Member? GetByUserId(string userId);
        ServiceResult Update(int id, string fullName, string? phone, string? address, bool isActive);
    }
}
