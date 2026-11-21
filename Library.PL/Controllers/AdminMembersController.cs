using Library.BLL.Services.Interfaces;
using Library.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.PL.Controllers
{
    [Authorize(Roles = "Admin,Librarian")]
    [Route("/admin/members")]
    public class AdminMembersController : Controller
    {
        private readonly IMemberService memberService;
        private readonly IBorrowingService borrowingService;

        public AdminMembersController(
            IMemberService _memberService,
            IBorrowingService _borrowingService)
        {
            memberService = _memberService;
            borrowingService = _borrowingService;
        }

        // /admin/members?search=ali
        [HttpGet]
        public IActionResult Index(string? search)
        {
            ViewBag.Search = search;
            return View(memberService.GetAll(search));
        }

        [HttpGet("details/{id:int}")]
        public IActionResult Details(int id)
        {
            var member = memberService.GetById(id);
            if (member == null) return NotFound();

            return View(new MemberOverviewViewModel
            {
                Member = member,
                Borrowings = borrowingService.GetByMember(id),
                UnpaidFines = borrowingService.GetUnpaidFines(id)
            });
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await memberService.CreateMemberAsync(
                model.FullName, model.Email, model.Password, model.PhoneNumber, model.Address);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = $"Member \"{model.FullName}\" was created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var member = memberService.GetById(id);
            if (member == null) return NotFound();

            return View(new MemberEditViewModel
            {
                Id = member.Id,
                MembershipNumber = member.MembershipNumber,
                FullName = member.FullName,
                PhoneNumber = member.PhoneNumber,
                Address = member.Address,
                IsActive = member.IsActive
            });
        }

        [HttpPost("edit/{id:int}")]
        public IActionResult Edit(int id, MemberEditViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var result = memberService.Update(id, model.FullName, model.PhoneNumber, model.Address, model.IsActive);
            if (!result.Success) return NotFound();

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
