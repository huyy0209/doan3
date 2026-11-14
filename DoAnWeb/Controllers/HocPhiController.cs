using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using DoAnWeb.ViewModels.HocPhi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers
{
    [Authorize(Roles = "QuanLy")]
    public class HocPhiController : Controller
    {
        private const string CanThu = "Cần thu (còn nợ + chưa đóng)";
        private readonly HocPhiService _hocPhi;
        private readonly BaoCaoService _baoCao;

        public HocPhiController(HocPhiService hocPhi, BaoCaoService baoCao)
        {
            _hocPhi = hocPhi;
            _baoCao = baoCao;
        }

        [HttpGet]
        public IActionResult Index(int? maLop, string tuKhoa, string tinhTrang)
        {
            return View(TaoModel(maLop, tuKhoa, tinhTrang, null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemThanhToan(HocPhiIndexViewModel model)
        {
            var input = model.ThanhToan;
            if (!ModelState.IsValid)
                return View("Index", TaoModel(input.MaLop, input.TuKhoa, input.TinhTrang, input));

            if (!SoTien.TryParse(input.SoTien, out var soTien))
            {
                ModelState.AddModelError("ThanhToan.SoTien", "Nhập số tiền, ví dụ 1.500.000.");
                return View("Index", TaoModel(input.MaLop, input.TuKhoa, input.TinhTrang, input));
            }

            try
            {
                _hocPhi.ThemThanhToan(input.MaLop, soTien, input.NgayDong.Date.Add(DateTime.Now.TimeOfDay),
                    input.HinhThuc, input.GhiChu);
                TempData["ThongBao"] = $"Đã ghi nhận {SoTien.Dinh(soTien)} đ.";
                return RedirectToAction(nameof(Index), new { maLop = input.MaLop, tuKhoa = input.TuKhoa, tinhTrang = input.TinhTrang });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", TaoModel(input.MaLop, input.TuKhoa, input.TinhTrang, input));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaThanhToan(int maTT, int maLop, string tuKhoa, string tinhTrang)
        {
            try
            {
                _hocPhi.XoaThanhToan(maTT);
                TempData["ThongBao"] = "Đã xóa lần đóng học phí.";
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                TempData["Loi"] = ex.Message;
            }
            return RedirectToAction(nameof(Index), new { maLop, tuKhoa, tinhTrang });
        }

        [HttpGet]
        public IActionResult XuatCsv(string tuKhoa, string tinhTrang)
        {
            var ds = LocCongNo(_hocPhi.GetCongNo(), tuKhoa, tinhTrang);
            var rows = new List<string[]>
            {
                new[] { "Lớp", "Học viên", "SĐT liên hệ", "Môn học", "Gia sư", "Học phí", "Đã đóng", "Còn nợ", "Tình trạng" }
            };
            rows.AddRange(ds.Select(c => new[]
            {
                c.TenLop, c.HocVien, c.SDTLienHe, c.MonHoc, c.GiaSu,
                c.HocPhi.ToString("0"), c.DaDong.ToString("0"), c.ConNo.ToString("0"), c.TinhTrang
            }));
            return File(_baoCao.XuatCsv(rows), "text/csv", $"CongNo_{DateTime.Now:yyyyMMdd}.csv");
        }

        private HocPhiIndexViewModel TaoModel(int? maLop, string tuKhoa, string tinhTrang, HocPhiThanhToanViewModel input)
        {
            string loc = tinhTrang ?? CanThu;
            var all = _hocPhi.GetCongNo();
            var ds = LocCongNo(all, tuKhoa, loc);
            var selected = maLop.HasValue ? all.FirstOrDefault(c => c.MaLop == maLop.Value) : null;
            var form = input ?? new HocPhiThanhToanViewModel
            {
                MaLop = selected?.MaLop ?? 0,
                SoTien = selected != null && selected.ConNo > 0 ? SoTien.Dinh(selected.ConNo) : "",
                NgayDong = DateTime.Today,
                HinhThuc = Constants.GoiY.HinhThucThanhToan[0],
                TuKhoa = tuKhoa,
                TinhTrang = loc
            };
            form.MaLop = selected?.MaLop ?? form.MaLop;
            form.TuKhoa = tuKhoa;
            form.TinhTrang = loc;

            return new HocPhiIndexViewModel
            {
                TuKhoa = tuKhoa,
                TinhTrang = loc,
                MaLop = maLop,
                DanhSach = ds,
                TongHocPhi = all.Where(c => c.TrangThaiLop != Constants.LopHocStatus.DaHuy).Sum(c => c.HocPhi),
                DaThu = all.Sum(c => c.DaDong),
                ConNo = all.Where(c => c.TrangThaiLop != Constants.LopHocStatus.DaHuy).Sum(c => c.ConNo),
                LopChon = selected,
                LichSu = selected == null ? new List<ThanhToan>() : _hocPhi.GetLichSu(selected.MaLop),
                ThanhToan = form
            };
        }

        private static List<CongNoLop> LocCongNo(IEnumerable<CongNoLop> congNo, string tuKhoa, string tinhTrang)
        {
            var ds = congNo.Where(c =>
                string.IsNullOrWhiteSpace(tinhTrang) || tinhTrang == "Tất cả" ||
                (tinhTrang == CanThu ? c.TinhTrang != HocPhiService.DaDu : c.TinhTrang == tinhTrang));
            if (!string.IsNullOrWhiteSpace(tuKhoa))
                ds = ds.Where(c => TimKiem.Khop(tuKhoa, c.TenLop, c.HocVien, c.SDTLienHe, c.MonHoc, c.GiaSu));
            return ds.ToList();
        }
    }
}
