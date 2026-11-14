using System.ComponentModel.DataAnnotations;
using DoAnWeb.Common;
using DoAnWeb.Services;
using DoAnWeb.Data.Entities;
using GiaSuEntity = DoAnWeb.Data.Entities.GiaSu;

namespace DoAnWeb.ViewModels.QuanLyTaiKhoan;

public class QuanLyTaiKhoanIndexVM
{
    public List<TaiKhoanRow> TaiKhoans { get; set; } = new();
    public List<GiaSuEntity> GiaSuChuaCoTaiKhoan { get; set; } = new();
    public TaoTaiKhoanVM TaoMoi { get; set; } = new();
}

public class TaoTaiKhoanVM : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
    [RegularExpression("^(QuanLy|GiaSu)$", ErrorMessage = "Vai trò không hợp lệ.")]
    [Display(Name = "Vai trò")]
    public string VaiTro { get; set; } = Constants.VaiTro.GiaSu;

    [Display(Name = "Gia sư")]
    public int? MaGS { get; set; }

    [Required(ErrorMessage = "Nhập tên đăng nhập.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự.")]
    [RegularExpression(@"^[A-Za-z0-9_.]+$", ErrorMessage = "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới.")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; }

    [Required(ErrorMessage = "Nhập mật khẩu ban đầu.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 100 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu ban đầu")]
    public string MatKhau { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (VaiTro == Constants.VaiTro.GiaSu && !MaGS.HasValue)
            yield return new ValidationResult("Chọn gia sư cần cấp tài khoản.", new[] { nameof(MaGS) });
    }
}

public class DatLaiMatKhauVM
{
    public int MaTK { get; set; }
    public string TenDangNhap { get; set; }

    [Required(ErrorMessage = "Nhập mật khẩu mới.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 100 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; }

    [Required(ErrorMessage = "Nhập lại mật khẩu mới.")]
    [DataType(DataType.Password)]
    [Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string XacNhan { get; set; }
}
