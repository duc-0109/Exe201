using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCookFinal.Models;
using SmartCookFinal.Controllers;
using System.Net;
using System.Net.Mail;

namespace SmartCookFinal.Controllers
{
    public class ContactController : Controller
    {
        private readonly SmartCookContext _context;
        private readonly IConfiguration _configuration;

        public ContactController(SmartCookContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Contact
        public async Task<IActionResult> Index()
        {
            var model = new Contact();

            // Nếu user đã đăng nhập, tự động điền thông tin từ session
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var userName = HttpContext.Session.GetString("UserName");
                var userEmail = HttpContext.Session.GetString("UserEmail");

                // Debug: In ra console để kiểm tra
                Console.WriteLine($"Debug - UserId: {userId}, UserName: {userName}, UserEmail: {userEmail}");

                // Nếu email không có trong session, lấy từ database
                if (string.IsNullOrEmpty(userEmail) && userId.HasValue)
                {
                    try
                    {
                        var user = await _context.NguoiDungs.FindAsync(userId.Value);
                        if (user != null)
                        {
                            userEmail = user.Email;
                            userName = user.TenNguoiDung ?? user.TenNguoiDung ?? userName; // Cập nhật cả tên nếu cần

                            // Lưu lại vào session để lần sau không phải query lại
                            HttpContext.Session.SetString("UserEmail", userEmail);
                            if (!string.IsNullOrEmpty(userName))
                            {
                                HttpContext.Session.SetString("UserName", userName);
                            }

                            Console.WriteLine($"Debug - Lấy từ DB - UserName: {userName}, UserEmail: {userEmail}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi lấy thông tin user: {ex.Message}");
                    }
                }

                // Điền thông tin vào model
                if (!string.IsNullOrEmpty(userName))
                {
                    model.Name = userName;
                }
                if (!string.IsNullOrEmpty(userEmail))
                {
                    model.Email = userEmail;
                }

                // Set ViewBag để View có thể sử dụng
                ViewBag.IsLoggedIn = true;
                ViewBag.LoggedInUserName = userName ?? "";
                ViewBag.LoggedInUserEmail = userEmail ?? "";

                Console.WriteLine($"Debug - ViewBag - Name: {ViewBag.LoggedInUserName}, Email: {ViewBag.LoggedInUserEmail}");
            }
            else
            {
                ViewBag.IsLoggedIn = false;
            }

            return View("~/Views/Home/Contact.cshtml", model);
        }


        // POST: Contact/SendContact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendContact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Đặt thời gian tạo
                    contact.CreatedAt = DateTime.Now;

                    // Thêm vào database
                    _context.Contacts.Add(contact);
                    await _context.SaveChangesAsync();

                    // Gửi email
                    await SendEmailAsync(contact);

                    // Thông báo thành công
                    TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";

                    // Redirect để tránh double submit
                    return RedirectToAction("Index");

                }
                catch (Exception ex)
                {
                    // Log lỗi (nên sử dụng ILogger trong thực tế)
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi yêu cầu. Vui lòng thử lại sau.";
                    Console.WriteLine($"Lỗi gửi contact: {ex.Message}");
                }
            }

            // Nếu có lỗi, giữ lại thông tin user đã đăng nhập
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                ViewBag.IsLoggedIn = true;
                ViewBag.LoggedInUserName = HttpContext.Session.GetString("UserName") ?? "";
                ViewBag.LoggedInUserEmail = HttpContext.Session.GetString("UserEmail") ?? "";
            }

            // Nếu model không hợp lệ, trả về view với errors
            return View("~/Views/Home/Contact.cshtml", contact);
        }

        private async Task SendEmailAsync(Contact contact)
        {
            try
            {
                // Cấu hình email từ appsettings.json
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var toEmail = _configuration["EmailSettings:ToEmail"]; 

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true;

                    // Email gửi tới admin
                    var adminMailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "An Thực Website"),
                        Subject = $"[An Thực] Liên hệ mới: {contact.Subject}",
                        Body = CreateEmailBody(contact),
                        IsBodyHtml = true
                    };
                    adminMailMessage.To.Add(toEmail);
                    adminMailMessage.ReplyToList.Add(new MailAddress(contact.Email, contact.Name));

                    await client.SendMailAsync(adminMailMessage);

                    // Email xác nhận gửi tới khách hàng
                    var customerMailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "An Thực Website"),
                        Subject = "Xác nhận đã nhận yêu cầu liên hệ",
                        Body = CreateCustomerConfirmationEmail(contact),
                        IsBodyHtml = true
                    };
                    customerMailMessage.To.Add(contact.Email);

                    await client.SendMailAsync(customerMailMessage);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi email nhưng không ảnh hưởng đến quá trình chính
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
            }
        }

        private string CreateEmailBody(Contact contact)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                        <h2 style='color: #007bff; border-bottom: 2px solid #007bff; padding-bottom: 10px;'>
                            Liên hệ mới từ Website An Thực
                        </h2>
                        
                        <div style='margin: 20px 0;'>
                            <h3 style='color: #333; margin-bottom: 15px;'>Thông tin người liên hệ:</h3>
                            
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 0; font-weight: bold; color: #555; width: 120px;'>Họ và tên:</td>
                                    <td style='padding: 8px 0; color: #333;'>{contact.Name}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; font-weight: bold; color: #555;'>Email:</td>
                                    <td style='padding: 8px 0; color: #333;'>{contact.Email}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; font-weight: bold; color: #555;'>Chủ đề:</td>
                                    <td style='padding: 8px 0; color: #333;'>{contact.Subject}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; font-weight: bold; color: #555;'>Thời gian:</td>
                                    <td style='padding: 8px 0; color: #333;'>{contact.CreatedAt:dd/MM/yyyy HH:mm}</td>
                                </tr>
                            </table>
                        </div>

                        <div style='margin: 20px 0;'>
                            <h3 style='color: #333; margin-bottom: 15px;'>Nội dung liên hệ:</h3>
                            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; border-left: 4px solid #007bff;'>
                                {contact.Message.Replace("\n", "<br>")}
                            </div>
                        </div>

                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #eee;'>
                        
                        <p style='color: #666; font-size: 12px; margin: 0;'>
                            Email này được gửi tự động từ website An Thực.<br>
                            Để trả lời khách hàng, vui lòng reply email này hoặc liên hệ trực tiếp qua email: {contact.Email}
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string CreateCustomerConfirmationEmail(Contact contact)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                        <h2 style='color: #007bff; border-bottom: 2px solid #007bff; padding-bottom: 10px;'>
                            An Thực - Xác nhận yêu cầu liên hệ
                        </h2>
                        
                        <p style='color: #333; font-size: 16px;'>Xin chào <strong>{contact.Name}</strong>,</p>
                        
                        <p style='color: #333;'>
                            Cảm ơn bạn đã liên hệ với chúng tôi. Chúng tôi đã nhận được yêu cầu của bạn với thông tin như sau:
                        </p>

                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p style='margin: 5px 0;'><strong>Chủ đề:</strong> {contact.Subject}</p>
                            <p style='margin: 5px 0;'><strong>Thời gian gửi:</strong> {contact.CreatedAt:dd/MM/yyyy HH:mm}</p>
                        </div>

                        <p style='color: #333;'>
                            Chúng tôi sẽ xem xét và phản hồi yêu cầu của bạn trong thời gian sớm nhất có thể.
                        </p>

                        <p style='color: #333;'>
                            Trân trọng,<br>
                            <strong>Đội ngũ An Thực</strong>
                        </p>

                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #eee;'>
                        
                        <p style='color: #666; font-size: 12px; margin: 0;'>
                            Đây là email tự động, vui lòng không trả lời email này.<br>
                            Nếu có thắc mắc, bạn có thể liên hệ lại qua form liên hệ trên website.
                        </p>
                    </div>
                </body>
                </html>";
        }

        // GET: Contact/Admin (Để admin xem danh sách liên hệ)
        public async Task<IActionResult> Admin()
        {
            var contacts = await _context.Contacts
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(contacts);
        }

        // GET: Contact/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contact/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Admin));
        }

        // POST: Contact/MarkAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                // Nếu muốn thêm trường IsRead vào model
                // contact.IsRead = true;
                // await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Admin));
        }
    }
}