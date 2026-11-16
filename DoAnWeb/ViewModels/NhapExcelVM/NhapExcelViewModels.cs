namespace DoAnWeb.ViewModels.NhapExcelVM;

public sealed class NhapExcelVM
{
    public string LoaiHoSo { get; set; } = "HocVien";
    public string MaPhien { get; set; }
    public int TongSo => Dong?.Count ?? 0;
    public int SoHopLe => Dong?.Count(x => x.HopLe) ?? 0;
    public List<DongNhapExcelVM> Dong { get; set; } = new();
}

public sealed class DongNhapExcelVM
{
    public int SoDong { get; set; }
    public string TomTat { get; set; }
    public bool HopLe => Loi.Count == 0;
    public List<string> Loi { get; set; } = new();
}
