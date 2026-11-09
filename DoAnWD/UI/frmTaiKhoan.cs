using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;

namespace DoAnWD
{
    /// <summary>Quản lý tài khoản (chỉ vai trò Quản lý): tạo, đặt lại mật khẩu, khóa / mở khóa</summary>
    public class frmTaiKhoan : Form, ITuGiaoDien, ILamMoi
    {
        public void LamMoi() { NapDuLieu(); }

        private readonly TaiKhoanService _service = new TaiKhoanService();
        private DataGridView dgv;
        private TextBox txtTen, txtMatKhau;
        private ComboBox cboVaiTro, cboGiaSu;
        private Label lblGiaSu;

        public frmTaiKhoan()
        {
            BackColor = Theme.Paper;
            Font = Theme.Body;
            FormBorderStyle = FormBorderStyle.None;
            Padding = new Padding(20, 14, 20, 16);
            Text = "Tài khoản";
            BuildUi();
            Load += (s, e) => NapDuLieu();
        }

        private void BuildUi()
        {
            dgv = Ui.Grid();
            var btnDatLai = Ui.Button("Đặt lại mật khẩu", ButtonKind.Secondary, Glyph.Key);
            btnDatLai.Click += (s, e) => DatLai();
            var btnKhoa = Ui.Button("Khóa / mở khóa", ButtonKind.Danger, Glyph.Close);
            btnKhoa.Click += (s, e) => KhoaMo();
            var secDs = Ui.Section("Danh sách tài khoản");
            var st = Ui.Stack(false, dgv, Ui.Row(btnDatLai, btnKhoa));
            st.RowStyles[0] = new RowStyle(SizeType.Percent, 100);
            dgv.Dock = DockStyle.Fill;
            secDs.Controls.Add(st);

            txtTen = Ui.Input("Ví dụ: gs.lan");
            txtMatKhau = Ui.Input($"Tối thiểu {TaiKhoanService.DoDaiMatKhauToiThieu} ký tự");
            txtMatKhau.UseSystemPasswordChar = true;
            cboVaiTro = Ui.Combo();
            cboVaiTro.Items.AddRange(new object[] { "Gia sư", "Quản lý" });
            cboVaiTro.SelectedIndex = 0;
            cboGiaSu = Ui.Combo();
            lblGiaSu = Ui.FieldLabel("Gắn với gia sư");
            cboVaiTro.SelectedIndexChanged += (s, e) => { bool gs = cboVaiTro.SelectedIndex == 0; cboGiaSu.Enabled = gs; };
            var btnTao = Ui.Button("Tạo tài khoản", ButtonKind.Primary, Glyph.Plus);
            btnTao.Click += (s, e) => Tao();

            var secTao = Ui.Section("Tạo tài khoản mới");
            secTao.Margin = new Padding(0);
            secTao.Controls.Add(Ui.Stack(false,
                Ui.Text("Gia sư đăng nhập chỉ thấy lịch và điểm danh các lớp của mình. Quản lý thấy toàn bộ.", true),
                Ui.FieldLabel("Vai trò"), cboVaiTro,
                lblGiaSu, cboGiaSu,
                Ui.FieldLabel("Tên đăng nhập"), txtTen,
                Ui.FieldLabel("Mật khẩu ban đầu"), txtMatKhau,
                btnTao));

            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            cot.Controls.Add(secDs, 0, 0);
            cot.Controls.Add(secTao, 1, 0);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(Ui.Title("Tài khoản"), 0, 0);
            root.Controls.Add(cot, 0, 1);
            Controls.Add(root);
        }

        private class MucGiaSu
        {
            public GiaSu GS;
            public override string ToString() => $"{GS.HoTen} ({GS.ChuyenMon})";
        }

        private void NapDuLieu()
        {
            try
            {
                dgv.DataSource = _service.GetAll();
                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["MaTK"].Visible = false;
                    dgv.Columns["TenDangNhap"].HeaderText = "Tên đăng nhập";
                    dgv.Columns["VaiTro"].HeaderText = "Vai trò";
                    dgv.Columns["GiaSu"].HeaderText = "Gia sư";
                    dgv.Columns["TrangThai"].HeaderText = "Trạng thái";
                    dgv.Columns["NgayTao"].HeaderText = "Ngày tạo";
                    dgv.Columns["NgayTao"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }
                cboGiaSu.Items.Clear();
                cboGiaSu.Items.AddRange(_service.GetGiaSuChuaCoTaiKhoan().Select(g => (object)new MucGiaSu { GS = g }).ToArray());
                if (cboGiaSu.Items.Count > 0) cboGiaSu.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tải được tài khoản: {ex.GetBaseException().Message}", Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private TaiKhoanRow Chon => dgv.CurrentRow?.DataBoundItem as TaiKhoanRow;

        private void Tao()
        {
            try
            {
                bool laGS = cboVaiTro.SelectedIndex == 0;
                int? maGS = laGS ? (cboGiaSu.SelectedItem as MucGiaSu)?.GS.MaGS : null;
                _service.TaoTaiKhoan(txtTen.Text, txtMatKhau.Text, laGS ? Constants.VaiTro.GiaSu : Constants.VaiTro.QuanLy, maGS);
                MessageBox.Show($"Đã tạo tài khoản {txtTen.Text.Trim().ToLowerInvariant()}.", Constants.Messages.SuccessTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtTen.Clear();
                txtMatKhau.Clear();
                NapDuLieu();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DatLai()
        {
            var tk = Chon;
            if (tk == null) return;
            using (var f = new frmDoiMatKhau(tk.MaTK, tk.TenDangNhap, datLai: true))
                f.ShowDialog(this);
        }

        private void KhoaMo()
        {
            var tk = Chon;
            if (tk == null) return;
            try
            {
                bool mo = _service.DoiTrangThai(tk.MaTK, PhienDangNhap.MaTK);
                MessageBox.Show(mo ? $"Đã mở khóa {tk.TenDangNhap}." : $"Đã khóa {tk.TenDangNhap}. Tài khoản này không đăng nhập được nữa.",
                    Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                NapDuLieu();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
