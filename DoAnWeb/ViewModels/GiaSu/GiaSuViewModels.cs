using System.ComponentModel.DataAnnotations;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services.Validators;
using GiaSuEntity = DoAnWeb.Data.Entities.GiaSu;

namespace DoAnWeb.ViewModels.GiaSu;

public class GiaSuIndexVM
{
    public string TuKhoa { get; set; }
    public string Mon { get; set; }
    public string Ca { get; set; }
    public string TrangThai { get; set; }
    public int TongSo { get; set; }
    public List<string> DanhSachMon { get; set; } = new();
    public List<GiaSuListItemVM> GiaSus { get; set; } = new();
}

public class GiaSuListItemVM
{
    public GiaSuEntity GiaSu { get; set; }
    public int SoLop { get; set; }
    public string CaNhanDay =>
        string.IsNullOrWhiteSpace(GiaSu.CaNhanDay) ? "Mọi ca" : GiaSu.CaNhanDay.Replace(",", ", ");
    public string TrangThai => GiaSu.TrangThai ?? Constants.GiaSuStatus.Ranh;
    public string TagClass => TrangThai == Constants.GiaSuStatus.TamNghi ? "warning"
        : TrangThai == Constants.GiaSuStatus.DangDay ? "success"
        : TrangThai == Constants.GiaSuStatus.Ban ? "info" : "neutral";
}

public class HoSoGiaSuVM
{
    public int? MaGS { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự.")]
    [RegularExpression(@"^[^\d]+$", ErrorMessage = "Họ tên không được chứa chữ số.")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải có 10-11 số và bắt đầu bằng 0.")]
    [Display(Name = "Số điện thoại")]
    public string SDT { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn chuyên môn.")]
    [StringLength(100, ErrorMessage = "Chuyên môn tối đa 100 ký tự.")]
    [Display(Name = "Chuyên môn")]
    public string ChuyenMon { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
    public string Email { get; set; }

    [StringLength(150, ErrorMessage = "Học vấn / bằng cấp tối đa 150 ký tự.")]
    [Display(Name = "Học vấn / bằng cấp")]
    public string HocVan { get; set; }

    [Range(0, 60, ErrorMessage = "Số năm kinh nghiệm từ 0 đến 60.")]
    [Display(Name = "Kinh nghiệm (năm)")]
    public int? KinhNghiem { get; set; }

    public List<string> CaNhanDay { get; set; } = new();

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
    public string GhiChu { get; set; }

    [Display(Name = "Tạm nghỉ (không nhận lớp mới)")]
    public bool TamNghi { get; set; }

    public List<string> DanhSachMon { get; set; } = new();
    public static string[] CacCa => Constants.CaHoc.TatCaCaHoc;

    public static HoSoGiaSuVM TuGiaSu(GiaSuEntity gs) => new()
    {
        MaGS = gs.MaGS,
        HoTen = gs.HoTen,
        SDT = gs.SDT,
        ChuyenMon = gs.ChuyenMon,
        Email = gs.Email,
        HocVan = gs.HocVan,
        KinhNghiem = gs.KinhNghiem,
        CaNhanDay = string.IsNullOrWhiteSpace(gs.CaNhanDay)
            ? new List<string>()
            : gs.CaNhanDay.Split(',').Select(c => c.Trim()).ToList(),
        GhiChu = gs.GhiChu,
        TamNghi = gs.TrangThai == Constants.GiaSuStatus.TamNghi
    };

    public HoSoGiaSu SangHoSo() => new()
    {
        HoTen = HoTen,
        SDT = SDT,
        ChuyenMon = ChuyenMon,
        Email = Email,
        HocVan = HocVan,
        KinhNghiem = KinhNghiem,
        CaNhanDay = CaNhanDay ?? new List<string>(),
        GhiChu = GhiChu,
        TamNghi = TamNghi
    };
}
