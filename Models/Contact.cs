using System;
using System.ComponentModel.DataAnnotations;

namespace SmartCookFinal.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
