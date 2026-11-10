using System;
using System.Drawing;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.UI.Controls;

namespace DoAnWD.UI.Dialogs
{
    /// <summary>
    /// Cài đặt trung tâm (chỉ quản lý): tên, địa chỉ, số điện thoại.
    /// Tên hiện ở màn hình đăng nhập, menu trái và đầu phiếu báo kết quả gửi phụ huynh.
    /// </summary>
    public class dlgCaiDat : Form, ITuGiaoDien
    {
        private readonly TextBox txtTen, txtDiaChi, txtSDT;

        public dlgCaiDat()
        {
            Text = "Cài đặt trung tâm";
            Font = Theme.Body;
            BackColor = Theme.Surface;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(26, 20, 26, 20);
            const int rong = 460;

            var tt = CaiDatService.TrungTam;
            txtTen = Ui.Input("Ví dụ: Trung tâm Gia sư Ánh Dương");
            txtDiaChi = Ui.Input("Số nhà, đường, quận/huyện");
            txtSDT = Ui.Input("Số để phụ huynh liên hệ");
            txtTen.Text = tt.Ten;
            txtDiaChi.Text = tt.DiaChi;
            txtSDT.Text = tt.SDT;
            txtTen.MaxLength = 100; txtDiaChi.MaxLength = 300; txtSDT.MaxLength = 50;

            var saoLuu = Ui.Text("Sao lưu dữ liệu: chạy file SaoLuuCSDL.bat trong thư mục ứng dụng, nên làm mỗi tuần một lần.", true);
            saoLuu.MaximumSize = new Size(rong, 0);
            saoLuu.Margin = new Padding(0, 12, 0, 0);

            var btnLuu = Ui.Button("Lưu thông tin", ButtonKind.Primary, Glyph.Check);
            var btnHuy = Ui.Button("Hủy", ButtonKind.Ghost, Glyph.None, 90);
            btnLuu.Click += (s, e) => Luu();
            btnHuy.Click += (s, e) => Close();
            AcceptButton = btnLuu;
            CancelButton = btnHuy;
            var nut = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Top, WrapContents = false, Margin = new Padding(0, 14, 0, 0) };
            nut.Controls.Add(btnLuu);
            nut.Controls.Add(btnHuy);

            var moTa = Ui.Text("Thông tin này hiện ở màn hình đăng nhập, menu và đầu phiếu báo kết quả gửi phụ huynh.", true);
            moTa.MaximumSize = new Size(rong, 0);

            var khung = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Location = new Point(Padding.Left, Padding.Top), BackColor = Theme.Surface };
            khung.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, rong));
            var tieuDe = new Label { Text = Text, Font = Theme.Chu(14F, 2), ForeColor = Theme.Text, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            foreach (var c in new Control[] { tieuDe, moTa, Ui.FieldLabel("Tên trung tâm"), txtTen, Ui.FieldLabel("Địa chỉ"), txtDiaChi, Ui.FieldLabel("Số điện thoại"), txtSDT, saoLuu, nut })
                khung.Controls.Add(c);
            Controls.Add(khung);

            ClientSize = new Size(rong + Padding.Horizontal, 420);
            Load += (s, e) =>
            {
                khung.PerformLayout();
                ClientSize = new Size(rong + Padding.Horizontal, khung.GetPreferredSize(new Size(rong, 0)).Height + Padding.Vertical);
            };
        }

        private void Luu()
        {
            try
            {
                CaiDatService.LuuTrungTam(txtTen.Text, txtDiaChi.Text, txtSDT.Text);
                ThongBao.ThanhCong("Đã lưu thông tin trung tâm");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTen.Focus();
            }
        }
    }
}
