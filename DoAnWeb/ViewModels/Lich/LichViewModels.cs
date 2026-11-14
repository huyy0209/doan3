using System;
using System.Collections.Generic;
using DoAnWeb.Data.Entities;

namespace DoAnWeb.ViewModels.Lich
{
    public class LichIndexViewModel
    {
        public DateTime Thang { get; set; }
        public int? MaGS { get; set; }
        public DateTime? NgayChon { get; set; }
        public List<DoAnWeb.Data.Entities.GiaSu> GiaSus { get; set; } = new List<DoAnWeb.Data.Entities.GiaSu>();
        public List<LichNgayViewModel> CacNgay { get; set; } = new List<LichNgayViewModel>();
        public int SoHang { get; set; }
        public List<LopHoc> LopNgayChon { get; set; } = new List<LopHoc>();
    }

    public class LichNgayViewModel
    {
        public DateTime? Ngay { get; set; }
        public List<LopHoc> LopHoc { get; set; } = new List<LopHoc>();
    }
}
