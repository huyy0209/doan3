using System;
using System.Linq;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    /// <summary>Thông tin trung tâm, hiện ở màn hình đăng nhập, khung chính và phiếu báo</summary>
    public class ThongTinTrungTam
    {
        public string Ten { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }

        /// <summary>Tên để hiển thị (có tên mặc định khi chưa cài đặt)</summary>
        public string TenHienThi => string.IsNullOrWhiteSpace(Ten) ? CaiDatService.TenMacDinh : Ten;
    }

    /// <summary>Đọc / ghi bảng CaiDat. Thông tin trung tâm được nhớ trong bộ nhớ, chỉ đọc CSDL một lần.</summary>
    public static class CaiDatService
    {
        public const string TenMacDinh = "Trung tâm gia sư";
        private const string KTen = "TenTrungTam", KDiaChi = "DiaChi", KSDT = "SDT";
        private static ThongTinTrungTam _nho;

        public static ThongTinTrungTam TrungTam
        {
            get
            {
                if (_nho != null) return _nho;
                try
                {
                    using (var db = new GiaSuContextDB())
                    {
                        var ds = db.CaiDats.ToList().ToDictionary(c => c.Khoa, c => c.GiaTri, StringComparer.OrdinalIgnoreCase);
                        Func<string, string> lay = k => { string v; return ds.TryGetValue(k, out v) ? v : null; };
                        _nho = new ThongTinTrungTam { Ten = lay(KTen), DiaChi = lay(KDiaChi), SDT = lay(KSDT) };
                    }
                }
                catch
                {
                    return new ThongTinTrungTam();   // CSDL chưa nâng cấp: dùng tên mặc định, lần sau đọc lại
                }
                return _nho;
            }
        }

        public static void LuuTrungTam(string ten, string diaChi, string sdt)
        {
            PhienDangNhap.YeuCauQuanLy();
            ten = (ten ?? "").Trim();
            if (ten.Length == 0) throw new ArgumentException("Nhập tên trung tâm.");
            if (ten.Length > 100) throw new ArgumentException("Tên trung tâm tối đa 100 ký tự.");
            if ((diaChi ?? "").Length > 300) throw new ArgumentException("Địa chỉ tối đa 300 ký tự.");
            if ((sdt ?? "").Length > 50) throw new ArgumentException("Số điện thoại tối đa 50 ký tự.");

            using (var db = new GiaSuContextDB())
            {
                Ghi(db, KTen, ten);
                Ghi(db, KDiaChi, diaChi);
                Ghi(db, KSDT, sdt);
                db.SaveChanges();
            }
            _nho = null;
        }

        private static void Ghi(GiaSuContextDB db, string khoa, string giaTri)
        {
            giaTri = string.IsNullOrWhiteSpace(giaTri) ? null : giaTri.Trim();
            var c = db.CaiDats.FirstOrDefault(x => x.Khoa == khoa);
            if (c == null) db.CaiDats.Add(new CaiDat { Khoa = khoa, GiaTri = giaTri });
            else c.GiaTri = giaTri;
        }
    }
}
