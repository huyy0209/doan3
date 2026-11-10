using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Ô chọn (ComboBox) tự vẽ cho rõ ràng, thay cho kiểu phẳng mặc định trắng mờ:
    ///  - viền xám đậm, rê chuột / đang chọn thì viền tím; nút mũi tên rõ ràng bên phải;
    ///  - chữ đang chọn không bị tô xanh dương của hệ thống;
    ///  - bộ lọc (LaBoLoc) khác giá trị mặc định thì nền tím nhạt + chữ đậm => nhìn là biết đang lọc;
    ///  - chưa chọn gì thì hiện chữ gợi ý (GoiY) màu nhạt.
    /// </summary>
    public class ThemedComboBox : ComboBox
    {
        private const int WM_PAINT = 0x000F;
        private const int RongMuiTen = 28;
        private bool _hover;

        /// <summary>Ô này là bộ lọc danh sách</summary>
        public bool LaBoLoc { get; set; }

        /// <summary>Mục mặc định của bộ lọc (không tô nền khi đang chọn mục này)</summary>
        public int MucMacDinh { get; set; }

        /// <summary>Chữ hiện khi chưa chọn mục nào</summary>
        public string GoiY { get; set; }

        /// <summary>Bộ lọc đang khác mặc định</summary>
        public bool DangLoc => LaBoLoc && SelectedIndex >= 0 && SelectedIndex != MucMacDinh;

        public ThemedComboBox()
        {
            FlatStyle = FlatStyle.Flat;
            Font = Theme.Body;
            BackColor = Theme.Surface;
            ForeColor = Theme.Text;
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 21;   // cao gần bằng ô nhập chữ cùng hàng
        }

        /// <summary>Đánh dấu là bộ lọc, lấy mục đang chọn làm mặc định</summary>
        public void DatLaBoLoc()
        {
            LaBoLoc = true;
            MucMacDinh = Math.Max(0, SelectedIndex);
            Invalidate();
        }

        private Color MauNen => !Enabled ? Theme.PaperAlt : DangLoc ? Theme.InkSoft : Theme.Surface;

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            var g = e.Graphics;
            bool oSua = (e.State & DrawItemState.ComboBoxEdit) != 0;
            bool chon = (e.State & DrawItemState.Selected) != 0;

            Color nen = oSua ? MauNen : chon ? Theme.RowSelect : Theme.Surface;
            using (var b = new SolidBrush(nen)) g.FillRectangle(b, e.Bounds);

            string chu;
            Color mau;
            Font font = Font;
            if (e.Index < 0)
            {
                chu = oSua ? GoiY : null;
                mau = Theme.TextFaint;
            }
            else
            {
                chu = GetItemText(Items[e.Index]);
                mau = !Enabled ? Theme.TextFaint : oSua && DangLoc ? Theme.Ink : Theme.Text;
                if (oSua && DangLoc) font = Theme.BodyStrong;
                if (!oSua && chon) font = Theme.BodyStrong;
            }
            if (string.IsNullOrEmpty(chu)) return;

            var vung = new Rectangle(e.Bounds.X + (oSua ? 6 : 10), e.Bounds.Y, e.Bounds.Width - (oSua ? 8 : 14), e.Bounds.Height);
            TextRenderer.DrawText(g, chu, font, vung, mau,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            // Không vẽ khung chấm chấm (focus) của hệ thống
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT && IsHandleCreated) VeKhung();
        }

        /// <summary>Viền + nút mũi tên, vẽ đè lên phần hệ thống vẽ</summary>
        private void VeKhung()
        {
            using (var g = Graphics.FromHwnd(Handle))
            {
                var r = ClientRectangle;
                if (r.Width < 10 || r.Height < 10) return;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Nút mũi tên
                var nut = new Rectangle(r.Right - RongMuiTen, r.Top + 1, RongMuiTen - 1, r.Height - 2);
                using (var b = new SolidBrush(MauNen)) g.FillRectangle(b, nut);
                bool noi = Focused || DroppedDown || _hover;
                using (var vach = new Pen(Theme.Rule)) g.DrawLine(vach, nut.Left, nut.Top + 5, nut.Left, nut.Bottom - 5);
                float cx = nut.Left + nut.Width / 2f, cy = nut.Top + nut.Height / 2f;
                using (var p = new Pen(!Enabled ? Theme.TextFaint : noi || DangLoc ? Theme.Ink : Theme.TextMuted, 1.8f)
                       { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(p, cx - 4.5f, cy - 2f, cx, cy + 2.5f);
                    g.DrawLine(p, cx, cy + 2.5f, cx + 4.5f, cy - 2f);
                }

                // Viền
                g.SmoothingMode = SmoothingMode.None;
                Color vien = !Enabled ? Theme.Rule
                           : Focused || DroppedDown ? Theme.Ink
                           : _hover ? Theme.InkHover
                           : DangLoc ? Theme.Ink
                           : Theme.RuleStrong;
                using (var p = new Pen(vien)) g.DrawRectangle(p, 0, 0, r.Width - 1, r.Height - 1);
                if (Focused || DroppedDown)
                    using (var p = new Pen(Color.FromArgb(70, Theme.Ink))) g.DrawRectangle(p, 1, 1, r.Width - 3, r.Height - 3);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void OnDropDownClosed(EventArgs e) { Invalidate(); base.OnDropDownClosed(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnSelectedIndexChanged(EventArgs e) { Invalidate(); base.OnSelectedIndexChanged(e); }
    }
}
