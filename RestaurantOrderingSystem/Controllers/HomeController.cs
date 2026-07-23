using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;
using System.Diagnostics;

namespace RestaurantOrderingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext context;

        public HomeController(DataContext ctx)
        {
            context = ctx;
        }

        // Trang chu: gioi thieu nha hang + goi y mot vai mon noi bat
        public async Task<IActionResult> Index()
        {
            var featuredDishes = await context.Dishes
                .Include(d => d.Category)
                .Where(d => d.IsAvailable)
                .OrderByDescending(d => d.DishId)
                .Take(6)
                .ToListAsync();

            return View(featuredDishes);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
