using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;
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
        private readonly IConfiguration configuration;

        public OrderController(DataContext ctx, IConfiguration config)
        {
            context = ctx;
            configuration = config;
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

        // Dung cho dropdown chon ban o trang Checkout, thay vi de khach tu go tay
        // (de nham/sai dinh dang). Dinh dang "01", "02"... GIONG HET voi Admin/Tables
        // (trang tao QR ban) va MenuController.ScanTable, de neu khach quet QR ban
        // truoc thi gia tri co san trong Session se khop dung voi 1 option trong list.
        // Muon doi so luong ban thi sua RestaurantSettings:TableCount trong appsettings.json.
        private List<SelectListItem> BuildTableOptions(string? selectedValue)
        {
            int tableCount = configuration.GetValue<int?>("RestaurantSettings:TableCount") ?? 10;

            var options = new List<SelectListItem>
            {
                new SelectListItem("Mang về (không ngồi bàn)", "")
            };

            for (int i = 1; i <= tableCount; i++)
            {
                string value = i.ToString("D2");
                options.Add(new SelectListItem($"Bàn {value}", value));
            }

            // Khach da quet QR ban truoc do (hoac dang sua lai form sau khi loi validate)
            // thi tu dong to dam dung option tuong ung
            foreach (var opt in options)
                opt.Selected = opt.Value == (selectedValue ?? "");

            return options;
        }

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
                PhoneNumber = currentUser?.PhoneNumber ?? string.Empty,
                // Neu khach da quet ma QR tren ban (xem MenuController.ScanTable)
                // thi so ban da duoc luu san trong Session - tu dong dien vao day,
                // khach van co the sua lai neu can (vi du doi ban)
                TableNumber = HttpContext.Session.GetString("TableNumber")
            };
            ViewBag.TableOptions = BuildTableOptions(model.TableNumber);
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
            {
                ViewBag.TableOptions = BuildTableOptions(model.TableNumber);
                return View(model);
            }

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

            // ===== QR thanh toan VietQR (Sacombank, cau hinh trong appsettings.json) =====
            // Dung Quick Link cua VietQR - tra ve thang anh QR chuyen khoan that,
            // da co san so tien va noi dung, khach chi can mo app ngan hang quet la duoc.
            // Chi hien QR thanh toan khi hoa don CHUA thanh toan (Paid/Cancelled thi khong can nua)
            if (order.Status != OrderStatus.Paid && order.Status != OrderStatus.Cancelled)
            {
                string bankBin = configuration["VietQrPayment:BankBin"] ?? "";
                string accountNumber = configuration["VietQrPayment:AccountNumber"] ?? "";
                string accountName = configuration["VietQrPayment:AccountName"] ?? "";

                string addInfo = Uri.EscapeDataString($"Thanh toan hoa don {order.OrderCode}");
                string accNameEncoded = Uri.EscapeDataString(accountName);

                ViewBag.VietQrUrl =
                    $"https://img.vietqr.io/image/{bankBin}-{accountNumber}-compact2.png" +
                    $"?amount={(long)order.TotalAmount}&addInfo={addInfo}&accountName={accNameEncoded}";
                ViewBag.BankName = configuration["VietQrPayment:BankName"];
                ViewBag.AccountNumber = accountNumber;
                ViewBag.AccountName = accountName;
            }

            // ===== QR "tong hop" hoa don: ma hoa link mo trang hoa don nay =====
            // Chon phuong an nhet LINK thay vi nhet toan bo text chi tiet mon,
            // vi nhet nhieu text se lam QR qua day, kho quet khi hoa don nhieu mon.
            // Quet ma nay se mo dung trang hoa don voi day du, moi nhat tu database.
            string invoiceUrl = $"{Request.Scheme}://{Request.Host}/Order/Invoice/{order.OrderId}";
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(invoiceUrl, QRCodeGenerator.ECCLevel.Q))
            {
                var qrCode = new PngByteQRCode(qrCodeData);
                ViewBag.InvoiceQrBase64 = Convert.ToBase64String(qrCode.GetGraphic(10));
            }
            ViewBag.InvoiceUrl = invoiceUrl;

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

        // POST: /Order/MarkPaid - Admin xac nhan da nhan duoc tien (bam ngay tren
        // trang Hoa don sau khi khach chuyen khoan qua QR). Chi doi Status -> Paid,
        // khong dong cham gi khac. Sau khi xac nhan, quay lai chinh trang hoa don
        // do de Admin thay ngay ket qua (badge doi mau + QR chuyen tien tu bien mat).
        [Authorize(Roles = UserRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var order = await context.Orders.FindAsync(id);
            if (order != null && order.Status != OrderStatus.Cancelled)
            {
                order.Status = OrderStatus.Paid;
                await context.SaveChangesAsync();
            }
            return RedirectToAction("Invoice", new { id });
        }
    }
}