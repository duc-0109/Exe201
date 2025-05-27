namespace SmartCookFinal.Models
{
	public class ThucDonNgay
	{
		public int Id { get; set; }
		public int NguoiDungId { get; set; }
		public NguoiDung NguoiDung { get; set; }

		public DateTime Ngay { get; set; }
		public int TongCalo { get; set; }
		public float TongCarbs { get; set; }
		public float TongProtein { get; set; }
		public float TongFat { get; set; }

		public ICollection<ThucDonChiTiet> ThucDonChiTiets { get; set; }
	}

}
