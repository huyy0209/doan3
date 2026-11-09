using System.Globalization;
using System.Linq;

namespace DoAnWD.Common
{
    /// <summary>Đọc / hiển thị số tiền VNĐ dùng chung cho mọi màn hình</summary>
    public static class SoTien
    {
        public static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

        /// <summary>Chấp nhận "1.500.000", "1,500,000", "1500000 đ"... (chỉ lấy chữ số), phải &gt; 0</summary>
        public static bool TryParse(string text, out decimal soTien)
        {
            soTien = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            string so = new string(text.Where(char.IsDigit).ToArray());
            return so.Length > 0 && so.Length <= 12 && decimal.TryParse(so, out soTien) && soTien > 0;
        }

        /// <summary>1500000 =&gt; "1.500.000"</summary>
        public static string Dinh(decimal soTien) => soTien.ToString("N0", Vi);
    }
}
