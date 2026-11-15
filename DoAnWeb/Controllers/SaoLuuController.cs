using DoAnWeb.Services;
using DoAnWeb.ViewModels.SaoLuuVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;

[Authorize(Roles = "QuanLy")]
public class SaoLuuController : Controller
{
    private readonly SaoLuuService _saoLuu;
    private readonly IConfiguration _cauHinh;
    private readonly ILogger<SaoLuuController> _logger;

    public SaoLuuController(SaoLuuService saoLuu, IConfiguration cauHinh, ILogger<SaoLuuController> logger)
    {
        _saoLuu = saoLuu;
        _cauHinh = cauHinh;
        _logger = logger;
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
