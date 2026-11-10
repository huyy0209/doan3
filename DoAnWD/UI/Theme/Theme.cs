using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DoAnWD.UI
{
    /// <summary>
    /// Bộ token giao diện "Bảng xanh": bảng xanh lớp học, phấn vàng, bút đỏ chấm bài.
    ///  - Xanh bảng (Board) cho khung chính; xanh lá (Ink) cho hành động chính;
    ///  - Vàng phấn (Chalk) đánh dấu mục đang mở, điểm nhấn trên nền tối;
    ///  - Đỏ bút chấm (Margin) cho lỗi, vắng, cảnh báo mạnh.
    /// Mọi màu / font trong ứng dụng lấy từ đây để giao diện thống nhất.
    /// </summary>
    public static class Theme
    {
        // ------------------------------------------------------------------
        // MÀU NỀN TẢNG
        // ------------------------------------------------------------------
        public static readonly Color Paper = Hex("#F2F5F3");      // nền trang
        public static readonly Color Surface = Color.White;       // thẻ / vùng nội dung
        public static readonly Color PaperAlt = Hex("#F8FAF9");   // dòng xen kẽ
        public static readonly Color Rule = Hex("#DFE6E2");       // đường kẻ mảnh
        public static readonly Color RuleStrong = Hex("#BCC9C3"); // viền ô nhập

        public static readonly Color Ink = Hex("#1B7A5E");        // xanh lá - màu hành động chính
        public static readonly Color InkHover = Hex("#14624B");
        public static readonly Color InkDark = Hex("#1D4A40");    // xanh bảng - sidebar
        public static readonly Color InkDarker = Hex("#163A32");
        public static readonly Color InkSoft = Hex("#E3F1EC");    // nền nhạt của màu chính
        public static readonly Color InkSelect = Hex("#CFE7DD");
        public static readonly Color RowSelect = Hex("#D8E8FB");  // dòng đang chọn: xanh dương nhạt
        public static readonly Color RowSelectBar = Hex("#2B6FD6");
        public static readonly Color InkFocus = Hex("#F1F8F5");   // ô nhập đang gõ
        public static readonly Color InkMuted = Hex("#A9C4BB");   // chữ phụ trên nền xanh bảng

        public static readonly Color Board = InkDark;             // xanh bảng
        public static readonly Color BoardLine = Hex("#2C5F53");  // nét kẻ trên bảng
        public static readonly Color Chalk = Hex("#F3C44F");      // phấn vàng
        public static readonly Color ChalkSoft = Hex("#FCF3D9");
        public static readonly Color ChalkWhite = Hex("#F4F8F6"); // phấn trắng

        public static readonly Color Margin = Hex("#C4403C");     // bút đỏ
        public static readonly Color MarginSoft = Hex("#FBE9E8");
        public static readonly Color Info = Hex("#2B6FD6");       // đang diễn ra
        public static readonly Color InfoSoft = Hex("#E4EEFB");

        public static readonly Color Text = Hex("#1B2A26");
        public static readonly Color TextMuted = Hex("#5C6C66");
        public static readonly Color TextFaint = Hex("#93A19B");

        public static readonly Color Success = Hex("#23825B");
        public static readonly Color SuccessSoft = Hex("#E2F3EA");
        public static readonly Color Warning = Hex("#B4730F");
        public static readonly Color WarningSoft = Hex("#FBF1DC");
        public static readonly Color Neutral = Hex("#7F8E88");
        public static readonly Color NeutralSoft = Hex("#ECF0EE");

        // ------------------------------------------------------------------
        // TYPOGRAPHY - Be Vietnam Pro (font vẽ riêng cho dấu tiếng Việt, đi kèm trong thư mục Fonts);
        // không nạp được thì dùng Segoe UI. Phân cấp bằng cỡ + độ đậm.
        // ------------------------------------------------------------------
        private static readonly PrivateFontCollection _boFont = new PrivateFontCollection();
        private static FontFamily _thuong, _vua, _dam;
        /// <summary>Đã nạp được font Be Vietnam Pro chưa (false = đang dùng Segoe UI)</summary>
        public static readonly bool CoFontRieng = NapFont();

        public const string FontDuPhong = "Segoe UI";
        public const string FontDuPhongDam = "Segoe UI Semibold";
        /// <summary>Tên họ chữ thường (để tạo font cỡ khác, kiểu nghiêng)</summary>
        public static string Family => _thuong != null ? _thuong.Name : FontDuPhong;

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

        private static bool NapFont()
        {
            try
            {
                string thuMuc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts");
                var tep = new[] { "BeVietnamPro-Regular.ttf", "BeVietnamPro-Medium.ttf", "BeVietnamPro-SemiBold.ttf" }
                    .Select(t => Path.Combine(thuMuc, t)).ToArray();
                if (!tep.All(File.Exists)) return false;

                bool laWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
                if (!laWindows)
                {
                    // Mono / Linux: font nạp riêng không dùng được, chỉ nhận font đã cài trong máy
                    _thuong = HoDaCai("Be Vietnam Pro");
                    _vua = HoDaCai("Be Vietnam Pro Medium");
                    _dam = HoDaCai("Be Vietnam Pro SemiBold");
                    return _thuong != null;
                }
                foreach (var t in tep)
                {
                    // GDI (ô nhập, TextRenderer) cần đăng ký riêng; FR_PRIVATE = chỉ ứng dụng này thấy font
                    if (AddFontResourceEx(t, 0x10, IntPtr.Zero) == 0) return false;
                    _boFont.AddFontFile(t);   // GDI+ (vẽ tự do)
                }
                var ho = _boFont.Families;
                _thuong = ho.FirstOrDefault(f => f.Name == "Be Vietnam Pro");
                _vua = ho.FirstOrDefault(f => f.Name.EndsWith("Medium"));
                _dam = ho.FirstOrDefault(f => f.Name.EndsWith("SemiBold"));
                return _thuong != null;
            }
            catch
            {
                _thuong = _vua = _dam = null;
                return false;
            }
        }

        private static FontFamily HoDaCai(string ten)
        {
            try
            {
                var ho = new FontFamily(ten);
                return ho.Name == ten ? ho : null;
            }
            catch { return null; }
        }

        /// <summary>Font theo độ đậm: 0 = thường, 1 = vừa (Medium), 2 = đậm (SemiBold)</summary>
        public static Font Chu(float co, int dam = 0, FontStyle kieu = FontStyle.Regular)
        {
            try
            {
                if (_thuong != null)
                {
                    var ho = dam == 2 ? _dam : dam == 1 ? _vua : _thuong;
                    if (ho != null) return new Font(ho, co, kieu);
                    // Thiếu file độ đậm riêng: dùng nét đậm của họ chữ thường
                    return new Font(_thuong, co, kieu | FontStyle.Bold);
                }
            }
            catch { /* rơi xuống font dự phòng */ }
            return new Font(dam == 0 ? FontDuPhong : FontDuPhongDam, co + 0.5f, kieu);
        }

        public static readonly Font Body = Chu(10F);
        public static readonly Font BodyStrong = Chu(10F, 2);
        /// <summary>Chữ trong ô bảng: nhỏ hơn chữ thường một chút để cột hẹp vẫn đủ chỗ</summary>
        public static readonly Font Cell = Chu(9.5F);
        public static readonly Font CellStrong = Chu(9.5F, 2);
        public static readonly Font Small = Chu(8.75F);
        public static readonly Font SmallStrong = Chu(8.75F, 2);
        public static readonly Font Label = Chu(9.25F, 1);
        public static readonly Font Section = Chu(11.5F, 2);
        public static readonly Font PageTitle = Chu(19F, 2);
        public static readonly Font Display = Chu(26F, 2);
        public static readonly Font Number = Chu(24F, 2);
        public static readonly Font NumberSmall = Chu(17F, 2);
        public static readonly Font Button = Chu(10F, 1);
        public static readonly Font Nav = Chu(10.25F);
        public static readonly Font NavActive = Chu(10.25F, 2);

        // ------------------------------------------------------------------
        // KÍCH THƯỚC
        // ------------------------------------------------------------------
        public const int Radius = 8;
        public const int RadiusCard = 12;
        public const int RowHeight = 40;
        public const int HeaderHeight = 42;

        // ------------------------------------------------------------------
        // MÀU THEO CA HỌC: từ nắng sáng đến đêm
        // ------------------------------------------------------------------
        public static Color ShiftColor(string caHoc)
        {
            if (string.IsNullOrEmpty(caHoc)) return Neutral;

            if (caHoc.StartsWith("Sáng muộn")) return Hex("#D9822B");
            if (caHoc.StartsWith("Sáng")) return Hex("#D8A31A");
            if (caHoc.StartsWith("Chiều muộn")) return Hex("#A8507A");
            if (caHoc.StartsWith("Chiều")) return Hex("#C9573E");
            if (caHoc.StartsWith("Tối muộn")) return Hex("#2F4A8A");
            if (caHoc.StartsWith("Tối")) return Hex("#4E6FC0");
            return Neutral;
        }

        // ------------------------------------------------------------------
        // MÀU THEO TRẠNG THÁI
        // ------------------------------------------------------------------
        public static Color StatusColor(string trangThai)
        {
            switch (trangThai)
            {
                case "Rảnh":
                case "Sẵn sàng":
                case "Có mặt":
                case "Giỏi":
                case "Đã đủ":
                case "Hoạt động":
                    return Success;
                case "Bận":
                case "Chưa bắt đầu":
                case "Đi trễ":
                case "Trung bình":
                case "Còn nợ":
                    return Warning;
                case "Khá":
                    return Info;
                case "Vắng không phép":
                case "Yếu":
                case "Chưa đóng":
                case "Đã khóa":
                    return Margin;
                case "Đang dạy":
                case "Đang diễn ra":
                case "Đang học":
                    return Info;
                case "Đã hủy":
                    return Margin;
                default:
                    return Neutral; // Tạm nghỉ, Đã kết thúc...
            }
        }

        public static Color StatusSoftColor(string trangThai)
        {
            return Blend(StatusColor(trangThai), Color.White, 0.86f);
        }

        // ------------------------------------------------------------------
        // TIỆN ÍCH
        // ------------------------------------------------------------------
        public static Color Hex(string hex)
        {
            hex = hex.TrimStart('#');
            return Color.FromArgb(
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16));
        }

        /// <summary>Trộn màu a với b, amount = tỉ lệ của b (0..1)</summary>
        public static Color Blend(Color a, Color b, float amount)
        {
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * amount),
                (int)(a.G + (b.G - a.G) * amount),
                (int)(a.B + (b.B - a.B) * amount));
        }

        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2;
            if (radius <= 0)
            {
                path.AddRectangle(r);
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Người dùng có tắt hiệu ứng chuyển động của Windows không</summary>
        public static bool MotionEnabled
        {
            get
            {
                try { return System.Windows.Forms.SystemInformation.UIEffectsEnabled; }
                catch { return true; }
            }
        }
    }
}
