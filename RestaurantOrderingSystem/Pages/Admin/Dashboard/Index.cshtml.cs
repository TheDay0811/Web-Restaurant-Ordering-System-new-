using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Pages.Admin.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly DataContext context;
        public IndexModel(DataContext ctx) => context = ctx;

        // ==== Filter (bind tu query string, VD: ?FromDate=...&ToDate=...&CategoryId=...) ====
        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        public List<Category> Categories { get; set; } = new();

        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }

        // Doanh thu theo tung mon
        public List<RevenueByDish> ByDish { get; set; } = new();
        // Doanh thu theo ngay
        public List<RevenueByDate> ByDate { get; set; } = new();
        // Doanh thu theo danh muc
        public List<RevenueByCategory> ByCategory { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await context.Categories.OrderBy(c => c.Name).ToListAsync();

            // Query goc: chi tinh hoa don da thanh toan (Paid), loc theo ngay/danh muc neu co
            var query = context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Dish)
                .ThenInclude(d => d!.Category)
                .Where(od => od.Order!.Status == OrderStatus.Paid)
                .AsQueryable();

            if (FromDate.HasValue)
                query = query.Where(od => od.Order!.OrderDate >= FromDate.Value.Date);
            if (ToDate.HasValue)
                query = query.Where(od => od.Order!.OrderDate < ToDate.Value.Date.AddDays(1));
            if (CategoryId.HasValue)
                query = query.Where(od => od.Dish!.CategoryId == CategoryId.Value);

            var details = await query.ToListAsync();

            TotalRevenue = details.Sum(d => d.SubTotal);
            TotalOrders = details.Select(d => d.OrderId).Distinct().Count();

            ByDish = details
                .GroupBy(d => d.DishName)
                .Select(g => new RevenueByDish
                {
                    DishName = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.SubTotal)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            ByDate = details
                .GroupBy(d => d.Order!.OrderDate.Date)
                .Select(g => new RevenueByDate
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.SubTotal)
                })
                .OrderBy(x => x.Date)
                .ToList();

            ByCategory = details
                .GroupBy(d => d.Dish!.Category!.Name)
                .Select(g => new RevenueByCategory
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(x => x.SubTotal)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();
        }
    }

    public class RevenueByDish { public string DishName { get; set; } = ""; public int Quantity { get; set; } public decimal Revenue { get; set; } }
    public class RevenueByDate { public DateTime Date { get; set; } public decimal Revenue { get; set; } }
    public class RevenueByCategory { public string CategoryName { get; set; } = ""; public decimal Revenue { get; set; } }
}