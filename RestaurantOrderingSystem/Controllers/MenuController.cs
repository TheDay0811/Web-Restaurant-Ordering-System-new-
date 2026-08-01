using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;
using System.Security.Claims;

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

        // GET: /Menu/ScanTable?table=05
        // Diem den cua ma QR dan tren ban - khach quet QR se vao thang day.
        // Luu so ban vao Session de tu dong dien lai o buoc Checkout,
        // khong bat khach phai tu go tay so ban nua.
        public IActionResult ScanTable(string table)
        {
            if (!string.IsNullOrWhiteSpace(table))
            {
                HttpContext.Session.SetString("TableNumber", table.Trim());
                TempData["Info"] = $"Đã nhận diện Bàn {table.Trim()}. Chọn món và đặt hàng nhé!";
            }

            return RedirectToAction("Index");
        }

        // GET: /Menu/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dish = await context.Dishes
                .Include(d => d.Category)
                .Include(d => d.Reviews!)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(d => d.DishId == id);

            if (dish == null)
                return NotFound();

            // Neu la Customer da dang nhap: kiem tra co du dieu kien danh gia khong
            // (da dat va thanh toan mon nay), va lay danh gia cu (neu co) de dien san len form
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole(UserRole.Customer))
            {
                int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                ViewBag.CoTheDanhGia = await context.OrderDetails
                    .Include(od => od.Order)
                    .AnyAsync(od => od.DishId == id
                        && od.Order != null
                        && od.Order.UserId == currentUserId
                        && od.Order.Status == OrderStatus.Paid);

                ViewBag.DanhGiaCuaToi = dish.Reviews?.FirstOrDefault(r => r.UserId == currentUserId);
            }

            return View(dish);
        }
    }
}
