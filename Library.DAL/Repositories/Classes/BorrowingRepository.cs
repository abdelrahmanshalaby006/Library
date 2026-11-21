using Library.DAL.Contexts;
using Library.DAL.Entities;
using Library.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Repositories.Classes
{
    public class BorrowingRepository : GenericRepository<Borrowing>, IBorrowingRepository
    {
        public BorrowingRepository(AppDbContext _context) : base(_context)
        {
        }

        public List<Borrowing> GetAllDetailed()
        {
            return context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.Member)
                .OrderByDescending(b => b.Id)
                .ToList();
        }

        public List<Borrowing> GetByMemberDetailed(int memberId)
        {
            return context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.Member)
                .Where(b => b.MemberId == memberId)
                .OrderByDescending(b => b.Id)
                .ToList();
        }

        public Borrowing? GetByIdDetailed(int id)
        {
            return context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.Member)
                .FirstOrDefault(b => b.Id == id);
        }

        public int CountOpenByMember(int memberId)
        {
            return context.Borrowings.Count(b => b.MemberId == memberId &&
                (b.Status == BorrowingStatus.Pending || b.Status == BorrowingStatus.Borrowed));
        }

        public bool HasOpenFor(int memberId, int bookId)
        {
            return context.Borrowings.Any(b => b.MemberId == memberId && b.BookId == bookId &&
                (b.Status == BorrowingStatus.Pending || b.Status == BorrowingStatus.Borrowed));
        }

        public bool HasOverdue(int memberId)
        {
            var today = DateTime.Now.Date;
            return context.Borrowings.Any(b => b.MemberId == memberId &&
                b.Status == BorrowingStatus.Borrowed && b.DueDate < today);
        }

        public decimal GetUnpaidFines(int memberId)
        {
            return context.Borrowings
                .Where(b => b.MemberId == memberId && b.FineStatus == FineStatus.Unpaid)
                .Sum(b => (decimal?)b.FineAmount) ?? 0m;
        }
    }
}
