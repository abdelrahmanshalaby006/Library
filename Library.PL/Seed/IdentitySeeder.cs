using Library.BLL.Services.Interfaces;
using Library.DAL.Contexts;
using Library.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace Library.PL.Seed
{
    // بيشتغل مرة عند تشغيل المشروع، وآمن لو اتشغل أكتر من مرة (مش بيكرر حاجة).
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var memberService = scope.ServiceProvider.GetRequiredService<IMemberService>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1) الأدوار
            foreach (var role in new[] { "Admin", "Librarian", "Member" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2) حسابات البداية (من appsettings.Development.json أو User Secrets)
            await EnsureStaffAsync(userManager, configuration.GetSection("SeedAdmin"), "Admin");
            await EnsureStaffAsync(userManager, configuration.GetSection("SeedLibrarian"), "Librarian");

            var memberEmail = configuration["SeedMember:Email"];
            var memberPassword = configuration["SeedMember:Password"];
            if (!string.IsNullOrWhiteSpace(memberEmail) && !string.IsNullOrWhiteSpace(memberPassword)
                && await userManager.FindByEmailAsync(memberEmail) == null)
            {
                await memberService.CreateMemberAsync("Demo Member", memberEmail, memberPassword, "01000000000", "Cairo, Egypt");
            }

            // 3) بيانات تجريبية للتصنيفات والكتب (لو الداتابيز فاضية)
            if (configuration.GetValue<bool>("SeedSampleData") && !db.Categories.Any() && !db.Books.Any())
            {
                var programming = new Category { Name = "Programming" };
                var novels = new Category { Name = "Novels" };
                var science = new Category { Name = "Science" };
                var history = new Category { Name = "History" };
                db.Categories.AddRange(programming, novels, science, history);
                db.SaveChanges();

                db.Books.AddRange(
                    NewBook("Clean Code", "Robert C. Martin", 2008, 3, programming.Id),
                    NewBook("The Pragmatic Programmer", "Andrew Hunt", 1999, 2, programming.Id),
                    NewBook("Cosmos", "Carl Sagan", 1980, 2, science.Id),
                    NewBook("Les Miserables", "Victor Hugo", 1862, 1, novels.Id),
                    NewBook("Sapiens", "Yuval Noah Harari", 2011, 2, history.Id));
                db.SaveChanges();
            }
        }

        private static Book NewBook(string title, string author, int year, int copies, int categoryId)
        {
            return new Book
            {
                Title = title,
                Author = author,
                Year = year,
                Copies = copies,
                AvailableCopies = copies,
                CategoryId = categoryId
            };
        }

        private static async Task EnsureStaffAsync(UserManager<ApplicationUser> userManager, IConfigurationSection section, string role)
        {
            var email = section["Email"];
            var password = section["Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }
    }
}
