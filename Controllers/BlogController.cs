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
        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs.Include(b => b.User).ToListAsync();
            return View(blogs);
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
