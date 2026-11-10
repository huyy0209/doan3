using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Mục menu bên trái: chữ căn trái, nền tím đậm; mục đang mở sáng hơn và đậm chữ.
    /// Vạch đỏ chỉ mục đang chọn do TrangChu vẽ và cho trượt giữa các mục.
    /// </summary>
    public class NavButton : Button
    {
        private bool _hover;
        private bool _active;

        public NavButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Height = 46;
            Dock = DockStyle.Top;
            BackColor = Theme.InkDark;
        }

        [DefaultValue(false)]
        public bool Active
        {
            get { return _active; }
            set { _active = value; Invalidate(); }
        }

        [DefaultValue(Glyph.None)]
        public Glyph Glyph { get; set; } = Glyph.None;

        /// <summary>Dòng chữ nhỏ bên phải (ví dụ số lớp), để trống nếu không cần</summary>
        [DefaultValue("")]
        public string Badge { get; set; } = "";

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Theme.Board);

            // Mục đang mở: viên bo tròn sáng hơn nền bảng + vạch phấn vàng bên trái
            var vien = new RectangleF(12, 3, Width - 24, Height - 6);
            if (_active || _hover)
                using (var path = Theme.RoundedRect(vien, 9))
                using (var b = new SolidBrush(Theme.Blend(Theme.Board, Color.White, _active ? 0.13f : 0.06f)))
                    g.FillPath(b, path);
            if (_active)
                using (var path = Theme.RoundedRect(new RectangleF(12, Height / 2f - 9, 4, 18), 2))
                using (var b = new SolidBrush(Theme.Chalk))
                    g.FillPath(b, path);

            Color fore = _active || _hover ? Color.White : Theme.InkMuted;
            Icons.Draw(g, Glyph, new RectangleF(30, (Height - 18) / 2f, 18, 18), _active ? Theme.Chalk : fore);

            var textRect = new Rectangle(60, 0, Width - 76, Height);
            TextRenderer.DrawText(g, Text, _active ? Theme.NavActive : Theme.Nav, textRect, fore,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);

            if (!string.IsNullOrEmpty(Badge))
            {
                // Số việc cần làm: viên tròn vàng phấn
                var co = TextRenderer.MeasureText(Badge, Theme.SmallStrong);
                var o = new RectangleF(Width - 26 - co.Width, Height / 2f - 10, co.Width + 8, 20);
                using (var path = Theme.RoundedRect(o, 10))
                using (var b = new SolidBrush(Theme.Chalk)) g.FillPath(b, path);
                TextRenderer.DrawText(g, Badge, Theme.SmallStrong, Rectangle.Round(o), Theme.InkDarker,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }

            if (Focused && ShowFocusCues)
                using (var path = Theme.RoundedRect(vien, 9))
                using (var p = new Pen(Theme.InkMuted) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                    g.DrawPath(p, path);
        }
    }
}
