using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Phân trang cho bảng: chỉ đưa vào lưới tối đa PageSize dòng mỗi lần
    /// => bảng vẽ nhanh, cuộn mượt kể cả khi có hàng nghìn bản ghi.
    /// </summary>
    public class Pager : FlowLayoutPanel
    {
        private readonly Label _lbl;
        private readonly ThemedButton _truoc, _sau;
        private int _tong;

        public int PageSize { get; set; } = 50;
        public int Page { get; private set; }
        public event EventHandler PageChanged;

        public Pager()
        {
            AutoSize = true;
            WrapContents = false;
            Margin = new Padding(0);
            FlowDirection = FlowDirection.LeftToRight;

            _truoc = Ui.Button("", ButtonKind.Ghost, Glyph.ChevronLeft, 40);
            _sau = Ui.Button("", ButtonKind.Ghost, Glyph.ChevronRight, 40);
            _truoc.AccessibleName = "Trang trước";
            _sau.AccessibleName = "Trang sau";
            _lbl = new Label { AutoSize = true, Font = Theme.Body, ForeColor = Theme.TextMuted, Margin = new Padding(4, 14, 4, 0) };
            _truoc.Click += (s, e) => Doi(Page - 1);
            _sau.Click += (s, e) => Doi(Page + 1);
            Controls.AddRange(new Control[] { _lbl, _truoc, _sau });
        }

        private int SoTrang => Math.Max(1, (int)Math.Ceiling(_tong / (double)PageSize));

        private void Doi(int trang)
        {
            trang = Math.Max(0, Math.Min(SoTrang - 1, trang));
            if (trang == Page) return;
            Page = trang;
            PageChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Về trang đầu (gọi khi đổi bộ lọc)</summary>
        public void Reset() { Page = 0; }

        /// <summary>Cắt danh sách theo trang hiện tại và cập nhật nhãn "1–50 / 230"</summary>
        public List<T> Cat<T>(IList<T> ds)
        {
            _tong = ds.Count;
            if (Page > SoTrang - 1) Page = SoTrang - 1;
            int tu = Page * PageSize;
            var trang = ds.Skip(tu).Take(PageSize).ToList();
            _lbl.Text = _tong == 0 ? "0 dòng" : $"{tu + 1}–{tu + trang.Count} / {_tong}";
            _truoc.Enabled = Page > 0;
            _sau.Enabled = Page < SoTrang - 1;
            _truoc.Visible = _sau.Visible = SoTrang > 1;
            return trang;
        }
    }
}
