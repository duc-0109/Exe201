namespace SmartCookFinal.Models
{
	public class DanhMucMonAn
	{
		public int Id { get; set; }
		public string TenDanhMuc { get; set; }

		public ICollection<MonAn> MonAns { get; set; }
	}

}
