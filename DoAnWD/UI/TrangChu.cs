using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace DoAnWD
{
    /// <summary>
    /// Khung chính của ứng dụng: menu trái, thanh tiêu đề (ngày + dải ca học + trạng thái),
    /// vùng nội dung có đường lề đỏ. Tự động cập nhật trạng thái lớp và thông báo lớp sắp học.
    /// </summary>
    public partial class TrangChu : Form
    {
        private Form activeChild = null;

        // ✨ CÁC THÀNH PHẦN MỚI CHO AUTO-UPDATE
        private Timer autoUpdateTimer;
        private DateTime lastUpdateTime;

        // Menu đang chọn + vạch chỉ báo trượt
        private NavButton activeMenuButton;
        private Timer indicatorTimer;
        private float indicatorTop;

        private static readonly string[] TenThu =
            { "Chủ nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };

        // ✨ NOTIFICATION SYSTEM
        private Timer notificationTimer;
        private NotificationService _notificationService;
        private DateTime lastNotificationCheck;

        public TrangChu()
        {
            InitializeComponent();

            // NotificationService giữ danh sách lớp đã báo => sống suốt vòng đời form
            _notificationService = new NotificationService();
            InitializeShell();

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

        /// <summary>
        /// Giảm nháy khi đổi màn hình / thay đổi kích thước (vẽ toàn bộ cửa sổ một lần)
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        private void InitializeShell()
        {
            ThemeManager.EnableDoubleBuffer(panelHost);
            ThemeManager.EnableDoubleBuffer(panelSidebar);

            panelLogo.Paint += PanelLogo_Paint;
            panelTop.Paint += (s, e) =>
            {
                using (var p = new Pen(Theme.Rule))
                    e.Graphics.DrawLine(p, 0, panelTop.Height - 1, panelTop.Width, panelTop.Height - 1);
            };
            panelMarginLine.Paint += (s, e) =>
            {
                using (var b = new SolidBrush(Theme.Margin))
                    e.Graphics.FillRectangle(b, 8, 0, 2, panelMarginLine.Height);
            };

            indicatorTimer = new Timer { Interval = 15 };
            indicatorTimer.Tick += IndicatorTimer_Tick;
        }

        /// <summary>
        /// Biểu tượng: trang vở nhỏ có đường lề đỏ và dòng kẻ
        /// </summary>
        private void PanelLogo_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var page = new RectangleF(24, 30, 26, 32);

            using (var path = Theme.RoundedRect(page, 3))
            using (var b = new SolidBrush(Color.White))
                g.FillPath(b, path);
            using (var p = new Pen(Theme.InkMuted))
                for (int i = 1; i <= 3; i++)
                    g.DrawLine(p, page.X + 9, page.Y + i * 8, page.Right - 4, page.Y + i * 8);
            using (var p = new Pen(Theme.Margin, 2f))
                g.DrawLine(p, page.X + 6, page.Y + 2, page.X + 6, page.Bottom - 2);
        }

        /// <summary>
        /// Cập nhật thanh tiêu đề: ngày, ca hiện tại, số lớp theo ca, lớp sắp bắt đầu
        /// </summary>
        private void CapNhatThanhTrangThai()
        {
            var now = DateTime.Now;
            lblTitle.Text = $"{TenThu[(int)now.DayOfWeek]}, {now:dd/MM/yyyy}";

            string ca = ShiftRibbon.CaHienTai(now);
            lblSubtitle.Text = ca != null ? $"Đang trong ca {ca}" : "Ngoài giờ học";

            try
            {
                using (var service = new LopHocService())
                    shiftRibbon.SetSoLop(service.GetSoLopHomNayTheoCa());

                int sapBatDau = _notificationService.GetUpcomingClassCount(24);
                lblStatus.Text = sapBatDau > 0
                    ? $"{sapBatDau} lớp bắt đầu trong 24 giờ tới"
                    : "Không có lớp sắp bắt đầu";
                lblStatus.ForeColor = sapBatDau > 0 ? Theme.Warning : Theme.TextMuted;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không đọc được dữ liệu";
                lblStatus.ForeColor = Theme.Margin;
                System.Diagnostics.Debug.WriteLine($"[HEADER] {ex.Message}");
            }

            lblUpdated.Text = $"Cập nhật lúc {lastUpdateTime:HH:mm}";
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

            indicatorTimer?.Stop();
            indicatorTimer?.Dispose();

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
            panelHost.SuspendLayout();
            try
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

                // Áp giao diện trước khi hiện để không thấy màu cũ chớp qua
                ThemeManager.Apply(frm);

                panelHost.Controls.Add(frm);
                frm.Show();
            }
            finally
            {
                panelHost.ResumeLayout(true);
            }
        }

        /// <summary>
        /// Đánh dấu mục menu đang mở; vạch đỏ trượt tới mục đó
        /// </summary>
        private void ChonMenu(NavButton btn)
        {
            if (activeMenuButton != null)
                activeMenuButton.Active = false;

            activeMenuButton = btn;
            btn.Active = true;

            if (!pnlIndicator.Visible || !Theme.MotionEnabled)
            {
                indicatorTop = btn.Top;
                pnlIndicator.Top = btn.Top;
                pnlIndicator.Height = btn.Height;
                pnlIndicator.Visible = true;
                pnlIndicator.BringToFront();
                return;
            }

            indicatorTop = pnlIndicator.Top;
            indicatorTimer.Start();
        }

        private void IndicatorTimer_Tick(object sender, EventArgs e)
        {
            if (activeMenuButton == null) { indicatorTimer.Stop(); return; }

            float target = activeMenuButton.Top;
            indicatorTop += (target - indicatorTop) * 0.3f; // ease-out

            if (Math.Abs(target - indicatorTop) < 0.5f)
            {
                indicatorTop = target;
                indicatorTimer.Stop();
            }

            pnlIndicator.Top = (int)Math.Round(indicatorTop);
            pnlIndicator.BringToFront();
        }

        private void btnGiaSu_Click(object sender, EventArgs e)
        {
            ChonMenu(btnGiaSu);
            LoadChildForm(new frmGiaSu());
        }

        private void btnHocVien_Click(object sender, EventArgs e)
        {
            ChonMenu(btnHocVien);
            LoadChildForm(new frmHocVien());
        }

        private void btnGhepLop_Click(object sender, EventArgs e)
        {
            ChonMenu(btnGhepLop);
            LoadChildForm(new frmGhepLop());
        }

        private void btnThongKe_Click(object sender, EventArgs e)
        {
            ChonMenu(btnThongKe);
            LoadChildForm(new frmThongKe());
        }

        private void btnCalendar_Click(object sender, EventArgs e)
        {
            ChonMenu(btnCalendar);
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