using System;
using System.Collections.Generic;

namespace DoAnWeb.Data.Entities;

public partial class BuoiHoc
{
    public int MaBuoi { get; set; }

    public int MaLop { get; set; }

    public DateTime NgayHoc { get; set; }

    public string DiemDanh { get; set; }

    public decimal? Diem { get; set; }

    public string NhanXet { get; set; }

    public string CaHoc { get; set; }

    public int? MaGSDay { get; set; }

    public int? MaBuoiGoc { get; set; }

    public bool ThiLenCap { get; set; }

    public string CapDoThi { get; set; }

    public virtual BuoiHoc BuoiBu { get; set; }

    public virtual BuoiHoc BuoiGoc { get; set; }

    public virtual GiaSu GiaSuDay { get; set; }

    public virtual LopHoc LopHoc { get; set; }
}
