using Library.DAL.Contexts;
using Library.DAL.Entities;
using Library.DAL.Repositories.Interfaces;

namespace Library.DAL.Repositories.Classes
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {
        public BookRepository(AppDbContext _context) : base(_context)
        {
        }

        public bool CheckBookExist(string title, string author, int ignoreId = 0)
        {
            return context.Books.Any(b => b.Title == title && b.Author == author && b.Id != ignoreId);
        }

        public bool HasHistory(int bookId)
        {
            return context.Borrowings.Any(b => b.BookId == bookId);
        }
    }
}
