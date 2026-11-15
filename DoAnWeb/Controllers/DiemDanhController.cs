using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using DoAnWeb.ViewModels.HocTapVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoAnWeb.Controllers
{
    [Authorize(Roles = "QuanLy,GiaSu")]
    public class DiemDanhController : Controller
    {
        private readonly HocTapService _hocTap;
        private readonly GiaSuService _giaSu;
        private readonly INguoiDungHienTai _nguoiDung;

        public DiemDanhController(HocTapService hocTap, GiaSuService giaSu, INguoiDungHienTai nguoiDung)
        {
            _hocTap = hocTap;
            _giaSu = giaSu;
            _nguoiDung = nguoiDung;
        }

        [HttpGet]
        public IActionResult Index(DateTime? ngay, string ca, int? maGS)
        {
            var ngayChon = ngay?.Date ?? DateTime.Today;
            if (ngayChon > DateTime.Today)
            {
                ngayChon = DateTime.Today;
                TempData["Loi"] = "Không thể điểm danh cho ngày trong tương lai.";
            }
            return View(TaoModel(ngayChon, ca, maGS));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Luu(DiemDanhViewModel model)
        {
            model.Loi ??= new List<string>();
            if (model.Ngay.Date > DateTime.Today)
            {
                model.Loi.Add("Không thể điểm danh cho ngày trong tương lai.");
                model.Ngay = DateTime.Today;
                return View("Index", TaoModel(model.Ngay, model.Ca, model.MaGS, model.Rows, model.Loi));
            }

            var rows = new List<DiemDanhRow>();
            foreach (var row in model.Rows ?? new List<DiemDanhDongViewModel>())
            {
                if (!HocTapService.TryParseDiem(row.DiemText, out var diem) || (diem.HasValue && (diem < 0 || diem > 10)))
                {
                    model.Loi.Add($"Điểm của {row.HocVien} không hợp lệ (nhập từ 0 đến 10, ví dụ 8,5).");
                    continue;
                }
                rows.Add(new DiemDanhRow
                {
                    MaLop = row.MaLop,
                    MaBuoi = row.MaBuoi,
                    DiemDanh = row.DiemDanh,
                    Diem = diem,
                    NhanXet = row.NhanXet,
                    ThiLenCap = row.ThiLenCap,
                    CapDoThi = row.CapDoThi
                });
            }

            try
            {
                if (rows.Count > 0)
                    model.Loi.AddRange(_hocTap.LuuNgay(model.Ngay, rows, _nguoiDung.GioiHanGiaSu));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                model.Loi.Add(ex.Message);
            }

            if (model.Loi.Count == 0)
            {
                TempData["ThongBao"] = $"Đã lưu điểm danh ngày {model.Ngay:dd/MM/yyyy}.";
                return RedirectToAction(nameof(Index), new { ngay = model.Ngay.ToString("yyyy-MM-dd"), ca = model.Ca, maGS = _nguoiDung.LaQuanLy ? model.MaGS : null });
            }

            var caHopLe = string.IsNullOrEmpty(model.Ca) || Constants.CaHoc.TatCaCaHoc.Contains(model.Ca);
            var maGiaSu = _nguoiDung.LaQuanLy ? model.MaGS : _nguoiDung.GioiHanGiaSu;
            var daLuu = _hocTap.GetLopTrongNgay(model.Ngay, caHopLe ? model.Ca : null, maGiaSu);
            foreach (var row in model.Rows ?? new List<DiemDanhDongViewModel>())
            {
                var luu = row.MaBuoi.HasValue
                    ? daLuu.FirstOrDefault(r => r.MaBuoi == row.MaBuoi)
                    : daLuu.FirstOrDefault(r => r.MaLop == row.MaLop && !r.LaHocBu);
                if (luu != null) row.DaLuu = luu.DaLuu;
            }
            return View("Index", TaoModel(model.Ngay, model.Ca, model.MaGS, model.Rows, model.Loi));
        }

        private DiemDanhViewModel TaoModel(DateTime ngay, string ca, int? maGS,
            IEnumerable<DiemDanhDongViewModel> rows = null, IEnumerable<string> loi = null)
        {
            bool laQuanLy = _nguoiDung.LaQuanLy;
            var maGiaSu = laQuanLy ? maGS : _nguoiDung.GioiHanGiaSu;
            var caHopLe = string.IsNullOrEmpty(ca) || Constants.CaHoc.TatCaCaHoc.Contains(ca);
            var duLieu = rows?.ToList() ?? _hocTap.GetLopTrongNgay(ngay, caHopLe ? ca : null, maGiaSu)
                .Select(r => new DiemDanhDongViewModel
                {
                    MaLop = r.MaLop,
                    MaBuoi = r.MaBuoi,
                    MaHV = r.MaHV,
                    TenLop = r.TenLop,
                    HocVien = r.HocVien,
                    MonHoc = r.MonHoc,
                    GiaSu = r.GiaSu,
                    CaHoc = r.CaHoc,
                    DiemDanh = r.DiemDanh,
                    DiemText = r.Diem?.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                    NhanXet = r.NhanXet,
                    ThiLenCap = r.ThiLenCap,
                    CapDoThi = r.CapDoThi,
                    GhiChu = r.GhiChu ?? (r.DaLuu ? "Đã lưu" : ""),
                    LaHocBu = r.LaHocBu,
                    LaNangKhieu = r.LaNangKhieu,
                    DaLuu = r.DaLuu,
                    DaXepBu = r.DaXepBu
                }).ToList();

            return new DiemDanhViewModel
            {
                Ngay = ngay,
                Ca = caHopLe ? ca : null,
                MaGS = laQuanLy ? maGS : null,
                LaQuanLy = laQuanLy,
                Rows = duLieu,
                Loi = loi?.ToList() ?? new List<string>(),
                CacCa = new[] { new SelectListItem("Mọi ca", "") }
                    .Concat(Constants.CaHoc.TatCaCaHoc.Select(c => new SelectListItem(c, c, c == ca))).ToList(),
                GiaSus = laQuanLy
                    ? new[] { new SelectListItem("Mọi gia sư", "") }
                        .Concat(_giaSu.GetAll().Select(g => new SelectListItem(g.HoTen, g.MaGS.ToString(), g.MaGS == maGS))).ToList()
                    : new List<SelectListItem>()
            };
        }
    }
}
