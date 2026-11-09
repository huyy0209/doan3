using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    public enum ButtonKind
    {
        Primary,    // hành động chính: Thêm, Ghép lớp
        Secondary,  // hành động phụ: Sửa, Hủy thao tác
        Danger,     // xóa / hủy lớp
        Ghost       // làm mới, điều hướng
    }

    /// <summary>
    /// Nút bo góc tự vẽ: không viền 3D, có trạng thái hover / nhấn / focus / disabled.
    /// Kế thừa Button nên vẫn dùng được trong Designer và các sự kiện Click cũ.
    /// </summary>
    public class ThemedButton : Button
    {
        private bool _hover;
        private bool _pressed;
        private ButtonKind _kind = ButtonKind.Primary;

        public ThemedButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Font = Theme.Button;
        }

        /// <summary>Icon vẽ bên trái chữ (giúp nhận ra nút nhanh hơn)</summary>
        [DefaultValue(Glyph.None)]
        public Glyph Glyph { get; set; } = Glyph.None;

        [DefaultValue(ButtonKind.Primary)]
        public ButtonKind Kind
        {
            get { return _kind; }
            set { _kind = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        private void GetColors(out Color back, out Color fore, out Color border)
        {
            switch (_kind)
            {
                case ButtonKind.Secondary:
                    // Nền tím nhạt, chữ mực tím: vẫn nổi trên nền trắng nhưng kém nút chính
                    back = _pressed ? Theme.InkSelect : _hover ? Theme.Blend(Theme.InkSoft, Theme.InkSelect, 0.6f) : Theme.InkSoft;
                    fore = Theme.InkHover;
                    border = Color.Empty;
                    break;
                case ButtonKind.Danger:
                    back = _pressed ? Theme.Blend(Theme.Margin, Color.Black, 0.15f)
                         : _hover ? Theme.Margin : Theme.MarginSoft;
                    fore = _hover || _pressed ? Color.White : Theme.Margin;
                    border = Color.Empty;
                    break;
                case ButtonKind.Ghost:
                    back = _pressed ? Theme.InkSoft : _hover ? Theme.Blend(Theme.InkSoft, Color.White, 0.4f) : Color.Empty;
                    fore = Theme.Ink;
                    border = Color.Empty;
                    break;
                default:
                    back = _pressed ? Theme.InkDark : _hover ? Theme.InkHover : Theme.Ink;
                    fore = Color.White;
                    border = Color.Empty;
                    break;
            }

            if (!Enabled)
            {
                back = _kind == ButtonKind.Ghost ? Color.Empty : Theme.NeutralSoft;
                fore = Theme.TextFaint;
                border = Color.Empty;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Theme.Paper);

            Color back, fore, border;
            GetColors(out back, out fore, out border);

            var rect = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (var path = Theme.RoundedRect(rect, Theme.Radius))
            {
                if (back != Color.Empty)
                    using (var b = new SolidBrush(back)) g.FillPath(b, path);
                if (border != Color.Empty)
                    using (var p = new Pen(border)) g.DrawPath(p, path);
            }

            // Vòng focus khi dùng bàn phím (Tab)
            if (Focused && ShowFocusCues && Enabled)
            {
                var focusRect = new RectangleF(2.5f, 2.5f, Width - 5.5f, Height - 5.5f);
                using (var path = Theme.RoundedRect(focusRect, Theme.Radius - 2))
                using (var p = new Pen(_kind == ButtonKind.Primary ? Color.White : Theme.Ink) { DashStyle = DashStyle.Dot })
                    g.DrawPath(p, path);
            }

            // Icon + chữ căn giữa như một khối. Nút đang ở chế độ "Lưu" luôn dùng dấu tích.
            Glyph glyph = Text.StartsWith("Lưu") ? Glyph.Check : Glyph;
            const int iconSize = 16, gap = 7;
            var textSize = TextRenderer.MeasureText(g, Text, Font, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            bool coIcon = glyph != Glyph.None && textSize.Width + iconSize + gap + 16 <= Width;
            int total = textSize.Width + (coIcon ? iconSize + gap : 0);
            int x = Math.Max(6, (Width - total) / 2);

            if (coIcon)
            {
                Icons.Draw(g, glyph, new RectangleF(x, (Height - iconSize) / 2f, iconSize, iconSize), fore);
                x += iconSize + gap;
            }

            var textRect = new Rectangle(x, 0, Width - x - 4, Height);
            TextRenderer.DrawText(g, Text, Font, textRect, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }
}
