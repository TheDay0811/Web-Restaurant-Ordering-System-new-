using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantOrderingSystem.Models
{
    // Chi tiet hoa don - moi dong la 1 mon an trong 1 don hang
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int DishId { get; set; }
        public Dish? Dish { get; set; }

        // Luu lai ten mon tai thoi diem dat, phong khi mon bi doi ten/xoa sau nay
        [Column(TypeName = "nvarchar(150)")]
        public string DishName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        [NotMapped]
        public decimal SubTotal => UnitPrice * Quantity;
    }
}
