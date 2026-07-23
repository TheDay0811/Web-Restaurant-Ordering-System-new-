using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Pages.Admin.Dishes
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly DataContext context;
        public IList<Dish> DishList { get; set; } = new List<Dish>();

        public IndexModel(DataContext ctx)
        {
            context = ctx;
        }

        public async Task OnGetAsync()
        {
            DishList = await context.Dishes
                .Include(d => d.Category)
                .OrderBy(d => d.Category!.Name)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }
    }
}
