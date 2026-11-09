using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Cửa sổ nhỏ bật ra khi bấm "Thêm học viên" / "Thêm gia sư".
    /// luu() trả về true khi lưu thành công thì cửa sổ đóng; lỗi nhập liệu thì giữ cửa sổ để sửa.
    /// </summary>
    public class dlgHoSo : Form, ITuGiaoDien
    {
        private readonly HoSoEditor _editor;
        private readonly Func<bool> _luu;

        public dlgHoSo(string tieuDe, string moTa, HoSoEditor editor, Func<bool> luu, string chuNutLuu = "Lưu")
        {
            _editor = editor;
            _luu = luu;

            Text = tieuDe;
            Font = Theme.Body;
            BackColor = Theme.Surface;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;
            Padding = new Padding(22, 16, 22, 16);

            var lblTieuDe = new Label { Text = tieuDe, Font = Theme.Section, ForeColor = Theme.Ink, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            var lblMoTa = Ui.Text(moTa, true);
            lblMoTa.Margin = new Padding(0, 0, 0, 10);

            var btnLuu = Ui.Button(chuNutLuu, ButtonKind.Primary, Glyph.Check, 120);
            var btnHuy = Ui.Button("Hủy", ButtonKind.Ghost, Glyph.None, 90);
            btnLuu.Click += (s, e) => { if (_luu()) { DialogResult = DialogResult.OK; Close(); } };
            btnHuy.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            AcceptButton = btnLuu;
            CancelButton = btnHuy;

            var nut = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Top,
                Margin = new Padding(0, 14, 0, 0), WrapContents = false
            };
            nut.Controls.Add(btnLuu);
            nut.Controls.Add(btnHuy);

            const int rong = 520;
            var khung = new TableLayoutPanel
            {
                ColumnCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Location = new Point(Padding.Left, Padding.Top), BackColor = Theme.Surface
            };
            khung.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, rong));
            khung.Controls.Add(lblTieuDe, 0, 0);
            khung.Controls.Add(lblMoTa, 0, 1);
            khung.Controls.Add(editor, 0, 2);
            khung.Controls.Add(nut, 0, 3);
            Controls.Add(khung);

            // Tự tính kích thước cửa sổ theo nội dung (không dùng Form.AutoSize vì dễ co sai)
            ClientSize = new Size(rong + Padding.Horizontal, 400);
            Load += (s, e) =>
            {
                khung.PerformLayout();
                var co = khung.GetPreferredSize(new Size(rong, 0));
                ClientSize = new Size(rong + Padding.Horizontal, co.Height + Padding.Vertical);
            };
            Shown += (s, e) => _editor.OBatDau.Focus();
        }
    }
}
