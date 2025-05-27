namespace SmartCookFinal.Models
{
	public class MonAnNguyenLieu
	{
		public int Id { get; set; }

		public int MonAnId { get; set; }
		public MonAn MonAn { get; set; }

		public int NguyenLieuId { get; set; }
		public NguyenLieu NguyenLieu { get; set; }

		public float SoLuong { get; set; }
		public string DonVi { get; set; }
	}

}
