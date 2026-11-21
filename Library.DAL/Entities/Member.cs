using System.ComponentModel.DataAnnotations;

namespace Library.DAL.Entities
{
    // بيانات العضوية داخل المكتبة. مرتبط بحساب ApplicationUser (One-to-One).
    public class Member
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string MembershipNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public List<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
    }
}
