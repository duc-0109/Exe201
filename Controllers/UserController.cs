using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartCookFinal.Models;
using System.Linq;

namespace SmartCookFinal.Controllers
{
    public class UserController : Controller
    {
        private readonly SmartCookContext _context;

        public UserController(SmartCookContext context)
        {
            _context = context;
        }

        // GET: /User/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Home");

            var user = _context.NguoiDungs.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: /User/Profile
        [HttpPost]
        public IActionResult Profile(NguoiDung model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Tìm user trong DB
            var user = _context.NguoiDungs.FirstOrDefault(u => u.Id == model.Id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Profile");
            }

            // Cập nhật các trường được phép
            user.TenNguoiDung = model.TenNguoiDung;
            user.GioiTinh = model.GioiTinh;
            user.ChieuCao = model.ChieuCao;
            user.CanNang = model.CanNang;

            try
            {
                _context.Update(user);
                _context.SaveChanges();

                TempData["Success"] = "Cập nhật thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
            }

            return RedirectToAction("Profile", new { id = model.Id });
        }

    }
}
