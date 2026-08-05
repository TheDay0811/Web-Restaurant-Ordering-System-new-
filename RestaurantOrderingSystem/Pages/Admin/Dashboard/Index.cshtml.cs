using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Pages.Admin.Dashboard
{
    // Trang Dashboard thong ke doanh thu cho Admin.
    // Tinh doanh thu tren "tat ca don tru don Da huy" (theo yeu cau) -
    // nghia la gom ca Pending/Confirmed/Paid, chi loai Cancelled.
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly DataContext context;

        public IndexModel(DataContext ctx)
        {
            context = ctx;
        }

        // ===== Bo loc khoang ngay (mac dinh 30 ngay gan nhat) =====
        [BindProperty(SupportsGet = true)]
        public DateTime? TuNgay { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DenNgay { get; set; }

        // ===== So lieu tong quan =====
        public decimal TongDoanhThu { get; set; }
        public int TongSoDon { get; set; }
        public decimal DoanhThuHomNay { get; set; }
        public int SoDonHomNay { get; set; }

        // ===== Du lieu cho bieu do =====
        public List<DoanhThuMon> DoanhThuTheoMon { get; set; } = new();
        public List<DoanhThuNgay> DoanhThuTheoNgay { get; set; } = new();
        public List<DoanhThuDanhMuc> DoanhThuTheoDanhMuc { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Mac dinh xem 30 ngay gan nhat neu chua chon
            DenNgay ??= DateTime.Today;
            TuNgay ??= DenNgay.Value.AddDays(-29);

            // Loc "tat ca don tru don Da huy" trong khoang ngay da chon
            var ordersQuery = context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Dish)
                        .ThenInclude(d => d!.Category)
                .Where(o => o.Status != OrderStatus.Cancelled
                            && o.OrderDate.Date >= TuNgay.Value.Date
                            && o.OrderDate.Date <= DenNgay.Value.Date);

            var orders = await ordersQuery.ToListAsync();

            TongDoanhThu = orders.Sum(o => o.TotalAmount);
            TongSoDon = orders.Count;

            var today = DateTime.Today;
            var ordersToday = orders.Where(o => o.OrderDate.Date == today).ToList();
            DoanhThuHomNay = ordersToday.Sum(o => o.TotalAmount);
            SoDonHomNay = ordersToday.Count;

            var allDetails = orders
                .Where(o => o.OrderDetails != null)
                .SelectMany(o => o.OrderDetails!)
                .ToList();

            // ----- Doanh thu theo mon (top 10 theo doanh thu giam dan) -----
            DoanhThuTheoMon = allDetails
                .GroupBy(d => d.DishName)
                .Select(g => new DoanhThuMon
                {
                    TenMon = g.Key,
                    SoLuongBan = g.Sum(x => x.Quantity),
                    DoanhThu = g.Sum(x => x.SubTotal)
                })
                .OrderByDescending(x => x.DoanhThu)
                .Take(10)
                .ToList();

            // ----- Doanh thu theo ngay (theo tung ngay trong khoang da chon) -----
            var doanhThuTheoNgayDict = orders
                .GroupBy(o => o.OrderDate.Date)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

            for (var d = TuNgay.Value.Date; d <= DenNgay.Value.Date; d = d.AddDays(1))
            {
                DoanhThuTheoNgay.Add(new DoanhThuNgay
                {
                    Ngay = d,
                    DoanhThu = doanhThuTheoNgayDict.TryGetValue(d, out var dt) ? dt : 0
                });
            }

            // ----- Doanh thu theo danh muc -----
            DoanhThuTheoDanhMuc = allDetails
                .Where(d => d.Dish != null && d.Dish.Category != null)
                .GroupBy(d => d.Dish!.Category!.Name)
                .Select(g => new DoanhThuDanhMuc
                {
                    TenDanhMuc = g.Key,
                    DoanhThu = g.Sum(x => x.SubTotal)
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();
        }

        public class DoanhThuMon
        {
            public string TenMon { get; set; } = string.Empty;
            public int SoLuongBan { get; set; }
            public decimal DoanhThu { get; set; }
        }

        public class DoanhThuNgay
        {
            public DateTime Ngay { get; set; }
            public decimal DoanhThu { get; set; }
        }

        public class DoanhThuDanhMuc
        {
            public string TenDanhMuc { get; set; } = string.Empty;
            public decimal DoanhThu { get; set; }
        }
    }
}
