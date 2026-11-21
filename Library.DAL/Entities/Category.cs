using System.ComponentModel.DataAnnotations;

namespace Library.DAL.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter category name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        public List<Book> Books { get; set; } = new List<Book>();
    }
}
