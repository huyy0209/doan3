using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Infrastructure;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using DoAnWeb.ViewModels.HocPhi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers
{
    [Authorize(Roles = "QuanLy")]
    public class HocPhiController : Controller
    {
        private const string CanThu = "Cần thu (còn nợ + chưa đóng)";
        private readonly HocPhiService _hocPhi;
        private readonly BaoCaoService _baoCao;
        private readonly XuatExcelService _excel;
        private readonly QuanLyGiaSuContext _db;

        public HocPhiController(HocPhiService hocPhi, BaoCaoService baoCao, XuatExcelService excel, QuanLyGiaSuContext db)
        {
            _hocPhi = hocPhi;
            _baoCao = baoCao;
            _excel = excel;
            _db = db;
        }

        [HttpGet]
        public IActionResult Index(int? maLop, string tuKhoa, string tinhTrang,
            int trang = 1, int soDong = PhanTrang.SoDongMacDinh, string sapXep = null, string chieu = null)
        {
            return View(TaoModel(maLop, tuKhoa, tinhTrang, null, trang, soDong, sapXep, chieu));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemThanhToan(HocPhiIndexViewModel model,
            int trang = 1, int soDong = PhanTrang.SoDongMacDinh, string sapXep = null, string chieu = null)
        {
            var input = model.ThanhToan;
            if (!ModelState.IsValid)
                return View("Index", TaoModel(input.MaLop, input.TuKhoa, input.TinhTrang, input, trang, soDong, sapXep, chieu));

            if (!SoTien.TryParse(input.SoTien, out var soTien))
            {
                ModelState.AddModelError("ThanhToan.SoTien", SoTien.ThongBaoDinhDang);
                return View("Index", TaoModel(input.MaLop, input.TuKhoa, input.TinhTrang, input, trang, soDong, sapXep, chieu));
            }

            try
            {
                _hocPhi.ThemThanhToan(input.MaLop, soTien, input.NgayDong.Date.Add(DateTime.Now.TimeOfDay),
                    input.HinhThuc, input.GhiChu);
                TempData["ThongBao"] = $"Đã ghi nhận {SoTien.Dinh(soTien)} đ.";
                return RedirectToAction(nameof(Index), new
                {
                    maLop = input.MaLop, tuKhoa = input.TuKhoa, tinhTrang = input.TinhTrang,
                    trang, soDong, sapXep, chieu
                });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", TaoModel(input.MaLop, input.TuKhoa, input.TinhTrang, input, trang, soDong, sapXep, chieu));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaThanhToan(int maTT, int maLop, string tuKhoa, string tinhTrang,
            int trang = 1, int soDong = PhanTrang.SoDongMacDinh, string sapXep = null, string chieu = null)
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
            return RedirectToAction(nameof(Index), new { maLop, tuKhoa, tinhTrang, trang, soDong, sapXep, chieu });
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

        /// <summary>Xuất công nợ học phí ra Excel (.xlsx) theo đúng bộ lọc của trang.</summary>
        [HttpGet]
        public IActionResult XuatExcel(string tuKhoa, string tinhTrang)
        {
            var ds = LocCongNo(_hocPhi.GetCongNo(), tuKhoa, tinhTrang);
            return File(_excel.CongNo(ds),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"CongNo_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public IActionResult PhieuThu(int id)
        {
            var thanhToan = _db.ThanhToans.AsNoTracking()
                .Include(t => t.LopHoc).ThenInclude(l => l.HocVien)
                .Include(t => t.LopHoc).ThenInclude(l => l.GiaSu)
                .FirstOrDefault(t => t.MaTT == id);
            if (thanhToan?.LopHoc == null) return NotFound();

            var lichSu = _hocPhi.GetLichSu(thanhToan.MaLop)
                .OrderBy(t => t.NgayDong).ThenBy(t => t.MaTT).ToList();
            ViewBag.DaDongLuyKe = lichSu.TakeWhile(t => t.MaTT != id).Sum(t => t.SoTien) + thanhToan.SoTien;
            ViewBag.ConNo = Math.Max(0, (thanhToan.LopHoc.HocPhi ?? 0) - (decimal)ViewBag.DaDongLuyKe);
            ViewBag.NguoiThu = User.Identity?.Name ?? "Quản lý";
            return View("PhieuThu", thanhToan);
        }

        [HttpGet]
        public IActionResult BangKe(int maLop)
        {
            var lop = _db.LopHocs.AsNoTracking().Include(l => l.HocVien).Include(l => l.GiaSu)
                .FirstOrDefault(l => l.MaLop == maLop);
            if (lop == null) return NotFound();
            ViewBag.LichSu = _hocPhi.GetLichSu(maLop).OrderBy(t => t.NgayDong).ThenBy(t => t.MaTT).ToList();
            ViewBag.TongDaDong = ((List<ThanhToan>)ViewBag.LichSu).Sum(t => t.SoTien);
            ViewBag.NguoiLap = User.Identity?.Name ?? "Quản lý";
            return View("BangKe", lop);
        }

        private HocPhiIndexViewModel TaoModel(int? maLop, string tuKhoa, string tinhTrang, HocPhiThanhToanViewModel input,
            int trang, int soDong, string sapXep, string chieu)
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

            bool giamDan = chieu == "desc";
            IEnumerable<CongNoLop> sapXepTheo = sapXep switch
            {
                "tenLop" => (giamDan ? ds.OrderByDescending(c => c.TenLop) : ds.OrderBy(c => c.TenLop)).ThenBy(c => c.MaLop),
                "hocVien" => (giamDan ? ds.OrderByDescending(c => c.HocVien) : ds.OrderBy(c => c.HocVien)).ThenBy(c => c.MaLop),
                "sdt" => (giamDan ? ds.OrderByDescending(c => c.SDTLienHe) : ds.OrderBy(c => c.SDTLienHe)).ThenBy(c => c.MaLop),
                "giaSu" => (giamDan ? ds.OrderByDescending(c => c.GiaSu) : ds.OrderBy(c => c.GiaSu)).ThenBy(c => c.MaLop),
                "hocPhi" => (giamDan ? ds.OrderByDescending(c => c.HocPhi) : ds.OrderBy(c => c.HocPhi)).ThenBy(c => c.MaLop),
                "daDong" => (giamDan ? ds.OrderByDescending(c => c.DaDong) : ds.OrderBy(c => c.DaDong)).ThenBy(c => c.MaLop),
                "conNo" => (giamDan ? ds.OrderByDescending(c => c.ConNo) : ds.OrderBy(c => c.ConNo)).ThenBy(c => c.MaLop),
                "tinhTrang" => (giamDan ? ds.OrderByDescending(c => c.TinhTrang) : ds.OrderBy(c => c.TinhTrang)).ThenBy(c => c.MaLop),
                _ => ds
            };
            var ketQuaTrang = PhanTrang.Tao(sapXepTheo, trang, soDong, sapXep, chieu,
                new Dictionary<string, string>
                {
                    ["maLop"] = maLop?.ToString() ?? "",
                    ["tuKhoa"] = tuKhoa ?? "",
                    ["tinhTrang"] = loc
                });
            ViewBag.PhanTrang = ketQuaTrang;
            return new HocPhiIndexViewModel
            {
                TuKhoa = tuKhoa,
                TinhTrang = loc,
                MaLop = maLop,
                DanhSach = ketQuaTrang.Items,
                TongHocPhi = all.Where(c => c.TrangThaiLop != Constants.LopHocStatus.DaHuy).Sum(c => c.HocPhi),
                DaThu = all.Sum(c => c.DaDong),
                ConNo = ds.Where(c => c.TrangThaiLop != Constants.LopHocStatus.DaHuy).Sum(c => c.ConNo),
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
