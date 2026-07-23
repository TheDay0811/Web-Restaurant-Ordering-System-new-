using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Pages.Admin.Categories
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly DataContext context;

        [BindProperty]
        public Category Category { get; set; } = new();

        public CreateModel(DataContext ctx)
        {
            context = ctx;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            context.Categories.Add(Category);
            await context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
