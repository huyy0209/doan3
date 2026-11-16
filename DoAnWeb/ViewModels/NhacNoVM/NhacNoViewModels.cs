using System.Collections.Generic;
using DoAnWeb.Services;

namespace DoAnWeb.ViewModels.NhacNo
{
    /// <summary>Dữ liệu trang Nhắc nợ: danh sách một trang + bộ lọc đang dùng.</summary>
    public class NhacNoIndexViewModel
    {
        public List<NhacNoDong> DanhSach { get; set; } = new List<NhacNoDong>();

        /// <summary>Chỉ giữ lớp còn nợ từ mức này trở lên (null = không lọc).</summary>
        public decimal? ConNoToiThieu { get; set; }

        /// <summary>Chỉ giữ lớp đã quá số ngày này chưa đóng (null = không lọc).</summary>
        public int? QuaNgay { get; set; }

        /// <summary>Môn học cần lọc (null = tất cả).</summary>
        public string Mon { get; set; }

        /// <summary>Các môn có trong danh sách nhắc nợ, dùng cho ô chọn lọc.</summary>
        public List<string> DanhSachMon { get; set; } = new List<string>();

        /// <summary>Tổng còn nợ của các lớp khớp bộ lọc (mọi trang).</summary>
        public decimal TongConNo { get; set; }

        /// <summary>Số lớp khớp bộ lọc (mọi trang).</summary>
        public int TongSoLop { get; set; }
    }
}
