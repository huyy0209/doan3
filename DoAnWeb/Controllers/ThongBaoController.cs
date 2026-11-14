using System.Linq;
using DoAnWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers
{
    [Authorize(Roles = "QuanLy,GiaSu")]
    public class ThongBaoController : Controller
    {
        private readonly ThongBaoService _thongBao;

        public ThongBaoController(ThongBaoService thongBao)
        {
            _thongBao = thongBao;
        }

        [HttpGet]
        public IActionResult SapBatDau()
        {
            var classes = _thongBao.GetUpcomingClassesForNotification(24)
                .Select(l => new
                {
                    id = l.MaLop,
                    tenLop = l.TenHienThi,
                    monHoc = l.MonHoc,
                    hocVien = l.HocVien?.HoTen,
                    giaSu = l.GiaSu?.HoTen,
                    batDau = l.ThoiGianBatDau
                })
                .ToList();
            return Json(new { count = _thongBao.GetUpcomingClassCount(24), classes });
        }
    }
}
