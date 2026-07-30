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

        // GET: /Menu?categoryId=1&searchTerm=ga
        public async Task<IActionResult> Index(int? categoryId, string? searchTerm)
        {
            var dishesQuery = context.Dishes
                .Include(d => d.Category)
                .Where(d => d.IsAvailable)
                .AsQueryable();

            if (categoryId.HasValue)
                dishesQuery = dishesQuery.Where(d => d.CategoryId == categoryId.Value);

            // Tim theo ten mon (khong phan biet hoa/thuong), bo qua khoang trang thua
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string keyword = searchTerm.Trim();
                dishesQuery = dishesQuery.Where(d => EF.Functions.Like(d.Name, $"%{keyword}%"));
            }

            ViewBag.Categories = await context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchTerm = searchTerm;

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