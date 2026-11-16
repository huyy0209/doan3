using System;

namespace DoAnWeb.Data.Entities;

public partial class NhatKy
{
    public int MaNK { get; set; }
    public DateTime ThoiGian { get; set; }
    public int? MaTK { get; set; }
    public string TenDangNhap { get; set; }
    public string HanhDong { get; set; }
    public string Bang { get; set; }
    public string KhoaChinh { get; set; }
    public string ChiTiet { get; set; }
    public string DiaChiIP { get; set; }
    public virtual TaiKhoan TaiKhoan { get; set; }
}
