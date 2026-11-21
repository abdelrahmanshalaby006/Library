using Library.DAL.Entities;
using Library.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.PL.Controllers
{
    // إدارة حسابات الموظفين (Admin / Librarian). للـ Admin بس.
    [Authorize(Roles = "Admin")]
    [Route("/admin/staff")]
    public class AdminStaffController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        public AdminStaffController(UserManager<ApplicationUser> _userManager)
        {
            userManager = _userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rows = new List<StaffRowViewModel>();
            foreach (var role in new[] { "Admin", "Librarian" })
            {
                var users = await userManager.GetUsersInRoleAsync(role);
                rows.AddRange(users.Select(u => new StaffRowViewModel { Email = u.Email ?? "", Role = role }));
            }
            return View(rows);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new StaffCreateViewModel());
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(StaffCreateViewModel model)
        {
            if (model.Role != "Admin" && model.Role != "Librarian")
                ModelState.AddModelError("Role", "Invalid role.");

            if (!ModelState.IsValid) return View(model);

            if (await userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await userManager.AddToRoleAsync(user, model.Role);
            TempData["Success"] = $"{model.Role} account \"{model.Email}\" was created.";
            return RedirectToAction(nameof(Index));
        }
    }
}
