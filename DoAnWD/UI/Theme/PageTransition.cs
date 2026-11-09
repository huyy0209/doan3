using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Hiệu ứng chuyển trang ~220ms: trang cũ mờ dần, trang mới hiện dần và trượt nhẹ
    /// từ phải sang 24px. Vẽ trên một lớp phủ bằng ảnh chụp của 2 trang, xong thì gỡ lớp phủ
    /// để người dùng thao tác với trang thật.
    /// </summary>
    public class PageTransition : Control
    {
        private const int Duration = 220;   // ms
        private const int Offset = 24;      // px trượt

        private readonly Bitmap _oldPage;
        private Bitmap _newPage;
        private readonly Timer _timer = new Timer { Interval = 15 };
        private DateTime _start;
        private float _t;

        private PageTransition(Bitmap oldPage)
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            _oldPage = oldPage;
            Dock = DockStyle.Fill;
            _timer.Tick += Timer_Tick;
        }

        /// <summary>Chụp ảnh một control (null nếu không chụp được)</summary>
        public static Bitmap Snapshot(Control c)
        {
            try
            {
                if (c == null || c.Width <= 0 || c.Height <= 0) return null;
                var bmp = new Bitmap(c.Width, c.Height, PixelFormat.Format32bppPArgb);
                c.DrawToBitmap(bmp, new Rectangle(Point.Empty, c.Size));
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Phủ ảnh trang cũ lên host ngay lập tức (che bước dựng trang mới),
        /// sau đó chụp trang mới khi nó đã nạp xong dữ liệu và chạy hiệu ứng.
        /// </summary>
        public static void Run(Control host, Bitmap oldPage, Control newPage)
        {
            if (oldPage == null || !Theme.MotionEnabled)
            {
                oldPage?.Dispose();
                return;
            }

            var overlay = new PageTransition(oldPage);
            host.Controls.Add(overlay);
            overlay.BringToFront();

            // Chờ các việc đã xếp hàng (nạp dữ liệu, chỉnh cột bảng) chạy xong rồi mới chụp
            host.BeginInvoke((MethodInvoker)(() =>
            {
                if (overlay.IsDisposed) return;
                overlay._newPage = Snapshot(newPage);
                if (overlay._newPage == null) { overlay.Finish(); return; }
                overlay._start = DateTime.Now;
                overlay._timer.Start();
            }));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _t = Math.Min(1f, (float)(DateTime.Now - _start).TotalMilliseconds / Duration);
            Invalidate();
            if (_t >= 1f) Finish();
        }

        private void Finish()
        {
            _timer.Stop();
            Parent?.Controls.Remove(this);
            Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Theme.Paper);

            float ease = 1f - (float)Math.Pow(1f - _t, 3); // ease-out cubic

            if (_oldPage != null && ease < 1f)
                DrawWithAlpha(g, _oldPage, 0, 1f - ease);

            if (_newPage != null)
                DrawWithAlpha(g, _newPage, (int)Math.Round(Offset * (1f - ease)), ease);
        }

        private static void DrawWithAlpha(Graphics g, Image img, int x, float alpha)
        {
            if (alpha <= 0.01f) return;
            var cm = new ColorMatrix { Matrix33 = alpha };
            using (var ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm);
                g.DrawImage(img, new Rectangle(x, 0, img.Width, img.Height),
                    0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                _oldPage?.Dispose();
                _newPage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
