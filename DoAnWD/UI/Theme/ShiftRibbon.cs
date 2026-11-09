using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DoAnWD.Common;

namespace DoAnWD.UI
{
    /// <summary>
    /// Dải 6 ca học trong ngày trên thanh tiêu đề.
    /// Ca đang diễn ra được tô đặc màu ca; mỗi ca hiện số lớp đang học hôm nay.
    /// </summary>
    public class ShiftRibbon : Control
    {
        private Dictionary<string, int> _soLop = new Dictionary<string, int>();
        private readonly ToolTip _tip = new ToolTip();
        private int _hoverIndex = -1;

        public ShiftRibbon()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            Height = 44;
            BackColor = Theme.Surface;
        }

        /// <summary>Số lớp đang diễn ra hôm nay theo từng ca</summary>
        public void SetSoLop(Dictionary<string, int> soLop)
        {
            _soLop = soLop ?? new Dictionary<string, int>();
            Invalidate();
        }

        /// <summary>Ca đang diễn ra lúc này, null nếu ngoài giờ học</summary>
        public static string CaHienTai(DateTime now)
        {
            foreach (var ca in Constants.CaHoc.TatCaCaHoc)
            {
                int bd, kt;
                if (Constants.CaHoc.TryGetGio(ca, out bd, out kt) && now.Hour >= bd && now.Hour < kt)
                    return ca;
            }
            return null;
        }

        private RectangleF SegmentRect(int i, int count)
        {
            const float gap = 4f;
            float w = (Width - gap * (count - 1)) / count;
            return new RectangleF(i * (w + gap), 0.5f, w - 1, Height - 1.5f);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var cas = Constants.CaHoc.TatCaCaHoc;
            int idx = -1;
            for (int i = 0; i < cas.Length; i++)
                if (SegmentRect(i, cas.Length).Contains(e.Location)) idx = i;

            if (idx == _hoverIndex) return;
            _hoverIndex = idx;
            if (idx >= 0)
            {
                int n;
                _soLop.TryGetValue(cas[idx], out n);
                _tip.SetToolTip(this, n > 0
                    ? $"{cas[idx]}: {n} lớp đang học hôm nay"
                    : $"{cas[idx]}: chưa có lớp nào hôm nay");
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            var cas = Constants.CaHoc.TatCaCaHoc;
            string caHienTai = CaHienTai(DateTime.Now);

            for (int i = 0; i < cas.Length; i++)
            {
                string ca = cas[i];
                bool current = ca == caHienTai;
                Color color = Theme.ShiftColor(ca);
                var r = SegmentRect(i, cas.Length);

                using (var path = Theme.RoundedRect(r, 5))
                {
                    Color fill = current ? color : Theme.Blend(color, Color.White, 0.88f);
                    using (var b = new SolidBrush(fill)) g.FillPath(b, path);
                }

                int bd, kt;
                Constants.CaHoc.TryGetGio(ca, out bd, out kt);
                Color fore = current ? Color.White : Theme.Blend(color, Theme.Text, 0.35f);
                Color foreMuted = current ? Theme.Blend(color, Color.White, 0.75f) : Theme.TextMuted;

                var inner = Rectangle.Round(new RectangleF(r.X + 10, r.Y + 4, r.Width - 20, r.Height - 8));
                int n;
                bool coLop = _soLop.TryGetValue(ca, out n) && n > 0;
                int textW = inner.Width - (coLop ? 22 : 0); // chừa chỗ cho số lớp bên phải
                var top = new Rectangle(inner.X, inner.Y, textW, inner.Height / 2);
                var bottom = new Rectangle(inner.X, inner.Y + inner.Height / 2, textW, inner.Height / 2);

                TextRenderer.DrawText(g, Theme.ShiftShortName(ca), Theme.SmallStrong, top, fore,
                    TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(g, $"{bd}h–{kt}h", Theme.Small, bottom, foreMuted,
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.SingleLine);

                if (coLop)
                    TextRenderer.DrawText(g, n.ToString(), Theme.Section, inner, fore,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tip.Dispose();
            base.Dispose(disposing);
        }
    }
}
