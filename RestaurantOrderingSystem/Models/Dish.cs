using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderingSystem.Models
{
    // Mon an trong thuc don (Online Menu)
    public class Dish
    {
        public int DishId { get; set; }

        [Required(ErrorMessage = "Vui long nhap ten mon an")]
        [StringLength(150)]
        [Display(Name = "Tên món ăn")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, 100000000, ErrorMessage = "Gia phai lon hon 0")]
        [Display(Name = "Đơn giá")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui long chon danh muc")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Link hinh anh mon an - Admin dan truc tiep link anh da upload san
        // (vi du: upload len Cloudinary roi copy link "Secure URL" dan vao day)
        [StringLength(500)]
        [Display(Name = "Link ảnh món ăn")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Còn phục vụ")]
        public bool IsAvailable { get; set; } = true;

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
