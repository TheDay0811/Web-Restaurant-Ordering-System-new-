using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;
using System.Security.Claims;

namespace RestaurantOrderingSystem.Controllers
{
    // Dang ky / Dang nhap / Dang xuat
    // Tai khoan duoc luu that trong Database (bang Users), khong con hard-code nhu ban dau.
    // Tai khoan mau: admin / 123 (Admin) - customer / 123 (Customer)
    // Khach hang moi tu dang ky qua /Account/Register se luon duoc gan vai tro Customer.
    public class AccountController : Controller
    {
        private readonly DataContext context;

        public AccountController(DataContext ctx)
        {
            context = ctx;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginViewModel model = new LoginViewModel();

            model.UserName = Request.Cookies["UserName"] ?? string.Empty;
            if (!string.IsNullOrEmpty(model.UserName))
                model.RememberMe = true;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);

            if (user == null || user.Password != model.Password)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
                return View(model);
            }

            SaveLoginCookie(model);
            await SignIn(user);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool existed = await context.Users.AnyAsync(u => u.UserName == model.UserName);
            if (existed)
            {
                ModelState.AddModelError(nameof(model.UserName), "Tên đăng nhập đã tồn tại, vui lòng chọn tên khác");
                return View(model);
            }

            var user = new User
            {
                UserName = model.UserName,
                Password = model.Password,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Role = UserRole.Customer
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            await SignIn(user);
            TempData["Success"] = $"Đăng ký thành công! Chào mừng {user.FullName} đến với Nhà Hàng Online.";
            return RedirectToAction("Index", "Home");
        }

        // Dang nhap bang Cookie Authentication: dong goi thong tin tai khoan thanh
        // cac "Claim" (mau tin nho: ai, ten gi, vai tro gi...), roi luu vao 1 cookie
        // ma trinh duyet tu dong gui kem moi request sau do. Nho vay cac noi khac
        // trong code co the doc lai bang User.Identity.Name, User.IsInRole(...),
        // hay User.FindFirstValue(ClaimTypes.NameIdentifier) ma khong can truy van
        // lai database moi lan. RoleId (NameIdentifier) dung de lien ket Order.UserId
        // va kiem tra quyen xem hoa don trong OrderController.
        private async Task SignIn(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, user.FullName),
                new Claim(ClaimTypes.Role, user.Role) // dung boi [Authorize(Roles = ...)]
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal); // ghi cookie dang nhap vao trinh duyet
        }

        private void SaveLoginCookie(LoginViewModel model)
        {
            if (model.RememberMe)
            {
                CookieOptions opt = new CookieOptions { Expires = DateTime.Now.AddDays(7) };
                Response.Cookies.Append("UserName", model.UserName, opt);
            }
            else
            {
                Response.Cookies.Delete("UserName");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
