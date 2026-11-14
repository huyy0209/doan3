using System;
using System.Collections.Generic;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm thử tiến trình cấp độ (HT-07) và xếp loại (HT-08) – không cần CSDL.
/// </summary>
public class TienTrinhCapDoTests
{
    private static readonly List<string> CacCap = new() { "Vỡ lòng", "Căn bản", "Sơ cấp", "Trung cấp", "Nâng cao" };

    private static BuoiHoc Thi(string cap, decimal? diem, DateTime ngay) =>
        new BuoiHoc { ThiLenCap = true, CapDoThi = cap, Diem = diem, NgayHoc = ngay };

    // ============ HT-07: cấp hiện tại = cao nhất trong (cấp đầu vào, các cấp thi đạt ≥ 8) ============

    [Fact]
    [Trait("Luat", "HT-07")]
    public void Tinh_KhongCoBaiThi_LayCapDauVao()
    {
        var tt = TienTrinhCapDo.Tinh(CacCap, "Căn bản", Array.Empty<BuoiHoc>());
        Assert.Equal("Căn bản", tt.CapHienTai);
        Assert.Equal("Sơ cấp", tt.CapKeTiep);
    }

    [Fact]
    [Trait("Luat", "HT-07")]
    public void Tinh_ThiDat_LenCap()
    {
        var tt = TienTrinhCapDo.Tinh(CacCap, "Căn bản",
            new[] { Thi("Sơ cấp", 8.5m, new DateTime(2026, 1, 1)) });
        Assert.Equal("Sơ cấp", tt.CapHienTai);
    }

    [Fact]
    [Trait("Luat", "HT-07")]
    public void Tinh_ThiKhongDat_KhongLenCap()
    {
        var tt = TienTrinhCapDo.Tinh(CacCap, "Căn bản",
            new[] { Thi("Sơ cấp", 7m, new DateTime(2026, 1, 1)) });
        Assert.Equal("Căn bản", tt.CapHienTai);
    }

    [Fact]
    [Trait("Luat", "HT-07")]
    public void Tinh_CaoNhatTrongCacCapThiDat()
    {
        var tt = TienTrinhCapDo.Tinh(CacCap, "Vỡ lòng",
            new[]
            {
                Thi("Căn bản", 8m, new DateTime(2026, 1, 1)),
                Thi("Sơ cấp", 9m, new DateTime(2026, 2, 1)),
                Thi("Trung cấp", 8.5m, new DateTime(2026, 3, 1))
            });
        Assert.Equal("Trung cấp", tt.CapHienTai);
        Assert.Equal("Nâng cao", tt.CapKeTiep);
    }

    [Fact]
    [Trait("Luat", "HT-07")]
    public void Tinh_DatCapCaoNhat_KhongConCapKeTiep()
    {
        var tt = TienTrinhCapDo.Tinh(CacCap, "Nâng cao", Array.Empty<BuoiHoc>());
        Assert.Equal("Nâng cao", tt.CapHienTai);
        Assert.Null(tt.CapKeTiep);
    }

    [Fact]
    [Trait("Luat", "HT-07")]
    public void Tinh_KhongTutCap_DuoiCapDauVao()
    {
        // Cấp đầu vào "Sơ cấp", dù không thi đạt cũng không tụt xuống "Căn bản".
        var tt = TienTrinhCapDo.Tinh(CacCap, "Sơ cấp", Array.Empty<BuoiHoc>());
        Assert.Equal("Sơ cấp", tt.CapHienTai);
    }

    [Fact]
    [Trait("Luat", "HT-07")]
    public void Tinh_DanhSachCapRong_KhongCoCapHienTai()
    {
        // Không có danh sách cấp → không tính được cấp hiện tại (trả null).
        var tt = TienTrinhCapDo.Tinh(new List<string>(), "X", Array.Empty<BuoiHoc>());
        Assert.Equal("X", tt.CapDauVao);
        Assert.Null(tt.CapHienTai);
        Assert.Null(tt.CapKeTiep);
    }

    // ============ HT-08: xếp loại theo điểm TB ============

    [Theory]
    [Trait("Luat", "HT-08")]
    [InlineData(8.0, "Giỏi")]
    [InlineData(9.5, "Giỏi")]
    [InlineData(6.5, "Khá")]
    [InlineData(7.9, "Khá")]
    [InlineData(5.0, "Trung bình")]
    [InlineData(6.4, "Trung bình")]
    [InlineData(4.9, "Yếu")]
    [InlineData(0.0, "Yếu")]
    public void XepLoai_TheoDiem(double diem, string loai)
    {
        Assert.Equal(loai, DanhGia.XepLoai((decimal)diem));
    }

    [Fact]
    [Trait("Luat", "HT-08")]
    public void XepLoai_KhongCoDiem_TraVeRong()
    {
        Assert.Equal("", DanhGia.XepLoai(null));
    }
}
