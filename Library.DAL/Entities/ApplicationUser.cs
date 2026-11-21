using Microsoft.AspNetCore.Identity;

namespace Library.DAL.Entities
{
    // حساب تسجيل الدخول (الإيميل + الباسورد + الأدوار). موروث من IdentityUser.
    // الموظف (Admin / Librarian) عنده ApplicationUser بس. العضو عنده ApplicationUser + Member.
    public class ApplicationUser : IdentityUser
    {
        public Member? Member { get; set; }
    }
}
