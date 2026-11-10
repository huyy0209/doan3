using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace DoAnWD.UI.Controls
{
    /// <summary>
    /// Thông báo nhỏ tự tắt ở cuối cửa sổ chính ("Đã lưu...") thay cho hộp thoại phải bấm OK.
    /// Không lấy mất con trỏ đang gõ; bấm vào để tắt ngay. Lỗi và câu hỏi xác nhận vẫn dùng hộp thoại.
    /// </summary>
    public static class ThongBao
    {
        private static BangBao _dangHien;

        public static void ThanhCong(string noiDung) => Hien(noiDung, Theme.Success, Glyph.Check);
        public static void LuuY(string noiDung) => Hien(noiDung, Theme.Warning, Glyph.None);

        private static void Hien(string noiDung, Color mau, Glyph glyph)
        {
            if (string.IsNullOrWhiteSpace(noiDung)) return;
            try
            {
                // Bám vào cửa sổ chính để không bị đóng theo hộp thoại vừa lưu xong
                var chu = Application.OpenForms.OfType<TrangChu>().FirstOrDefault(f => f.Visible)
                          ?? Application.OpenForms.Cast<Form>().LastOrDefault(f => f.Visible && f.TopLevel && !(f is BangBao));
                if (chu == null) return;
                if (_dangHien != null && !_dangHien.IsDisposed) _dangHien.Close();
                _dangHien = new BangBao(noiDung, mau, glyph);
                var b = chu.Bounds;
                _dangHien.Location = new Point(b.Left + (b.Width - _dangHien.Width) / 2, b.Bottom - _dangHien.Height - 36);
                _dangHien.Show(chu);
            }
            catch { /* thông báo phụ, lỗi hiển thị thì bỏ qua */ }
        }

        private class BangBao : Form
        {
            private readonly string _noiDung;
            private readonly Color _mau;
            private readonly Glyph _glyph;
            private readonly Timer _hen = new Timer();

            public BangBao(string noiDung, Color mau, Glyph glyph)
            {
                // Chỉ lấy dòng đầu cho gọn; chi tiết đã có trên màn hình
                _noiDung = noiDung.Split('\n')[0].Trim();
                _mau = mau;
                _glyph = glyph;
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                BackColor = Theme.InkDarker;
                DoubleBuffered = true;
                Cursor = Cursors.Hand;
                int rong = Math.Min(620, TextRenderer.MeasureText(_noiDung, Theme.BodyStrong).Width + 84);
                Size = new Size(Math.Max(220, rong), 48);
                using (var path = Theme.RoundedRect(new RectangleF(0, 0, Width, Height), 10)) Region = new Region(path);
                Click += (s, e) => Close();
                _hen.Interval = Math.Min(6000, 2200 + _noiDung.Length * 35);
                _hen.Tick += (s, e) => Close();
                _hen.Start();
                FormClosed += (s, e) => _hen.Dispose();
            }

            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= 0x08000000 | 0x00000080;   // WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW
                    return cp;
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(_mau)) g.FillEllipse(b, 14, 12, 24, 24);
                if (_glyph != Glyph.None) Icons.Draw(g, _glyph, new RectangleF(18, 16, 16, 16), Color.White);
                else TextRenderer.DrawText(g, "!", Theme.BodyStrong, new Rectangle(14, 12, 24, 24), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, _noiDung, Theme.BodyStrong, new Rectangle(50, 0, Width - 64, Height), Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }
    }
}
