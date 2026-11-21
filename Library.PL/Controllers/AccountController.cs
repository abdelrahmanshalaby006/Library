using Library.BLL.Services.Interfaces;
using Library.DAL.Entities;
using Library.PL.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.PL.Controllers
{
    [Route("/account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IMemberService memberService;

        public AccountController(
            UserManager<ApplicationUser> _userManager,
            SignInManager<ApplicationUser> _signInManager,
            IMemberService _memberService)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            memberService = _memberService;
        }

        // ---------- Login ----------
        [HttpGet("login")]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var result = await signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account locked for 5 minutes after too many failed attempts.");
                return View(model);
            }
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // رجّعه للصفحة اللي كان رايحلها (لو رابط داخلي بس)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // الموظف للوحة التحكم، والعضو لصفحته
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null && await userManager.IsInRoleAsync(user, "Member"))
                return RedirectToAction("Index", "MyLibrary");

            return RedirectToAction("Index", "AdminBooks");
        }

        // ---------- Register (للأعضاء بس) ----------
        [HttpGet("register")]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // الدور ثابت "Member": المستخدم مايقدرش يختار Admin أو Librarian من هنا
            var result = await memberService.CreateMemberAsync(
                model.FullName, model.Email, model.Password, model.PhoneNumber, model.Address);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
                await signInManager.SignInAsync(user, isPersistent: false);

            TempData["Success"] = "Welcome! Your membership was created.";
            return RedirectToAction("Index", "MyLibrary");
        }

        // ---------- Logout ----------
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
