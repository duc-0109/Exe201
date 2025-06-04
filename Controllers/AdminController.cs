using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCookFinal.Models;

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

        return View("UserDetail", user);
    }

    [HttpPost]
    public IActionResult UpdateUserDetail(NguoiDung updatedUser)
    {
        var existingUser = _context.NguoiDungs.Find(updatedUser.Id);
        if (existingUser == null)
            return NotFound();

        // Cập nhật thông tin
        _context.Entry(existingUser).CurrentValues.SetValues(updatedUser);
        _context.SaveChanges();
        TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
        return RedirectToAction("UserManage");
        
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
    public IActionResult ContactList(string search, int page = 1, int pageSize = 10)
    {
        var query = _context.Contacts.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Name.Contains(search) || c.Email.Contains(search) || c.Subject.Contains(search));
        }

        int totalContacts = query.Count();
        var totalPages = (int)Math.Ceiling(totalContacts / (double)pageSize);

        var contacts = query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Search = search;

        return View(contacts);
    }
    [HttpPost]
    public IActionResult ToggleReplyStatus(int id)
    {
        var contact = _context.Contacts.FirstOrDefault(c => c.Id == id);
        if (contact != null)
        {
            contact.IsReplied = !contact.IsReplied;
            _context.SaveChanges();
        }
        return RedirectToAction("ContactList");
    }

}


