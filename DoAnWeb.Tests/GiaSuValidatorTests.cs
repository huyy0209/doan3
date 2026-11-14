using System.Collections.Generic;
using DoAnWeb.Common;
using DoAnWeb.Services.Validators;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm thử validator hồ sơ gia sư (GS-01..GS-04) – không cần CSDL.
/// </summary>
public class GiaSuValidatorTests
{
    // ============ GS-01: họ tên bắt buộc, 2–100 ký tự sau trim, không chứa chữ số ============

    [Fact]
    [Trait("Luat", "GS-01")]
    public void ValidateHoTen_HopLe_True()
    {
        Assert.True(GiaSuValidator.ValidateHoTen("Nguyễn Văn A", out _));
        Assert.True(GiaSuValidator.ValidateHoTen("  Trần Thị B  ", out _)); // sau trim
        Assert.True(GiaSuValidator.ValidateHoTen("Ab", out _)); // đúng 2 ký tự
    }

    [Theory]
    [Trait("Luat", "GS-01")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateHoTen_Rong_False(string hoTen)
    {
        Assert.False(GiaSuValidator.ValidateHoTen(hoTen, out _));
    }

    [Theory]
    [Trait("Luat", "GS-01")]
    [InlineData("A")]                       // 1 ký tự < 2
    [InlineData("Nguyễn Văn 1")]            // chứa chữ số
    [InlineData("123")]
    public void ValidateHoTen_QuaNganHoacChuaSo_False(string hoTen)
    {
        Assert.False(GiaSuValidator.ValidateHoTen(hoTen, out _));
    }

    [Fact]
    [Trait("Luat", "GS-01")]
    public void ValidateHoTen_QuaDai_False()
    {
        Assert.False(GiaSuValidator.ValidateHoTen(new string('a', 101), out _));
    }

    // ============ GS-02: SĐT bắt buộc, regex ^0\d{9,10}$ ============

    [Theory]
    [Trait("Luat", "GS-02")]
    [InlineData("0901234567")]   // 10 số
    [InlineData("09012345678")]  // 11 số
    [InlineData("0123456789")]
    public void ValidateSDT_HopLe_True(string sdt)
    {
        Assert.True(GiaSuValidator.ValidateSDT(sdt, out _));
    }

    [Theory]
    [Trait("Luat", "GS-02")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateSDT_Rong_False(string sdt)
    {
        Assert.False(GiaSuValidator.ValidateSDT(sdt, out _));
    }

    [Theory]
    [Trait("Luat", "GS-02")]
    [InlineData("901234567")]      // không bắt đầu bằng 0
    [InlineData("09012345")]       // quá ngắn (8 số)
    [InlineData("090123456789")]   // quá dài (12 số)
    [InlineData("09012abc67")]     // chứa chữ
    [InlineData("1901234567")]     // bắt đầu bằng 1
    public void ValidateSDT_SaiDinhDang_False(string sdt)
    {
        Assert.False(GiaSuValidator.ValidateSDT(sdt, out _));
    }

    // ============ GS-03: chuyên môn bắt buộc, ≤100 ký tự ============

    [Fact]
    [Trait("Luat", "GS-03")]
    public void ValidateChuyenMon_HopLe_True()
    {
        Assert.True(GiaSuValidator.ValidateChuyenMon("Toán, Lý", out _));
        Assert.True(GiaSuValidator.ValidateChuyenMon(new string('x', 100), out _));
    }

    [Theory]
    [Trait("Luat", "GS-03")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateChuyenMon_Rong_False(string chuyenMon)
    {
        Assert.False(GiaSuValidator.ValidateChuyenMon(chuyenMon, out _));
    }

    [Fact]
    [Trait("Luat", "GS-03")]
    public void ValidateChuyenMon_QuaDai_False()
    {
        Assert.False(GiaSuValidator.ValidateChuyenMon(new string('x', 101), out _));
    }

    // ============ GS-04: email, kinh nghiệm, học vấn, ghi chú, ca nhận dạy ============

    [Fact]
    [Trait("Luat", "GS-04")]
    public void KiemTraHoSo_HopLe_Null()
    {
        var hs = new HoSoGiaSu
        {
            HoTen = "Nguyễn Văn A",
            SDT = "0901234567",
            ChuyenMon = "Toán",
            Email = "giasu@example.com",
            KinhNghiem = 5,
            HocVan = "Đại học",
            GhiChu = "kinh nghiệm lâu năm",
            CaNhanDay = new List<string> { "Sáng", "Tối" }
        };
        Assert.Null(GiaSuValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "GS-04")]
    public void KiemTraHoSo_EmailKhongHopLe_Loi()
    {
        var hs = HoSoHopLe();
        hs.Email = "khong-phai-email";
        Assert.NotNull(GiaSuValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "GS-04")]
    public void KiemTraHoSo_EmailQuaDai_Loi()
    {
        var hs = HoSoHopLe();
        hs.Email = new string('a', 101) + "@x.y";
        Assert.NotNull(GiaSuValidator.KiemTraHoSo(hs));
    }

    [Theory]
    [Trait("Luat", "GS-04")]
    [InlineData(-1)]
    [InlineData(61)]
    public void KiemTraHoSo_KinhNghiemNgoaiKhoang_Loi(int nam)
    {
        var hs = HoSoHopLe();
        hs.KinhNghiem = nam;
        Assert.NotNull(GiaSuValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "GS-04")]
    public void KiemTraHoSo_HocVanQuaDai_Loi()
    {
        var hs = HoSoHopLe();
        hs.HocVan = new string('x', 151);
        Assert.NotNull(GiaSuValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "GS-04")]
    public void KiemTraHoSo_GhiChuQuaDai_Loi()
    {
        var hs = HoSoHopLe();
        hs.GhiChu = new string('x', 501);
        Assert.NotNull(GiaSuValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "GS-04")]
    public void KiemTraHoSo_CaNhanDayKhongHopLe_Loi()
    {
        var hs = HoSoHopLe();
        hs.CaNhanDay = new List<string> { "Buổi trưa" }; // không thuộc 6 ca
        Assert.NotNull(GiaSuValidator.KiemTraHoSo(hs));
    }

    [Fact]
    [Trait("Luat", "GS-04")]
    public void KiemTraHoSo_CaNhanDayDu6Ca_HopLe()
    {
        var hs = HoSoHopLe();
        hs.CaNhanDay = new List<string> { "Sáng", "Sáng muộn", "Chiều", "Chiều muộn", "Tối", "Tối muộn" };
        Assert.Null(GiaSuValidator.KiemTraHoSo(hs));
    }

    private static HoSoGiaSu HoSoHopLe() => new HoSoGiaSu
    {
        HoTen = "Nguyễn Văn A",
        SDT = "0901234567",
        ChuyenMon = "Toán"
    };
}
