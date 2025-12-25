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

            // Không validate Email nữa

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
    }
}