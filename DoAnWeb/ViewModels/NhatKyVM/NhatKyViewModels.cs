using System;
using System.Collections.Generic;
using DoAnWeb.Data.Entities;
using DoAnWeb.Infrastructure;

namespace DoAnWeb.ViewModels.NhatKyVM;

public sealed class NhatKyIndexVM
{
    public KetQuaTrang<NhatKy> KetQua { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public string Nguoi { get; set; }
    public string HanhDong { get; set; }
    public string Bang { get; set; }
    public List<string> DanhSachNguoi { get; set; } = new();
    public List<string> DanhSachBang { get; set; } = new();
}

public sealed class NhatKyChiTietTrangVM
{
    public NhatKy NhatKy { get; set; }
    public List<NhatKyTruongVM> CacTruong { get; } = new();
    public string JsonKhongHopLe { get; set; }
}

public sealed class NhatKyTruongVM
{
    public string Ten { get; set; }
    public string Cu { get; set; }
    public string Moi { get; set; }
}
