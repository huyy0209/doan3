using System;
using System.Collections.Generic;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services.Dto;

namespace DoAnWeb.ViewModels.HocPhi
{
    public class HocPhiIndexViewModel
    {
        public string TuKhoa { get; set; }
        public string TinhTrang { get; set; }
        public int? MaLop { get; set; }
        public List<CongNoLop> DanhSach { get; set; } = new List<CongNoLop>();
        public decimal TongHocPhi { get; set; }
        public decimal DaThu { get; set; }
        public decimal ConNo { get; set; }
        public CongNoLop LopChon { get; set; }
        public List<ThanhToan> LichSu { get; set; } = new List<ThanhToan>();
        public HocPhiThanhToanViewModel ThanhToan { get; set; } = new HocPhiThanhToanViewModel();
    }

    public class HocPhiThanhToanViewModel
    {
        public int MaLop { get; set; }
        public string SoTien { get; set; }
        public DateTime NgayDong { get; set; } = DateTime.Today;
        public string HinhThuc { get; set; }
        public string GhiChu { get; set; }
        public string TuKhoa { get; set; }
        public string TinhTrang { get; set; }
    }
}
