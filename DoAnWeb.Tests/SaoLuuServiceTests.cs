using DoAnWeb.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests;

[Collection("Database")]
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
    public async Task Chan_ten_CSDL_WinForms_truoc_khi_ket_noi()
    {
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:QuanLyGiaSu"] = "Server=localhost;Database=QuanLyGiaSu;Integrated Security=true;TrustServerCertificate=true",
            ["SaoLuu:ThuMuc"] = Path.GetTempPath()
        }).Build();
        var service = new SaoLuuService(cfg, NullLogger<SaoLuuService>.Instance, null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.KhoiPhucAsync("QuanLyGiaSu_20261115_230000.bak"));
    }

    [Fact]
    public void Mat_khau_sai_khong_duoc_xac_thuc()
    {
        var hash = DoAnWeb.Services.MatKhau.Bam("Dung123");
        Assert.False(DoAnWeb.Services.MatKhau.KiemTra("Sai123", hash));
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
        var service = new SaoLuuService(cauHinh, NullLogger<SaoLuuService>.Instance, null);
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

    [Fact]
    [Trait("Can", "SqlServerWindows")]
    public async Task Khoi_phuc_that_tren_CSDL_test_cua_tien_trinh()
    {
        TestDb.DamBaoDaTao();
        string folder;
        await using (var cn = new SqlConnection(new SqlConnectionStringBuilder(TestDb.ConnStr) { InitialCatalog = "master" }.ConnectionString))
        {
            await cn.OpenAsync();
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000))";
            folder = (string)await cmd.ExecuteScalarAsync();
        }
        Assert.False(string.IsNullOrWhiteSpace(folder));
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:QuanLyGiaSu"] = TestDb.ConnStr,
            ["SaoLuu:ThuMuc"] = folder,
            ["SaoLuu:GiuSoBan"] = "14"
        }).Build();
        var service = new SaoLuuService(cfg, NullLogger<SaoLuuService>.Instance, null);
        string source = await service.SaoLuuAsync();
        try
        {
            var before = await service.KhoiPhucAsync(source);
            Assert.Matches("^QuanLyGiaSu_\\d{8}_\\d{6}\\.bak$", before);
            Assert.True(new FileInfo(Path.Combine(folder, before)).Length > 0);
            await using var db = TestDb.TaoContext();
            await db.Database.OpenConnectionAsync();
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT DB_NAME()";
            var actualDatabase = (string)await command.ExecuteScalarAsync();
            Assert.Equal(TestDb.TenCsdl, actualDatabase);
        }
        finally
        {
            foreach (var f in Directory.EnumerateFiles(folder, "*.bak")) File.Delete(f);
        }
    }

    [Fact]
    [Trait("Can", "SqlServerWindows")]
    public async Task Backup_thieu_bang_web_bi_rollback_ve_ban_truoc()
    {
        TestDb.DamBaoDaTao();
        string folder;
        await using (var cn = new SqlConnection(new SqlConnectionStringBuilder(TestDb.ConnStr) { InitialCatalog = "master" }.ConnectionString))
        {
            await cn.OpenAsync();
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000))";
            folder = (string)await cmd.ExecuteScalarAsync();
        }
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:QuanLyGiaSu"] = TestDb.ConnStr,
            ["SaoLuu:ThuMuc"] = folder
        }).Build();
        var service = new SaoLuuService(cfg, NullLogger<SaoLuuService>.Instance, null);
        var backupsBefore = new HashSet<string>(Directory.EnumerateFiles(folder, "QuanLyGiaSu_*.bak"), StringComparer.OrdinalIgnoreCase);
        var healthy = await service.SaoLuuAsync();
        string incompatible = null;
        try
        {
            await using (var cn = new SqlConnection(TestDb.ConnStr))
            {
                await cn.OpenAsync();
                await using var cmd = cn.CreateCommand();
                cmd.CommandText = "DROP TABLE dbo.ThongBao";
                await cmd.ExecuteNonQueryAsync();
            }
            incompatible = await service.SaoLuuAsync();
            await service.KhoiPhucAsync(healthy);

            var error = await Assert.ThrowsAsync<SaoLuuKhongTuongThichException>(() => service.KhoiPhucAsync(incompatible));
            Assert.Contains("Bản sao lưu quá cũ, không tương thích; đã quay về dữ liệu trước khi khôi phục", error.Message);
            await using var verify = new SqlConnection(TestDb.ConnStr);
            await verify.OpenAsync();
            await using var check = verify.CreateCommand();
            check.CommandText = "SELECT OBJECT_ID(N'dbo.ThongBao', N'U')";
            Assert.NotNull(await check.ExecuteScalarAsync());
        }
        finally
        {
            foreach (var file in Directory.EnumerateFiles(folder, "QuanLyGiaSu_*.bak").Where(file => !backupsBefore.Contains(file))) File.Delete(file);
        }
    }

    [Fact]
    public void Khoi_phuc_bat_gioi_han_tan_suat_giong_dang_nhap()
    {
        var method = typeof(DoAnWeb.Controllers.SaoLuuController).GetMethod(nameof(DoAnWeb.Controllers.SaoLuuController.KhoiPhuc), new[] { typeof(DoAnWeb.ViewModels.SaoLuuVM.KhoiPhucSaoLuuVM), typeof(CancellationToken) });
        Assert.NotNull(method);
        var attribute = Assert.Single(method!.GetCustomAttributes(typeof(Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute), false));
        Assert.Equal("gioi-han-dang-nhap", ((Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute)attribute).PolicyName);
    }
}

