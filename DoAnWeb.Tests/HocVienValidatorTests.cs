using DoAnWeb.Common;
using DoAnWeb.Services.Validators;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm thử validator hồ sơ học viên (HV-01..HV-03) – không cần CSDL.
/// </summary>
public class HocVienValidatorTests
{
    // ============ HV-01: họ tên như GS-01 ============

    [Fact]
    [Trait("Luat", "HV-01")]
    public void ValidateHoTen_HopLe_True()
    {
        Assert.True(HocVienValidator.ValidateHoTen("Lê Thị C", out _));
        Assert.True(HocVienValidator.ValidateHoTen("  Phạm Văn D  ", out _));
    }

    [Theory]
    [Trait("Luat", "HV-01")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("Lê Thị 3")]
    public void ValidateHoTen_KhongHopLe_False(string hoTen)
    {
        Assert.False(HocVienValidator.ValidateHoTen(hoTen, out _));
    }

    // ============ HV-02: cần ít nhất 1 SĐT (học viên hoặc phụ huynh), SĐT nào nhập thì đúng regex ============

    [Fact]
    [Trait("Luat", "HV-02")]
    public void KiemTraHoSo_ChiCoSDTHocVien_HopLe()
    {
        var hs = new HoSoHocVien { HoTen = "Lê Thị C", SDT = "0901234567" };
        Assert.Null(HocVienValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "HV-02")]
    public void KiemTraHoSo_ChiCoSDTPhuHuynh_HopLe()
    {
        var hs = new HoSoHocVien { HoTen = "Lê Thị C", SDTPhuHuynh = "0912345678" };
        Assert.Null(HocVienValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "HV-02")]
    public void KiemTraHoSo_KhongCoSDTNaoo_Loi()
    {
        var hs = new HoSoHocVien { HoTen = "Lê Thị C" };
        Assert.NotNull(HocVienValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "HV-02")]
    public void KiemTraHoSo_SDTHocVienSai_Loi()
    {
        var hs = new HoSoHocVien { HoTen = "Lê Thị C", SDT = "12345", SDTPhuHuynh = "0912345678" };
        Assert.NotNull(HocVienValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "HV-02")]
    public void KiemTraHoSo_SDTPhuHuynhSai_Loi()
    {
        var hs = new HoSoHocVien { HoTen = "Lê Thị C", SDT = "0901234567", SDTPhuHuynh = "abc" };
        Assert.NotNull(HocVienValidator.KiemTraHoSo(hs));
    }

    // ============ HV-03: độ dài các trường ============

    [Fact]
    [Trait("Luat", "HV-03")]
    public void KiemTraHoSo_DiaChiQuaDai_Loi()
    {
        var hs = HoSoHopLe();
        hs.DiaChi = new string('x', 201);
        Assert.NotNull(HocVienValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "HV-03")]
    public void KiemTraHoSo_TenPhuHuynhQuaDai_Loi()
    {
        var hs = HoSoHopLe();
        hs.TenPhuHuynh = new string('x', 101);
        Assert.NotNull(HocVienValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "HV-03")]
    public void KiemTraHoSo_TruongLopQuaDai_Loi()
    {
        var hs = HoSoHopLe();
        hs.TruongLop = new string('x', 151);
        Assert.NotNull(HocVienValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "HV-03")]
    public void KiemTraHoSo_TrinhDoQuaDai_Loi()
    {
        var hs = HoSoHopLe();
        hs.TrinhDo = new string('x', 51);
        Assert.NotNull(HocVienValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "HV-03")]
    public void KiemTraHoSo_GhiChuQuaDai_Loi()
    {
        var hs = HoSoHopLe();
        hs.GhiChu = new string('x', 501);
        Assert.NotNull(HocVienValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "HV-03")]
    public void KiemTraHoSo_DayDu_HopLe()
    {
        var hs = new HoSoHocVien
        {
            HoTen = "Lê Thị C",
            SDT = "0901234567",
            DiaChi = new string('x', 200),
            TenPhuHuynh = new string('x', 100),
            TruongLop = new string('x', 150),
            TrinhDo = new string('x', 50),
            GhiChu = new string('x', 500)
        };
        Assert.Null(HocVienValidator.KiemTraHoSo(hs));
    }

    private static HoSoHocVien HoSoHopLe() => new HoSoHocVien { HoTen = "Lê Thị C", SDT = "0901234567" };
}
