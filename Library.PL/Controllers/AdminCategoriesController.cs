using Library.DAL.Entities;
using Library.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.PL.Controllers
{
    [Authorize(Roles = "Admin,Librarian")]
    [Route("/admin/categories")]
    public class AdminCategoriesController : Controller
    {
        private readonly ICategoryRepository categoryRepo;
        public AdminCategoriesController(ICategoryRepository _categoryRepo)
        {
            categoryRepo = _categoryRepo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(categoryRepo.GetAll());
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        public IActionResult Create(Category model)
        {
            if (categoryRepo.CheckNameExist(model.Name))
                ModelState.AddModelError("Name", "Duplicate Entry For Name");

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            categoryRepo.Add(model);
            categoryRepo.Save();
            TempData["Success"] = $"Category \"{model.Name}\" was added.";
            return RedirectToAction(nameof(Index));
        }
    }
}
