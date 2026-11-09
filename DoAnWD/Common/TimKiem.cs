using System.Globalization;
using System.Linq;
using System.Text;

namespace DoAnWD.Common
{
    /// <summary>
    /// Tìm kiếm không phân biệt hoa thường và dấu tiếng Việt:
    /// gõ "ngoc 0981" vẫn khớp "Đỗ Bảo Ngọc - 0981111111".
    /// Mọi từ khóa (cách nhau bởi dấu cách) đều phải xuất hiện.
    /// </summary>
    public static class TimKiem
    {
        public static string ChuanHoa(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var norm = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(norm.Length);
            foreach (var ch in norm)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(ch == 'đ' ? 'd' : ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static bool Khop(string tuKhoa, params string[] truong)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa)) return true;
            string noiDung = ChuanHoa(string.Join(" ", truong.Where(t => !string.IsNullOrEmpty(t))));
            return ChuanHoa(tuKhoa)
                .Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
                .All(noiDung.Contains);
        }
    }
}
