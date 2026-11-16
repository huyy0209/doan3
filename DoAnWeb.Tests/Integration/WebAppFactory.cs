using System;
using System.Net.Http;
using DoAnWeb.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DoAnWeb.Tests.Integration;

/// <summary>
/// Chạy cả ứng dụng web thật trong bộ nhớ (middleware, filter, cookie, antiforgery…) nhưng trỏ sang CSDL
/// QuanLyGiaSu_Test. KHÔNG BAO GIỜ trỏ tới QuanLyGiaSu: ngay khi khởi động kiểm lại tên CSDL thật sự dùng.
/// </summary>
public sealed class WebAppFactory : WebApplicationFactory<Program>
{
    private readonly int _soLanDangNhap;

    public WebAppFactory(int soLanDangNhap = 10000)
    {
        _soLanDangNhap = soLanDangNhap;
        TestDb.DamBaoDaTao();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:QuanLyGiaSu", TestDb.ConnStr);
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("ConnectionStrings:QuanLyGiaSu", TestDb.ConnStr),
                new System.Collections.Generic.KeyValuePair<string, string>("SaoLuu:ThuMuc", System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DoAnWeb.Tests.SaoLuu")),
                new System.Collections.Generic.KeyValuePair<string, string>("SaoLuu:GioTuDong", "Tat"),
                new System.Collections.Generic.KeyValuePair<string, string>("BaoMat:GioiHanDangNhap:SoLan", _soLanDangNhap.ToString()),
                new System.Collections.Generic.KeyValuePair<string, string>("BaoMat:GioiHanDangNhap:TrongPhut", "5")
            }));
        // Máy test chạy runtime .NET 10 còn TestServer là bản 8.0: System.Text.Json 10 đòi PipeWriter.UnflushedBytes
        // mà PipeWriter của TestServer không có → mọi action trả Json() bị 500 (chỉ trong test, không phải lỗi app).
        // Đệm body vào MemoryStream (StreamPipeWriter có UnflushedBytes) ở middleware ngoài cùng.
        builder.ConfigureServices(s => s.AddTransient<IStartupFilter, DemBodyStartupFilter>());
    }

    private sealed class DemBodyStartupFilter : IStartupFilter
    {
        public Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> Configure(
            Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> next) => app =>
        {
            Microsoft.AspNetCore.Builder.UseExtensions.Use(app, async (Microsoft.AspNetCore.Http.HttpContext ctx, Microsoft.AspNetCore.Http.RequestDelegate nextMw) =>
            {
                var goc = ctx.Response.Body;
                await using var dem = new System.IO.MemoryStream();
                ctx.Response.Body = dem;
                try
                {
                    await nextMw(ctx);
                    dem.Position = 0;
                    await dem.CopyToAsync(goc);
                }
                finally { ctx.Response.Body = goc; }
            });
            next(app);
        };
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        // Chốt an toàn: nếu cấu hình nào đó làm app dùng CSDL khác QuanLyGiaSu_Test thì dừng ngay.
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuanLyGiaSuContext>();
        var ten = new SqlConnectionStringBuilder(db.Database.GetConnectionString()).InitialCatalog;
        if (!TestDb.LaCsdlTest(ten))
        {
            host.Dispose();
            throw new InvalidOperationException($"Test web trỏ nhầm CSDL '{ten}' – chỉ được dùng CSDL test (QuanLyGiaSu_Test*).");
        }
        return host;
    }

    /// <summary>Client không tự theo redirect, có giữ cookie (như một trình duyệt)</summary>
    public HttpClient TaoClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
}
