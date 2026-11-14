using System;
using DoAnWeb.Infrastructure;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>Review #4: lỗi CSDL không lộ ra giao diện, chỉ vào log.</summary>
public class LoiHeThongTests
{
    private sealed class LogGhiNho : ILogger
    {
        public LogLevel Muc; public Exception Loi; public string NoiDung;
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        { Muc = logLevel; Loi = exception; NoiDung = formatter(state, exception); }
    }

    [Fact]
    public void LoiCsdl_ThongBaoChung_KhongLoChiTiet_VaDuocGhiLog()
    {
        var log = new LogGhiNho();
        var loi = new InvalidCastException("outer", new Exception("FK_LopHoc_GiaSu constraint violation, table dbo.LopHoc"));

        string tb = LoiHeThong.GhiLogVaLayThongBao(log, loi, "ghép lớp");

        Assert.Equal("Không lưu được, vui lòng thử lại.", tb);
        Assert.DoesNotContain("FK_", tb);
        Assert.Equal(LogLevel.Error, log.Muc);
        Assert.Same(loi, log.Loi);
        Assert.Contains("ghép lớp", log.NoiDung);
    }
}
