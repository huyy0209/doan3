using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using DoAnWeb.ViewModels.TongQuan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers
{
    [Authorize(Roles = "QuanLy")]
    public class TongQuanController : Controller
    {
        private readonly TongQuanService _tongQuan;
        private readonly HocPhiService _hocPhi;
        private readonly LopHocService _lopHoc;
        private readonly INguoiDungHienTai _nguoiDung;
        private readonly ThongKeBieuDoService _bieuDo;

        public TongQuanController(TongQuanService tongQuan, HocPhiService hocPhi, LopHocService lopHoc,
            INguoiDungHienTai nguoiDung, ThongKeBieuDoService bieuDo)
        {
            _tongQuan = tongQuan;
            _hocPhi = hocPhi;
            _lopHoc = lopHoc;
            _nguoiDung = nguoiDung;
            _bieuDo = bieuDo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var data = _tongQuan.Lay();
            var lopSapBatDau = _lopHoc.GetLopTrongKhoang(DateTime.Today, DateTime.Today.AddDays(TongQuanService.SoNgayBaoTruoc + 1).AddTicks(-1))
                .Where(l => l.TrangThaiTheoThoiGian == Constants.LopHocStatus.ChuaBatDau)
                .OrderBy(l => l.ThoiGianBatDau)
                .FirstOrDefault();
            var lopConNo = _hocPhi.GetCongNo()
                .Where(c => c.TrangThaiLop != Constants.LopHocStatus.DaHuy && c.ConNo > 0)
                .OrderByDescending(c => c.ConNo)
                .FirstOrDefault();

            var viec = data.Viec.Select(v => new ViecTongQuanViewModel
            {
                Viec = v,
                Url = TaoUrlViec(v, lopConNo?.MaLop, data.SapKetThuc.FirstOrDefault()?.MaLop, lopSapBatDau?.MaLop)
            }).ToList();

            return View(new TongQuanIndexViewModel
            {
                DuLieu = data,
                SoLopTheoCa = _lopHoc.GetSoLopHomNayTheoCa(),
                TenQuanLy = _nguoiDung.TenDangNhap,
                CaHienTai = CaHienTai(DateTime.Now),
                Viec = viec
            });
        }

        /// <summary>Số liệu cho 4 biểu đồ ở trang Tổng quan (chỉ quản lý – cả controller đã gắn [Authorize(Roles = "QuanLy")]).</summary>
        [HttpGet]
        public IActionResult BieuDo() => Json(_bieuDo.Lay());

        private string TaoUrlViec(ViecCanLam viec, int? maLopNo, int? maLopSapKetThuc, int? maLopSapBatDau)
        {
            switch (viec.Loai)
            {
                case LoaiViec.ChuaDiemDanh:
                    return Url.Action("Index", "DiemDanh", new { ngay = (viec.Ngay ?? DateTime.Today).ToString("yyyy-MM-dd") });
                case LoaiViec.VangChuaBu:
                    return viec.MaHV.HasValue ? $"/HocTap/KetQua/{viec.MaHV.Value}" : "/HocTap";
                case LoaiViec.ConNo:
                    return maLopNo.HasValue ? $"/HocPhi?maLop={maLopNo.Value}" : "/HocPhi";
                case LoaiViec.SapKetThuc:
                    return maLopSapKetThuc.HasValue ? $"/LopHoc/Details/{maLopSapKetThuc.Value}" : "/LopHoc";
                case LoaiViec.SapBatDau:
                    return maLopSapBatDau.HasValue ? $"/LopHoc/Details/{maLopSapBatDau.Value}" : "/LopHoc";
                default:
                    return "/";
            }
        }

        private static string CaHienTai(DateTime now)
        {
            foreach (var ca in Constants.CaHoc.TatCaCaHoc)
            {
                if (Constants.CaHoc.TryGetGio(ca, out var batDau, out var ketThuc) &&
                    now.Hour >= batDau && now.Hour < ketThuc)
                    return ca;
            }
            return null;
        }
    }
}
