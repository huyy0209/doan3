using System;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Áp bộ giao diện "Vở ô li" lên một form (và mọi control con) lúc chạy.
    /// Gọi ThemeManager.Apply(form) TRƯỚC khi form hiển thị; code của form
    /// (tô màu trạng thái, bật/tắt nút...) chạy sau trong sự kiện Load vẫn giữ nguyên.
    /// </summary>
    /// <summary>
    /// Đánh dấu màn hình dựng bằng code đã tự áp giao diện (Ui.*):
    /// ThemeManager.Apply sẽ bỏ qua để không ghi đè font/màu đã chọn.
    /// </summary>
    public interface ITuGiaoDien { }

    /// <summary>Trang được giữ lại trong bộ nhớ: khi mở lại chỉ cần nạp lại dữ liệu</summary>
    public interface ILamMoi { void LamMoi(); }

    public static class ThemeManager
    {
        private static readonly PropertyInfo DoubleBufferedProp =
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Apply(Control root)
        {
            if (root is ITuGiaoDien) return;
            root.SuspendLayout();
            try
            {
                ApplyRecursive(root);
            }
            finally
            {
                root.ResumeLayout(true);
            }
        }

        private static void ApplyRecursive(Control c)
        {
            Style(c);
            // Sao chép danh sách: Style() có thể chèn control mới (TabStrip) vào cây
            var children = new Control[c.Controls.Count];
            c.Controls.CopyTo(children, 0);
            foreach (Control child in children)
                ApplyRecursive(child);
        }

        /// <summary>Bật double buffering cho control (giảm nháy khi vẽ lại / cuộn)</summary>
        public static void EnableDoubleBuffer(Control c)
        {
            if (c is TextBoxBase || c is ComboBox || c is DateTimePicker) return;
            try { DoubleBufferedProp?.SetValue(c, true, null); } catch { /* bỏ qua */ }
        }

        private static void Style(Control c)
        {
            EnableDoubleBuffer(c);

            if (c is Form) { StyleForm((Form)c); return; }
            if (c is NavButton) return;
            if (c is ThemedButton) { StyleThemedButton((ThemedButton)c); return; }
            if (c is Button) { StylePlainButton((Button)c); return; }
            if (c is GroupBox) { StyleGroup((GroupBox)c); return; }
            if (c is DataGridView) { StyleGrid((DataGridView)c); return; }
            if (c is TextBox) { StyleTextBox((TextBox)c); return; }
            if (c is ComboBox) { StyleCombo((ComboBox)c); return; }
            if (c is DateTimePicker) { StyleDate((DateTimePicker)c); return; }
            if (c is ListBox) { StyleList((ListBox)c); return; }
            if (c is TabControl) { StyleTabs((TabControl)c); return; }
            if (c is TabPage) { c.BackColor = Theme.Paper; c.Font = Theme.Body; return; }
            if (c is SplitContainer) { StyleSplit((SplitContainer)c); return; }
            if (c is Label) { StyleLabel((Label)c); return; }
            if (c is Panel) { StylePanel((Panel)c); return; }
        }

        // ------------------------------------------------------------------

        private static void StyleForm(Form f)
        {
            f.BackColor = Theme.Paper;
            f.ForeColor = Theme.Text;
            f.Font = Theme.Body;
        }

        public static ButtonKind KindFor(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            if (n.Contains("xoa") || n.Contains("delete")) return ButtonKind.Danger;
            if (n.Contains("them") || n.Contains("gheplop") || n.Contains("luu") || n.Contains("save")) return ButtonKind.Primary;
            if (n.Contains("refresh") || n.Contains("lammoi") || n.Contains("prev") || n.Contains("next") || n.Contains("today"))
                return ButtonKind.Ghost;
            return ButtonKind.Secondary;
        }

        public static Glyph GlyphFor(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            if (n.Contains("xoa") || n.Contains("delete")) return Glyph.Trash;
            if (n.Contains("them") || n.Contains("gheplop")) return Glyph.Plus;
            if (n.Contains("sua")) return Glyph.Edit;
            if (n.Contains("luu") || n.Contains("save")) return Glyph.Check;
            if (n.Contains("huy")) return Glyph.Close;
            if (n.Contains("refresh") || n.Contains("lammoi")) return Glyph.Refresh;
            if (n.Contains("prev")) return Glyph.ChevronLeft;
            if (n.Contains("next")) return Glyph.ChevronRight;
            if (n.Contains("today")) return Glyph.Calendar;
            return Glyph.None;
        }

        private static void StyleThemedButton(ThemedButton b)
        {
            // Nút dựng bằng code (không có Name) đã tự chọn kiểu => giữ nguyên
            if (string.IsNullOrEmpty(b.Name)) { b.Font = Theme.Button; return; }
            b.Kind = KindFor(b.Name);
            if (b.Glyph == Glyph.None) b.Glyph = GlyphFor(b.Name);
            b.Font = Theme.Button;
            // Nút quá thấp khó bấm: tối thiểu 38px
            if (b.Height < 38 && b.Dock == DockStyle.None) b.Height = 38;
        }

        private static void StylePlainButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Theme.Rule;
            b.BackColor = Theme.Surface;
            b.ForeColor = Theme.Text;
            b.Font = Theme.Button;
            b.Cursor = Cursors.Hand;
        }

        private static void StyleGroup(GroupBox g)
        {
            g.BackColor = Theme.Surface;
            g.ForeColor = Theme.Text;
            g.Font = Theme.Section;
        }

        public static void StyleGrid(DataGridView dgv)
        {
            EnableDoubleBuffer(dgv);
            dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = Theme.Surface;
            dgv.GridColor = Theme.Rule;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToResizeRows = false;

            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = Theme.HeaderHeight;
            var head = dgv.ColumnHeadersDefaultCellStyle;
            head.BackColor = Theme.Surface;
            head.ForeColor = Theme.TextMuted;
            head.SelectionBackColor = Theme.Surface;
            head.SelectionForeColor = Theme.TextMuted;
            head.Font = Theme.Label;
            head.ForeColor = Theme.Ink;
            head.Padding = new Padding(8, 0, 8, 0);
            head.Alignment = DataGridViewContentAlignment.MiddleLeft;
            head.WrapMode = DataGridViewTriState.False;

            var cell = dgv.DefaultCellStyle;
            cell.BackColor = Theme.Surface;
            cell.ForeColor = Theme.Text;
            cell.Font = Theme.Body;
            cell.SelectionBackColor = Theme.RowSelect;
            cell.SelectionForeColor = Theme.Text;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Theme.RowSelect;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Theme.Text;

            // Cho phép copy chữ: Ctrl+C hoặc chuột phải => "Sao chép ô / dòng"
            dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            if (dgv.ContextMenuStrip == null) dgv.ContextMenuStrip = TaoMenuSaoChep(dgv);
            dgv.CellMouseDown -= Grid_CellMouseDown;
            dgv.CellMouseDown += Grid_CellMouseDown;
            cell.Padding = new Padding(8, 0, 8, 0);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Theme.PaperAlt;
            dgv.RowTemplate.Height = Theme.RowHeight;
            foreach (DataGridViewRow r in dgv.Rows) r.Height = Theme.RowHeight;

            dgv.CellPainting -= PaintSpecialCell;
            dgv.CellPainting += PaintSpecialCell;
            dgv.EditingControlShowing -= Grid_EditingControlShowing;
            dgv.EditingControlShowing += Grid_EditingControlShowing;
            dgv.CurrentCellChanged -= Grid_CurrentCellChanged;
            dgv.CurrentCellChanged += Grid_CurrentCellChanged;
            dgv.DataBindingComplete -= Grid_DataBindingComplete;
            dgv.DataBindingComplete += Grid_DataBindingComplete;
            dgv.SizeChanged -= Grid_SizeChanged;
            dgv.SizeChanged += Grid_SizeChanged;
        }

        // ---------------- Sao chép ----------------

        private static DataGridViewCell _oChuotPhai;

        /// <summary>Chuột phải vào ô: chọn luôn dòng đó để menu sao chép đúng chỗ</summary>
        private static void Grid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var dgv = (DataGridView)sender;
            var o = dgv[e.ColumnIndex, e.RowIndex];
            _oChuotPhai = o;
            if (!o.Selected && o.Visible)
            {
                try { dgv.CurrentCell = o; } catch (InvalidOperationException) { /* đang sửa ô dở dang */ }
            }
        }

        private static ContextMenuStrip TaoMenuSaoChep(DataGridView dgv)
        {
            var menu = new ContextMenuStrip { Font = Theme.Body, ShowImageMargin = false };
            var oItem = new ToolStripMenuItem("Sao chép ô") { ShortcutKeyDisplayString = "" };
            var dongItem = new ToolStripMenuItem("Sao chép dòng") { ShortcutKeyDisplayString = "Ctrl+C" };
            var bangItem = new ToolStripMenuItem("Sao chép cả bảng (dán vào Excel)");
            oItem.Click += (s, e) =>
            {
                var o = _oChuotPhai != null && _oChuotPhai.DataGridView == dgv ? _oChuotPhai : dgv.CurrentCell;
                DatClipboard(o?.FormattedValue?.ToString());
            };
            dongItem.Click += (s, e) =>
            {
                var r = (_oChuotPhai != null && _oChuotPhai.DataGridView == dgv ? _oChuotPhai.OwningRow : null) ?? dgv.CurrentRow;
                if (r == null) return;
                DatClipboard(string.Join("\t", r.Cells.Cast<DataGridViewCell>()
                    .Where(c => c.OwningColumn.Visible)
                    .OrderBy(c => c.OwningColumn.DisplayIndex)
                    .Select(c => c.FormattedValue?.ToString() ?? "")));
            };
            bangItem.Click += (s, e) =>
            {
                var cols = dgv.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToList();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine(string.Join("\t", cols.Select(c => c.HeaderText)));
                foreach (DataGridViewRow r in dgv.Rows)
                    sb.AppendLine(string.Join("\t", cols.Select(c => r.Cells[c.Index].FormattedValue?.ToString() ?? "")));
                DatClipboard(sb.ToString());
            };
            menu.Items.AddRange(new ToolStripItem[] { oItem, dongItem, new ToolStripSeparator(), bangItem });
            menu.Opening += (s, e) =>
            {
                bool coDong = dgv.Rows.Count > 0;
                oItem.Enabled = dongItem.Enabled = coDong && (dgv.CurrentCell != null || _oChuotPhai != null);
                bangItem.Enabled = coDong;
            };
            return menu;
        }

        private static void DatClipboard(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            try { Clipboard.SetText(text); }
            catch (System.Runtime.InteropServices.ExternalException) { /* clipboard đang bị app khác giữ */ }
        }

        private static void Grid_SizeChanged(object sender, EventArgs e)
        {
            SizeColumns((DataGridView)sender);
        }

        // ---------------- Tiêu đề cột ----------------

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<DataGridView, System.Collections.Generic.Dictionary<string, string>>
            _tieuDe = new System.Runtime.CompilerServices.ConditionalWeakTable<DataGridView, System.Collections.Generic.Dictionary<string, string>>();

        /// <summary>
        /// Khai báo tiêu đề tiếng Việt cho các cột (theo tên thuộc tính). Giá trị null = ẩn cột.
        /// Được áp lại mỗi lần bảng nạp dữ liệu, kể cả bảng nằm ở tab chưa mở (lúc đó cột chưa được tạo).
        /// </summary>
        public static void TieuDeCot(DataGridView dgv, params string[] cap)
        {
            var map = new System.Collections.Generic.Dictionary<string, string>();
            for (int i = 0; i + 1 < cap.Length; i += 2) map[cap[i]] = cap[i + 1];
            _tieuDe.Remove(dgv);
            _tieuDe.Add(dgv, map);
            ApTieuDe(dgv);
        }

        private static void ApTieuDe(DataGridView dgv)
        {
            System.Collections.Generic.Dictionary<string, string> map;
            if (!_tieuDe.TryGetValue(dgv, out map)) return;
            foreach (DataGridViewColumn c in dgv.Columns)
            {
                string key = !string.IsNullOrEmpty(c.DataPropertyName) ? c.DataPropertyName : c.Name;
                string ten;
                if (!map.TryGetValue(key, out ten)) continue;
                if (ten == null) c.Visible = false;
                else c.HeaderText = ten;
            }
        }

        private static void Grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var dgv = (DataGridView)sender;
            ApTieuDe(dgv);
            // Chạy sau code cấu hình cột của từng form
            if (dgv.IsHandleCreated) dgv.BeginInvoke((MethodInvoker)(() => SizeColumns(dgv)));
            else SizeColumns(dgv);
        }

        /// <summary>
        /// Độ rộng cột thống nhất: mã / số / ngày / trạng thái cố định, cột chữ (tên, môn...) co giãn
        /// </summary>
        public static void SizeColumns(DataGridView dgv)
        {
            if (dgv.IsDisposed || dgv.Columns.Count == 0) return;

            const int fillDesired = 150;
            var plan = new System.Collections.Generic.List<System.Tuple<DataGridViewColumn, int>>();
            int total = 0;

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (!col.Visible) continue;
                string key = !string.IsNullOrEmpty(col.DataPropertyName) ? col.DataPropertyName : col.Name;
                int width = 0;

                if (key.StartsWith("Ma")) width = 60;
                else if (key == "TrangThai" || key == "TrangThaiTheoThoiGian") width = 124;
                else if (key == "CaHoc") width = 156;
                else if (key == "SDT") width = 112;
                else if (key == "HocPhi" || key.StartsWith("TongHocPhi") || key.StartsWith("DoanhThu")) width = 108;
                else if (key.StartsWith("BatDau") || key.StartsWith("KetThuc") || key.StartsWith("ThoiGian") || key.StartsWith("Ngay")) width = 98;
                else if (key == "SoNgayConLai") width = 86;
                else if (key.StartsWith("So")) width = 70;

                // Cột tên người được ưu tiên rộng hơn khi phải co
                bool laTen = key == "HoTen" || key == "HocVien" || key == "GiaSu" || key == "TenDangNhap";

                if (key == "HocPhi" || key.StartsWith("TongHocPhi"))
                {
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                plan.Add(System.Tuple.Create(col, width > 0 ? width : (laTen ? -190 : 0)));
                total += width > 0 ? width : (laTen ? 190 : fillDesired);
            }

            bool vua = total <= dgv.ClientSize.Width;
            // Toàn cột cố định (ví dụ bảng thống kê theo ca) mà còn dư chỗ => giãn đều theo tỉ lệ để lấp kín bảng
            if (vua && plan.All(t => t.Item2 > 0)) vua = false;
            foreach (var item in plan)
            {
                var col = item.Item1;
                int width = Math.Max(0, item.Item2);
                int trongSo = item.Item2 < 0 ? -item.Item2 : (item.Item2 > 0 ? item.Item2 : fillDesired);
                col.MinimumWidth = 40;

                if (vua && width > 0)
                {
                    // Đủ chỗ: cột số liệu cố định, cột chữ co giãn
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    col.Width = width;
                }
                else
                {
                    // Hẹp: mọi cột co theo tỉ lệ, không hiện thanh cuộn ngang
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    col.FillWeight = trongSo;
                }
            }
        }

        /// <summary>
        /// Tự vẽ mọi ô của bảng để hiển thị luôn đồng nhất (không phụ thuộc viền mặc định của DataGridView,
        /// vốn bị mất / lệch khi click hoặc khi ô vừa sửa xong):
        /// - nền: xanh nhạt nếu dòng đang chọn, ngược lại theo màu dòng;
        /// - ô được sửa (bảng điểm danh): khung như ô nhập, viền xanh khi đang chọn;
        /// - cột trạng thái: nhãn bo tròn; cột ca học: chấm màu theo ca;
        /// - một đường kẻ mảnh dưới mỗi dòng.
        /// </summary>
        private static void PaintSpecialCell(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var dgv = (DataGridView)sender;
            var col = dgv.Columns[e.ColumnIndex];
            var cell = dgv[e.ColumnIndex, e.RowIndex];
            string key = !string.IsNullOrEmpty(col.DataPropertyName) ? col.DataPropertyName : col.Name;
            bool suaDuoc = !dgv.ReadOnly && !cell.ReadOnly;
            bool isStatus = !suaDuoc && (key == "TrangThai" || key == "TrangThaiTheoThoiGian" || key == "XepLoai" ||
                            key == "TinhTrang" || (key == "DiemDanh" && dgv.ReadOnly));
            bool isShift = !suaDuoc && key == "CaHoc";
            bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
            bool oHienTai = dgv.CurrentCell != null && dgv.CurrentCell.RowIndex == e.RowIndex && dgv.CurrentCell.ColumnIndex == e.ColumnIndex;
            string text = e.FormattedValue?.ToString() ?? "";
            var g = e.Graphics;
            var b = e.CellBounds;

            // 1. Nền
            Color nen = selected && !suaDuoc ? Theme.RowSelect : e.CellStyle.BackColor;
            using (var br = new SolidBrush(nen)) g.FillRectangle(br, b);

            // 2. Nội dung
            if (suaDuoc)
                VeONhap(g, b, text, e.CellStyle, oHienTai, cell is DataGridViewComboBoxCell);
            else if (isStatus && text.Length > 0)
                VeNhan(g, b, text);
            else if (isShift && text.Length > 0)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Theme.ShiftColor(text)))
                    g.FillEllipse(brush, b.X + 10, b.Y + b.Height / 2f - 4, 8, 8);
                var textRect = new Rectangle(b.X + 24, b.Y, b.Width - 28, b.Height);
                TextRenderer.DrawText(g, text, e.CellStyle.Font ?? Theme.Body, textRect, e.CellStyle.ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
            else
                e.Paint(b, DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.ContentForeground |
                           DataGridViewPaintParts.ErrorIcon);

            // 3. Kẻ dòng + vạch chọn
            using (var p = new Pen(dgv.GridColor)) g.DrawLine(p, b.Left, b.Bottom - 1, b.Right, b.Bottom - 1);
            if (selected) VeVachChon(dgv, e);
            e.Handled = true;
        }

        private static void VeNhan(Graphics g, Rectangle b, string text)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var size = TextRenderer.MeasureText(text, Theme.SmallStrong);
            float w = Math.Min(size.Width + 12, b.Width - 12);
            var pill = new RectangleF(b.X + 8, b.Y + (b.Height - 22) / 2f, w, 22);
            using (var path = Theme.RoundedRect(pill, 11))
            using (var brush = new SolidBrush(Theme.StatusSoftColor(text)))
                g.FillPath(brush, path);
            var textRect = Rectangle.Round(new RectangleF(pill.X + 6, pill.Y, pill.Width - 10, pill.Height));
            TextRenderer.DrawText(g, text, Theme.SmallStrong, textRect, Theme.StatusColor(text),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }

        /// <summary>Ô sửa được vẽ như một ô nhập: nền trắng, viền xám; ô đang chọn viền xanh</summary>
        private static void VeONhap(Graphics g, Rectangle b, string text, DataGridViewCellStyle st, bool dangChon, bool laCombo)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var khung = new RectangleF(b.X + 4.5f, b.Y + 4.5f, b.Width - 9, b.Height - 10);
            using (var path = Theme.RoundedRect(khung, 4))
            {
                using (var br = new SolidBrush(Theme.Surface)) g.FillPath(br, path);
                using (var p = new Pen(dangChon ? Theme.RowSelectBar : Theme.RuleStrong, dangChon ? 1.6f : 1f)) g.DrawPath(p, path);
            }

            int phai = laCombo ? 22 : 0;
            var vungChu = new Rectangle(b.X + 11, b.Y, b.Width - 22 - phai, b.Height - 1);
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
            var canh = st.Alignment;
            if (canh == DataGridViewContentAlignment.MiddleCenter || canh == DataGridViewContentAlignment.TopCenter ||
                canh == DataGridViewContentAlignment.BottomCenter) flags |= TextFormatFlags.HorizontalCenter;
            else if (canh == DataGridViewContentAlignment.MiddleRight || canh == DataGridViewContentAlignment.TopRight ||
                     canh == DataGridViewContentAlignment.BottomRight) flags |= TextFormatFlags.Right;
            TextRenderer.DrawText(g, text, st.Font ?? Theme.Body, vungChu, st.ForeColor, flags);

            if (laCombo)
            {
                // Mũi tên xổ xuống
                float cx = khung.Right - 12, cy = khung.Y + khung.Height / 2f;
                using (var p = new Pen(Theme.TextMuted, 1.5f))
                {
                    g.DrawLine(p, cx - 4, cy - 2, cx, cy + 2);
                    g.DrawLine(p, cx, cy + 2, cx + 4, cy - 2);
                }
            }
        }

        /// <summary>Vạch xanh đậm ở đầu dòng đang chọn (chỉ vẽ ở cột hiển thị đầu tiên)</summary>
        private static void VeVachChon(DataGridView dgv, DataGridViewCellPaintingEventArgs e)
        {
            if (dgv.SelectionMode != DataGridViewSelectionMode.FullRowSelect) return;
            var dau = dgv.Columns.GetFirstColumn(DataGridViewElementStates.Visible);
            if (dau == null || dau.Index != e.ColumnIndex) return;
            using (var b = new SolidBrush(Theme.RowSelectBar))
                e.Graphics.FillRectangle(b, e.CellBounds.X, e.CellBounds.Y, 3, e.CellBounds.Height - 1);
        }

        /// <summary>Ô nhập của bảng (khi đang gõ) nằm gọn trong khung, nền trắng như ô nhập thường</summary>
        private static void Grid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.CellStyle.BackColor = Theme.Surface;
            e.CellStyle.SelectionBackColor = Theme.Surface;
            var tb = e.Control as TextBox;
            if (tb != null) tb.BorderStyle = BorderStyle.None;
        }

        /// <summary>Đổi ô đang chọn thì vẽ lại hai ô (cũ và mới) để viền xanh đi theo</summary>
        private static void Grid_CurrentCellChanged(object sender, EventArgs e)
        {
            var dgv = (DataGridView)sender;
            if (!dgv.ReadOnly) dgv.Invalidate();
        }

        private static void StyleTextBox(TextBox t)
        {
            // Fixed3D + visual styles = viền mảnh xám của Windows 10/11 (FixedSingle là viền đen)
            t.BorderStyle = BorderStyle.Fixed3D;
            t.Font = t.Font.Size >= 13 ? Theme.NumberSmall : (t.Font.Bold ? Theme.BodyStrong : Theme.Body);
            t.BackColor = t.ReadOnly ? Theme.Paper : Theme.Surface;

            // Tô nền tím rất nhạt cho ô đang gõ => biết ngay con trỏ đang ở đâu
            t.Enter -= TextBox_Enter; t.Enter += TextBox_Enter;
            t.Leave -= TextBox_Leave; t.Leave += TextBox_Leave;

            // Ô tìm kiếm dùng Color.Gray làm dấu hiệu "đang hiện placeholder" => giữ nguyên
            Color fc = t.ForeColor;
            bool oTrangThai = (t.Name ?? "").Contains("TrangThai"); // ô trạng thái tô màu bằng code
            if (fc != Color.Gray && !(oTrangThai && fc == Color.White))
                t.ForeColor = Theme.Text;
        }

        private static void TextBox_Enter(object sender, EventArgs e)
        {
            var t = (TextBox)sender;
            if (!t.ReadOnly && t.Enabled) t.BackColor = Theme.InkFocus;
        }

        private static void TextBox_Leave(object sender, EventArgs e)
        {
            var t = (TextBox)sender;
            if (!t.ReadOnly && t.BackColor == Theme.InkFocus) t.BackColor = Theme.Surface;
        }

        private static void StyleCombo(ComboBox c)
        {
            c.FlatStyle = FlatStyle.Flat;
            c.BackColor = Theme.Surface;
            c.ForeColor = Theme.Text;
            c.Font = Theme.Body;
        }

        private static void StyleDate(DateTimePicker d)
        {
            d.Font = Theme.Body;
            d.CalendarTitleBackColor = Theme.Ink;
            d.CalendarTitleForeColor = Color.White;
            d.CalendarMonthBackground = Theme.Surface;
            d.CalendarForeColor = Theme.Text;
            d.CalendarTrailingForeColor = Theme.TextFaint;
        }

        private static void StyleList(ListBox l)
        {
            l.BorderStyle = BorderStyle.None;
            l.BackColor = Theme.Surface;
            l.ForeColor = Theme.Text;
            if (l.Font.Name != "Consolas") l.Font = Theme.Body;
        }

        private static void StyleSplit(SplitContainer s)
        {
            s.BackColor = Theme.Paper;
            s.Panel1.BackColor = Theme.Paper;
            s.Panel2.BackColor = Theme.Paper;
            s.SplitterWidth = 12;
        }

        private static void StyleLabel(Label l)
        {
            l.BackColor = Color.Transparent;
            float size = l.Font.Size;

            // Nhãn trong ô số liệu: số lớn theo màu của ô
            string parentName = l.Parent?.Name ?? "";
            if (parentName.StartsWith("card"))
            {
                bool laSo = size >= 18;
                l.Font = laSo ? Theme.Number : Theme.Section;
                l.ForeColor = laSo ? CardColor(parentName) : Theme.Text;
                return;
            }

            if (l.Name == "lblTitle")
            {
                l.Font = Theme.PageTitle;
                l.ForeColor = Theme.Text;
                l.AutoSize = true;
            }
            else if (size >= 18)
            {
                l.Font = Theme.Number;
                l.ForeColor = Theme.Text;
            }
            else if (size >= 14)
            {
                l.Font = Theme.NumberSmall;
                l.ForeColor = Theme.Text;
            }
            else if (l.Font.Bold && size >= 11.5f)
            {
                // Tiêu đề khối nội dung
                l.Font = Theme.Section;
                l.ForeColor = Theme.Text;
            }
            else if (l.Font.Bold)
            {
                // Nhãn ô nhập: đậm, màu chữ chính (không dùng xám nhạt khó đọc)
                l.Font = Theme.Label;
                l.ForeColor = Theme.Text;
            }
            else
            {
                l.Font = l.Font.Italic ? new Font(Theme.Family, 9F, FontStyle.Italic) : Theme.Body;
                l.ForeColor = Theme.Text;
            }
        }

        private static bool IsNear(Color c, int r, int g, int b)
        {
            return Math.Abs(c.R - r) < 12 && Math.Abs(c.G - g) < 12 && Math.Abs(c.B - b) < 12;
        }

        private static void StylePanel(Panel p)
        {
            string n = p.Name ?? "";

            if (n == "panelTop" || n == "panelButtons")
            {
                p.BackColor = Theme.Paper;
                return;
            }

            if (n.StartsWith("card"))
            {
                // Ô số liệu: mỗi loại một màu (gia sư tím, học viên xanh, lớp cam), nền nhạt cùng tông
                Color accent = CardColor(n);
                p.BackColor = Theme.Blend(accent, Color.White, 0.9f);
                p.Paint += PaintCard;
                return;
            }

            if (p.BackColor == Color.White || p.BackColor == Theme.Surface)
            {
                p.BackColor = Theme.Surface;
                if (p.Dock != DockStyle.Fill && p.Dock != DockStyle.Top && p.Dock != DockStyle.Bottom)
                    p.Paint += PaintBorder;
                return;
            }

            if (IsNear(p.BackColor, 236, 240, 241) || p.BackColor == SystemColors.Control)
            {
                p.BackColor = Theme.Paper;
                return;
            }

            if (p.BackColor != Color.Transparent)
                p.BackColor = Theme.Paper;
        }

        private static void PaintBorder(object sender, PaintEventArgs e)
        {
            var c = (Control)sender;
            using (var pen = new Pen(Theme.Rule))
                e.Graphics.DrawRectangle(pen, 0, 0, c.Width - 1, c.Height - 1);
        }

        public static Color CardColor(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            if (n.Contains("hv") || n.Contains("hocvien")) return Theme.Success;
            if (n.Contains("lop")) return Theme.Warning;
            return Theme.Ink;
        }

        private static void PaintCard(object sender, PaintEventArgs e)
        {
            var c = (Control)sender;
            Color accent = CardColor(c.Name);
            using (var b = new SolidBrush(accent))
                e.Graphics.FillRectangle(b, 0, 0, 5, c.Height);
        }

        // ------------------------------------------------------------------
        // TAB: thay tiêu đề tab gốc bằng TabStrip (gạch chân mực tím)
        // ------------------------------------------------------------------
        private static void StyleTabs(TabControl tabs)
        {
            tabs.Font = Theme.Body;
            if (tabs.Parent != null && !(tabs.Tag is TabStrip))
                tabs.Tag = TabStrip.Attach(tabs);
        }
    }
}
