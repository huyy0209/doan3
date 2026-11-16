using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace DoAnWeb.Tests.Integration;

[Collection("Database")]
public sealed class BaoMatNenHttpTests
{
    [Fact]
    public async Task Trang_co_header_bao_mat_va_file_tinh_co_cache()
    {
        using var factory = new WebAppFactory();
        using var client = factory.TaoClient();
        using var response = await client.GetAsync("/TaiKhoan/DangNhap");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Contains("script-src 'self'; style-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Equal("camera=(), microphone=(), geolocation=()", response.Headers.GetValues("Permissions-Policy").Single());

        using var asset = await client.GetAsync("/css/site.css");
        Assert.Equal(HttpStatusCode.OK, asset.StatusCode);
        Assert.Contains("max-age=604800", asset.Headers.GetValues("Cache-Control").Single());
    }

    [Fact]
    public async Task Dang_nhap_vuot_nguong_tra_429_va_thong_bao_tieng_Viet()
    {
        using var factory = new WebAppFactory(2);
        using var browser = new TrinhDuyet(factory);

        await browser.DangNhap("khong_ton_tai", "SaiSai999");
        await browser.DangNhap("khong_ton_tai", "SaiSai999");
        var biGioiHan = await browser.DangNhap("khong_ton_tai", "SaiSai999");

        Assert.Equal((HttpStatusCode)429, biGioiHan.Status);
        Assert.Contains("quá nhiều yêu cầu đăng nhập", biGioiHan.Chu);
        Assert.Contains("chờ vài phút", biGioiHan.Chu);
    }

    [Fact]
    public async Task Chap_nhan_gzip_thi_phan_hoi_duoc_nen()
    {
        using var factory = new WebAppFactory();
        using var client = factory.TaoClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/TaiKhoan/DangNhap");
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Content.Headers.ContentEncoding, encoding => encoding == "gzip");
    }
}
