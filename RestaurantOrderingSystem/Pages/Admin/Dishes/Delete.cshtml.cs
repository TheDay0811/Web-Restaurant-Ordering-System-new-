using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Pages.Admin.Dishes
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly DataContext context;

        [BindProperty]
        public Dish Dish { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public DeleteModel(DataContext ctx)
        {
            context = ctx;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var dish = await context.Dishes.Include(d => d.Category).FirstOrDefaultAsync(d => d.DishId == id);
            if (dish == null)
                return NotFound();

            Dish = dish;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var dish = await context.Dishes
                .Include(d => d.OrderDetails)
                .FirstOrDefaultAsync(d => d.DishId == id);

            if (dish == null)
                return RedirectToPage("Index");

            // Neu mon an da xuat hien trong hoa don thi khong xoa, chi ngung phuc vu
            // de khong lam mat du lieu lich su hoa don
            if (dish.OrderDetails != null && dish.OrderDetails.Any())
            {
                ErrorMessage = "Món ăn đã có trong hóa đơn nên không thể xóa. Hệ thống đã tự động chuyển sang trạng thái \"Ngừng phục vụ\".";
                dish.IsAvailable = false;
                await context.SaveChangesAsync();
                Dish = dish;
                return Page();
            }

            // Chi la link anh (khong luu file that tren server), nen xoa mon an
            // la xong, khong can goi API nao de xoa anh ca
            context.Dishes.Remove(dish);
            await context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
