using System;
using System.Collections.Generic;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoAnWeb.ViewModels.HocTap
{
    public class DiemDanhViewModel
    {
        public DateTime Ngay { get; set; }
        public string Ca { get; set; }
        public int? MaGS { get; set; }
        public bool LaQuanLy { get; set; }
        public List<SelectListItem> CacCa { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> GiaSus { get; set; } = new List<SelectListItem>();
        public List<DiemDanhDongViewModel> Rows { get; set; } = new List<DiemDanhDongViewModel>();
        public List<string> Loi { get; set; } = new List<string>();
    }

    public class DiemDanhDongViewModel
    {
        public int MaLop { get; set; }
        public int? MaBuoi { get; set; }
        public int MaHV { get; set; }
        public string TenLop { get; set; }
        public string HocVien { get; set; }
        public string MonHoc { get; set; }
        public string GiaSu { get; set; }
        public string CaHoc { get; set; }
        public string DiemDanh { get; set; }
        public string DiemText { get; set; }
        public string NhanXet { get; set; }
        public bool ThiLenCap { get; set; }
        public string CapDoThi { get; set; }
        public string GhiChu { get; set; }
        public bool LaHocBu { get; set; }
        public bool LaNangKhieu { get; set; }
        public bool DaLuu { get; set; }
        public bool DaXepBu { get; set; }
    }

    public class HocTapDanhSachViewModel
    {
        public string TuKhoa { get; set; }
        public List<DoAnWeb.Data.Entities.HocVien> HocViens { get; set; } = new List<DoAnWeb.Data.Entities.HocVien>();
    }

    public class HocVienKetQuaViewModel
    {
        public int MaHV { get; set; }
        public string TenHocVien { get; set; }
        public bool LaQuanLy { get; set; }
        public List<KetQuaLop> CacLop { get; set; } = new List<KetQuaLop>();
        public List<BuoiHocSuaViewModel> Buoi { get; set; } = new List<BuoiHocSuaViewModel>();
        public List<string> Loi { get; set; } = new List<string>();
    }

    public class BuoiHocSuaViewModel
    {
        public int MaBuoi { get; set; }
        public int MaLop { get; set; }
        public BuoiHocRow Goc { get; set; }
        public string DiemDanh { get; set; }
        public string DiemText { get; set; }
        public string NhanXet { get; set; }
        public bool ThiLenCap { get; set; }
        public bool LaNangKhieu { get; set; }
        public bool KhoaSua { get; set; }
        public bool ChoXepBu { get; set; }
        public bool ChoHuyBu { get; set; }
    }

    public class BuoiHocSuaDongInput
    {
        public int MaBuoi { get; set; }
        public int MaLop { get; set; }
        public string DiemDanh { get; set; }
        public string DiemText { get; set; }
        public string NhanXet { get; set; }
        public bool ThiLenCap { get; set; }
    }
}
