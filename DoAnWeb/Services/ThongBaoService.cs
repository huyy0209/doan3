using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services
{
    /// <summary>Thông báo lớp sắp bắt đầu; trạng thái đã hiển thị do trình duyệt quản lý.</summary>
    public class ThongBaoService
    {
        private readonly QuanLyGiaSuContext _db;
        private readonly INguoiDungHienTai _nguoiDung;

        public ThongBaoService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung)
        {
            _db = db;
            _nguoiDung = nguoiDung;
        }

        private List<LopHoc> QueryLopSapBatDau(int hours)
        {
            var now = DateTime.Now;
            var thresholdTime = now.AddHours(hours);
            var q = _db.LopHocs.AsNoTracking()
                .Include(l => l.GiaSu)
                .Include(l => l.HocVien)
                .Where(l => l.TrangThai == Constants.LopHocStatus.ChuaBatDau &&
                            l.ThoiGianBatDau >= now &&
                            l.ThoiGianBatDau <= thresholdTime);
            if (_nguoiDung.GioiHanGiaSu.HasValue)
                q = q.Where(l => l.MaGS == _nguoiDung.GioiHanGiaSu.Value);
            return q.OrderBy(l => l.ThoiGianBatDau).ToList();
        }

        // Nguồn: DoAnWD/BLL/Services/NotificationService.cs:40-66
        public List<LopHoc> GetUpcomingClassesForNotification(int hoursBeforeStart = 24)
        {
            return QueryLopSapBatDau(hoursBeforeStart).Take(5).ToList();
        }

        // Nguồn: DoAnWD/BLL/Services/NotificationService.cs:85-99
        public int GetUpcomingClassCount(int hours = 24)
        {
            return QueryLopSapBatDau(hours).Count;
        }
    }
}
