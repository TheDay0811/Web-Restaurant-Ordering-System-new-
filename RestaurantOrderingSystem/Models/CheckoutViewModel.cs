using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{
    // Thong tin khach hang nhap khi dat mon (tao hoa don)
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        [StringLength(100)]
        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số bàn")]
        public string? TableNumber { get; set; }

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        public List<CartItem> Items { get; set; } = new();

        public decimal GrandTotal => Items.Sum(i => i.SubTotal);
    }
}
