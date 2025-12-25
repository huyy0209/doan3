using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD
{
    /// <summary>
    /// ✨ Calendar View - Xem lịch học theo tháng
    /// </summary>
    public partial class frmCalendarView : Form
    {
        private LopHocService _lopHocService;
        private GiaSuService _giaSuService;
        private DateTime _currentMonth;
        private int? _selectedGiaSuId = null;
        private Dictionary<DateTime, List<LopHoc>> _lichHocTheoNgay;

        public frmCalendarView()
        {
            InitializeComponent();
            _lopHocService = new LopHocService();
            _giaSuService = new GiaSuService();
            _currentMonth = DateTime.Now.Date;
            _lichHocTheoNgay = new Dictionary<DateTime, List<LopHoc>>();

            Load += FrmCalendarView_Load;
            FormClosed += FrmCalendarView_FormClosed;
        }

        private void FrmCalendarView_Load(object sender, EventArgs e)
        {
            LoadGiaSuFilter();
            LoadCalendar();
        }

        // ===================================================================
        // ✨ LOAD GIA SƯ FILTER
        // ===================================================================

        private void LoadGiaSuFilter()
        {
            try
            {
                var giaSus = _giaSuService.GetAll().ToList();

                cboGiaSu.Items.Clear();
                cboGiaSu.Items.Add("-- Tất cả Gia sư --");

                foreach (var gs in giaSus)
                {
                    cboGiaSu.Items.Add(new ComboBoxItem
                    {
                        Text = $"{gs.HoTen} ({gs.ChuyenMon})",
                        Value = gs.MaGS
                    });
                }

                cboGiaSu.DisplayMember = "Text";
                cboGiaSu.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách gia sư: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===================================================================
        // ✨ LOAD CALENDAR
        // ===================================================================

        private void LoadCalendar()
        {
            try
            {
                // Cập nhật tiêu đề tháng
                lblMonth.Text = _currentMonth.ToString("MMMM yyyy");

                // Lấy dữ liệu lớp học
                var lopHocs = _lopHocService.GetAll()
                    .Where(l => l.DangHoatDong &&
                                l.ThoiGianBatDau.HasValue &&
                                l.ThoiGianBatDau.Value.Year == _currentMonth.Year &&
                                l.ThoiGianBatDau.Value.Month == _currentMonth.Month)
                    .ToList();

                // Filter theo gia sư nếu được chọn
                if (_selectedGiaSuId.HasValue)
                {
                    lopHocs = lopHocs.Where(l => l.MaGS == _selectedGiaSuId.Value).ToList();
                }

                // Group theo ngày
                _lichHocTheoNgay = lopHocs
                    .GroupBy(l => l.ThoiGianBatDau.Value.Date)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Vẽ calendar
                DrawCalendar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải lịch: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===================================================================
        // ✨ VẼ CALENDAR
        // ===================================================================

        private void DrawCalendar()
        {
            // Clear panel
            panelCalendar.Controls.Clear();

            // Lấy ngày đầu tiên của tháng
            DateTime firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
            int startDayOfWeek = (int)firstDay.DayOfWeek; // 0 = Sunday

            // Chuyển sang Monday = 0
            startDayOfWeek = (startDayOfWeek == 0) ? 6 : startDayOfWeek - 1;

            // Tạo header (T2-CN)
            string[] dayNames = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
            for (int i = 0; i < 7; i++)
            {
                var lblDay = new Label
                {
                    Text = dayNames[i],
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = i == 6 ? Color.Red : Constants.Colors.DarkGray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.FromArgb(236, 240, 241),
                    Width = 140,
                    Height = 30,
                    Left = i * 140,
                    Top = 0
                };
                panelCalendar.Controls.Add(lblDay);
            }

            // Vẽ các ngày
            int row = 1;
            int col = startDayOfWeek;

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDate = new DateTime(_currentMonth.Year, _currentMonth.Month, day);

                var pnlDay = CreateDayPanel(currentDate, col, row);
                panelCalendar.Controls.Add(pnlDay);

                col++;
                if (col > 6)
                {
                    col = 0;
                    row++;
                }
            }

            // Set chiều cao panel
            panelCalendar.Height = (row + 1) * 100 + 30;
        }

        // ===================================================================
        // ✨ TẠO PANEL CHO MỖI NGÀY
        // ===================================================================

        private Panel CreateDayPanel(DateTime date, int col, int row)
        {
            bool isToday = date.Date == DateTime.Now.Date;
            bool hasClasses = _lichHocTheoNgay.ContainsKey(date);

            var panel = new Panel
            {
                Width = 140,
                Height = 100,
                Left = col * 140,
                Top = row * 100 + 30,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = isToday ? Color.FromArgb(255, 243, 224) : Color.White,
                Tag = date,
                Cursor = hasClasses ? Cursors.Hand : Cursors.Default
            };

            // Label ngày
            var lblDate = new Label
            {
                Text = date.Day.ToString(),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = isToday ? Constants.Colors.Orange :
                           (date.DayOfWeek == DayOfWeek.Sunday ? Color.Red : Constants.Colors.DarkGray),
                AutoSize = false,
                Width = 30,
                Height = 20,
                Left = 5,
                Top = 5
            };
            panel.Controls.Add(lblDate);

            // Hiển thị số lớp học
            if (hasClasses)
            {
                var classes = _lichHocTheoNgay[date];
                int top = 30;

                foreach (var lop in classes.Take(3)) // Hiển thị tối đa 3 lớp
                {
                    var lblClass = new Label
                    {
                        Text = $"{lop.CaHoc?.Substring(0, Math.Min(10, lop.CaHoc.Length))}",
                        Font = new Font("Segoe UI", 7F),
                        ForeColor = Color.White,
                        BackColor = GetColorByCaHoc(lop.CaHoc),
                        AutoSize = false,
                        Width = 125,
                        Height = 18,
                        Left = 5,
                        Top = top,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Padding = new Padding(3, 0, 0, 0)
                    };
                    panel.Controls.Add(lblClass);
                    top += 20;
                }

                if (classes.Count > 3)
                {
                    var lblMore = new Label
                    {
                        Text = $"+{classes.Count - 3} lớp nữa...",
                        Font = new Font("Segoe UI", 7F, FontStyle.Italic),
                        ForeColor = Constants.Colors.Gray,
                        AutoSize = true,
                        Left = 5,
                        Top = top
                    };
                    panel.Controls.Add(lblMore);
                }
            }

            // Sự kiện click
            panel.Click += (s, e) => OnDayClick(date);
            foreach (Control ctrl in panel.Controls)
            {
                ctrl.Click += (s, e) => OnDayClick(date);
            }

            return panel;
        }

        // ===================================================================
        // ✨ MÀU SẮC THEO CA HỌC
        // ===================================================================

        private Color GetColorByCaHoc(string caHoc)
        {
            if (string.IsNullOrEmpty(caHoc))
                return Constants.Colors.Gray;

            if (caHoc.Contains("Sáng"))
                return Color.FromArgb(52, 152, 219); // Xanh dương
            if (caHoc.Contains("Chiều"))
                return Color.FromArgb(230, 126, 34); // Cam
            if (caHoc.Contains("Tối"))
                return Color.FromArgb(142, 68, 173); // Tím

            return Constants.Colors.Gray;
        }

        // ===================================================================
        // ✨ SỰ KIỆN CLICK NGÀY
        // ===================================================================

        private void OnDayClick(DateTime date)
        {
            if (!_lichHocTheoNgay.ContainsKey(date))
            {
                MessageBox.Show($"Không có lớp học nào vào ngày {date:dd/MM/yyyy}",
                    Constants.Messages.InfoTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Hiển thị chi tiết lớp học
            var classes = _lichHocTheoNgay[date];
            var details = $"📅 Lịch học ngày {date:dd/MM/yyyy}\n\n";

            foreach (var lop in classes)
            {
                details += $"🕐 {lop.CaHoc}\n";
                details += $"   Môn: {lop.MonHoc}\n";
                details += $"   GS: {lop.GiaSu?.HoTen ?? "N/A"}\n";
                details += $"   HV: {lop.HocVien?.HoTen ?? "N/A"}\n";
                details += $"   Học phí: {lop.HocPhi:N0} VNĐ\n\n";
            }

            MessageBox.Show(details,
                "Chi tiết lớp học",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // ===================================================================
        // ✨ NAVIGATION EVENTS
        // ===================================================================

        private void btnPrevMonth_Click(object sender, EventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            LoadCalendar();
        }

        private void btnNextMonth_Click(object sender, EventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(1);
            LoadCalendar();
        }

        private void btnToday_Click(object sender, EventArgs e)
        {
            _currentMonth = DateTime.Now.Date;
            LoadCalendar();
        }

        private void cboGiaSu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboGiaSu.SelectedIndex == 0)
            {
                _selectedGiaSuId = null;
            }
            else
            {
                var item = cboGiaSu.SelectedItem as ComboBoxItem;
                _selectedGiaSuId = item?.Value;
            }

            LoadCalendar();
        }

        private void FrmCalendarView_FormClosed(object sender, FormClosedEventArgs e)
        {
            _lopHocService?.Dispose();
            _giaSuService?.Dispose();
        }

        // ===================================================================
        // ✨ HELPER CLASS
        // ===================================================================

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }
    }
}