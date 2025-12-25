using System;
using System.Drawing;
using System.Windows.Forms;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD
{
    /// <summary>
    /// ✨ Toast Notification - Thông báo popup nhẹ
    /// </summary>
    public partial class frmNotificationToast : Form
    {
        private LopHoc _lopHoc;
        private Timer _autoCloseTimer;
        private Timer _slideInTimer;
        private int _targetY;
        private Label lblContent;

        public frmNotificationToast(LopHoc lopHoc)
        {
            // ✅ GỌI Designer TRƯỚC
            InitializeComponent();

            _lopHoc = lopHoc;

            // Setup form properties
            this.ShowInTaskbar = false;
            this.Size = new Size(400, 120);

            // Position ở góc phải màn hình
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(screen.Right - this.Width - 10, screen.Bottom);
            _targetY = screen.Bottom - this.Height - 10;

            // ✅ SAU ĐÓ MỚI TẠO CONTROLS
            CreateControls();
            LoadData();
            StartSlideIn();

            // Auto close sau 10 giây
            _autoCloseTimer = new Timer();
            _autoCloseTimer.Interval = 10000;
            _autoCloseTimer.Tick += (s, e) => CloseToast();
            _autoCloseTimer.Start();

            // ✅ ĐĂNG KÝ CLEANUP KHI FORM ĐÓNG
            this.FormClosed += FrmNotificationToast_FormClosed;
        }

        private void CreateControls()
        {
            // Panel chính
            var panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(52, 152, 219),
                Padding = new Padding(15)
            };

            // Icon
            var lblIcon = new Label
            {
                Text = "🔔",
                Font = new Font("Segoe UI", 24F),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(50, 50),
                Location = new Point(15, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Title
            var lblTitle = new Label
            {
                Text = "⏰ LỚP SẮP BẮT ĐẦU!",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(300, 25),
                Location = new Point(75, 15)
            };

            // Content
            lblContent = new Label
            {
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(300, 60),
                Location = new Point(75, 40)
            };

            // Close button
            var btnClose = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(360, 10),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => CloseToast();

            // Snooze button
            var btnSnooze = new Button
            {
                Text = "⏰ Nhắc sau 15p",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(41, 128, 185),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 25),
                Location = new Point(240, 85),
                Cursor = Cursors.Hand
            };
            btnSnooze.FlatAppearance.BorderSize = 0;
            btnSnooze.Click += BtnSnooze_Click;

            // Add controls
            panelMain.Controls.Add(lblIcon);
            panelMain.Controls.Add(lblTitle);
            panelMain.Controls.Add(lblContent);
            panelMain.Controls.Add(btnClose);
            panelMain.Controls.Add(btnSnooze);
            this.Controls.Add(panelMain);

            // Click để xem chi tiết
            this.Cursor = Cursors.Hand;
            this.Click += (s, e) => ShowDetails();
            panelMain.Click += (s, e) => ShowDetails();
            lblIcon.Click += (s, e) => ShowDetails();
            lblTitle.Click += (s, e) => ShowDetails();
            lblContent.Click += (s, e) => ShowDetails();
        }

        private void LoadData()
        {
            if (_lopHoc == null || lblContent == null) return;

            try
            {
                var giaSu = _lopHoc.GiaSu?.HoTen ?? "N/A";
                var hocVien = _lopHoc.HocVien?.HoTen ?? "N/A";
                var batDau = _lopHoc.ThoiGianBatDau?.ToString("dd/MM/yyyy HH:mm") ?? "";
                var caHoc = _lopHoc.CaHoc ?? "";
                var soGioConLai = _lopHoc.SoNgayConLai;

                lblContent.Text =
                    $"📚 Môn: {_lopHoc.MonHoc}\n" +
                    $"👨‍🏫 GS: {giaSu} | 👨‍🎓 HV: {hocVien}\n" +
                    $"🕐 {caHoc} - {batDau} (còn {soGioConLai} ngày)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TOAST] Lỗi LoadData: {ex.Message}");
            }
        }

        private void StartSlideIn()
        {
            _slideInTimer = new Timer();
            _slideInTimer.Interval = 10;
            _slideInTimer.Tick += (s, e) =>
            {
                if (this.Top > _targetY)
                {
                    this.Top -= 5;
                }
                else
                {
                    this.Top = _targetY;
                    _slideInTimer?.Stop();
                    _slideInTimer?.Dispose();
                    _slideInTimer = null;
                }
            };
            _slideInTimer.Start();
        }

        private void CloseToast()
        {
            _autoCloseTimer?.Stop();
            _slideInTimer?.Stop();

            // Animation slide out
            var slideOutTimer = new Timer();
            slideOutTimer.Interval = 10;
            slideOutTimer.Tick += (s, e) =>
            {
                this.Opacity -= 0.05;
                if (this.Opacity <= 0)
                {
                    slideOutTimer.Stop();
                    slideOutTimer.Dispose();
                    this.Close();
                }
            };
            slideOutTimer.Start();
        }

        private void BtnSnooze_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Sẽ nhắc lại sau 15 phút!",
                Constants.Messages.InfoTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            CloseToast();
        }

        private void ShowDetails()
        {
            if (_lopHoc == null) return;

            var details =
                $"📋 CHI TIẾT LỚP HỌC\n" +
                $"━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                $"📚 Môn học: {_lopHoc.MonHoc}\n" +
                $"👨‍🏫 Gia sư: {_lopHoc.GiaSu?.HoTen ?? "N/A"}\n" +
                $"👨‍🎓 Học viên: {_lopHoc.HocVien?.HoTen ?? "N/A"}\n" +
                $"🕐 Ca học: {_lopHoc.CaHoc}\n" +
                $"📅 Bắt đầu: {_lopHoc.ThoiGianBatDau?.ToString("dd/MM/yyyy HH:mm")}\n" +
                $"📅 Kết thúc: {_lopHoc.ThoiGianKetThuc?.ToString("dd/MM/yyyy HH:mm")}\n" +
                $"💰 Học phí: {_lopHoc.HocPhi:N0} VNĐ\n" +
                $"📊 Trạng thái: {_lopHoc.TrangThai}";

            MessageBox.Show(details,
                "Chi tiết lớp học",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // ✅ CLEANUP KHI FORM ĐÓNG - THAY VÌ OVERRIDE Dispose
        private void FrmNotificationToast_FormClosed(object sender, FormClosedEventArgs e)
        {
            _autoCloseTimer?.Stop();
            _autoCloseTimer?.Dispose();
            _slideInTimer?.Stop();
            _slideInTimer?.Dispose();
        }
    }
}