namespace SmartCookFinal.Models
{
	public class NguoiDung
	{
		public int Id { get; set; }
		public string TenNguoiDung { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string? GioiTinh { get; set; }
        public int? Tuoi { get; set; }
        public float? ChieuCao { get; set; }
        public float? CanNang { get; set; }
        public int? SoBuaMotNgay { get; set; }
        public string? MucDoHoatDong { get; set; }
		public string? MucTieu { get; set; }
		public decimal? NganSachToiDa { get; set; }
		public string? CheDoAn { get; set; }
		public string? DiUng { get; set; }
		public string? KhongThich { get; set; }

		public ICollection<ThucDonNgay> ThucDonNgays { get; set; }

        public ICollection<Blog> Blogs { get; set; }
        public ICollection<BlogComment> Comments { get; set; }
    }

}
