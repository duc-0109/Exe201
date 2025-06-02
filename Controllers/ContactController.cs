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
        private readonly SmartCookContext _context; // Thay đổi tên DbContext theo project của bạn
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

            // Nếu user đã đăng nhập, tự động điền thông tin từ session hoặc cookie
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                int userId = HttpContext.Session.GetInt32("UserId").Value;
                var user = await _context.NguoiDungs.FindAsync(userId);

                if (user != null)
                {
                    model.Name = user.TenNguoiDung;
                    model.Email = user.Email;
                }
            }

            return View(model);
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
                    ViewBag.Message = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";

                    // Reset form
                    ModelState.Clear();
                    return View("Index", new Contact());
                }
                catch (Exception ex)
                {
                    // Log lỗi (nên sử dụng ILogger trong thực tế)
                    ViewBag.Error = "Có lỗi xảy ra khi gửi yêu cầu. Vui lòng thử lại sau.";
                    return View("Index", contact);
                }
            }

            // Nếu model không hợp lệ, trả về view với errors
            return View("Index", contact);
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
                var toEmail = _configuration["EmailSettings:ToEmail"]; // Email của bạn

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "Smart Cook Website"),
                        Subject = $"[Smart Cook] Liên hệ mới: {contact.Subject}",
                        Body = CreateEmailBody(contact),
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    // Gửi email trả lời tự động cho khách hàng
                    mailMessage.ReplyToList.Add(new MailAddress(contact.Email, contact.Name));

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi email nhưng không ảnh hưởng đến quá trình chính
                // Có thể sử dụng ILogger để log chi tiết
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
                            Liên hệ mới từ Website Smart Cook
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
                            Email này được gửi tự động từ website Smart Cook.<br>
                            Để trả lời khách hàng, vui lòng reply email này hoặc liên hệ trực tiếp qua email: {contact.Email}
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