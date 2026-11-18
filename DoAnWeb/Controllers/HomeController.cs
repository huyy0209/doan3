using System.Diagnostics;
using DoAnWeb.ViewModels;
using DoAnWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;


public class HomeController : Controller
{
    private readonly INguoiDungHienTai _nguoiDung;
    public HomeController(INguoiDungHienTai nguoiDung) { _nguoiDung = nguoiDung; }

    /// <summary>Màn hình đầu: quản lý vào Tổng quan, gia sư vào thẳng Điểm danh (TrangChu.cs:375-381)</summary>
    public IActionResult Index() =>
        _nguoiDung.LaQuanLy ? RedirectToAction("Index", "TongQuan") : RedirectToAction("Index", "DiemDanh");

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}