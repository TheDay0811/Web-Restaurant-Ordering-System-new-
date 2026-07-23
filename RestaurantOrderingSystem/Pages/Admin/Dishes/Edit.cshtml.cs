using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Pages.Admin.Dishes
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly DataContext context;

        [BindProperty]
        public Dish Dish { get; set; } = new();

        public SelectList? CategoryOptions { get; set; }

        public EditModel(DataContext ctx)
        {
            context = ctx;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var dish = await context.Dishes.FindAsync(id);
            if (dish == null)
                return NotFound();

            Dish = dish;
            await LoadCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            var dishInDb = await context.Dishes.FindAsync(Dish.DishId);
            if (dishInDb == null)
                return NotFound();

            // Cap nhat toan bo cac cot cua Dish (bao gom ca ImageUrl - Admin tu go/sua
            // link anh truc tiep trong form, khong con upload file/goi API nao nua)
            context.Entry(dishInDb).CurrentValues.SetValues(Dish);
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
