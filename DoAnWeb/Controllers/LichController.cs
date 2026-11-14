using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.Lich;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers
{
    [Authorize(Roles = "QuanLy,GiaSu")]
    public class LichController : Controller
    {
        private readonly LopHocService _lopHoc;
        private readonly GiaSuService _giaSu;
        private readonly INguoiDungHienTai _nguoiDung;

        public LichController(LopHocService lopHoc, GiaSuService giaSu, INguoiDungHienTai nguoiDung)
        {
            _lopHoc = lopHoc;
            _giaSu = giaSu;
            _nguoiDung = nguoiDung;
        }

        [HttpGet]
        public IActionResult Index(int? nam, int? thang, int? maGS, DateTime? ngay)
        {
            int year = nam.GetValueOrDefault(DateTime.Today.Year);
            int month = thang.GetValueOrDefault(DateTime.Today.Month);
            if (year < 1 || year > 9998) year = DateTime.Today.Year;
            if (month < 1 || month > 12) month = DateTime.Today.Month;
            var first = new DateTime(year, month, 1);
            var last = first.AddMonths(1).AddTicks(-1);
            var tutors = _nguoiDung.LaQuanLy
                ? _giaSu.GetAll()
                : new List<DoAnWeb.Data.Entities.GiaSu>();
            int? selectedTutor = _nguoiDung.LaQuanLy ? maGS : _nguoiDung.GioiHanGiaSu;
            if (_nguoiDung.LaQuanLy && selectedTutor.HasValue && !tutors.Any(g => g.MaGS == selectedTutor.Value))
                selectedTutor = null;

            var classes = !_nguoiDung.LaQuanLy && !selectedTutor.HasValue
                ? new List<LopHoc>()
                : _lopHoc.GetLopTrongKhoang(first, last, selectedTutor);
            var byDate = new Dictionary<DateTime, List<LopHoc>>();
            for (var date = first; date <= last; date = date.AddDays(1))
            {
                var daily = classes.Where(l => l.HocNgay(date))
                    .OrderBy(l => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, l.CaHoc))
                    .ThenBy(l => l.TenHienThi)
                    .ToList();
                if (daily.Count > 0) byDate[date.Date] = daily;
            }

            DateTime? selectedDate = ngay?.Date;
            if (selectedDate.HasValue && (selectedDate.Value.Year != year || selectedDate.Value.Month != month))
                selectedDate = null;

            var cells = new List<LichNgayViewModel>();
            int offset = ((int)first.DayOfWeek + 6) % 7;
            for (int i = 0; i < offset; i++) cells.Add(new LichNgayViewModel());
            for (var date = first; date <= last; date = date.AddDays(1))
                cells.Add(new LichNgayViewModel
                {
                    Ngay = date.Date,
                    LopHoc = byDate.TryGetValue(date.Date, out var daily) ? daily : new List<LopHoc>()
                });
            while (cells.Count % 7 != 0) cells.Add(new LichNgayViewModel());

            return View(new LichIndexViewModel
            {
                Thang = first,
                MaGS = selectedTutor,
                NgayChon = selectedDate,
                GiaSus = tutors,
                CacNgay = cells,
                SoHang = cells.Count / 7,
                LopNgayChon = selectedDate.HasValue && byDate.TryGetValue(selectedDate.Value, out var selectedClasses)
                    ? selectedClasses : new List<LopHoc>()
            });
        }
    }
}
