using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DoAnWD.Common;

namespace DoAnWD.UI.Controls
{
    /// <summary>
    /// Chọn một trong 6 ca học bằng lưới ô bấm (2 cột x 3 hàng).
    /// Ca mà học viên đang bận được tô xám kèm chữ "Bận" (vẫn bấm được để xem lý do).
    /// </summary>
    public class ChonCa : Control
    {
        private const int Cot = 2, CaoO = 36, Ke = 6;
        private string _giaTri;
        private int _hover = -1;

        public event EventHandler ThayDoi;

        /// <summary>Hàm cho biết một ca đang bận (trùng lịch) hay không</summary>
        public Func<string, bool> Ban { get; set; }

        public ChonCa()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            int hang = (Constants.CaHoc.TatCaCaHoc.Length + Cot - 1) / Cot;
            Height = hang * CaoO + (hang - 1) * Ke + 2;
            Dock = DockStyle.Top;
            Margin = new Padding(0, 0, 0, 4);
            Cursor = Cursors.Hand;
        }

        /// <summary>Ca đang chọn (chuỗi đầy đủ như trong Constants.CaHoc), null nếu chưa chọn</summary>
        public string GiaTri
        {
            get { return _giaTri; }
            set { if (_giaTri == value) return; _giaTri = value; Invalidate(); ThayDoi?.Invoke(this, EventArgs.Empty); }
        }

        private RectangleF O(int i)
        {
            float rong = (Width - (Cot - 1) * Ke) / (float)Cot;
            return new RectangleF(i % Cot * (rong + Ke), i / Cot * (CaoO + Ke), rong, CaoO);
        }

        private int Tim(Point p)
        {
            for (int i = 0; i < Constants.CaHoc.TatCaCaHoc.Length; i++) if (O(i).Contains(p)) return i;
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
            if (i >= 0) GiaTri = Constants.CaHoc.TatCaCaHoc[i];
            base.OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Theme.Surface);
            var cas = Constants.CaHoc.TatCaCaHoc;
            for (int i = 0; i < cas.Length; i++)
            {
                string ca = cas[i];
                bool chon = ca == _giaTri, ban = Ban != null && Ban(ca);
                var r = O(i);
                using (var path = Theme.RoundedRect(new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1, r.Height - 1), 8))
                {
                    Color nen = chon ? Theme.Ink : ban ? Theme.NeutralSoft : _hover == i ? Theme.InkSoft : Theme.Surface;
                    using (var b = new SolidBrush(nen)) g.FillPath(b, path);
                    if (!chon) using (var p = new Pen(_hover == i && !ban ? Theme.Ink : ban ? Theme.Rule : Theme.RuleStrong)) g.DrawPath(p, path);
                }

                using (var b = new SolidBrush(chon ? Color.White : ban ? Theme.RuleStrong : Theme.ShiftColor(ca)))
                    g.FillEllipse(b, r.X + 11, r.Y + r.Height / 2f - 4, 8, 8);

                int bd, kt;
                Constants.CaHoc.TryGetGio(ca, out bd, out kt);
                string phu = ban ? "Bận" : $"{bd}h–{kt}h";
                int rongPhu = TextRenderer.MeasureText(phu, Theme.Small).Width;
                var oPhu = Rectangle.Round(new RectangleF(r.Right - rongPhu - 8, r.Y, rongPhu, r.Height));
                TextRenderer.DrawText(g, phu, Theme.Small, oPhu, chon ? Theme.Blend(Theme.Ink, Color.White, 0.8f) : ban ? Theme.Margin : Theme.TextMuted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, Constants.CaHoc.TenNgan(ca), chon ? Theme.BodyStrong : Theme.Body,
                    Rectangle.Round(new RectangleF(r.X + 26, r.Y, r.Width - 26 - rongPhu - 14, r.Height)),
                    chon ? Color.White : ban ? Theme.TextFaint : Theme.Text,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            }
        }
    }
}
