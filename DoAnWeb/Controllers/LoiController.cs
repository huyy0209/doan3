using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DoAnWeb.Controllers
{
    /// <summary>
    /// Đề xuất #5: trang lỗi tiếng Việt đúng giao diện app.
    /// UseStatusCodePagesWithReExecute("/Loi/{0}") gọi các action dưới đây cho 404/403,
    /// UseExceptionHandler("/Loi/500") gọi LoiMayChu khi có lỗi ngoài dự kiến (Production).
    /// </summary>
    [AllowAnonymous]
    public class LoiController : Controller
    {
        private readonly ILogger<LoiController> _log;
        public LoiController(ILogger<LoiController> log) { _log = log; }

        /// <summary>GET đường dẫn không tồn tại → 404</summary>
        [Route("Loi/404")]
        public IActionResult KhongTimThay() => DungTrang(404, "Không tìm thấy trang",
            "Đường dẫn không tồn tại hoặc đã bị đổi. Anh/chị kiểm tra lại địa chỉ, hoặc dùng nút bên dưới để về trang trước.");

        [Route("Loi/429")]
        public IActionResult QuaGioiHan() => DungTrang(429, "Thử đăng nhập quá nhiều lần",
            "Anh/chị đã gửi quá nhiều yêu cầu đăng nhập. Vui lòng chờ vài phút rồi thử lại.");

        /// <summary>Không đủ quyền (403)</summary>
        [Route("Loi/403")]
        public IActionResult KhongCoQuyen() => DungTrang(403, "Bạn không có quyền vào trang này",
            "Tài khoản đang đăng nhập không được phép xem hoặc thao tác ở mục này. Nếu cần dùng, hãy liên hệ quản lý trung tâm để được cấp quyền.");

        /// <summary>Lỗi ngoài dự kiến (500): hiện mã tra cứu và ghi log kèm mã đó</summary>
        [Route("Loi/500")]
        public IActionResult LoiMayChu()
        {
            var maTraCuu = HttpContext?.TraceIdentifier ?? "?";
            var loi = HttpContext?.Features.Get<IExceptionHandlerFeature>()?.Error;
            _log.LogError(loi, "Lỗi máy chủ (mã tra cứu {MaTraCuu}) tại {DuongDan}",
                maTraCuu, HttpContext?.Request?.Path.ToString());

            Response.StatusCode = 500;
            return View("LoiMayChu", new TrangLoiModel
            {
                Ma = 500,
                TieuDe = "Có lỗi xảy ra khi xử lý yêu cầu",
                MoTa = "Hệ thống chưa hoàn tất được thao tác. Anh/chị thử lại; nếu vẫn lỗi, báo quản lý kèm mã tra cứu bên dưới.",
                MaTraCuu = maTraCuu
            });
        }

        /// <summary>Các mã trạng thái khác (400, 405…): dùng lại trang gần nhất</summary>
        [Route("Loi/{ma:int}")]
        public IActionResult TheoMa(int ma)
        {
            if (ma == 403) return KhongCoQuyen();
            if (ma >= 500) return LoiMayChu();
            if (ma == 404) return KhongTimThay();
            // Giữ đúng mã gốc (400, 405…) – trước đây mọi mã khác đều bị đổi thành 404
            if (ma == 400)
                return DungTrang(400, "Yêu cầu không hợp lệ",
                    "Biểu mẫu đã hết hạn hoặc không hợp lệ. Hãy tải lại trang rồi thử lại.");
            return DungTrang(ma, $"Lỗi {ma}", "Không xử lý được yêu cầu này.");
        }

        private IActionResult DungTrang(int ma, string tieuDe, string moTa)
        {
            Response.StatusCode = ma;
            return View(ma == 403 ? "KhongCoQuyen" : "KhongTimThay",
                new TrangLoiModel { Ma = ma, TieuDe = tieuDe, MoTa = moTa });
        }
    }

    /// <summary>Dữ liệu cho các view trong Views/Loi</summary>
    public class TrangLoiModel
    {
        public int Ma { get; set; }
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        /// <summary>Mã tra cứu (TraceIdentifier) – chỉ trang 500 dùng để người dùng báo lại</summary>
        public string MaTraCuu { get; set; }
    }
}
