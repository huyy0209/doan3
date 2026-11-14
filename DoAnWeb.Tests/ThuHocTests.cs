using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm thử Constants.ThuHoc – quy ước thứ học "2,4,6" (LH-08). Không cần CSDL.
/// </summary>
public class ThuHocTests
{
    // ============ LH-08: thứ học lưu "2,4,6" (2=T2…8=CN); chọn 0 hoặc đủ 7 → null = hằng ngày ============

    [Theory]
    [Trait("Luat", "LH-08")]
    [InlineData(DayOfWeek.Monday, 2)]
    [InlineData(DayOfWeek.Tuesday, 3)]
    [InlineData(DayOfWeek.Wednesday, 4)]
    [InlineData(DayOfWeek.Thursday, 5)]
    [InlineData(DayOfWeek.Friday, 6)]
    [InlineData(DayOfWeek.Saturday, 7)]
    [InlineData(DayOfWeek.Sunday, 8)]
    public void Cua_DungSoTheoThu(DayOfWeek thu, int maSo)
    {
        // Lấy một ngày rơi đúng vào thứ này.
        var ngay = new DateTime(2026, 11, 9); // thứ Hai
        while (ngay.DayOfWeek != thu) ngay = ngay.AddDays(1);
        Assert.Equal(maSo, Constants.ThuHoc.Cua(ngay));
    }

    [Fact]
    [Trait("Luat", "LH-08")]
    public void Doc_PhanTachChuoiThang()
    {
        Assert.Equal(new HashSet<int> { 2, 4, 6 }, Constants.ThuHoc.Doc("2,4,6"));
        Assert.Equal(new HashSet<int> { 2, 4, 6 }, Constants.ThuHoc.Doc(" 2 , 4, 6 "));
    }

    [Fact]
    [Trait("Luat", "LH-08")]
    public void Doc_RongHoacTrang_ThanhTapRong()
    {
        Assert.Empty(Constants.ThuHoc.Doc(null));
        Assert.Empty(Constants.ThuHoc.Doc(""));
        Assert.Empty(Constants.ThuHoc.Doc("   "));
    }

    [Fact]
    [Trait("Luat", "LH-08")]
    public void Doc_Du7Thu_ThanhTapRong_HangNgay()
    {
        Assert.Empty(Constants.ThuHoc.Doc("2,3,4,5,6,7,8"));
    }

    [Fact]
    [Trait("Luat", "LH-08")]
    public void Doc_BoGiaTriNgoaiKhoang()
    {
        Assert.Equal(new HashSet<int> { 2, 6 }, Constants.ThuHoc.Doc("1,2,6,9"));
    }

    [Fact]
    [Trait("Luat", "LH-08")]
    public void Ghi_KhongChonHoacDu7_Null()
    {
        Assert.Null(Constants.ThuHoc.Ghi(new int[0]));
        Assert.Null(Constants.ThuHoc.Ghi(null));
        Assert.Null(Constants.ThuHoc.Ghi(new[] { 2, 3, 4, 5, 6, 7, 8 }));
    }

    [Fact]
    [Trait("Luat", "LH-08")]
    public void Ghi_SapXepVaDedup()
    {
        Assert.Equal("2,4,6", Constants.ThuHoc.Ghi(new[] { 6, 2, 4, 2 }));
    }

    [Fact]
    [Trait("Luat", "LH-08")]
    public void Ghi_BoGiaTriNgoaiKhoang()
    {
        Assert.Equal("2,6", Constants.ThuHoc.Ghi(new[] { 1, 2, 6, 9 }));
    }

    [Fact]
    [Trait("Luat", "LH-08")]
    public void CoHoc_DungThu()
    {
        var thuHai = new DateTime(2026, 11, 9);  // thứ Hai = 2
        var thuBa = new DateTime(2026, 11, 10);  // thứ Ba = 3

        Assert.True(Constants.ThuHoc.CoHoc("2,4", thuHai));
        Assert.False(Constants.ThuHoc.CoHoc("2,4", thuBa));
        Assert.True(Constants.ThuHoc.CoHoc(null, thuBa));      // hằng ngày
        Assert.True(Constants.ThuHoc.CoHoc("", thuBa));        // hằng ngày
    }

    [Fact]
    [Trait("Luat", "LH-08")]
    public void MoTa_ChuoiHienThi()
    {
        Assert.Equal("T2-T4-T6", Constants.ThuHoc.MoTa("2,4,6"));
        Assert.Equal("Hằng ngày", Constants.ThuHoc.MoTa(null));
    }
}
