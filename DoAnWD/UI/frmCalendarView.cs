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
            monthGrid.DayClicked += OnDayClick;
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
                cboGiaSu.Items.Add("Tất cả gia sư");

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

                // Gia sư đăng nhập: chỉ xem lịch của mình
                if (!BLL.Services.PhienDangNhap.LaQuanLy && BLL.Services.PhienDangNhap.MaGS.HasValue)
                {
                    _selectedGiaSuId = BLL.Services.PhienDangNhap.MaGS;
                    cboGiaSu.Visible = false;
                }
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
                lblMonth.Text = $"Tháng {_currentMonth.Month}, {_currentMonth.Year}";

                var dauThang = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
                var cuoiThang = dauThang.AddMonths(1).AddDays(-1);

                // Lớp có ít nhất một buổi trong tháng (bỏ lớp đã hủy)
                // Lọc theo tháng + gia sư ngay trong SQL
                var lopHocs = _lopHocService.GetLopTrongKhoang(dauThang, cuoiThang.AddDays(1).AddTicks(-1), _selectedGiaSuId);

                // Một lớp học mỗi ngày trong khoảng [bắt đầu, kết thúc]
                // => hiện ở MỌI ngày đó (trước đây chỉ hiện ở ngày bắt đầu)
                _lichHocTheoNgay = new Dictionary<DateTime, List<LopHoc>>();
                for (var ngay = dauThang; ngay <= cuoiThang; ngay = ngay.AddDays(1))
                {
                    var trongNgay = lopHocs
                        .Where(l => l.ThoiGianBatDau.Value.Date <= ngay && l.ThoiGianKetThuc.Value.Date >= ngay)
                        .ToList();
                    if (trongNgay.Count > 0)
                        _lichHocTheoNgay[ngay] = trongNgay;
                }

                monthGrid.Month = dauThang;
                monthGrid.SetData(_lichHocTheoNgay);
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
        // CHI TIẾT NGÀY
        // ===================================================================

        private void OnDayClick(DateTime date)
        {
            List<LopHoc> classes;
            if (!_lichHocTheoNgay.TryGetValue(date, out classes))
            {
                MessageBox.Show($"Ngày {date:dd/MM/yyyy} không có lớp nào.",
                    "Lịch học", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var details = new System.Text.StringBuilder();
            details.AppendLine($"Ngày {date:dd/MM/yyyy}: {classes.Count} lớp");
            details.AppendLine();

            foreach (var lop in classes.OrderBy(l => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, l.CaHoc)))
            {
                details.AppendLine($"{lop.CaHoc}  {lop.MonHoc}");
                details.AppendLine($"    Gia sư: {lop.GiaSu?.HoTen ?? "?"}    Học viên: {lop.HocVien?.HoTen ?? "?"}");
                details.AppendLine();
            }

            MessageBox.Show(details.ToString(), "Lịch học", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (!BLL.Services.PhienDangNhap.LaQuanLy && BLL.Services.PhienDangNhap.MaGS.HasValue) return;

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