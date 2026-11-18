using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DoAnWeb.Common;


/// <summary>Đọc / hiển thị số tiền VNĐ dùng chung cho mọi màn hình</summary>
public static class SoTien
{
    public static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
    public const string ThongBaoDinhDang = "Số tiền không hợp lệ. Nhập số nguyên dương, phân nhóm đủ 3 chữ số (ví dụ 1.500.000), có thể thêm đ hoặc VND.";

    /// <summary>Chấp nhận số nguyên dương có dấu phân nhóm hợp lệ và hậu tố tiền tệ tùy chọn.</summary>
    public static bool TryParse(string text, out decimal soTien)
    {
        soTien = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        string so = text.Trim();
        if (so.EndsWith("VND", StringComparison.OrdinalIgnoreCase))
            so = so.Substring(0, so.Length - 3).TrimEnd();
        else if (so.EndsWith("đ", StringComparison.OrdinalIgnoreCase) ||
                 so.EndsWith("d", StringComparison.OrdinalIgnoreCase))
            so = so.Substring(0, so.Length - 1).TrimEnd();

        if (so.Length == 0 || so.Any(c => !char.IsDigit(c) && c != '.' && c != ',' && !char.IsWhiteSpace(c)))
            return false;

        char[] dauPhanNhom = so.Where(c => c == '.' || c == ',').Distinct().ToArray();
        if (dauPhanNhom.Length > 1) return false;

        string[] nhom;
        if (dauPhanNhom.Length == 1)
        {
            nhom = so.Split(dauPhanNhom[0]);
            if (!CacNhomHopLe(nhom)) return false;
        }
        else if (so.Any(char.IsWhiteSpace))
        {
            nhom = Regex.Split(so.Trim(), @"\s+");
            if (!CacNhomHopLe(nhom)) return false;
        }
        else
        {
            nhom = new[] { so };
            if (!LaChuSo(so)) return false;
        }

        string chuSo = string.Concat(nhom);
        return chuSo.Length <= 12 && decimal.TryParse(chuSo, NumberStyles.None, CultureInfo.InvariantCulture, out soTien) && soTien > 0;
    }

    private static bool CacNhomHopLe(string[] nhom) =>
        nhom.Length > 1 && nhom[0].Length >= 1 && nhom[0].Length <= 3 && LaChuSo(nhom[0]) &&
        nhom.Skip(1).All(n => n.Length == 3 && LaChuSo(n));

    private static bool LaChuSo(string nhom) =>
        nhom.Length > 0 && nhom.All(c => c >= '0' && c <= '9');

    /// <summary>1500000 =&gt; "1.500.000"</summary>
    public static string Dinh(decimal soTien) => soTien.ToString("N0", Vi);

    /// <summary>Đọc số tiền nguyên bằng tiếng Việt, dùng trên phiếu thu.</summary>
    public static string DocBangChu(decimal soTien)
    {
        if (soTien < 0 || soTien != decimal.Truncate(soTien) || soTien > 999_999_999_999_999m)
            throw new ArgumentOutOfRangeException(nameof(soTien), "Số tiền phải là số nguyên không âm tối đa 15 chữ số.");
        if (soTien == 0) return "Không đồng";

        string[] hang = { "", "nghìn", "triệu", "tỷ", "nghìn tỷ" };
        long conLai = (long)soTien;
        var nhom = new List<int>();
        while (conLai > 0)
        {
            nhom.Add((int)(conLai % 1000));
            conLai /= 1000;
        }

        var cacPhan = new List<string>();
        for (int i = nhom.Count - 1; i >= 0; i--)
        {
            int giaTri = nhom[i];
            if (giaTri == 0) continue;
            bool docDuSoKhong = i < nhom.Count - 1 && giaTri < 100;
            string phan = DocNhom(giaTri, docDuSoKhong);
            if (!string.IsNullOrEmpty(hang[i])) phan += " " + hang[i];
            cacPhan.Add(phan);
        }
        return string.Join(" ", cacPhan).Trim() + " đồng";
    }

    private static string DocNhom(int so, bool docDuSoKhong)
    {
        string[] chuSo = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
        int tram = so / 100, chuc = so / 10 % 10, donVi = so % 10;
        var tu = new List<string>();
        if (tram > 0) tu.Add(chuSo[tram] + " trăm");
        else if (docDuSoKhong) tu.Add("không trăm");

        if (chuc > 1)
        {
            tu.Add(chuSo[chuc] + " mươi");
            if (donVi == 1) tu.Add("mốt");
            else if (donVi == 4) tu.Add("tư");
            else if (donVi == 5) tu.Add("lăm");
            else if (donVi > 0) tu.Add(chuSo[donVi]);
        }
        else if (chuc == 1)
        {
            tu.Add("mười");
            if (donVi == 5) tu.Add("lăm");
            else if (donVi > 0) tu.Add(chuSo[donVi]);
        }
        else if (donVi > 0)
        {
            if (tram > 0 || docDuSoKhong) tu.Add("lẻ");
            tu.Add(chuSo[donVi]);
        }
        return string.Join(" ", tu);
    }
}