using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Controllers;

[Authorize(Roles = "QuanLy,GiaSu")]
public class ThongBaoController : Controller
{
    private readonly ThongBaoService _thongBao;
    private readonly INguoiDungHienTai _nguoiDung;
    private readonly QuanLyGiaSuContext _db;

    public ThongBaoController(
        ThongBaoService thongBao, INguoiDungHienTai nguoiDung, QuanLyGiaSuContext db)
    {
        _thongBao = thongBao;
        _nguoiDung = nguoiDung;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int trang = 1, int soDong = 20, CancellationToken cancellationToken = default)
    {
        var ketQua = await _thongBao.LayDanhSachAsync(_nguoiDung.MaTK, trang, soDong, cancellationToken);
        var model = new ThongBaoDanhSachViewModel
        {
            KetQua = ketQua,
            ChuaDoc = await _db.ThongBaos.CountAsync(
                x => x.MaTK == _nguoiDung.MaTK && !x.DaDoc, cancellationToken)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Doc(int id, CancellationToken cancellationToken = default)
    {
        string duongDan = await _thongBao.DanhDauDaDocAsync(_nguoiDung.MaTK, id, cancellationToken);
        if (string.IsNullOrWhiteSpace(duongDan) || !Url.IsLocalUrl(duongDan))
            return RedirectToAction(nameof(Index));

        return LocalRedirect(duongDan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DanhDauTatCaDaDoc(CancellationToken cancellationToken = default)
    {
        await _thongBao.DanhDauTatCaDaDocAsync(_nguoiDung.MaTK, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> SapBatDau(CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var thresholdTime = now.AddHours(24);
        var q = _db.LopHocs.AsNoTracking()
            .Include(l => l.GiaSu)
            .Include(l => l.HocVien)
            .Where(l => l.TrangThai == Constants.LopHocStatus.ChuaBatDau &&
                        l.ThoiGianBatDau >= now &&
                        l.ThoiGianBatDau <= thresholdTime);
        if (_nguoiDung.GioiHanGiaSu.HasValue)
            q = q.Where(l => l.MaGS == _nguoiDung.GioiHanGiaSu.Value);
        var classes = await q.OrderBy(l => l.ThoiGianBatDau).ToListAsync(cancellationToken);
        return Json(new
        {
            count = classes.Count,
            classes = classes.Take(5).Select(l => new
            {
                id = l.MaLop,
                tenLop = l.TenHienThi,
                monHoc = l.MonHoc,
                hocVien = l.HocVien?.HoTen,
                giaSu = l.GiaSu?.HoTen,
                batDau = l.ThoiGianBatDau
            })
        });
    }
}
