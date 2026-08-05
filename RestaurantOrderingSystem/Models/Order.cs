using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderingSystem.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Chờ xác nhận")]
        Pending = 0,
        [Display(Name = "Đã xác nhận")]
        Confirmed = 1,
        [Display(Name = "Đã thanh toán")]
        Paid = 2,
        [Display(Name = "Đã hủy")]
        Cancelled = 3
    }

    // Don hang / Hoa don (Invoice)
    public class Order
    {
        public int OrderId { get; set; }

        // Ma hoa don hien thi cho khach, vi du: HD000123
        [StringLength(20)]
        public string OrderCode { get; set; } = string.Empty;

        // Tai khoan Customer da dat don nay (de loc "Don hang cua toi")
        public int? UserId { get; set; }
        public User? User { get; set; }

        [Required(ErrorMessage = "Vui long nhap ten khach hang")]
        [StringLength(100)]
        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Số bàn")]
        public string? TableNumber { get; set; }

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Display(Name = "Ngày đặt")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        // Bep da lam xong toan bo don nay chua - RIENG BIET voi Status o tren
        // (Status danh cho Admin quan ly hoa don/thanh toan, con KitchenDone
        // chi danh cho man hinh bep danh dau mon da nau xong hay chua)
        [Display(Name = "Bếp đã xong")]
        public bool KitchenDone { get; set; } = false;

        // Ly do huy don - chi co gia tri khi Status = Cancelled va don bi huy
        // tu man hinh Bep (xem KitchenController.Cancel). Bat buoc phai nhap
        // ly do moi huy duoc, xem validate o KitchenController.
        [StringLength(300)]
        [Display(Name = "Lý do hủy")]
        public string? CancelReason { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}