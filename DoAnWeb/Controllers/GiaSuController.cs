using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Infrastructure;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.GiaSuVM;
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
    public IActionResult Index(string tuKhoa, string mon, string ca, string trangThai,
        int trang = 1, int soDong = PhanTrang.SoDongMacDinh, string sapXep = null, string chieu = null)
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

        IEnumerable<GiaSuDong> danhSach = ds;
        bool giamDan = chieu == "desc";
        danhSach = sapXep switch
        {
            "hoTen" => (giamDan ? danhSach.OrderByDescending(g => g.GiaSu.HoTen) : danhSach.OrderBy(g => g.GiaSu.HoTen)).ThenBy(g => g.GiaSu.MaGS),
            "sdt" => (giamDan ? danhSach.OrderByDescending(g => g.GiaSu.SDT) : danhSach.OrderBy(g => g.GiaSu.SDT)).ThenBy(g => g.GiaSu.MaGS),
            "chuyenMon" => (giamDan ? danhSach.OrderByDescending(g => g.GiaSu.ChuyenMon) : danhSach.OrderBy(g => g.GiaSu.ChuyenMon)).ThenBy(g => g.GiaSu.MaGS),
            "ca" => (giamDan ? danhSach.OrderByDescending(g => g.GiaSu.CaNhanDay) : danhSach.OrderBy(g => g.GiaSu.CaNhanDay)).ThenBy(g => g.GiaSu.MaGS),
            "soLop" => (giamDan ? danhSach.OrderByDescending(g => g.SoLop) : danhSach.OrderBy(g => g.SoLop)).ThenBy(g => g.GiaSu.MaGS),
            "trangThai" => (giamDan ? danhSach.OrderByDescending(g => g.GiaSu.TrangThai) : danhSach.OrderBy(g => g.GiaSu.TrangThai)).ThenBy(g => g.GiaSu.MaGS),
            _ => danhSach
        };
        var ketQuaTrang = PhanTrang.Tao(danhSach, trang, soDong, sapXep, chieu,
            new Dictionary<string, string>
            {
                ["tuKhoa"] = tuKhoa ?? "",
                ["mon"] = mon ?? "",
                ["ca"] = ca ?? "",
                ["trangThai"] = trangThai ?? ""
            });
        ViewBag.PhanTrang = ketQuaTrang;
        return View(new GiaSuIndexVM
        {
            TuKhoa = tuKhoa,
            Mon = mon,
            Ca = ca,
            TrangThai = trangThai,
            TongSo = ketQuaTrang.TongSo,
            DanhSachMon = danhSachMon,
            GiaSus = ketQuaTrang.Items.Select(g => new GiaSuListItemVM { GiaSu = g.GiaSu, SoLop = g.SoLop }).ToList()
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
