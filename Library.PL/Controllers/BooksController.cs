using Library.DAL.Repositories.Interfaces;
using Library.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Library.PL.Controllers
{
    // الجانب العام للموقع: أي زائر يقدر يتصفح ويبحث
    [Route("/books")]
    public class BooksController : Controller
    {
        private readonly IBookRepository bookRepo;
        private readonly ICategoryRepository categoryRepo;
        public BooksController(IBookRepository _bookRepo, ICategoryRepository _categoryRepo)
        {
            bookRepo = _bookRepo;
            categoryRepo = _categoryRepo;
        }

        // /books?search=clean&categoryId=1
        [HttpGet]
        public IActionResult Index(string? search, int? categoryId)
        {
            var books = bookRepo.GetAll();
            var categories = categoryRepo.GetAll();

            if (!string.IsNullOrWhiteSpace(search))
            {
                books = books.Where(b => b.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                                      || b.Author.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                books = books.Where(b => b.CategoryId == categoryId.Value).ToList();
            }

            ViewBag.Categories = categories;
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            return View(BookRowViewModel.From(books, categories));
        }

        // /books/details/5
        [HttpGet("details/{id:int}")]
        public IActionResult Details(int id)
        {
            var book = bookRepo.GetById(id);
            if (book == null) return NotFound();
            ViewBag.CategoryName = categoryRepo.GetById(book.CategoryId)?.Name ?? "-";
            return View(book);
        }
    }
}
