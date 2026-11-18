using System;
using DoAnWeb.Common;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.CaiDat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;


[Authorize(Roles = "QuanLy")]
public class CaiDatController : Controller
{
    private readonly CaiDatService _caiDat;

    public CaiDatController(CaiDatService caiDat)
    {
        _caiDat = caiDat;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var trungTam = _caiDat.TrungTam;
        return View(new CaiDatIndexViewModel
        {
            Ten = trungTam.Ten,
            DiaChi = trungTam.DiaChi,
            SDT = trungTam.SDT
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(CaiDatIndexViewModel model)
    {
        try
        {
            string ten = string.IsNullOrWhiteSpace(model.Ten) ? CaiDatService.TenMacDinh : model.Ten;
            _caiDat.LuuTrungTam(ten, model.DiaChi, model.SDT);
            TempData["ThongBao"] = "Đã lưu thông tin trung tâm.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}