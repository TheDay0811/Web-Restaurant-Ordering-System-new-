using Microsoft.EntityFrameworkCore;

namespace RestaurantOrderingSystem.Models
{
    public static class SeedData
    {
        // Ghi chu: Anh mau lay tu cloud demo cua Cloudinary.
        // Khi trien khai that, hay upload anh mon an that len tai khoan Cloudinary
        // cua ban roi cap nhat lai ImageUrl (xem Admin/Dishes/Create de upload).
        private const string SampleImage =
            "https://res.cloudinary.com/demo/image/upload/w_600,h_400,c_fill,q_auto/sample.jpg";

        public static void SeedDatabase(DataContext context)
        {
            // EnsureCreated: tu dong tao Database + toan bo bang theo dung cac Model
            // (Category, Dish, Order, OrderDetail, User...) ngay lan chay dau tien,
            // KHONG can phai tu chay lenh Add-Migration / Update-Database.
            // Luu y: neu sau nay sua Model (them cot, them bang...) thi EnsureCreated
            // se KHONG tu cap nhat database da ton tai - phai xoa database cu di
            // (Drop-Database trong Package Manager Console, hoac xoa trong SQL Server
            // Object Explorer) de no tao lai tu dau theo Model moi.
            context.Database.EnsureCreated();

            // Tai khoan mau: admin/123 (vai tro Admin), customer/123 (vai tro Customer)
            // Khach hang moi nen tu dang ky tai /Account/Register
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        UserName = "admin",
                        Password = "123",
                        FullName = "Quản trị viên",
                        PhoneNumber = "0900000000",
                        Role = UserRole.Admin
                    },
                    new User
                    {
                        UserName = "customer",
                        Password = "123",
                        FullName = "Khách hàng demo",
                        PhoneNumber = "0900000001",
                        Role = UserRole.Customer
                    },
                     new User
                     {
                         UserName = "Huy",
                         Password = "huy08112006",
                         FullName = "HuyNguyen",
                         PhoneNumber = "0902637150",
                         Role = UserRole.Customer
                     }
                );
                context.SaveChanges();
            }

            if (!context.Categories.Any())
            {
                var c1 = new Category { Name = "Khai vị", Description = "Các món khai vị nhẹ nhàng" };
                var c2 = new Category { Name = "Món chính", Description = "Các món chính đậm đà" };
                var c3 = new Category { Name = "Tráng miệng", Description = "Món tráng miệng ngọt mát" };
                var c4 = new Category { Name = "Đồ uống", Description = "Nước uống giải khát" };

                context.Categories.AddRange(c1, c2, c3, c4);
                context.SaveChanges();

                context.Dishes.AddRange(
                    new Dish { Name = "Gỏi cuốn tôm thịt", Description = "Gỏi cuốn tươi mát cùng nước chấm đặc biệt", Price = 45000, Category = c1, ImageUrl = "https://res.cloudinary.com/ddi52ejfg/image/upload/v1784357190/goi-cuon-tom-thit-thumbnail-1_vwnygq.jpg", IsAvailable = true },
                    new Dish { Name = "Súp bí đỏ", Description = "Súp bí đỏ kem béo thơm ngậy", Price = 35000, Category = c1, ImageUrl = "https://res.cloudinary.com/ddi52ejfg/image/upload/v1784357241/cach-lam-sup-bi-do-kem-tuoi-beo-ngay-chuan-vi-au-tai-nha-202208251728476970_la5ftq.jpg", IsAvailable = true },
                    new Dish { Name = "Cơm gà xối mỡ", Description = "Cơm gà giòn da ăn kèm dưa leo", Price = 65000, Category = c2, ImageUrl = "https://res.cloudinary.com/ddi52ejfg/image/upload/v1784357297/tha_CC_80nh-pha_CC_89m-2_fz4arm.jpg", IsAvailable = true },
                    new Dish { Name = "Bò lúc lắc", Description = "Bò lúc lắc sốt tiêu đen ăn kèm khoai tây", Price = 95000, Category = c2, ImageUrl = "https://res.cloudinary.com/ddi52ejfg/image/upload/v1784357333/cach-lam-bo-luc-lac-khoai-tay_qowet0.jpg", IsAvailable = true },
                    new Dish { Name = "Cá hồi áp chảo", Description = "Cá hồi Nauy áp chảo sốt bơ chanh", Price = 145000, Category = c2, ImageUrl = "https://res.cloudinary.com/ddi52ejfg/image/upload/v1784357368/2-cach-lam-ca-hoi-ap-chao-sot-bo-chanh-va-sot-cam-thom-ngon-dam-da-huong-vi-16_eve1nq.jpg", IsAvailable = true },
                    new Dish { Name = "Chè khúc bạch", Description = "Chè khúc bạch hạnh nhân thanh mát", Price = 30000, Category = c3, ImageUrl = "https://res.cloudinary.com/ddi52ejfg/image/upload/v1784357405/Che-khuc-bach_btkqpm.jpg", IsAvailable = true },
                    new Dish { Name = "Bánh flan caramel", Description = "Bánh flan mềm mịn vị caramel", Price = 25000, Category = c3, ImageUrl = "https://res.cloudinary.com/ddi52ejfg/image/upload/v1784357443/banh-flan-beo-ngay_pwbruh.jpg", IsAvailable = true },
                    new Dish { Name = "Trà đào cam sả", Description = "Trà đào thơm mát kèm cam và sả", Price = 39000, Category = c4, ImageUrl = "https://res.cloudinary.com/ddi52ejfg/image/upload/v1784357475/cach_lam_tra_dao_cam_sa_2313d177e5_rbjico.webp", IsAvailable = true },
                    new Dish { Name = "Nước ép cam tươi", Description = "Nước cam vắt nguyên chất", Price = 35000, Category = c4, ImageUrl = "https://res.cloudinary.com/ddi52ejfg/image/upload/v1784357508/202405130834075941_ssx1wr.webp", IsAvailable = true }
                );
                context.SaveChanges();
            }
        }
    }
}
