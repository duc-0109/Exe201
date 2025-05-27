using System.ComponentModel.DataAnnotations;

namespace SmartCookFinal.ModelDTO
{
	public class ThucDonRequestViewModel
	{
		[Required]
		public string GioiTinh { get; set; } // Nam / Nữ
        [Required]
        [Range(10, 120, ErrorMessage = "Tuổi phải từ 10 đến 120")]
        public int Tuoi { get; set; }

        [Required]
        [Range(50, 250, ErrorMessage = "Chiều cao phải từ 50 đến 250 cm")]
        public float ChieuCao { get; set; }

        [Required]
        [Range(20, 200, ErrorMessage = "Cân nặng phải từ 20 đến 200 kg")]
        public float CanNang { get; set; }

        [Range(10000, 10000000, ErrorMessage = "Ngân sách phải từ 10,000 đến 10,000,000 VND")]
        public decimal? NganSachToiDa { get; set; }
        [Required]
		public string MucDoHoatDong { get; set; } // Ít, Vừa, Nhiều

		[Required]
		public string MucTieu { get; set; } // Giảm cân / Giữ cân / Tăng cân

		public string CheDoAn { get; set; } // Chay / Keto / Không Gluten...

		public string DiUng { get; set; } // Dị ứng gì không

		public string KhongThich { get; set; } // Món không thích

		[Required]
		public int SoBuaMotNgay { get; set; } = 3;

	}
}
