using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.Common;

namespace DoAnWD.UI.Controls
{
    /// <summary>
    /// Chọn thứ học trong tuần bằng 7 ô bấm (T2 ... CN). Chọn đủ 7 ô = học hằng ngày.
    /// GiaTri là chuỗi lưu vào CSDL ("2,4,6"; null = hằng ngày).
    /// </summary>
    public class ChonThu : Control
    {
        private readonly HashSet<int> _chon = new HashSet<int>(Constants.ThuHoc.TatCa);
        private int _hover = -1;

        public event EventHandler ThayDoi;

        public ChonThu()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Height = 36;
            Dock = DockStyle.Top;
            Margin = new Padding(0, 0, 0, 4);
            Cursor = Cursors.Hand;
        }

        public string GiaTri
        {
            get { return Constants.ThuHoc.Ghi(_chon); }
            set
            {
                var ds = Constants.ThuHoc.Doc(value);
                _chon.Clear();
                foreach (int t in ds.Count == 0 ? (IEnumerable<int>)Constants.ThuHoc.TatCa : ds) _chon.Add(t);
                Invalidate();
            }
        }

        /// <summary>Có ít nhất một thứ được chọn</summary>
        public bool HopLe => _chon.Count > 0;

        public string MoTa => _chon.Count == 0 ? "Chưa chọn thứ" : Constants.ThuHoc.MoTa(GiaTri);

        private RectangleF O(int i)
        {
            float rong = Math.Min(46f, (Width - 6 * 5f) / 7f);
            return new RectangleF(i * (rong + 5), 2, rong, Height - 5);
        }

        private int Tim(Point p)
        {
            for (int i = 0; i < 7; i++) if (O(i).Contains(p)) return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int h = Tim(e.Location);
            if (h != _hover) { _hover = h; Invalidate(); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int i = Tim(e.Location);
            if (i >= 0 && Enabled)
            {
                int thu = Constants.ThuHoc.TatCa[i];
                if (!_chon.Remove(thu)) _chon.Add(thu);
                Invalidate();
                ThayDoi?.Invoke(this, EventArgs.Empty);
            }
            base.OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Theme.Surface);
            for (int i = 0; i < 7; i++)
            {
                int thu = Constants.ThuHoc.TatCa[i];
                bool chon = _chon.Contains(thu);
                var r = O(i);
                using (var path = Theme.RoundedRect(new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1, r.Height - 1), 8))
                {
                    Color nen = !Enabled ? Theme.NeutralSoft : chon ? Theme.Ink : _hover == i ? Theme.InkSoft : Theme.Surface;
                    using (var b = new SolidBrush(nen)) g.FillPath(b, path);
                    if (!chon) using (var p = new Pen(_hover == i ? Theme.Ink : Theme.RuleStrong)) g.DrawPath(p, path);
                }
                TextRenderer.DrawText(g, Constants.ThuHoc.Ten(thu), chon ? Theme.BodyStrong : Theme.Body, Rectangle.Round(r),
                    !Enabled ? Theme.TextFaint : chon ? Color.White : Theme.Text,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }
    }
}
