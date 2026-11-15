using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace DoAnWeb.E2E;

/// <summary>
/// Cơ sở cho test giao diện: mỗi test một BrowserContext sạch (cookie riêng), tự chụp ảnh khi fail.
/// Các test cùng collection "E2E" nên chạy tuần tự, dùng chung một web + một bộ dữ liệu E2E_.
/// </summary>
[Collection("E2E")]
public abstract class GiaoDienTestBase
{
    protected readonly GiaoDienFixture F;

    protected GiaoDienTestBase(GiaoDienFixture f) => F = f;

    protected DuLieuE2E Du => F.Du;
    protected string Url(string duongDan) => F.Web.DiaChi + duongDan;

    /// <summary>Khung desktop mặc định</summary>
    protected static readonly ViewportSize Desktop = new() { Width = 1366, Height = 768 };

    /// <summary>Khung điện thoại 390×844 (kịch bản 5)</summary>
    protected static readonly ViewportSize DienThoai = new() { Width = 390, Height = 844 };

    /// <summary>
    /// Chạy một kịch bản trong context mới; nếu ném lỗi (assert / timeout) thì chụp ảnh vào
    /// DoAnWeb.E2E/artifacts/&lt;tên&gt;-&lt;giờ&gt;.png rồi ném lại lỗi cũ.
    /// </summary>
    protected async Task Chay(string ten, Func<IPage, Task> kichBan, ViewportSize khung = null)
    {
        await using var context = await F.TrinhDuyet.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = khung ?? Desktop,
            Locale = "vi-VN",
            TimezoneId = "Asia/Ho_Chi_Minh"
        });
        context.SetDefaultTimeout(15000);
        var trang = await context.NewPageAsync();
        try
        {
            await kichBan(trang);
        }
        catch
        {
            try
            {
                Directory.CreateDirectory(GiaoDienFixture.ThuMucAnh);
                string tep = Path.Combine(GiaoDienFixture.ThuMucAnh, $"{ten}-{DateTime.Now:yyyyMMdd-HHmmss}.png");
                await trang.ScreenshotAsync(new PageScreenshotOptions { Path = tep, FullPage = true });
            }
            catch { /* không chụp được thì thôi, giữ lỗi gốc */ }
            throw;
        }
    }

    protected async Task DangNhap(IPage trang, string tenDangNhap)
    {
        await trang.GotoAsync(Url("/TaiKhoan/DangNhap"));
        await trang.FillAsync("#TenDangNhap", tenDangNhap);
        await trang.FillAsync("#MatKhau", DuLieuE2E.MatKhau);
        await trang.ClickAsync("form button[type=submit]");
    }

    /// <summary>Toast thông báo thành công (TempData["ThongBao"]) chứa đoạn chữ</summary>
    protected static Task CoThongBao(IPage trang, string doanChu) =>
        Expect(trang.Locator(".vo-toast-success")).ToContainTextAsync(doanChu);
}
