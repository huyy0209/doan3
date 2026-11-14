using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Media;
using DoAnWD.Common;
using DoAnWD.DAL.Repositories;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    /// <summary>
    /// ✨ Notification Service - Thông báo lớp sắp bắt đầu
    /// </summary>
    public class NotificationService : IDisposable
    {
        // Danh sách các lớp đã thông báo (tránh spam) - sống suốt phiên làm việc.
        // Mỗi lần truy vấn dùng DbContext mới để luôn đọc dữ liệu mới nhất
        // (lớp vừa hủy / vừa thêm ở form khác).
        private readonly HashSet<int> _notifiedLopIds;

        public NotificationService()
        {
            _notifiedLopIds = new HashSet<int>();
        }

        /// <summary>
        /// Lấy lớp "Chưa bắt đầu" có giờ bắt đầu trong [now, now + hours], kèm gia sư + học viên
        /// </summary>
        private static List<LopHoc> QueryLopSapBatDau(int hours)
        {
            var now = DateTime.Now;
            var thresholdTime = now.AddHours(hours);

            using (var db = new GiaSuContextDB())
            {
                return db.LopHocs
                    .Include(l => l.GiaSu)
                    .Include(l => l.HocVien)
                    .Where(l => l.TrangThai == Constants.LopHocStatus.ChuaBatDau &&
                                l.ThoiGianBatDau >= now &&
                                l.ThoiGianBatDau <= thresholdTime)
                    .OrderBy(l => l.ThoiGianBatDau)
                    .ToList()
                    // Gia sư đăng nhập chỉ nhận thông báo lớp của mình
                    .Where(l => !PhienDangNhap.GioiHanGiaSu.HasValue || l.MaGS == PhienDangNhap.GioiHanGiaSu.Value)
                    .ToList();
            }
        }

        /// <summary>
        /// ✨ Kiểm tra và lấy danh sách lớp cần thông báo
        /// </summary>
        /// <param name="hoursBeforeStart">Số giờ trước khi bắt đầu (mặc định 24h)</param>
        public List<LopHoc> GetUpcomingClassesForNotification(int hoursBeforeStart = 24)
        {
            try
            {
                var upcomingClasses = QueryLopSapBatDau(hoursBeforeStart)
                    .Where(l => !_notifiedLopIds.Contains(l.MaLop)) // Chưa thông báo
                    .Take(5) // Tối đa 5 thông báo/lần
                    .ToList();

                // Đánh dấu đã thông báo
                foreach (var lop in upcomingClasses)
                {
                    _notifiedLopIds.Add(lop.MaLop);
                }

                return upcomingClasses;
            }
            catch (Exception ex)
            {
                // MED-05: ghi lại lỗi để sự cố truy vấn nền không biến mất ở bản Release.
                System.Diagnostics.Trace.WriteLine($"[NOTIFICATION] Lỗi khi lấy danh sách: {ex}");
                return new List<LopHoc>();
            }
        }

        /// <summary>
        /// ✨ Phát âm thanh thông báo
        /// </summary>
        public void PlayNotificationSound()
        {
            try
            {
                // Phát âm thanh hệ thống
                SystemSounds.Asterisk.Play();
            }
            catch (Exception ex)
            {
                // MED-05: lưu dấu lỗi phát thông báo mà không làm gián đoạn giao diện.
                System.Diagnostics.Trace.WriteLine($"[NOTIFICATION] Lỗi phát âm thanh: {ex}");
            }
        }

        /// <summary>
        /// ✨ Lấy số lượng lớp sắp bắt đầu trong N giờ
        /// </summary>
        public int GetUpcomingClassCount(int hours = 24)
        {
            try
            {
                return QueryLopSapBatDau(hours).Count;
            }
            catch (Exception ex)
            {
                // MED-05: giữ giá trị dự phòng nhưng ghi lại lỗi truy vấn.
                System.Diagnostics.Trace.WriteLine($"[NOTIFICATION] Lỗi đếm lớp sắp bắt đầu: {ex}");
                return 0;
            }
        }

        public void Dispose()
        {
            _notifiedLopIds.Clear();
        }
    }
}