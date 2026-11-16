using DoAnWeb.ViewModels.SaoLuuVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using DoAnWeb.Data;
using DoAnWeb.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authentication;
using DoAnWeb.Data.Entities;

namespace DoAnWeb.Controllers;

[Authorize(Roles = "QuanLy")]
public class SaoLuuController : Controller
{
    private readonly SaoLuuService _saoLuu;
    private readonly IConfiguration _cauHinh;
    private readonly ILogger<SaoLuuController> _logger;
    private readonly QuanLyGiaSuContext _db;
    private readonly IMemoryCache _cache;
    private readonly INguoiDungHienTai _nguoiDung;

    public SaoLuuController(SaoLuuService saoLuu, IConfiguration cauHinh, ILogger<SaoLuuController> logger,
        QuanLyGiaSuContext db, IMemoryCache cache, INguoiDungHienTai nguoiDung)
    {
        _saoLuu = saoLuu;
        _cauHinh = cauHinh;
        _logger = logger;
        _db = db; _cache = cache; _nguoiDung = nguoiDung;
    }

    [HttpGet]
    public async Task<IActionResult> KhoiPhuc(string tenTep, CancellationToken cancellationToken)
    {
        try { SaoLuuService.KiemTraTenTepCongKhai(tenTep); }
        catch (ArgumentException) { return NotFound(); }
        var tep = (await _saoLuu.LayDanhSachAsync(cancellationToken)).FirstOrDefault(t => t.Ten == tenTep);
        if (tep == null) return NotFound();
        return View(new KhoiPhucSaoLuuVM { TenTep = tenTep });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("gioi-han-dang-nhap")]
    public async Task<IActionResult> KhoiPhuc(KhoiPhucSaoLuuVM vm, CancellationToken cancellationToken)
    {
        if (!string.Equals(vm.TenTep, vm.XacNhanTenTep, StringComparison.Ordinal))
            ModelState.AddModelError(nameof(vm.XacNhanTenTep), "Tên tệp xác nhận chưa chính xác.");
        TaiKhoan tk = await _db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(t => t.MaTK == _nguoiDung.MaTK, cancellationToken);
        if (tk == null || !MatKhau.KiemTra(vm.MatKhauHienTai, tk.MatKhauHash))
            ModelState.AddModelError(nameof(vm.MatKhauHienTai), "Mật khẩu hiện tại không chính xác.");
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var banTruoc = await _saoLuu.KhoiPhucAsync(vm.TenTep, cancellationToken);
            if (_cache is MemoryCache memoryCache) memoryCache.Compact(1.0);
            DoAnWeb.Infrastructure.KiemTraPhienDangNhap.XoaCache(_cache, _nguoiDung.MaTK);
            await HttpContext.SignOutAsync();
            try
            {
                await _db.Database.ExecuteSqlRawAsync(
                    "INSERT INTO dbo.NhatKy (ThoiGian, MaTK, TenDangNhap, HanhDong, ChiTiet) VALUES ({0}, {1}, {2}, {3}, {4})",
                    new object[] { DateTime.Now, _nguoiDung.MaTK, tk.TenDangNhap, "KhoiPhuc", $"Tệp {vm.TenTep}; bản trước {banTruoc}" }, cancellationToken);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Không ghi được nhật ký khôi phục {TenTep}.", vm.TenTep); }
            TempData["ThongBao"] = $"Đã khôi phục CSDL thành công. Bản trước khi khôi phục: {banTruoc}. Vui lòng đăng nhập lại.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException) { return NotFound(); }
        catch (SaoLuuKhongTuongThichException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khôi phục CSDL từ {TenTep} thất bại.", vm.TenTep);
            ModelState.AddModelError(string.Empty, "Không thể khôi phục CSDL. Hãy kiểm tra nhật ký hệ thống và quyền truy cập SQL Server.");
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new SaoLuuIndexVM
        {
            GioTuDong = _cauHinh["SaoLuu:GioTuDong"] ?? "23:00"
        };
        try
        {
            model.Tep = await _saoLuu.LayDanhSachAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể đọc danh sách file sao lưu.");
            model.Loi = "Không thể đọc danh sách file sao lưu. Kiểm tra cấu hình thư mục và kết nối SQL Server.";
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaoLuuNgay(CancellationToken cancellationToken)
    {
        try
        {
            var tenTep = await _saoLuu.SaoLuuAsync(cancellationToken);
            TempData["ThongBao"] = $"Đã sao lưu CSDL thành công: {tenTep}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sao lưu CSDL theo yêu cầu thất bại.");
            TempData["Loi"] = "Không thể sao lưu CSDL. Kiểm tra quyền ghi của SQL Server vào thư mục sao lưu và nhật ký hệ thống.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Tai(string tenTep, CancellationToken cancellationToken)
    {
        try
        {
            var tep = await _saoLuu.MoTepAsync(tenTep, cancellationToken);
            return File(tep, "application/octet-stream", Path.GetFileName(tep.Name));
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(string tenTep, CancellationToken cancellationToken)
    {
        try
        {
            await _saoLuu.XoaTepAsync(tenTep, cancellationToken);
            TempData["ThongBao"] = "Đã xóa file sao lưu.";
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể xóa file sao lưu {TenTep}.", tenTep);
            TempData["Loi"] = "Không thể xóa file sao lưu. Kiểm tra quyền truy cập thư mục.";
        }

        return RedirectToAction(nameof(Index));
    }
}
