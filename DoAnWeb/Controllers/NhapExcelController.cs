using DoAnWeb.Services.Excel;
using DoAnWeb.Common;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.NhapExcel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;

[Authorize(Roles = Constants.VaiTro.QuanLy)]
public sealed class NhapExcelController : Controller
{
    private readonly NhapExcelService _service;
    private readonly INguoiDungHienTai _nguoiDung;

    public NhapExcelController(NhapExcelService service, INguoiDungHienTai nguoiDung)
    {
        _service = service;
        _nguoiDung = nguoiDung;
    }

    [HttpGet]
    public IActionResult Index() => View(new NhapExcelVM());

    [HttpGet]
    public IActionResult TaiMau(string loai = "HocVien")
    {
        try
        {
            byte[] noiDung = _service.TaoMau(loai);
            return File(noiDung, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Mau-{loai}.xlsx");
        }
        catch (ArgumentException) { return BadRequest("Loại hồ sơ không hợp lệ."); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public IActionResult XemTruoc(string loaiHoSo, IFormFile tep)
    {
        var vm = new NhapExcelVM { LoaiHoSo = loaiHoSo };
        if (tep == null || tep.Length == 0)
            ModelState.AddModelError(nameof(tep), "Chọn tệp .xlsx cần tải lên.");
        else if (!string.Equals(Path.GetExtension(tep.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(tep), "Chỉ chấp nhận tệp .xlsx.");
        else if (tep.Length > NhapExcelService.GioiHanByte)
            ModelState.AddModelError(nameof(tep), "Tệp không được lớn hơn 2 MB.");
        if (!ModelState.IsValid) return View("Index", vm);

        try
        {
            using var stream = tep.OpenReadStream();
            vm = _service.XemTruoc(loaiHoSo, stream, _nguoiDung.TenDangNhap);
            return View("Index", vm);
        }
        catch (InvalidDataException ex) { ModelState.AddModelError(nameof(tep), ex.Message); }
        catch (ArgumentException ex) { ModelState.AddModelError(nameof(tep), ex.Message); }
        catch (Exception ex) when ((ex.GetType().Namespace?.StartsWith("ClosedXML", StringComparison.Ordinal) ?? false) ||
                                   ex is System.IO.IOException || ex is System.FormatException)
        {
            ModelState.AddModelError(nameof(tep), "Không đọc được tệp Excel. Hãy dùng tệp mẫu .xlsx.");
        }
        return View("Index", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Nhap(string maPhien)
    {
        try
        {
            var (soNhap, soBoQua) = _service.NhapCacDongHopLe(maPhien, _nguoiDung.TenDangNhap);
            TempData["ThongBao"] = $"Đã nhập {soNhap} dòng hợp lệ; bỏ qua {soBoQua} dòng không còn hợp lệ.";
        }
        catch (InvalidOperationException ex) { TempData["Loi"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
