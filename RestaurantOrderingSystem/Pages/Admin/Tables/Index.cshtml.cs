using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;

namespace RestaurantOrderingSystem.Pages.Admin.Tables
{
    // Trang Admin: tao va tai ma QR cho tung ban.
    // Moi ma QR khi khach quet se dan toi /Menu/ScanTable?table=xx
    // (xem MenuController.ScanTable) de tu dong luu so ban vao Session.
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        // So luong ban muon tao QR (vi du 10 ban -> Ban 01 den Ban 10)
        [BindProperty(SupportsGet = true)]
        public int SoBan { get; set; } = 10;

        public List<TableQrItem> Tables { get; set; } = new();

        public void OnGet()
        {
            if (SoBan < 1) SoBan = 1;
            if (SoBan > 100) SoBan = 100; // gioi han hop ly, tranh tao qua nhieu cung luc

            for (int i = 1; i <= SoBan; i++)
            {
                // Dinh dang 2 chu so: 01, 02, ... 10, 11 - de nhin gon va thong nhat
                string tableNumber = i.ToString("D2");
                Tables.Add(new TableQrItem
                {
                    TableNumber = tableNumber,
                    ScanUrl = BuildScanUrl(tableNumber),
                    Base64Png = Convert.ToBase64String(GenerateQrBytes(tableNumber))
                });
            }
        }

        // GET: /Admin/Tables?handler=Download&table=05
        // Tai rieng 1 anh QR ve may dinh dang PNG, de in ra dan len ban
        public IActionResult OnGetDownload(string table)
        {
            if (string.IsNullOrWhiteSpace(table))
                return NotFound();

            byte[] bytes = GenerateQrBytes(table);
            return File(bytes, "image/png", $"QR-Ban-{table}.png");
        }

        // Sinh URL day du nhung vao ma QR, vi du:
        // https://tenweb.com/Menu/ScanTable?table=05
        // Dung chinh domain hien tai (Host) nen khong can sua code khi doi domain that
        private string BuildScanUrl(string table)
        {
            var request = HttpContext.Request;
            return $"{request.Scheme}://{request.Host}/Menu/ScanTable?table={table}";
        }

        private byte[] GenerateQrBytes(string table)
        {
            string url = BuildScanUrl(table);
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20); // 20 = kich thuoc moi o vuong trong ma QR (pixel)
        }

        public class TableQrItem
        {
            public string TableNumber { get; set; } = string.Empty;
            public string ScanUrl { get; set; } = string.Empty;
            public string Base64Png { get; set; } = string.Empty;
        }
    }
}
