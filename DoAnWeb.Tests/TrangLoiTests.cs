using System;
using DoAnWeb.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>Đề xuất #5: trang lỗi 404/403/500 tiếng Việt và mã tra cứu của lỗi 500.</summary>
public class TrangLoiTests
{
    private sealed class LogGhiNho<T> : ILogger<T>
    {
        public LogLevel Muc; public Exception Loi; public string NoiDung;
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Muc = logLevel; Loi = exception; NoiDung = formatter(state, exception);
        }
    }

    private static (LoiController, LogGhiNho<LoiController>) TaoController(string maTraCuu = "ma-mac-dinh")
    {
        var log = new LogGhiNho<LoiController>();
        var controller = new LoiController(log)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { TraceIdentifier = maTraCuu }
            }
        };
        return (controller, log);
    }

    [Fact]
    public void KhongTimThay_TraVe404_CoTieuDeTiengViet()
    {
        var (controller, _) = TaoController();

        var ketQua = Assert.IsType<ViewResult>(controller.KhongTimThay());
        var model = Assert.IsType<TrangLoiModel>(ketQua.Model);

        Assert.Equal(404, model.Ma);
        Assert.Equal(404, controller.Response.StatusCode);
        Assert.Equal("KhongTimThay", ketQua.ViewName);
        Assert.Contains("Không tìm thấy", model.TieuDe);
        Assert.Contains("Đường dẫn", model.MoTa);
        Assert.Contains("không tồn tại", model.MoTa);
    }

    [Fact]
    public void KhongCoQuyen_TraVe403_CoTieuDeTiengViet()
    {
        var (controller, _) = TaoController();

        var ketQua = Assert.IsType<ViewResult>(controller.KhongCoQuyen());
        var model = Assert.IsType<TrangLoiModel>(ketQua.Model);

        Assert.Equal(403, model.Ma);
        Assert.Equal(403, controller.Response.StatusCode);
        Assert.Equal("KhongCoQuyen", ketQua.ViewName);
        Assert.Contains("không có quyền", model.TieuDe);
    }

    [Fact]
    public void LoiMayChu_HienMaTraCuu_VaGhiLogKemMaDo()
    {
        var (controller, log) = TaoController("0HN7ABC:00000001");

        var ketQua = Assert.IsType<ViewResult>(controller.LoiMayChu());
        var model = Assert.IsType<TrangLoiModel>(ketQua.Model);

        Assert.Equal(500, model.Ma);
        Assert.Equal(500, controller.Response.StatusCode);
        Assert.Equal("LoiMayChu", ketQua.ViewName);
        Assert.Equal("0HN7ABC:00000001", model.MaTraCuu);
        Assert.Equal(LogLevel.Error, log.Muc);
        Assert.Contains("0HN7ABC:00000001", log.NoiDung);
        Assert.Contains("Lỗi máy chủ", log.NoiDung);
    }

    [Fact]
    public void TheoMa_400_DungTrang404_TheoMa_500_DungTrang500()
    {
        var (controller, _) = TaoController("ma-tra-cuu");

        var ma400 = Assert.IsType<ViewResult>(controller.TheoMa(400));
        Assert.Equal("KhongTimThay", ma400.ViewName);

        var ma500 = Assert.IsType<ViewResult>(controller.TheoMa(500));
        Assert.Equal("LoiMayChu", ma500.ViewName);
        Assert.Equal("ma-tra-cuu", Assert.IsType<TrangLoiModel>(ma500.Model).MaTraCuu);
    }
}
