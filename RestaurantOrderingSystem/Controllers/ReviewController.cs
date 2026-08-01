using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;
using System.Security.Claims;

namespace RestaurantOrderingSystem.Controllers
{
    // Danh gia mon an - chi Customer da dang nhap va da THUC SU dat + thanh toan
    // mon do moi duoc danh gia, tranh danh gia khong (spam)
    [Authorize(Roles = UserRole.Customer)]
    public class ReviewController : Controller
    {
        private readonly DataContext context;

        public ReviewController(DataContext ctx)
        {
            context = ctx;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // POST: /Review/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int dishId, int rating, string? comment)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Vui lòng chọn số sao từ 1 đến 5.";
                return RedirectToAction("Details", "Menu", new { id = dishId });
            }

            // Kiem tra khach da tung dat mon nay va don da o trang thai "Da thanh toan" chua -
            // chi khach hang thuc su da dung mon moi duoc danh gia
            bool daMuaMon = await context.OrderDetails
                .Include(od => od.Order)
                .AnyAsync(od => od.DishId == dishId
                    && od.Order != null
                    && od.Order.UserId == CurrentUserId
                    && od.Order.Status == OrderStatus.Paid);

            if (!daMuaMon)
            {
                TempData["Error"] = "Bạn cần đặt và hoàn tất thanh toán món này trước khi đánh giá.";
                return RedirectToAction("Details", "Menu", new { id = dishId });
            }

            // Moi khach chi co 1 danh gia cho 1 mon - danh gia lai se cap nhat de len danh gia cu
            var existing = await context.Reviews
                .FirstOrDefaultAsync(r => r.DishId == dishId && r.UserId == CurrentUserId);

            if (existing != null)
            {
                existing.Rating = rating;
                existing.Comment = comment;
                existing.CreatedDate = DateTime.Now;
            }
            else
            {
                context.Reviews.Add(new Review
                {
                    DishId = dishId,
                    UserId = CurrentUserId,
                    Rating = rating,
                    Comment = comment,
                    CreatedDate = DateTime.Now
                });
            }

            await context.SaveChangesAsync();
            TempData["Success"] = "Cảm ơn bạn đã đánh giá món ăn!";
            return RedirectToAction("Details", "Menu", new { id = dishId });
        }
    }
}
