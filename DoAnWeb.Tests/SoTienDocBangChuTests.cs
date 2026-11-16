using DoAnWeb.Common;
using Xunit;

namespace DoAnWeb.Tests;

public class SoTienDocBangChuTests
{
    [Theory]
    [InlineData(0, "Không đồng")]
    [InlineData(15, "mười lăm đồng")]
    [InlineData(105, "một trăm lẻ năm đồng")]
    [InlineData(1000000, "một triệu đồng")]
    [InlineData(1250500, "một triệu hai trăm năm mươi nghìn năm trăm đồng")]
    [InlineData(2000000000, "hai tỷ đồng")]
    public void Doc_bang_chu_cac_moc_thuong_gap(decimal soTien, string mongDoi)
    {
        Assert.Equal(mongDoi, SoTien.DocBangChu(soTien));
    }
}
