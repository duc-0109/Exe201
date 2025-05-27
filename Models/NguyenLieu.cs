namespace SmartCookFinal.Models
{
	public class NguyenLieu
	{
		public int Id { get; set; }
		public string TenNguyenLieu { get; set; }
		public string DonViTinh { get; set; }
		public float Calo { get; set; }
		public float Carbs { get; set; }
		public float Protein { get; set; }
		public float Fat { get; set; }
		public bool TrangThai { get; set; }

		public ICollection<MonAnNguyenLieu> MonAnNguyenLieus { get; set; }
	}

}
