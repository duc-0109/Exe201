namespace SmartCookFinal.Models
{
	public class ThucDonChiTiet
	{
		public int Id { get; set; }

		public int ThucDonNgayId { get; set; }
		public ThucDonNgay ThucDonNgay { get; set; }

		public int MonAnId { get; set; }
		public MonAn MonAn { get; set; }

		public string LoaiBua { get; set; } // Sáng, Trưa, Tối, Phụ
		public string GhiChu { get; set; }
	}

}
