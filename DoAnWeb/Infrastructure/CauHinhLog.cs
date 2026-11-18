using System;
using System.IO;
using Serilog;

namespace DoAnWeb.Infrastructure;


/// <summary>
/// Đề xuất #5: ghi nhật ký hệ thống ra file theo ngày bằng Serilog.
/// Cấu hình nằm ở appsettings.json mục "Serilog"; file log ở DoAnWeb/logs/doanweb-YYYYMMDD.log,
/// giữ 14 ngày gần nhất, mức Information (Microsoft.* chỉ ghi Warning).
/// </summary>
public static class CauHinhLog
{
    /// <summary>Đăng ký Serilog làm nhà ghi log của ứng dụng (gọi một dòng trong Program.cs)</summary>
    public static WebApplicationBuilder ThemCauHinhLog(this WebApplicationBuilder builder)
    {
        // Đường dẫn trong appsettings tính theo thư mục project, để log luôn nằm ở DoAnWeb/logs
        // dù chạy bằng "dotnet run" từ thư mục nào hay bằng QuanLyGiaSu.bat web.
        var duongDan = builder.Configuration["Serilog:WriteTo:0:Args:path"];
        if (!string.IsNullOrWhiteSpace(duongDan) && !Path.IsPathRooted(duongDan))
        {
            var duongDanTuyetDoi = Path.Combine(builder.Environment.ContentRootPath, duongDan);
            builder.Configuration["Serilog:WriteTo:0:Args:path"] = duongDanTuyetDoi;
            try
            {
                var thuMuc = Path.GetDirectoryName(duongDanTuyetDoi);
                if (!string.IsNullOrWhiteSpace(thuMuc)) Directory.CreateDirectory(thuMuc);
            }
            catch (Exception)
            {
                // Không tạo được thư mục: để Serilog tự báo qua SelfLog, ứng dụng vẫn chạy
            }
        }

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }
}