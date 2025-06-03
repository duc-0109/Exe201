using SmartCookFinal.Models;

namespace SmartCookFinal.ModelDTO
{
    public class ThucDonTheoNgayViewModel
    {
        public DateTime Ngay { get; set; }
        public List<MonAn> DanhSachMonAn { get; set; }
    }

}
