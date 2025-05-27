namespace SmartCookFinal.Models
{
	public class MonAn
	{
        public int Id { get; set; }
        public string? TenMon { get; set; }
        public string? MoTa { get; set; }
        public string? LoaiBuaAn { get; set; }
        public int? ThoiGianNau { get; set; }
        public int? LuongCalo { get; set; }
		public double? Carbs { get; set; }
		public double? Protein { get; set; }
		public double? Fat { get; set; }
		public decimal? ChiPhiUocTinh { get; set; }
        public bool? Chay { get; set; }
        public bool? AnKeto { get; set; }
        public bool? AnKhongGluten { get; set; }
        public string? NguyenLieuChinh { get; set; }
        public string? DinhDuongChiTiet { get; set; }
        public string? UrlHinhAnh { get; set; }
        public string? CachNau { get; set; }
        public bool? TrangThai { get; set; }

        public int? DanhMucId { get; set; }
		public DanhMucMonAn DanhMuc { get; set; }

		public ICollection<MonAnNguyenLieu> MonAnNguyenLieus { get; set; }
		public ICollection<ThucDonChiTiet> ThucDonChiTiets { get; set; }
	}

}
