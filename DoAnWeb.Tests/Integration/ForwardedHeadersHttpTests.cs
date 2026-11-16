using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DoAnWeb.Tests.Integration;

public class ForwardedHeadersHttpTests : HttpTestBase
{
    [Fact]
    public async Task XForwardedProto_https_tu_loopback_dat_Request_IsHttps()
    {
        using var client = Factory.TaoClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/__test/forwarded-https");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");

        using var response = await client.SendAsync(request);

        Assert.Equal("true", response.Headers.GetValues("X-Test-Request-IsHttps").Single());
    }

    [Fact]
    public async Task XForwardedProto_https_tu_proxy_khong_duoc_tin_khong_dat_IsHttps()
    {
        using var client = Factory.TaoClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/__test/forwarded-https-untrusted");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");

        using var response = await client.SendAsync(request);

        Assert.Equal("false", response.Headers.GetValues("X-Test-Request-IsHttps").Single());
    }
}
