using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.GiaSu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Controllers;

[Authorize(Roles = Constants.VaiTro.QuanLy)]
public class GiaSuController : Controller
{
    private const string DangCoLop = "Đang có lớp";
    private readonly GiaSuService _giaSu;
    private readonly QuanLyGiaSuContext _db;

    public GiaSuController(GiaSuService giaSu, QuanLyGiaSuContext db)
    {
        _giaSu = giaSu;
        _db = db;
    }

    [HttpGet]
    public IActionResult Index(string tuKhoa, string mon, string ca, string trangThai)
    {
        var ds = _giaSu.GetDanhSach(tuKhoa);
        var danhSachMon = _db.MonHocs.AsNoTracking().Where(m => !m.NgungDay).Select(m => m.TenMon).ToList()
            .Concat(ds.Select(g => g.GiaSu.ChuyenMon))
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => m)
            .ToList();

        if (!string.IsNullOrWhiteSpace(mon))
            ds = ds.Where(g => g.GiaSu.ChuyenMon == mon).ToList();
        if (!string.IsNullOrWhiteSpace(ca))
            ds = ds.Where(g => g.GiaSu.NhanCa(ca)).ToList();
        if (!string.IsNullOrWhiteSpace(trangThai))
            ds = ds.Where(g => trangThai == DangCoLop
                ? g.GiaSu.TrangThai == Constants.GiaSuStatus.Ban || g.GiaSu.TrangThai == Constants.GiaSuStatus.DangDay
                : (g.GiaSu.TrangThai ?? Constants.GiaSuStatus.Ranh) == trangThai).ToList();

        return View(new GiaSuIndexVM
        {
            TuKhoa = tuKhoa,
            Mon = mon,
            Ca = ca,
            TrangThai = trangThai,
            TongSo = ds.Count,
            DanhSachMon = danhSachMon,
            GiaSus = ds.Select(g => new GiaSuListItemVM { GiaSu = g.GiaSu, SoLop = g.SoLop }).ToList()
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        var vm = new HoSoGiaSuVM();
        NapDanhSachMon(vm);
        return View("Form", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(HoSoGiaSuVM vm)
    {
        NapDanhSachMon(vm);
        if (!ModelState.IsValid) return View("Form", vm);
        try
        {
            var gs = _giaSu.LuuHoSo(null, vm.SangHoSo());
            TempData["ThongBao"] = $"Đã thêm gia sư {gs.HoTen}.";
            return RedirectToAction(nameof(Details), new { id = gs.MaGS });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View("Form", vm);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var gs = _giaSu.GetById(id);
        if (gs == null) return NotFound();
        var vm = HoSoGiaSuVM.TuGiaSu(gs);
        NapDanhSachMon(vm);
        if (!vm.DanhSachMon.Contains(gs.ChuyenMon, StringComparer.OrdinalIgnoreCase))
            vm.DanhSachMon.Add(gs.ChuyenMon);
        return View("Form", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, HoSoGiaSuVM vm)
    {
        vm.MaGS = id;
        NapDanhSachMon(vm);
        if (!ModelState.IsValid) return View("Form", vm);
        try
        {
            var gs = _giaSu.LuuHoSo(id, vm.SangHoSo());
            TempData["ThongBao"] = $"Đã lưu hồ sơ {gs.HoTen}.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        if (!vm.DanhSachMon.Contains(vm.ChuyenMon, StringComparer.OrdinalIgnoreCase))
            vm.DanhSachMon.Add(vm.ChuyenMon);
        return View("Form", vm);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var gs = _giaSu.GetById(id);
        if (gs == null) return NotFound();
        ViewBag.LopHocs = _db.LopHocs.AsNoTracking()
            .Include(l => l.HocVien)
            .Where(l => l.MaGS == id)
            .OrderByDescending(l => l.ThoiGianBatDau)
            .ToList();
        return View(gs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        try
        {
            _giaSu.Delete(id);
            TempData["ThongBao"] = "Đã xóa gia sư.";
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            TempData["Loi"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private void NapDanhSachMon(HoSoGiaSuVM vm)
    {
        vm.DanhSachMon = _db.MonHocs.AsNoTracking()
            .Where(m => !m.NgungDay)
            .OrderBy(m => m.TenMon)
            .Select(m => m.TenMon)
            .ToList();
        if (!string.IsNullOrWhiteSpace(vm.ChuyenMon) &&
            !vm.DanhSachMon.Contains(vm.ChuyenMon, StringComparer.OrdinalIgnoreCase))
            vm.DanhSachMon.Add(vm.ChuyenMon);
    }
}
