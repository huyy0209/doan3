using DoAnWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.ViewComponents;

public sealed class ThongBaoViewComponent : ViewComponent
{
    private readonly ThongBaoService _thongBao;
    private readonly INguoiDungHienTai _nguoiDung;

    public ThongBaoViewComponent(ThongBaoService thongBao, INguoiDungHienTai nguoiDung)
    {
        _thongBao = thongBao;
        _nguoiDung = nguoiDung;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!_nguoiDung.DaDangNhap)
            return Content(string.Empty);
        ThongBaoMenuViewModel model = await _thongBao.LayMenuAsync(_nguoiDung.MaTK);
        return View(model);
    }
}
