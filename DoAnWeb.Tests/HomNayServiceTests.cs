using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

[Collection("Database")]
public class HomNayServiceTests : DbTestBase
{
    [Fact]
    public void GetLopTrongKhoangNgay_tra_lich_cua_tat_ca_cac_ngay_trong_mot_lan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var homNay = DateTime.Today;
        var homQua = homNay.AddDays(-1);
        var lopHomQua = TaoLop(gs, TaoHocVien(TenTest(), SdtTest()), batDau: homNay.AddDays(-10),
            ketThuc: homNay.AddDays(10), thuHoc: Constants.ThuHoc.Cua(homQua).ToString());
        var lopHomNay = TaoLop(gs, TaoHocVien(TenTest(), SdtTest()), batDau: homNay.AddDays(-10),
            ketThuc: homNay.AddDays(10), thuHoc: Constants.ThuHoc.Cua(homNay).ToString());

        var lich = new HocTapService(Db, GiaSu(7002, gs.MaGS))
            .GetLopTrongKhoangNgay(homNay.AddDays(-6), homNay, gs.MaGS);

        Assert.Equal(7, lich.Count);
        Assert.Contains(lich[homQua], b => b.MaLop == lopHomQua.MaLop);
        Assert.Contains(lich[homNay], b => b.MaLop == lopHomNay.MaLop);
        Assert.DoesNotContain(lich[homQua], b => b.MaLop == lopHomNay.MaLop);
    }

    [Fact]
    public void QuanLy_ChiHienThiBuoiDaQuaGioVaNoQua30Ngay()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var ngay = DateTime.Today;
        var thu = Constants.ThuHoc.Cua(ngay).ToString();
        var lopQuaGio = TaoLop(gs, hv, batDau: ngay.AddDays(-3), ketThuc: ngay.AddDays(5),
            caHoc: Constants.CaHoc.Sang, thuHoc: thu);
        var lopDaDiemDanh = TaoLop(gs, TaoHocVien(TenTest(), SdtTest()), batDau: ngay.AddDays(-3), ketThuc: ngay.AddDays(5),
            caHoc: Constants.CaHoc.Sang, thuHoc: thu);
        var lopChuaQuaGio = TaoLop(gs, TaoHocVien(TenTest(), SdtTest()), batDau: ngay.AddDays(-3), ketThuc: ngay.AddDays(5),
            caHoc: Constants.CaHoc.Chieu, thuHoc: thu);
        Db.BuoiHocs.Add(new BuoiHoc
        {
            MaLop = lopDaDiemDanh.MaLop,
            NgayHoc = ngay,
            CaHoc = lopDaDiemDanh.CaHoc,
            DiemDanh = Constants.DiemDanh.CoMat
        });

        var thuKhacHomNay = (Constants.ThuHoc.Cua(ngay) == 8 ? 2 : Constants.ThuHoc.Cua(ngay) + 1).ToString();
        var noCu = TaoLop(gs, hv, batDau: ngay.AddDays(-40), ketThuc: ngay.AddDays(5), thuHoc: thuKhacHomNay);
        var noMoi = TaoLop(gs, TaoHocVien(TenTest(), SdtTest()), batDau: ngay.AddDays(-20), ketThuc: ngay.AddDays(5),
            thuHoc: thuKhacHomNay);
        Db.SaveChanges();

        var hocPhi = new HocPhiService(Db, QuanLy());
        var service = new HomNayService(new HocTapService(Db, QuanLy()),
            new NhacNoService(Db, hocPhi, QuanLy()), QuanLy());
        var duLieu = service.LayChoQuanLy(ngay.AddHours(11));

        Assert.Contains(duLieu.LopHomNay, b => b.MaLop == lopQuaGio.MaLop);
        Assert.Contains(duLieu.LopHomNay, b => b.MaLop == lopChuaQuaGio.MaLop);
        Assert.Contains(duLieu.ChuaDiemDanhQuaGio, b => b.Buoi.MaLop == lopQuaGio.MaLop);
        Assert.DoesNotContain(duLieu.ChuaDiemDanhQuaGio, b => b.Buoi.MaLop == lopDaDiemDanh.MaLop);
        Assert.DoesNotContain(duLieu.ChuaDiemDanhQuaGio, b => b.Buoi.MaLop == lopChuaQuaGio.MaLop);
        Assert.Contains(duLieu.NoQuaHan, n => n.MaLop == noCu.MaLop && n.SoNgayChuaDong > 30);
        Assert.DoesNotContain(duLieu.NoQuaHan, n => n.MaLop == noMoi.MaLop);
    }

    [Fact]
    public void GiaSu_ChiThayBuoiChuaDiemDanhCuaMinhTrongBayNgay()
    {
        var giaSuToi = TaoGiaSu(TenTest(), SdtTest());
        var giaSuKhac = TaoGiaSu(TenTest(), SdtTest());
        var ngay = DateTime.Today;
        var homQua = ngay.AddDays(-1);
        var maThuHomQua = Constants.ThuHoc.Cua(homQua).ToString();
        var buoiCuaToi = TaoLop(giaSuToi, TaoHocVien(TenTest(), SdtTest()), batDau: ngay.AddDays(-10),
            ketThuc: ngay.AddDays(10), caHoc: Constants.CaHoc.Sang, thuHoc: maThuHomQua);
        var buoiCuaNguoiKhac = TaoLop(giaSuKhac, TaoHocVien(TenTest(), SdtTest()), batDau: ngay.AddDays(-10),
            ketThuc: ngay.AddDays(10), caHoc: Constants.CaHoc.Sang, thuHoc: maThuHomQua);
        var buoiDaDiemDanh = TaoLop(giaSuToi, TaoHocVien(TenTest(), SdtTest()), batDau: ngay.AddDays(-10),
            ketThuc: ngay.AddDays(10), caHoc: Constants.CaHoc.Sang, thuHoc: maThuHomQua);
        Db.BuoiHocs.Add(new BuoiHoc
        {
            MaLop = buoiDaDiemDanh.MaLop,
            NgayHoc = homQua,
            CaHoc = buoiDaDiemDanh.CaHoc,
            DiemDanh = Constants.DiemDanh.CoMat
        });
        var buoiHomNay = TaoLop(giaSuToi, TaoHocVien(TenTest(), SdtTest()), batDau: ngay.AddDays(-2),
            ketThuc: ngay.AddDays(10), caHoc: Constants.CaHoc.Toi, thuHoc: Constants.ThuHoc.Cua(ngay).ToString());
        Db.SaveChanges();

        var user = GiaSu(7001, giaSuToi.MaGS);
        var service = new HomNayService(new HocTapService(Db, user),
            new NhacNoService(Db, new HocPhiService(Db, QuanLy()), QuanLy()), user);
        var duLieu = service.LayChoGiaSu(ngay.AddHours(12));

        Assert.Equal(1, duLieu.SoBuoiHomNay);
        var pending = Assert.Single(duLieu.ChuaDiemDanh7Ngay);
        Assert.Equal(buoiCuaToi.MaLop, pending.Buoi.MaLop);
        Assert.Equal(homQua, pending.Ngay);
        Assert.DoesNotContain(duLieu.ChuaDiemDanh7Ngay, b => b.Buoi.MaLop == buoiCuaNguoiKhac.MaLop);
        Assert.DoesNotContain(duLieu.ChuaDiemDanh7Ngay, b => b.Buoi.MaLop == buoiDaDiemDanh.MaLop);
        Assert.DoesNotContain(duLieu.ChuaDiemDanh7Ngay, b => b.Buoi.MaLop == buoiHomNay.MaLop);
    }
}
