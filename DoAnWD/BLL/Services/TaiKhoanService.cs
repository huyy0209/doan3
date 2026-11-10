using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    /// <summary>
    /// Băm mật khẩu bằng PBKDF2 (HMAC-SHA256, salt ngẫu nhiên 16 byte, 100.000 vòng).
    /// Chuỗi lưu trong CSDL: "vòng.saltBase64.hashBase64".
    /// </summary>
    public static class MatKhau
    {
        private const int SoVong = 100000;
        private const int DoDaiSalt = 16;
        private const int DoDaiHash = 32;

        public static string Bam(string matKhau)
        {
            var salt = new byte[DoDaiSalt];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            byte[] hash = Pbkdf2(matKhau, salt, SoVong);
            return $"{SoVong}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool KiemTra(string matKhau, string chuoiLuu)
        {
            if (string.IsNullOrEmpty(matKhau) || string.IsNullOrEmpty(chuoiLuu)) return false;
            var phan = chuoiLuu.Split('.');
            if (phan.Length != 3) return false;
            try
            {
                int vong = int.Parse(phan[0]);
                byte[] salt = Convert.FromBase64String(phan[1]);
                byte[] mong = Convert.FromBase64String(phan[2]);
                byte[] thuc = Pbkdf2(matKhau, salt, vong);
                return SoSanhHangThoiGian(mong, thuc);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static byte[] Pbkdf2(string matKhau, byte[] salt, int vong)
        {
            using (var kdf = new Rfc2898DeriveBytes(matKhau, salt, vong, HashAlgorithmName.SHA256))
                return kdf.GetBytes(DoDaiHash);
        }

        /// <summary>So sánh không phụ thuộc vị trí khác nhau (tránh đoán qua thời gian)</summary>
        private static bool SoSanhHangThoiGian(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }

    /// <summary>Thông tin người đang đăng nhập (dùng chung toàn ứng dụng)</summary>
    public static class PhienDangNhap
    {
        public static int MaTK { get; private set; }
        public static string TenDangNhap { get; private set; }
        public static string VaiTro { get; private set; }
        public static int? MaGS { get; private set; }
        public static string TenGiaSu { get; private set; }

        public static bool DaDangNhap => MaTK > 0;
        public static bool LaQuanLy => VaiTro == Constants.VaiTro.QuanLy;

        /// <summary>Gia sư chỉ xem dữ liệu lớp của mình; quản lý = null (xem tất cả)</summary>
        public static int? GioiHanGiaSu => LaQuanLy ? (int?)null : MaGS;

        public static string TenHienThi =>
            LaQuanLy ? $"{TenDangNhap} (Quản lý)" : $"{TenGiaSu ?? TenDangNhap} (Gia sư)";

        public static void BatDau(TaiKhoan tk)
        {
            MaTK = tk.MaTK;
            TenDangNhap = tk.TenDangNhap;
            VaiTro = tk.VaiTro;
            MaGS = tk.MaGS;
            TenGiaSu = tk.GiaSu?.HoTen;
        }

        /// <summary>
        /// Chặn ở tầng nghiệp vụ: đã đăng nhập mà không phải quản lý thì không được làm.
        /// (Chưa đăng nhập = chạy kiểm thử / công cụ, cho qua.)
        /// </summary>
        public static void YeuCauQuanLy()
        {
            if (DaDangNhap && !LaQuanLy)
                throw new UnauthorizedAccessException("Chỉ quản lý mới được thực hiện thao tác này.");
        }

        public static void KetThuc()
        {
            MaTK = 0; TenDangNhap = null; VaiTro = null; MaGS = null; TenGiaSu = null;
        }
    }

    public class TaiKhoanRow
    {
        public int MaTK { get; set; }
        public string TenDangNhap { get; set; }
        public string VaiTro { get; set; }
        public string GiaSu { get; set; }
        public int? MaGS { get; set; }
        public string TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
    }

    /// <summary>Đăng nhập, đổi mật khẩu, quản lý tài khoản</summary>
    public class TaiKhoanService
    {
        public const string AdminMacDinh = "admin";
        public const string MatKhauMacDinh = "admin123";
        public const int DoDaiMatKhauToiThieu = 6;
        /// <summary>Nhập sai mật khẩu liên tiếp bấy nhiêu lần thì tạm khóa đăng nhập</summary>
        public const int SoLanSaiToiDa = 5;
        public const int SoPhutKhoa = 5;

        /// <summary>
        /// Lần chạy đầu (chưa có tài khoản nào): tạo admin / admin123.
        /// Trả về true nếu vừa tạo, để giao diện nhắc đổi mật khẩu.
        /// </summary>
        public bool DamBaoTaiKhoanMacDinh()
        {
            using (var db = new GiaSuContextDB())
            {
                if (db.TaiKhoans.Any()) return false;
                db.TaiKhoans.Add(new TaiKhoan
                {
                    TenDangNhap = AdminMacDinh,
                    MatKhauHash = MatKhau.Bam(MatKhauMacDinh),
                    VaiTro = Constants.VaiTro.QuanLy,
                    HoatDong = true,
                    NgayTao = DateTime.Now
                });
                db.SaveChanges();
                return true;
            }
        }

        /// <summary>Đăng nhập; sai thì ném lỗi chung (không tiết lộ sai tên hay sai mật khẩu)</summary>
        public TaiKhoan DangNhap(string tenDangNhap, string matKhau)
        {
            const string loiChung = "Sai tên đăng nhập hoặc mật khẩu.";
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrEmpty(matKhau))
                throw new ArgumentException("Nhập tên đăng nhập và mật khẩu.");

            string ten = tenDangNhap.Trim().ToLowerInvariant();
            using (var db = new GiaSuContextDB())
            {
                var tk = db.TaiKhoans.Include(t => t.GiaSu).FirstOrDefault(t => t.TenDangNhap == ten);
                if (tk == null) throw new UnauthorizedAccessException(loiChung);

                // Đang bị tạm khóa do nhập sai nhiều lần
                if (tk.KhoaDen.HasValue && tk.KhoaDen.Value > DateTime.Now)
                    throw new UnauthorizedAccessException(
                        $"Nhập sai quá {SoLanSaiToiDa} lần. Thử lại sau {tk.KhoaDen:HH:mm}.");

                if (!MatKhau.KiemTra(matKhau, tk.MatKhauHash))
                {
                    tk.SoLanSai++;
                    string loi = loiChung;
                    if (tk.SoLanSai >= SoLanSaiToiDa)
                    {
                        tk.KhoaDen = DateTime.Now.AddMinutes(SoPhutKhoa);
                        tk.SoLanSai = 0;
                        loi = $"Nhập sai {SoLanSaiToiDa} lần liên tiếp. Tài khoản tạm khóa {SoPhutKhoa} phút.";
                    }
                    else if (tk.SoLanSai >= SoLanSaiToiDa - 2)
                        loi += $" Còn {SoLanSaiToiDa - tk.SoLanSai} lần thử.";
                    db.SaveChanges();
                    throw new UnauthorizedAccessException(loi);
                }
                if (!tk.HoatDong)
                    throw new UnauthorizedAccessException("Tài khoản đã bị khóa. Liên hệ quản lý.");
                if (tk.SoLanSai != 0 || tk.KhoaDen.HasValue)
                {
                    tk.SoLanSai = 0;
                    tk.KhoaDen = null;
                    db.SaveChanges();
                }
                return tk;
            }
        }

        public static string KiemTraMatKhauMoi(string matKhau)
        {
            if (string.IsNullOrEmpty(matKhau) || matKhau.Length < DoDaiMatKhauToiThieu)
                return $"Mật khẩu tối thiểu {DoDaiMatKhauToiThieu} ký tự.";
            if (matKhau.Length > 100) return "Mật khẩu quá dài.";
            return null;
        }

        public static string KiemTraTenDangNhap(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return "Nhập tên đăng nhập.";
            ten = ten.Trim();
            if (ten.Length < 3 || ten.Length > 50) return "Tên đăng nhập 3-50 ký tự.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(ten, @"^[A-Za-z0-9_.]+$"))
                return "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới.";
            return null;
        }

        public void DoiMatKhau(int maTK, string matKhauCu, string matKhauMoi)
        {
            string loi = KiemTraMatKhauMoi(matKhauMoi);
            if (loi != null) throw new ArgumentException(loi);
            if (matKhauMoi == matKhauCu) throw new ArgumentException("Mật khẩu mới phải khác mật khẩu hiện tại.");
            if (matKhauMoi == MatKhauMacDinh) throw new ArgumentException("Không dùng lại mật khẩu mặc định.");

            using (var db = new GiaSuContextDB())
            {
                var tk = db.TaiKhoans.Find(maTK);
                if (tk == null) throw new InvalidOperationException("Không tìm thấy tài khoản.");
                if (!MatKhau.KiemTra(matKhauCu, tk.MatKhauHash))
                    throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng.");
                tk.MatKhauHash = MatKhau.Bam(matKhauMoi);
                db.SaveChanges();
            }
        }

        public List<TaiKhoanRow> GetAll()
        {
            using (var db = new GiaSuContextDB())
            {
                return db.TaiKhoans.Include(t => t.GiaSu).OrderBy(t => t.TenDangNhap).ToList()
                    .Select(t => new TaiKhoanRow
                    {
                        MaTK = t.MaTK,
                        TenDangNhap = t.TenDangNhap,
                        VaiTro = Constants.VaiTro.TenHienThi(t.VaiTro),
                        GiaSu = t.GiaSu?.HoTen ?? "",
                        MaGS = t.MaGS,
                        TrangThai = t.HoatDong ? "Hoạt động" : "Đã khóa",
                        NgayTao = t.NgayTao
                    }).ToList();
            }
        }

        public void TaoTaiKhoan(string tenDangNhap, string matKhau, string vaiTro, int? maGS)
        {
            PhienDangNhap.YeuCauQuanLy();
            string loi = KiemTraTenDangNhap(tenDangNhap) ?? KiemTraMatKhauMoi(matKhau);
            if (loi != null) throw new ArgumentException(loi);
            if (vaiTro != Constants.VaiTro.QuanLy && vaiTro != Constants.VaiTro.GiaSu)
                throw new ArgumentException("Vai trò không hợp lệ.");
            if (vaiTro == Constants.VaiTro.GiaSu && !maGS.HasValue)
                throw new ArgumentException("Tài khoản gia sư phải gắn với một gia sư.");

            string ten = tenDangNhap.Trim().ToLowerInvariant();
            using (var db = new GiaSuContextDB())
            {
                if (db.TaiKhoans.Any(t => t.TenDangNhap == ten))
                    throw new ArgumentException("Tên đăng nhập đã tồn tại.");
                if (maGS.HasValue && db.TaiKhoans.Any(t => t.MaGS == maGS.Value))
                    throw new ArgumentException("Gia sư này đã có tài khoản.");

                db.TaiKhoans.Add(new TaiKhoan
                {
                    TenDangNhap = ten,
                    MatKhauHash = MatKhau.Bam(matKhau),
                    VaiTro = vaiTro,
                    MaGS = vaiTro == Constants.VaiTro.GiaSu ? maGS : null,
                    HoatDong = true,
                    NgayTao = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>Quản lý đặt lại mật khẩu cho tài khoản khác</summary>
        public void DatLaiMatKhau(int maTK, string matKhauMoi)
        {
            PhienDangNhap.YeuCauQuanLy();
            string loi = KiemTraMatKhauMoi(matKhauMoi);
            if (loi != null) throw new ArgumentException(loi);
            using (var db = new GiaSuContextDB())
            {
                var tk = db.TaiKhoans.Find(maTK);
                if (tk == null) throw new InvalidOperationException("Không tìm thấy tài khoản.");
                tk.MatKhauHash = MatKhau.Bam(matKhauMoi);
                tk.SoLanSai = 0;
                tk.KhoaDen = null;   // đặt lại mật khẩu thì gỡ luôn tạm khóa
                db.SaveChanges();
            }
        }

        /// <summary>Khóa / mở khóa. Không cho khóa chính mình hoặc quản lý hoạt động cuối cùng.</summary>
        public bool DoiTrangThai(int maTK, int maTKDangDung)
        {
            PhienDangNhap.YeuCauQuanLy();
            if (maTK == maTKDangDung)
                throw new InvalidOperationException("Không thể khóa tài khoản đang đăng nhập.");
            using (var db = new GiaSuContextDB())
            {
                var tk = db.TaiKhoans.Find(maTK);
                if (tk == null) throw new InvalidOperationException("Không tìm thấy tài khoản.");
                if (tk.HoatDong && tk.VaiTro == Constants.VaiTro.QuanLy &&
                    db.TaiKhoans.Count(t => t.HoatDong && t.VaiTro == Constants.VaiTro.QuanLy) <= 1)
                    throw new InvalidOperationException("Phải còn ít nhất một quản lý đang hoạt động.");
                tk.HoatDong = !tk.HoatDong;
                db.SaveChanges();
                return tk.HoatDong;
            }
        }

        /// <summary>Gia sư chưa có tài khoản (để tạo mới)</summary>
        public List<GiaSu> GetGiaSuChuaCoTaiKhoan()
        {
            using (var db = new GiaSuContextDB())
            {
                var daCo = db.TaiKhoans.Where(t => t.MaGS != null).Select(t => t.MaGS.Value).ToList();
                return db.GiaSus.Where(g => !daCo.Contains(g.MaGS)).OrderBy(g => g.HoTen).ToList();
            }
        }
    }
}
