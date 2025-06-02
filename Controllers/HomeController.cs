using Microsoft.AspNetCore.Mvc;
using SmartCookFinal.Models;
using System.Data.Entity;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MimeKit;
namespace SmartCookFinal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SmartCookContext _context; // thêm context ?? truy v?n DB

        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, SmartCookContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
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
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }


        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View();
            }

            // Tìm người dùng theo email
            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View();
            }

            // Kiểm tra tài khoản đã xác thực chưa
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản của bạn chưa được xác thực. Vui lòng kiểm tra email để kích hoạt.");
                return View();
            }

            // Đăng nhập thành công
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.TenNguoiDung);
            HttpContext.Session.SetString("Email", user.Email);

            return RedirectToAction("Index");
        }
        public IActionResult Contact()
        {
            return View();
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
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin.");
                return View(model);
            }

            // Kiểm tra định dạng mật khẩu
            var passwordPattern = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.Password, passwordPattern))
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 8 ký tự, bao gồm cả chữ và số.");
                return View(model);
            }

            // Kiểm tra email đã tồn tại chưa
            var existingUser = _context.NguoiDungs.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
                return View(model);
            }

            // Mã hóa mật khẩu và lưu
            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            model.IsActive = false;

            _context.NguoiDungs.Add(model);
            _context.SaveChanges();

            // Tạo token xác thực
            var token = Guid.NewGuid().ToString();
            var emailConfirm = new EmailConfirmation
            {
                Id = model.Id,
                Token = token,
                ExpirationTime = DateTime.Now.AddHours(24),
                IsConfirmed = false
            };
            _context.EmailConfirmations.Add(emailConfirm);
            _context.SaveChanges();

            // Gửi email xác thực
            SendVerificationEmail(model.Email, token);

            ViewBag.Message = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.";
            return View("Register");
        }


        // Hàm gửi mail
        private void SendVerificationEmail(string email, string token)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SmartCook", "sadboy4102003@gmail.com"));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Xác thực tài khoản SmartCook";

            var link = Url.Action("ConfirmEmail", "Home", new { token = token }, Request.Scheme);

            message.Body = new TextPart("html")
            {
                Text = $"<p>Vui lòng nhấn <a href='{link}'>vào đây</a> để xác thực tài khoản của bạn.</p>" +
                       "<p>Link sẽ hết hạn trong 24 giờ.</p>"
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();

            client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate("sadboy4102003@gmail.com", "qzsf grkb iigm xqkn"); // Thay bằng email & mật khẩu của bạn
            client.Send(message);
            client.Disconnect(true);
        }
        public IActionResult ConfirmEmail(string token)
        {
            var confirmation = _context.EmailConfirmations
                .FirstOrDefault(c => c.Token == token);

            if (confirmation == null || confirmation.IsConfirmed || confirmation.ExpirationTime < DateTime.Now)
            {
                return View("Error", "Link xác thực không hợp lệ hoặc đã hết hạn.");
            }

            confirmation.IsConfirmed = true;
            _context.NguoiDungs.Find(confirmation.Id).IsActive = true;
            _context.SaveChanges();

            ViewBag.Message = "Xác thực email thành công! Bạn có thể đăng nhập.";
            return View("Login");
        }
        [HttpGet]
        public IActionResult ForgotPassword(string sent = null, string error = null)
        {
            ViewData["sent"] = sent;
            ViewData["error"] = error;
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword", new { error = "empty" });
            }

            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                var token = Guid.NewGuid().ToString();

                var resetToken = new PasswordResetToken
                {
                    Id = user.Id, // hoặc Id tùy tên cột của bạn
                    Token = token,
                    ExpirationTime = DateTime.UtcNow.AddHours(1),
                    IsUsed = false
                };

                _context.PasswordResetTokens.Add(resetToken);
                await _context.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Home", new { userId = user.Id, token = token }, Request.Scheme);
                await SendResetEmailAsync(user.Email, resetLink);
            }

            return RedirectToAction("ForgotPassword", new { sent = "true" });
        }


        private async Task SendResetEmailAsync(string email, string resetLink)
        {
            // Cấu hình gửi email, bạn có thể dùng SMTP của Gmail, SendGrid, hoặc dịch vụ khác
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPass"];
            var fromEmail = _configuration["Email:FromEmail"];
            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new Exception("Email FromEmail is not configured.");
            }
            


            var message = new MailMessage();
            message.To.Add(email);

            message.From = new MailAddress(fromEmail);
            message.Subject = "Khôi phục mật khẩu SmartCook";
            message.Body = $"Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng nhấp vào liên kết sau để thiết lập mật khẩu mới:\n\n{resetLink}\n\nNếu bạn không yêu cầu, hãy bỏ qua email này.";
            message.IsBodyHtml = false;

            using (var client = new System.Net.Mail.SmtpClient(smtpHost, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;
                await client.SendMailAsync(message);
            }
        }

        // Action ResetPassword (chỉ ví dụ, bạn cần tạo thêm view và logic xử lý)
        [HttpGet]

        public IActionResult ResetPassword(int userId, string token)
        {
            if (userId <= 0 || string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Link xác thực không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("ForgotPassword");
            }

            var resetToken = _context.PasswordResetTokens
                .FirstOrDefault(p => p.Id == userId && p.Token == token && p.IsUsed == false);

            if (resetToken == null || resetToken.ExpirationTime < DateTime.Now)
            {
                TempData["Error"] = "Link xác thực không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("ForgotPassword");
            }

            // Nếu token hợp lệ, hiển thị form reset mật khẩu
            return View(resetToken);
        }


        [HttpPost]
        public IActionResult ResetPassword(int userId, string token, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không trùng khớp.");
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                return View();
            }

            if (!IsValidPassword(newPassword))
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ và số.");
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                return View();
            }

            var resetToken = _context.PasswordResetTokens
                               .FirstOrDefault(t => t.Token == token && t.Id == userId);

            if (resetToken == null || resetToken.IsUsed || resetToken.ExpirationTime < DateTime.UtcNow)
            {
                ViewBag.Error = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return View("Error");
            }

            var user = _context.NguoiDungs.Find(userId);
            if (user == null)
            {
                ViewBag.Error = "Người dùng không tồn tại.";
                return View("Error");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            // EF6 không có Update, chỉ cần set entity và gọi SaveChanges
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Bạn có thể đăng nhập lại.";
            return RedirectToAction("Login");

        }


        // Hàm kiểm tra mật khẩu hợp lệ (ít nhất 8 ký tự, gồm chữ và số)
        private bool IsValidPassword(string password)
        {
            if (password.Length < 8)
                return false;

            bool hasLetter = false;
            bool hasDigit = false;

            foreach (var c in password)
            {
                if (char.IsLetter(c)) hasLetter = true;
                else if (char.IsDigit(c)) hasDigit = true;

                if (hasLetter && hasDigit) return true;
            }

            return false;
        }
    }
    }
