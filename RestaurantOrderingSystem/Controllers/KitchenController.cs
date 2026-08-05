using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    // Man hinh bep (Kitchen View) - danh cho nhan vien bep, TACH BIET hoan toan
    // voi trang quan ly hoa don cua Admin (Order/History).
    // Bep chi xem danh sach mon can lam theo thoi gian gui va danh dau "Da xong"
    // cho ca don hang. Viec danh dau nay dung cot Order.KitchenDone la chinh;
    // rieng MarkDone co tien Status Pending -> Confirmed luon (xem comment trong
    // MarkDone) de ben Admin biet don nao bep da lam xong ma khong can sua tay.
    [Authorize(Roles = UserRole.Kitchen)]
    public class KitchenController : Controller
    {
        private readonly DataContext context;

        public KitchenController(DataContext ctx)
        {
            context = ctx;
        }

        // GET: /Kitchen - danh sach don CHUA lam xong, sap xep theo thoi gian
        // gui don (cu nhat / gui truoc thi lam truoc). Hien thi ngay tu luc
        // khach vua dat (Pending) cho den khi bep bam "Da xong". Don da bi
        // Admin huy (Cancelled) thi khong can bep lam nua nen an di.
        public async Task<IActionResult> Index()
        {
            var orders = await context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => !o.KitchenDone && o.Status != OrderStatus.Cancelled)
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // POST: /Kitchen/MarkDone/5 - danh dau toan bo don hang nay la da lam xong,
        // don se bien khoi danh sach Index ngay lan tai lai/refresh ke tiep.
        // Dong thoi tu dong chuyen Status Pending -> Confirmed de ben Admin
        // (Order/History, Order/MyOrders) thay ngay don nao bep da lam xong,
        // khong con cach nao khac de doi trang thai nay (da bo dropdown thu cong).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDone(int id)
        {
            var order = await context.Orders.FindAsync(id);
            if (order != null)
            {
                order.KitchenDone = true;
                if (order.Status == OrderStatus.Pending)
                    order.Status = OrderStatus.Confirmed;
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Kitchen/Cancel/5 - Bep huy don (vi du het mon, khach doi y...).
        // BAT BUOC phai nhap ly do huy - khong co ly do thi tu choi, khong luu,
        // tra thong bao loi qua TempData de View hien thi lai cho bep nhap lai.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string cancelReason)
        {
            if (string.IsNullOrWhiteSpace(cancelReason))
            {
                TempData["Error"] = "Vui lòng nhập lý do hủy đơn.";
                return RedirectToAction(nameof(Index));
            }

            var order = await context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = OrderStatus.Cancelled;
                order.CancelReason = cancelReason.Trim();
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}