using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm thử kiểm tra mật khẩu mới (TK-12) và tên đăng nhập (TK-13) – không cần CSDL.
/// </summary>
public class TaiKhoanValidatorTests
{
    // ============ TK-12: mật khẩu mới 6–100 ký tự ============

    [Fact]
    [Trait("Luat", "TK-12")]
    public void KiemTraMatKhauMoi_HopLe_TraVeNull()
    {
        Assert.Null(TaiKhoanService.KiemTraMatKhauMoi("abcdef"));
        Assert.Null(TaiKhoanService.KiemTraMatKhauMoi("MatKhau@123"));
        Assert.Null(TaiKhoanService.KiemTraMatKhauMoi(new string('x', 100))); // đúng 100
    }

    [Fact]
    [Trait("Luat", "TK-12")]
    public void KiemTraMatKhauMoi_RongHoacNull_Loi()
    {
        Assert.NotNull(TaiKhoanService.KiemTraMatKhauMoi(null));
        Assert.NotNull(TaiKhoanService.KiemTraMatKhauMoi(""));
    }

    [Theory]
    [Trait("Luat", "TK-12")]
    [InlineData("abcde")]      // 5 ký tự < 6
    [InlineData("1")]
    [InlineData("")]
    public void KiemTraMatKhauMoi_QuaNgan_Loi(string matKhau)
    {
        Assert.NotNull(TaiKhoanService.KiemTraMatKhauMoi(matKhau));
    }

    [Fact]
    [Trait("Luat", "TK-12")]
    public void KiemTraMatKhauMoi_QuaDai_Loi()
    {
        Assert.NotNull(TaiKhoanService.KiemTraMatKhauMoi(new string('x', 101)));
    }

    // ============ TK-13: tên đăng nhập 3–50 ký tự, [A-Za-z0-9_.] ============

    [Fact]
    [Trait("Luat", "TK-13")]
    public void KiemTraTenDangNhap_HopLe_TraVeNull()
    {
        Assert.Null(TaiKhoanService.KiemTraTenDangNhap("gs1"));
        Assert.Null(TaiKhoanService.KiemTraTenDangNhap("Admin_2026.abc"));
        Assert.Null(TaiKhoanService.KiemTraTenDangNhap(new string('a', 50))); // đúng 50
    }

    [Fact]
    [Trait("Luat", "TK-13")]
    public void KiemTraTenDangNhap_Rong_Loi()
    {
        Assert.NotNull(TaiKhoanService.KiemTraTenDangNhap(null));
        Assert.NotNull(TaiKhoanService.KiemTraTenDangNhap("   "));
    }

    [Theory]
    [Trait("Luat", "TK-13")]
    [InlineData("ab")]          // 2 ký tự < 3
    [InlineData("a")]
    public void KiemTraTenDangNhap_QuaNgan_Loi(string ten)
    {
        Assert.NotNull(TaiKhoanService.KiemTraTenDangNhap(ten));
    }

    [Fact]
    [Trait("Luat", "TK-13")]
    public void KiemTraTenDangNhap_QuaDai_Loi()
    {
        Assert.NotNull(TaiKhoanService.KiemTraTenDangNhap(new string('a', 51)));
    }

    [Theory]
    [Trait("Luat", "TK-13")]
    [InlineData("tên có dấu")]     // chữ có dấu
    [InlineData("gs 1")]           // khoảng trắng
    [InlineData("gs-1")]           // gạch ngang không hợp lệ
    [InlineData("gs@1")]           // ký tự đặc biệt
    public void KiemTraTenDangNhap_KyTuKhongHopLe_Loi(string ten)
    {
        Assert.NotNull(TaiKhoanService.KiemTraTenDangNhap(ten));
    }
}
