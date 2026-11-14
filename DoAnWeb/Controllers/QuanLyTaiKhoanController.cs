using DoAnWeb.Common;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.QuanLyTaiKhoan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;

[Authorize(Roles = Constants.VaiTro.QuanLy)]
public class QuanLyTaiKhoanController : Controller
{
    private readonly TaiKhoanService _taiKhoan;

    public QuanLyTaiKhoanController(TaiKhoanService taiKhoan)
    {
        _taiKhoan = taiKhoan;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(TaoIndexVM(new TaoTaiKhoanVM()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(QuanLyTaiKhoanIndexVM vm)
    {
        var taoMoi = vm.TaoMoi ?? new TaoTaiKhoanVM();
        if (!ModelState.IsValid) return View(TaoIndexVM(taoMoi));
        try
        {
            _taiKhoan.TaoTaiKhoan(taoMoi.TenDangNhap, taoMoi.MatKhau, taoMoi.VaiTro, taoMoi.MaGS);
            TempData["ThongBao"] = $"Đã tạo tài khoản {taoMoi.TenDangNhap.Trim().ToLowerInvariant()}.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View(TaoIndexVM(taoMoi));
    }

    [HttpGet]
    public IActionResult DatLaiMatKhau(int id)
    {
        var tk = _taiKhoan.GetAll().FirstOrDefault(t => t.MaTK == id);
        if (tk == null) return NotFound();
        return View(new DatLaiMatKhauVM { MaTK = tk.MaTK, TenDangNhap = tk.TenDangNhap });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DatLaiMatKhau(DatLaiMatKhauVM vm)
    {
        vm.TenDangNhap = _taiKhoan.GetAll().FirstOrDefault(t => t.MaTK == vm.MaTK)?.TenDangNhap;
        if (vm.TenDangNhap == null) return NotFound();
        if (!ModelState.IsValid) return View(vm);
        try
        {
            _taiKhoan.DatLaiMatKhau(vm.MaTK, vm.MatKhauMoi);
            TempData["ThongBao"] = $"Đã đặt lại mật khẩu cho {vm.TenDangNhap}.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DoiTrangThai(int id)
    {
        try
        {
            bool hoatDong = _taiKhoan.DoiTrangThai(id);
            TempData["ThongBao"] = hoatDong ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            TempData["Loi"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private QuanLyTaiKhoanIndexVM TaoIndexVM(TaoTaiKhoanVM taoMoi) => new()
    {
        TaiKhoans = _taiKhoan.GetAll(),
        GiaSuChuaCoTaiKhoan = _taiKhoan.GetGiaSuChuaCoTaiKhoan(),
        TaoMoi = taoMoi
    };
}
