using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.UI;
using DoAnWD.UI.Controls;
using DoAnWD.UI.Pages;

namespace DoAnWD.UI.Dialogs
{
    /// <summary>
    /// Màn hình đăng nhập (mở trước cửa sổ chính): nửa trái là tấm bảng xanh mang tên trung tâm,
    /// nửa phải là ô nhập. Đăng nhập bằng mật khẩu mặc định thì phải đặt mật khẩu mới ngay.
    /// </summary>
    public class frmDangNhap : Form, ITuGiaoDien
    {
        private const int RongBia = 380, RongO = 340;
        private static readonly Font FontTen = Theme.Chu(21F, 2);
        private static readonly Font FontTieuDe = Theme.Chu(18F, 2);

        private readonly TextBox txtTen, txtMatKhau;
        private readonly Label lblLoi;
        private readonly ThemedButton btnDangNhap;
        private readonly ThongTinTrungTam _trungTam = CaiDatService.TrungTam;
        private int _soLanSai;

        public frmDangNhap(bool vuaTaoAdmin)
        {
            Text = "Đăng nhập - " + _trungTam.TenHienThi;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(860, 500);
            BackColor = Theme.Surface;
            Font = Theme.Body;

            var trai = new Panel { Dock = DockStyle.Left, Width = RongBia, BackColor = Theme.Board };
            ThemeManager.EnableDoubleBuffer(trai);
            trai.Paint += VeBiaTrai;

            txtTen = Ui.Input("Tên đăng nhập", RongO);
            txtMatKhau = Ui.Input("Mật khẩu", RongO);
            txtMatKhau.UseSystemPasswordChar = true;

            lblLoi = new Label { ForeColor = Theme.Margin, Font = Theme.Body, AutoSize = true, MaximumSize = new Size(RongO, 0), Margin = new Padding(0, 6, 0, 0), UseMnemonic = false };
            btnDangNhap = Ui.Button("Đăng nhập", ButtonKind.Primary, Glyph.None, RongO);
            btnDangNhap.Height = 44;
            btnDangNhap.Margin = new Padding(0, 16, 0, 0);
            btnDangNhap.Click += (s, e) => DangNhap();

            var tieuDe = new Label { Text = "Đăng nhập", Font = FontTieuDe, ForeColor = Theme.Text, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            var goiY = Ui.Text(vuaTaoAdmin
                ? $"Lần đầu sử dụng: đăng nhập bằng {TaiKhoanService.AdminMacDinh} / {TaiKhoanService.MatKhauMacDinh}. Ứng dụng sẽ yêu cầu đặt mật khẩu mới ngay sau đó."
                : "Quản lý và gia sư dùng tài khoản được cấp.", true);
            goiY.MaximumSize = new Size(RongO, 0);
            goiY.Margin = new Padding(0, 0, 0, 10);

            var form = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Theme.Surface };
            form.Controls.AddRange(new Control[] { tieuDe, goiY, Ui.FieldLabel("Tên đăng nhập"), txtTen, Ui.FieldLabel("Mật khẩu"), txtMatKhau, lblLoi, btnDangNhap });
            Controls.Add(form);
            Controls.Add(trai);

            // Canh khối nhập vào giữa nửa phải
            Load += (s, e) =>
            {
                var co = form.GetPreferredSize(Size.Empty);
                form.Location = new Point(RongBia + (ClientSize.Width - RongBia - RongO) / 2, Math.Max(40, (ClientSize.Height - co.Height) / 2 - 10));
            };
            AcceptButton = btnDangNhap;
            Shown += (s, e) => { if (txtTen.Text.Length > 0) txtMatKhau.Focus(); else txtTen.Focus(); };
            if (vuaTaoAdmin) txtTen.Text = TaiKhoanService.AdminMacDinh;
        }

        private void VeBiaTrai(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var p = (Panel)sender;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            const int le = 44;

            // Dấu hiệu: ô phấn vàng có quyển sổ
            var o = new RectangleF(le, 58, 56, 56);
            using (var path = Theme.RoundedRect(o, 15))
            using (var b = new SolidBrush(Theme.Chalk)) g.FillPath(b, path);
            Icons.Draw(g, Glyph.Book, new RectangleF(o.X + 13, o.Y + 13, 30, 30), Theme.InkDarker, 2.2f);

            var oTen = new Rectangle(le - 2, 136, p.Width - 2 * le, 76);
            TextRenderer.DrawText(g, _trungTam.TenHienThi, FontTen, oTen, Color.White,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            int caoTen = TextRenderer.MeasureText(g, _trungTam.TenHienThi, FontTen, oTen.Size, TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
            int y = oTen.Y + Math.Min(oTen.Height, caoTen) + 6;
            TextRenderer.DrawText(g, "Phần mềm quản lý gia sư", Theme.Body, new Point(le, y), Theme.InkMuted);

            // Ba việc chính, viết như gạch đầu dòng bằng phấn
            y += 44;
            using (var but = new Pen(Theme.BoardLine)) g.DrawLine(but, le, y - 14, p.Width - le, y - 14);
            foreach (string viec in new[] { "Ghép lớp theo ca và thứ học", "Điểm danh, điểm test, xếp học bù", "Học phí và công nợ từng lớp" })
            {
                using (var b = new SolidBrush(Theme.Chalk)) g.FillEllipse(b, le + 1, y + 8, 6, 6);
                TextRenderer.DrawText(g, viec, Theme.Body, new Rectangle(le + 18, y, p.Width - 2 * le - 18, 24), Theme.ChalkWhite,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                y += 30;
            }

            // Liên hệ của trung tâm ở chân
            string lienHe = string.Join("\n", new[] { _trungTam.DiaChi, _trungTam.SDT }.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (lienHe.Length > 0)
                TextRenderer.DrawText(g, lienHe, Theme.Small, new Rectangle(le, p.Height - 78, p.Width - 2 * le, 44), Theme.InkMuted,
                    TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
        }

        private void DangNhap()
        {
            lblLoi.Text = "";
            try
            {
                Cursor = Cursors.WaitCursor;
                var tk = new TaiKhoanService().DangNhap(txtTen.Text, txtMatKhau.Text);

                // Mật khẩu mặc định ai cũng biết: bắt đặt mật khẩu riêng rồi mới cho vào
                if (txtMatKhau.Text == TaiKhoanService.MatKhauMacDinh)
                {
                    using (var doi = new frmDoiMatKhau(tk.MaTK, tk.TenDangNhap, false, txtMatKhau.Text))
                        if (doi.ShowDialog(this) != DialogResult.OK)
                        {
                            lblLoi.Text = "Cần đặt mật khẩu mới thay cho mật khẩu mặc định trước khi dùng.";
                            return;
                        }
                }

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
                // Sai nhiều lần: chờ tăng dần để chống dò mật khẩu (tài khoản còn bị khóa 5 phút sau 5 lần sai)
                if (_soLanSai >= 3)
                {
                    btnDangNhap.Enabled = false;
                    var t = new Timer { Interval = Math.Min(30, (_soLanSai - 2) * 5) * 1000 };
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
        private readonly string _matKhauCu;

        /// <param name="datLai">true: quản lý đặt lại cho tài khoản khác (không cần mật khẩu cũ)</param>
        /// <param name="matKhauCu">Có giá trị: vừa đăng nhập bằng mật khẩu mặc định, buộc đặt mật khẩu mới (không hỏi lại mật khẩu cũ)</param>
        public frmDoiMatKhau(int maTK, string tenTaiKhoan, bool datLai = false, string matKhauCu = null)
        {
            _maTK = maTK;
            _datLai = datLai;
            _matKhauCu = matKhauCu;
            bool batBuoc = matKhauCu != null;
            Text = batBuoc ? "Đặt mật khẩu mới" : datLai ? "Đặt lại mật khẩu" : "Đổi mật khẩu";
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

            var tieuDe = new Label { Text = Text, Font = Theme.Chu(14F, 2), ForeColor = Theme.Text, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            var moTa = Ui.Text(batBuoc
                ? $"Tài khoản {tenTaiKhoan} đang dùng mật khẩu mặc định mà ai cũng biết. Đặt mật khẩu riêng để vào ứng dụng."
                : "Tài khoản: " + tenTaiKhoan, true);
            moTa.MaximumSize = new Size(320, 0);
            var items = new System.Collections.Generic.List<Control> { tieuDe, moTa };
            if (!datLai && !batBuoc) { items.Add(Ui.FieldLabel("Mật khẩu hiện tại")); items.Add(txtCu); }
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
                else service.DoiMatKhau(_maTK, _matKhauCu ?? txtCu.Text, txtMoi.Text);
                ThongBao.ThanhCong("Đã lưu mật khẩu mới.");
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
