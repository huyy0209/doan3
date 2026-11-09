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
    }
}