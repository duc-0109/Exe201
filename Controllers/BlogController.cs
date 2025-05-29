using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartCookFinal.Models;

namespace SmartCookFinal.Controllers
{
    // Controllers/BlogController.cs
    public class BlogController : Controller
    {
        private readonly SmartCookContext _context;

        public BlogController(SmartCookContext context)
        {
            _context = context;
        }

        // GET: /Blog

        public async Task<IActionResult> Index(string searchText, int page = 1)
        {
            int pageSize = 4; // Số bài viết mỗi trang

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





        // GET: /Blog/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Comments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(b => b.BlogId == id);

            if (blog == null)
                return NotFound();

            // Lấy 3 bài viết mới nhất (không tính bài hiện tại)
            var recentPosts = await _context.Blogs
                .Where(b => b.BlogId != id)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.RecentPosts = recentPosts;

            return View(blog);
        }


        // GET: /Blog/Create
        public IActionResult Create()
        {
            ViewBag.Users = new SelectList(_context.NguoiDungs, "Id", "TenNguoiDung");
            return View();
        }

        // POST: /Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {

            blog.CreatedAt = DateTime.Now;
            _context.Add(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


            //ViewBag.Users = new SelectList(_context.Users, "UserId", "Username", blog.UserId);
            //return View(blog);
        }

        // GET: /Blog/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return NotFound();

            ViewBag.Users = new SelectList(_context.NguoiDungs, "Id", "TenNguoiDung", blog.UserId);
            return View(blog);
        }

        // POST: /Blog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Blog blog)
        {

            ViewBag.Users = new SelectList(_context.NguoiDungs, "Id", "TenNguoiDung", blog.UserId);
            // return View(blog);


            try
            {
                _context.Update(blog);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Blogs.Any(e => e.BlogId == blog.BlogId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Blog/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BlogId == id);

            if (blog == null)
                return NotFound();

            return View(blog);
        }

        // POST: /Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

       
    }

}
