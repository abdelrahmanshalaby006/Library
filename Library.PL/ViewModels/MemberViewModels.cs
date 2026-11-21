using System.ComponentModel.DataAnnotations;
using Library.DAL.Entities;

namespace Library.PL.ViewModels
{
    public class MemberEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Membership number")]
        public string MembershipNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Phone")]
        public string? PhoneNumber { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [Display(Name = "Active membership")]
        public bool IsActive { get; set; } = true;
    }

    // صفحة العضو (My Library) وصفحة تفاصيل العضو عند الموظف
    public class MemberOverviewViewModel
    {
        public Member Member { get; set; } = null!;
        public List<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
        public decimal UnpaidFines { get; set; }
    }
}
