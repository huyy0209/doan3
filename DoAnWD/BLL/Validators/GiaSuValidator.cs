using System.Linq;
using System.Text.RegularExpressions;
using DoAnWD.Common;

namespace DoAnWD.BLL.Validators
{
    /// <summary>
    /// Validation logic cho Gia Sư
    /// OOP: Single Responsibility - Chỉ làm validate
    /// </summary>
    public static class GiaSuValidator
    {
        /// <summary>
        /// Validate tất cả thông tin gia sư
        /// </summary>
        public static bool Validate(string hoTen, string sdt, string chuyenMon, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!ValidateHoTen(hoTen, out errorMessage))
                return false;

            if (!ValidateSDT(sdt, out errorMessage))
                return false;

            if (!ValidateChuyenMon(chuyenMon, out errorMessage))
                return false;

            return true;
        }

        /// <summary>
        /// Validate họ tên
        /// </summary>
        public static bool ValidateHoTen(string hoTen, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(hoTen))
            {
                errorMessage = Constants.Messages.VuiLongNhapHoTen;
                return false;
            }

            string ten = hoTen.Trim();
            if (ten.Length < Constants.Validation.MinNameLength ||
                ten.Length > Constants.Validation.MaxNameLength ||
                ten.Any(char.IsDigit))
            {
                errorMessage = Constants.Messages.HoTenKhongHopLe;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate số điện thoại
        /// </summary>
        public static bool ValidateSDT(string sdt, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(sdt))
            {
                errorMessage = Constants.Messages.VuiLongNhapSDT;
                return false;
            }

            if (!Regex.IsMatch(sdt.Trim(), Constants.Validation.PhoneRegex))
            {
                errorMessage = Constants.Messages.SDTKhongHopLe;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate chuyên môn
        /// </summary>
        public static bool ValidateChuyenMon(string chuyenMon, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(chuyenMon))
            {
                errorMessage = Constants.Messages.VuiLongNhapChuyenMon;
                return false;
            }

            if (chuyenMon.Trim().Length > Constants.Validation.MaxChuyenMonLength)
            {
                errorMessage = $"Chuyên môn tối đa {Constants.Validation.MaxChuyenMonLength} ký tự.";
                return false;
            }

            return true;
        }

        /// <summary>Kiểm tra hồ sơ gia sư đầy đủ. Trả về thông báo lỗi hoặc null.</summary>
        public static string KiemTraHoSo(HoSoGiaSu hs)
        {
            string msg;
            if (!Validate(hs.HoTen, hs.SDT, hs.ChuyenMon, out msg)) return msg;

            if (!string.IsNullOrWhiteSpace(hs.Email))
            {
                string e = hs.Email.Trim();
                if (e.Length > 100 || !Regex.IsMatch(e, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return "Email không hợp lệ, ví dụ ten@gmail.com.";
            }
            if (hs.KinhNghiem.HasValue && (hs.KinhNghiem < 0 || hs.KinhNghiem > 60))
                return "Số năm kinh nghiệm từ 0 đến 60.";
            if (!string.IsNullOrEmpty(hs.HocVan) && hs.HocVan.Trim().Length > 150)
                return "Học vấn / bằng cấp tối đa 150 ký tự.";
            if (!string.IsNullOrEmpty(hs.GhiChu) && hs.GhiChu.Trim().Length > 500)
                return "Ghi chú tối đa 500 ký tự.";
            if (hs.CaNhanDay != null)
            {
                var hopLe = Constants.CaHoc.TatCaCaHoc.Select(Constants.CaHoc.TenNgan).ToList();
                if (hs.CaNhanDay.Any(c => !hopLe.Contains(c))) return "Ca nhận dạy không hợp lệ.";
            }
            return null;
        }
    }

    /// <summary>Dữ liệu nhập của form gia sư</summary>
    public class HoSoGiaSu
    {
        public string HoTen { get; set; }
        public string SDT { get; set; }
        public string ChuyenMon { get; set; }
        public string Email { get; set; }
        public string HocVan { get; set; }
        public int? KinhNghiem { get; set; }
        /// <summary>Tên ngắn các ca nhận dạy; rỗng/null = mọi ca</summary>
        public System.Collections.Generic.List<string> CaNhanDay { get; set; }
        public string GhiChu { get; set; }
        public bool TamNghi { get; set; }
    }
}
