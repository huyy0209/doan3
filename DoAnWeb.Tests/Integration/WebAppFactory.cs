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
    public WebAppFactory()
    {
        TestDb.DamBaoDaTao();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:QuanLyGiaSu", TestDb.ConnStr);
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("ConnectionStrings:QuanLyGiaSu", TestDb.ConnStr)
            }));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        // Chốt an toàn: nếu cấu hình nào đó làm app dùng CSDL khác QuanLyGiaSu_Test thì dừng ngay.
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuanLyGiaSuContext>();
        var ten = new SqlConnectionStringBuilder(db.Database.GetConnectionString()).InitialCatalog;
        if (!string.Equals(ten, "QuanLyGiaSu_Test", StringComparison.OrdinalIgnoreCase))
        {
            host.Dispose();
            throw new InvalidOperationException($"Test web trỏ nhầm CSDL '{ten}' – chỉ được dùng QuanLyGiaSu_Test.");
        }
        return host;
    }

    /// <summary>Client không tự theo redirect, có giữ cookie (như một trình duyệt)</summary>
    public HttpClient TaoClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
}
