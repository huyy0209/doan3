using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Biểu đồ đường điểm test theo ngày (thang 10), có vạch 5 và 8 để nhìn nhanh mức Trung bình / Giỏi.
    /// Dùng khi trao đổi với phụ huynh về tiến bộ của học viên.
    /// </summary>
    public class ScoreChart : Control
    {
        private List<KeyValuePair<DateTime, decimal>> _diem = new List<KeyValuePair<DateTime, decimal>>();

        public ScoreChart()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            Height = 150;
        }

        public void SetData(IEnumerable<KeyValuePair<DateTime, decimal>> diem)
        {
            _diem = diem.OrderBy(d => d.Key).ToList();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var vung = new Rectangle(34, 10, Width - 50, Height - 34);
            if (_diem.Count == 0)
            {
                TextRenderer.DrawText(g, "Chưa có bài test nào.", Theme.Body, ClientRectangle, Theme.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            Func<decimal, float> y = d => vung.Bottom - (float)d / 10f * vung.Height;
            using (var p = new Pen(Theme.Rule))
            {
                foreach (int muc in new[] { 0, 5, 8, 10 })
                {
                    g.DrawLine(p, vung.Left, y(muc), vung.Right, y(muc));
                    TextRenderer.DrawText(g, muc.ToString(), Theme.Small, new Rectangle(0, (int)y(muc) - 9, 28, 18),
                        muc == 8 ? Theme.Success : muc == 5 ? Theme.Warning : Theme.TextFaint,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }
            }

            var pts = new PointF[_diem.Count];
            for (int i = 0; i < _diem.Count; i++)
            {
                float x = _diem.Count == 1 ? vung.Left + vung.Width / 2f
                        : vung.Left + 8 + i * (vung.Width - 16f) / (_diem.Count - 1);
                pts[i] = new PointF(x, y(_diem[i].Value));
            }

            if (pts.Length > 1)
                using (var p = new Pen(Theme.Ink, 2f)) g.DrawLines(p, pts);

            for (int i = 0; i < pts.Length; i++)
            {
                decimal d = _diem[i].Value;
                Color c = Theme.StatusColor(BLL.Services.HocTapService.XepLoai(d));
                using (var b = new SolidBrush(Theme.Surface)) g.FillEllipse(b, pts[i].X - 5, pts[i].Y - 5, 10, 10);
                using (var p = new Pen(c, 2.5f)) g.DrawEllipse(p, pts[i].X - 4, pts[i].Y - 4, 8, 8);
            }

            // Ngày đầu và ngày cuối
            TextRenderer.DrawText(g, _diem[0].Key.ToString("dd/MM"), Theme.Small,
                new Point((int)pts[0].X - 14, vung.Bottom + 6), Theme.TextMuted);
            if (_diem.Count > 1)
                TextRenderer.DrawText(g, _diem[_diem.Count - 1].Key.ToString("dd/MM"), Theme.Small,
                    new Point((int)pts[pts.Length - 1].X - 30, vung.Bottom + 6), Theme.TextMuted);
        }
    }
}
