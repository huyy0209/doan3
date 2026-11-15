using Microsoft.Playwright;
using Xunit;

namespace DoAnWeb.E2E;

[CollectionDefinition("E2E")]
public class E2ECollection : ICollectionFixture<GiaoDienFixture> { }

/// <summary>
/// Dùng chung cho cả bộ E2E (một web Kestrel + một Chromium + một bộ dữ liệu E2E_):
/// tạo CSDL test → dọn dữ liệu cũ → nạp dữ liệu → bật web → bật trình duyệt; Dispose làm ngược lại.
/// </summary>
public sealed class GiaoDienFixture : IAsyncLifetime
{
    public MayChuWeb Web { get; private set; }
    public DuLieuE2E Du { get; private set; }
    public IBrowser TrinhDuyet { get; private set; }
    private IPlaywright _playwright;

    /// <summary>Thư mục ảnh chụp khi test fail: DoAnWeb.E2E/artifacts/ (đã ignore trong .gitignore).</summary>
    public static string ThuMucAnh { get; } = TimThuMucAnh();

    private static string TimThuMucAnh()
    {
        // …/DoAnWeb.E2E/bin/Debug/net8.0 → …/DoAnWeb.E2E/artifacts
        var goc = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)).Parent!.Parent!.Parent!;
        return Path.Combine(goc.FullName, "artifacts");
    }

    public async Task InitializeAsync()
    {
        DoAnWeb.Tests.TestDb.DamBaoDaTao();
        Du = new DuLieuE2E();
        using (var db = DoAnWeb.Tests.TestDb.TaoContext())
        {
            DuLieuE2E.DonDep(db);
            Du.Nap(db);
        }

        Web = await MayChuWeb.KhoiDong(DoAnWeb.Tests.TestDb.ConnStr);

        _playwright = await Playwright.CreateAsync();
        TrinhDuyet = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task DisposeAsync()
    {
        if (TrinhDuyet != null) await TrinhDuyet.CloseAsync();
        _playwright?.Dispose();
        Web?.Dispose();
        try
        {
            using var db = DoAnWeb.Tests.TestDb.TaoContext();
            DuLieuE2E.DonDep(db);
        }
        catch { /* dọn thất bại thì lần chạy sau DonDep đầu InitializeAsync sẽ dọn tiếp */ }
    }
}
