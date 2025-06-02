using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartCookFinal.Models;

namespace SmartCookFinal.Controllers
{
    public class NewsController : Controller
    {
        private readonly SmartCookContext _context;

        public NewsController(SmartCookContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchText, int page = 1)
        {
            int pageSize = 4;
            IQueryable<Blog> blogs = _context.Blogs.Include(b => b.User);

            if (!string.IsNullOrEmpty(searchText))
            {
                blogs = blogs.Where(b => b.Title.Contains(searchText));
            }

            ViewBag.SearchText = searchText;

            int totalItems = await blogs.CountAsync();
            var pagedBlogs = await blogs
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(pagedBlogs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Comments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(b => b.BlogId == id);

            if (blog == null) return NotFound();

            var recentPosts = await _context.Blogs
                .Where(b => b.BlogId != id)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.RecentPosts = recentPosts;

            return View(blog);
        }
    }
}
