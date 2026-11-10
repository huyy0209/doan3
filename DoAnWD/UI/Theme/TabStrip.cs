using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Thanh tab thay cho phần tiêu đề tab của TabControl (Windows vẽ khung 3D cũ kỹ):
    /// chữ trên nền giấy, tab đang chọn gạch chân mực tím, dòng kẻ mảnh phía dưới.
    /// TabControl bên dưới bị ẩn tiêu đề, chỉ còn nội dung trang.
    /// </summary>
    public class TabStrip : Control
    {
        private readonly TabControl _tabs;
        private int _hover = -1;


        public TabStrip(TabControl tabs)
        {
            _tabs = tabs;
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            Height = 42;
            Dock = DockStyle.Top;
            BackColor = Theme.Paper;
            Cursor = Cursors.Hand;
            TabStop = true;
            _tabs.SelectedIndexChanged += (s, e) => Invalidate();
        }

        /// <summary>
        /// Ẩn tiêu đề tab gốc và chèn TabStrip ngay phía trên TabControl
        /// </summary>
        public static TabStrip Attach(TabControl tabs)
        {
            var parent = tabs.Parent;
            if (parent == null) return null;

            tabs.Appearance = TabAppearance.FlatButtons;
            tabs.ItemSize = new Size(0, 1);
            tabs.SizeMode = TabSizeMode.Fixed;

            var strip = new TabStrip(tabs) { Padding = new Padding(tabs.Left + 20, 0, 0, 0) };
            parent.Controls.Add(strip);
            // Thứ tự dock: control có chỉ số lớn hơn được dock trước
            int idx = parent.Controls.GetChildIndex(tabs);
            parent.Controls.SetChildIndex(strip, idx + 1);
            return strip;
        }

        private int TabWidth(int i)
        {
            return TextRenderer.MeasureText(_tabs.TabPages[i].Text, Theme.BodyStrong, Size.Empty, TextFormatFlags.NoPrefix).Width + 40;
        }

        private Rectangle TabRect(int i)
        {
            int x = Padding.Left;
            for (int k = 0; k < i; k++) x += TabWidth(k);
            return new Rectangle(x, 0, TabWidth(i), Height);
        }

        private int HitTest(Point p)
        {
            for (int i = 0; i < _tabs.TabPages.Count; i++)
                if (TabRect(i).Contains(p)) return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int h = HitTest(e.Location);
            if (h != _hover) { _hover = h; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int h = HitTest(e.Location);
            if (h >= 0) _tabs.SelectedIndex = h;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Left || keyData == Keys.Right || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            int n = _tabs.TabPages.Count;
            if (e.KeyCode == Keys.Right) _tabs.SelectedIndex = (_tabs.SelectedIndex + 1) % n;
            if (e.KeyCode == Keys.Left) _tabs.SelectedIndex = (_tabs.SelectedIndex + n - 1) % n;
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);

            using (var rule = new Pen(Theme.Rule))
                g.DrawLine(rule, Padding.Left, Height - 1, Width, Height - 1);

            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                var r = TabRect(i);
                bool selected = i == _tabs.SelectedIndex;
                Color fore = selected ? Theme.Ink : i == _hover ? Theme.Text : Theme.TextMuted;

                TextRenderer.DrawText(g, _tabs.TabPages[i].Text, selected ? Theme.BodyStrong : Theme.Body,
                    r, fore, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

                if (selected)
                {
                    using (var b = new SolidBrush(Theme.Ink))
                        g.FillRectangle(b, r.X + 12, Height - 3, r.Width - 24, 3);

                    if (Focused && ShowFocusCues)
                        using (var p = new Pen(Theme.Ink) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                            g.DrawRectangle(p, r.X + 6, 6, r.Width - 13, Height - 14);
                }
            }
        }
    }
}
