using System;
using System.Drawing;
using System.Windows.Forms;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;
using DoAnWD.UI.Controls;
using DoAnWD.UI.Pages;

namespace DoAnWD.UI.Dialogs
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
            this.Size = new Size(380, 116);

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

        /// <summary>
        /// Toast nền trắng, vạch màu của ca học bên trái, chữ mực tím
        /// </summary>
        private void CreateControls()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = DoAnWD.UI.Theme.Rule; // viền 1px
            Padding = new Padding(1);
            Size = new Size(380, 116);

            Color shift = DoAnWD.UI.Theme.ShiftColor(_lopHoc?.CaHoc);

            var panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DoAnWD.UI.Theme.Surface
            };
            panelMain.Paint += (s, e) =>
            {
                using (var b = new SolidBrush(shift))
                    e.Graphics.FillRectangle(b, 0, 0, 4, panelMain.Height);
            };

            var lblTitle = new Label
            {
                Text = "Lớp sắp bắt đầu",
                Font = DoAnWD.UI.Theme.Section,
                ForeColor = DoAnWD.UI.Theme.Ink,
                AutoSize = false,
                Size = new Size(300, 24),
                Location = new Point(18, 12)
            };

            lblContent = new Label
            {
                Font = DoAnWD.UI.Theme.Body,
                ForeColor = DoAnWD.UI.Theme.Text,
                AutoSize = false,
                Size = new Size(340, 46),
                Location = new Point(18, 38)
            };

            var btnClose = new Button
            {
                Text = "×",
                Font = new Font("Segoe UI", 13F),
                ForeColor = DoAnWD.UI.Theme.TextMuted,
                BackColor = DoAnWD.UI.Theme.Surface,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(342, 6),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = DoAnWD.UI.Theme.NeutralSoft;
            btnClose.Click += (s, e) => CloseToast();

            var btnDetail = new DoAnWD.UI.ThemedButton
            {
                Text = "Xem chi tiết",
                Kind = DoAnWD.UI.ButtonKind.Ghost,
                Size = new Size(110, 26),
                Location = new Point(250, 82)
            };
            btnDetail.Click += (s, e) => ShowDetails();

            panelMain.Controls.Add(lblTitle);
            panelMain.Controls.Add(lblContent);
            panelMain.Controls.Add(btnClose);
            panelMain.Controls.Add(btnDetail);
            this.Controls.Add(panelMain);

            // Bấm vào thân thông báo để xem chi tiết
            panelMain.Cursor = Cursors.Hand;
            panelMain.Click += (s, e) => ShowDetails();
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

                lblContent.Text =
                    $"{_lopHoc.MonHoc}, {caHoc}, bắt đầu {batDau}\n" +
                    $"Gia sư {giaSu}, học viên {hocVien}";
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
                if (this.Top > _targetY + 1)
                {
                    this.Top -= Math.Max(1, (this.Top - _targetY) / 5); // chậm dần khi tới nơi
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

        private void ShowDetails()
        {
            if (_lopHoc == null) return;

            var details =
                $"Môn học: {_lopHoc.MonHoc}\n" +
                $"Gia sư: {_lopHoc.GiaSu?.HoTen ?? "?"}\n" +
                $"Học viên: {_lopHoc.HocVien?.HoTen ?? "?"}\n" +
                $"Ca học: {_lopHoc.CaHoc}\n" +
                $"Bắt đầu: {_lopHoc.ThoiGianBatDau?.ToString("dd/MM/yyyy HH:mm")}\n" +
                $"Kết thúc: {_lopHoc.ThoiGianKetThuc?.ToString("dd/MM/yyyy HH:mm")}\n" +
                $"Học phí: {_lopHoc.HocPhi:N0} đ\n" +
                $"Trạng thái: {_lopHoc.TrangThai}";

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