using Library.DAL.Entities;

namespace Library.PL.ViewModels
{
    // كتاب + اسم التصنيف (للجداول والكروت)
    public class BookRowViewModel
    {
        public Book Book { get; set; } = new Book();
        public string CategoryName { get; set; } = string.Empty;

        public static List<BookRowViewModel> From(List<Book> books, List<Category> categories)
        {
            return books.Select(b => new BookRowViewModel
            {
                Book = b,
                CategoryName = categories.FirstOrDefault(c => c.Id == b.CategoryId)?.Name ?? "-"
            }).ToList();
        }
    }
}
