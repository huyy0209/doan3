using System;
using DoAnWeb.Common;
using DoAnWeb.Services.Validators;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm thử validator ghép lớp (LH-02, LH-03) – không cần CSDL.
/// </summary>
public class LopHocValidatorTests
{
    // ============ LH-02: bắt buộc HV, GS, môn (≤50), học phí >0 và ≤100.000.000 ============

    [Fact]
    [Trait("Luat", "LH-02")]
    public void Validate_HopLe_True()
    {
        Assert.True(LopHocValidator.Validate(1, 2, "Toán", 1000000m, out _));
        Assert.True(LopHocValidator.Validate(1, 2, new string('x', 50), 100000000m, out _)); // đúng giới hạn trên
    }

    [Theory]
    [Trait("Luat", "LH-02")]
    [InlineData(null, 2, "Toán", 1000)]
    [InlineData(0, 2, "Toán", 1000)]
    [InlineData(1, null, "Toán", 1000)]
    [InlineData(1, 0, "Toán", 1000)]
    public void Validate_ThieuHVHoacGS_False(int? maHV, int? maGS, string mon, decimal hocPhi)
    {
        Assert.False(LopHocValidator.Validate(maHV, maGS, mon, hocPhi, out _));
    }

    [Theory]
    [Trait("Luat", "LH-02")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ThieuMon_False(string mon)
    {
        Assert.False(LopHocValidator.Validate(1, 2, mon, 1000m, out _));
    }

    [Fact]
    [Trait("Luat", "LH-02")]
    public void Validate_MonQuaDai_False()
    {
        Assert.False(LopHocValidator.Validate(1, 2, new string('x', 51), 1000m, out _));
    }

    [Fact]
    [Trait("Luat", "LH-02")]
    public void Validate_HocPhiNull_False()
    {
        Assert.False(LopHocValidator.Validate(1, 2, "Toán", null, out _));
    }

    [Theory]
    [Trait("Luat", "LH-02")]
    [InlineData(0.0)]
    [InlineData(-1000.0)]
    public void Validate_HocPhiKhongHopLe_False(double hocPhi)
    {
        Assert.False(LopHocValidator.Validate(1, 2, "Toán", (decimal)hocPhi, out _));
    }

    [Fact]
    [Trait("Luat", "LH-02")]
    public void Validate_HocPhiVuotMucToiDa_False()
    {
        Assert.False(LopHocValidator.Validate(1, 2, "Toán", 100000001m, out _));
    }

    // ============ LH-03: ca ∈ 6 ca, bắt đầu < kết thúc, không bắt đầu trong quá khứ ============

    [Fact]
    [Trait("Luat", "LH-03")]
    public void ValidateLich_HopLe_True()
    {
        var bd = DateTime.Now.Date.AddDays(1);
        var kt = bd.AddMonths(3);
        Assert.True(LopHocValidator.ValidateLich(bd, kt, Constants.CaHoc.Toi, out _));
    }

    [Theory]
    [Trait("Luat", "LH-03")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Buổi trưa")] // không thuộc 6 ca
    public void ValidateLich_CaKhongHopLe_False(string ca)
    {
        var bd = DateTime.Now.Date.AddDays(1);
        Assert.False(LopHocValidator.ValidateLich(bd, bd.AddMonths(1), ca, out _));
    }

    [Fact]
    [Trait("Luat", "LH-03")]
    public void ValidateLich_BatDauSauKetThuc_False()
    {
        var bd = DateTime.Now.Date.AddDays(10);
        Assert.False(LopHocValidator.ValidateLich(bd, bd.AddDays(-1), Constants.CaHoc.Toi, out _));
    }

    [Fact]
    [Trait("Luat", "LH-03")]
    public void ValidateLich_BatDauTrongQuaKhu_False()
    {
        var bd = DateTime.Now.Date.AddDays(-1);
        Assert.False(LopHocValidator.ValidateLich(bd, bd.AddMonths(3), Constants.CaHoc.Toi, out _));
    }
}
