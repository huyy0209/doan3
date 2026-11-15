using DoAnWeb.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DoAnWeb.Tests;

public class SaoLuuServiceTests
{
    [Theory]
    [InlineData("../QuanLyGiaSu_20261115_230000.bak")]
    [InlineData("..\\QuanLyGiaSu_20261115_230000.bak")]
    [InlineData("C:\\temp\\QuanLyGiaSu_20261115_230000.bak")]
    [InlineData("other.bak")]
    public void Chan_ten_file_traversal_va_ten_khong_hop_le(string tenTep)
    {
        Assert.Throws<ArgumentException>(() =>
            SaoLuuService.LayDuongDanAnToan(Path.GetTempPath(), tenTep));
    }

    [Fact]
    public void Chi_tao_duong_dan_file_trong_thu_muc_sao_luu()
    {
        var thuMuc = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var duongDan = SaoLuuService.LayDuongDanAnToan(thuMuc, "QuanLyGiaSu_20261115_230000.bak");

        Assert.Equal(Path.GetFullPath(Path.Combine(thuMuc, "QuanLyGiaSu_20261115_230000.bak")), duongDan);
    }

    [Fact]
    [Trait("Can", "SqlServerWindows")]
    public async Task Sao_luu_CSDL_Test_tao_file_bak_va_don_file()
    {
        TestDb.DamBaoDaTao();
        var cauHinh = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:QuanLyGiaSu"] = TestDb.ConnStr,
            ["SaoLuu:ThuMuc"] = "",
            ["SaoLuu:GiuSoBan"] = "14"
        }).Build();
        var service = new SaoLuuService(cauHinh, NullLogger<SaoLuuService>.Instance);
        string tenTep = null;

        try
        {
            tenTep = await service.SaoLuuAsync();
            using var tep = await service.MoTepAsync(tenTep);
            Assert.True(tep.Length > 0);
        }
        finally
        {
            if (tenTep != null)
                await service.XoaTepAsync(tenTep);
        }
    }
}
