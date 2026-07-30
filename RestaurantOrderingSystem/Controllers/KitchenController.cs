using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    // Man hinh bep (Kitchen View) - danh cho nhan vien bep, TACH BIET hoan toan
    // voi trang quan ly hoa don cua Admin (Order/History).
    // Bep chi xem danh sach mon can lam theo thoi gian gui va danh dau "Da xong"
    // cho ca don hang. Viec danh dau nay dung cot Order.KitchenDone, KHONG dung
    // Order.Status (Status danh rieng cho Admin quan ly thanh toan/hoa don).
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
        // don se bien khoi danh sach Index ngay lan tai lai/refresh ke tiep
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDone(int id)
        {
            var order = await context.Orders.FindAsync(id);
            if (order != null)
            {
                order.KitchenDone = true;
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}