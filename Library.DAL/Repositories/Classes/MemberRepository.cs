using Library.DAL.Contexts;
using Library.DAL.Entities;
using Library.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Repositories.Classes
{
    public class MemberRepository : GenericRepository<Member>, IMemberRepository
    {
        public MemberRepository(AppDbContext _context) : base(_context)
        {
        }

        public Member? GetByUserId(string userId)
        {
            return context.Members
                .Include(m => m.ApplicationUser)
                .FirstOrDefault(m => m.ApplicationUserId == userId);
        }

        public Member? GetByIdWithUser(int id)
        {
            return context.Members
                .Include(m => m.ApplicationUser)
                .FirstOrDefault(m => m.Id == id);
        }

        public List<Member> GetAllWithUser()
        {
            return context.Members
                .Include(m => m.ApplicationUser)
                .OrderBy(m => m.Id)
                .ToList();
        }

        public string GetNextMembershipNumber()
        {
            var max = context.Members.Max(m => (int?)m.Id) ?? 0;
            return "MEM-" + (max + 1).ToString("D5");
        }
    }
}
