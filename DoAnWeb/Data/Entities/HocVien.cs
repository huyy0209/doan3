using System;
using System.Collections.Generic;

namespace DoAnWeb.Data.Entities;

public partial class HocVien
{
    public int MaHV { get; set; }

    public string HoTen { get; set; }

    public string SDT { get; set; }

    public string DiaChi { get; set; }

    public string TenPhuHuynh { get; set; }

    public string SDTPhuHuynh { get; set; }

    public string TruongLop { get; set; }

    public string TrinhDo { get; set; }

    public string GhiChu { get; set; }

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
}
