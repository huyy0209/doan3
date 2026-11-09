using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Dto;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;

namespace DoAnWD
{
    /// <summary>
    /// Học phí: công nợ từng lớp (học phí, đã đóng, còn nợ) + ghi nhận các lần phụ huynh đóng tiền.
    /// </summary>
    public class frmHocPhi : Form, ITuGiaoDien
    {
        private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
        private readonly HocPhiService _service = new HocPhiService();
        private List<CongNoLop> _congNo = new List<CongNoLop>();

        private Label lblTong, lblDaThu, lblConNo;
        private TextBox txtTim;
        private ComboBox cboTinhTrang;
        private DataGridView dgv, dgvLichSu;
        private Pager pager;
        private Label lblLopChon;
        private TextBox txtSoTien, txtGhiChu;
        private DateTimePicker dtpNgay;
        private ComboBox cboHinhThuc;
        private ThemedButton btnGhiNhan, btnXoaTT;

        public frmHocPhi()
        {
            BackColor = Theme.Paper;
            Font = Theme.Body;
            FormBorderStyle = FormBorderStyle.None;
            Padding = new Padding(20, 14, 20, 16);
            Text = "Học phí";
            BuildUi();
            Load += (s, e) => NapDuLieu();
        }

        private Control O(string nhan, Label so, Color mau)
        {
            so.Font = Theme.NumberSmall;
            so.ForeColor = mau;
            so.AutoSize = true;
            so.Margin = new Padding(0);
            var p = new Panel { BackColor = Theme.Blend(mau, Color.White, 0.9f), Height = 78, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(16, 10, 10, 8) };
            p.Paint += (s, e) => { using (var b = new SolidBrush(mau)) e.Graphics.FillRectangle(b, 0, 0, 5, p.Height); };
            var l = new Label { Text = nhan, Font = Theme.Label, ForeColor = Theme.Text, AutoSize = true, Location = new Point(16, 8), BackColor = Color.Transparent };
            so.Location = new Point(14, 32);
            so.BackColor = Color.Transparent;
            p.Controls.Add(l);
            p.Controls.Add(so);
            return p;
        }

        private void BuildUi()
        {
            lblTong = new Label(); lblDaThu = new Label(); lblConNo = new Label();
            var tiles = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, Height = 82, Margin = new Padding(0, 6, 0, 10) };
            for (int i = 0; i < 3; i++) tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            tiles.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            tiles.Controls.Add(O("Tổng học phí các lớp", lblTong, Theme.Ink), 0, 0);
            tiles.Controls.Add(O("Đã thu", lblDaThu, Theme.Success), 1, 0);
            var oNo = O("Còn phải thu", lblConNo, Theme.Warning);
            oNo.Margin = new Padding(0);
            tiles.Controls.Add(oNo, 2, 0);

            // ---- Danh sách công nợ ----
            txtTim = Ui.Input("Tìm học viên, SĐT, môn, gia sư...");
            txtTim.TextChanged += (s, e) => ApDungLoc();
            cboTinhTrang = Ui.Combo(false, 230);
            cboTinhTrang.Items.AddRange(new object[] { "Tất cả", CanThu, HocPhiService.ConNo, HocPhiService.ChuaDong, HocPhiService.DaDu });
            cboTinhTrang.SelectedIndex = 1; // mặc định chỉ hiện lớp còn phải thu => ít dòng hơn
            cboTinhTrang.SelectedIndexChanged += (s, e) => ApDungLoc();
            var loc = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            loc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            loc.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtTim.Margin = new Padding(0, 2, 10, 4);
            loc.Controls.Add(txtTim, 0, 0);
            loc.Controls.Add(cboTinhTrang, 1, 0);

            dgv = Ui.Grid();
            dgv.SelectionChanged += (s, e) => HienLopChon();
            var btnXuat = Ui.Button("Xuất CSV", ButtonKind.Ghost, Glyph.None, 110);
            btnXuat.Click += (s, e) => XuatCsv();
            pager = new Pager { PageSize = 50 };
            pager.PageChanged += (s, e) => ApDungLoc(null, false);
            var chan = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            chan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            chan.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            chan.Controls.Add(pager, 0, 0);
            chan.Controls.Add(btnXuat, 1, 0);

            var secDs = Ui.Section("Công nợ theo lớp");
            var st = Ui.Stack(false, loc, dgv, chan);
            st.RowStyles[1] = new RowStyle(SizeType.Percent, 100);
            dgv.Dock = DockStyle.Fill;
            secDs.Controls.Add(st);

            // ---- Ghi nhận đóng tiền ----
            lblLopChon = new Label { Font = Theme.BodyStrong, ForeColor = Theme.Ink, AutoSize = true, MaximumSize = new Size(380, 0), Margin = new Padding(0, 0, 0, 4) };
            txtSoTien = Ui.Input("Số tiền (đ)");
            txtSoTien.Leave += (s, e) => { decimal v; if (TryTien(txtSoTien.Text, out v)) txtSoTien.Text = v.ToString("N0", Vi); };
            dtpNgay = Ui.Date(150);
            dtpNgay.MaxDate = DateTime.Now.Date;
            dtpNgay.Value = DateTime.Now.Date;
            cboHinhThuc = Ui.Combo(true);
            cboHinhThuc.Items.AddRange(Constants.GoiY.HinhThucThanhToan);
            cboHinhThuc.SelectedIndex = 0;
            txtGhiChu = Ui.Input("Ví dụ: Đóng đợt 2");
            btnGhiNhan = Ui.Button("Ghi nhận đóng tiền", ButtonKind.Primary, Glyph.Money);
            btnGhiNhan.Click += (s, e) => GhiNhan();
            dgvLichSu = Ui.Grid();
            dgvLichSu.DataBindingComplete += (s, e) =>
            {
                if (dgvLichSu.Columns.Count == 0) return;
                dgvLichSu.Columns["MaTT"].Visible = false;
                dgvLichSu.Columns["NgayDong"].HeaderText = "Ngày";
                dgvLichSu.Columns["NgayDong"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvLichSu.Columns["SoTien"].HeaderText = "Số tiền";
                dgvLichSu.Columns["SoTien"].DefaultCellStyle.Format = "N0";
                dgvLichSu.Columns["HinhThuc"].HeaderText = "Hình thức";
                dgvLichSu.Columns["GhiChu"].HeaderText = "Ghi chú";
            };
            btnXoaTT = Ui.Button("Xóa lần đóng đã chọn", ButtonKind.Danger, Glyph.Trash);
            btnXoaTT.Click += (s, e) => XoaThanhToan();

            var secTT = Ui.Section("Đóng học phí");
            secTT.Margin = new Padding(0);
            var st2 = Ui.Stack(false, lblLopChon,
                Ui.FieldLabel("Số tiền"), txtSoTien,
                Ui.FieldLabel("Ngày đóng"), dtpNgay,
                Ui.FieldLabel("Hình thức"), cboHinhThuc,
                Ui.FieldLabel("Ghi chú"), txtGhiChu,
                btnGhiNhan,
                Ui.FieldLabel("Các lần đã đóng"), dgvLichSu, btnXoaTT);
            st2.RowStyles[11] = new RowStyle(SizeType.Percent, 100);
            dgvLichSu.Dock = DockStyle.Fill;
            secTT.Controls.Add(st2);

            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            cot.Controls.Add(secDs, 0, 0);
            cot.Controls.Add(secTT, 1, 0);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(Ui.Title("Học phí"), 0, 0);
            root.Controls.Add(tiles, 0, 1);
            root.Controls.Add(cot, 0, 2);
            Controls.Add(root);
        }

        private static bool TryTien(string text, out decimal v)
        {
            v = 0;
            string so = new string((text ?? "").Where(char.IsDigit).ToArray());
            return so.Length > 0 && so.Length <= 12 && decimal.TryParse(so, out v) && v > 0;
        }

        private CongNoLop LopChon
        {
            get
            {
                if (dgv.CurrentRow == null || !dgv.Columns.Contains("MaLop")) return null;
                int ma = (int)dgv.CurrentRow.Cells["MaLop"].Value;
                return _congNo.FirstOrDefault(c => c.MaLop == ma);
            }
        }

        private void NapDuLieu(int? chonMaLop = null)
        {
            try
            {
                chonMaLop = chonMaLop ?? LopChon?.MaLop;
                _congNo = _service.GetCongNo();
                var tinh = _congNo.Where(c => c.TrangThaiLop != Constants.LopHocStatus.DaHuy).ToList();
                lblTong.Text = tinh.Sum(c => c.HocPhi).ToString("N0", Vi) + " đ";
                lblDaThu.Text = _congNo.Sum(c => c.DaDong).ToString("N0", Vi) + " đ";
                lblConNo.Text = tinh.Sum(c => c.ConNo).ToString("N0", Vi) + " đ";
                ApDungLoc(chonMaLop);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tải được học phí: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private const string CanThu = "Cần thu (còn nợ + chưa đóng)";

        private void ApDungLoc(int? chonMaLop = null, bool veTrangDau = true)
        {
            if (veTrangDau) pager.Reset();
            string tt = cboTinhTrang.SelectedIndex > 0 ? cboTinhTrang.SelectedItem as string : null;
            var ds = _congNo
                .Where(c => tt == null || (tt == CanThu ? c.TinhTrang != HocPhiService.DaDu : c.TinhTrang == tt))
                .Where(c => TimKiem.Khop(txtTim.Text, c.HocVien, c.SDTLienHe, c.MonHoc, c.GiaSu))
                .Select(c => new { c.MaLop, c.HocVien, SDT = c.SDTLienHe, c.MonHoc, c.GiaSu, c.HocPhi, DaDong = c.DaDong, ConNo = c.ConNo, c.TinhTrang })
                .ToList();
            dgv.DataSource = pager.Cat(ds);
            if (dgv.Columns.Count > 0)
            {
                dgv.Columns["MaLop"].Visible = false;
                dgv.Columns["HocVien"].HeaderText = "Học viên";
                dgv.Columns["SDT"].HeaderText = "SĐT liên hệ";
                dgv.Columns["MonHoc"].HeaderText = "Môn";
                dgv.Columns["GiaSu"].HeaderText = "Gia sư";
                dgv.Columns["GiaSu"].Visible = false; // bớt cột cho dễ đọc; tìm kiếm vẫn theo tên gia sư
                dgv.Columns["HocPhi"].HeaderText = "Học phí";
                foreach (var c in new[] { "DaDong", "ConNo" })
                {
                    dgv.Columns[c].DefaultCellStyle.Format = "N0";
                    dgv.Columns[c].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                dgv.Columns["DaDong"].HeaderText = "Đã đóng";
                dgv.Columns["ConNo"].HeaderText = "Còn nợ";
                dgv.Columns["ConNo"].DefaultCellStyle.ForeColor = Theme.Warning;
                dgv.Columns["ConNo"].DefaultCellStyle.Font = Theme.BodyStrong;
                dgv.Columns["TinhTrang"].HeaderText = "Tình trạng";
            }
            if (chonMaLop.HasValue)
                foreach (DataGridViewRow r in dgv.Rows)
                    if ((int)r.Cells["MaLop"].Value == chonMaLop.Value) { dgv.CurrentCell = r.Cells["HocVien"]; break; }
            HienLopChon();
        }

        private void HienLopChon()
        {
            var c = LopChon;
            bool co = c != null;
            foreach (Control x in new Control[] { txtSoTien, dtpNgay, cboHinhThuc, txtGhiChu, btnGhiNhan, btnXoaTT }) x.Enabled = co;
            if (!co)
            {
                lblLopChon.Text = "Chọn một lớp ở danh sách bên trái.";
                dgvLichSu.DataSource = null;
                return;
            }

            lblLopChon.Text = $"{c.HocVien}, {c.MonHoc}: còn nợ {c.ConNo.ToString("N0", Vi)} đ";
            txtSoTien.Text = c.ConNo > 0 ? c.ConNo.ToString("N0", Vi) : "";
            btnGhiNhan.Enabled = c.ConNo > 0;

            dgvLichSu.DataSource = _service.GetLichSu(c.MaLop)
                .Select(t => new { t.MaTT, t.NgayDong, t.SoTien, t.HinhThuc, t.GhiChu }).ToList();
            btnXoaTT.Enabled = dgvLichSu.Rows.Count > 0;
        }

        private void GhiNhan()
        {
            var c = LopChon;
            decimal tien;
            if (c == null) return;
            if (!TryTien(txtSoTien.Text, out tien))
            {
                MessageBox.Show("Nhập số tiền, ví dụ 750.000.", Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoTien.Focus();
                return;
            }
            try
            {
                _service.ThemThanhToan(c.MaLop, tien, dtpNgay.Value.Date.Add(DateTime.Now.TimeOfDay), cboHinhThuc.Text, txtGhiChu.Text);
                txtGhiChu.Clear();
                NapDuLieu(c.MaLop);
                MessageBox.Show($"Đã ghi nhận {tien.ToString("N0", Vi)} đ cho {c.HocVien}.", Constants.Messages.SuccessTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không lưu được: {ex.GetBaseException().Message}", Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void XoaThanhToan()
        {
            if (dgvLichSu.CurrentRow == null) return;
            int maTT = (int)dgvLichSu.CurrentRow.Cells["MaTT"].Value;
            decimal tien = (decimal)dgvLichSu.CurrentRow.Cells["SoTien"].Value;
            if (MessageBox.Show($"Xóa lần đóng {tien.ToString("N0", Vi)} đ? Dùng khi ghi nhầm.", Constants.Messages.ConfirmTitle,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                _service.XoaThanhToan(maTT);
                NapDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().Message, Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void XuatCsv()
        {
            using (var dlg = new SaveFileDialog { Filter = "CSV (Excel)|*.csv", FileName = $"HocPhi_{DateTime.Now:yyyyMMdd}.csv" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    new BaoCaoService().XuatCsv(dgv, dlg.FileName);
                    MessageBox.Show("Đã xuất danh sách công nợ.", Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không xuất được: {ex.Message}", Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
