using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    // Nhan Webhook tu SePay moi khi co giao dich chuyen khoan THAT vao tai khoan
    // ngan hang cua nha hang. Khi noi dung chuyen khoan trung voi Ma hoa don (OrderCode,
    // vi du HD000123) va so tien khop, tu dong cap nhat Order.Status = Paid.
    //
    // Cach hoat dong: SePay theo doi bien dong so du ngan hang (qua SMS Banking hoac
    // Open API tuy ngan hang) roi POST JSON toi URL nay ngay khi tien vao tai khoan.
    // Xem huong dan cau hinh SePay trong ghi chu cuoi file appsettings.json.
    [ApiController]
    [Route("api/sepay-webhook")]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly DataContext context;
        private readonly IConfiguration configuration;

        public PaymentWebhookController(DataContext ctx, IConfiguration config)
        {
            context = ctx;
            configuration = config;
        }

        // Mau OrderCode dang dung trong he thong: HD + 6 chu so, vi du HD000123
        // (xem OrderController.Checkout: order.OrderCode = "HD" + order.OrderId.ToString("D6"))
        private static readonly Regex OrderCodeRegex = new(@"HD\d{6}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] SePayWebhookPayload payload)
        {
            // ===== 1. Xac thuc request thuc su den tu SePay (khong phai gia mao) =====
            string expectedKey = configuration["SePay:ApiKey"] ?? "";
            string authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(expectedKey) || authHeader != $"Apikey {expectedKey}")
            {
                return Unauthorized(new { success = false, message = "Sai hoac thieu Api Key" });
            }

            if (payload == null)
                return BadRequest(new { success = false, message = "Thieu du lieu" });

            // Chi xu ly giao dich TIEN VAO (khach chuyen khoan cho nha hang)
            if (!string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase))
                return Ok(new { success = true, message = "Bo qua giao dich tien ra" });

            // ===== 2. Tim ma hoa don (HDxxxxxx) trong noi dung chuyen khoan =====
            string content = payload.Content ?? payload.Description ?? "";
            var match = OrderCodeRegex.Match(content);
            if (!match.Success)
                return Ok(new { success = true, message = "Khong tim thay ma hoa don trong noi dung" });

            string orderCode = match.Value.ToUpper();

            var order = await context.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            if (order == null)
                return Ok(new { success = true, message = "Khong tim thay hoa don " + orderCode });

            // Idempotent: da Paid roi thi thoi, tranh SePay retry lam cap nhat trung
            if (order.Status == OrderStatus.Paid)
                return Ok(new { success = true, message = "Hoa don da o trang thai Paid tu truoc" });

            // ===== 3. Doi chieu so tien - phai chuyen DU hoac HON, tranh khach chuyen thieu =====
            if (payload.TransferAmount < order.TotalAmount)
            {
                return Ok(new
                {
                    success = true,
                    message = $"So tien chuyen ({payload.TransferAmount:N0}) nho hon tong hoa don ({order.TotalAmount:N0}), chua tu dong xac nhan"
                });
            }

            // ===== 4. Khop het -> tu dong cap nhat hoa don thanh Da thanh toan =====
            order.Status = OrderStatus.Paid;
            await context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Da xac nhan thanh toan cho {orderCode}" });
        }
    }

    // Cac truong SePay gui ve - chi khai bao nhung truong can dung,
    // JSON du thua khac se tu bi bo qua khi deserialize.
    public class SePayWebhookPayload
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("gateway")]
        public string? Gateway { get; set; }

        [JsonPropertyName("transactionDate")]
        public string? TransactionDate { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("transferType")]
        public string? TransferType { get; set; } // "in" hoac "out"

        [JsonPropertyName("transferAmount")]
        public decimal TransferAmount { get; set; }

        [JsonPropertyName("referenceCode")]
        public string? ReferenceCode { get; set; }
    }
}
