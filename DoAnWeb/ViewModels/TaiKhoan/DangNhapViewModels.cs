using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.ViewModels.TaiKhoan;


/// <summary>Form đăng nhập (frmDangNhap)</summary>
public class DangNhapVM
{
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; }

    [Display(Name = "Mật khẩu")]
    [DataType(DataType.Password)]
    public string MatKhau { get; set; }

    /// <summary>Trang cần quay lại sau khi đăng nhập</summary>
    public string ReturnUrl { get; set; }

    /// <summary>Bảng tài khoản vừa rỗng và hệ thống vừa tạo admin/admin123 (TK-10): hiện gợi ý đăng nhập lần đầu</summary>
    public bool LanDau { get; set; }
}

/// <summary>Form đổi mật khẩu (frmDoiMatKhau)</summary>
public class DoiMatKhauVM
{
    [Display(Name = "Mật khẩu hiện tại")]
    [DataType(DataType.Password)]
    public string MatKhauCu { get; set; }

    [Display(Name = "Mật khẩu mới")]
    [DataType(DataType.Password)]
    public string MatKhauMoi { get; set; }

    [Display(Name = "Nhập lại mật khẩu mới")]
    [DataType(DataType.Password)]
    [Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
    public string XacNhan { get; set; }

    /// <summary>true = đang dùng mật khẩu mặc định, bắt buộc đổi mới được vào (TK-11)</summary>
    public bool BatBuoc { get; set; }
}