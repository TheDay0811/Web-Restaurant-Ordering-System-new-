using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Pages.Admin.Categories
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly DataContext context;

        [BindProperty]
        public Category Category { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public DeleteModel(DataContext ctx)
        {
            context = ctx;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await context.Categories
                .Include(c => c.Dishes)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            Category = category;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var category = await context.Categories
                .Include(c => c.Dishes)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return RedirectToPage("Index");

            // Khong cho xoa danh muc neu con mon an dang thuoc danh muc do
            if (category.Dishes != null && category.Dishes.Any())
            {
                ErrorMessage = "Không thể xóa danh mục vì vẫn còn món ăn thuộc danh mục này.";
                Category = category;
                return Page();
            }

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
