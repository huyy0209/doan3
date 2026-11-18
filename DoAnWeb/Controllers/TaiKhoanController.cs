using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.TaiKhoan;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DoAnWeb.Controllers;


/// <summary>Đăng nhập / đăng xuất / đổi mật khẩu của chính mình (frmDangNhap, frmDoiMatKhau)</summary>
public class TaiKhoanController : Controller
{
    private readonly TaiKhoanService _taiKhoan;
    private readonly INguoiDungHienTai _nguoiDung;
    private readonly NhatKyService _nhatKy;

    public TaiKhoanController(TaiKhoanService taiKhoan, INguoiDungHienTai nguoiDung, NhatKyService nhatKy)
    {
        _taiKhoan = taiKhoan;
        _nguoiDung = nguoiDung;
        _nhatKy = nhatKy;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult DangNhap(string returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        // TK-10: tạo mặc định lần đầu hoặc gợi ý nếu admin duy nhất chưa đổi mật khẩu
        bool lanDau = false;
        try
        {
            lanDau = _taiKhoan.DamBaoTaiKhoanMacDinh() || _taiKhoan.CoTaiKhoanMacDinh();
        }
        catch { /* CSDL lỗi: báo khi bấm đăng nhập */ }
        return View(new DangNhapVM { ReturnUrl = returnUrl, LanDau = lanDau });
    }

    [AllowAnonymous]
    [HttpPost]
    [EnableRateLimiting("gioi-han-dang-nhap")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapVM vm)
    {
        try
        {
            var tk = _taiKhoan.DangNhap(vm.TenDangNhap, vm.MatKhau);
            // TK-11: dùng mật khẩu mặc định ai cũng biết => bắt đặt mật khẩu riêng
            bool phaiDoi = vm.MatKhau == TaiKhoanService.MatKhauMacDinh;
            await _nhatKy.GhiSuKienAsync("DangNhap", tk.TenDangNhap, tk.MaTK);

            var phatHanhUtc = DateTimeOffset.UtcNow;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, tk.TenDangNhap),
                new Claim(ClaimTypes.Role, tk.VaiTro),
                new Claim(LoaiClaim.MaTK, tk.MaTK.ToString()),
                new Claim(LoaiClaim.PhaiDoiMatKhau, phaiDoi ? "1" : "0"),
                new Claim(LoaiClaim.PhatHanhPhienUtc, phatHanhUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture))
            };
            if (tk.MaGS.HasValue) claims.Add(new Claim(LoaiClaim.MaGS, tk.MaGS.Value.ToString()));
            if (tk.GiaSu?.HoTen != null) claims.Add(new Claim(LoaiClaim.TenGiaSu, tk.GiaSu.HoTen));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties { IssuedUtc = phatHanhUtc });

            if (phaiDoi) return RedirectToAction(nameof(DoiMatKhau));
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl)) return LocalRedirect(vm.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is ArgumentException)
        {
            await _nhatKy.GhiSuKienAsync("DangNhapLoi", vm.TenDangNhap);
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Không kết nối được cơ sở dữ liệu. Kiểm tra SQL Server.");
        }
        vm.MatKhau = null;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangXuat()
    {
        await _nhatKy.GhiSuKienAsync("DangXuat", _nguoiDung.TenDangNhap, _nguoiDung.MaTK);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(DangNhap));
    }

    [HttpGet]
    public IActionResult DoiMatKhau() =>
        View(new DoiMatKhauVM { BatBuoc = User.FindFirst(LoaiClaim.PhaiDoiMatKhau)?.Value == "1" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauVM vm)
    {
        vm.BatBuoc = User.FindFirst(LoaiClaim.PhaiDoiMatKhau)?.Value == "1";
        // Bắt buộc đổi (vừa đăng nhập bằng mật khẩu mặc định): form ẩn ô mật khẩu cũ, mật khẩu cũ chính là mặc định
        // (giống frmDangNhap.cs:126 truyền sẵn matKhauCu)
        if (vm.BatBuoc)
        {
            vm.MatKhauCu = TaiKhoanService.MatKhauMacDinh;
            ModelState.Remove(nameof(vm.MatKhauCu));
        }
        if (!ModelState.IsValid) return View(vm);
        try
        {
            _taiKhoan.DoiMatKhau(_nguoiDung.MaTK, vm.MatKhauCu, vm.MatKhauMoi);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is UnauthorizedAccessException || ex is InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }

        if (vm.BatBuoc)
        {
            // Bỏ cờ bắt đổi mật khẩu: cấp lại cookie với claim mới
            var id = new ClaimsIdentity(User.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var cu = id.FindFirst(LoaiClaim.PhaiDoiMatKhau);
            if (cu != null) id.RemoveClaim(cu);
            id.AddClaim(new Claim(LoaiClaim.PhaiDoiMatKhau, "0"));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        TempData["ThongBao"] = "Đã đổi mật khẩu.";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult KhongCoQuyen() => View();
}