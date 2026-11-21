using System.Security.Claims;
using Library.BLL.Common;
using Library.BLL.Services.Interfaces;
using Library.DAL.Entities;
using Library.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.PL.Controllers
{
    // Use cases 6, 7, 8: طلب استعارة / إلغاء الطلب / صفحة العضو
    [Authorize(Roles = "Member")]
    [Route("/my")]
    public class MyLibraryController : Controller
    {
        private readonly IMemberService memberService;
        private readonly IBorrowingService borrowingService;

        public MyLibraryController(
            IMemberService _memberService,
            IBorrowingService _borrowingService)
        {
            memberService = _memberService;
            borrowingService = _borrowingService;
        }

        // العضو المرتبط بالحساب اللي مسجل دخول دلوقتي
        private Member? CurrentMember()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId == null ? null : memberService.GetByUserId(userId);
        }

        private IActionResult Back(ServiceResult result, string action = "Index", int? bookId = null)
        {
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            if (bookId != null)
                return RedirectToAction("Details", "Books", new { id = bookId });
            return RedirectToAction(action);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var member = CurrentMember();
            if (member == null) return Forbid();

            var vm = new MemberOverviewViewModel
            {
                Member = member,
                Borrowings = borrowingService.GetByMember(member.Id),
                UnpaidFines = borrowingService.GetUnpaidFines(member.Id)
            };
            return View(vm);
        }

        [HttpPost("borrow/{bookId:int}")]
        public IActionResult Borrow(int bookId)
        {
            var member = CurrentMember();
            if (member == null) return Forbid();
            var result = borrowingService.RequestBorrow(member.Id, bookId);
            return result.Success ? Back(result) : Back(result, bookId: bookId);
        }

        [HttpPost("cancel-request/{id:int}")]
        public IActionResult CancelRequest(int id)
        {
            var member = CurrentMember();
            if (member == null) return Forbid();
            return Back(borrowingService.Cancel(id, member.Id));
        }
    }
}
