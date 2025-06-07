using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartCookFinal.Models;

namespace SmartCookFinal.Controllers
{
    public class ManageNewsController : Controller
    {
        private readonly SmartCookContext _context;

        public ManageNewsController(SmartCookContext context)
        {
            _context = context;
        }

        // GET: /ManageNews
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            int pageSize = 5;

            IQueryable<News> newsQuery = _context.News
                .Include(n => n.User)
                .Include(n => n.Category); // thêm Category

            if (!string.IsNullOrEmpty(search))
            {
                newsQuery = newsQuery.Where(n => n.Title.Contains(search));
            }

            int totalItems = await newsQuery.CountAsync();

            var pagedNews = await newsQuery
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchText = search;

            return View(pagedNews);
        }

        // GET: /ManageNews/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            ViewBag.Users = new SelectList(_context.NguoiDungs, "Id", "TenNguoiDung");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name"); // thêm
            return View();
        }

        // POST: /ManageNews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(News news, IFormFile ImageFile)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Home");

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                news.UrlImage = "/uploads/" + uniqueFileName;
            }

            news.UserId = userId.Value;
            news.CreatedAt = DateTime.Now;

            _context.Add(news);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /ManageNews/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", news.CategoryId);
            return View(news);
        }

        // POST: /ManageNews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News news, IFormFile imageFile)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            if (id != news.NewsId) return NotFound();

            var existingNews = await _context.News.AsNoTracking().FirstOrDefaultAsync(n => n.NewsId == id);
            if (existingNews == null) return NotFound();

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    news.UrlImage = "/uploads/" + uniqueFileName;
                }
                else
                {
                    news.UrlImage = existingNews.UrlImage;
                }

                news.CreatedAt = existingNews.CreatedAt;
                news.UserId = existingNews.UserId;

                _context.Update(news);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", news.CategoryId);
                return View(news);
            }
        }

        // GET: /ManageNews/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            var news = await _context.News
                .Include(n => n.User)
                .Include(n => n.Category)
                .FirstOrDefaultAsync(n => n.NewsId == id);

            if (news == null) return NotFound();

            return View(news);
        }

        // POST: /ManageNews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return !string.IsNullOrEmpty(role) && role.ToLower() == "admin";
        }
    }
}
