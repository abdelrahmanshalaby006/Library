using Library.BLL.Common;
using Library.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.PL.Controllers
{
    // Use case 13: Manage Returns - تأكيد الإرجاع + تحصيل الغرامات
    [Authorize(Roles = "Admin,Librarian")]
    [Route("/admin/returns")]
    public class AdminReturnsController : Controller
    {
        private readonly IBorrowingService borrowingService;

        public AdminReturnsController(IBorrowingService _borrowingService)
        {
            borrowingService = _borrowingService;
        }

        // /admin/returns?filter=active|overdue|fines|returned
        [HttpGet]
        public IActionResult Index(string? filter)
        {
            var current = string.IsNullOrWhiteSpace(filter) ? "active" : filter.ToLower();
            ViewBag.Filter = current;

            var status = current switch
            {
                "overdue" => "overdue",
                "fines" => "fines",
                "returned" => "returned",
                _ => "borrowed"
            };
            return View(borrowingService.GetAll(status));
        }

        [HttpPost("return/{id:int}")]
        public IActionResult Return(int id) => Back(borrowingService.Return(id));

        [HttpPost("pay-fine/{id:int}")]
        public IActionResult PayFine(int id) => Back(borrowingService.PayFine(id));

        private IActionResult Back(ServiceResult result)
        {
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
