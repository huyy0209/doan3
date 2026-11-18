using DoAnWeb.Services.Excel;
using System;
using DoAnWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;


[Authorize(Roles = "QuanLy")]
public class BaoCaoController : Controller
{
    private const string LoaiExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly BaoCaoThangService _baoCao;
    private readonly XuatExcelService _excel;

    public BaoCaoController(BaoCaoThangService baoCao, XuatExcelService excel)
    {
        _baoCao = baoCao;
        _excel = excel;
    }

    [HttpGet]
    public IActionResult Index(int? thang, int? nam)
    {
        if (!HopLe(thang, nam)) return BadRequest();
        var macDinh = DateTime.Today.AddMonths(-1);
        return View(_baoCao.Lay(thang ?? macDinh.Month, nam ?? macDinh.Year));
    }

    [HttpGet]
    public IActionResult XuatExcel(int? thang, int? nam)
    {
        if (!HopLe(thang, nam)) return BadRequest();
        var macDinh = DateTime.Today.AddMonths(-1);
        var model = _baoCao.Lay(thang ?? macDinh.Month, nam ?? macDinh.Year);
        return File(_excel.BaoCaoThang(model), LoaiExcel, $"BaoCao_{model.Nam}{model.Thang:D2}.xlsx");
    }

    private static bool HopLe(int? thang, int? nam)
    {
        return (!thang.HasValue || thang.Value >= 1 && thang.Value <= 12) &&
            (!nam.HasValue || nam.Value >= 1900 && nam.Value <= DateTime.Today.Year + 1);
    }
}