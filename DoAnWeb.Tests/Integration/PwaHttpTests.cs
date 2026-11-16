using System.Net;
using Xunit;

namespace DoAnWeb.Tests.Integration;

public class PwaHttpTests : HttpTestBase
{
    [Fact]
    public async Task Manifest_va_trang_ngoai_tuyen_truy_cap_duoc_khi_chua_dang_nhap()
    {
        using var client = Factory.TaoClient();

        var manifest = await client.GetAsync("/manifest.webmanifest");
        Assert.Equal(HttpStatusCode.OK, manifest.StatusCode);
        Assert.Equal("application/manifest+json", manifest.Content.Headers.ContentType?.MediaType);
        Assert.Contains("\"name\": \"Quản lý gia sư\"", await manifest.Content.ReadAsStringAsync());

        var offline = await client.GetAsync("/offline.html");
        Assert.Equal(HttpStatusCode.OK, offline.StatusCode);
        Assert.Contains("Bạn đang ngoại tuyến", await offline.Content.ReadAsStringAsync());

        var worker = await client.GetAsync("/service-worker.js");
        Assert.Equal(HttpStatusCode.OK, worker.StatusCode);
        Assert.StartsWith("text/javascript", worker.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Layout_co_nut_giao_dien_va_nap_script_truoc_css()
    {
        var quanLy = await DangNhapQL();
        var trang = await quanLy.Get("/GiaSu");

        Assert.Equal(HttpStatusCode.OK, trang.Status);
        Assert.NotNull(trang.Doc.QuerySelector("button[data-theme-toggle]"));
        Assert.True(trang.Html.IndexOf("/js/giao-dien.js", System.StringComparison.Ordinal)
                    < trang.Html.IndexOf("/css/site.css", System.StringComparison.Ordinal));
    }
}
