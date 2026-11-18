using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services;


/// <summary>
/// Băm mật khẩu bằng PBKDF2 (HMAC-SHA256, salt ngẫu nhiên 16 byte, 100.000 vòng) – TK-01, TK-02.
/// Chuỗi lưu trong CSDL: "vòng.saltBase64.hashBase64". GIỮ ĐÚNG thuật toán của WinForms
/// (DoAnWD/BLL/Services/TaiKhoanService.cs:15-62): constructor Rfc2898DeriveBytes(string,…) của .NET 4.8 mã hóa
/// mật khẩu bằng UTF-8, nên Pbkdf2(UTF8 bytes) ở đây cho cùng kết quả → tài khoản seed (123456) đăng nhập được.
/// </summary>
public static class MatKhau
{
    public const int SoVong = 100000;
    private const int DoDaiSalt = 16;
    private const int DoDaiHash = 32;

    public static string Bam(string matKhau)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(DoDaiSalt);
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
            // So sánh không phụ thuộc vị trí khác nhau (tránh đoán qua thời gian)
            return mong.Length == thuc.Length && CryptographicOperations.FixedTimeEquals(mong, thuc);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] Pbkdf2(string matKhau, byte[] salt, int vong) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(matKhau), salt, vong, HashAlgorithmName.SHA256, DoDaiHash);
}

/// <summary>Một dòng danh sách tài khoản (frmTaiKhoan)</summary>
public class TaiKhoanRow
{
    public int MaTK { get; set; }
    public string TenDangNhap { get; set; }
    public string VaiTro { get; set; }
    public string GiaSu { get; set; }
    public int? MaGS { get; set; }
    public string TrangThai { get; set; }
    public bool HoatDong { get; set; }
    public bool DangTamKhoa { get; set; }
    public DateTime NgayTao { get; set; }
}

/// <summary>
/// Đăng nhập, đổi mật khẩu, quản lý tài khoản.
/// Nguồn: DoAnWD/BLL/Services/TaiKhoanService.cs:119-321 (luật TK-04..TK-19).
/// </summary>
public class TaiKhoanService
{
    public const string AdminMacDinh = "admin";
    public const string MatKhauMacDinh = "admin123";
    public const int DoDaiMatKhauToiThieu = 6;
    /// <summary>Nhập sai mật khẩu liên tiếp bấy nhiêu lần thì tạm khóa đăng nhập</summary>
    public const int SoLanSaiToiDa = 5;
    public const int SoPhutKhoa = 5;

    private readonly QuanLyGiaSuContext _db;
    private readonly INguoiDungHienTai _nguoiDung;

    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    /// <param name="cache">Cache trạng thái phiên đăng nhập (null khi test) – xóa khi khóa / đặt lại mật khẩu</param>
    public TaiKhoanService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung,
        Microsoft.Extensions.Caching.Memory.IMemoryCache cache = null)
    {
        _db = db;
        _nguoiDung = nguoiDung;
        _cache = cache;
    }

    /// <summary>Lần chạy đầu (chưa có tài khoản nào): tạo admin / admin123 (TK-10). Trả về true nếu vừa tạo.</summary>
    public bool DamBaoTaiKhoanMacDinh()
    {
        if (_db.TaiKhoans.Any()) return false;
        _db.TaiKhoans.Add(new TaiKhoan
        {
            TenDangNhap = AdminMacDinh,
            MatKhauHash = MatKhau.Bam(MatKhauMacDinh),
            VaiTro = Constants.VaiTro.QuanLy,
            HoatDong = true,
            NgayTao = DateTime.Now
        });
        _db.SaveChanges();
        return true;
    }

    /// <summary>Cho phép hiện gợi ý đăng nhập khi tài khoản admin duy nhất vẫn dùng mật khẩu mặc định.</summary>
    public bool CoTaiKhoanMacDinh()
    {
        var taiKhoans = _db.TaiKhoans.AsNoTracking()
            .Select(t => new { t.TenDangNhap, t.MatKhauHash })
            .ToList();
        return taiKhoans.Count == 1
            && string.Equals(taiKhoans[0].TenDangNhap, AdminMacDinh, StringComparison.OrdinalIgnoreCase)
            && MatKhau.KiemTra(MatKhauMacDinh, taiKhoans[0].MatKhauHash);
    }

    /// <summary>Đăng nhập; sai thì ném lỗi chung, không tiết lộ sai tên hay sai mật khẩu (TK-04..TK-09)</summary>
    public TaiKhoan DangNhap(string tenDangNhap, string matKhau)
    {
        const string loiChung = "Sai tên đăng nhập hoặc mật khẩu.";
        if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            throw new ArgumentException("Nhập tên đăng nhập và mật khẩu.");

        string ten = tenDangNhap.Trim().ToLowerInvariant();
        var tk = _db.TaiKhoans.Include(t => t.GiaSu).FirstOrDefault(t => t.TenDangNhap == ten);
        if (tk == null) throw new UnauthorizedAccessException(loiChung);

        // HIGH-03: đang bị tạm khóa cũng trả cùng một câu
        if (tk.KhoaDen.HasValue && tk.KhoaDen.Value > DateTime.Now)
            throw new UnauthorizedAccessException(loiChung);

        if (!MatKhau.KiemTra(matKhau, tk.MatKhauHash))
        {
            tk.SoLanSai++;
            if (tk.SoLanSai >= SoLanSaiToiDa)
            {
                tk.KhoaDen = DateTime.Now.AddMinutes(SoPhutKhoa);
                tk.SoLanSai = 0;
            }
            _db.SaveChanges();
            throw new UnauthorizedAccessException(loiChung);
        }
        if (!tk.HoatDong)
            throw new UnauthorizedAccessException("Tài khoản đã bị khóa. Liên hệ quản lý.");
        if (tk.SoLanSai != 0 || tk.KhoaDen.HasValue)
        {
            tk.SoLanSai = 0;
            tk.KhoaDen = null;
            _db.SaveChanges();
        }
        return tk;
    }

    /// <summary>TK-12: mật khẩu mới 6–100 ký tự. Trả về lỗi hoặc null.</summary>
    public static string KiemTraMatKhauMoi(string matKhau)
    {
        if (string.IsNullOrEmpty(matKhau) || matKhau.Length < DoDaiMatKhauToiThieu)
            return $"Mật khẩu tối thiểu {DoDaiMatKhauToiThieu} ký tự.";
        if (matKhau.Length > 100) return "Mật khẩu quá dài.";
        return null;
    }

    /// <summary>TK-13: tên đăng nhập 3–50 ký tự, chữ không dấu / số / . / _</summary>
    public static string KiemTraTenDangNhap(string ten)
    {
        if (string.IsNullOrWhiteSpace(ten)) return "Nhập tên đăng nhập.";
        ten = ten.Trim();
        if (ten.Length < 3 || ten.Length > 50) return "Tên đăng nhập 3-50 ký tự.";
        if (!Regex.IsMatch(ten, @"^[A-Za-z0-9_.]+$"))
            return "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới.";
        return null;
    }

    /// <summary>TK-14: đổi mật khẩu của chính mình</summary>
    public void DoiMatKhau(int maTK, string matKhauCu, string matKhauMoi)
    {
        string loi = KiemTraMatKhauMoi(matKhauMoi);
        if (loi != null) throw new ArgumentException(loi);
        if (matKhauMoi == matKhauCu) throw new ArgumentException("Mật khẩu mới phải khác mật khẩu hiện tại.");
        if (matKhauMoi == MatKhauMacDinh) throw new ArgumentException("Không dùng lại mật khẩu mặc định.");

        var tk = _db.TaiKhoans.Find(maTK);
        if (tk == null) throw new InvalidOperationException("Không tìm thấy tài khoản.");
        if (!MatKhau.KiemTra(matKhauCu, tk.MatKhauHash))
            throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng.");
        tk.MatKhauHash = MatKhau.Bam(matKhauMoi);
        _db.SaveChanges();
    }

    public List<TaiKhoanRow> GetAll()
    {
        var now = DateTime.Now;
        return _db.TaiKhoans.AsNoTracking().Include(t => t.GiaSu).OrderBy(t => t.TenDangNhap).ToList()
            .Select(t => new TaiKhoanRow
            {
                MaTK = t.MaTK,
                TenDangNhap = t.TenDangNhap,
                VaiTro = Constants.VaiTro.TenHienThi(t.VaiTro),
                GiaSu = t.GiaSu?.HoTen ?? "",
                MaGS = t.MaGS,
                TrangThai = t.HoatDong ? "Hoạt động" : "Đã khóa",
                HoatDong = t.HoatDong,
                DangTamKhoa = t.KhoaDen.HasValue && t.KhoaDen.Value > now,
                NgayTao = t.NgayTao
            }).ToList();
    }

    /// <summary>TK-15: tạo tài khoản (chỉ quản lý)</summary>
    public void TaoTaiKhoan(string tenDangNhap, string matKhau, string vaiTro, int? maGS)
    {
        _nguoiDung.YeuCauQuanLy();
        string loi = KiemTraTenDangNhap(tenDangNhap) ?? KiemTraMatKhauMoi(matKhau);
        if (loi != null) throw new ArgumentException(loi);
        if (vaiTro != Constants.VaiTro.QuanLy && vaiTro != Constants.VaiTro.GiaSu)
            throw new ArgumentException("Vai trò không hợp lệ.");
        if (vaiTro == Constants.VaiTro.GiaSu && !maGS.HasValue)
            throw new ArgumentException("Tài khoản gia sư phải gắn với một gia sư.");

        string ten = tenDangNhap.Trim().ToLowerInvariant();
        if (_db.TaiKhoans.Any(t => t.TenDangNhap == ten))
            throw new ArgumentException("Tên đăng nhập đã tồn tại.");
        if (maGS.HasValue && vaiTro == Constants.VaiTro.GiaSu && _db.TaiKhoans.Any(t => t.MaGS == maGS.Value))
            throw new ArgumentException("Gia sư này đã có tài khoản.");

        _db.TaiKhoans.Add(new TaiKhoan
        {
            TenDangNhap = ten,
            MatKhauHash = MatKhau.Bam(matKhau),
            VaiTro = vaiTro,
            MaGS = vaiTro == Constants.VaiTro.GiaSu ? maGS : null,
            HoatDong = true,
            NgayTao = DateTime.Now
        });
        _db.SaveChanges();
    }

    /// <summary>TK-16: quản lý đặt lại mật khẩu cho tài khoản khác (gỡ luôn tạm khóa)</summary>
    public void DatLaiMatKhau(int maTK, string matKhauMoi)
    {
        _nguoiDung.YeuCauQuanLy();
        string loi = KiemTraMatKhauMoi(matKhauMoi);
        if (loi != null) throw new ArgumentException(loi);
        var tk = _db.TaiKhoans.Find(maTK);
        if (tk == null) throw new InvalidOperationException("Không tìm thấy tài khoản.");
        tk.MatKhauHash = MatKhau.Bam(matKhauMoi);
        tk.SoLanSai = 0;
        tk.KhoaDen = null;
        _db.SaveChanges();
        DoAnWeb.Infrastructure.KiemTraPhienDangNhap.XoaCache(_cache, maTK);
    }

    /// <summary>TK-17: khóa / mở khóa. Không khóa chính mình hoặc quản lý hoạt động cuối cùng. Trả về trạng thái mới.</summary>
    public bool DoiTrangThai(int maTK)
    {
        _nguoiDung.YeuCauQuanLy();
        if (maTK == _nguoiDung.MaTK)
            throw new InvalidOperationException("Không thể khóa tài khoản đang đăng nhập.");
        var tk = _db.TaiKhoans.Find(maTK);
        if (tk == null) throw new InvalidOperationException("Không tìm thấy tài khoản.");
        if (tk.HoatDong && tk.VaiTro == Constants.VaiTro.QuanLy &&
            _db.TaiKhoans.Count(t => t.HoatDong && t.VaiTro == Constants.VaiTro.QuanLy) <= 1)
            throw new InvalidOperationException("Phải còn ít nhất một quản lý đang hoạt động.");
        tk.HoatDong = !tk.HoatDong;
        _db.SaveChanges();
        DoAnWeb.Infrastructure.KiemTraPhienDangNhap.XoaCache(_cache, maTK);
        return tk.HoatDong;
    }

    /// <summary>TK-18: gia sư chưa có tài khoản (để tạo mới)</summary>
    public List<GiaSu> GetGiaSuChuaCoTaiKhoan()
    {
        var daCo = _db.TaiKhoans.Where(t => t.MaGS != null).Select(t => t.MaGS.Value).ToList();
        return _db.GiaSus.AsNoTracking().Where(g => !daCo.Contains(g.MaGS)).OrderBy(g => g.HoTen).ToList();
    }
}