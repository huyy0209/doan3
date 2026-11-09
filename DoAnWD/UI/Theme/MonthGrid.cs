using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD.UI
{
    /// <summary>
    /// Lịch tháng tự vẽ (một control duy nhất thay cho ~40 Panel + Label):
    /// co giãn theo cửa sổ, không nháy khi đổi tháng.
    /// Mỗi ô ngày liệt kê các lớp học diễn ra hôm đó, chấm màu theo ca.
    /// </summary>
    public class MonthGrid : Control
    {
        private static readonly string[] DayNames = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        private const int HeadHeight = 32;

        private DateTime _month = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        private Dictionary<DateTime, List<LopHoc>> _data = new Dictionary<DateTime, List<LopHoc>>();
        private DateTime? _hoverDate;

        /// <summary>Người dùng bấm vào một ngày</summary>
        public event Action<DateTime> DayClicked;

        public MonthGrid()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            BackColor = Theme.Surface;
            Cursor = Cursors.Hand;
        }

        public DateTime Month
        {
            get { return _month; }
            set { _month = new DateTime(value.Year, value.Month, 1); Invalidate(); }
        }

        public void SetData(Dictionary<DateTime, List<LopHoc>> data)
        {
            _data = data ?? new Dictionary<DateTime, List<LopHoc>>();
            Invalidate();
        }

        // Thứ Hai = cột 0
        private int FirstColumn => ((int)_month.DayOfWeek + 6) % 7;
        private int RowCount => (int)Math.Ceiling((FirstColumn + DateTime.DaysInMonth(_month.Year, _month.Month)) / 7.0);

        private RectangleF CellRect(int col, int row)
        {
            float w = (Width - 1) / 7f;
            float h = (Height - HeadHeight - 1) / (float)RowCount;
            return new RectangleF(col * w, HeadHeight + row * h, w, h);
        }

        private DateTime? DateAt(Point p)
        {
            if (p.Y < HeadHeight) return null;
            float w = (Width - 1) / 7f;
            float h = (Height - HeadHeight - 1) / (float)RowCount;
            int col = (int)(p.X / w), row = (int)((p.Y - HeadHeight) / h);
            int day = row * 7 + col - FirstColumn + 1;
            if (col < 0 || col > 6 || day < 1 || day > DateTime.DaysInMonth(_month.Year, _month.Month))
                return null;
            return new DateTime(_month.Year, _month.Month, day);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var d = DateAt(e.Location);
            if (d != _hoverDate) { _hoverDate = d; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverDate = null;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            Focus();
            var d = DateAt(e.Location);
            if (d.HasValue) DayClicked?.Invoke(d.Value);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float w = (Width - 1) / 7f;

            // Hàng tên thứ - Chủ nhật màu lề đỏ
            for (int i = 0; i < 7; i++)
            {
                var r = Rectangle.Round(new RectangleF(i * w + 10, 0, w - 20, HeadHeight));
                TextRenderer.DrawText(g, DayNames[i], Theme.Label, r, i == 6 ? Theme.Margin : Theme.TextMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }

            int days = DateTime.DaysInMonth(_month.Year, _month.Month);
            int rows = RowCount;

            using (var rulePen = new Pen(Theme.Rule))
            {
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < 7; col++)
                    {
                        int day = row * 7 + col - FirstColumn + 1;
                        var cell = CellRect(col, row);

                        if (day < 1 || day > days)
                        {
                            using (var b = new SolidBrush(Theme.PaperAlt)) g.FillRectangle(b, cell);
                            g.DrawRectangle(rulePen, cell.X, cell.Y, cell.Width, cell.Height);
                            continue;
                        }

                        var date = new DateTime(_month.Year, _month.Month, day);
                        DrawDay(g, cell, date, rulePen);
                    }
                }
            }
        }

        private void DrawDay(Graphics g, RectangleF cell, DateTime date, Pen rulePen)
        {
            bool today = date == DateTime.Now.Date;
            bool hover = _hoverDate == date;

            if (today)
                using (var b = new SolidBrush(Theme.InkSoft)) g.FillRectangle(b, cell);
            g.DrawRectangle(rulePen, cell.X, cell.Y, cell.Width, cell.Height);

            // Số ngày
            var numRect = Rectangle.Round(new RectangleF(cell.X + 8, cell.Y + 6, 40, 20));
            Color numColor = today ? Theme.Ink : date.DayOfWeek == DayOfWeek.Sunday ? Theme.Margin : Theme.Text;
            TextRenderer.DrawText(g, date.Day.ToString(), today ? Theme.BodyStrong : Theme.Body, numRect, numColor,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding);

            // Danh sách lớp theo thứ tự ca
            List<LopHoc> classes;
            if (_data.TryGetValue(date, out classes) && classes.Count > 0)
            {
                var sorted = classes
                    .OrderBy(l => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, l.CaHoc))
                    .ToList();

                const float chipH = 18f;
                float top = cell.Y + 28;
                int fit = Math.Max(0, (int)((cell.Bottom - top - 4) / chipH));
                int show = sorted.Count > fit ? Math.Max(0, fit - 1) : sorted.Count;

                for (int i = 0; i < show; i++)
                {
                    var lop = sorted[i];
                    Color c = Theme.ShiftColor(lop.CaHoc);
                    float y = top + i * chipH;

                    using (var b = new SolidBrush(c))
                        g.FillEllipse(b, cell.X + 9, y + 6, 7, 7);

                    var textRect = Rectangle.Round(new RectangleF(cell.X + 20, y, cell.Width - 26, chipH));
                    string text = $"{Theme.ShiftShortName(lop.CaHoc)}: {lop.MonHoc}";
                    TextRenderer.DrawText(g, text, Theme.Small, textRect, Theme.Text,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                }

                if (show < sorted.Count)
                {
                    var moreRect = Rectangle.Round(new RectangleF(cell.X + 20, top + show * chipH, cell.Width - 26, chipH));
                    TextRenderer.DrawText(g, $"+{sorted.Count - show} lớp", Theme.SmallStrong, moreRect, Theme.Ink,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                }
            }

            if (hover)
                using (var p = new Pen(Theme.Ink, 1.5f))
                    g.DrawRectangle(p, cell.X + 1, cell.Y + 1, cell.Width - 2, cell.Height - 2);
        }
    }
}
