using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;
using System.Security.Claims;

namespace RestaurantOrderingSystem.Controllers
{
    // Dat mon (Checkout) va Hoa don (Invoice)
    // Dat mon / Xem "Don hang cua toi": chi danh cho Customer da dang nhap
    // Xem danh sach toan bo hoa don / cap nhat trang thai: chi danh cho Admin
    public class OrderController : Controller
    {
        // Cung tien to voi CartController: moi mon an trong gio la 1 key rieng
        // trong Session, vi du "cart_5" = 2 (khong dung JSON)
        private const string CartKeyPrefix = "cart_";
        private readonly DataContext context;

        public OrderController(DataContext ctx)
        {
            context = ctx;
        }

        // Doc gio hang tu Session - cach lam giong het CartController.GetCartAsync()
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

        // Lay UserId cua tai khoan dang dang nhap tu Claims (thong tin dinh kem
        // trong cookie dang nhap - xem AccountController.SignIn de biet cach tao Claims).
        // Chi goi thuoc tinh nay trong cac action da co [Authorize] nen chac chan
        // luon co claim NameIdentifier, khong lo bi loi khi Parse.
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /Order/Checkout
        [Authorize(Roles = UserRole.Customer)]
        public async Task<IActionResult> Checkout()
        {
            var cart = await GetCartAsync();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            // Dien san thong tin tu tai khoan dang dang nhap cho tien
            var currentUser = await context.Users.FindAsync(CurrentUserId);
            var model = new CheckoutViewModel
            {
                Items = cart,
                CustomerName = currentUser?.FullName ?? string.Empty,
                PhoneNumber = currentUser?.PhoneNumber ?? string.Empty
            };
            return View(model);
        }

        // POST: /Order/Checkout
        [Authorize(Roles = UserRole.Customer)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = await GetCartAsync();
            model.Items = cart;

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
                return View(model);

            var order = new Order
            {
                // Gan don hang nay cho tai khoan Customer dang dat, de sau nay
                // ho xem lai duoc trong "Don hang cua toi" (MyOrders)
                UserId = CurrentUserId,
                CustomerName = model.CustomerName,
                PhoneNumber = model.PhoneNumber,
                TableNumber = model.TableNumber,
                Note = model.Note,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                TotalAmount = cart.Sum(i => i.SubTotal),
                // Chuyen tung dong trong gio hang (CartItem, luu trong Session)
                // thanh OrderDetail that de luu vao database
                OrderDetails = cart.Select(i => new OrderDetail
                {
                    DishId = i.DishId,
                    DishName = i.DishName,
                    UnitPrice = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Phai Luu (SaveChanges) lan dau de EF Core sinh ra OrderId truoc,
            // roi moi dung OrderId do de tao ma hoa don (OrderCode) va luu lan 2
            order.OrderCode = "HD" + order.OrderId.ToString("D6");
            await context.SaveChangesAsync();

            // Xoa gio hang sau khi da tao hoa don - Session hien chi dung de luu
            // gio hang nen xoa toan bo Session la du, khong can duyet tung mon
            HttpContext.Session.Clear();

            return RedirectToAction("Invoice", new { id = order.OrderId });
        }

        // GET: /Order/Invoice/5  - trang hoa don co the in
        // Customer chi xem duoc hoa don cua chinh minh, Admin xem duoc tat ca
        [Authorize]
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            // [Authorize] o day chi yeu cau "da dang nhap" (khong phan biet vai tro),
            // vi ca Customer va Admin deu duoc xem hoa don - nhung phai kiem tra
            // rieng ben duoi de dam bao Customer A khong xem duoc hoa don cua Customer B
            bool isAdmin = User.IsInRole(UserRole.Admin);
            bool isOwner = order.UserId.HasValue && order.UserId.Value == CurrentUserId;

            if (!isAdmin && !isOwner)
                return Forbid(); // Khong du quyen -> chuyen huong sang trang AccessDenied

            return View(order);
        }

        // GET: /Order/MyOrders - danh sach hoa don cua chinh Customer dang dang nhap
        [Authorize(Roles = UserRole.Customer)]
        public async Task<IActionResult> MyOrders()
        {
            var orders = await context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == CurrentUserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Order/History - danh sach toan bo hoa don, chi danh cho Admin
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> History()
        {
            var orders = await context.Orders
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // POST: /Order/UpdateStatus - Admin cap nhat trang thai hoa don
        [Authorize(Roles = UserRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await context.SaveChangesAsync();
            }
            return RedirectToAction("History");
        }
    }
}
