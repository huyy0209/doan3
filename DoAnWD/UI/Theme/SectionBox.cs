using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Nhóm nội dung thay cho GroupBox: nền trắng, viền dòng kẻ mảnh bo góc,
    /// tiêu đề chữ đậm nằm trong khung (không cắt ngang viền như GroupBox mặc định).
    /// </summary>
    public class SectionBox : GroupBox
    {
        public SectionBox()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            ForeColor = Theme.Text;
            Font = Theme.Section;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Theme.Paper);

            // Thẻ trắng bo góc; một nét bóng mờ ở đáy cho thẻ nổi khỏi nền
            var bong = new RectangleF(0.5f, 1.5f, Width - 1.5f, Height - 2.5f);
            using (var path = Theme.RoundedRect(bong, Theme.RadiusCard))
            using (var p = new Pen(Theme.Blend(Theme.Rule, Theme.Paper, 0.35f), 2f)) g.DrawPath(p, path);
            var rect = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 2.5f);
            using (var path = Theme.RoundedRect(rect, Theme.RadiusCard))
            {
                using (var b = new SolidBrush(BackColor)) g.FillPath(b, path);
                using (var p = new Pen(Theme.Rule)) g.DrawPath(p, path);
            }

            if (!string.IsNullOrEmpty(Text))
                TextRenderer.DrawText(g, Text, Theme.Section, new Point(18, 13), Theme.Text,
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }
}
