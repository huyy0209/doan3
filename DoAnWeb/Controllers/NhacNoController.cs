using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Infrastructure;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.NhacNo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;


/// <summary>
/// Nhắc nợ học phí (chỉ quản lý): danh sách lớp còn nợ, lọc theo mức nợ/số ngày/môn,
/// sắp xếp, phân trang, xuất Excel và sao chép tin nhắn nhắc nợ.
/// Số liệu lấy từ <see cref="HocPhiService.GetCongNo"/> nên luôn khớp trang Học phí.
/// </summary>
[Authorize(Roles = "QuanLy")]
public class NhacNoController : Controller
{
    private readonly NhacNoService _nhacNo;
    private readonly XuatExcelService _excel;

    public NhacNoController(NhacNoService nhacNo, XuatExcelService excel)
    {
        _nhacNo = nhacNo;
        _excel = excel;
    }

    [HttpGet]
    public IActionResult Index(decimal? conNoToiThieu, int? quaNgay, string mon,
        int trang = 1, int soDong = PhanTrang.SoDongMacDinh, string sapXep = null, string chieu = null)
    {
        return View(TaoModel(conNoToiThieu, quaNgay, mon, trang, soDong, sapXep, chieu));
    }

    /// <summary>
    /// Xuất Excel danh sách đang lọc: dùng hàm riêng <see cref="XuatExcelService.NhacNo"/>
    /// để có thêm cột "Lần đóng gần nhất" và "Số ngày chưa đóng".
    /// </summary>
    [HttpGet]
    public IActionResult XuatExcel(decimal? conNoToiThieu, int? quaNgay, string mon)
    {
        var ds = LocTheoBoLoc(conNoToiThieu, quaNgay, mon);
        return File(_excel.NhacNo(ds),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"NhacNo_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    private List<NhacNoDong> LocTheoBoLoc(decimal? conNoToiThieu, int? quaNgay, string mon)
    {
        return NhacNoService.Loc(_nhacNo.GetDanhSach(), conNoToiThieu, quaNgay, mon);
    }

    private NhacNoIndexViewModel TaoModel(decimal? conNoToiThieu, int? quaNgay, string mon,
        int trang, int soDong, string sapXep, string chieu)
    {
        var all = _nhacNo.GetDanhSach();
        var ds = NhacNoService.Loc(all, conNoToiThieu, quaNgay, mon);

        string sapXepChon = sapXep == NhacNoService.SapXepSoNgay ? NhacNoService.SapXepSoNgay : NhacNoService.SapXepConNo;
        bool giamDan = chieu != "asc";
        var daSapXep = NhacNoService.SapXep(ds, sapXepChon, giamDan);

        var ketQuaTrang = PhanTrang.Tao(daSapXep, trang, soDong, sapXepChon, giamDan ? "desc" : "asc",
            new Dictionary<string, string>
            {
                ["conNoToiThieu"] = conNoToiThieu?.ToString() ?? "",
                ["quaNgay"] = quaNgay?.ToString() ?? "",
                ["mon"] = mon ?? ""
            });
        ViewBag.PhanTrang = ketQuaTrang;

        return new NhacNoIndexViewModel
        {
            DanhSach = ketQuaTrang.Items,
            ConNoToiThieu = conNoToiThieu,
            QuaNgay = quaNgay,
            Mon = mon,
            DanhSachMon = NhacNoService.DanhSachMon(all),
            TongConNo = ds.Sum(c => c.ConNo),
            TongSoLop = ds.Count
        };
    }
}