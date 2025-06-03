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

        // GET: /News
        public async Task<IActionResult> Index(string searchText, int page = 1)
        {
            int pageSize = 4;
            IQueryable<News> newsQuery = _context.News.Include(n => n.User);

            if (!string.IsNullOrEmpty(searchText))
            {
                newsQuery = newsQuery.Where(n => n.Title.Contains(searchText));
            }

            ViewBag.SearchText = searchText;

            int totalItems = await newsQuery.CountAsync();
            var pagedNews = await newsQuery
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(pagedNews);
        }

        // GET: /News/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var news = await _context.News
                .Include(n => n.User)
               
                .FirstOrDefaultAsync(n => n.NewsId == id);

            if (news == null)
                return NotFound();

            var recentNews = await _context.News
                .Where(n => n.NewsId != id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.RecentNews = recentNews;

            return View(news);
        }

        // GET: /News/Create
        public IActionResult Create()
        {
            return View();
        }

        //// POST: /News/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(News news, IFormFile ImageFile)
        //{
        //    var userId = HttpContext.Session.GetInt32("UserId");
        //    if (userId == null)
        //        return RedirectToAction("Login", "Home");

        //    if (ImageFile != null && ImageFile.Length > 0)
        //    {
        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        //        Directory.CreateDirectory(uploadsFolder);

        //        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await ImageFile.CopyToAsync(stream);
        //        }

        //        news.UrlImage = "/uploads/" + uniqueFileName;
        //    }

        //    news.UserId = userId.Value;
        //    news.CreatedAt = DateTime.Now;

        //    _context.Add(news);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(MyNews));
        //}

        // GET: /News/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();

            return View(news);
        }

        //// POST: /News/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, News news, IFormFile ImageFile)
        //{
        //    if (id != news.NewsId) return NotFound();

        //    var existing = await _context.News.AsNoTracking().FirstOrDefaultAsync(n => n.NewsId == id);
        //    if (existing == null) return NotFound();

        //    if (ImageFile != null && ImageFile.Length > 0)
        //    {
        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        //        Directory.CreateDirectory(uploadsFolder);

        //        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await ImageFile.CopyToAsync(stream);
        //        }

        //        news.UrlImage = "/uploads/" + uniqueFileName;
        //    }
        //    else
        //    {
        //        news.UrlImage = existing.UrlImage;
        //    }

        //    news.UserId = existing.UserId;
        //    news.CreatedAt = existing.CreatedAt;

        //    _context.Update(news);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(MyNews));
        //}

        // GET: /News/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _context.News.Include(n => n.User).FirstOrDefaultAsync(n => n.NewsId == id);
            if (news == null) return NotFound();

            return View(news);
        }

        //// POST: /News/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var news = await _context.News.FindAsync(id);
        //    if (news != null)
        //    {
        //        _context.News.Remove(news);
        //        await _context.SaveChangesAsync();
        //    }
        //    return RedirectToAction(nameof(MyNews));
        //}

        //public async Task<IActionResult> MyNews()
        //{
        //    int? userId = HttpContext.Session.GetInt32("UserId");
        //    if (userId == null) return RedirectToAction("Login", "Home");

        //    var userNews = await _context.News
        //        .Include(n => n.User)
        //        .Where(n => n.UserId == userId)
        //        .OrderByDescending(n => n.CreatedAt)
        //        .ToListAsync();

        //    return View(userNews);
        //}

       

        
    }
}
