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
    /// Dialog Ghép Lớp - ✨ CẬP NHẬT CHO OPTION 2: Quản lý ca học tại trung tâm
    /// </summary>
    public partial class frmChucNangGhepLop : Form
    {
        private HocVienService _hocVienService;
        private GiaSuService _giaSuService;
        private LopHocService _lopHocService;

        public frmChucNangGhepLop()
        {
            InitializeComponent();
            _hocVienService = new HocVienService();
            _giaSuService = new GiaSuService();
            _lopHocService = new LopHocService();
            Load += FrmChucNangGhepLop_Load;
            FormClosed += FrmChucNangGhepLop_FormClosed;

            // ✨ Đăng ký sự kiện cho DateTimePickers và ComboBox
            dtpBatDau.ValueChanged += DtpBatDau_ValueChanged;
            dtpKetThuc.ValueChanged += DtpKetThuc_ValueChanged;
            cboCaHoc.SelectedIndexChanged += CboCaHoc_SelectedIndexChanged;

            // ✨ MỚI: Đăng ký DrawItem cho ComboBox để custom draw
            cboCaHoc.DrawMode = DrawMode.OwnerDrawFixed;
            cboCaHoc.DrawItem += CboCaHoc_DrawItem;
        }

        private void FrmChucNangGhepLop_Load(object sender, EventArgs e)
        {
            try
            {
                // Load Học Viên
                var hocViens = _hocVienService.GetAll().ToList();
                cboHocVien.DataSource = hocViens;
                cboHocVien.DisplayMember = "HoTen";
                cboHocVien.ValueMember = "MaHV";

                // Load Gia Sư rảnh
                var giaSus = _giaSuService.GetAvailable().ToList();
                cboGiaSu.DataSource = giaSus;
                cboGiaSu.DisplayMember = "HoTen";
                cboGiaSu.ValueMember = "MaGS";

                // ✨ Load Ca học từ Constants
                cboCaHoc.DataSource = Constants.CaHoc.TatCaCaHoc.ToList();

                // ✨ Set mặc định trạng thái mới
                cboTrangThai.Items.Clear();
                cboTrangThai.Items.AddRange(new object[]
                {
                    Constants.LopHocStatus.ChuaBatDau,
                    Constants.LopHocStatus.DangDienRa,
                    Constants.LopHocStatus.DaKetThuc
                });
                cboTrangThai.SelectedIndex = 0;

                // ✨ Set thời gian mặc định
                dtpBatDau.Value = DateTime.Now.Date.AddDays(1).AddHours(7); // Ngày mai 7h sáng
                dtpKetThuc.Value = dtpBatDau.Value.AddMonths(1); // Sau 1 tháng

                if (cboHocVien.Items.Count > 0)
                    cboHocVien.SelectedIndex = 0;
                if (cboGiaSu.Items.Count > 0)
                    cboGiaSu.SelectedIndex = 0;
                if (cboCaHoc.Items.Count > 0)
                    cboCaHoc.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void cboHocVien_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = cboHocVien.SelectedItem as HocVien;
            if (selected != null)
            {
                txtSDT_HV.Text = selected.SDT ?? "";
                txtDiaChi_HV.Text = selected.DiaChi ?? "";
            }
        }

        // ===================================================================
        // ✨ HIỂN THỊ LỊCH GIA SƯ - TÍNH NĂNG MỚI
        // ===================================================================

        private void cboGiaSu_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = cboGiaSu.SelectedItem as GiaSu;
            if (selected != null)
            {
                txtSDT_GS.Text = selected.SDT ?? "";
                txtChuyenMon_GS.Text = selected.ChuyenMon ?? "";
                txtChuyenMon_GS.BackColor = Constants.Colors.ActiveOrange;
                txtChuyenMon_GS.ForeColor = Constants.Colors.Orange;

                // ✨ Hiển thị lịch dạy của gia sư
                HienThiLichGiaSu(selected.MaGS);

                // ✨ Filter và highlight ca học
                FilterVaHighlightCaHoc();

                // ✨ Kiểm tra xung đột ngay
                KiemTraXungDotCaHoc();
            }
        }

        /// <summary>
        /// ✨ Hiển thị lịch dạy hiện tại của gia sư
        /// </summary>
        private void HienThiLichGiaSu(int maGS)
        {
            try
            {
                lstLichGiaSu.Items.Clear();

                // Lấy tất cả lớp đang hoạt động của gia sư
                var lichHoc = _lopHocService.GetAll()
                    .Where(l => l.MaGS == maGS && l.DangHoatDong)
                    .OrderBy(l => l.CaHoc)
                    .ThenBy(l => l.ThoiGianBatDau)
                    .ToList();

                if (lichHoc.Count == 0)
                {
                    lstLichGiaSu.Items.Add("✅ Gia sư hiện đang rảnh - chưa có lớp nào!");
                    lstLichGiaSu.ForeColor = Color.Green;
                    lblTitleLichGS.Text = "Gia sư rảnh - có thể ghép lớp bất kỳ ca nào";
                    lblTitleLichGS.ForeColor = Color.Green;
                }
                else
                {
                    lstLichGiaSu.Items.Add($"📚 Đang dạy {lichHoc.Count} lớp:");
                    lstLichGiaSu.Items.Add("─────────────────────────────────────────────────────────────────");

                    foreach (var lop in lichHoc)
                    {
                        string trangThai = lop.TrangThaiTheoThoiGian;
                        string icon = trangThai == Constants.LopHocStatus.DangDienRa ? "🟢" : "🔵";

                        lstLichGiaSu.Items.Add(
                            $"{icon} {lop.CaHoc,-20} | {lop.MonHoc,-10} | " +
                            $"{lop.ThoiGianBatDau:dd/MM/yyyy} - {lop.ThoiGianKetThuc:dd/MM/yyyy} | " +
                            $"[{trangThai}]");
                    }

                    lstLichGiaSu.ForeColor = Color.FromArgb(52, 73, 94);
                    lblTitleLichGS.Text = $"Gia sư đang dạy {lichHoc.Count} lớp - Chú ý kiểm tra xung đột ca học!";
                    lblTitleLichGS.ForeColor = Color.OrangeRed;
                }
            }
            catch (Exception ex)
            {
                lstLichGiaSu.Items.Clear();
                lstLichGiaSu.Items.Add($"❌ Lỗi khi load lịch: {ex.Message}");
                lstLichGiaSu.ForeColor = Color.Red;
            }
        }

        // ===================================================================
        // ✨ FILTER VÀ HIGHLIGHT CA HỌC - TÍNH NĂNG MỚI
        // ===================================================================

        /// <summary>
        /// ✨ Lấy danh sách ca đã bận của gia sư
        /// </summary>
        private HashSet<string> GetCaHocDaBan()
        {
            var caBan = new HashSet<string>();

            if (cboGiaSu.SelectedValue == null)
                return caBan;

            try
            {
                int maGS = (int)cboGiaSu.SelectedValue;
                DateTime batDau = dtpBatDau.Value;
                DateTime ketThuc = dtpKetThuc.Value;

                var lopMoi = new LopHoc
                {
                    ThoiGianBatDau = batDau,
                    ThoiGianKetThuc = ketThuc
                };

                // Lấy tất cả lớp đang hoạt động
                var cacLopHienTai = _lopHocService.GetAll()
                    .Where(l => l.MaGS == maGS && l.DangHoatDong)
                    .ToList();

                // Tìm các ca bị trùng thời gian
                foreach (var lop in cacLopHienTai)
                {
                    if (lop.TrungThoiGian(lopMoi) && !string.IsNullOrEmpty(lop.CaHoc))
                    {
                        caBan.Add(lop.CaHoc);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy ca bận: {ex.Message}");
            }

            return caBan;
        }

        /// <summary>
        /// ✨ Filter và highlight ca học khả dụng
        /// </summary>
        private void FilterVaHighlightCaHoc()
        {
            // Force refresh ComboBox để trigger DrawItem event
            cboCaHoc.Invalidate();
            cboCaHoc.Refresh();
        }

        /// <summary>
        /// ✨ Custom draw cho ComboBox Ca học
        /// </summary>
        private void CboCaHoc_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            string caHoc = cboCaHoc.Items[e.Index].ToString();
            var caBan = GetCaHocDaBan();
            bool isBusy = caBan.Contains(caHoc);

            // ✨ Chọn màu dựa trên trạng thái
            Color backgroundColor;
            Color textColor;
            string prefix;

            if (isBusy)
            {
                // ❌ Ca đã bận - Màu đỏ nhạt
                backgroundColor = Color.FromArgb(255, 220, 220);
                textColor = Color.FromArgb(192, 57, 43);
                prefix = "❌ ";
            }
            else
            {
                // ✅ Ca còn trống - Màu xanh nhạt
                backgroundColor = Color.FromArgb(212, 239, 223);
                textColor = Color.FromArgb(39, 174, 96);
                prefix = "✅ ";
            }

            // Vẽ background
            using (SolidBrush bgBrush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            // Vẽ text
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                Font font = new Font(e.Font, isBusy ? FontStyle.Strikeout : FontStyle.Regular);
                e.Graphics.DrawString(prefix + caHoc, font, textBrush, e.Bounds.Left + 2, e.Bounds.Top + 2);
            }

            // Vẽ focus rectangle
            e.DrawFocusRectangle();
        }

        // ===================================================================
        // ✨ KIỂM TRA XUNG ĐỘT CA HỌC REAL-TIME
        // ===================================================================

        private void DtpBatDau_ValueChanged(object sender, EventArgs e)
        {
            FilterVaHighlightCaHoc();
            KiemTraXungDotCaHoc();
        }

        private void DtpKetThuc_ValueChanged(object sender, EventArgs e)
        {
            FilterVaHighlightCaHoc();
            KiemTraXungDotCaHoc();
        }

        private void CboCaHoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            KiemTraXungDotCaHoc();
        }

        /// <summary>
        /// ✨ Kiểm tra xung đột ca học và hiển thị cảnh báo
        /// </summary>
        private void KiemTraXungDotCaHoc()
        {
            if (cboGiaSu.SelectedValue == null || cboCaHoc.SelectedItem == null)
            {
                lblWarningCaHoc.Visible = false;
                return;
            }

            try
            {
                int maGS = (int)cboGiaSu.SelectedValue;
                string caHoc = cboCaHoc.SelectedItem.ToString();
                DateTime batDau = dtpBatDau.Value;
                DateTime ketThuc = dtpKetThuc.Value;

                // Tạo lớp học tạm để kiểm tra
                var lopMoi = new LopHoc
                {
                    ThoiGianBatDau = batDau,
                    ThoiGianKetThuc = ketThuc,
                    CaHoc = caHoc
                };

                // Lấy các lớp đang hoạt động của gia sư
                var cacLopHienTai = _lopHocService.GetAll()
                    .Where(l => l.MaGS == maGS && l.DangHoatDong)
                    .ToList();

                // Kiểm tra xung đột
                var lopBiTrung = cacLopHienTai.FirstOrDefault(l =>
                    l.TrungThoiGian(lopMoi) && l.TrungCaHoc(lopMoi));

                if (lopBiTrung != null)
                {
                    // ❌ CÓ XUNG ĐỘT
                    lblWarningCaHoc.Text = $"⚠️ CA HỌC BỊ TRÙNG với lớp {lopBiTrung.MonHoc} " +
                                          $"({lopBiTrung.ThoiGianBatDau:dd/MM/yyyy} - {lopBiTrung.ThoiGianKetThuc:dd/MM/yyyy})";
                    lblWarningCaHoc.ForeColor = Color.Red;
                    lblWarningCaHoc.Visible = true;
                    btnGhepLop.Enabled = false;
                    btnGhepLop.BackColor = Color.Gray;
                }
                else
                {
                    // ✅ KHÔNG XUNG ĐỘT
                    lblWarningCaHoc.Text = "✅ Ca học khả dụng - Có thể ghép lớp";
                    lblWarningCaHoc.ForeColor = Color.Green;
                    lblWarningCaHoc.Visible = true;
                    btnGhepLop.Enabled = true;
                    btnGhepLop.BackColor = Color.FromArgb(46, 204, 113);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi kiểm tra xung đột: {ex.Message}");
            }
        }

        // ===================================================================
        // CODE CŨ - GIỮ NGUYÊN
        // ===================================================================

        private void btnGhepLop_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ Validate selections
                if (cboHocVien.SelectedValue == null)
                {
                    MessageBox.Show(Constants.Messages.VuiLongChonHocVien,
                        Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (cboGiaSu.SelectedValue == null)
                {
                    MessageBox.Show(Constants.Messages.VuiLongChonGiaSu,
                        Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var selectedGiaSu = cboGiaSu.SelectedItem as GiaSu;
                if (selectedGiaSu == null || string.IsNullOrWhiteSpace(selectedGiaSu.ChuyenMon))
                {
                    MessageBox.Show("Gia sư chưa có chuyên môn. Vui lòng cập nhật chuyên môn cho gia sư trước.",
                        Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // ✅ Validate học phí
                decimal hocPhi = 0;
                if (!decimal.TryParse(txtHocPhi.Text, out hocPhi))
                {
                    MessageBox.Show("Học phí phải là số.",
                        Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtHocPhi.Focus();
                    return;
                }

                if (hocPhi <= 0)
                {
                    MessageBox.Show(Constants.Messages.HocPhiKhongHopLe,
                        Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtHocPhi.Focus();
                    return;
                }

                // ✅ Validate ca học
                if (cboCaHoc.SelectedItem == null)
                {
                    MessageBox.Show(Constants.Messages.VuiLongChonCaHoc,
                        Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    cboCaHoc.Focus();
                    return;
                }

                // ✅ Lấy thông tin
                int maHV = (int)cboHocVien.SelectedValue;
                int maGS = (int)cboGiaSu.SelectedValue;
                string monHoc = selectedGiaSu.ChuyenMon.Trim();
                string trangThai = cboTrangThai.SelectedItem.ToString();
                DateTime thoiGianBatDau = dtpBatDau.Value;
                DateTime thoiGianKetThuc = dtpKetThuc.Value;
                string caHoc = cboCaHoc.SelectedItem.ToString();

                // ✅ Ghép lớp với thời gian (gọi overload mới)
                _lopHocService.GhepLop(maHV, maGS, monHoc, hocPhi, trangThai,
                    thoiGianBatDau, thoiGianKetThuc, caHoc);

                // ✅ Thông báo thành công
                MessageBox.Show(
                    $"Ghép lớp thành công!\n\n" +
                    $"Môn học: {monHoc}\n" +
                    $"Học viên: {cboHocVien.Text}\n" +
                    $"Gia sư: {cboGiaSu.Text}\n" +
                    $"Ca học: {caHoc}\n" +
                    $"Thời gian: {thoiGianBatDau:dd/MM/yyyy} - {thoiGianKetThuc:dd/MM/yyyy}",
                    Constants.Messages.SuccessTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message,
                    Constants.Messages.WarningTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message,
                    Constants.Messages.WarningTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Constants.Messages.LoiKhiGhepLop}{ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmChucNangGhepLop_FormClosed(object sender, FormClosedEventArgs e)
        {
            _hocVienService?.Dispose();
            _giaSuService?.Dispose();
            _lopHocService?.Dispose();
        }
    }
}