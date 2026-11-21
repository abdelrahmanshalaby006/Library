using Library.BLL.Common;
using Library.DAL.Entities;

namespace Library.BLL.Services.Interfaces
{
    public interface IBorrowingService
    {
        // العضو بيطلب استعارة (بتفضل Pending لحد موافقة الموظف)
        ServiceResult RequestBorrow(int memberId, int bookId);

        // الموظف بيسلّم الكتاب مباشرة من غير طلب
        ServiceResult Lend(int memberId, int bookId);

        ServiceResult Approve(int borrowingId);                   // موافقة على طلب
        ServiceResult Reject(int borrowingId);                    // رفض طلب
        ServiceResult Cancel(int borrowingId, int memberId);      // العضو يلغي طلبه
        ServiceResult Return(int borrowingId);                    // تأكيد الإرجاع + حساب الغرامة
        ServiceResult PayFine(int borrowingId);                   // تسجيل دفع الغرامة كاش

        List<Borrowing> GetAll(string? status);                   // all / pending / borrowed / overdue / returned / fines
        List<Borrowing> GetByMember(int memberId);
        decimal GetUnpaidFines(int memberId);
    }
}
