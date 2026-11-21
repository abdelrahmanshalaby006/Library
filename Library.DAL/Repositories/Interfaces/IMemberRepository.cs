using Library.DAL.Entities;

namespace Library.DAL.Repositories.Interfaces
{
    public interface IMemberRepository : IGenericRepository<Member>
    {
        Member? GetByUserId(string userId);          // العضو بتاع الحساب ده
        Member? GetByIdWithUser(int id);             // العضو + حسابه (عشان الإيميل)
        List<Member> GetAllWithUser();
        string GetNextMembershipNumber();            // MEM-00001, MEM-00002, ...
    }
}
