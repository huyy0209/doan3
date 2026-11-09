using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.BLL.Validators;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;

namespace DoAnWD
{
    /// <summary>
    /// Gia sư: danh sách có tìm kiếm + lọc (môn, trạng thái, ca nhận dạy) + phân trang bên trái,
    /// hồ sơ chi tiết bên phải (email, học vấn, kinh nghiệm, ca nhận dạy, tạm nghỉ, ghi chú).
    /// </summary>
    public partial class frmGiaSu : Form, ITuGiaoDien
    {
        private List<GiaSu> _giaSus = new List<GiaSu>();
        private List<LopHoc> _lopHoatDong = new List<LopHoc>();
        private Dictionary<int, TaiKhoanRow> _taiKhoan = new Dictionary<int, TaiKhoanRow>();
        private int? _maDangSua;

        private TextBox txtTim;
        private ComboBox cboLocMon, cboLocTrangThai, cboLocCa;
        private DataGridView dgv;
        private Label lblDem;
        private Pager pager;

        private Label lblTieuDeForm, lblLich, lblTaiKhoan;
        private TextBox txtHoTen, txtSDT, txtEmail, txtHocVan, txtGhiChu;
        private ComboBox cboChuyenMon;
        private NumericUpDown nudKinhNghiem;
        private CheckBox[] chkCa;
        private CheckBox chkTamNghi;
        private ThemedButton btnThemMoi, btnLuu, btnXoa;

        public frmGiaSu()
        {
            InitializeComponent();
            BuildUi();
            Load += (s, e) => { NapDuLieu(); BatDauThemMoi(); };
        }

        // ===================================================================
        // GIAO DIỆN
        // ===================================================================

        private void BuildUi()
        {
            BackColor = Theme.Paper;
            Font = Theme.Body;
            Padding = new Padding(20, 14, 20, 16);

            // ---- Danh sách ----
            txtTim = Ui.Input("Tìm tên, SĐT, email, môn, học vấn... (không cần dấu)");
            txtTim.TextChanged += (s, e) => ApDungLoc();
            cboLocMon = Ui.Combo(false, 125);
            cboLocTrangThai = Ui.Combo(false, 130);
            cboLocTrangThai.Items.Add("Mọi trạng thái");
            cboLocTrangThai.Items.AddRange(Constants.GiaSuStatus.TatCa);
            cboLocTrangThai.SelectedIndex = 0;
            cboLocCa = Ui.Combo(false, 140);
            cboLocCa.Items.Add("Nhận mọi ca");
            cboLocCa.Items.AddRange(Constants.CaHoc.TatCaCaHoc.Select(c => (object)("Nhận ca " + Constants.CaHoc.TenNgan(c))).ToArray());
            cboLocCa.SelectedIndex = 0;
            foreach (var c in new[] { cboLocMon, cboLocTrangThai, cboLocCa })
                c.SelectedIndexChanged += (s, e) => ApDungLoc();

            var loc = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true, Margin = new Padding(0) };
            loc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 3; i++) loc.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtTim.Margin = new Padding(0, 2, 8, 4);
            cboLocMon.Margin = cboLocTrangThai.Margin = new Padding(0, 2, 8, 4);
            cboLocCa.Margin = new Padding(0, 2, 0, 4);
            loc.Controls.Add(txtTim, 0, 0);
            loc.Controls.Add(cboLocMon, 1, 0);
            loc.Controls.Add(cboLocTrangThai, 2, 0);
            loc.Controls.Add(cboLocCa, 3, 0);

            dgv = Ui.Grid();
            dgv.SelectionChanged += (s, e) => HienHoSoDangChon();
            lblDem = Ui.Text("", true);
            pager = new Pager { PageSize = 50 };
            pager.PageChanged += (s, e) => ApDungLoc(null, false);
            var chanDs = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            chanDs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            chanDs.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            chanDs.Controls.Add(lblDem, 0, 0);
            chanDs.Controls.Add(pager, 1, 0);

            var secDs = Ui.Section("Danh sách gia sư");
            var stDs = Ui.Stack(false, loc, dgv, chanDs);
            stDs.RowStyles[1] = new RowStyle(SizeType.Percent, 100);
            dgv.Dock = DockStyle.Fill;
            secDs.Controls.Add(stDs);

            // ---- Hồ sơ ----
            lblTieuDeForm = new Label { Font = Theme.Section, ForeColor = Theme.Ink, AutoSize = true, Margin = new Padding(0, 0, 0, 4) };
            txtHoTen = Ui.Input("Bắt buộc");
            txtSDT = Ui.Input("Bắt buộc, ví dụ 0909123456");
            cboChuyenMon = Ui.Combo(true);
            txtEmail = Ui.Input("ten@gmail.com");
            txtHocVan = Ui.Input("Ví dụ: SV năm 4 HUTECH, Kiện tướng quốc gia");
            nudKinhNghiem = new NumericUpDown { Minimum = 0, Maximum = 60, Width = 90, Font = Theme.Body, Margin = new Padding(0, 0, 0, 4) };
            txtGhiChu = Ui.Input();
            txtGhiChu.Multiline = true;
            txtGhiChu.Height = 56;
            txtGhiChu.ScrollBars = ScrollBars.Vertical;

            // Ca nhận dạy: bỏ chọn hết = nhận mọi ca
            var cas = Constants.CaHoc.TatCaCaHoc;
            chkCa = new CheckBox[cas.Length];
            var luoiCa = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            for (int i = 0; i < 3; i++) luoiCa.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            for (int i = 0; i < cas.Length; i++)
            {
                chkCa[i] = new CheckBox
                {
                    Text = Constants.CaHoc.TenNgan(cas[i]), Tag = cas[i], AutoSize = true, Font = Theme.Body,
                    ForeColor = Theme.Blend(Theme.ShiftColor(cas[i]), Theme.Text, 0.35f), Margin = new Padding(0, 2, 6, 2)
                };
                luoiCa.Controls.Add(chkCa[i], i % 3, i / 3);
            }

            chkTamNghi = new CheckBox
            {
                Text = "Tạm nghỉ (không nhận lớp mới)", AutoSize = true, Font = Theme.Body, ForeColor = Theme.Text,
                Margin = new Padding(0, 8, 0, 2)
            };
            lblLich = Ui.Text("", true);
            lblLich.MaximumSize = new Size(440, 0);
            lblTaiKhoan = Ui.Text("", true);

            btnThemMoi = Ui.Button("Thêm gia sư", ButtonKind.Secondary, Glyph.Plus);
            btnThemMoi.Click += (s, e) => BatDauThemMoi();
            btnLuu = Ui.Button("Thêm", ButtonKind.Primary, Glyph.Check, 110);
            btnLuu.Click += (s, e) => Luu();
            btnXoa = Ui.Button("Xóa", ButtonKind.Danger, Glyph.Trash, 96);
            btnXoa.Click += (s, e) => Xoa();

            var hai = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Margin = new Padding(0) };
            hai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            hai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            Action<int, int, string, Control> o = (col, row, nhan, ctl) =>
            {
                var st = Ui.Stack(false, Ui.FieldLabel(nhan), ctl);
                st.AutoSize = true;
                st.Dock = DockStyle.Top;
                st.Margin = new Padding(0, 0, col == 0 ? 10 : 0, 0);
                hai.Controls.Add(st, col, row);
            };
            o(0, 0, "Số điện thoại *", txtSDT);
            o(1, 0, "Chuyên môn *", cboChuyenMon);
            o(0, 1, "Email", txtEmail);
            o(1, 1, "Kinh nghiệm (năm)", nudKinhNghiem);

            var secHs = Ui.Section("Hồ sơ");
            secHs.Margin = new Padding(0);
            secHs.Controls.Add(Ui.Stack(false,
                lblTieuDeForm,
                Ui.FieldLabel("Họ tên *"), txtHoTen,
                hai,
                Ui.FieldLabel("Học vấn / bằng cấp"), txtHocVan,
                Ui.FieldLabel("Ca nhận dạy (bỏ trống = nhận mọi ca)"), luoiCa,
                chkTamNghi,
                Ui.FieldLabel("Ghi chú"), txtGhiChu,
                lblLich, lblTaiKhoan,
                Ui.Row(btnLuu, btnXoa)));

            // ---- Trang ----
            var tieuDe = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tieuDe.Controls.Add(Ui.Title("Gia sư"), 0, 0);
            tieuDe.Controls.Add(btnThemMoi, 1, 0);

            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = new Padding(0, 10, 0, 0) };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            cot.Controls.Add(secDs, 0, 0);
            cot.Controls.Add(secHs, 1, 0);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(tieuDe, 0, 0);
            root.Controls.Add(cot, 0, 1);
            Controls.Add(root);
        }

        // ===================================================================
        // DỮ LIỆU
        // ===================================================================

        private class Dong
        {
            public int MaGS { get; set; }
            public string HoTen { get; set; }
            public string SDT { get; set; }
            public string ChuyenMon { get; set; }
            public string KinhNghiem { get; set; }
            public string CaNhanDay { get; set; }
            public int SoLop { get; set; }
            public string TrangThai { get; set; }
        }

        private void NapDuLieu(int? chon = null)
        {
            try
            {
                using (var gs = new GiaSuService()) _giaSus = gs.GetAll().ToList();
                using (var ls = new LopHocService()) _lopHoatDong = ls.GetLopConHoatDong();
                try { _taiKhoan = new TaiKhoanService().GetAll().Where(t => t.MaGS.HasValue).ToDictionary(t => t.MaGS.Value); }
                catch { _taiKhoan = new Dictionary<int, TaiKhoanRow>(); }

                var mon = _giaSus.Select(g => g.ChuyenMon).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().OrderBy(m => m).ToArray();
                string monChon = cboLocMon.SelectedIndex > 0 ? cboLocMon.SelectedItem as string : null;
                cboLocMon.Items.Clear();
                cboLocMon.Items.Add("Mọi môn");
                cboLocMon.Items.AddRange(mon);
                cboLocMon.SelectedItem = monChon;
                if (cboLocMon.SelectedIndex < 0) cboLocMon.SelectedIndex = 0;

                string monDangGo = cboChuyenMon.Text;
                cboChuyenMon.Items.Clear();
                cboChuyenMon.Items.AddRange(mon);
                cboChuyenMon.Text = monDangGo;

                ApDungLoc(chon);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tải được danh sách gia sư: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApDungLoc(int? chon = null, bool veTrangDau = true)
        {
            if (cboLocMon.Items.Count == 0) return;
            if (veTrangDau) pager.Reset();
            string mon = cboLocMon.SelectedIndex > 0 ? cboLocMon.SelectedItem as string : null;
            string tt = cboLocTrangThai.SelectedIndex > 0 ? cboLocTrangThai.SelectedItem as string : null;
            string ca = cboLocCa.SelectedIndex > 0 ? Constants.CaHoc.TatCaCaHoc[cboLocCa.SelectedIndex - 1] : null;

            var ds = _giaSus
                .Where(g => TimKiem.Khop(txtTim.Text, g.HoTen, g.SDT, g.Email, g.ChuyenMon, g.HocVan))
                .Where(g => mon == null || g.ChuyenMon == mon)
                .Where(g => tt == null || (g.TrangThai ?? Constants.GiaSuStatus.Ranh) == tt)
                .Where(g => ca == null || g.NhanCa(ca))
                .OrderBy(g => g.HoTen)
                .Select(g => new Dong
                {
                    MaGS = g.MaGS,
                    HoTen = g.HoTen,
                    SDT = g.SDT,
                    ChuyenMon = g.ChuyenMon,
                    KinhNghiem = g.KinhNghiem.HasValue ? g.KinhNghiem + " năm" : "",
                    CaNhanDay = string.IsNullOrWhiteSpace(g.CaNhanDay) ? "Mọi ca" : g.CaNhanDay.Replace(",", ", "),
                    SoLop = _lopHoatDong.Count(l => l.MaGS == g.MaGS),
                    TrangThai = g.TrangThai ?? Constants.GiaSuStatus.Ranh
                })
                .ToList();

            int? giu = chon ?? _maDangSua;
            int tong = ds.Count;
            dgv.DataSource = pager.Cat(ds);
            if (dgv.Columns.Count > 0)
            {
                dgv.Columns["MaGS"].Visible = false;
                dgv.Columns["HoTen"].HeaderText = "Họ tên";
                dgv.Columns["SDT"].HeaderText = "Số ĐT";
                dgv.Columns["ChuyenMon"].HeaderText = "Chuyên môn";
                dgv.Columns["KinhNghiem"].HeaderText = "Kinh nghiệm";
                dgv.Columns["CaNhanDay"].HeaderText = "Ca nhận dạy";
                dgv.Columns["SoLop"].HeaderText = "Lớp đang dạy";
                dgv.Columns["TrangThai"].HeaderText = "Trạng thái";
            }
            lblDem.Text = $"{tong} / {_giaSus.Count} gia sư";

            if (giu.HasValue)
                foreach (DataGridViewRow r in dgv.Rows)
                    if ((int)r.Cells["MaGS"].Value == giu.Value) { dgv.CurrentCell = r.Cells["HoTen"]; break; }
        }

        private void HienHoSoDangChon()
        {
            if (dgv.CurrentRow == null || dgv.SelectedRows.Count == 0) return;
            int ma = (int)dgv.CurrentRow.Cells["MaGS"].Value;
            var gs = _giaSus.FirstOrDefault(g => g.MaGS == ma);
            if (gs == null) return;

            _maDangSua = gs.MaGS;
            lblTieuDeForm.Text = gs.HoTen;
            txtHoTen.Text = gs.HoTen;
            txtSDT.Text = gs.SDT;
            cboChuyenMon.Text = gs.ChuyenMon ?? "";
            txtEmail.Text = gs.Email;
            txtHocVan.Text = gs.HocVan;
            nudKinhNghiem.Value = Math.Max(0, Math.Min(60, gs.KinhNghiem ?? 0));
            txtGhiChu.Text = gs.GhiChu;
            foreach (var c in chkCa) c.Checked = !string.IsNullOrWhiteSpace(gs.CaNhanDay) && gs.NhanCa((string)c.Tag);
            chkTamNghi.Checked = gs.TrangThai == Constants.GiaSuStatus.TamNghi;

            var lop = _lopHoatDong.Where(l => l.MaGS == gs.MaGS)
                .OrderBy(l => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, l.CaHoc)).ToList();
            lblLich.Text = lop.Count == 0
                ? "Chưa có lớp đang dạy."
                : "Đang dạy: " + string.Join("; ", lop.Select(l => $"{Constants.CaHoc.TenNgan(l.CaHoc)} {l.MonHoc} ({l.HocVien?.HoTen})"));
            TaiKhoanRow tk;
            lblTaiKhoan.Text = _taiKhoan.TryGetValue(gs.MaGS, out tk)
                ? $"Tài khoản đăng nhập: {tk.TenDangNhap} ({tk.TrangThai.ToLower()})"
                : "Chưa có tài khoản đăng nhập (tạo ở mục Tài khoản).";

            btnXoa.Enabled = true;
            btnLuu.Text = "Lưu thay đổi";
            btnLuu.Width = 150;
        }

        private void BatDauThemMoi()
        {
            _maDangSua = null;
            dgv.ClearSelection();
            lblTieuDeForm.Text = "Gia sư mới";
            foreach (var t in new[] { txtHoTen, txtSDT, txtEmail, txtHocVan, txtGhiChu }) t.Clear();
            cboChuyenMon.Text = "";
            nudKinhNghiem.Value = 0;
            foreach (var c in chkCa) c.Checked = false;
            chkTamNghi.Checked = false;
            lblLich.Text = "";
            lblTaiKhoan.Text = "";
            btnXoa.Enabled = false;
            btnLuu.Text = "Thêm";
            btnLuu.Width = 110;
            txtHoTen.Focus();
        }

        private void Luu()
        {
            var hs = new HoSoGiaSu
            {
                HoTen = txtHoTen.Text, SDT = txtSDT.Text, ChuyenMon = cboChuyenMon.Text,
                Email = txtEmail.Text, HocVan = txtHocVan.Text,
                KinhNghiem = nudKinhNghiem.Value > 0 ? (int?)nudKinhNghiem.Value : null,
                CaNhanDay = chkCa.Where(c => c.Checked).Select(c => c.Text).ToList(),
                GhiChu = txtGhiChu.Text, TamNghi = chkTamNghi.Checked
            };
            try
            {
                GiaSu gs;
                bool moi = !_maDangSua.HasValue;
                using (var service = new GiaSuService()) gs = service.LuuHoSo(_maDangSua, hs);
                NapDuLieu(gs.MaGS);
                _maDangSua = gs.MaGS;
                HienHoSoDangChon();
                MessageBox.Show(moi ? $"Đã thêm gia sư {gs.HoTen}." : $"Đã lưu hồ sơ {gs.HoTen}.",
                    Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                string m = ex.Message.ToLower();
                if (m.Contains("họ tên")) txtHoTen.Focus();
                else if (m.Contains("điện thoại")) txtSDT.Focus();
                else if (m.Contains("chuyên môn")) cboChuyenMon.Focus();
                else if (m.Contains("email")) txtEmail.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không lưu được: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Xoa()
        {
            if (!_maDangSua.HasValue) return;
            var gs = _giaSus.FirstOrDefault(g => g.MaGS == _maDangSua.Value);
            if (MessageBox.Show($"Xóa gia sư {gs?.HoTen}? Thao tác này không hoàn tác được.\n\n" +
                                "Gợi ý: nếu gia sư chỉ nghỉ một thời gian, hãy chọn \"Tạm nghỉ\" thay vì xóa.",
                    Constants.Messages.ConfirmTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            try
            {
                using (var service = new GiaSuService()) service.Delete(_maDangSua.Value);
                _maDangSua = null;
                NapDuLieu();
                BatDauThemMoi();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Constants.Messages.LoiKhiXoa}{ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
