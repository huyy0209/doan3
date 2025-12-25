using System;
using System.Collections.Generic;
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
        private readonly GiaSuContextDB _context;
        private readonly LopHocRepository _lopHocRepo;

        // Danh sách các lớp đã thông báo (tránh spam)
        private HashSet<int> _notifiedLopIds;

        public NotificationService()
        {
            _context = new GiaSuContextDB();
            _lopHocRepo = new LopHocRepository(_context);
            _notifiedLopIds = new HashSet<int>();
        }

        /// <summary>
        /// ✨ Kiểm tra và lấy danh sách lớp cần thông báo
        /// </summary>
        /// <param name="hoursBeforeStart">Số giờ trước khi bắt đầu (mặc định 24h)</param>
        public List<LopHoc> GetUpcomingClassesForNotification(int hoursBeforeStart = 24)
        {
            try
            {
                var now = DateTime.Now;
                var thresholdTime = now.AddHours(hoursBeforeStart);

                var upcomingClasses = _lopHocRepo.GetAll()
                    .Where(l => l.ThoiGianBatDau.HasValue &&
                                l.ThoiGianBatDau.Value >= now &&
                                l.ThoiGianBatDau.Value <= thresholdTime &&
                                l.TrangThai == Constants.LopHocStatus.ChuaBatDau &&
                                !_notifiedLopIds.Contains(l.MaLop)) // Chưa thông báo
                    .OrderBy(l => l.ThoiGianBatDau)
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
                System.Diagnostics.Debug.WriteLine($"[NOTIFICATION] Lỗi khi lấy danh sách: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"[NOTIFICATION] Lỗi phát âm thanh: {ex.Message}");
            }
        }

        /// <summary>
        /// ✨ Reset danh sách đã thông báo (chạy mỗi ngày)
        /// </summary>
        public void ResetNotifiedList()
        {
            _notifiedLopIds.Clear();
            System.Diagnostics.Debug.WriteLine("[NOTIFICATION] Đã reset danh sách thông báo.");
        }

        /// <summary>
        /// ✨ Xóa một lớp khỏi danh sách đã thông báo (để thông báo lại)
        /// </summary>
        public void RemoveFromNotified(int maLop)
        {
            _notifiedLopIds.Remove(maLop);
        }

        /// <summary>
        /// ✨ Lấy số lượng lớp sắp bắt đầu trong N giờ
        /// </summary>
        public int GetUpcomingClassCount(int hours = 24)
        {
            try
            {
                var now = DateTime.Now;
                var thresholdTime = now.AddHours(hours);

                return _lopHocRepo.GetAll()
                    .Count(l => l.ThoiGianBatDau.HasValue &&
                                l.ThoiGianBatDau.Value >= now &&
                                l.ThoiGianBatDau.Value <= thresholdTime &&
                                l.TrangThai == Constants.LopHocStatus.ChuaBatDau);
            }
            catch
            {
                return 0;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}