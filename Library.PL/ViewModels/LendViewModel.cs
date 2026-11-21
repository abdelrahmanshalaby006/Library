using System.ComponentModel.DataAnnotations;

namespace Library.PL.ViewModels
{
    public class LendViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a member")]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please choose a book")]
        [Display(Name = "Book")]
        public int BookId { get; set; }
    }
}
