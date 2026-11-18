using System;
using System.Collections.Generic;

namespace DoAnWeb.Data.Entities;

public partial class GiaSu
{
    public int MaGS { get; set; }

    public string HoTen { get; set; }

    public string SDT { get; set; }

    public string ChuyenMon { get; set; }

    public string TrangThai { get; set; }

    public string Email { get; set; }

    public string HocVan { get; set; }

    public int? KinhNghiem { get; set; }

    public string CaNhanDay { get; set; }

    public string GhiChu { get; set; }

    public virtual ICollection<BuoiHoc> BuoiHocs { get; set; } = new List<BuoiHoc>();

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();

    public virtual TaiKhoan TaiKhoan { get; set; }
}
