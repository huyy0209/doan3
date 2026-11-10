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
    /// Ca đang diễn ra tô xanh đặc; mỗi ca hiện số lớp học hôm nay (không có lớp thì hiện giờ học).
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
            Invalidate();
            if (idx >= 0)
            {
                int n;
                _soLop.TryGetValue(cas[idx], out n);
                _tip.SetToolTip(this, n > 0
                    ? $"{cas[idx]}: {n} lớp học hôm nay"
                    : $"{cas[idx]}: chưa có lớp nào hôm nay");
            }
        }

        protected override void OnMouseLeave(EventArgs e) { _hoverIndex = -1; Invalidate(); base.OnMouseLeave(e); }

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
                var r = SegmentRect(i, cas.Length);

                // Ca đang diễn ra: xanh đặc; ca khác: nền giấy nhạt
                using (var path = Theme.RoundedRect(r, 8))
                using (var b = new SolidBrush(current ? Theme.Ink : _hoverIndex == i ? Theme.InkSoft : Theme.Paper))
                    g.FillPath(b, path);

                int bd, kt;
                Constants.CaHoc.TryGetGio(ca, out bd, out kt);
                int n;
                bool coLop = _soLop.TryGetValue(ca, out n) && n > 0;
                Color fore = current ? Color.White : Theme.Text;
                Color foreMuted = current ? Theme.Blend(Theme.Ink, Color.White, 0.78f) : coLop ? Theme.Ink : Theme.TextFaint;

                var inner = Rectangle.Round(new RectangleF(r.X + 10, r.Y + 5, r.Width - 16, r.Height - 10));
                var top = new Rectangle(inner.X, inner.Y, inner.Width, inner.Height / 2);
                var bottom = new Rectangle(inner.X, inner.Y + inner.Height / 2, inner.Width, inner.Height / 2);
                const TextFormatFlags co = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
                TextRenderer.DrawText(g, Constants.CaHoc.TenNgan(ca), Theme.SmallStrong, top, fore, co);
                // Dòng 2: số lớp hôm nay, không có lớp thì ghi giờ học
                TextRenderer.DrawText(g, coLop ? $"{n} lớp" : $"{bd}h–{kt}h", coLop ? Theme.SmallStrong : Theme.Small, bottom, foreMuted, co);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tip.Dispose();
            base.Dispose(disposing);
        }
    }
}
