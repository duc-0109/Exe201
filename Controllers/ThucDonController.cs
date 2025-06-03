using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCookFinal.ModelDTO;
using SmartCookFinal.Models;

namespace SmartCookFinal.Controllers
{
    public class ThucDonController : Controller
    {

        private readonly SmartCookContext _context;
        public ThucDonController(SmartCookContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult NhapChiSo()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine("UserId trong session: " + userId);

            if (userId.HasValue)
            {
                // Lấy danh sách thực đơn trong 7 ngày gần nhất của người dùng
                var thucDonTrongTuan = _context.ThucDonNgays
                    .Include(t => t.ThucDonChiTiets)
                        .ThenInclude(ct => ct.MonAn)
                    .Where(x => x.NguoiDungId == userId && x.Ngay >= DateTime.Today.AddDays(-6))
                    .OrderBy(x => x.Ngay)
                    .ToList();

                if (thucDonTrongTuan.Any())
                {
                    var thucDon7Ngay = thucDonTrongTuan.Select(td => new ThucDonTheoNgayViewModel
                    {
                        Ngay = td.Ngay,
                        DanhSachMonAn = td.ThucDonChiTiets.Select(ct => ct.MonAn).ToList()
                    }).ToList();

                    var tongCalo = 0;
                    var tongSoBua = thucDonTrongTuan.Sum(td => td.ThucDonChiTiets?.Count ?? 0);
                    var soNgay = thucDon7Ngay.Count;

                    var ketQua = new ThucDonKetQuaViewModel
                    {
                        BMR = 0,
                        TDEE = 0,
                        TongCaloDieuChinh = tongCalo,
                        CaloMoiBua = tongSoBua > 0 ? tongCalo / (tongSoBua * 1.0) : 0,
                        SoBuaMotNgay = soNgay > 0 ? tongSoBua / soNgay : 0
                    };

                    var viewTongHop = new ThucDonViewTongHop
                    {
                        ThucDon7Ngay = thucDon7Ngay,
                        KetQua = ketQua
                    };

                    return View("TaoThucDon", viewTongHop);
                }

                // Nếu chưa có thực đơn
                var user = _context.NguoiDungs.Find(userId.Value);
                if (user != null)
                {
                    var model = new ThucDonRequestViewModel
                    {
                        GioiTinh = user.GioiTinh,
                        Tuoi = user.Tuoi ?? 0,
                        ChieuCao = user.ChieuCao ?? 0,
                        CanNang = user.CanNang ?? 0,
                        MucDoHoatDong = user.MucDoHoatDong,
                        MucTieu = user.MucTieu,
                        SoBuaMotNgay = user.SoBuaMotNgay ?? 3,
                        NganSachToiDa = user.NganSachToiDa ?? 0,
                        CheDoAn = user.CheDoAn,
                        DiUng = user.DiUng,
                        KhongThich = user.KhongThich
                    };

                    return View(model);
                }
            }


            return RedirectToAction("Login", "Home");

        }




        [HttpPost]
        public IActionResult NhapChiSo(ThucDonRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "Home");
                }

                var existingUser = _context.NguoiDungs.Find(userId.Value);
                if (existingUser == null)
                {
                    Console.WriteLine("Không tìm thấy user trong DB.");
                }


                if (existingUser != null)
                {
                    // Cập nhật thông tin người dùng hiện tại
                    existingUser.GioiTinh = model.GioiTinh;
                    existingUser.Tuoi = model.Tuoi;
                    existingUser.ChieuCao = model.ChieuCao;
                    existingUser.CanNang = model.CanNang;
                    existingUser.MucDoHoatDong = model.MucDoHoatDong;
                    existingUser.MucTieu = model.MucTieu;
                    existingUser.SoBuaMotNgay = model.SoBuaMotNgay;
                    existingUser.NganSachToiDa = model.NganSachToiDa;
                    existingUser.CheDoAn = model.CheDoAn;
                    existingUser.DiUng = model.DiUng;
                    existingUser.KhongThich = model.KhongThich;

                    _context.NguoiDungs.Update(existingUser);
                    _context.SaveChanges();
                }

                return TaoThucDon(model);
            }

            return View(model);
        }




        private double TinhBMR(string gioiTinh, int tuoi, double canNang, double chieuCao)
        {
            if (gioiTinh.ToLower() == "nam")
            {
                return 66.47 + (13.75 * canNang) + (5.003 * chieuCao) - (6.755 * tuoi);
            }
            else
            {
                return 655.1 + (9.563 * canNang) + (1.850 * chieuCao) - (4.676 * tuoi);
            }
        }

        private double TinhTDEE(double bmr, string mucDoHoatDong)
        {
            switch (mucDoHoatDong.ToLower())
            {
                case "it":
                    return bmr * 1.2;
                case "vua":
                    return bmr * 1.55;
                case "nhieu":
                    return bmr * 1.9;
                default:
                    return bmr * 1.2;
            }
        }

        private double DieuChinhTheoMucTieu(double tdee, string mucTieu)
        {
            switch (mucTieu.ToLower())
            {
                case "giam can":
                    return tdee - 300;
                case "tang can":
                    return tdee + 300;
                default:
                    return tdee;
            }
        }

        public List<MonAn> TimMonAnPhuHop(
    double caloMoiBua,
    string cheDoAn,
    string loaiBua,
    List<string> diUng,
    List<string> khongThich,
    int soMon)
        {
            var query = _context.MonAns
                .Where(m =>
                    m.LuongCalo != null &&
                    m.LoaiBuaAn != null &&
                    m.TrangThai == true &&
                    m.LoaiBuaAn == loaiBua);

            // Lọc theo chế độ ăn
            if (!string.IsNullOrEmpty(cheDoAn))
            {
                switch (cheDoAn.ToLower())
                {
                    case "chay":
                        query = query.Where(m => m.Chay == true);
                        break;
                    case "keto":
                        query = query.Where(m => m.AnKeto == true);
                        break;
                    case "không gluten":
                        query = query.Where(m => m.AnKhongGluten == true);
                        break;
                }
            }

            // Dị ứng hoặc không thích
            if (diUng != null && diUng.Any())
            {
                foreach (var item in diUng)
                {
                    query = query.Where(m => m.NguyenLieuChinh != null && !m.NguyenLieuChinh.Contains(item));
                }
            }

            if (khongThich != null && khongThich.Any())
            {
                foreach (var item in khongThich)
                {
                    query = query.Where(m => m.TenMon != null && !m.TenMon.Contains(item));
                }
            }

            var danhSach = query.ToList();
            var random = new Random();

            // Thử ngẫu nhiên tối đa 1000 lần để tìm nhóm món có tổng calo phù hợp
            for (int i = 0; i < 1000; i++)
            {
                var monNgauNhien = danhSach.OrderBy(x => random.Next()).Take(soMon).ToList();
                var tongCalo = monNgauNhien.Sum(m => m.LuongCalo ?? 0);

                if (tongCalo >= caloMoiBua - 500 && tongCalo <= caloMoiBua + 500)
                {
                    return monNgauNhien;
                }
            }

            // Nếu không tìm được tổ hợp phù hợp sau nhiều lần thử, trả về tạm thời bất kỳ
            return danhSach.OrderBy(x => random.Next()).Take(soMon).ToList();
        }


        // Hàm tổ hợp
        private IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetCombinations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                            (t1, t2) => t1.Concat(new T[] { t2 }));
        }


        [HttpPost]
        public IActionResult TaoThucDon(ThucDonRequestViewModel model)
        {
            // Kiểm tra số bữa ăn
            if (model.SoBuaMotNgay <= 0 || model.SoBuaMotNgay > 3)
            {
                model.SoBuaMotNgay = 3;
            }

            // Tính BMR, TDEE và tổng calo cần thiết
            double bmr = TinhBMR(model.GioiTinh, model.Tuoi, model.CanNang, model.ChieuCao);
            double tdee = TinhTDEE(bmr, model.MucDoHoatDong);
            double tongCalo = DieuChinhTheoMucTieu(tdee, model.MucTieu);

            var thucDon7Ngay = new List<ThucDonTheoNgayViewModel>();

            for (int ngay = 1; ngay <= 7; ngay++)
            {
                var danhSachThucDon = new List<MonAn>();
                var chiTietList = new List<ThucDonChiTiet>();
                var buas = new List<(string TenBua, double TiLeCalo, int SoMon)>
            {
                ("Sáng", 0.2, 1),
                ("Trưa", 0.4, 2),
                ("Tối", 0.4, 2)
            };

                foreach (var (tenBua, tiLe, soMon) in buas.Take(model.SoBuaMotNgay))
                {
                    double caloBua = tongCalo * tiLe;

                    var monAnTheoBua = TimMonAnPhuHop(
                        caloBua,
                        model.CheDoAn,
                        tenBua,
                        model.DiUng?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() ?? new List<string>(),
                        model.KhongThich?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() ?? new List<string>(),
                        soMon
                    );

                    foreach (var monAn in monAnTheoBua)
                    {
                        danhSachThucDon.Add(monAn);
                        chiTietList.Add(new ThucDonChiTiet
                        {
                            MonAnId = monAn.Id,
                            LoaiBua = tenBua,
                            GhiChu = ""
                        });
                    }
                }

                var ngayThucDon = DateTime.Today.AddDays(ngay - 1).Date;


                thucDon7Ngay.Add(new ThucDonTheoNgayViewModel
                {
                    Ngay = ngayThucDon,
                    DanhSachMonAn = danhSachThucDon
                });

                // Lưu thực đơn ngày vào DB
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    var thucDonDb = new ThucDonNgay
                    {
                        NguoiDungId = userId.Value,
                        Ngay = ngayThucDon,
                        TongCalo = (int)danhSachThucDon.Sum(m => m.LuongCalo ?? 0),
                        TongCarbs = (float)danhSachThucDon.Sum(m => m.Carbs ?? 0),
                        TongProtein = (float)danhSachThucDon.Sum(m => m.Protein ?? 0),
                        TongFat = (float)danhSachThucDon.Sum(m => m.Fat ?? 0),
                        ThucDonChiTiets = chiTietList
                    };
                    _context.ThucDonNgays.Add(thucDonDb);
                }
            }

            _context.SaveChanges();

            var viewTongHop = new ThucDonViewTongHop
            {
                KetQua = new ThucDonKetQuaViewModel
                {
                    BMR = Math.Round(bmr, 2),
                    TDEE = Math.Round(tdee, 2),
                    TongCaloDieuChinh = Math.Round(tongCalo, 2),
                    CaloMoiBua = Math.Round(tongCalo / model.SoBuaMotNgay, 2),
                    SoBuaMotNgay = model.SoBuaMotNgay
                },

                // Tesst thuc don 7 ngay 
                ThucDon7Ngay = thucDon7Ngay
            };

            return View("TaoThucDon", viewTongHop);
        }


        public IActionResult XemThucDon()
        {

            return View("TaoThucDon");
        }
    }

}
