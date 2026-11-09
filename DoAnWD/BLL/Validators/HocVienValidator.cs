using System.Linq;
using System.Text.RegularExpressions;
using DoAnWD.Common;

namespace DoAnWD.BLL.Validators
{
    /// <summary>
    /// Validation logic cho Học Viên (KHÔNG CÓ EMAIL)
    /// </summary>
    public static class HocVienValidator
    {
        /// <summary>
        /// Validate tất cả thông tin học viên
        /// </summary>
        public static bool Validate(string hoTen, string sdt, string diaChi, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!ValidateHoTen(hoTen, out errorMessage))
                return false;

            if (!ValidateSDT(sdt, out errorMessage))
                return false;

            if (!string.IsNullOrWhiteSpace(diaChi) && diaChi.Trim().Length > Constants.Validation.MaxDiaChiLength)
            {
                errorMessage = $"Địa chỉ tối đa {Constants.Validation.MaxDiaChiLength} ký tự.";
                return false;
            }

            return true;
        }

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
        /// Kiểm tra hồ sơ đầy đủ: cần ít nhất một số điện thoại (học viên nhỏ tuổi thường chỉ có SĐT phụ huynh)
        /// </summary>
        public static string KiemTraHoSo(HoSoHocVien hs)
        {
            string msg;
            if (!ValidateHoTen(hs.HoTen, out msg)) return msg;

            if (string.IsNullOrWhiteSpace(hs.SDT) && string.IsNullOrWhiteSpace(hs.SDTPhuHuynh))
                return "Nhập ít nhất một số điện thoại: của học viên hoặc của phụ huynh.";
            if (!string.IsNullOrWhiteSpace(hs.SDT) && !ValidateSDT(hs.SDT, out msg)) return msg;
            if (!string.IsNullOrWhiteSpace(hs.SDTPhuHuynh) && !ValidateSDT(hs.SDTPhuHuynh, out msg))
                return "SĐT phụ huynh không hợp lệ (10-11 số, bắt đầu bằng 0).";

            if (Dai(hs.DiaChi, Constants.Validation.MaxDiaChiLength)) return "Địa chỉ tối đa 200 ký tự.";
            if (Dai(hs.TenPhuHuynh, 100)) return "Tên phụ huynh tối đa 100 ký tự.";
            if (Dai(hs.TruongLop, 150)) return "Trường / lớp tối đa 150 ký tự.";
            if (Dai(hs.TrinhDo, 50)) return "Trình độ tối đa 50 ký tự.";
            if (Dai(hs.GhiChu, 500)) return "Ghi chú tối đa 500 ký tự.";
            return null;
        }

        private static bool Dai(string s, int max) => !string.IsNullOrEmpty(s) && s.Trim().Length > max;
    }

    /// <summary>Dữ liệu nhập của form học viên</summary>
    public class HoSoHocVien
    {
        public string HoTen { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public string TenPhuHuynh { get; set; }
        public string SDTPhuHuynh { get; set; }
        public string TruongLop { get; set; }
        public string TrinhDo { get; set; }
        public string GhiChu { get; set; }
    }
}
