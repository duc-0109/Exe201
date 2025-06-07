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
        public async Task<IActionResult> Index(string searchText, int? categoryId, int page = 1)
        {
            int pageSize = 5;

            // Load danh sách Category với kiểm tra null
            var categoryList = await _context.Categories.ToListAsync();
            ViewBag.CategoryList = categoryList ?? new List<Category>();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchText = searchText;

            IQueryable<News> newsQuery = _context.News
                .Include(n => n.User)
                .Include(n => n.Category);

            if (!string.IsNullOrEmpty(searchText))
            {
                newsQuery = newsQuery.Where(n => n.Title.Contains(searchText));
            }

            if (categoryId.HasValue && categoryId.Value != 0)
            {
                newsQuery = newsQuery.Where(n => n.CategoryId == categoryId.Value);
            }

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

            // ✅ Tăng lượt xem
            news.ViewCount += 1;
            _context.News.Update(news);
            await _context.SaveChangesAsync();

            // ✅ Lấy các tin gần đây
            var recentNews = await _context.News
                .Where(n => n.NewsId != id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.RecentNews = recentNews;

            return View(news);
        }


    }
}
