using System;
using DoAnWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DoAnWeb.Infrastructure
{
    /// <summary>
    /// TK-11: đăng nhập bằng mật khẩu mặc định (admin123) thì mọi trang đều chuyển về Đổi mật khẩu
    /// cho tới khi đặt mật khẩu riêng (giống frmDangNhap.cs:124-132 bắt đổi trước khi vào).
    /// </summary>
    public class BatDoiMatKhauFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true) return;
            if (user.FindFirst(LoaiClaim.PhaiDoiMatKhau)?.Value != "1") return;

            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            bool choPhep = string.Equals(controller, "TaiKhoan", StringComparison.OrdinalIgnoreCase) &&
                           (string.Equals(action, "DoiMatKhau", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(action, "DangXuat", StringComparison.OrdinalIgnoreCase));
            if (!choPhep)
                context.Result = new RedirectToActionResult("DoiMatKhau", "TaiKhoan", null);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }

    /// <summary>
    /// Service ném UnauthorizedAccessException (không phải quản lý / không phải lớp của mình) → trang 403.
    /// Lỗi nghiệp vụ khác (ArgumentException, InvalidOperationException) controller tự bắt để hiện thông báo.
    /// </summary>
    public class XuLyLoiNghiepVuFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is UnauthorizedAccessException)
            {
                context.Result = new RedirectToActionResult("KhongCoQuyen", "TaiKhoan", null);
                context.ExceptionHandled = true;
            }
        }
    }
}

namespace DoAnWeb.Infrastructure
{
    /// <summary>Đặt ViewData["TenTrungTam"] cho _Layout (tên trung tâm trong Cài đặt – CD-01)</summary>
    public class TenTrungTamFilter : Microsoft.AspNetCore.Mvc.Filters.IResultFilter
    {
        private readonly DoAnWeb.Services.CaiDatService _caiDat;
        public TenTrungTamFilter(DoAnWeb.Services.CaiDatService caiDat) { _caiDat = caiDat; }

        public void OnResultExecuting(Microsoft.AspNetCore.Mvc.Filters.ResultExecutingContext context)
        {
            if (context.Controller is Microsoft.AspNetCore.Mvc.Controller c && c.ViewData["TenTrungTam"] == null)
            {
                try { c.ViewData["TenTrungTam"] = _caiDat.TrungTam?.TenHienThi; }
                catch { /* CSDL lỗi: layout dùng tên mặc định */ }
            }
        }

        public void OnResultExecuted(Microsoft.AspNetCore.Mvc.Filters.ResultExecutedContext context) { }
    }
}
