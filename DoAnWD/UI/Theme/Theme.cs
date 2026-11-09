using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DoAnWD.UI
{
    /// <summary>
    /// Bộ token giao diện "Vở ô li": giấy trắng xanh, mực tím, lề đỏ.
    /// Mọi màu / font trong ứng dụng lấy từ đây để giao diện thống nhất.
    /// </summary>
    public static class Theme
    {
        // ------------------------------------------------------------------
        // MÀU NỀN TẢNG
        // ------------------------------------------------------------------
        public static readonly Color Paper = Hex("#F6F7FB");      // nền giấy
        public static readonly Color Surface = Color.White;       // vùng nội dung
        public static readonly Color PaperAlt = Hex("#FAFAFD");   // dòng xen kẽ
        public static readonly Color Rule = Hex("#DCDDEB");       // dòng kẻ ô li
        public static readonly Color RuleStrong = Hex("#C4C6DA");

        public static readonly Color Ink = Hex("#4B3F9E");        // mực tím - màu chính
        public static readonly Color InkHover = Hex("#3E3388");
        public static readonly Color InkDark = Hex("#2A2360");    // sidebar
        public static readonly Color InkDarker = Hex("#211B4D");
        public static readonly Color InkSoft = Hex("#ECEAF7");    // nền đang chọn
        public static readonly Color InkMuted = Hex("#B9B4E0");   // chữ phụ trên nền tím

        public static readonly Color Margin = Hex("#C8414B");     // lề đỏ
        public static readonly Color MarginSoft = Hex("#FBE9EA");

        public static readonly Color Text = Hex("#23233A");
        public static readonly Color TextMuted = Hex("#6B6F86");
        public static readonly Color TextFaint = Hex("#9A9DB2");

        public static readonly Color Success = Hex("#2E7D5B");
        public static readonly Color SuccessSoft = Hex("#E3F2EA");
        public static readonly Color Warning = Hex("#B86E14");
        public static readonly Color WarningSoft = Hex("#FBF0DC");
        public static readonly Color Neutral = Hex("#8A8DA3");
        public static readonly Color NeutralSoft = Hex("#EEF0F6");

        // ------------------------------------------------------------------
        // TYPOGRAPHY - một họ chữ Segoe UI, phân cấp bằng cỡ + độ đậm
        // ------------------------------------------------------------------
        public const string Family = "Segoe UI";
        public const string FamilySemibold = "Segoe UI Semibold";
        public const string FamilyLight = "Segoe UI Light";

        public static readonly Font Body = new Font(Family, 9.75F);
        public static readonly Font BodyStrong = new Font(FamilySemibold, 9.75F);
        public static readonly Font Small = new Font(Family, 8.5F);
        public static readonly Font SmallStrong = new Font(FamilySemibold, 8.5F);
        public static readonly Font Label = new Font(FamilySemibold, 9F);
        public static readonly Font Section = new Font(FamilySemibold, 11F);
        public static readonly Font PageTitle = new Font(FamilySemibold, 17F);
        public static readonly Font Number = new Font(FamilyLight, 28F);
        public static readonly Font NumberSmall = new Font(FamilyLight, 20F);
        public static readonly Font Button = new Font(FamilySemibold, 9.75F);
        public static readonly Font Nav = new Font(Family, 10.5F);
        public static readonly Font NavActive = new Font(FamilySemibold, 10.5F);

        // ------------------------------------------------------------------
        // KÍCH THƯỚC
        // ------------------------------------------------------------------
        public const int Radius = 6;
        public const int RowHeight = 36;
        public const int HeaderHeight = 40;

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
            if (caHoc.StartsWith("Tối muộn")) return Hex("#2F3A73");
            if (caHoc.StartsWith("Tối")) return Hex("#5B4FA8");
            return Neutral;
        }

        /// <summary>"Tối (18h-20h)" =&gt; "Tối"</summary>
        public static string ShiftShortName(string caHoc)
        {
            if (string.IsNullOrEmpty(caHoc)) return "";
            int i = caHoc.IndexOf(" (", StringComparison.Ordinal);
            return i > 0 ? caHoc.Substring(0, i) : caHoc;
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
                    return Success;
                case "Bận":
                case "Chưa bắt đầu":
                    return Warning;
                case "Đang dạy":
                case "Đang diễn ra":
                case "Đang học":
                    return Ink;
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
