using Library.DAL.Entities;

namespace Library.DAL.Repositories.Interfaces
{
    public interface IBorrowingRepository : IGenericRepository<Borrowing>
    {
        List<Borrowing> GetAllDetailed();                       // مع الكتاب والعضو
        List<Borrowing> GetByMemberDetailed(int memberId);
        Borrowing? GetByIdDetailed(int id);

        int CountOpenByMember(int memberId);                    // الطلبات المعلّقة + المستعارة حالياً
        bool HasOpenFor(int memberId, int bookId);              // العضو معاه الكتاب ده أو طالبه؟
        bool HasOverdue(int memberId);                          // عنده كتاب متأخر؟
        decimal GetUnpaidFines(int memberId);                   // مجموع الغرامات غير المدفوعة
    }
}
