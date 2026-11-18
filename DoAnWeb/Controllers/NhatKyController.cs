using DoAnWeb.Services.Excel;
using DoAnWeb.Services.NhatKy;
using System;
using DoAnWeb.Services;
using DoAnWeb.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;

[Authorize(Roles = "QuanLy")]
public sealed class NhatKyController : Controller
{
    private readonly NhatKyService _nhatKy;
    private readonly NhatKyExcelService _excel;

    public NhatKyController(NhatKyService nhatKy, NhatKyExcelService excel)
    {
        _nhatKy = nhatKy;
        _excel = excel;
    }

    [HttpGet]
    public IActionResult Index(DateTime? tuNgay, DateTime? denNgay, string nguoi, string hanhDong, string bang,
        int trang = 1, int soDong = PhanTrang.SoDongMacDinh) =>
        View(_nhatKy.TaoTrang(tuNgay, denNgay, nguoi, hanhDong, bang, trang, soDong));

    [HttpGet]
    public IActionResult ChiTiet(int id)
    {
        var vm = _nhatKy.ChiTiet(id);
        return vm == null ? NotFound() : View(vm);
    }

    [HttpGet]
    public IActionResult XuatExcel(DateTime? tuNgay, DateTime? denNgay, string nguoi, string hanhDong, string bang)
    {
        var file = _excel.Tao(_nhatKy.Xuat(tuNgay, denNgay, nguoi, hanhDong, bang));
        return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"NhatKy_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}
