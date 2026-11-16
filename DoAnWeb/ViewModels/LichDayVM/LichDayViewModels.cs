using DoAnWeb.Data.Entities;

namespace DoAnWeb.ViewModels.LichDayVM;

public class LichDayIndexViewModel
{
    public DateTime BatDauTuan { get; set; }
    public List<LichDayNgayViewModel> CacNgay { get; set; } = new();
    public bool LaQuanLy { get; set; }
    public int? MaGS { get; set; }
    public List<GiaSu> GiaSus { get; set; } = new();
    public string MonHoc { get; set; }
    public List<string> CacMon { get; set; } = new();
    public int SoLop { get; set; }
    public DateTime KetThucTuan => BatDauTuan.AddDays(6);
    public DateTime TuanTruoc => BatDauTuan.AddDays(-7);
    public DateTime TuanSau => BatDauTuan.AddDays(7);
}

public class LichDayNgayViewModel
{
    public DateTime Ngay { get; set; }
    public bool HomNay { get; set; }
    public List<LichDayCaViewModel> CacCa { get; set; } = new();
}

public class LichDayCaViewModel
{
    public string Ten { get; set; }
    public List<LopHoc> LopHoc { get; set; } = new();
}

public class LichDayChiTietViewModel
{
    public LopHoc LopHoc { get; set; }
}
