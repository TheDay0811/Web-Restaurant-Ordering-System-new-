using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{
    // Vai trò tài khoản trong hệ thống
    public static class UserRole
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
        // Nhan vien bep: chi xem duoc man hinh Kitchen (danh sach mon can lam),
        // khong duoc vao trang quan ly hoa don/mon an/danh muc cua Admin
        public const string Kitchen = "Kitchen";
    }

    // Tài khoản đăng nhập - lưu trong database (thay cho tài khoản hard-code trước đây)
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        // Mat khau luu truc tiep (khong ma hoa) de don gian, phu hop muc do
        // do an mon hoc - KHONG nen lam vay voi ung dung thuc te trien khai that
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        // Chỉ nhận giá trị UserRole.Admin hoặc UserRole.Customer
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = UserRole.Customer;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ICollection<Order>? Orders { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}