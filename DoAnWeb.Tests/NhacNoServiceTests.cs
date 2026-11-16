using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// G6: danh sách nhắc nợ dùng đúng công thức công nợ của trang Học phí
/// (<see cref="HocPhiService.GetCongNo"/>), lọc theo mức nợ/số ngày/môn và chỉ quản lý được xem.
/// </summary>
[Collection("Database")]
public class NhacNoServiceTests : DbTestBase
{
    private HocPhiService HocPhi() => new(Db, QuanLy());

    private NhacNoService TaoService() => new(Db, HocPhi(), QuanLy());

    [Fact]
    [Trait("Luat", "NO-01")]
    public void SoLieuNo_KhopTrangHocPhi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv);
        HocPhi().ThemThanhToan(lop.MaLop, 25000, DateTime.Today.AddDays(-5), "Tiền mặt", "Đợt 1");

        var congNo = HocPhi().GetCongNo().ToDictionary(c => c.MaLop);
        var nhacNo = TaoService().GetDanhSach();

        var row = nhacNo.Single(c => c.MaLop == lop.MaLop);
        Assert.Equal(congNo[lop.MaLop].HocPhi, row.PhaiDong);
        Assert.Equal(congNo[lop.MaLop].DaDong, row.DaDong);
        Assert.Equal(congNo[lop.MaLop].ConNo, row.ConNo);
        Assert.Equal(75000, row.ConNo);
        Assert.Equal(DateTime.Today.AddDays(-5), row.LanDongGanNhat);
        Assert.Equal(5, row.SoNgayChuaDong);

        // Không có dòng nào lệch tổng nợ của trang Học phí (bỏ lớp đã hủy).
        var tongTrangHocPhi = HocPhi().GetCongNo()
            .Where(c => c.ConNo > 0 && c.TrangThaiLop != Constants.LopHocStatus.DaHuy)
            .Sum(c => c.ConNo);
        Assert.Equal(tongTrangHocPhi, nhacNo.Sum(c => c.ConNo));
        Assert.All(nhacNo, c => Assert.True(c.ConNo > 0));
    }

    [Fact]
    [Trait("Luat", "NO-02")]
    public void ChuaDongLanNao_TinhTuNgayBatDauLop()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv, batDau: DateTime.Today.AddDays(-40));

        var row = TaoService().GetDanhSach().Single(c => c.MaLop == lop.MaLop);

        Assert.True(row.ChuaDongLanNao);
        Assert.Null(row.LanDongGanNhat);
        Assert.Equal(0, row.SoLanDong);
        Assert.Equal(40, row.SoNgayChuaDong);
    }

    [Fact]
    [Trait("Luat", "NO-03")]
    public void GiaSuBiChan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var service = new NhacNoService(Db, new HocPhiService(Db, QuanLy()), GiaSu(999, gs.MaGS));

        Assert.Throws<UnauthorizedAccessException>(() => service.GetDanhSach());
    }

    [Fact]
    [Trait("Luat", "NO-04")]
    public void LocTheoMucNoSoNgayVaMon()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hvA = TaoHocVien(TenTest(), SdtTest());
        var hvB = TaoHocVien(TenTest(), SdtTest());
        string monA = TenTest();
        string monB = TenTest();
        var lopA = TaoLop(gs, hvA, monA);
        lopA.HocPhi = 300000;
        Db.SaveChanges();
        HocPhi().ThemThanhToan(lopA.MaLop, 100000, DateTime.Today.AddDays(-10), null, null);
        var lopB = TaoLop(gs, hvB, monB, batDau: DateTime.Today.AddDays(-40));

        var cuaToi = TaoService().GetDanhSach()
            .Where(c => c.MaLop == lopA.MaLop || c.MaLop == lopB.MaLop)
            .ToList();

        Assert.Equal(2, cuaToi.Count);
        Assert.Single(NhacNoService.Loc(cuaToi, null, null, monA));
        Assert.Equal(lopB.MaLop, NhacNoService.Loc(cuaToi, null, null, monB).Single().MaLop);
        Assert.Equal(lopA.MaLop, NhacNoService.Loc(cuaToi, 150000, null, null).Single().MaLop);
        Assert.Equal(lopB.MaLop, NhacNoService.Loc(cuaToi, null, 30, null).Single().MaLop);
        Assert.Equal(2, NhacNoService.Loc(cuaToi, 100000, 0, null).Count);
    }

    [Fact]
    [Trait("Luat", "NO-04")]
    public void SapXepTheoConNoVaSoNgay()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hvA = TaoHocVien(TenTest(), SdtTest());
        var hvB = TaoHocVien(TenTest(), SdtTest());
        var lopA = TaoLop(gs, hvA);
        lopA.HocPhi = 300000;
        Db.SaveChanges();
        HocPhi().ThemThanhToan(lopA.MaLop, 100000, DateTime.Today.AddDays(-10), null, null);
        var lopB = TaoLop(gs, hvB, batDau: DateTime.Today.AddDays(-40));

        var ds = TaoService().GetDanhSach()
            .Where(c => c.MaLop == lopA.MaLop || c.MaLop == lopB.MaLop)
            .ToList();

        Assert.Equal(lopA.MaLop, NhacNoService.SapXep(ds, NhacNoService.SapXepConNo, true).First().MaLop);
        Assert.Equal(lopA.MaLop, NhacNoService.SapXep(ds, NhacNoService.SapXepConNo, false).Last().MaLop);
        Assert.Equal(lopB.MaLop, NhacNoService.SapXep(ds, NhacNoService.SapXepSoNgay, true).First().MaLop);
        Assert.Equal(lopB.MaLop, NhacNoService.SapXep(ds, NhacNoService.SapXepSoNgay, false).Last().MaLop);
    }
}
