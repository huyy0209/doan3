using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using DoAnWeb.ViewModels.HocTap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers
{
    [Authorize(Roles = "QuanLy,GiaSu")]
    public class HocTapController : Controller
    {
        private readonly HocTapService _hocTap;
        private readonly HocVienService _hocVien;
        private readonly BaoCaoService _baoCao;
        private readonly INguoiDungHienTai _nguoiDung;

        public HocTapController(HocTapService hocTap, HocVienService hocVien, BaoCaoService baoCao, INguoiDungHienTai nguoiDung)
        {
            _hocTap = hocTap;
            _hocVien = hocVien;
            _baoCao = baoCao;
            _nguoiDung = nguoiDung;
        }

        [HttpGet]
        public IActionResult Index(string tuKhoa)
        {
            var ds = _hocTap.GetHocVienCoLop(_nguoiDung.GioiHanGiaSu);
            if (!string.IsNullOrWhiteSpace(tuKhoa))
                ds = ds.Where(h => DoAnWeb.Common.TimKiem.Khop(tuKhoa, h.HoTen, h.SDT, h.TenPhuHuynh)).ToList();
            return View(new HocTapDanhSachViewModel { TuKhoa = tuKhoa, HocViens = ds });
        }

        [HttpGet("HocTap/KetQua/{maHV:int}")]
        public IActionResult KetQua(int maHV)
        {
            if (!_hocVien.DuocXem(maHV)) return StatusCode(403);
            var hocVien = _hocVien.GetById(maHV);
            if (hocVien == null) return NotFound();
            return View(TaoKetQua(hocVien, null, null));
        }

        [HttpPost("HocTap/KetQua/{maHV:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult KetQua(int maHV, List<BuoiHocSuaDongInput> buoi)
        {
            if (!_hocVien.DuocXem(maHV)) return StatusCode(403);
            var hocVien = _hocVien.GetById(maHV);
            if (hocVien == null) return NotFound();

            var ketQua = _hocTap.GetKetQuaHocVien(maHV, _nguoiDung.GioiHanGiaSu);
            var goc = ketQua.SelectMany(k => k.Buoi.Select(b => new { Lop = k, Buoi = b }))
                .ToDictionary(x => x.Buoi.MaBuoi);
            var loi = new List<string>();
            var rows = new List<BuoiHocRow>();
            var ids = new HashSet<int>();

            foreach (var input in buoi ?? new List<BuoiHocSuaDongInput>())
            {
                if (!ids.Add(input.MaBuoi) || !goc.TryGetValue(input.MaBuoi, out var saved) || saved.Lop.MaLop != input.MaLop)
                {
                    loi.Add("Danh sách buổi học không hợp lệ. Hãy tải lại trang và thử lại.");
                    continue;
                }

                var b = saved.Buoi;
                bool duocSua = _nguoiDung.LaQuanLy || saved.Lop.MaGS == _nguoiDung.GioiHanGiaSu ||
                               (b.LaHocBu && b.MaGSDay == _nguoiDung.GioiHanGiaSu);
                bool khoa = (b.LaHocBu && b.ChuaToiNgay) || !duocSua;
                if (khoa)
                {
                    if (input.DiemDanh != b.DiemDanh || input.DiemText != b.Diem?.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) ||
                        input.NhanXet != b.NhanXet || input.ThiLenCap != b.ThiLenCap)
                        loi.Add($"{b.NgayHoc:dd/MM}: không có quyền sửa buổi này.");
                    continue;
                }

                if (!HocTapService.TryParseDiem(input.DiemText, out var diem) || (diem.HasValue && (diem < 0 || diem > 10)))
                {
                    loi.Add($"Điểm ngày {b.NgayHoc:dd/MM} không hợp lệ (nhập từ 0 đến 10, ví dụ 8,5).");
                    continue;
                }
                rows.Add(new BuoiHocRow
                {
                    MaBuoi = b.MaBuoi,
                    MaLop = b.MaLop,
                    NgayHoc = b.NgayHoc,
                    DiemDanh = input.DiemDanh,
                    Diem = diem,
                    NhanXet = input.NhanXet,
                    ThiLenCap = saved.Lop.LaNangKhieu && input.ThiLenCap,
                    CapDoThi = b.CapDoThi
                });
            }

            if (rows.Count > 0)
            {
                try { loi.AddRange(_hocTap.LuuCacBuoi(rows, _nguoiDung.GioiHanGiaSu)); }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
                {
                    loi.Add(ex.Message);
                }
            }

            if (loi.Count == 0)
            {
                TempData["ThongBao"] = $"Đã lưu {rows.Count} buổi học.";
                return RedirectToAction(nameof(KetQua), new { maHV });
            }

            var model = TaoKetQua(hocVien, loi, buoi);
            return View(model);
        }

        [HttpGet]
        public IActionResult GiaSuHocBu(DateTime ngay, string ca, string monHoc)
        {
            var ds = _hocTap.GetGiaSuChoHocBu(ngay, ca)
                .OrderBy(g => g.Ban != null)
                .ThenBy(g => !string.Equals(g.ChuyenMon, monHoc, StringComparison.OrdinalIgnoreCase))
                .ThenBy(g => g.HoTen)
                .Select(g => new { g.MaGS, g.HoTen, g.ChuyenMon, g.Ban })
                .ToList();
            return Json(ds);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XepHocBu(int maBuoiVang, DateTime ngay, string ca, int maGSDay, bool boQuaCanhBao = false)
        {
            try
            {
                var result = _hocTap.XepHocBu(maBuoiVang, ngay, ca, maGSDay, boQuaCanhBao, _nguoiDung.GioiHanGiaSu);
                if (!string.IsNullOrEmpty(result.CanhBao)) return Json(new { CanhBao = result.CanhBao });
                if (!result.ThanhCong) return Json(new { Loi = result.Loi ?? "Không xếp được lịch học bù." });
                return Json(new { ThanhCong = true });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                return Json(new { Loi = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HuyHocBu(int maBuoiBu, int maHV)
        {
            if (!_hocVien.DuocXem(maHV)) return StatusCode(403);
            try
            {
                _hocTap.HuyHocBu(maBuoiBu, _nguoiDung.GioiHanGiaSu);
                TempData["ThongBao"] = "Đã hủy buổi học bù. Buổi vắng được giữ nguyên.";
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                TempData["Loi"] = ex.Message;
            }
            return RedirectToAction(nameof(KetQua), new { maHV });
        }

        [HttpGet("HocTap/PhieuBao/{maHV:int}")]
        public IActionResult PhieuBao(int maHV)
        {
            if (!_hocVien.DuocXem(maHV)) return StatusCode(403);
            HocVien hocVien = _hocVien.GetById(maHV);
            if (hocVien == null) return NotFound();
            var ketQua = _hocTap.GetKetQuaHocVien(maHV, _nguoiDung.GioiHanGiaSu);
            return Content(_baoCao.TaoPhieuBao(hocVien, ketQua), "text/html; charset=utf-8");
        }

        private HocVienKetQuaViewModel TaoKetQua(HocVien hocVien, IEnumerable<string> loi, IEnumerable<BuoiHocSuaDongInput> posted)
        {
            var classes = _hocTap.GetKetQuaHocVien(hocVien.MaHV, _nguoiDung.GioiHanGiaSu);
            var postedById = (posted ?? Enumerable.Empty<BuoiHocSuaDongInput>()).GroupBy(x => x.MaBuoi)
                .ToDictionary(g => g.Key, g => g.First());
            var sessions = new List<BuoiHocSuaViewModel>();
            foreach (var k in classes)
            {
                foreach (var b in k.Buoi)
                {
                    var input = postedById.TryGetValue(b.MaBuoi, out var p) ? p : null;
                    bool duocSua = _nguoiDung.LaQuanLy || k.MaGS == _nguoiDung.GioiHanGiaSu ||
                                   (b.LaHocBu && b.MaGSDay == _nguoiDung.GioiHanGiaSu);
                    sessions.Add(new BuoiHocSuaViewModel
                    {
                        MaBuoi = b.MaBuoi,
                        MaLop = b.MaLop,
                        Goc = b,
                        DiemDanh = input?.DiemDanh ?? b.DiemDanh,
                        DiemText = input?.DiemText ?? b.Diem?.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                        NhanXet = input?.NhanXet ?? b.NhanXet,
                        ThiLenCap = input?.ThiLenCap ?? b.ThiLenCap,
                        LaNangKhieu = k.LaNangKhieu,
                        KhoaSua = (b.LaHocBu && b.ChuaToiNgay) || !duocSua,
                        ChoXepBu = b.CoTheXepBu && (_nguoiDung.LaQuanLy || k.MaGS == _nguoiDung.GioiHanGiaSu),
                        ChoHuyBu = (b.LaHocBu || b.MaBuoiBu.HasValue) &&
                                   (_nguoiDung.LaQuanLy || k.MaGS == _nguoiDung.GioiHanGiaSu ||
                                    (b.LaHocBu && b.MaGSDay == _nguoiDung.GioiHanGiaSu))
                    });
                }
            }
            return new HocVienKetQuaViewModel
            {
                MaHV = hocVien.MaHV,
                TenHocVien = hocVien.HoTen,
                LaQuanLy = _nguoiDung.LaQuanLy,
                CacLop = classes,
                Buoi = sessions,
                Loi = loi?.ToList() ?? new List<string>()
            };
        }
    }
}
