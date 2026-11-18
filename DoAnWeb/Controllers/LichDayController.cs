using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.LichDay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;

[Authorize(Roles = "QuanLy,GiaSu")]
public class LichDayController : Controller
{
    private readonly LichDayService _lichDay;
    private readonly GiaSuService _giaSu;
    private readonly INguoiDungHienTai _nguoiDung;

    public LichDayController(LichDayService lichDay, GiaSuService giaSu, INguoiDungHienTai nguoiDung)
    {
        _lichDay = lichDay;
        _giaSu = giaSu;
        _nguoiDung = nguoiDung;
    }

    [HttpGet]
    public IActionResult Index(DateTime? ngay, int? maGS, string monHoc)
    {
        var ngayChon = ngay?.Date ?? DateTime.Today;
        if (ngayChon < DateTime.MinValue.Date.AddDays(6) ||
            ngayChon > DateTime.MaxValue.Date.AddDays(-6))
            ngayChon = DateTime.Today;

        int soNgayTuThuHai = ((int)ngayChon.DayOfWeek + 6) % 7;
        var batDauTuan = ngayChon.AddDays(-soNgayTuThuHai).Date;
        bool laQuanLy = _nguoiDung.LaQuanLy;
        int? gioiHanGiaSu = laQuanLy ? null : _nguoiDung.GioiHanGiaSu;
        var dsGiaSu = _nguoiDung.LaQuanLy ? _giaSu.GetAll().ToList() : new List<GiaSu>();
        if (laQuanLy && maGS.HasValue && dsGiaSu.All(g => g.MaGS != maGS.Value))
            maGS = null;
        if (!laQuanLy)
            maGS = null;

        int? maGiaSuLoc = laQuanLy ? maGS : gioiHanGiaSu;
        var cacLop = maGiaSuLoc == null && !laQuanLy
            ? new List<LopHoc>()
            : _lichDay.LayLopTrongTuan(batDauTuan, maGiaSuLoc);
        var cacMon = cacLop.Select(l => l.MonHoc)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => m, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        if (!string.IsNullOrWhiteSpace(monHoc) && cacMon.Contains(monHoc, StringComparer.OrdinalIgnoreCase))
            cacLop = cacLop.Where(l => string.Equals(l.MonHoc, monHoc, StringComparison.OrdinalIgnoreCase)).ToList();
        else
            monHoc = null;

        var cacNgay = Enumerable.Range(0, 7).Select(i =>
        {
            var ngayTrongTuan = batDauTuan.AddDays(i);
            return new LichDayNgayViewModel
            {
                Ngay = ngayTrongTuan,
                HomNay = ngayTrongTuan == DateTime.Today,
                CacCa = Constants.CaHoc.TatCaCaHoc.Select(ca => new LichDayCaViewModel
                {
                    Ten = ca,
                    LopHoc = cacLop.Where(l => l.CaHoc == ca && l.HocNgay(ngayTrongTuan))
                        .OrderBy(l => l.TenHienThi)
                        .ToList()
                }).ToList()
            };
        }).ToList();

        return View(new LichDayIndexViewModel
        {
            BatDauTuan = batDauTuan,
            CacNgay = cacNgay,
            LaQuanLy = laQuanLy,
            MaGS = maGS,
            GiaSus = dsGiaSu,
            MonHoc = monHoc,
            CacMon = cacMon,
            SoLop = cacLop.Select(l => l.MaLop).Distinct().Count()
        });
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        if (!_nguoiDung.LaQuanLy && !_nguoiDung.GioiHanGiaSu.HasValue)
            return NotFound();
        var lop = _lichDay.LayLop(id, _nguoiDung.GioiHanGiaSu);
        if (lop == null) return NotFound();
        return View(new LichDayChiTietViewModel { LopHoc = lop });
    }
}
