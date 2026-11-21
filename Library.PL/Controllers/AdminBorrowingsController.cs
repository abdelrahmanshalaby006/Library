using Library.BLL.Common;
using Library.BLL.Services.Interfaces;
using Library.DAL.Repositories.Interfaces;
using Library.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.PL.Controllers
{
    // Use case 12: Manage Borrowing - الطلبات + الموافقة/الرفض + التسليم المباشر
    [Authorize(Roles = "Admin,Librarian")]
    [Route("/admin/borrowings")]
    public class AdminBorrowingsController : Controller
    {
        private readonly IBorrowingService borrowingService;
        private readonly IMemberService memberService;
        private readonly IBookRepository bookRepo;

        public AdminBorrowingsController(
            IBorrowingService _borrowingService,
            IMemberService _memberService,
            IBookRepository _bookRepo)
        {
            borrowingService = _borrowingService;
            memberService = _memberService;
            bookRepo = _bookRepo;
        }

        private IActionResult Back(ServiceResult result)
        {
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        private void LoadLists()
        {
            ViewBag.Members = memberService.GetAll(null).Where(m => m.IsActive).ToList();
            ViewBag.Books = bookRepo.GetAll().Where(b => b.AvailableCopies > 0).ToList();
        }

        // /admin/borrowings?status=all|pending|borrowed
        [HttpGet]
        public IActionResult Index(string? status)
        {
            var current = string.IsNullOrWhiteSpace(status) ? "all" : status.ToLower();
            ViewBag.Status = current;

            var rows = current switch
            {
                "pending" => borrowingService.GetAll("pending"),
                "borrowed" => borrowingService.GetAll("borrowed"),
                "rejected" => borrowingService.GetAll("all").Where(b => b.Status == Library.DAL.Entities.BorrowingStatus.Rejected
                                    || b.Status == Library.DAL.Entities.BorrowingStatus.Cancelled).ToList(),
                _ => borrowingService.GetAll("all")
            };

            ViewBag.PendingCount = borrowingService.GetAll("pending").Count;
            ViewBag.BorrowedCount = borrowingService.GetAll("borrowed").Count;
            return View(rows);
        }

        // تسليم كتاب لعضو مباشرة
        [HttpGet("lend")]
        public IActionResult Lend(int? memberId, int? bookId)
        {
            LoadLists();
            return View(new LendViewModel { MemberId = memberId ?? 0, BookId = bookId ?? 0 });
        }

        [HttpPost("lend")]
        public IActionResult Lend(LendViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = borrowingService.Lend(model.MemberId, model.BookId);
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            LoadLists();
            return View(model);
        }

        [HttpPost("approve/{id:int}")]
        public IActionResult Approve(int id) => Back(borrowingService.Approve(id));

        [HttpPost("reject/{id:int}")]
        public IActionResult Reject(int id) => Back(borrowingService.Reject(id));
    }
}
