using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Pages.Admin.Categories
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly DataContext context;

        [BindProperty]
        public Category Category { get; set; } = new();

        public EditModel(DataContext ctx)
        {
            context = ctx;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            Category = category;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            context.Attach(Category).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
