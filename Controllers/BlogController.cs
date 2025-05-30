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
        [HttpPost]
        public async Task<IActionResult> Create(Blog blog, IFormFile ImageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Xử lý ảnh upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                blog.UrlImage = "/uploads/" + uniqueFileName; // Lưu đường dẫn tương đối
            }

            blog.UserId = userId.Value;
            blog.CreatedAt = DateTime.Now;
            blog.isChecked = true;

            _context.Add(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBlog));
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound();

            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog, IFormFile imageFile)
        {
            if (id != blog.BlogId) return NotFound();

            var existingBlog = await _context.Blogs.AsNoTracking().FirstOrDefaultAsync(b => b.BlogId == id);
            if (existingBlog == null) return NotFound();

                try
                {
                    // Upload ảnh nếu có file mới
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        Directory.CreateDirectory(uploadsFolder); // tạo thư mục nếu chưa có

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        blog.UrlImage = "/uploads/" + uniqueFileName;
                    }
                    else
                    {
                        blog.UrlImage = existingBlog.UrlImage; // giữ ảnh cũ nếu không upload mới
                    }

                    // Giữ lại CreatedAt, isChecked, UserId từ bản gốc
                    blog.CreatedAt = existingBlog.CreatedAt;
                    blog.isChecked = existingBlog.isChecked;
                    blog.UserId = existingBlog.UserId;

                    _context.Update(blog);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("MyBlog");
                }
                catch
                {
                    return View(blog);
                }
            

            //return View(blog);
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

        public async Task<IActionResult> MyBlog()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect nếu chưa đăng nhập
            }

            var userBlogs = await _context.Blogs
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View("MyBlog", userBlogs); // Truyền model đến view MyBlog.cshtml
        }

    }

}
