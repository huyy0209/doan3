using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Hàm dựng control đã áp giao diện, dùng cho các màn hình mới dựng bằng code
    /// (bố cục bằng TableLayoutPanel để tự co giãn theo cửa sổ).
    /// </summary>
    public static class Ui
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        /// <summary>Chữ gợi ý mờ trong ô nhập (placeholder) - tính năng có sẵn của Windows</summary>
        public static void SetPlaceholder(TextBox t, string text)
        {
            EventHandler apply = (s, e) =>
            {
                try { SendMessage(t.Handle, EM_SETCUEBANNER, (IntPtr)1, text); }
                catch { /* không phải Windows: bỏ qua */ }
            };
            if (t.IsHandleCreated) apply(t, EventArgs.Empty);
            else t.HandleCreated += apply;
        }

        public static Label Title(string text)
        {
            return new Label { Text = text, Font = Theme.PageTitle, ForeColor = Theme.Text, AutoSize = true, Margin = new Padding(0, 0, 0, 2), UseMnemonic = false };
        }

        public static Label Text(string text, bool muted = false)
        {
            return new Label
            {
                Text = text, Font = Theme.Body, AutoSize = true,
                ForeColor = muted ? Theme.TextMuted : Theme.Text, Margin = new Padding(0, 4, 0, 4), UseMnemonic = false
            };
        }

        public static Label FieldLabel(string text)
        {
            return new Label { Text = text, Font = Theme.Label, ForeColor = Theme.Text, AutoSize = true, Margin = new Padding(0, 8, 0, 3), UseMnemonic = false };
        }

        public static TextBox Input(string placeholder = null, int width = 0)
        {
            var t = new TextBox { Font = Theme.Body, BorderStyle = BorderStyle.Fixed3D, Margin = new Padding(0, 0, 0, 4) };
            if (width > 0) t.Width = width; else t.Dock = DockStyle.Top;
            if (placeholder != null) SetPlaceholder(t, placeholder);
            t.Enter += (s, e) => { if (!t.ReadOnly) t.BackColor = Theme.InkFocus; };
            t.Leave += (s, e) => { if (!t.ReadOnly) t.BackColor = Theme.Surface; };
            return t;
        }

        public static ComboBox Combo(bool editable = false, int width = 0)
        {
            var c = new ComboBox
            {
                Font = Theme.Body, FlatStyle = FlatStyle.Flat,
                DropDownStyle = editable ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 0, 0, 4)
            };
            if (width > 0) c.Width = width; else c.Dock = DockStyle.Top;
            return c;
        }

        public static DateTimePicker Date(int width = 150)
        {
            return new DateTimePicker
            {
                Font = Theme.Body, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy",
                Width = width, Margin = new Padding(0, 0, 8, 4),
                CalendarTitleBackColor = Theme.Ink, CalendarTitleForeColor = Color.White
            };
        }

        public static ThemedButton Button(string text, ButtonKind kind, Glyph glyph = Glyph.None, int width = 0)
        {
            var b = new ThemedButton { Text = text, Kind = kind, Glyph = glyph, Height = 40, Margin = new Padding(0, 4, 8, 4) };
            b.Width = width > 0 ? width : Math.Max(96, TextRenderer.MeasureText(text, Theme.Button).Width + (glyph != Glyph.None ? 56 : 36));
            return b;
        }

        public static SectionBox Section(string title)
        {
            return new SectionBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(14, 36, 14, 12), Margin = new Padding(0, 0, 12, 0) };
        }

        public static DataGridView Grid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, MultiSelect = false
            };
            ThemeManager.StyleGrid(g);
            return g;
        }

        public static FlowLayoutPanel Row(params Control[] controls)
        {
            var f = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false,
                Dock = DockStyle.Top, Margin = new Padding(0), Padding = new Padding(0)
            };
            f.Controls.AddRange(controls);
            return f;
        }

        /// <summary>Bảng dọc: mỗi control một hàng, hàng cuối (nếu fillLast) chiếm phần còn lại</summary>
        public static TableLayoutPanel Stack(bool fillLast, params Control[] controls)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Margin = new Padding(0), Padding = new Padding(0) };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < controls.Length; i++)
            {
                bool last = i == controls.Length - 1;
                t.RowStyles.Add(last && fillLast ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.AutoSize));
                if (controls[i].Dock == DockStyle.None && !(controls[i] is Label)) controls[i].Anchor = AnchorStyles.Left | AnchorStyles.Top;
                if (last && fillLast) controls[i].Dock = DockStyle.Fill;
                t.Controls.Add(controls[i], 0, i);
            }
            ThemeManager.EnableDoubleBuffer(t);
            return t;
        }

        /// <summary>Danh sách tự vẽ, mỗi mục cao itemHeight</summary>
        public static ListBox List(int itemHeight, DrawItemEventHandler draw)
        {
            var l = new ListBox
            {
                DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = itemHeight, BorderStyle = BorderStyle.None,
                IntegralHeight = false, Dock = DockStyle.Fill, BackColor = Theme.Surface, Font = Theme.Body
            };
            l.DrawItem += draw;
            ThemeManager.EnableDoubleBuffer(l);
            return l;
        }

        /// <summary>Nền + đường kẻ dưới cho một mục danh sách tự vẽ; trả về vùng nội dung</summary>
        public static Rectangle DrawListItemBackground(DrawItemEventArgs e, bool dim = false)
        {
            bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (var b = new SolidBrush(sel ? Theme.RowSelect : dim ? Theme.PaperAlt : Theme.Surface))
                e.Graphics.FillRectangle(b, e.Bounds);
            using (var p = new Pen(Theme.Rule))
                e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            if (sel)
                using (var b = new SolidBrush(Theme.Ink))
                    e.Graphics.FillRectangle(b, e.Bounds.Left, e.Bounds.Top, 3, e.Bounds.Height);
            return new Rectangle(e.Bounds.Left + 12, e.Bounds.Top + 4, e.Bounds.Width - 20, e.Bounds.Height - 8);
        }

        /// <summary>Nhãn bo tròn (trạng thái) vẽ căn phải trong vùng r; trả về bề rộng đã dùng</summary>
        public static int DrawTag(Graphics g, string text, Rectangle r, Color fore, Color back)
        {
            var size = TextRenderer.MeasureText(text, Theme.SmallStrong);
            var tag = new RectangleF(r.Right - size.Width - 12, r.Top + (r.Height - 22) / 2f, size.Width + 12, 22);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var path = Theme.RoundedRect(tag, 11))
            using (var b = new SolidBrush(back))
                g.FillPath(b, path);
            TextRenderer.DrawText(g, text, Theme.SmallStrong, Rectangle.Round(tag), fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            return (int)tag.Width + 8;
        }
    }
}
