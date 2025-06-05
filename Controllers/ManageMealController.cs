using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartCookFinal.Models;

namespace SmartCookFinal.Controllers
{
    public class ManageMealController : Controller
    {
        private readonly SmartCookContext _context;

        public ManageMealController(SmartCookContext context)
        {
            _context = context;
        }

        // GET: /ManageMeal
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            int pageSize = 5;
            var query = _context.MonAns.Include(m => m.DanhMuc).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.TenMon.Contains(search));
            }

            int totalItems = await query.CountAsync();
            var meals = await query
                .OrderByDescending(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchText = search;

            return View(meals);
        }

        // GET: /ManageMeal/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            ViewBag.DanhMucList = new SelectList(_context.DanhMucMonAns, "Id", "TenDanhMuc");
            return View();
        }

        // POST: /ManageMeal/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonAn meal, IFormFile imageFile)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                meal.UrlHinhAnh = "/uploads/" + fileName;
            }

            _context.MonAns.Add(meal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /ManageMeal/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            var meal = await _context.MonAns.FindAsync(id);
            if (meal == null) return NotFound();

            ViewBag.DanhMucList = new SelectList(_context.DanhMucMonAns, "Id", "TenDanhMuc", meal.DanhMucId);
            return View(meal);
        }

        // POST: /ManageMeal/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MonAn meal, IFormFile imageFile)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            if (id != meal.Id) return NotFound();

            var existingMeal = await _context.MonAns.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (existingMeal == null) return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                meal.UrlHinhAnh = "/uploads/" + fileName;
            }
            else
            {
                meal.UrlHinhAnh = existingMeal.UrlHinhAnh;
            }

            try
            {
                _context.Update(meal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.DanhMucList = new SelectList(_context.DanhMucMonAns, "Id", "TenDanhMuc", meal.DanhMucId);
                return View(meal);
            }
        }

        // GET: /ManageMeal/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            var meal = await _context.MonAns
                .Include(m => m.DanhMuc)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meal == null) return NotFound();

            return View(meal);
        }

        // POST: /ManageMeal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Home");

            var meal = await _context.MonAns.FindAsync(id);
            if (meal != null)
            {
                _context.MonAns.Remove(meal);
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
