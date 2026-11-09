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

            var rect = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (var path = Theme.RoundedRect(rect, Theme.Radius + 2))
            {
                using (var b = new SolidBrush(BackColor)) g.FillPath(b, path);
                using (var p = new Pen(Theme.Rule)) g.DrawPath(p, path);
            }

            TextRenderer.DrawText(g, Text, Theme.Section, new Point(10, 7), Theme.Text,
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
        }
    }
}
