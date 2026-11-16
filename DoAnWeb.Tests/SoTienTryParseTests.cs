using DoAnWeb.Common;
using Xunit;

namespace DoAnWeb.Tests;

public class SoTienTryParseTests
{
    [Theory]
    [InlineData("250.000", 250000)]
    [InlineData("250,000", 250000)]
    [InlineData("250000", 250000)]
    [InlineData("1.250.500đ", 1250500)]
    [InlineData("  250 000 VND  ", 250000)]
    public void TryParse_chap_nhan_tien_nguyen_dinh_dang_hop_le(string text, decimal mongDoi)
    {
        Assert.True(SoTien.TryParse(text, out var soTien));
        Assert.Equal(mongDoi, soTien);
    }

    [Theory]
    [InlineData("-250.000")]
    [InlineData("1,5")]
    [InlineData("12a")]
    [InlineData("1.25.000")]
    [InlineData("1.250,000")]
    [InlineData("250 00")]
    public void TryParse_tu_choi_dau_am_chu_thua_va_nhom_sai(string text)
    {
        Assert.False(SoTien.TryParse(text, out _));
    }
}
