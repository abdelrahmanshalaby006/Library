using Library.DAL.Repositories.Interfaces;
using Library.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Library.PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBookRepository bookRepo;
        private readonly ICategoryRepository categoryRepo;
        public HomeController(IBookRepository _bookRepo, ICategoryRepository _categoryRepo)
        {
            bookRepo = _bookRepo;
            categoryRepo = _categoryRepo;
        }

        public IActionResult Index()
        {
            var books = bookRepo.GetAll();
            var categories = categoryRepo.GetAll();
            ViewBag.BooksCount = books.Count;
            ViewBag.CategoriesCount = categories.Count;
            ViewBag.Categories = categories;
            var latest = books.OrderByDescending(b => b.Id).Take(4).ToList();
            return View(BookRowViewModel.From(latest, categories));
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
