using System.Drawing;

namespace DoAnWD.Common
{
    /// <summary>
    /// Lớp chứa các hằng số dùng chung trong ứng dụng
    /// OOP: Encapsulation - Tập trung quản lý các giá trị constant
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// ✨ Trạng thái Gia Sư - CẬP NHẬT CHO OPTION 2
        /// </summary>
        public static class GiaSuStatus
        {
            public const string Ranh = "Rảnh";
            public const string Ban = "Bận";
            public const string DangDay = "Đang dạy";
            public const string SanSang = "Sẵn sàng";
            public const string TamNghi = "Tạm nghỉ"; // Không nhận lớp mới

            /// <summary>Các trạng thái hiển thị trên form Gia Sư</summary>
            public static readonly string[] TatCa = { Ranh, Ban, DangDay, TamNghi };
        }

        /// <summary>
        /// ✨ Trạng thái Lớp Học - CẬP NHẬT CHO OPTION 2
        /// </summary>
        public static class LopHocStatus
        {
            // Trạng thái mới cho Option 2
            public const string ChuaBatDau = "Chưa bắt đầu";
            public const string DangDienRa = "Đang diễn ra";
            public const string DaKetThuc = "Đã kết thúc";

            // Trạng thái cũ - Backward compatible
            public const string DangHoc = "Đang học";
            public const string HoanThanh = "Hoàn thành";
            public const string SanSang = "Sẵn sàng";

            // Lớp bị hủy giữa chừng - giữ lại để còn lịch sử/thống kê
            public const string DaHuy = "Đã hủy";

            /// <summary>
            /// Các trạng thái được coi là lớp "đang hoạt động" (chiếm lịch gia sư/học viên).
            /// Dùng mảng để EF dịch được sang câu lệnh SQL IN (...).
            /// </summary>
            public static readonly string[] HoatDong = { ChuaBatDau, DangDienRa, DangHoc };

            public static bool LaHoatDong(string trangThai)
            {
                return trangThai == ChuaBatDau || trangThai == DangDienRa || trangThai == DangHoc;
            }
        }

        /// <summary>
        /// ✨ Ca học tại trung tâm - MỚI CHO OPTION 2
        /// </summary>
        public static class CaHoc
        {
            public const string Sang = "Sáng (7h-9h)";
            public const string SangMuon = "Sáng muộn (9h-11h)";
            public const string Chieu = "Chiều (14h-16h)";
            public const string ChieuMuon = "Chiều muộn (16h-18h)";
            public const string Toi = "Tối (18h-20h)";
            public const string ToiMuon = "Tối muộn (20h-22h)";

            public static readonly string[] TatCaCaHoc = new string[]
            {
                Sang,
                SangMuon,
                Chieu,
                ChieuMuon,
                Toi,
                ToiMuon
            };

            /// <summary>
            /// Lấy giờ bắt đầu / kết thúc từ tên ca, ví dụ "Tối (18h-20h)" => (18, 20).
            /// Trả về false nếu tên ca không đúng định dạng.
            /// </summary>
            public static bool TryGetGio(string caHoc, out int gioBatDau, out int gioKetThuc)
            {
                gioBatDau = gioKetThuc = 0;
                if (string.IsNullOrEmpty(caHoc)) return false;

                var m = System.Text.RegularExpressions.Regex.Match(caHoc, @"\((\d{1,2})h-(\d{1,2})h\)");
                if (!m.Success) return false;

                gioBatDau = int.Parse(m.Groups[1].Value);
                gioKetThuc = int.Parse(m.Groups[2].Value);
                return true;
            }
        }

        /// <summary>
        /// Màu sắc chuẩn
        /// </summary>
        /// <summary>
        /// Màu dùng chung - lấy từ bộ giao diện "Vở ô li" (DoAnWD.UI.Theme)
        /// </summary>
        public static class Colors
        {
            public static readonly Color Green = DoAnWD.UI.Theme.Success;
            public static readonly Color Red = DoAnWD.UI.Theme.Margin;
            public static readonly Color Blue = DoAnWD.UI.Theme.Ink;
            public static readonly Color Orange = DoAnWD.UI.Theme.Warning;
            public static readonly Color Gray = DoAnWD.UI.Theme.Neutral;
            public static readonly Color LightGray = DoAnWD.UI.Theme.NeutralSoft;
            public static readonly Color DarkGray = DoAnWD.UI.Theme.Text;

            public static readonly Color ActiveGreen = DoAnWD.UI.Theme.SuccessSoft;
            public static readonly Color ActiveOrange = DoAnWD.UI.Theme.WarningSoft;
            public static readonly Color ActiveBlue = DoAnWD.UI.Theme.InkSoft;
            public static readonly Color InactiveGray = DoAnWD.UI.Theme.NeutralSoft;
        }

        /// <summary>
        /// ✅ Thông báo - BỔ SUNG ĐẦY ĐỦ
        /// </summary>
        public static class Messages
        {
            // Titles
            public const string SuccessTitle = "Thành công";
            public const string ErrorTitle = "Lỗi";
            public const string WarningTitle = "Cảnh báo";
            public const string InfoTitle = "Thông tin";
            public const string ConfirmTitle = "Xác nhận";

            // Common messages
            public const string ThemThanhCong = "Thêm mới thành công!";
            public const string SuaThanhCong = "Cập nhật thành công!";
            public const string XoaThanhCong = "Xóa thành công!";
            public const string LamMoiThanhCong = "Làm mới dữ liệu thành công!";
            public const string GhepLopThanhCong = "Ghép lớp thành công!";
            public const string HuyThaoTac = "Đã hủy thao tác!";  // ✅ THÊM MỚI

            public const string LoiKhiThem = "Lỗi khi thêm: ";
            public const string LoiKhiSua = "Lỗi khi sửa: ";
            public const string LoiKhiXoa = "Lỗi khi xóa: ";
            public const string LoiKhiGhepLop = "Lỗi khi ghép lớp: ";

            // Validation messages cho Gia Sư
            public const string VuiLongNhapHoTen = "Vui lòng nhập họ tên!";
            public const string VuiLongNhapSDT = "Vui lòng nhập số điện thoại!";
            public const string SDTKhongHopLe = "Số điện thoại không hợp lệ! (10-11 số)";
            public const string VuiLongNhapChuyenMon = "Vui lòng nhập chuyên môn!";

            // Validation messages cho Học Viên
            public const string VuiLongNhapDiaChi = "Vui lòng nhập địa chỉ!";

            // Validation messages - General
            public const string VuiLongNhapDayDu = "Vui lòng nhập đầy đủ thông tin!";
            public const string VuiLongChonHocVien = "Vui lòng chọn học viên!";
            public const string VuiLongChonGiaSu = "Vui lòng chọn gia sư!";
            public const string VuiLongChonCaHoc = "Vui lòng chọn ca học!";
            public const string HocPhiKhongHopLe = "Học phí phải lớn hơn 0!";
            public const string GiaSuKhongRanh = "Gia sư đang bận, không thể ghép lớp!";
            public const string CaHocBiTrung = "Ca học đã bị trùng với lớp khác!";
            public const string HoTenKhongHopLe = "Họ tên phải từ 2-100 ký tự và không chứa chữ số!";
            public const string SDTDaTonTai = "Số điện thoại đã được dùng cho gia sư khác!";
            public const string GiaSuTamNghi = "Gia sư đang tạm nghỉ, không thể nhận lớp mới!";
            public const string CaHocKhongHopLe = "Ca học không hợp lệ!";

            // Messages mới cho Option 2
            public const string ThoiGianKhongHopLe = "Thời gian bắt đầu phải trước thời gian kết thúc!";
            public const string ThoiGianQuaKhu = "Không thể tạo lớp học với thời gian trong quá khứ!";
            public const string ThoiGianTrung = "Thời gian học bị trùng với lớp khác!";

            // Confirmation messages
            public const string XacNhanXoa = "Bạn có chắc chắn muốn xóa?";
        }

        /// <summary>
        /// Validation Patterns
        /// </summary>
        public static class Validation
        {
            public const string PhoneRegex = @"^0\d{9,10}$";
            public const string EmailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            public const int MinNameLength = 2;
            public const int MaxNameLength = 100;
            public const int MaxChuyenMonLength = 100;
            public const int MaxDiaChiLength = 200;
            public const int MaxMonHocLength = 50;
            public const decimal MaxHocPhi = 100000000m; // 100 triệu / khóa
        }

        /// <summary>
        /// ✨ Cấu hình Timer - MỚI CHO OPTION 2
        /// </summary>
        public static class Timer
        {
            public const int AutoUpdateInterval = 60000; // milliseconds
            public const int DashboardRefreshInterval = 30000; // milliseconds
        }

        /// <summary>
        /// Format hiển thị
        /// </summary>
        public static class Format
        {
            public const string DateFormat = "dd/MM/yyyy";
            public const string DateTimeFormat = "dd/MM/yyyy HH:mm";
            public const string TimeFormat = "HH:mm";
            public const string CurrencyFormat = "N0";
        }
    }
}