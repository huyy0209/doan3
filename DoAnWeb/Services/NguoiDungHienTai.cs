using System;
using System.Security.Claims;
using DoAnWeb.Common;
using Microsoft.AspNetCore.Http;

namespace DoAnWeb.Services
{
    /// <summary>
    /// Người đang đăng nhập – thay cho lớp tĩnh PhienDangNhap của WinForms (DoAnWD/BLL/Services/TaiKhoanService.cs:65-105).
    /// Service tự kiểm tra quyền qua đây, không tin tham số từ tầng giao diện (HIGH-02).
    /// </summary>
    public interface INguoiDungHienTai
    {
        bool DaDangNhap { get; }
        int MaTK { get; }
        string TenDangNhap { get; }
        string VaiTro { get; }
        int? MaGS { get; }
        string TenGiaSu { get; }
        bool LaQuanLy { get; }
        /// <summary>Gia sư chỉ xem dữ liệu lớp của mình; quản lý = null (xem tất cả)</summary>
        int? GioiHanGiaSu { get; }
        string TenHienThi { get; }
        /// <summary>Chỉ quản lý đã đăng nhập mới được làm (TK-19)</summary>
        void YeuCauQuanLy();
    }

    /// <summary>Tên các claim lưu trong cookie đăng nhập</summary>
    public static class LoaiClaim
    {
        public const string MaTK = "MaTK";
        public const string MaGS = "MaGS";
        public const string TenGiaSu = "TenGiaSu";
        /// <summary>"1" khi đăng nhập bằng mật khẩu mặc định: bắt đổi mật khẩu trước khi dùng (TK-11)</summary>
        public const string PhaiDoiMatKhau = "PhaiDoiMatKhau";
    }

    /// <summary>Phần dùng chung cho mọi cách cài đặt</summary>
    public abstract class NguoiDungCoSo : INguoiDungHienTai
    {
        public abstract bool DaDangNhap { get; }
        public abstract int MaTK { get; }
        public abstract string TenDangNhap { get; }
        public abstract string VaiTro { get; }
        public abstract int? MaGS { get; }
        public abstract string TenGiaSu { get; }

        public bool LaQuanLy => VaiTro == Constants.VaiTro.QuanLy;
        public int? GioiHanGiaSu => LaQuanLy ? (int?)null : MaGS;
        public string TenHienThi => LaQuanLy ? $"{TenDangNhap} (Quản lý)" : $"{TenGiaSu ?? TenDangNhap} (Gia sư)";

        public void YeuCauQuanLy()
        {
            if (!DaDangNhap || !LaQuanLy)
                throw new UnauthorizedAccessException("Chỉ quản lý mới được thực hiện thao tác này.");
        }
    }

    /// <summary>Đọc người dùng từ cookie (claims) của request hiện tại</summary>
    public class NguoiDungHttp : NguoiDungCoSo
    {
        private readonly IHttpContextAccessor _http;
        public NguoiDungHttp(IHttpContextAccessor http) { _http = http; }

        private ClaimsPrincipal User => _http.HttpContext?.User;
        private string Lay(string loai) => User?.FindFirst(loai)?.Value;

        public override bool DaDangNhap => User?.Identity?.IsAuthenticated == true && MaTK > 0;
        public override int MaTK => int.TryParse(Lay(LoaiClaim.MaTK), out var ma) ? ma : 0;
        public override string TenDangNhap => Lay(ClaimTypes.Name);
        public override string VaiTro => Lay(ClaimTypes.Role);
        public override int? MaGS => int.TryParse(Lay(LoaiClaim.MaGS), out var ma) ? ma : (int?)null;
        public override string TenGiaSu => Lay(LoaiClaim.TenGiaSu);
    }

    /// <summary>Người dùng gán sẵn – dùng cho test và tác vụ nền (đặt VaiTro = QuanLy để chạy như quản lý)</summary>
    public class NguoiDungCoDinh : NguoiDungCoSo
    {
        public int Ma { get; set; }
        public string Ten { get; set; }
        public string Vai { get; set; }
        public int? GiaSu { get; set; }
        public string HoTenGiaSu { get; set; }

        public override bool DaDangNhap => Ma > 0;
        public override int MaTK => Ma;
        public override string TenDangNhap => Ten;
        public override string VaiTro => Vai;
        public override int? MaGS => GiaSu;
        public override string TenGiaSu => HoTenGiaSu;

        public static NguoiDungCoDinh QuanLy(int maTK = 1, string ten = "admin") =>
            new NguoiDungCoDinh { Ma = maTK, Ten = ten, Vai = Constants.VaiTro.QuanLy };

        public static NguoiDungCoDinh GiaSuDay(int maTK, int maGS, string ten = null) =>
            new NguoiDungCoDinh { Ma = maTK, Ten = ten ?? "gs" + maGS, Vai = Constants.VaiTro.GiaSu, GiaSu = maGS };
    }
}
