using DoAnWeb.Infrastructure;
using Xunit;

namespace DoAnWeb.Tests;

public class PhanTrangTests
{
    [Fact]
    public void Tao_TrangKhongHopLe_ClampsVaoTrangDau()
    {
        var ketQua = PhanTrang.Tao(Enumerable.Range(1, 45), 0, 20);

        Assert.Equal(1, ketQua.Trang);
        Assert.Equal(3, ketQua.SoTrang);
        Assert.Equal(1, ketQua.TuDong);
        Assert.Equal(20, ketQua.DenDong);
        Assert.Equal(Enumerable.Range(1, 20), ketQua.Items);
    }

    [Fact]
    public void Tao_TrangVuotQua_ClampsVaoTrangCuoi()
    {
        var ketQua = PhanTrang.Tao(Enumerable.Range(1, 45), 99, 20);

        Assert.Equal(3, ketQua.Trang);
        Assert.Equal(41, ketQua.TuDong);
        Assert.Equal(45, ketQua.DenDong);
        Assert.Equal(Enumerable.Range(41, 5), ketQua.Items);
    }

    [Fact]
    public void Tao_DanhSachRong_TraTrangTrongVaKhoangRong()
    {
        var ketQua = PhanTrang.Tao(Array.Empty<int>(), 3, 50);

        Assert.Equal(1, ketQua.Trang);
        Assert.Equal(1, ketQua.SoTrang);
        Assert.Equal(0, ketQua.TongSo);
        Assert.Equal(0, ketQua.TuDong);
        Assert.Equal(0, ketQua.DenDong);
        Assert.Empty(ketQua.Items);
    }
}
