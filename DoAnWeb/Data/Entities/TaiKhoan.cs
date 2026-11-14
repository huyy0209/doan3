using System;
using System.Collections.Generic;

namespace DoAnWeb.Data.Entities;

public partial class TaiKhoan
{
    public int MaTK { get; set; }

    public string TenDangNhap { get; set; }

    public string MatKhauHash { get; set; }

    public string VaiTro { get; set; }

    public int? MaGS { get; set; }

    public bool HoatDong { get; set; }

    public DateTime NgayTao { get; set; }

    public int SoLanSai { get; set; }

    public DateTime? KhoaDen { get; set; }

    public virtual GiaSu GiaSu { get; set; }
}
