using System;
using System.Collections.Generic;

namespace DoAnWeb.Data.Entities;

public partial class MonHocDanhMuc
{
    public int MaMon { get; set; }

    public string TenMon { get; set; }

    public string LoaiMon { get; set; }

    public string CapDo { get; set; }

    public bool NgungDay { get; set; }

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
