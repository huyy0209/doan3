using System;
using System.Collections.Generic;

namespace DoAnWeb.Data.Entities;

public partial class ThanhToan
{
    public int MaTT { get; set; }

    public int MaLop { get; set; }

    public decimal SoTien { get; set; }

    public DateTime NgayDong { get; set; }

    public string HinhThuc { get; set; }

    public string GhiChu { get; set; }

    public virtual LopHoc LopHoc { get; set; }
}
