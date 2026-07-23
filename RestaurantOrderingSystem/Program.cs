using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== Database (EF Core - Code First, giong file mau DemoWeb) =====
            builder.Services.AddDbContext<DataContext>(opts =>
            {
                opts.UseSqlServer(builder.Configuration["ConnectionStrings:RestaurantConnection"]);
                opts.EnableSensitiveDataLogging(true);
            });

            // ===== MVC + Razor Pages =====
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // ===== Session (dung luu gio hang - Cart) =====
            builder.Services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromHours(2);
            });
            builder.Services.AddHttpContextAccessor();

            // ===== Authentication (Cookie - giong DemoWeb) =====
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt =>
                {
                    opt.LoginPath = "/Account/Login";
                    opt.AccessDeniedPath = "/Account/AccessDenied";
                });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllerRoute(
                name: "Default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // ===== Seed Database (giong file mau DemoWeb) =====
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                SeedData.SeedDatabase(context);
            }

            app.Run();
        }
    }
}
