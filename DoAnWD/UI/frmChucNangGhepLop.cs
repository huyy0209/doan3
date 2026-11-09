using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD
{
    /// <summary>
    /// Dialog Ghép Lớp - quản lý ca học tại trung tâm.
    ///
    /// - Người dùng chọn NGÀY bắt đầu / kết thúc; GIỜ lấy theo ca học
    ///   (ví dụ "Tối (18h-20h)" => 18:00 ngày bắt đầu đến 20:00 ngày kết thúc).
    /// - Ca bị trùng của gia sư HOẶC học viên được tô đỏ trong danh sách ca.
    /// - Trạng thái lớp tự tính theo thời gian, không cho chọn tay.
    /// - Mọi kiểm tra xung đột dùng chung LopHocService.TimXungDot.
    /// </summary>
    public partial class frmChucNangGhepLop : Form
    {
        private HocVienService _hocVienService;
        private GiaSuService _giaSuService;
        private LopHocService _lopHocService;

        // Cache các ca đã bận => DrawItem không phải truy vấn CSDL mỗi lần vẽ
        private HashSet<string> _caDaBan = new HashSet<string>();
        private bool _dangKhoiTao = true;

        public frmChucNangGhepLop()
        {
            InitializeComponent();
            _hocVienService = new HocVienService();
            _giaSuService = new GiaSuService();
            _lopHocService = new LopHocService();
            Load += FrmChucNangGhepLop_Load;
            FormClosed += FrmChucNangGhepLop_FormClosed;

            // Ngày học chọn theo ngày, giờ lấy theo ca
            dtpBatDau.CustomFormat = Constants.Format.DateFormat;
            dtpKetThuc.CustomFormat = Constants.Format.DateFormat;

            dtpBatDau.ValueChanged += (s, e) => CapNhatKiemTra();
            dtpKetThuc.ValueChanged += (s, e) => CapNhatKiemTra();
            cboCaHoc.SelectedIndexChanged += (s, e) => CapNhatKiemTra();

            // Hiển thị gia sư kèm chuyên môn + trạng thái
            cboGiaSu.Format += CboGiaSu_Format;

            // Custom draw cho ComboBox ca học
            cboCaHoc.DrawMode = DrawMode.OwnerDrawFixed;
            cboCaHoc.DrawItem += CboCaHoc_DrawItem;

            // Học phí: cho phép gõ "1.500.000" / "1,500,000"; tự định dạng khi rời ô
            txtHocPhi.Leave += (s, e) =>
            {
                decimal hp;
                if (TryParseHocPhi(txtHocPhi.Text, out hp))
                    txtHocPhi.Text = hp.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
            };
        }

        private void FrmChucNangGhepLop_Load(object sender, EventArgs e)
        {
            try
            {
                cboHocVien.DataSource = _hocVienService.GetAll().OrderBy(h => h.HoTen).ToList();
                cboHocVien.DisplayMember = "HoTen";
                cboHocVien.ValueMember = "MaHV";

                // Mọi gia sư trừ "Tạm nghỉ" - xung đột được kiểm tra theo ca
                cboGiaSu.DataSource = _giaSuService.GetForGhepLop().ToList();
                cboGiaSu.DisplayMember = "HoTen";
                cboGiaSu.ValueMember = "MaGS";

                cboCaHoc.DataSource = Constants.CaHoc.TatCaCaHoc.ToList();

                // Trạng thái chỉ để xem - tính tự động theo thời gian
                cboTrangThai.Items.Clear();
                cboTrangThai.Items.AddRange(new object[]
                {
                    Constants.LopHocStatus.ChuaBatDau,
                    Constants.LopHocStatus.DangDienRa
                });
                cboTrangThai.Enabled = false;

                dtpBatDau.MinDate = DateTime.Now.Date;
                dtpBatDau.Value = DateTime.Now.Date.AddDays(1);
                dtpKetThuc.Value = dtpBatDau.Value.AddMonths(1);

                if (cboHocVien.Items.Count == 0 || cboGiaSu.Items.Count == 0)
                {
                    lblWarningCaHoc.Text = cboHocVien.Items.Count == 0
                        ? "⚠️ Chưa có học viên - hãy thêm học viên trước."
                        : "⚠️ Không có gia sư nào nhận lớp (tất cả đang tạm nghỉ).";
                    lblWarningCaHoc.ForeColor = Color.Red;
                    lblWarningCaHoc.Visible = true;
                    DatTrangThaiNutGhep(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo: {ex.Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _dangKhoiTao = false;
                CapNhatKiemTra();
            }
        }

        // ===================================================================
        // CHỌN HỌC VIÊN / GIA SƯ
        // ===================================================================

        private void cboHocVien_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = cboHocVien.SelectedItem as HocVien;
            if (selected != null)
            {
                txtSDT_HV.Text = selected.SDT ?? "";
                txtDiaChi_HV.Text = selected.DiaChi ?? "";
            }
            CapNhatKiemTra();
        }

        private void cboGiaSu_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = cboGiaSu.SelectedItem as GiaSu;
            if (selected != null)
            {
                txtSDT_GS.Text = selected.SDT ?? "";
                txtChuyenMon_GS.Text = selected.ChuyenMon ?? "";
                txtChuyenMon_GS.BackColor = Constants.Colors.ActiveOrange;
                txtChuyenMon_GS.ForeColor = Constants.Colors.Orange;

                HienThiLichGiaSu(selected.MaGS);
            }
            CapNhatKiemTra();
        }

        private void CboGiaSu_Format(object sender, ListControlConvertEventArgs e)
        {
            var gs = e.ListItem as GiaSu;
            if (gs != null)
                e.Value = $"{gs.HoTen} - {gs.ChuyenMon} ({gs.TrangThai ?? Constants.GiaSuStatus.Ranh})";
        }

        /// <summary>
        /// Hiển thị lịch dạy hiện tại của gia sư
        /// </summary>
        private void HienThiLichGiaSu(int maGS)
        {
            try
            {
                lstLichGiaSu.Items.Clear();

                var lichHoc = _lopHocService.GetAll()
                    .Where(l => l.MaGS == maGS && l.DangHoatDong)
                    .OrderBy(l => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, l.CaHoc))
                    .ThenBy(l => l.ThoiGianBatDau)
                    .ToList();

                if (lichHoc.Count == 0)
                {
                    lstLichGiaSu.Items.Add("✅ Gia sư hiện đang rảnh - chưa có lớp nào!");
                    lstLichGiaSu.ForeColor = Color.Green;
                    lblTitleLichGS.Text = "Gia sư rảnh - có thể ghép lớp bất kỳ ca nào";
                    lblTitleLichGS.ForeColor = Color.Green;
                    return;
                }

                lstLichGiaSu.Items.Add($"📚 Đang có {lichHoc.Count} lớp:");
                lstLichGiaSu.Items.Add(new string('─', 65));

                foreach (var lop in lichHoc)
                {
                    string trangThai = lop.TrangThaiTheoThoiGian;
                    string icon = trangThai == Constants.LopHocStatus.DangDienRa ? "🟢" : "🔵";

                    lstLichGiaSu.Items.Add(
                        $"{icon} {lop.CaHoc,-20} | {lop.MonHoc,-10} | " +
                        $"{lop.ThoiGianBatDau:dd/MM/yyyy} - {lop.ThoiGianKetThuc:dd/MM/yyyy} | [{trangThai}]");
                }

                lstLichGiaSu.ForeColor = Constants.Colors.DarkGray;
                lblTitleLichGS.Text = $"Gia sư có {lichHoc.Count} lớp - chọn ca còn trống (màu xanh)";
                lblTitleLichGS.ForeColor = Color.OrangeRed;
            }
            catch (Exception ex)
            {
                lstLichGiaSu.Items.Clear();
                lstLichGiaSu.Items.Add($"❌ Lỗi khi load lịch: {ex.Message}");
                lstLichGiaSu.ForeColor = Color.Red;
            }
        }

        // ===================================================================
        // THỜI GIAN THEO CA
        // ===================================================================

        /// <summary>
        /// Ghép ngày đã chọn với giờ của ca học
        /// </summary>
        private void LayKhoangThoiGian(out DateTime batDau, out DateTime ketThuc)
        {
            int gioBD, gioKT;
            string ca = cboCaHoc.SelectedItem as string;

            if (Constants.CaHoc.TryGetGio(ca, out gioBD, out gioKT))
            {
                batDau = dtpBatDau.Value.Date.AddHours(gioBD);
                ketThuc = dtpKetThuc.Value.Date.AddHours(gioKT);
            }
            else
            {
                batDau = dtpBatDau.Value.Date;
                ketThuc = dtpKetThuc.Value.Date.AddDays(1).AddTicks(-1);
            }
        }

        // ===================================================================
        // KIỂM TRA XUNG ĐỘT REAL-TIME
        // ===================================================================

        /// <summary>
        /// Tính lại cache ca bận + cảnh báo xung đột + trạng thái dự kiến
        /// </summary>
        private void CapNhatKiemTra()
        {
            if (_dangKhoiTao) return;

            if (!(cboGiaSu.SelectedValue is int) || !(cboHocVien.SelectedValue is int))
            {
                _caDaBan.Clear();
                cboCaHoc.Invalidate();
                return;
            }

            int maGS = (int)cboGiaSu.SelectedValue;
            int maHV = (int)cboHocVien.SelectedValue;
            DateTime batDau, ketThuc;
            LayKhoangThoiGian(out batDau, out ketThuc);

            try
            {
                _caDaBan = _lopHocService.GetCaDaBan(maGS, maHV, batDau, ketThuc);
                cboCaHoc.Invalidate();

                // Trạng thái dự kiến của lớp mới
                var lopTam = new LopHoc { ThoiGianBatDau = batDau, ThoiGianKetThuc = ketThuc };
                cboTrangThai.SelectedItem = lopTam.TrangThaiTheoThoiGian;

                string loi = null;
                if (batDau >= ketThuc)
                    loi = Constants.Messages.ThoiGianKhongHopLe;
                else if (cboCaHoc.SelectedItem != null)
                {
                    var gs = cboGiaSu.SelectedItem as GiaSu;
                    loi = _lopHocService.TimXungDot(maGS, maHV, gs?.ChuyenMon,
                        cboCaHoc.SelectedItem.ToString(), batDau, ketThuc);
                }

                if (loi != null)
                {
                    lblWarningCaHoc.Text = "⚠️ " + loi;
                    lblWarningCaHoc.ForeColor = Color.Red;
                    DatTrangThaiNutGhep(false);
                }
                else
                {
                    int soNgay = (ketThuc.Date - batDau.Date).Days + 1;
                    lblWarningCaHoc.Text = $"✅ Ca học khả dụng - {soNgay} ngày, {batDau:HH:mm}-{ketThuc:HH:mm}";
                    lblWarningCaHoc.ForeColor = Color.Green;
                    DatTrangThaiNutGhep(true);
                }
                lblWarningCaHoc.Visible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi kiểm tra xung đột: {ex.Message}");
            }
        }

        private void DatTrangThaiNutGhep(bool choPhep)
        {
            btnGhepLop.Enabled = choPhep;
            btnGhepLop.BackColor = choPhep ? Constants.Colors.Green : Constants.Colors.Gray;
        }

        /// <summary>
        /// Vẽ ca học: đỏ gạch ngang = đã bận, xanh = còn trống
        /// </summary>
        private void CboCaHoc_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string caHoc = cboCaHoc.Items[e.Index].ToString();
            bool isBusy = _caDaBan.Contains(caHoc);
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color backgroundColor = isBusy ? Color.FromArgb(255, 220, 220) : Constants.Colors.ActiveGreen;
            Color textColor = isBusy ? Color.FromArgb(192, 57, 43) : Color.FromArgb(39, 174, 96);
            if (isSelected)
                backgroundColor = ControlPaint.Dark(backgroundColor, 0.05f);

            using (var bgBrush = new SolidBrush(backgroundColor))
                e.Graphics.FillRectangle(bgBrush, e.Bounds);

            using (var textBrush = new SolidBrush(textColor))
            using (var font = new Font(e.Font, isBusy ? FontStyle.Strikeout : FontStyle.Regular))
            {
                string prefix = isBusy ? "❌ " : "✅ ";
                e.Graphics.DrawString(prefix + caHoc, font, textBrush, e.Bounds.Left + 2, e.Bounds.Top + 2);
            }

            e.DrawFocusRectangle();
        }

        // ===================================================================
        // GHÉP LỚP
        // ===================================================================

        private static bool TryParseHocPhi(string text, out decimal hocPhi)
        {
            hocPhi = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;

            string sach = new string(text.Where(char.IsDigit).ToArray());
            return sach.Length > 0 && sach.Length <= 12 && decimal.TryParse(sach, out hocPhi);
        }

        private void btnGhepLop_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(cboHocVien.SelectedValue is int))
                {
                    CanhBao(Constants.Messages.VuiLongChonHocVien, cboHocVien);
                    return;
                }

                var selectedGiaSu = cboGiaSu.SelectedItem as GiaSu;
                if (selectedGiaSu == null)
                {
                    CanhBao(Constants.Messages.VuiLongChonGiaSu, cboGiaSu);
                    return;
                }

                if (string.IsNullOrWhiteSpace(selectedGiaSu.ChuyenMon))
                {
                    CanhBao("Gia sư chưa có chuyên môn. Vui lòng cập nhật chuyên môn cho gia sư trước.", cboGiaSu);
                    return;
                }

                decimal hocPhi;
                if (!TryParseHocPhi(txtHocPhi.Text, out hocPhi))
                {
                    CanhBao("Học phí phải là số (ví dụ: 1.500.000).", txtHocPhi);
                    return;
                }

                if (cboCaHoc.SelectedItem == null)
                {
                    CanhBao(Constants.Messages.VuiLongChonCaHoc, cboCaHoc);
                    return;
                }

                int maHV = (int)cboHocVien.SelectedValue;
                int maGS = selectedGiaSu.MaGS;
                string monHoc = selectedGiaSu.ChuyenMon.Trim();
                string caHoc = cboCaHoc.SelectedItem.ToString();
                DateTime batDau, ketThuc;
                LayKhoangThoiGian(out batDau, out ketThuc);

                var lop = _lopHocService.GhepLop(maHV, maGS, monHoc, hocPhi, null, batDau, ketThuc, caHoc);

                MessageBox.Show(
                    "Ghép lớp thành công!\n\n" +
                    $"Môn học: {monHoc}\n" +
                    $"Học viên: {cboHocVien.Text}\n" +
                    $"Gia sư: {selectedGiaSu.HoTen}\n" +
                    $"Ca học: {caHoc}\n" +
                    $"Thời gian: {batDau:dd/MM/yyyy} - {ketThuc:dd/MM/yyyy}\n" +
                    $"Học phí: {hocPhi:N0} đ\n" +
                    $"Trạng thái: {lop.TrangThai}",
                    Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CapNhatKiemTra();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Constants.Messages.LoiKhiGhepLop}{ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CanhBao(string message, Control focus)
        {
            MessageBox.Show(message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            focus?.Focus();
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FrmChucNangGhepLop_FormClosed(object sender, FormClosedEventArgs e)
        {
            _hocVienService?.Dispose();
            _giaSuService?.Dispose();
            _lopHocService?.Dispose();
        }
    }
}
