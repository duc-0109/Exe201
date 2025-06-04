using Microsoft.AspNetCore.Mvc;
using SmartCookFinal.Models;
using Microsoft.EntityFrameworkCore;

public class AdminController : Controller
{
    private readonly SmartCookContext _context;

    public AdminController(SmartCookContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult UserManage(string search, int page = 1, int pageSize = 10)
    {
        var query = _context.NguoiDungs.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.TenNguoiDung.Contains(search) || u.Email.Contains(search));
        }

        int totalUsers = query.Count();
        var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

        var users = query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Search = search;

        return View(users);
    }


    public IActionResult EditUser(int id)
    {
        var user = _context.NguoiDungs.Find(id);
        if (user == null)
            return NotFound();
        return View(user);
    }

    public IActionResult DeleteUser(int id)
    {
        var user = _context.NguoiDungs
                           .Include(u => u.Blogs) // nạp bài viết liên quan
                           .FirstOrDefault(u => u.Id == id);

        if (user != null)
        {
            // Xóa các bài viết trước
            if (user.Blogs != null && user.Blogs.Any())
            {
                _context.Blogs.RemoveRange(user.Blogs);
            }

            // Xóa người dùng
            _context.NguoiDungs.Remove(user);
            _context.SaveChanges();
        }

        return RedirectToAction("UserManage");
    }

}
