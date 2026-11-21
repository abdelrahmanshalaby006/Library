using Library.BLL.Services.Classes;
using Library.BLL.Services.Interfaces;
using Library.DAL.Contexts;
using Library.DAL.Entities;
using Library.DAL.Repositories.Classes;
using Library.DAL.Repositories.Interfaces;
using Library.PL.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC + حماية كل فورم POST بـ Anti-Forgery Token تلقائياً
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            // Database
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // Repositories (DAL)
            builder.Services.AddScoped<IBookRepository, BookRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IMemberRepository, MemberRepository>();
            builder.Services.AddScoped<IBorrowingRepository, BorrowingRepository>();

            // Services (BLL)
            builder.Services.AddScoped<IMemberService, MemberService>();
            builder.Services.AddScoped<IBorrowingService, BorrowingService>();

            // Identity (تسجيل الدخول + الأدوار)
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/access-denied";
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseRouting();

            // Authentication لازم قبل Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            // أول تشغيل: يطبّق الـ Migrations + يعمل الأدوار وحسابات البداية وبيانات تجريبية
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (!db.Database.GetMigrations().Any())
                {
                    throw new InvalidOperationException(
                        "مفيش Migrations في المشروع. شغّل الأمر ده في Package Manager Console الأول: " +
                        "Add-Migration InitCreate -Project Library.DAL -StartupProject Library.PL");
                }
                await db.Database.MigrateAsync();
            }
            await IdentitySeeder.SeedAsync(app.Services, builder.Configuration);

            await app.RunAsync();
        }
    }
}
