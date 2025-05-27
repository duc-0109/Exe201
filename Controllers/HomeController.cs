using Microsoft.AspNetCore.Mvc;
using SmartCookFinal.Models;
using System.Data.Entity;
using System.Diagnostics;

namespace SmartCookFinal.Controllers
{
	public class HomeController : Controller
	{
        private readonly ILogger<HomeController> _logger;
        private readonly SmartCookContext _context; // thêm context ?? truy v?n DB

        public HomeController(ILogger<HomeController> logger, SmartCookContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
		{
			var fixedMonAnList = _context.MonAns
			   .Include(m => m.DanhMuc)
			   .Where(m => m.Id >= 74 && m.Id <= 77)
			   .OrderBy(m => m.Id)
			   .ToList();

			return View(fixedMonAnList);
		}

		public IActionResult Index1()
		{
			return View();
		}



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email ho?c m?t kh?u không dúng");
                return View();
            }

            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user == null)
            {
                ModelState.AddModelError("", "Email ho?c m?t kh?u không dúng");
                return View();
            }

            // ??ng nh?p thành công
            HttpContext.Session.SetInt32("UserId", user.Id);
			HttpContext.Session.SetString("UserName", user.TenNguoiDung);
			return RedirectToAction("Index");
        }


        public IActionResult Logout()
        {
			HttpContext.Session.Clear();
			return RedirectToAction("Index");
        }
        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}


		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Register(NguoiDung model)
		{
            if (string.IsNullOrEmpty(model.TenNguoiDung) ||
       string.IsNullOrEmpty(model.Email) ||
       string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("", "Vui lòng ?i?n ??y ?? thông tin.");
                return View(model);
            }

            // Ki?m tra email ?ã t?n t?i ch?a
            var existingUser = _context.NguoiDungs.FirstOrDefault(u => u.Email == model.Email);
			if (existingUser != null)
			{
				ModelState.AddModelError("Email", "Email t?n t?i.");
				return View(model);
			}

			_context.NguoiDungs.Add(model);
			_context.SaveChanges();

			// T? ??ng ??ng nh?p sau khi ??ng ký thành công
			HttpContext.Session.SetInt32("UserId", model.Id);
			HttpContext.Session.SetString("UserName", model.TenNguoiDung);

			return RedirectToAction("Index");
		}

	}
}
