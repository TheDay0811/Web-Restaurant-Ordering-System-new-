using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    // Gio hang - danh sach mon khach da chon, luu tam trong Session (chua tao hoa don)
    // Chi tai khoan vai tro Customer da dang nhap moi duoc dung gio hang;
    // khach chua dang nhap chi duoc xem thuc don (Menu/Home)
    [Authorize(Roles = UserRole.Customer)]
    public class CartController : Controller
    {
        // Tien to dat truoc ten key trong Session, vi du: "cart_5" nghia la
        // mon an co DishId = 5. Cach nay don gian: moi mon an la 1 dong rieng
        // trong Session (kieu so nguyen), KHONG dung JSON, khong can vong lop
        // chuyen doi phuc tap.
        private const string CartKeyPrefix = "cart_";

        private readonly DataContext context;

        public CartController(DataContext ctx)
        {
            context = ctx;
        }

        // Doc gio hang: duyet qua toan bo Session, tim cac key bat dau bang
        // "cart_" (vi du "cart_5" = 2 nghia la mon DishId=5 dang co 2 phan),
        // roi truy van database 1 lan de lay ten mon/gia/anh tuong ung.
        private async Task<List<CartItem>> GetCartAsync()
        {
            var quantityByDishId = new Dictionary<int, int>();

            foreach (var key in HttpContext.Session.Keys)
            {
                if (!key.StartsWith(CartKeyPrefix)) continue;

                string dishIdText = key.Substring(CartKeyPrefix.Length);
                if (!int.TryParse(dishIdText, out int dishId)) continue;

                int quantity = HttpContext.Session.GetInt32(key) ?? 0;
                if (quantity > 0)
                    quantityByDishId[dishId] = quantity;
            }

            if (quantityByDishId.Count == 0)
                return new List<CartItem>();

            var dishIds = quantityByDishId.Keys.ToList();
            var dishes = await context.Dishes
                .Where(d => dishIds.Contains(d.DishId))
                .ToListAsync();

            return dishes.Select(d => new CartItem
            {
                DishId = d.DishId,
                DishName = d.Name,
                Price = d.Price,
                ImageUrl = d.ImageUrl,
                Quantity = quantityByDishId[d.DishId]
            }).ToList();
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            return View(await GetCartAsync());
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int dishId, int quantity = 1)
        {
            var dish = await context.Dishes.FindAsync(dishId);
            if (dish == null || !dish.IsAvailable)
            {
                TempData["Error"] = "Món ăn không tồn tại hoặc đã ngừng phục vụ.";
                return RedirectToAction("Index", "Menu");
            }

            if (quantity < 1) quantity = 1;

            // Neu mon nay da co trong gio thi cong don so luong, chua co thi tao moi -
            // tat ca chi la 1 dong SetInt32 duy nhat, khong can doc/ghi danh sach nhu truoc
            string key = CartKeyPrefix + dishId;
            int currentQuantity = HttpContext.Session.GetInt32(key) ?? 0;
            HttpContext.Session.SetInt32(key, currentQuantity + quantity);

            TempData["Success"] = $"Đã thêm \"{dish.Name}\" vào giỏ hàng.";
            return RedirectToAction("Index", "Menu");
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int dishId, int quantity)
        {
            string key = CartKeyPrefix + dishId;

            if (quantity < 1)
                HttpContext.Session.Remove(key);
            else
                HttpContext.Session.SetInt32(key, quantity);

            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int dishId)
        {
            HttpContext.Session.Remove(CartKeyPrefix + dishId);
            return RedirectToAction("Index");
        }

        // POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            // Session hien tai chi dung de luu gio hang nen xoa toan bo Session
            // la du va don gian, khong can duyet tung mon de xoa rieng le
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
