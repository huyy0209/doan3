using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using Xunit;

namespace DoAnWeb.Tests;

[Collection("Database")]
public class LopHocSuaHuyXoaTests : DbTestBase
{
    private static SuaLopYeuCau YeuCauSua(LopHoc lop, DateTime ketThuc, decimal? hocPhi = null) =>
        new()
        {
            MaGS = lop.MaGS.Value,
            HocPhi = hocPhi ?? lop.HocPhi.Value,
            KetThuc = ketThuc,
            ThuHoc = lop.ThuHoc
        };

    [Fact]
    [Trait("Luat", "LH-19")]
    public void SuaLop_HocPhiNhoHonDaDong_BaoLoi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv, batDau: DateTime.Today.AddDays(2), ketThuc: DateTime.Today.AddDays(20));
        Db.ThanhToans.Add(new ThanhToan
        {
            MaLop = lop.MaLop,
            SoTien = 200000,
            NgayDong = DateTime.Today
        });
        Db.SaveChanges();

        Assert.Throws<ArgumentException>(() =>
            new LopHocService(Db, QuanLy()).SuaLop(lop.MaLop, YeuCauSua(lop, DateTime.Today.AddDays(30), 100000)));
    }

    [Fact]
    [Trait("Luat", "LH-19")]
    public void SuaLop_KetThucTruocBuoiDaDiemDanh_BaoLoi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var batDau = DateTime.Today.AddDays(-10);
        var lop = TaoLop(gs, hv, batDau: batDau, ketThuc: DateTime.Today.AddDays(20));
        Db.BuoiHocs.Add(new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today.AddDays(3),
            DiemDanh = Constants.DiemDanh.CoMat
        });
        Db.SaveChanges();

        Assert.Throws<ArgumentException>(() =>
            new LopHocService(Db, QuanLy()).SuaLop(lop.MaLop, YeuCauSua(lop, DateTime.Today.AddDays(2))));
    }

    [Fact]
    [Trait("Luat", "LH-20")]
    public void SuaLop_KeoDaiDenKhoangTrungLich_BaoLoi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv1 = TaoHocVien(TenTest(), SdtTest());
        var hv2 = TaoHocVien(TenTest(), SdtTest());
        var ngay1 = DateTime.Today.AddDays(2);
        var lop1 = TaoLop(gs, hv1, batDau: ngay1, ketThuc: ngay1.AddDays(5));
        TaoLop(gs, hv2, batDau: ngay1.AddDays(8), ketThuc: ngay1.AddDays(12));

        Assert.Throws<InvalidOperationException>(() =>
            new LopHocService(Db, QuanLy()).SuaLop(lop1.MaLop, YeuCauSua(lop1, ngay1.AddDays(10))));
    }

    [Fact]
    [Trait("Luat", "LH-22")]
    public void HuyLop_LopChuaKetThuc_DanhDauHuyVaXoaBuoiBuTuongLai()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var batDau = DateTime.Today.AddDays(2);
        var lop = TaoLop(gs, hv, trangThai: Constants.LopHocStatus.ChuaBatDau,
            batDau: batDau, ketThuc: batDau.AddDays(20));
        var goc = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = batDau,
            DiemDanh = Constants.DiemDanh.VangKhongPhep
        };
        Db.BuoiHocs.Add(goc);
        Db.SaveChanges();
        var bu = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = batDau.AddDays(3),
            DiemDanh = Constants.DiemDanh.CoMat,
            CaHoc = Constants.CaHoc.Sang,
            MaGSDay = gs.MaGS,
            MaBuoiGoc = goc.MaBuoi
        };
        Db.BuoiHocs.Add(bu);
        Db.SaveChanges();

        new LopHocService(Db, QuanLy()).HuyLop(lop.MaLop);

        Assert.Equal(Constants.LopHocStatus.DaHuy, lop.TrangThai);
        Assert.Null(Db.BuoiHocs.Find(bu.MaBuoi));
        Assert.NotNull(Db.BuoiHocs.Find(goc.MaBuoi));
    }

    [Fact]
    [Trait("Luat", "LH-22")]
    public void HuyLop_LopDaKetThuc_BiChan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var ketThuc = DateTime.Now.AddDays(-1);
        var lop = TaoLop(gs, hv, trangThai: Constants.LopHocStatus.DaKetThuc,
            batDau: ketThuc.AddDays(-10), ketThuc: ketThuc);

        Assert.Throws<InvalidOperationException>(() => new LopHocService(Db, QuanLy()).HuyLop(lop.MaLop));
    }

    [Fact]
    [Trait("Luat", "LH-23")]
    public void XoaLop_DaDiemDanh_BiChan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv);
        Db.BuoiHocs.Add(new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today,
            DiemDanh = Constants.DiemDanh.CoMat
        });
        Db.SaveChanges();

        Assert.Throws<InvalidOperationException>(() => new LopHocService(Db, QuanLy()).Delete(lop.MaLop));
    }

    [Fact]
    [Trait("Luat", "LH-23")]
    public void XoaLop_DaDongTien_BiChan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv);
        Db.ThanhToans.Add(new ThanhToan
        {
            MaLop = lop.MaLop,
            SoTien = 100000,
            NgayDong = DateTime.Today
        });
        Db.SaveChanges();

        Assert.Throws<InvalidOperationException>(() => new LopHocService(Db, QuanLy()).Delete(lop.MaLop));
    }

    [Fact]
    [Trait("Luat", "LH-23")]
    public void XoaLop_ChuaPhatSinhLichSu_XoaVaDongBoGiaSu()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gs, hv, batDau: DateTime.Today.AddDays(2), ketThuc: DateTime.Today.AddDays(20));

        new LopHocService(Db, QuanLy()).Delete(lop.MaLop);

        Assert.Null(Db.LopHocs.Find(lop.MaLop));
        Assert.Equal(Constants.GiaSuStatus.Ranh, gs.TrangThai);
    }
}
