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
    public partial class frmGiaSu : Form, ITuGiaoDien, ILamMoi
    {
        public void LamMoi() { NapDuLieu(_maDangSua); }

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
        private HoSoGiaSuEditor editor;
        private string[] _monGoiY = new string[0];
        private ThemedButton btnThemMoi, btnLuu, btnXoa;

        public frmGiaSu()
        {
            InitializeComponent();
            BuildUi();
            Load += (s, e) => { NapDuLieu(); if (!_maDangSua.HasValue) ChuaChon(); };
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
            editor = new HoSoGiaSuEditor();
            lblLich = Ui.Text("", true);
            lblLich.MaximumSize = new Size(440, 0);
            lblTaiKhoan = Ui.Text("", true);

            btnThemMoi = Ui.Button("Thêm gia sư", ButtonKind.Secondary, Glyph.Plus);
            btnThemMoi.Click += (s, e) => MoCuaSoThem();
            btnLuu = Ui.Button("Lưu thay đổi", ButtonKind.Primary, Glyph.Check, 150);
            btnLuu.Click += (s, e) => Luu();
            btnXoa = Ui.Button("Xóa", ButtonKind.Danger, Glyph.Trash, 96);
            btnXoa.Click += (s, e) => Xoa();

            var secHs = Ui.Section("Hồ sơ");
            secHs.Margin = new Padding(0);
            secHs.Controls.Add(Ui.Stack(false,
                lblTieuDeForm,
                editor,
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

                _monGoiY = mon;
                editor.DatGoiYMon(mon);

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
            editor.Enabled = true;
            editor.Nap(gs);

            var lop = _lopHoatDong.Where(l => l.MaGS == gs.MaGS)
                .OrderBy(l => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, l.CaHoc)).ToList();
            lblLich.Text = lop.Count == 0
                ? "Chưa có lớp đang dạy."
                : "Đang dạy: " + string.Join("; ", lop.Select(l => $"{Constants.CaHoc.TenNgan(l.CaHoc)} {l.MonHoc} ({l.HocVien?.HoTen})"));
            TaiKhoanRow tk;
            lblTaiKhoan.Text = _taiKhoan.TryGetValue(gs.MaGS, out tk)
                ? $"Tài khoản đăng nhập: {tk.TenDangNhap} ({tk.TrangThai.ToLower()})"
                : "Chưa có tài khoản đăng nhập (tạo ở mục Tài khoản).";

            btnLuu.Enabled = btnXoa.Enabled = true;
        }

        /// <summary>Chưa chọn ai: khung hồ sơ để trống và khóa lại</summary>
        private void ChuaChon()
        {
            _maDangSua = null;
            dgv.ClearSelection();
            lblTieuDeForm.Text = "Chưa chọn gia sư";
            editor.XoaTrang();
            editor.Enabled = false;
            lblLich.Text = "Chọn một dòng ở danh sách để xem và sửa hồ sơ.\nBấm \"Thêm gia sư\" để nhập người mới.";
            lblTaiKhoan.Text = "";
            btnLuu.Enabled = btnXoa.Enabled = false;
        }

        /// <summary>Bật cửa sổ nhập gia sư mới; lưu xong thì chọn luôn người vừa thêm</summary>
        private void MoCuaSoThem()
        {
            var ed = new HoSoGiaSuEditor();
            ed.DatGoiYMon(_monGoiY);
            GiaSu moi = null;
            using (var dlg = new dlgHoSo("Thêm gia sư", "Họ tên, số điện thoại và chuyên môn là bắt buộc. Các mục khác có thể bổ sung sau.",
                       ed, () => LuuHoSo(null, ed, out moi), "Thêm"))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK || moi == null) return;
            }
            txtTim.Clear();
            cboLocMon.SelectedIndex = cboLocTrangThai.SelectedIndex = cboLocCa.SelectedIndex = 0;
            NapDuLieu(moi.MaGS);
            HienHoSoDangChon();
        }

        /// <summary>Lưu qua service; lỗi nhập liệu thì báo và đưa con trỏ về đúng ô</summary>
        private bool LuuHoSo(int? ma, HoSoGiaSuEditor ed, out GiaSu kq)
        {
            kq = null;
            try
            {
                using (var service = new GiaSuService()) kq = service.LuuHoSo(ma, ed.Lay());
                return true;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ed.FocusTheoLoi(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không lưu được: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void Luu()
        {
            if (!_maDangSua.HasValue) return;
            GiaSu gs;
            if (!LuuHoSo(_maDangSua, editor, out gs)) return;
            NapDuLieu(gs.MaGS);
            HienHoSoDangChon();
            MessageBox.Show($"Đã lưu hồ sơ {gs.HoTen}.", Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                ChuaChon();
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
