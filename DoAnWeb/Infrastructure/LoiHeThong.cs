using System;
using Microsoft.Extensions.Logging;

namespace DoAnWeb.Infrastructure
{
    /// <summary>
    /// Review #4: lỗi ngoài dự kiến (thường là lỗi CSDL) chỉ ghi log, giao diện hiện câu chung –
    /// không lộ tên bảng / ràng buộc SQL ra cho người dùng.
    /// </summary>
    public static class LoiHeThong
    {
        public const string ThongBaoChung = "Không lưu được, vui lòng thử lại.";

        /// <summary>Ghi log đầy đủ (kèm exception) và trả về câu chung để hiện cho người dùng</summary>
        public static string GhiLogVaLayThongBao(ILogger logger, Exception ex, string hanhDong)
        {
            logger?.LogError(ex, "Lỗi khi {HanhDong}", hanhDong);
            return ThongBaoChung;
        }
    }
}
