using System.ComponentModel.DataAnnotations;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services.Validators;
using HocVienEntity = DoAnWeb.Data.Entities.HocVien;

namespace DoAnWeb.ViewModels.HocVien;

public class HocVienIndexVM
{
    public string TuKhoa { get; set; }
    public string TrinhDo { get; set; }
    public string TinhTrang { get; set; }
    public List<string> DanhSachTrinhDo { get; set; } = new();
    public List<HocVienListItemVM> HocViens { get; set; } = new();
    public int TongSo { get; set; }
    public bool LaQuanLy { get; set; }
}

public class HocVienListItemVM
{
    public HocVienEntity HocVien { get; set; }
    public string DangHocMon { get; set; }
    public string PhuHuynh =>
        string.Join(" · ", new[] { HocVien.TenPhuHuynh, HocVien.SDTPhuHuynh }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
}

public class HocVienDetailsVM
{
    public HocVienEntity HocVien { get; set; }
    public List<DoAnWeb.Data.Entities.LopHoc> LopHocs { get; set; } = new();
    public bool LaQuanLy { get; set; }
}

public class HoSoHocVienVM : IValidatableObject
{
    public int? MaHV { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự.")]
    [RegularExpression(@"^[^\d]+$", ErrorMessage = "Họ tên không được chứa chữ số.")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; }

    [RegularExpression(@"^\s*$|^0\d{9,10}$", ErrorMessage = "Số điện thoại phải có 10-11 số và bắt đầu bằng 0.")]
    [Display(Name = "SĐT học viên")]
    public string SDT { get; set; }

    [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự.")]
    public string DiaChi { get; set; }

    [StringLength(100, ErrorMessage = "Tên phụ huynh tối đa 100 ký tự.")]
    [Display(Name = "Phụ huynh")]
    public string TenPhuHuynh { get; set; }

    [RegularExpression(@"^\s*$|^0\d{9,10}$", ErrorMessage = "SĐT phụ huynh phải có 10-11 số và bắt đầu bằng 0.")]
    [Display(Name = "SĐT phụ huynh")]
    public string SDTPhuHuynh { get; set; }

    [StringLength(150, ErrorMessage = "Trường / lớp tối đa 150 ký tự.")]
    [Display(Name = "Trường / lớp")]
    public string TruongLop { get; set; }

    [StringLength(50, ErrorMessage = "Trình độ tối đa 50 ký tự.")]
    [Display(Name = "Trình độ")]
    public string TrinhDo { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
    public string GhiChu { get; set; }

    public static string[] GoiYTrinhDo => Constants.GoiY.TrinhDo;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(SDT) && string.IsNullOrWhiteSpace(SDTPhuHuynh))
            yield return new ValidationResult(
                "Nhập ít nhất một số điện thoại: của học viên hoặc của phụ huynh.",
                new[] { nameof(SDT), nameof(SDTPhuHuynh) });
    }

    public static HoSoHocVienVM TuHocVien(HocVienEntity hv) => new()
    {
        MaHV = hv.MaHV,
        HoTen = hv.HoTen,
        SDT = hv.SDT,
        DiaChi = hv.DiaChi,
        TenPhuHuynh = hv.TenPhuHuynh,
        SDTPhuHuynh = hv.SDTPhuHuynh,
        TruongLop = hv.TruongLop,
        TrinhDo = hv.TrinhDo,
        GhiChu = hv.GhiChu
    };

    public HoSoHocVien SangHoSo() => new()
    {
        HoTen = HoTen,
        SDT = SDT,
        DiaChi = DiaChi,
        TenPhuHuynh = TenPhuHuynh,
        SDTPhuHuynh = SDTPhuHuynh,
        TruongLop = TruongLop,
        TrinhDo = TrinhDo,
        GhiChu = GhiChu
    };
}
