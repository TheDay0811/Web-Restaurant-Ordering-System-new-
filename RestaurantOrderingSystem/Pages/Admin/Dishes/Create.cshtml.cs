using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Pages.Admin.Dishes
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly DataContext context;

        [BindProperty]
        public Dish Dish { get; set; } = new();

        public SelectList? CategoryOptions { get; set; }

        public CreateModel(DataContext ctx)
        {
            context = ctx;
        }

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ImageUrl khong bat buoc (khong co [Required]) - Admin co the bo trong
            // neu chua co link anh, sau nay vao Sua de dan link vao cung duoc
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            context.Dishes.Add(Dish);
            await context.SaveChangesAsync();
            return RedirectToPage("Index");
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await context.Categories.OrderBy(c => c.Name).ToListAsync();
            CategoryOptions = new SelectList(categories, "CategoryId", "Name");
        }
    }
}
