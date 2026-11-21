using System.ComponentModel.DataAnnotations;

namespace Library.DAL.Entities
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required")]
        [StringLength(80)]
        public string Author { get; set; } = string.Empty;

        [Display(Name = "Published Year")]
        [Range(1000, 2100, ErrorMessage = "Enter a valid year")]
        public int Year { get; set; } = DateTime.Now.Year;

        // إجمالي النسخ اللي المكتبة تملكها
        [Display(Name = "Total Copies")]
        [Range(0, 1000, ErrorMessage = "Copies must be between 0 and 1000")]
        public int Copies { get; set; } = 1;

        // النسخ المتاحة دلوقتي (مش مستعارة). الكود هو اللي بيعدّلها مش المستخدم.
        public int AvailableCopies { get; set; }

        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a category")]
        public int CategoryId { get; set; }

        // Nullable عشان الفورم مش بيبعت Category كاملة، فمينفعش تتعامل كـ Required.
        public Category? Category { get; set; }
    }
}
