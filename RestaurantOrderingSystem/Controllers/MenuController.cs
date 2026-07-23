using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    // Thuc don online: noi khach hang xem va chon mon
    public class MenuController : Controller
    {
        private readonly DataContext context;

        public MenuController(DataContext ctx)
        {
            context = ctx;
        }

        // GET: /Menu?categoryId=1
        public async Task<IActionResult> Index(int? categoryId)
        {
            var dishesQuery = context.Dishes
                .Include(d => d.Category)
                .Where(d => d.IsAvailable)
                .AsQueryable();

            if (categoryId.HasValue)
                dishesQuery = dishesQuery.Where(d => d.CategoryId == categoryId.Value);

            ViewBag.Categories = await context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.SelectedCategoryId = categoryId;

            var dishes = await dishesQuery.OrderBy(d => d.Category!.Name).ThenBy(d => d.Name).ToListAsync();
            return View(dishes);
        }

        // GET: /Menu/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dish = await context.Dishes
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.DishId == id);

            if (dish == null)
                return NotFound();

            return View(dish);
        }
    }
}
