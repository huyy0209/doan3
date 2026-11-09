using System;
using System.Linq;
using DoAnWD.Common;

namespace DoAnWD.BLL.Validators
{
    /// <summary>
    /// Validation logic cho Lớp Học
    /// </summary>
    public static class LopHocValidator
    {
        /// <summary>
        /// Validate thông tin ghép lớp
        /// </summary>
        public static bool Validate(int? maHV, int? maGS, string monHoc, decimal? hocPhi, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!maHV.HasValue || maHV.Value <= 0)
            {
                errorMessage = Constants.Messages.VuiLongChonHocVien;
                return false;
            }

            if (!maGS.HasValue || maGS.Value <= 0)
            {
                errorMessage = Constants.Messages.VuiLongChonGiaSu;
                return false;
            }

            if (string.IsNullOrWhiteSpace(monHoc))
            {
                errorMessage = "Vui lòng nhập môn học.";
                return false;
            }

            if (monHoc.Trim().Length > Constants.Validation.MaxMonHocLength)
            {
                errorMessage = $"Môn học tối đa {Constants.Validation.MaxMonHocLength} ký tự.";
                return false;
            }

            if (!hocPhi.HasValue || hocPhi.Value <= 0)
            {
                errorMessage = Constants.Messages.HocPhiKhongHopLe;
                return false;
            }

            if (hocPhi.Value > Constants.Validation.MaxHocPhi)
            {
                errorMessage = $"Học phí không được vượt quá {Constants.Validation.MaxHocPhi:N0} đ.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate lịch học: ca hợp lệ, bắt đầu trước kết thúc, không bắt đầu trong quá khứ
        /// </summary>
        public static bool ValidateLich(DateTime batDau, DateTime ketThuc, string caHoc, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(caHoc))
            {
                errorMessage = Constants.Messages.VuiLongChonCaHoc;
                return false;
            }

            if (!Constants.CaHoc.TatCaCaHoc.Contains(caHoc))
            {
                errorMessage = Constants.Messages.CaHocKhongHopLe;
                return false;
            }

            if (batDau >= ketThuc)
            {
                errorMessage = Constants.Messages.ThoiGianKhongHopLe;
                return false;
            }

            if (batDau < DateTime.Now.Date)
            {
                errorMessage = Constants.Messages.ThoiGianQuaKhu;
                return false;
            }

            return true;
        }
    }
}