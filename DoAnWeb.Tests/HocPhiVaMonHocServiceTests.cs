using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

[Collection("Database")]
public class HocPhiVaMonHocServiceTests : DbTestBase
{
    [Fact]
    [Trait("Luat", "HP-03")]
    public void ThemThanhToan_SoTienHopLe_LuuVaCapNhatDuNo()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv);

        var thanhToan = new HocPhiService(Db, QuanLy()).ThemThanhToan(
            lop.MaLop, 25000, DateTime.Today, "  Tiền mặt  ", "  Đợt 1  ");

        Assert.Equal(25000, thanhToan.SoTien);
        Assert.Equal("Tiền mặt", thanhToan.HinhThuc);
        Assert.Equal("Đợt 1", thanhToan.GhiChu);
        Assert.Equal(25000, new HocPhiService(Db, QuanLy()).GetCongNo().Single(c => c.MaLop == lop.MaLop).DaDong);
    }

    [Theory]
    [Trait("Luat", "HP-03")]
    [InlineData(0)]
    [InlineData(-1)]
    public void ThemThanhToan_SoTienKhongDuong_BaoLoi(decimal soTien)
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv);

        Assert.Throws<ArgumentException>(() =>
            new HocPhiService(Db, QuanLy()).ThemThanhToan(lop.MaLop, soTien, DateTime.Today, null, null));
    }

    [Fact]
    [Trait("Luat", "HP-03")]
    public void ThemThanhToan_VuotSoNo_BaoLoi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv);

        Assert.Throws<ArgumentException>(() =>
            new HocPhiService(Db, QuanLy()).ThemThanhToan(lop.MaLop, lop.HocPhi.Value + 1, DateTime.Today, null, null));
    }

    [Fact]
    [Trait("Luat", "HP-03")]
    public void ThemThanhToan_LopDaHuy_BaoLoi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv, trangThai: Constants.LopHocStatus.DaHuy);

        Assert.Throws<InvalidOperationException>(() =>
            new HocPhiService(Db, QuanLy()).ThemThanhToan(lop.MaLop, 1000, DateTime.Today, null, null));
    }

    [Fact]
    [Trait("Luat", "MH-01")]
    public void MonHoc_KiemTraNangKhieu_YeuCauCapDoHopLe()
    {
        Assert.NotNull(MonHocService.KiemTra(" ", Constants.LoaiMon.Chinh, null));
        Assert.NotNull(MonHocService.KiemTra("TEST_Mon", Constants.LoaiMon.NangKhieu, new List<string> { "A" }));
        Assert.NotNull(MonHocService.KiemTra("TEST_Mon", Constants.LoaiMon.NangKhieu, new List<string> { "A", " a " }));
        Assert.Null(MonHocService.KiemTra("TEST_Mon", Constants.LoaiMon.NangKhieu, new List<string> { "A", "B" }));
    }

    [Fact]
    [Trait("Luat", "MH-01")]
    public void MonHoc_LuuTenTrung_BaoLoi()
    {
        var mon = TaoMon(TenTest());

        Assert.Throws<ArgumentException>(() =>
            new MonHocService(Db, QuanLy()).Luu(null, mon.TenMon, Constants.LoaiMon.Chinh, null, false));
    }

    [Fact]
    [Trait("Luat", "MH-02")]
    public void MonHoc_DoiTen_DongBoLopVaChuyenMonGiaSu()
    {
        var mon = TaoMon(TenTest());
        var gs = TaoGiaSu(TenTest(), SdtTest(), chuyenMon: mon.TenMon);
        var hv = TaoHocVien(TenTest(), SdtTest());
        TaoLop(gs, hv, mon.TenMon, maMon: mon.MaMon);
        var tenMoi = TenTest();

        new MonHocService(Db, QuanLy()).Luu(mon.MaMon, tenMoi, Constants.LoaiMon.Chinh, null, false);

        Assert.Equal(tenMoi, gs.ChuyenMon);
        Assert.Equal(tenMoi, Db.LopHocs.Single(l => l.MaMon == mon.MaMon).MonHoc);
    }

    [Fact]
    [Trait("Luat", "MH-02")]
    public void MonHoc_KhongBoCapDangDuocLopDung()
    {
        var mon = TaoMon(TenTest(), Constants.LoaiMon.NangKhieu, "A;B;C");
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv, mon.TenMon, maMon: mon.MaMon);
        lop.CapDoDauVao = "B";
        Db.SaveChanges();

        Assert.Throws<InvalidOperationException>(() =>
            new MonHocService(Db, QuanLy()).Luu(
                mon.MaMon, mon.TenMon, Constants.LoaiMon.NangKhieu, new[] { "A", "C" }, false));
    }

    [Fact]
    [Trait("Luat", "MH-03")]
    public void MonHoc_XoaMonDangCoLop_BiChan()
    {
        var mon = TaoMon(TenTest());
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        TaoLop(gs, hv, mon.TenMon, maMon: mon.MaMon);

        Assert.Throws<InvalidOperationException>(() => new MonHocService(Db, QuanLy()).Xoa(mon.MaMon));
    }

    [Fact]
    [Trait("Luat", "MH-03")]
    public void MonHoc_XoaMonChuaCoLop_ThanhCong()
    {
        var mon = TaoMon(TenTest());

        new MonHocService(Db, QuanLy()).Xoa(mon.MaMon);

        Assert.Null(Db.MonHocs.Find(mon.MaMon));
    }
}
