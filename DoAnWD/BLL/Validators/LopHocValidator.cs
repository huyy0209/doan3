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

            if (!hocPhi.HasValue || hocPhi.Value <= 0)
            {
                errorMessage = Constants.Messages.HocPhiKhongHopLe;
                return false;
            }

            return true;
        }
    }
}