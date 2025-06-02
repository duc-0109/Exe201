using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace SmartCookFinal.Models
{
    public class NguoiDung
    {
        public NguoiDung()
        {
            // Khởi tạo collections để tránh lỗi required
            ThucDonNgays = new List<ThucDonNgay>();
            Blogs = new List<Blog>();
            Comments = new List<BlogComment>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được quá 100 ký tự")]
        public string TenNguoiDung { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public string Password { get; set; }
        public string? GioiTinh { get; set; }

        [Range(1, 120, ErrorMessage = "Tuổi phải từ 1 đến 120")]
        public int? Tuoi { get; set; }

        [Range(50, 250, ErrorMessage = "Chiều cao phải từ 50cm đến 250cm")]
        public float? ChieuCao { get; set; }

        [Range(20, 300, ErrorMessage = "Cân nặng phải từ 20kg đến 300kg")]
        public float? CanNang { get; set; }

        public int? SoBuaMotNgay { get; set; }
        public string? MucDoHoatDong { get; set; }
        public string? MucTieu { get; set; }
        public decimal? NganSachToiDa { get; set; }
        public string? CheDoAn { get; set; }
        public string? DiUng { get; set; }
        public string? KhongThich { get; set; }
        public bool IsActive { get; set; } = false;

        // Navigation properties - giữ nullable nhưng đã khởi tạo trong constructor
        public ICollection<ThucDonNgay>? ThucDonNgays { get; set; }
        public ICollection<Blog>? Blogs { get; set; }
        public ICollection<BlogComment>? Comments { get; set; }
    }
}