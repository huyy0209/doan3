using System.Text;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

public class BaoCaoCsvTests
{
    [Theory]
    [InlineData("=1", "'=1")]
    [InlineData("+1", "'+1")]
    [InlineData("-1", "'-1")]
    [InlineData("@1", "'@1")]
    [InlineData("\t1", "'\t1")]
    [InlineData("\r1", "\"'\r1\"")]
    public void XuatCsv_ThemDauNhayDonChoGiaTriCoTheLaCongThuc(string giaTri, string mongDoi)
    {
        var service = new BaoCaoService(null!);

        var bytes = service.XuatCsv(new[] { new[] { giaTri } });
        var csv = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);

        Assert.Equal(mongDoi + Environment.NewLine, csv);
    }

    [Fact]
    public void XuatCsv_GiuNguyenGiaTriThongThuong()
    {
        var service = new BaoCaoService(null!);

        var bytes = service.XuatCsv(new[] { new[] { "Ten hoc vien" } });
        var csv = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);

        Assert.Equal("Ten hoc vien" + Environment.NewLine, csv);
    }
}
