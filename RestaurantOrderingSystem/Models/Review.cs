using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{
    // Danh gia mon an - 1 khach (Customer) chi danh gia duoc 1 mon toi da 1 lan
    // (danh gia lai se cap nhat de len danh gia cu, xem ReviewController.Submit)
    public class Review
    {
        public int ReviewId { get; set; }

        public int DishId { get; set; }
        public Dish? Dish { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Range(1, 5, ErrorMessage = "Vui lòng chọn số sao từ 1 đến 5")]
        [Display(Name = "Số sao")]
        public int Rating { get; set; }

        [StringLength(500)]
        [Display(Name = "Nhận xét")]
        public string? Comment { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
