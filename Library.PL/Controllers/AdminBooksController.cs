using Library.DAL.Entities;
using Library.DAL.Repositories.Interfaces;
using Library.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.PL.Controllers
{
    [Authorize(Roles = "Admin,Librarian")]
    [Route("/admin/books")]
    public class AdminBooksController : Controller
    {
        private readonly IBookRepository bookRepo;
        private readonly ICategoryRepository categoryRepo;
        public AdminBooksController(IBookRepository _bookRepo, ICategoryRepository _categoryRepo)
        {
            bookRepo = _bookRepo;
            categoryRepo = _categoryRepo;
        }

        // /admin/books   --> READ (all)
        [HttpGet]
        public IActionResult Index()
        {
            var books = bookRepo.GetAll();
            var categories = categoryRepo.GetAll();
            return View(BookRowViewModel.From(books, categories));
        }

        // /admin/books/details/5   --> READ (one)
        [HttpGet("details/{id:int}")]
        public IActionResult Details(int id)
        {
            var book = bookRepo.GetById(id);
            if (book == null) return NotFound();
            ViewBag.CategoryName = categoryRepo.GetById(book.CategoryId)?.Name ?? "-";
            return View(book);
        }

        // /admin/books/create   --> CREATE (form)
        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.Categories = categoryRepo.GetAll();
            return View(new Book());
        }

        // CREATE (save)
        [HttpPost("create")]
        public IActionResult Create(Book model)
        {
            if (bookRepo.CheckBookExist(model.Title, model.Author))
                ModelState.AddModelError("Title", "This book already exists for the same author");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = categoryRepo.GetAll();
                return View(model);
            }

            model.AvailableCopies = model.Copies;   // كل النسخ متاحة في البداية
            bookRepo.Add(model);
            bookRepo.Save();
            TempData["Success"] = $"Book \"{model.Title}\" was added.";
            return RedirectToAction(nameof(Index));
        }

        // /admin/books/edit/5   --> UPDATE (form)
        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var book = bookRepo.GetById(id);
            if (book == null) return NotFound();
            ViewBag.Categories = categoryRepo.GetAll();
            return View(book);
        }

        // UPDATE (save)
        [HttpPost("edit/{id:int}")]
        public IActionResult Edit(int id, Book model)
        {
            if (id != model.Id) return BadRequest();

            var book = bookRepo.GetById(id);
            if (book == null) return NotFound();

            if (bookRepo.CheckBookExist(model.Title, model.Author, model.Id))
                ModelState.AddModelError("Title", "This book already exists for the same author");

            // النسخ المستعارة حالياً = الإجمالي - المتاح. مينفعش الإجمالي الجديد يبقى أقل منها.
            int borrowedNow = book.Copies - book.AvailableCopies;
            if (model.Copies < borrowedNow)
                ModelState.AddModelError("Copies", $"{borrowedNow} copy(ies) are currently borrowed, so total copies can't be less than that.");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = categoryRepo.GetAll();
                return View(model);
            }

            // بنعدّل الكتاب الأصلي من الداتابيز (مش اللي جاي من الفورم) عشان نحافظ على AvailableCopies
            book.Title = model.Title;
            book.Author = model.Author;
            book.Year = model.Year;
            book.CategoryId = model.CategoryId;
            book.Copies = model.Copies;
            book.AvailableCopies = model.Copies - borrowedNow;
            bookRepo.Save();

            TempData["Success"] = $"Book \"{book.Title}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // /admin/books/delete/5   --> DELETE (confirmation page)
        [HttpGet("delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            var book = bookRepo.GetById(id);
            if (book == null) return NotFound();
            ViewBag.CategoryName = categoryRepo.GetById(book.CategoryId)?.Name ?? "-";
            ViewBag.HasHistory = bookRepo.HasHistory(id);
            return View(book);
        }

        // DELETE (really delete)
        [HttpPost("delete/{id:int}")]
        public IActionResult DeleteConfirmed(int id)
        {
            var book = bookRepo.GetById(id);
            if (book == null) return NotFound();

            // الكتاب اللي عليه استعارات ممنوع يتمسح (بنحافظ على السجل)
            if (bookRepo.HasHistory(id))
            {
                TempData["Error"] = "This book has borrowing history and can't be deleted. Set its copies to 0 instead.";
                return RedirectToAction(nameof(Index));
            }

            bookRepo.Delete(book);
            bookRepo.Save();
            TempData["Success"] = $"Book \"{book.Title}\" was deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
