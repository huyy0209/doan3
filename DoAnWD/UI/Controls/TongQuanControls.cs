using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Dto;
using DoAnWD.BLL.Services;
using DoAnWD.Common;

namespace DoAnWD.UI.Controls
{
    /// <summary>Viết gọn số tiền cho ô số và biểu đồ: 12,5 tr; 1,2 tỷ</summary>
    public static class Tien
    {
        private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

        public static string Gon(decimal soTien)
        {
            if (soTien >= 1000000000m) return (soTien / 1000000000m).ToString("0.##", Vi) + " tỷ";
            if (soTien >= 1000000m) return (soTien / 1000000m).ToString("0.#", Vi) + " tr";
            return soTien.ToString("N0", Vi) + " đ";
        }
    }

    /// <summary>
    /// Tấm bảng "Hôm nay": 6 cột ứng với 6 ca học, mỗi dòng là một lớp học hôm nay.
    /// Chấm vàng = đã điểm danh, vòng rỗng = chưa. Bấm vào một ca để mở điểm danh ca đó.
    /// </summary>
    public class BangHomNay : Control
    {
        private const int Le = 18, CaoTieuDe = 50, CaoDauCot = 44, CaoDong = 25;
        private static readonly Font FontTieuDe = Theme.Chu(13F, 2);
        private List<DiemDanhRow> _ds = new List<DiemDanhRow>();
        private int _hover = -1;

        /// <summary>Người dùng bấm vào cột của một ca</summary>
        public event Action<string> ChonCa;

        public BangHomNay()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Dock = DockStyle.Fill;
            Margin = new Padding(0, 0, 14, 0);
        }

        public void Nap(List<DiemDanhRow> ds)
        {
            _ds = ds ?? new List<DiemDanhRow>();
            Invalidate();
        }

        private RectangleF Cot(int i)
        {
            float rong = (Width - 2f * Le) / 6f;
            return new RectangleF(Le + i * rong, CaoTieuDe, rong, Height - CaoTieuDe - 12);
        }

        private int TimCot(Point p)
        {
            for (int i = 0; i < 6; i++) if (Cot(i).Contains(p)) return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int h = TimCot(e.Location);
            if (h != _hover) { _hover = h; Invalidate(); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            int i = TimCot(e.Location);
            if (i >= 0) ChonCa?.Invoke(Constants.CaHoc.TatCaCaHoc[i]);
            base.OnMouseClick(e);
        }

        /// <summary>Hai từ cuối của họ tên: "Nguyễn Văn An" => "Văn An" (cột hẹp)</summary>
        private static string TenNgan(string hoTen)
        {
            var tu = (hoTen ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return tu.Length <= 2 ? string.Join(" ", tu) : tu[tu.Length - 2] + " " + tu[tu.Length - 1];
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Theme.Paper);
            using (var path = Theme.RoundedRect(new RectangleF(0, 0, Width - 1, Height - 1), 14))
            using (var b = new SolidBrush(Theme.Board))
                g.FillPath(b, path);

            // Tiêu đề bảng
            int daDiemDanh = _ds.Count(r => r.DaLuu);
            TextRenderer.DrawText(g, "Hôm nay", FontTieuDe, new Rectangle(Le, 10, 200, 32), Theme.ChalkWhite,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            string tom = _ds.Count == 0 ? "Không có lớp nào" : $"{_ds.Count} lớp, đã điểm danh {daDiemDanh}";
            TextRenderer.DrawText(g, tom, Theme.Body, new Rectangle(Width - Le - 320, 10, 320, 32), Theme.InkMuted,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);

            var cas = Constants.CaHoc.TatCaCaHoc;
            string caHienTai = ShiftRibbon.CaHienTai(DateTime.Now);
            for (int i = 0; i < cas.Length && i < 6; i++)
            {
                var c = Cot(i);
                bool hienTai = cas[i] == caHienTai;
                if (_hover == i || hienTai)
                    using (var path = Theme.RoundedRect(new RectangleF(c.X + 3, c.Y, c.Width - 6, c.Height), 9))
                    using (var b = new SolidBrush(Theme.Blend(Theme.Board, Color.White, _hover == i ? 0.10f : 0.06f)))
                        g.FillPath(b, path);
                if (i > 0)
                    using (var p = new Pen(Theme.BoardLine) { DashStyle = DashStyle.Dash })
                        g.DrawLine(p, c.X, c.Y + 6, c.X, c.Bottom - 6);

                // Đầu cột: tên ca + giờ
                int bd, kt;
                Constants.CaHoc.TryGetGio(cas[i], out bd, out kt);
                var o = Rectangle.Round(new RectangleF(c.X + 12, c.Y + 6, c.Width - 20, 18));
                TextRenderer.DrawText(g, Constants.CaHoc.TenNgan(cas[i]), Theme.BodyStrong, o, hienTai ? Theme.Chalk : Theme.ChalkWhite,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                o.Y += 18;
                TextRenderer.DrawText(g, $"{bd}h–{kt}h", Theme.Small, o, hienTai ? Theme.Chalk : Theme.InkMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                // Các lớp của ca
                var lops = _ds.Where(r => r.CaHoc == cas[i]).OrderBy(r => r.HocVien).ToList();
                float y = c.Y + CaoDauCot + 4;
                int vua = Math.Max(1, (int)((c.Bottom - y) / CaoDong));
                int hien = lops.Count > vua ? vua - 1 : lops.Count;
                if (lops.Count == 0)
                    TextRenderer.DrawText(g, "Trống", Theme.Small, Rectangle.Round(new RectangleF(c.X + 12, y, c.Width - 20, CaoDong)), Theme.Blend(Theme.BoardLine, Theme.InkMuted, 0.35f),
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                for (int k = 0; k < hien; k++, y += CaoDong)
                {
                    var r = lops[k];
                    var cham = new RectangleF(c.X + 13, y + CaoDong / 2f - 3.5f, 7, 7);
                    if (r.DaLuu) using (var b = new SolidBrush(Theme.Chalk)) g.FillEllipse(b, cham);
                    else using (var p = new Pen(Theme.InkMuted, 1.4f)) g.DrawEllipse(p, cham);

                    string ten = TenNgan(r.HocVien) + (r.LaHocBu ? " (bù)" : "");
                    var oTen = Rectangle.Round(new RectangleF(c.X + 27, y, c.Width - 33, CaoDong));
                    TextRenderer.DrawText(g, ten, Theme.Body, oTen, Theme.ChalkWhite,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    // Môn học viết mờ phía sau nếu còn chỗ
                    int rongTen = TextRenderer.MeasureText(g, ten, Theme.Body, oTen.Size, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
                    if (oTen.Width - rongTen > 44)
                        TextRenderer.DrawText(g, r.MonHoc, Theme.Small, new Rectangle(oTen.X + rongTen + 7, oTen.Y + 1, oTen.Width - rongTen - 7, CaoDong), Theme.InkMuted,
                            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
                if (lops.Count > hien)
                    TextRenderer.DrawText(g, $"+{lops.Count - hien} lớp nữa", Theme.Small, Rectangle.Round(new RectangleF(c.X + 27, y, c.Width - 33, CaoDong)), Theme.Chalk,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }
    }

    /// <summary>Ô số liệu ở trang Tổng quan: nhãn, con số lớn, một dòng ghi chú. Bấm để mở trang liên quan.</summary>
    public class TheSo : Control
    {
        private bool _hover;
        private string _giaTri = "", _ghiChu = "";
        private Color _mauGhiChu = Theme.TextMuted;

        public Glyph Glyph { get; set; } = Glyph.None;

        public TheSo(string nhan, Glyph glyph)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Text = nhan;
            Glyph = glyph;
            Height = 104;
            Dock = DockStyle.Fill;
            Margin = new Padding(0, 0, 14, 0);
            Cursor = Cursors.Hand;
        }

        public void Dat(string giaTri, string ghiChu, Color? mauGhiChu = null)
        {
            _giaTri = giaTri ?? "";
            _ghiChu = ghiChu ?? "";
            _mauGhiChu = mauGhiChu ?? Theme.TextMuted;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Theme.Paper);
            using (var path = Theme.RoundedRect(new RectangleF(0.5f, 0.5f, Width - 1, Height - 1), Theme.RadiusCard))
            {
                using (var b = new SolidBrush(Theme.Surface)) g.FillPath(b, path);
                using (var p = new Pen(_hover ? Theme.Ink : Theme.Rule)) g.DrawPath(p, path);
            }

            var oIcon = new RectangleF(Width - 52, 16, 34, 34);
            using (var b = new SolidBrush(Theme.InkSoft)) g.FillEllipse(b, oIcon);
            Icons.Draw(g, Glyph, new RectangleF(oIcon.X + 8, oIcon.Y + 8, 18, 18), Theme.Ink);

            const TextFormatFlags co = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            TextRenderer.DrawText(g, Text, Theme.Label, new Rectangle(18, 13, Width - 78, 20), Theme.TextMuted, co);
            TextRenderer.DrawText(g, _giaTri, Theme.Number, new Rectangle(17, 33, Width - 34, 40), Theme.Text, co);
            TextRenderer.DrawText(g, _ghiChu, Theme.Small, new Rectangle(18, 75, Width - 36, 18), _mauGhiChu, co);
        }
    }

    /// <summary>Biểu đồ cột học phí đã thu theo tháng; tháng hiện tại tô đậm</summary>
    public class BieuDoThu : Control
    {
        private List<ThuThang> _ds = new List<ThuThang>();

        public BieuDoThu()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Dock = DockStyle.Fill;
            BackColor = Theme.Surface;
        }

        public void Nap(List<ThuThang> ds)
        {
            _ds = ds ?? new List<ThuThang>();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);
            if (_ds.Count == 0 || _ds.All(t => t.SoTien == 0))
            {
                TextRenderer.DrawText(g, "Chưa ghi nhận khoản thu nào trong 6 tháng gần đây.", Theme.Body, ClientRectangle, Theme.TextFaint,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }

            const int caoNhan = 24, caoSo = 22;
            float day = Height - caoNhan, cao = day - caoSo - 4;
            float rongO = Width / (float)_ds.Count, rongCot = Math.Min(64f, rongO * 0.56f);
            decimal lonNhat = _ds.Max(t => t.SoTien);
            using (var p = new Pen(Theme.Rule)) g.DrawLine(p, 0, day, Width, day);

            for (int i = 0; i < _ds.Count; i++)
            {
                var t = _ds[i];
                bool hienTai = i == _ds.Count - 1;
                float h = t.SoTien <= 0 ? 0 : Math.Max(4f, (float)(t.SoTien / lonNhat) * cao);
                float x = i * rongO + (rongO - rongCot) / 2f;
                if (h > 0)
                    using (var path = CotBoTren(new RectangleF(x, day - h, rongCot, h), 6))
                    using (var b = new SolidBrush(hienTai ? Theme.Ink : Theme.InkSelect))
                        g.FillPath(b, path);

                var oSo = Rectangle.Round(new RectangleF(i * rongO, day - h - caoSo, rongO, caoSo));
                if (t.SoTien > 0)
                    TextRenderer.DrawText(g, Tien.Gon(t.SoTien), hienTai ? Theme.SmallStrong : Theme.Small, oSo, hienTai ? Theme.Text : Theme.TextMuted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, "Tháng " + t.Thang.Month, hienTai ? Theme.SmallStrong : Theme.Small,
                    Rectangle.Round(new RectangleF(i * rongO, day + 3, rongO, caoNhan - 3)), hienTai ? Theme.Text : Theme.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }

        /// <summary>Hình chữ nhật chỉ bo hai góc trên</summary>
        private static GraphicsPath CotBoTren(RectangleF r, float bo)
        {
            bo = Math.Min(bo, Math.Min(r.Width / 2f, r.Height));
            var path = new GraphicsPath();
            if (bo < 1f) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, bo * 2, bo * 2, 180, 90);
            path.AddArc(r.Right - bo * 2, r.Y, bo * 2, bo * 2, 270, 90);
            path.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
            path.CloseFigure();
            return path;
        }
    }
}
