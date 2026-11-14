using System.Security.Claims;
using DoAnWeb.Common;
using DoAnWeb.Infrastructure;
using DoAnWeb.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>Review #1 (TK-08, TK-17): khóa tài khoản / đổi vai trò có hiệu lực ngay với cookie đang dùng.</summary>
[Collection("Database")]
public class KiemTraPhienDangNhapTests : DbTestBase
{
    private static ClaimsPrincipal Principal(int maTK, string vaiTro) =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(LoaiClaim.MaTK, maTK.ToString()),
            new Claim(ClaimTypes.Name, "x"),
            new Claim(ClaimTypes.Role, vaiTro)
        }, "test"));

    private static IMemoryCache Cache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    [Trait("Luat", "TK-17")]
    public void TaiKhoanHoatDong_HopLe()
    {
        var tk = TaoTaiKhoan(TenTest(), "abc123", Constants.VaiTro.QuanLy);
        Assert.True(KiemTraPhienDangNhap.HopLe(Principal(tk.MaTK, tk.VaiTro), Db, Cache()));
    }

    [Fact]
    [Trait("Luat", "TK-17")]
    public void KhoaTaiKhoan_LanKiemTraSauBiTuChoi()
    {
        var admin = TaoTaiKhoan(TenTest(), "abc123", Constants.VaiTro.QuanLy);
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var tk = TaoTaiKhoan(TenTest(), "abc123", Constants.VaiTro.GiaSu, gs.MaGS);
        var cache = Cache();
        var user = Principal(tk.MaTK, tk.VaiTro);
        Assert.True(KiemTraPhienDangNhap.HopLe(user, Db, cache));   // đã vào cache

        // Quản lý khóa qua service → cache của tài khoản đó bị xóa ngay
        new TaiKhoanService(Db, QuanLy(admin.MaTK), cache).DoiTrangThai(tk.MaTK);

        Assert.False(KiemTraPhienDangNhap.HopLe(user, Db, cache));
    }

    [Fact]
    [Trait("Luat", "TK-08")]
    public void KhoaTrucTiepTrongDb_ChoToiKhiHetCache_RoiBiTuChoi()
    {
        var tk = TaoTaiKhoan(TenTest(), "abc123", Constants.VaiTro.QuanLy);
        var user = Principal(tk.MaTK, tk.VaiTro);
        var cache = Cache();
        Assert.True(KiemTraPhienDangNhap.HopLe(user, Db, cache));

        tk.HoatDong = false;
        Db.SaveChanges();
        Assert.True(KiemTraPhienDangNhap.HopLe(user, Db, cache));   // còn cache ~30 giây

        KiemTraPhienDangNhap.XoaCache(cache, tk.MaTK);              // hết cache
        Assert.False(KiemTraPhienDangNhap.HopLe(user, Db, cache));
    }

    [Fact]
    [Trait("Luat", "TK-08")]
    public void VaiTroKhacClaim_BiTuChoi()
    {
        var tk = TaoTaiKhoan(TenTest(), "abc123", Constants.VaiTro.GiaSu, TaoGiaSu(TenTest(), SdtTest()).MaGS);
        Assert.False(KiemTraPhienDangNhap.HopLe(Principal(tk.MaTK, Constants.VaiTro.QuanLy), Db, Cache()));
    }

    [Fact]
    [Trait("Luat", "TK-08")]
    public void TaiKhoanKhongCon_HoacClaimThieu_BiTuChoi()
    {
        Assert.False(KiemTraPhienDangNhap.HopLe(Principal(2_000_000_000, Constants.VaiTro.QuanLy), Db, Cache()));
        Assert.False(KiemTraPhienDangNhap.HopLe(new ClaimsPrincipal(new ClaimsIdentity()), Db, Cache()));
    }

    [Fact]
    [Trait("Luat", "TK-16")]
    public void DatLaiMatKhau_XoaCache()
    {
        var admin = TaoTaiKhoan(TenTest(), "abc123", Constants.VaiTro.QuanLy);
        var tk = TaoTaiKhoan(TenTest(), "abc123", Constants.VaiTro.QuanLy);
        var cache = Cache();
        var user = Principal(tk.MaTK, tk.VaiTro);
        Assert.True(KiemTraPhienDangNhap.HopLe(user, Db, cache));

        tk.HoatDong = false;
        Db.SaveChanges();
        new TaiKhoanService(Db, QuanLy(admin.MaTK), cache).DatLaiMatKhau(tk.MaTK, "matkhau-moi");

        Assert.False(KiemTraPhienDangNhap.HopLe(user, Db, cache));
    }
}
