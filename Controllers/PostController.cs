using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartCookFinal.Models;
using System.Security.Claims;


namespace SmartCookFinal.Controllers
{
    public class PostController : Controller
    {
        private readonly SmartCookContext _context;
        private const int PageSize = 5; // Số bài viết mỗi trang

        public PostController(SmartCookContext context)
        {
            _context = context;
        }

        //Index
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var query = _context.Blogs
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.Contains(search) || b.Content.Contains(search));
            }

            int totalItems = await query.CountAsync();
            var blogs = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            return View(blogs);
        }



        // GET: /Post/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BlogId == id);

            if (blog == null) return NotFound();

            return View(blog);
        }

        // POST: /Post/Detail/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Detail(int id, bool isChecked)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound();

            blog.isChecked = isChecked;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.Comments) // Load luôn Comments để xóa
                .FirstOrDefaultAsync(b => b.BlogId == id);

            if (blog == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài viết.";
                return RedirectToAction(nameof(Index));
            }

            // Xóa toàn bộ comment của bài viết
            if (blog.Comments != null && blog.Comments.Any())
            {
                _context.BlogComments.RemoveRange(blog.Comments);
            }

            // Xóa bài viết
            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bài viết đã được xóa.";
            return RedirectToAction(nameof(Index));
        }




    }
}
