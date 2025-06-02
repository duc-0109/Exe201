using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartCookFinal.Models;
using System.Linq;

namespace SmartCookFinal.Controllers
{
    public class UserController : Controller
    {
        private readonly SmartCookContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(SmartCookContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /User/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            _logger.LogInformation($"GET Profile - UserId from session: {userId}");

            if (userId == null)
                return RedirectToAction("Login", "Home");

            var user = _context.NguoiDungs.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
                return NotFound();

            _logger.LogInformation($"Found user: {user.TenNguoiDung}, Email: {user.Email}");
            return View(user);
        }

        // POST: /User/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(NguoiDung model)
        {
            _logger.LogInformation("=== POST Profile Started ===");
            _logger.LogInformation($"Model received - ID: {model.Id}, TenNguoiDung: {model.TenNguoiDung}");
            _logger.LogInformation($"Model - Tuoi: {model.Tuoi}, ChieuCao: {model.ChieuCao}, CanNang: {model.CanNang}");
            _logger.LogInformation($"Model - GioiTinh: {model.GioiTinh}");

            // Kiểm tra session trước
            var userId = HttpContext.Session.GetInt32("UserId");
            _logger.LogInformation($"Session UserId: {userId}");

            if (userId == null)
            {
                _logger.LogWarning("No UserId in session, redirecting to login");
                return RedirectToAction("Login", "Home");
            }

            // Tìm user trong DB
            var user = _context.NguoiDungs.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
            {
                _logger.LogError($"User not found with ID: {userId.Value}");
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Profile");
            }

            _logger.LogInformation($"Found user in DB - ID: {user.Id}, Name: {user.TenNguoiDung}");

            // Loại bỏ validation cho các trường không cần thiết
            ModelState.Remove("Email");
            ModelState.Remove("Password");
            ModelState.Remove("IsActive");
            ModelState.Remove("Blogs");
            ModelState.Remove("Comments");
            ModelState.Remove("ThucDonNgays");
            ModelState.Remove("SoBuaMotNgay");
            ModelState.Remove("MucDoHoatDong");
            ModelState.Remove("MucTieu");
            ModelState.Remove("NganSachToiDa");
            ModelState.Remove("CheDoAn");
            ModelState.Remove("DiUng");
            ModelState.Remove("KhongThich");

            // Kiểm tra ModelState
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    _logger.LogWarning($"ModelState Error - Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return View(user);
            }

            try
            {
                _logger.LogInformation("Starting to update user properties...");

                // Log giá trị cũ
                _logger.LogInformation($"Old values - Name: {user.TenNguoiDung}, Age: {user.Tuoi}, Height: {user.ChieuCao}, Weight: {user.CanNang}, Gender: {user.GioiTinh}");

                // Cập nhật các trường
                user.TenNguoiDung = model.TenNguoiDung;
                user.GioiTinh = model.GioiTinh;
                user.Tuoi = model.Tuoi;
                user.ChieuCao = model.ChieuCao;
                user.CanNang = model.CanNang;

                // Log giá trị mới
                _logger.LogInformation($"New values - Name: {user.TenNguoiDung}, Age: {user.Tuoi}, Height: {user.ChieuCao}, Weight: {user.CanNang}, Gender: {user.GioiTinh}");

                // Kiểm tra có thay đổi không
                var hasChanges = _context.ChangeTracker.HasChanges();
                _logger.LogInformation($"Has changes: {hasChanges}");

                // Lưu thay đổi
                _logger.LogInformation("Calling SaveChanges...");
                var changeCount = _context.SaveChanges();
                _logger.LogInformation($"SaveChanges completed. Changes saved: {changeCount}");

                TempData["Success"] = "Cập nhật thông tin thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
                return View(user);
            }

            _logger.LogInformation("=== POST Profile Completed ===");
            return RedirectToAction("Profile");
        }
    }
}