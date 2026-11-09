using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;

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
            ThemeManager.Apply(this);
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
            cboCaHoc.ItemHeight = 22;
            cboCaHoc.DrawItem += CboCaHoc_DrawItem;

            // Lịch dạy của gia sư: mỗi dòng một lớp, chấm màu theo ca
            lstLichGiaSu.DrawMode = DrawMode.OwnerDrawFixed;
            lstLichGiaSu.ItemHeight = 22;
            lstLichGiaSu.Font = Theme.Body;
            lstLichGiaSu.DrawItem += LstLichGiaSu_DrawItem;

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
                        ? "Chưa có học viên. Thêm học viên trước khi ghép lớp."
                        : "Không có gia sư nào nhận lớp: tất cả đang tạm nghỉ.";
                    lblWarningCaHoc.ForeColor = Theme.Margin;
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
                txtChuyenMon_GS.BackColor = Theme.InkSoft;
                txtChuyenMon_GS.ForeColor = Theme.Ink;

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
                    lstLichGiaSu.Items.Add("Chưa có lớp nào. Có thể ghép ở bất kỳ ca nào.");
                    lblTitleLichGS.Text = "Gia sư đang rảnh";
                    lblTitleLichGS.ForeColor = Theme.Success;
                    return;
                }

                foreach (var lop in lichHoc)
                    lstLichGiaSu.Items.Add(lop);

                lblTitleLichGS.Text = $"Đang dạy {lichHoc.Count} lớp";
                lblTitleLichGS.ForeColor = Theme.TextMuted;
            }
            catch (Exception ex)
            {
                lstLichGiaSu.Items.Clear();
                lstLichGiaSu.Items.Add($"Không đọc được lịch dạy: {ex.Message}");
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
                    lblWarningCaHoc.Text = loi;
                    lblWarningCaHoc.ForeColor = Theme.Margin;
                    DatTrangThaiNutGhep(false);
                }
                else
                {
                    int soNgay = (ketThuc.Date - batDau.Date).Days + 1;
                    lblWarningCaHoc.Text = $"Ca còn trống: {soNgay} ngày, {batDau:HH:mm}–{ketThuc:HH:mm} mỗi ngày";
                    lblWarningCaHoc.ForeColor = Theme.Success;
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
        }

        /// <summary>
        /// Vẽ ca học: chấm màu theo ca; ca đã có lớp thì chữ mờ, gạch ngang, ghi chú "đã có lớp"
        /// </summary>
        private void CboCaHoc_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string caHoc = cboCaHoc.Items[e.Index].ToString();
            bool isBusy = _caDaBan.Contains(caHoc);
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool inEditBox = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;

            var g = e.Graphics;
            using (var bg = new SolidBrush(isSelected && !inEditBox ? Theme.InkSoft : Theme.Surface))
                g.FillRectangle(bg, e.Bounds);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var dot = new SolidBrush(isBusy ? Theme.Rule : Theme.ShiftColor(caHoc)))
                g.FillEllipse(dot, e.Bounds.Left + 6, e.Bounds.Top + e.Bounds.Height / 2f - 4, 8, 8);

            var textRect = new Rectangle(e.Bounds.Left + 20, e.Bounds.Top, e.Bounds.Width - 24, e.Bounds.Height);
            var font = isBusy ? new Font(Theme.Body, FontStyle.Strikeout) : Theme.Body;
            TextRenderer.DrawText(g, caHoc, font, textRect, isBusy ? Theme.TextFaint : Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            if (isBusy)
            {
                font.Dispose();
                TextRenderer.DrawText(g, "đã có lớp", Theme.Small, textRect, Theme.Margin,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }

        /// <summary>
        /// Một dòng lịch dạy: chấm màu ca, tên ca, môn, khoảng ngày, trạng thái
        /// </summary>
        private void LstLichGiaSu_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var g = e.Graphics;
            using (var bg = new SolidBrush(Theme.Surface)) g.FillRectangle(bg, e.Bounds);

            var item = lstLichGiaSu.Items[e.Index];
            var lop = item as LopHoc;
            var b = e.Bounds;

            if (lop == null)
            {
                TextRenderer.DrawText(g, item.ToString(), Theme.Body, b, Theme.TextMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                return;
            }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var dot = new SolidBrush(Theme.ShiftColor(lop.CaHoc)))
                g.FillEllipse(dot, b.Left + 2, b.Top + b.Height / 2f - 4, 8, 8);

            var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
            TextRenderer.DrawText(g, lop.CaHoc, Theme.BodyStrong, new Rectangle(b.Left + 16, b.Top, 170, b.Height), Theme.Text, flags);
            TextRenderer.DrawText(g, lop.MonHoc, Theme.Body, new Rectangle(b.Left + 190, b.Top, 130, b.Height), Theme.Text, flags);
            TextRenderer.DrawText(g, $"{lop.ThoiGianBatDau:dd/MM} – {lop.ThoiGianKetThuc:dd/MM/yyyy}", Theme.Body,
                new Rectangle(b.Left + 325, b.Top, 190, b.Height), Theme.TextMuted, flags);

            string tt = lop.TrangThaiTheoThoiGian;
            TextRenderer.DrawText(g, tt, Theme.SmallStrong, new Rectangle(b.Left + 520, b.Top, b.Width - 520, b.Height),
                Theme.StatusColor(tt), flags);
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
