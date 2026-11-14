using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using Xunit;

namespace DoAnWeb.Tests;

[Collection("Database")]
public class HocTapServiceTests : DbTestBase
{
    [Fact]
    [Trait("Luat", "HT-05")]
    public void DiemDanh_BuoiVangDaXepBu_KhongDoiThanhCoMat()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var batDau = DateTime.Today.AddDays(-5);
        var lop = TaoLop(gs, hv, batDau: batDau, ketThuc: DateTime.Today.AddDays(20));
        var goc = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today.AddDays(-1),
            DiemDanh = Constants.DiemDanh.VangCoPhep
        };
        Db.BuoiHocs.Add(goc);
        Db.SaveChanges();
        var service = new HocTapService(Db, QuanLy());
        var ketQuaBu = service.XepHocBu(goc.MaBuoi, DateTime.Today.AddDays(2), Constants.CaHoc.Chieu, gs.MaGS);
        Assert.True(ketQuaBu.ThanhCong);

        var loi = service.LuuCacBuoi(new[]
        {
            new BuoiHocRow
            {
                MaBuoi = goc.MaBuoi,
                MaLop = lop.MaLop,
                NgayHoc = goc.NgayHoc,
                DiemDanh = Constants.DiemDanh.CoMat
            }
        });

        Assert.Single(loi);
        Assert.Contains("đã xếp học bù", loi[0]);
        Db.Entry(goc).Reload();
        Assert.Equal(Constants.DiemDanh.VangCoPhep, goc.DiemDanh);
    }

    [Fact]
    [Trait("Luat", "HT-06")]
    public void DiemDanh_ThiLenCapMonNangKhieu_LuuCapKeTiep()
    {
        var mon = TaoMon(TenTest(), Constants.LoaiMon.NangKhieu, "A;B;C");
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv, mon.TenMon, batDau: DateTime.Today.AddDays(-10), ketThuc: DateTime.Today.AddDays(10));
        lop.CapDoDauVao = "A";
        Db.SaveChanges();
        var buoi = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today.AddDays(-1),
            DiemDanh = Constants.DiemDanh.CoMat
        };
        Db.BuoiHocs.Add(buoi);
        Db.SaveChanges();

        var loi = new HocTapService(Db, QuanLy()).LuuCacBuoi(new[]
        {
            new BuoiHocRow
            {
                MaBuoi = buoi.MaBuoi,
                MaLop = lop.MaLop,
                NgayHoc = buoi.NgayHoc,
                DiemDanh = Constants.DiemDanh.CoMat,
                Diem = 9,
                ThiLenCap = true
            }
        });

        Assert.Empty(loi);
        Db.Entry(buoi).Reload();
        Assert.True(buoi.ThiLenCap);
        Assert.Equal("B", buoi.CapDoThi);
    }

    [Fact]
    [Trait("Luat", "HT-06")]
    public void DiemDanh_ThiLenCapMonChinh_BaoLoi()
    {
        var mon = TaoMon(TenTest());
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv, mon.TenMon, batDau: DateTime.Today.AddDays(-10), ketThuc: DateTime.Today.AddDays(10));
        var buoi = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today.AddDays(-1),
            DiemDanh = Constants.DiemDanh.CoMat
        };
        Db.BuoiHocs.Add(buoi);
        Db.SaveChanges();

        var loi = new HocTapService(Db, QuanLy()).LuuCacBuoi(new[]
        {
            new BuoiHocRow
            {
                MaBuoi = buoi.MaBuoi,
                MaLop = lop.MaLop,
                NgayHoc = buoi.NgayHoc,
                DiemDanh = Constants.DiemDanh.CoMat,
                Diem = 9,
                ThiLenCap = true
            }
        });

        Assert.Single(loi);
        Assert.Contains("chỉ môn năng khiếu", loi[0]);
    }

    [Fact]
    [Trait("Luat", "HT-10")]
    public void XepHocBu_BuoiVangHopLe_TaoBuoiBu()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv, batDau: DateTime.Today.AddDays(-5), ketThuc: DateTime.Today.AddDays(20));
        var goc = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today.AddDays(-1),
            DiemDanh = Constants.DiemDanh.VangKhongPhep
        };
        Db.BuoiHocs.Add(goc);
        Db.SaveChanges();

        var ketQua = new HocTapService(Db, QuanLy()).XepHocBu(
            goc.MaBuoi, DateTime.Today.AddDays(2), Constants.CaHoc.Chieu, gs.MaGS);

        Assert.True(ketQua.ThanhCong);
        var bu = Db.BuoiHocs.Single(b => b.MaBuoiGoc == goc.MaBuoi);
        Assert.Equal(DateTime.Today.AddDays(2), bu.NgayHoc);
        Assert.Equal(Constants.CaHoc.Chieu, bu.CaHoc);
        Assert.Equal(gs.MaGS, bu.MaGSDay);
    }

    [Fact]
    [Trait("Luat", "HT-10")]
    public void XepHocBu_NgayQua60Ngay_BaoLoi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv, batDau: DateTime.Today.AddDays(-5), ketThuc: DateTime.Today.AddDays(90));
        var goc = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today.AddDays(-1),
            DiemDanh = Constants.DiemDanh.VangKhongPhep
        };
        Db.BuoiHocs.Add(goc);
        Db.SaveChanges();

        var ketQua = new HocTapService(Db, QuanLy()).XepHocBu(
            goc.MaBuoi, DateTime.Today.AddDays(61), Constants.CaHoc.Chieu, gs.MaGS);

        Assert.False(ketQua.ThanhCong);
        Assert.Contains("60 ngày", ketQua.Loi);
    }

    [Fact]
    [Trait("Luat", "HT-10")]
    public void XepHocBu_GiaSuBan_TraCanhBaoVaXepDuocSauKhiXacNhan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hvLop = TaoHocVien(TenTest(), SdtTest());
        var hvBan = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hvLop, batDau: DateTime.Today.AddDays(-5), ketThuc: DateTime.Today.AddDays(20));
        var buoiGoc = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today.AddDays(-1),
            DiemDanh = Constants.DiemDanh.VangKhongPhep
        };
        Db.BuoiHocs.Add(buoiGoc);
        TaoLop(gs, hvBan, batDau: DateTime.Today.AddDays(2), ketThuc: DateTime.Today.AddDays(8),
            caHoc: Constants.CaHoc.Chieu);
        Db.SaveChanges();

        var service = new HocTapService(Db, QuanLy());
        var canhBao = service.XepHocBu(
            buoiGoc.MaBuoi, DateTime.Today.AddDays(3), Constants.CaHoc.Chieu, gs.MaGS);

        Assert.False(canhBao.ThanhCong);
        Assert.Contains("đang có lớp", canhBao.CanhBao);
        var xacNhan = service.XepHocBu(
            buoiGoc.MaBuoi, DateTime.Today.AddDays(3), Constants.CaHoc.Chieu, gs.MaGS, boQuaCanhBao: true);
        Assert.True(xacNhan.ThanhCong);
    }

    [Fact]
    [Trait("Luat", "HT-10")]
    public void XepHocBu_HocVienDaBanCungCa_BaoLoi()
    {
        var gsLop = TaoGiaSu(TenTest(), SdtTest());
        var gsBu = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gsLop, hv, batDau: DateTime.Today.AddDays(-5), ketThuc: DateTime.Today.AddDays(20));
        var buoiGoc = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today.AddDays(-1),
            DiemDanh = Constants.DiemDanh.VangKhongPhep
        };
        Db.BuoiHocs.Add(buoiGoc);
        TaoLop(gsBu, hv, batDau: DateTime.Today.AddDays(2), ketThuc: DateTime.Today.AddDays(8),
            caHoc: Constants.CaHoc.Chieu);
        Db.SaveChanges();

        var ketQua = new HocTapService(Db, QuanLy()).XepHocBu(
            buoiGoc.MaBuoi, DateTime.Today.AddDays(3), Constants.CaHoc.Chieu, gsBu.MaGS);

        Assert.False(ketQua.ThanhCong);
        Assert.Contains("Học viên đã có", ketQua.Loi);
    }
}
