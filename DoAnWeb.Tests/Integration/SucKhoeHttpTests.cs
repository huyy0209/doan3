using System.Net;
using System.Text.Json;
using Xunit;

namespace DoAnWeb.Tests.Integration;

public sealed class SucKhoeHttpTests : HttpTestBase
{
    [Fact]
    public async Task SucKhoe_KhachChuaDangNhap_TraTrangThaiCSDLKhongLoChuoiKetNoi()
    {
        using var client = Factory.TaoClient();
        using var response = await client.GetAsync("/suc-khoe");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", json.RootElement.GetProperty("trangThai").GetString());
        Assert.DoesNotContain("QuanLyGiaSu_Web_Test", json.RootElement.ToString());
        Assert.DoesNotContain("Password", json.RootElement.ToString());
    }
}
