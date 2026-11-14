using System.Collections.Generic;
using System.Security.Claims;
using DoAnWeb.Infrastructure;
using DoAnWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// TK-11: đăng nhập bằng mật khẩu mặc định (admin123) → mọi trang buộc chuyển sang Đổi mật khẩu.
/// Logic nằm ở filter BatDoiMatKhauFilter (đọc claim PhaiDoiMatKhau = "1").
/// </summary>
public class BatDoiMatKhauFilterTests
{
    private static ActionExecutingContext TaoContext(string controller, string action, bool phaiDoi, bool dangNhap = true)
    {
        var http = new DefaultHttpContext();
        if (dangNhap)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(LoaiClaim.PhaiDoiMatKhau, phaiDoi ? "1" : "0")
            };
            http.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        }

        var routeData = new RouteData(new RouteValueDictionary
        {
            ["controller"] = controller,
            ["action"] = action
        });
        return new ActionExecutingContext(
            new ActionContext(http, routeData, new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new object());
    }

    [Fact]
    [Trait("Luat", "TK-11")]
    public void PhaiDoiMatKhau_TrangThuong_ChuyenHuongDoiMatKhau()
    {
        var filter = new BatDoiMatKhauFilter();
        var ctx = TaoContext("Home", "Index", phaiDoi: true);

        filter.OnActionExecuting(ctx);

        var redirect = Assert.IsType<RedirectToActionResult>(ctx.Result);
        Assert.Equal("DoiMatKhau", redirect.ActionName);
        Assert.Equal("TaiKhoan", redirect.ControllerName);
    }

    [Theory]
    [Trait("Luat", "TK-11")]
    [InlineData("DoiMatKhau")]
    [InlineData("DangXuat")]
    public void PhaiDoiMatKhau_TrangChoPhep_KhongChuyenHuong(string action)
    {
        var filter = new BatDoiMatKhauFilter();
        var ctx = TaoContext("TaiKhoan", action, phaiDoi: true);

        filter.OnActionExecuting(ctx);

        Assert.Null(ctx.Result);
    }

    [Fact]
    [Trait("Luat", "TK-11")]
    public void KhongPhaiDoiMatKhau_KhongChuyenHuong()
    {
        var filter = new BatDoiMatKhauFilter();
        var ctx = TaoContext("Home", "Index", phaiDoi: false);

        filter.OnActionExecuting(ctx);

        Assert.Null(ctx.Result);
    }

    [Fact]
    [Trait("Luat", "TK-11")]
    public void ChuaDangNhap_KhongChuyenHuong()
    {
        var filter = new BatDoiMatKhauFilter();
        var ctx = TaoContext("Home", "Index", phaiDoi: true, dangNhap: false);

        filter.OnActionExecuting(ctx);

        Assert.Null(ctx.Result);
    }
}
