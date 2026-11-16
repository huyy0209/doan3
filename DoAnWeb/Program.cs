using System.Globalization;
using DoAnWeb.Data;
using DoAnWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ---------- Chạy như dịch vụ Windows (đề xuất #7); ngoài dịch vụ vẫn chạy console như cũ ----------
builder.Host.UseWindowsService();

// ---------- Nhật ký hệ thống ra file theo ngày (Serilog, đề xuất #5) ----------
DoAnWeb.Infrastructure.CauHinhLog.ThemCauHinhLog(builder);

// ---------- CSDL: dùng nguyên CSDL QuanLyGiaSu của bản WinForms (không migration) ----------
builder.Services.AddScoped<NhatKySaveChangesInterceptor>();
builder.Services.AddDbContext<QuanLyGiaSuContext>((services, o) =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("QuanLyGiaSu"));
    o.AddInterceptors(services.GetRequiredService<NhatKySaveChangesInterceptor>());
});
builder.Services.AddHealthChecks()
    .AddDbContextCheck<QuanLyGiaSuContext>();

// ---------- Đăng nhập bằng cookie, đọc bảng TaiKhoan ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/TaiKhoan/DangNhap";
        o.LogoutPath = "/TaiKhoan/DangXuat";
        o.AccessDeniedPath = "/TaiKhoan/KhongCoQuyen";
        o.Cookie.Name = "QuanLyGiaSu.Auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        // Mặc định hỗ trợ localhost qua HTTP; triển khai HTTPS thì bật BaoMat:BatBuocHttps.
        o.Cookie.SecurePolicy = builder.Configuration.GetValue<bool>("BaoMat:BatBuocHttps")
            ? Microsoft.AspNetCore.Http.CookieSecurePolicy.Always
            : Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        // Review #1: kiểm lại tài khoản (khóa / đổi vai trò) ở mỗi request, có cache ngắn
        o.Events.OnValidatePrincipal = DoAnWeb.Infrastructure.KiemTraPhienDangNhap.KiemTra;
    });
builder.Services.AddAuthorization();

// ---------- Mặc định mọi trang phải đăng nhập; trang công khai đánh dấu [AllowAnonymous] ----------
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
    o.Filters.Add<DoAnWeb.Infrastructure.BatDoiMatKhauFilter>();
    o.Filters.Add<DoAnWeb.Infrastructure.XuLyLoiNghiepVuFilter>();
    o.Filters.Add<DoAnWeb.Infrastructure.TenTrungTamFilter>();
});
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    o.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});
builder.Services.AddRateLimiter(o =>
{
    int soLan = Math.Clamp(builder.Configuration.GetValue<int?>("BaoMat:GioiHanDangNhap:SoLan") ?? 10, 1, 1000);
    int soPhut = Math.Clamp(builder.Configuration.GetValue<int?>("BaoMat:GioiHanDangNhap:TrongPhut") ?? 5, 1, 1440);
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("gioi-han-dang-nhap", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "khong-ro-ip",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = soLan,
            Window = TimeSpan.FromMinutes(soPhut),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

// ---------- Service nghiệp vụ (scoped: mỗi request một DbContext) ----------
builder.Services.AddScoped<INguoiDungHienTai, NguoiDungHttp>();
builder.Services.AddScoped<NhatKyService>();
builder.Services.AddScoped<NhatKyExcelService>();
builder.Services.AddScoped<TaiKhoanService>();
builder.Services.AddScoped<GiaSuService>();
builder.Services.AddScoped<HocVienService>();
builder.Services.AddScoped<TimKiemNhanhService>();
builder.Services.AddScoped<NhapExcelService>();
builder.Services.AddScoped<LopHocService>();
builder.Services.AddScoped<LichDayService>();
builder.Services.AddScoped<SaoLuuService>();
// Service phụ do W0 chuyển: HocTap, HocPhi, MonHoc, TongQuan, ThongBao, CaiDat, ThongKe, BaoCao
builder.Services.AddDichVuPhu();
builder.Services.AddScoped<ThongKeBieuDoService>();
builder.Services.AddScoped<NhacNoService>();
builder.Services.AddHostedService<CapNhatTrangThaiNen>();
builder.Services.AddHostedService<TuSaoLuuNen>();
builder.Services.AddHostedService<NhatKyDonDepNen>();

var app = builder.Build();

// Endpoint cho Docker và bộ giám sát; chỉ công khai trạng thái, không trả chi tiết lỗi kết nối.
app.MapHealthChecks("/suc-khoe", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(new { trangThai = report.Status.ToString() });
    }
}).AllowAnonymous();

// ---------- Trang lỗi tiếng Việt: 404/403/500 dùng chung giao diện app ----------
app.UseStatusCodePagesWithReExecute("/Loi/{0}");

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; font-src 'self' data:; connect-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        return Task.CompletedTask;
    });
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Loi/500");
    app.UseHsts();
}

// ---------- Lần chạy đầu: bảng TaiKhoan rỗng thì tạo admin/admin123 (TK-10) ----------
using (var scope = app.Services.CreateScope())
{
    try
    {
        var tk = scope.ServiceProvider.GetRequiredService<TaiKhoanService>();
        if (tk.DamBaoTaiKhoanMacDinh())
            app.Logger.LogWarning("Đã tạo tài khoản mặc định admin / admin123 – đăng nhập và đổi mật khẩu ngay.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Không kết nối được CSDL QuanLyGiaSu_Web. Kiểm tra SQL Server và chuỗi kết nối trong appsettings.json.");
    }
}

// Định dạng tiếng Việt (ngày dd/MM/yyyy, số 1.500.000)
var vi = new CultureInfo("vi-VN");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(vi),
    SupportedCultures = new[] { vi },
    SupportedUICultures = new[] { vi }
});

app.UseHttpsRedirection();
app.UseResponseCompression();
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider,
    OnPrepareResponse = context =>
    {
        string path = context.Context.Request.Path.Value ?? "";
        if (path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase))
            context.Context.Response.Headers.CacheControl = "public,max-age=604800";
    }
});
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

/// <summary>Để project test dùng WebApplicationFactory nếu cần</summary>
public partial class Program { }
