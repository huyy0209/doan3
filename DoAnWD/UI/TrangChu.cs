using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;
using System;
using System.Linq;
using System.Windows.Forms;

namespace DoAnWD
{
    /// <summary>
    /// Form Trang Chủ - ✨ CẬP NHẬT CHO OPTION 2: Tự động cập nhật trạng thái lớp học
    /// </summary>
    public partial class TrangChu : Form
    {
        private Form activeChild = null;

        // ✨ CÁC THÀNH PHẦN MỚI CHO AUTO-UPDATE
        private Timer autoUpdateTimer;
        private DateTime lastUpdateTime;

        // Thanh trạng thái góc phải + nút menu đang chọn
        private Label lblStatus;
        private Button activeMenuButton;

        // ✨ NOTIFICATION SYSTEM
        private Timer notificationTimer;
        private NotificationService _notificationService;
        private DateTime lastNotificationCheck;

        public TrangChu()
        {
            InitializeComponent();

            // NotificationService giữ danh sách lớp đã báo => sống suốt vòng đời form
            _notificationService = new NotificationService();
            InitializeStatusLabel();

            InitializeAutoUpdateTimer();
            InitializeNotificationTimer();

            // Subscribe events
            Load += TrangChu_Load;
            FormClosing += TrangChu_FormClosing;
        }

        // ===================================================================
        // ✨ AUTO-UPDATE TIMER SETUP
        // ===================================================================

        private void InitializeAutoUpdateTimer()
        {
            autoUpdateTimer = new Timer();
            autoUpdateTimer.Interval = Constants.Timer.AutoUpdateInterval; // 60 giây
            autoUpdateTimer.Tick += AutoUpdateTimer_Tick;
            autoUpdateTimer.Start();
            lastUpdateTime = DateTime.Now;

            System.Diagnostics.Debug.WriteLine("[TIMER] Auto-update timer đã khởi động.");
        }

        private void AutoUpdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                int updatedCount = ChayCapNhatTrangThai();
                lastUpdateTime = DateTime.Now;
                CapNhatThanhTrangThai();

                if (updatedCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[TIMER] {lastUpdateTime:HH:mm:ss} - Đã cập nhật {updatedCount} lớp học.");
                    RefreshActiveFormIfNeeded();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[TIMER] {lastUpdateTime:HH:mm:ss} - Không có lớp nào cần cập nhật.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TIMER ERROR] {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật trạng thái lớp/gia sư bằng DbContext MỚI mỗi lần,
        /// tránh ghi đè dữ liệu cũ (ví dụ lớp vừa bị hủy ở form khác).
        /// </summary>
        private static int ChayCapNhatTrangThai()
        {
            using (var service = new LopHocService())
            {
                return service.AutoUpdateStatus();
            }
        }

        // ===================================================================
        // THANH TRẠNG THÁI
        // ===================================================================

        private void InitializeStatusLabel()
        {
            lblStatus = new Label
            {
                AutoSize = false,
                Width = 520,
                Height = 40,
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                ForeColor = Constants.Colors.Gray,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblStatus.Location = new System.Drawing.Point(panelTop.Width - lblStatus.Width - 20, 20);
            panelTop.Controls.Add(lblStatus);
        }

        private void CapNhatThanhTrangThai()
        {
            if (lblStatus == null) return;

            int sapBatDau = _notificationService.GetUpcomingClassCount(24);
            lblStatus.Text = $"🔄 Cập nhật lúc {lastUpdateTime:HH:mm}   |   " +
                             (sapBatDau > 0
                                 ? $"🔔 {sapBatDau} lớp bắt đầu trong 24 giờ tới"
                                 : "✅ Không có lớp sắp bắt đầu");
            lblStatus.ForeColor = sapBatDau > 0 ? Constants.Colors.Orange : Constants.Colors.Gray;
        }

        private void RefreshActiveFormIfNeeded()
        {
            if (activeChild != null)
            {
                if (activeChild is frmGhepLop || activeChild is frmThongKe)
                {
                    var type = activeChild.GetType();
                    LoadChildForm((Form)Activator.CreateInstance(type));
                }
            }
        }

        // ===================================================================
        // ✨ NOTIFICATION TIMER SETUP - MỚI
        // ===================================================================

        private void InitializeNotificationTimer()
        {
            notificationTimer = new Timer();
            notificationTimer.Interval = 300000; // 5 phút = 300,000ms
            notificationTimer.Tick += NotificationTimer_Tick;
            notificationTimer.Start();
            lastNotificationCheck = DateTime.Now;

            System.Diagnostics.Debug.WriteLine("[NOTIFICATION] Notification timer đã khởi động - Check mỗi 5 phút.");

            // Chạy check ngay lần đầu (sau 10 giây)
            var initialTimer = new Timer();
            initialTimer.Interval = 10000;
            initialTimer.Tick += (s, ev) =>
            {
                CheckAndShowNotifications();
                initialTimer.Stop();
                initialTimer.Dispose();
            };
            initialTimer.Start();
        }

        private void NotificationTimer_Tick(object sender, EventArgs e)
        {
            lastNotificationCheck = DateTime.Now;
            CheckAndShowNotifications();
            CapNhatThanhTrangThai();
        }

        private void CheckAndShowNotifications()
        {
            try
            {
                // Lấy danh sách lớp sắp bắt đầu trong 24h
                var upcomingClasses = _notificationService.GetUpcomingClassesForNotification(24);

                if (upcomingClasses.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[NOTIFICATION] Tìm thấy {upcomingClasses.Count} lớp sắp bắt đầu!");

                    // Phát âm thanh
                    _notificationService.PlayNotificationSound();

                    // Hiển thị toast notification cho từng lớp
                    int delay = 0;
                    foreach (var lop in upcomingClasses)
                    {
                        var timer = new Timer();
                        timer.Interval = delay;
                        timer.Tick += (s, ev) =>
                        {
                            ShowToastNotification(lop);
                            timer.Stop();
                            timer.Dispose();
                        };
                        timer.Start();
                        delay += 2000; // Mỗi toast cách nhau 2 giây
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[NOTIFICATION] {DateTime.Now:HH:mm:ss} - Không có lớp nào sắp bắt đầu.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NOTIFICATION ERROR] {ex.Message}");
            }
        }

        private void ShowToastNotification(LopHoc lop)
        {
            try
            {
                var toast = new frmNotificationToast(lop);
                toast.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NOTIFICATION] Lỗi hiển thị toast: {ex.Message}");
            }
        }

        // ===================================================================
        // ✨ FORM EVENTS
        // ===================================================================

        private void TrangChu_Load(object sender, EventArgs e)
        {
            // Đồng bộ trạng thái ngay khi mở app (không đợi timer 60 giây)
            try
            {
                ChayCapNhatTrangThai();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[APP] Lỗi cập nhật ban đầu: {ex.Message}");
            }

            lastUpdateTime = DateTime.Now;
            CapNhatThanhTrangThai();

            // Mở màn hình Thống kê làm dashboard mặc định
            btnThongKe_Click(btnThongKe, EventArgs.Empty);
        }

        private void TrangChu_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ✅ Dừng timers
            if (autoUpdateTimer != null)
            {
                autoUpdateTimer.Stop();
                autoUpdateTimer.Dispose();
                System.Diagnostics.Debug.WriteLine("[TIMER] Auto-update timer đã dừng.");
            }

            if (notificationTimer != null)
            {
                notificationTimer.Stop();
                notificationTimer.Dispose();
                System.Diagnostics.Debug.WriteLine("[NOTIFICATION] Notification timer đã dừng.");
            }

            // ✅ Dispose services
            if (_notificationService != null)
            {
                _notificationService.Dispose();
                System.Diagnostics.Debug.WriteLine("[SERVICE] NotificationService đã dispose.");
            }
        }

        // ===================================================================
        // NAVIGATION METHODS - GIỮ NGUYÊN
        // ===================================================================

        private void LoadChildForm(Form frm)
        {
            if (activeChild != null)
            {
                activeChild.Close();
                activeChild.Dispose();
                panelHost.Controls.Clear();
                activeChild = null;
            }

            activeChild = frm;
            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            panelHost.Controls.Add(frm);
            frm.Show();
        }

        /// <summary>
        /// Tô sáng nút menu đang mở và hiển thị tên màn hình trên tiêu đề
        /// </summary>
        private void ChonMenu(Button btn, string tenManHinh)
        {
            if (activeMenuButton != null)
                activeMenuButton.BackColor = panelSidebar.BackColor;

            activeMenuButton = btn;
            if (btn != null)
                btn.BackColor = Constants.Colors.Blue;

            lblTitle.Text = "QUẢN LÝ GIA SƯ  ›  " + tenManHinh;
        }

        private void btnGiaSu_Click(object sender, EventArgs e)
        {
            ChonMenu(btnGiaSu, "Gia sư");
            LoadChildForm(new frmGiaSu());
        }

        private void btnHocVien_Click(object sender, EventArgs e)
        {
            ChonMenu(btnHocVien, "Học viên");
            LoadChildForm(new frmHocVien());
        }

        private void btnGhepLop_Click(object sender, EventArgs e)
        {
            ChonMenu(btnGhepLop, "Ghép lớp");
            LoadChildForm(new frmGhepLop());
        }

        private void btnThongKe_Click(object sender, EventArgs e)
        {
            ChonMenu(btnThongKe, "Thống kê");
            LoadChildForm(new frmThongKe());
        }

        private void btnCalendar_Click(object sender, EventArgs e)
        {
            ChonMenu(btnCalendar, "Lịch học");
            LoadChildForm(new frmCalendarView());
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (activeChild != null)
            {
                try
                {
                    int count = ChayCapNhatTrangThai();
                    lastUpdateTime = DateTime.Now;
                    CapNhatThanhTrangThai();
                    System.Diagnostics.Debug.WriteLine($"[MANUAL REFRESH] Cập nhật {count} lớp học.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MANUAL REFRESH ERROR] {ex.Message}");
                }

                var type = activeChild.GetType();
                LoadChildForm((Form)Activator.CreateInstance(type));
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ===================================================================
        // ✨ HELPER METHODS - MỞ RỘNG
        // ===================================================================

        public void PauseAutoUpdate()
        {
            if (autoUpdateTimer != null && autoUpdateTimer.Enabled)
            {
                autoUpdateTimer.Stop();
                System.Diagnostics.Debug.WriteLine("[TIMER] Timer tạm dừng.");
            }
        }

        public void ResumeAutoUpdate()
        {
            if (autoUpdateTimer != null && !autoUpdateTimer.Enabled)
            {
                autoUpdateTimer.Start();
                System.Diagnostics.Debug.WriteLine("[TIMER] Timer đã khởi động lại.");
            }
        }

        public void SetUpdateInterval(int milliseconds)
        {
            if (autoUpdateTimer != null)
            {
                autoUpdateTimer.Stop();
                autoUpdateTimer.Interval = milliseconds;
                autoUpdateTimer.Start();
                System.Diagnostics.Debug.WriteLine($"[TIMER] Interval thay đổi thành {milliseconds}ms.");
            }
        }

        public string GetTimerInfo()
        {
            if (autoUpdateTimer == null)
                return "Timer chưa khởi tạo";

            return $"Timer: {(autoUpdateTimer.Enabled ? "Đang chạy" : "Đã dừng")} | " +
                   $"Interval: {autoUpdateTimer.Interval / 1000}s | " +
                   $"Lần update cuối: {lastUpdateTime:HH:mm:ss}";
        }
    }
}