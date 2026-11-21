using Library.BLL.Common;
using Library.BLL.Services.Interfaces;
using Library.DAL.Entities;
using Library.DAL.Repositories.Interfaces;

namespace Library.BLL.Services.Classes
{
    public class BorrowingService : IBorrowingService
    {
        private readonly IBorrowingRepository borrowingRepo;
        private readonly IBookRepository bookRepo;
        private readonly IMemberRepository memberRepo;

        public BorrowingService(
            IBorrowingRepository _borrowingRepo,
            IBookRepository _bookRepo,
            IMemberRepository _memberRepo)
        {
            borrowingRepo = _borrowingRepo;
            bookRepo = _bookRepo;
            memberRepo = _memberRepo;
        }

        // ---------- قواعد مشتركة ----------

        // هل العضو مسموحله يستعير أصلاً؟ (نشط + مفيش تأخير + مفيش غرامات)
        private ServiceResult CheckMember(Member member)
        {
            if (!member.IsActive)
                return ServiceResult.Fail("This membership is not active.");
            if (borrowingRepo.HasOverdue(member.Id))
                return ServiceResult.Fail("There is an overdue book. It must be returned first.");

            var fines = borrowingRepo.GetUnpaidFines(member.Id);
            if (fines > 0)
                return ServiceResult.Fail($"There are unpaid fines ({fines} {LibraryRules.Currency}). Please pay them first.");

            return ServiceResult.Ok();
        }

        // قواعد الطلب الجديد: نفس القواعد + مفيش تكرار + مفيش تعدّي للحد الأقصى
        private ServiceResult CheckNewLoan(Member? member, Book? book)
        {
            if (member == null) return ServiceResult.Fail("Member not found.");
            if (book == null) return ServiceResult.Fail("Book not found.");

            var memberCheck = CheckMember(member);
            if (!memberCheck.Success) return memberCheck;

            if (borrowingRepo.HasOpenFor(member.Id, book.Id))
                return ServiceResult.Fail("This member already has this book (or a pending request for it).");

            if (borrowingRepo.CountOpenByMember(member.Id) >= LibraryRules.MaxOpenLoans)
                return ServiceResult.Fail($"Maximum of {LibraryRules.MaxOpenLoans} open loans/requests reached.");

            if (book.AvailableCopies <= 0)
                return ServiceResult.Fail("No copies are available right now.");

            return ServiceResult.Ok();
        }

        // تحويل السجل لـ "مستعار": تاريخ + ميعاد إرجاع + إنقاص نسخة
        private static void StartLoan(Borrowing borrowing, Book book)
        {
            borrowing.Status = BorrowingStatus.Borrowed;
            borrowing.BorrowedAt = DateTime.Now;
            borrowing.DueDate = DateTime.Now.Date.AddDays(LibraryRules.LoanDays);
            book.AvailableCopies--;
        }

        // ---------- العمليات ----------

        public ServiceResult RequestBorrow(int memberId, int bookId)
        {
            var member = memberRepo.GetById(memberId);
            var book = bookRepo.GetById(bookId);

            var check = CheckNewLoan(member, book);
            if (!check.Success) return check;

            borrowingRepo.Add(new Borrowing { MemberId = memberId, BookId = bookId });
            borrowingRepo.Save();
            return ServiceResult.Ok($"Your request for \"{book!.Title}\" was sent. A librarian will review it.");
        }

        public ServiceResult Lend(int memberId, int bookId)
        {
            var member = memberRepo.GetById(memberId);
            var book = bookRepo.GetById(bookId);

            var check = CheckNewLoan(member, book);
            if (!check.Success) return check;

            var borrowing = new Borrowing { MemberId = memberId, BookId = bookId };
            StartLoan(borrowing, book!);
            borrowingRepo.Add(borrowing);
            borrowingRepo.Save();   // Save واحدة بتحفظ السجل + نقص النسخة مع بعض
            return ServiceResult.Ok($"\"{book!.Title}\" was lent to {member!.FullName}. Due date: {borrowing.DueDate:yyyy-MM-dd}.");
        }

        public ServiceResult Approve(int borrowingId)
        {
            var borrowing = borrowingRepo.GetByIdDetailed(borrowingId);
            if (borrowing == null) return ServiceResult.Fail("Request not found.");
            if (borrowing.Status != BorrowingStatus.Pending)
                return ServiceResult.Fail("Only pending requests can be approved.");

            var memberCheck = CheckMember(borrowing.Member);
            if (!memberCheck.Success) return memberCheck;

            if (borrowing.Book.AvailableCopies <= 0)
                return ServiceResult.Fail("No copies available right now.");

            StartLoan(borrowing, borrowing.Book);
            borrowingRepo.Save();
            return ServiceResult.Ok($"Approved. Due date: {borrowing.DueDate:yyyy-MM-dd}.");
        }

        public ServiceResult Reject(int borrowingId)
        {
            var borrowing = borrowingRepo.GetById(borrowingId);
            if (borrowing == null) return ServiceResult.Fail("Request not found.");
            if (borrowing.Status != BorrowingStatus.Pending)
                return ServiceResult.Fail("Only pending requests can be rejected.");

            borrowing.Status = BorrowingStatus.Rejected;
            borrowingRepo.Save();
            return ServiceResult.Ok("Request rejected.");
        }

        public ServiceResult Cancel(int borrowingId, int memberId)
        {
            var borrowing = borrowingRepo.GetById(borrowingId);
            if (borrowing == null || borrowing.MemberId != memberId)
                return ServiceResult.Fail("Request not found.");
            if (borrowing.Status != BorrowingStatus.Pending)
                return ServiceResult.Fail("Only pending requests can be cancelled.");

            borrowing.Status = BorrowingStatus.Cancelled;
            borrowingRepo.Save();
            return ServiceResult.Ok("Request cancelled.");
        }

        public ServiceResult Return(int borrowingId)
        {
            var borrowing = borrowingRepo.GetByIdDetailed(borrowingId);
            if (borrowing == null) return ServiceResult.Fail("Record not found.");
            if (borrowing.Status != BorrowingStatus.Borrowed)
                return ServiceResult.Fail("This book is not currently borrowed.");

            borrowing.ReturnedAt = DateTime.Now;
            borrowing.Status = BorrowingStatus.Returned;

            // الغرامة = أيام التأخير x الغرامة اليومية
            borrowing.FineAmount = borrowing.OverdueDays * LibraryRules.FinePerDay;
            borrowing.FineStatus = borrowing.FineAmount > 0 ? FineStatus.Unpaid : FineStatus.None;

            if (borrowing.Book.AvailableCopies < borrowing.Book.Copies)
                borrowing.Book.AvailableCopies++;

            borrowingRepo.Save();

            if (borrowing.FineAmount > 0)
                return ServiceResult.Ok($"{borrowing.Book.Title} returned {borrowing.OverdueDays} day(s) late. Fine: {borrowing.FineAmount} {LibraryRules.Currency}.");
            return ServiceResult.Ok($"{borrowing.Book.Title} returned on time. Thank you!");
        }

        public ServiceResult PayFine(int borrowingId)
        {
            var borrowing = borrowingRepo.GetById(borrowingId);
            if (borrowing == null) return ServiceResult.Fail("Record not found.");
            if (borrowing.FineStatus != FineStatus.Unpaid)
                return ServiceResult.Fail("There is no unpaid fine on this record.");

            borrowing.FineStatus = FineStatus.Paid;
            borrowingRepo.Save();
            return ServiceResult.Ok($"Fine of {borrowing.FineAmount} {LibraryRules.Currency} marked as paid.");
        }

        // ---------- الاستعلامات ----------

        public List<Borrowing> GetAll(string? status)
        {
            var list = borrowingRepo.GetAllDetailed();
            switch (status?.ToLower())
            {
                case "pending": return list.Where(b => b.Status == BorrowingStatus.Pending).ToList();
                case "borrowed": return list.Where(b => b.Status == BorrowingStatus.Borrowed).ToList();
                case "overdue": return list.Where(b => b.IsOverdue).ToList();
                case "returned": return list.Where(b => b.Status == BorrowingStatus.Returned).ToList();
                case "fines": return list.Where(b => b.FineStatus == FineStatus.Unpaid).ToList();
                default: return list;
            }
        }

        public List<Borrowing> GetByMember(int memberId) => borrowingRepo.GetByMemberDetailed(memberId);

        public decimal GetUnpaidFines(int memberId) => borrowingRepo.GetUnpaidFines(memberId);
    }
}
