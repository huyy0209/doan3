using System;
using System.Collections.Generic;

namespace DoAnWeb.Data.Entities;

public partial class LopHoc
{
    public int MaLop { get; set; }

    public int? MaGS { get; set; }

    public int? MaHV { get; set; }

    public string MonHoc { get; set; }

    public decimal? HocPhi { get; set; }

    public string TrangThai { get; set; }

    public DateTime? ThoiGianBatDau { get; set; }

    public DateTime? ThoiGianKetThuc { get; set; }

    public string CaHoc { get; set; }

    public string NgayHocTrongTuan { get; set; }

    public string TenLop { get; set; }

    public string CapDoDauVao { get; set; }

    public string ThuHoc { get; set; }

    public int? MaMon { get; set; }

    public virtual ICollection<BuoiHoc> BuoiHocs { get; set; } = new List<BuoiHoc>();

    public virtual GiaSu GiaSu { get; set; }

    public virtual HocVien HocVien { get; set; }

    public virtual MonHocDanhMuc MonHocDM { get; set; }

    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
