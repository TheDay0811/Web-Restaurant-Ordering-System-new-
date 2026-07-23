using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{
    // Danh muc mon an: Khai vi, Mon chinh, Trang mieng, Do uong ...
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui long nhap ten danh muc")]
        [StringLength(100)]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        public ICollection<Dish>? Dishes { get; set; }
    }
}
