using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Pages.Admin.Categories
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly DataContext context;
        public IList<Category> CategoryList { get; set; } = new List<Category>();

        public IndexModel(DataContext ctx)
        {
            context = ctx;
        }

        public async Task OnGetAsync()
        {
            CategoryList = await context.Categories
                .Include(c => c.Dishes)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
