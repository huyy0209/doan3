using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Infrastructure;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.HocVien;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Controllers;

[Authorize]
public class HocVienController : Controller
{
    private const string DangHoc = "Đang học";
    private const string ChuaCoLop = "Chưa có lớp";
    private const string DaNghi = "Đã nghỉ";

    private readonly HocVienService _hocVien;
    private readonly QuanLyGiaSuContext _db;
    private readonly INguoiDungHienTai _nguoiDung;

    public HocVienController(HocVienService hocVien, QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung)
    {
        _hocVien = hocVien;
        _db = db;
        _nguoiDung = nguoiDung;
    }

    [HttpGet]
    public IActionResult Index(string tuKhoa, string trinhDo, string tinhTrang,
        int trang = 1, int soDong = PhanTrang.SoDongMacDinh, string sapXep = null, string chieu = null)
    {
        var ds = _hocVien.GetDuocXem(tuKhoa);
        var levels = ds.Select(h => h.TrinhDo).Where(t => !string.IsNullOrWhiteSpace(t))
            .Concat(Constants.GoiY.TrinhDo)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
        if (!string.IsNullOrWhiteSpace(trinhDo))
            ds = ds.Where(h => h.TrinhDo == trinhDo).ToList();

        var ids = ds.Select(h => h.MaHV).ToList();
        var lopTruyCap = _db.LopHocs.AsNoTracking()
            .Where(l => l.MaHV.HasValue && ids.Contains(l.MaHV.Value));
        if (!_nguoiDung.LaQuanLy)
        {
            int maGS = _nguoiDung.MaGS.Value;
            lopTruyCap = lopTruyCap.Where(l => l.MaGS == maGS || l.BuoiHocs.Any(b => b.MaGSDay == maGS));
        }
        var lopHoc = lopTruyCap.ToList();
        var dangHoc = lopHoc.Where(l => l.DangHoatDong && Constants.LopHocStatus.LaHoatDong(l.TrangThaiTheoThoiGian))
            .GroupBy(l => l.MaHV.Value)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(l => l.MonHoc).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct()));
        var coLop = lopHoc.Select(l => l.MaHV.Value).ToHashSet();
        if (!string.IsNullOrWhiteSpace(tinhTrang))
        {
            ds = ds.Where(h =>
            {
                bool dangTheoHoc = dangHoc.ContainsKey(h.MaHV);
                bool coLichSu = coLop.Contains(h.MaHV);
                return tinhTrang == DangHoc ? dangTheoHoc
                    : tinhTrang == ChuaCoLop ? !coLichSu
                    : tinhTrang == DaNghi && coLichSu && !dangTheoHoc;
            }).ToList();
        }

        IEnumerable<HocVienListItemVM> hocViens = ds.Select(h => new HocVienListItemVM
        {
            HocVien = h,
            DangHocMon = dangHoc.TryGetValue(h.MaHV, out var mon) ? mon : ""
        }).ToList();
        bool giamDan = chieu == "desc";
        hocViens = sapXep switch
        {
            "hoTen" => (giamDan ? hocViens.OrderByDescending(h => h.HocVien.HoTen) : hocViens.OrderBy(h => h.HocVien.HoTen)).ThenBy(h => h.HocVien.MaHV),
            "sdt" => (giamDan ? hocViens.OrderByDescending(h => h.HocVien.SDTLienHe) : hocViens.OrderBy(h => h.HocVien.SDTLienHe)).ThenBy(h => h.HocVien.MaHV),
            "phuHuynh" => (giamDan ? hocViens.OrderByDescending(h => h.PhuHuynh) : hocViens.OrderBy(h => h.PhuHuynh)).ThenBy(h => h.HocVien.MaHV),
            "truongLop" => (giamDan ? hocViens.OrderByDescending(h => h.HocVien.TruongLop) : hocViens.OrderBy(h => h.HocVien.TruongLop)).ThenBy(h => h.HocVien.MaHV),
            "trinhDo" => (giamDan ? hocViens.OrderByDescending(h => h.HocVien.TrinhDo) : hocViens.OrderBy(h => h.HocVien.TrinhDo)).ThenBy(h => h.HocVien.MaHV),
            "dangHoc" => (giamDan ? hocViens.OrderByDescending(h => h.DangHocMon) : hocViens.OrderBy(h => h.DangHocMon)).ThenBy(h => h.HocVien.MaHV),
            _ => hocViens
        };
        var ketQuaTrang = PhanTrang.Tao(hocViens, trang, soDong, sapXep, chieu,
            new Dictionary<string, string>
            {
                ["tuKhoa"] = tuKhoa ?? "",
                ["trinhDo"] = trinhDo ?? "",
                ["tinhTrang"] = tinhTrang ?? ""
            });
        ViewBag.PhanTrang = ketQuaTrang;
        return View(new HocVienIndexVM
        {
            TuKhoa = tuKhoa,
            TrinhDo = trinhDo,
            TinhTrang = tinhTrang,
            DanhSachTrinhDo = levels,
            TongSo = ketQuaTrang.TongSo,
            LaQuanLy = _nguoiDung.LaQuanLy,
            HocViens = ketQuaTrang.Items
        });
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        if (!_hocVien.DuocXem(id)) return Forbid();
        var hv = _hocVien.GetById(id);
        if (hv == null) return NotFound();
        IQueryable<DoAnWeb.Data.Entities.LopHoc> lopTruyCap = _db.LopHocs.AsNoTracking()
            .Include(l => l.GiaSu)
            .Where(l => l.MaHV == id);
        if (!_nguoiDung.LaQuanLy)
        {
            int maGS = _nguoiDung.MaGS.Value;
            lopTruyCap = lopTruyCap.Where(l => l.MaGS == maGS || l.BuoiHocs.Any(b => b.MaGSDay == maGS));
        }
        return View(new HocVienDetailsVM
        {
            HocVien = hv,
            LaQuanLy = _nguoiDung.LaQuanLy,
            LopHocs = lopTruyCap
                .OrderByDescending(l => l.ThoiGianBatDau)
                .ToList()
        });
    }

    [Authorize(Roles = Constants.VaiTro.QuanLy)]
    [HttpGet]
    public IActionResult Create() => View("Form", new HoSoHocVienVM());

    [Authorize(Roles = Constants.VaiTro.QuanLy)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(HoSoHocVienVM vm)
    {
        if (!ModelState.IsValid) return View("Form", vm);
        try
        {
            var hv = _hocVien.LuuHoSo(null, vm.SangHoSo());
            TempData["ThongBao"] = $"Đã thêm học viên {hv.HoTen}.";
            return RedirectToAction(nameof(Details), new { id = hv.MaHV });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View("Form", vm);
    }

    [Authorize(Roles = Constants.VaiTro.QuanLy)]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var hv = _hocVien.GetById(id);
        if (hv == null) return NotFound();
        return View("Form", HoSoHocVienVM.TuHocVien(hv));
    }

    [Authorize(Roles = Constants.VaiTro.QuanLy)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, HoSoHocVienVM vm)
    {
        vm.MaHV = id;
        if (!ModelState.IsValid) return View("Form", vm);
        try
        {
            var hv = _hocVien.LuuHoSo(id, vm.SangHoSo());
            TempData["ThongBao"] = $"Đã lưu hồ sơ {hv.HoTen}.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View("Form", vm);
    }

    [Authorize(Roles = Constants.VaiTro.QuanLy)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        try
        {
            _hocVien.Delete(id);
            TempData["ThongBao"] = "Đã xóa học viên.";
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            TempData["Loi"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
