using Microsoft.EntityFrameworkCore;

namespace RestaurantOrderingSystem.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> opts) : base(opts) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Dish> Dishes => Set<Dish>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ten dang nhap khong duoc trung nhau
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            // 1 User (Customer) - nhieu Order da dat
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1 Category - nhieu Dish
            modelBuilder.Entity<Dish>()
                .HasOne(d => d.Category)
                .WithMany(c => c.Dishes)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1 Order - nhieu OrderDetail
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1 Dish - nhieu OrderDetail (khong xoa cung mon an de tranh mat lich su hoa don)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Dish)
                .WithMany(d => d.OrderDetails)
                .HasForeignKey(od => od.DishId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
