using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace DoAnWeb.Common
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
}
