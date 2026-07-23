namespace RestaurantOrderingSystem.Models
{
    // 1 dong trong gio hang - duoc luu trong Session (JSON), khong phai bang trong DB
    public class CartItem
    {
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }

        public decimal SubTotal => Price * Quantity;
    }
}
