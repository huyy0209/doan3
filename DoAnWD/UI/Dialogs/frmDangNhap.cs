using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.UI;
using DoAnWD.UI.Controls;
using DoAnWD.UI.Pages;

namespace DoAnWD.UI.Dialogs
{
    /// <summary>Màn hình đăng nhập (mở trước cửa sổ chính)</summary>
    public class frmDangNhap : Form, ITuGiaoDien
    {
        private readonly TextBox txtTen, txtMatKhau;
        private readonly Label lblLoi;
        private readonly ThemedButton btnDangNhap;
        private int _soLanSai;

        public frmDangNhap(bool vuaTaoAdmin)
        {
            Text = "Đăng nhập - Quản lý gia sư";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(820, 480);
            BackColor = Theme.Surface;
            Font = Theme.Body;

            // Nửa trái: nền tím đậm + trang vở có lề đỏ
            var trai = new Panel { Dock = DockStyle.Left, Width = 340, BackColor = Theme.InkDark };
            trai.Paint += VeBiaTrai;

            txtTen = Ui.Input("Tên đăng nhập");
            txtMatKhau = Ui.Input("Mật khẩu");
            txtMatKhau.UseSystemPasswordChar = true;
            txtTen.Width = txtMatKhau.Width = 340;
            txtTen.Dock = txtMatKhau.Dock = DockStyle.None;

            lblLoi = new Label { ForeColor = Theme.Margin, Font = Theme.Body, AutoSize = true, MaximumSize = new Size(340, 0), Margin = new Padding(0, 6, 0, 0) };
            btnDangNhap = Ui.Button("Đăng nhập", ButtonKind.Primary, Glyph.Key, 340);
            btnDangNhap.Height = 44;
            btnDangNhap.Margin = new Padding(0, 14, 0, 0);
            btnDangNhap.Click += (s, e) => DangNhap();

            var goiY = Ui.Text(vuaTaoAdmin
                ? $"Lần đầu sử dụng: đăng nhập bằng {TaiKhoanService.AdminMacDinh} / {TaiKhoanService.MatKhauMacDinh}, sau đó vào \"Đổi mật khẩu\"."
                : "Gia sư dùng tài khoản do quản lý cấp.", true);
            goiY.MaximumSize = new Size(340, 0);

            var form = Ui.Stack(false,
                Ui.Title("Đăng nhập"), goiY,
                Ui.FieldLabel("Tên đăng nhập"), txtTen,
                Ui.FieldLabel("Mật khẩu"), txtMatKhau,
                lblLoi, btnDangNhap);
            form.Dock = DockStyle.None;
            form.AutoSize = true;
            form.Location = new Point(400, 70);

            Controls.Add(form);
            Controls.Add(trai);
            AcceptButton = btnDangNhap;
            Shown += (s, e) => txtTen.Focus();
            if (vuaTaoAdmin) txtTen.Text = TaiKhoanService.AdminMacDinh;
        }

        private static readonly Font FontTen = new Font(Theme.FamilySemibold, 20F);

        private static void VeBiaTrai(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var page = new RectangleF(48, 70, 70, 86);
            using (var path = Theme.RoundedRect(page, 6))
            using (var b = new SolidBrush(Color.White)) g.FillPath(b, path);
            using (var p = new Pen(Theme.InkMuted, 1.5f))
                for (int i = 1; i <= 4; i++) g.DrawLine(p, page.X + 22, page.Y + i * 17, page.Right - 10, page.Y + i * 17);
            using (var p = new Pen(Theme.Margin, 3f)) g.DrawLine(p, page.X + 15, page.Y + 4, page.X + 15, page.Bottom - 4);

            TextRenderer.DrawText(g, "Quản lý gia sư", FontTen, new Point(46, 186), Color.White);
            TextRenderer.DrawText(g, "Ghép lớp theo ca, điểm danh,\nđiểm test và học phí ở một nơi.",
                Theme.Body, new Rectangle(48, 226, 260, 60), Theme.InkMuted, TextFormatFlags.WordBreak);
        }

        private void DangNhap()
        {
            lblLoi.Text = "";
            try
            {
                Cursor = Cursors.WaitCursor;
                var tk = new TaiKhoanService().DangNhap(txtTen.Text, txtMatKhau.Text);
                PhienDangNhap.BatDau(tk);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                _soLanSai++;
                lblLoi.Text = ex.Message;
                txtMatKhau.Clear();
                txtMatKhau.Focus();
                // Sai nhiều lần: chờ tăng dần để chống dò mật khẩu
                if (_soLanSai >= 3)
                {
                    btnDangNhap.Enabled = false;
                    var t = new Timer { Interval = Math.Min(30, (_soLanSai - 2) * 5) * 1000 };
                    lblLoi.Text += $" Thử lại sau {t.Interval / 1000} giây.";
                    t.Tick += (s, e2) => { t.Stop(); t.Dispose(); btnDangNhap.Enabled = true; };
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                lblLoi.Text = "Không kết nối được CSDL: " + ex.GetBaseException().Message;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }

    /// <summary>Hộp thoại đổi mật khẩu của chính mình, hoặc quản lý đặt lại mật khẩu cho người khác</summary>
    public class frmDoiMatKhau : Form, ITuGiaoDien
    {
        private readonly TextBox txtCu, txtMoi, txtNhapLai;
        private readonly int _maTK;
        private readonly bool _datLai;

        /// <param name="datLai">true: quản lý đặt lại cho tài khoản khác (không cần mật khẩu cũ)</param>
        public frmDoiMatKhau(int maTK, string tenTaiKhoan, bool datLai = false)
        {
            _maTK = maTK;
            _datLai = datLai;
            Text = datLai ? "Đặt lại mật khẩu" : "Đổi mật khẩu";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Surface;
            Font = Theme.Body;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(28, 20, 28, 20);

            txtCu = Ui.Input(null, 320); txtCu.UseSystemPasswordChar = true;
            txtMoi = Ui.Input($"Tối thiểu {TaiKhoanService.DoDaiMatKhauToiThieu} ký tự", 320); txtMoi.UseSystemPasswordChar = true;
            txtNhapLai = Ui.Input(null, 320); txtNhapLai.UseSystemPasswordChar = true;

            var btnLuu = Ui.Button("Lưu mật khẩu", ButtonKind.Primary, Glyph.Check);
            btnLuu.Click += (s, e) => Luu();
            var btnHuy = Ui.Button("Hủy", ButtonKind.Secondary, Glyph.Close);
            btnHuy.Click += (s, e) => Close();

            var items = new System.Collections.Generic.List<Control> { Ui.Title(Text), Ui.Text("Tài khoản: " + tenTaiKhoan, true) };
            if (!datLai) { items.Add(Ui.FieldLabel("Mật khẩu hiện tại")); items.Add(txtCu); }
            items.Add(Ui.FieldLabel("Mật khẩu mới")); items.Add(txtMoi);
            items.Add(Ui.FieldLabel("Nhập lại mật khẩu mới")); items.Add(txtNhapLai);
            items.Add(Ui.Row(btnLuu, btnHuy));
            var st = Ui.Stack(false, items.ToArray());
            st.AutoSize = true;
            st.Dock = DockStyle.None;
            Controls.Add(st);
            AcceptButton = btnLuu;
            CancelButton = btnHuy;
        }

        private void Luu()
        {
            if (txtMoi.Text != txtNhapLai.Text)
            {
                MessageBox.Show("Hai lần nhập mật khẩu mới không khớp.", Common.Constants.Messages.WarningTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNhapLai.Focus();
                return;
            }
            try
            {
                var service = new TaiKhoanService();
                if (_datLai) service.DatLaiMatKhau(_maTK, txtMoi.Text);
                else service.DoiMatKhau(_maTK, txtCu.Text, txtMoi.Text);
                MessageBox.Show("Đã lưu mật khẩu mới.", Common.Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is UnauthorizedAccessException || ex is InvalidOperationException)
            {
                MessageBox.Show(ex.Message, Common.Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
