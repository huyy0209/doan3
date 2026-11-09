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
            Height = 44;
            Dock = DockStyle.Top;
            BackColor = Theme.InkDark;
        }

        [DefaultValue(false)]
        public bool Active
        {
            get { return _active; }
            set { _active = value; Invalidate(); }
        }

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
            Color back = _active ? Theme.Blend(Theme.InkDark, Color.White, 0.10f)
                       : _hover ? Theme.Blend(Theme.InkDark, Color.White, 0.05f)
                       : Theme.InkDark;
            g.Clear(back);

            var textRect = new Rectangle(28, 0, Width - 40, Height);
            TextRenderer.DrawText(g, Text, _active ? Theme.NavActive : Theme.Nav, textRect,
                _active ? Color.White : Theme.InkMuted,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine);

            if (!string.IsNullOrEmpty(Badge))
                TextRenderer.DrawText(g, Badge, Theme.SmallStrong, textRect, Theme.InkMuted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.SingleLine);

            if (Focused && ShowFocusCues)
                using (var p = new Pen(Theme.InkMuted) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                    g.DrawRectangle(p, 6, 4, Width - 13, Height - 9);
        }
    }
}
