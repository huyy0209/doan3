using System.Collections.Generic;

namespace DoAnWeb.ViewModels.BaoCaoVM
{
    public class BaoCaoThangViewModel
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal HocPhiThu { get; set; }
        public int HocVienMoi { get; set; }
        public int HocVienNghi { get; set; }
        public decimal NoCuoiThang { get; set; }
        public List<BaoCaoGiaSuViewModel> TheoGiaSu { get; set; } = new();
        public List<BaoCaoMonViewModel> TheoMon { get; set; } = new();
    }

    public class BaoCaoGiaSuViewModel
    {
        public int MaGS { get; set; }
        public string HoTen { get; set; }
        public int SoLopDangDay { get; set; }
        public int SoBuoiDaDiemDanh { get; set; }
        public int SoBuoiVang { get; set; }
        public decimal TyLeChuyenCan { get; set; }
    }

    public class BaoCaoMonViewModel
    {
        public string TenMon { get; set; }
        public int SoLop { get; set; }
        public int SoHocVien { get; set; }
        public decimal HocPhiThu { get; set; }
    }
}
