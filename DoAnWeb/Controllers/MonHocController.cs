using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.MonHocVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers
{
    /// <summary>
    /// Danh mục môn học (dlgMonHoc.cs): thêm, sửa, xóa môn; môn năng khiếu có danh sách cấp độ.
    /// Môn đang có lớp không xóa được – dùng "Ngừng dạy". Chỉ quản lý.
    /// </summary>
    [Authorize(Roles = "QuanLy")]
    public class MonHocController : Controller
    {
        private readonly MonHocService _monHoc;

        public MonHocController(MonHocService monHoc)
        {
            _monHoc = monHoc;
        }

        [HttpGet]
        public IActionResult Index(int? maMon)
        {
            var vm = new MonHocVM
            {
                DanhSach = _monHoc.GetAll(),
                SoLopTheoMon = _monHoc.SoLopTheoMon()
            };

            var chon = maMon.HasValue && maMon.Value > 0
                ? vm.DanhSach.FirstOrDefault(m => m.MaMon == maMon.Value)
                : null;
            if (chon != null)
            {
                vm.MaMon = chon.MaMon;
                vm.TenMon = chon.TenMon;
                vm.LoaiMon = chon.LoaiMon;
                vm.CapDoText = string.Join(Environment.NewLine, chon.DanhSachCapDo);
                vm.NgungDay = chon.NgungDay;
                vm.SoLopCuaMon = vm.SoLop(chon);
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Luu(MonHocVM vm)
        {
            var capDo = (vm.CapDoText ?? "")
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => c.Length > 0)
                .ToList();

            try
            {
                var mon = _monHoc.Luu(vm.MaMon, vm.TenMon, vm.LoaiMon, capDo, vm.NgungDay);
                TempData["ThongBao"] = vm.MaMon.HasValue ? "Đã lưu môn học." : "Đã thêm môn học.";
                return RedirectToAction(nameof(Index), new { maMon = mon.MaMon });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                TempData["Loi"] = ex.Message;
                return RedirectToAction(nameof(Index), new { maMon = vm.MaMon });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Xoa(int maMon)
        {
            try
            {
                _monHoc.Xoa(maMon);
                TempData["ThongBao"] = Constants.Messages.XoaThanhCong;
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                TempData["Loi"] = ex.Message;
                return RedirectToAction(nameof(Index), new { maMon });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
