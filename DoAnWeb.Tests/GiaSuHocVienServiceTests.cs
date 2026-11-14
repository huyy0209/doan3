using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using DoAnWeb.Services.Validators;
using Xunit;

namespace DoAnWeb.Tests;

[Collection("Database")]
public class GiaSuHocVienServiceTests : DbTestBase
{
    [Fact]
    [Trait("Luat", "GS-05")]
    public void GiaSu_SdtTrung_BiPhatHienVaBoQuaChinhMinhKhiSua()
    {
        var gs1 = TaoGiaSu(TenTest(), SdtTest());
        var gs2 = TaoGiaSu(TenTest(), SdtTest());
        var service = new GiaSuService(Db, QuanLy());

        Assert.True(service.ExistsSDT(gs1.SDT));
        Assert.True(service.ExistsSDT(gs1.SDT, gs2.MaGS));
        Assert.False(service.ExistsSDT(gs1.SDT, gs1.MaGS));
        Assert.False(service.ExistsSDT("  "));
    }

    [Fact]
    [Trait("Luat", "GS-07")]
    public void GiaSu_LuuHoSo_TamNghiLaTrangThaiNguoiDungChon()
    {
        var hoSo = new HoSoGiaSu
        {
            HoTen = TenHoSoTest(),
            SDT = SdtTest(),
            ChuyenMon = "Toán",
            TamNghi = true
        };

        var gs = new GiaSuService(Db, QuanLy()).LuuHoSo(null, hoSo);

        Assert.Equal(Constants.GiaSuStatus.TamNghi, gs.TrangThai);
    }

    [Fact]
    [Trait("Luat", "GS-08")]
    public void GiaSu_DongBoTrangThai_TheoLichVaGiuTamNghi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest(), Constants.GiaSuStatus.Ranh);
        GiaSuService.DongBoTrangThai(Db, gs.MaGS);
        Assert.Equal(Constants.GiaSuStatus.Ranh, gs.TrangThai);

        var hv = TaoHocVien(TenTest(), SdtTest());
        var lopTuongLai = TaoLop(gs, hv, batDau: DateTime.Now.AddDays(1));
        GiaSuService.DongBoTrangThai(Db, gs.MaGS);
        Assert.Equal(Constants.GiaSuStatus.Ban, gs.TrangThai);

        Db.LopHocs.Remove(lopTuongLai);
        Db.SaveChanges();
        TaoLop(gs, hv, batDau: DateTime.Now.AddHours(-1), ketThuc: DateTime.Now.AddHours(1));
        GiaSuService.DongBoTrangThai(Db, gs.MaGS);
        Assert.Equal(Constants.GiaSuStatus.DangDay, gs.TrangThai);

        gs.TrangThai = Constants.GiaSuStatus.TamNghi;
        GiaSuService.DongBoTrangThai(Db, gs.MaGS);
        Assert.Equal(Constants.GiaSuStatus.TamNghi, gs.TrangThai);
    }

    [Fact]
    [Trait("Luat", "GS-09")]
    public void GiaSu_Xoa_CoLopHoatDong_BiChan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        TaoLop(gs, hv);

        Assert.Throws<InvalidOperationException>(() => new GiaSuService(Db, QuanLy()).Delete(gs.MaGS));
    }

    [Fact]
    [Trait("Luat", "GS-09")]
    public void GiaSu_Xoa_CoLichSuLop_BiChan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        TaoLop(gs, hv, trangThai: Constants.LopHocStatus.DaKetThuc,
            batDau: DateTime.Now.AddDays(-10), ketThuc: DateTime.Now.AddDays(-1));

        Assert.Throws<InvalidOperationException>(() => new GiaSuService(Db, QuanLy()).Delete(gs.MaGS));
    }

    [Fact]
    [Trait("Luat", "GS-09")]
    public void GiaSu_Xoa_DaDayHocBu_BiChan()
    {
        var gsDayBu = TaoGiaSu(TenTest(), SdtTest());
        var gsLop = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var lop = TaoLop(gsLop, hv);
        var buoi = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today,
            DiemDanh = Constants.DiemDanh.CoMat,
            MaGSDay = gsDayBu.MaGS,
            CaHoc = Constants.CaHoc.Sang,
            MaBuoiGoc = 1
        };
        // MaBuoiGoc phải trỏ tới một buổi có thật; tạo buổi gốc rồi gắn buổi bù.
        var buoiGoc = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today.AddDays(-1),
            DiemDanh = Constants.DiemDanh.VangKhongPhep
        };
        Db.BuoiHocs.Add(buoiGoc);
        Db.SaveChanges();
        buoi.MaBuoiGoc = buoiGoc.MaBuoi;
        Db.BuoiHocs.Add(buoi);
        Db.SaveChanges();

        Assert.Throws<InvalidOperationException>(() => new GiaSuService(Db, QuanLy()).Delete(gsDayBu.MaGS));
    }

    [Fact]
    [Trait("Luat", "GS-09")]
    public void GiaSu_Xoa_ChuaCoLop_XoaKemTaiKhoan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var tk = TaoTaiKhoan(TenTest(), "MatKhau@123", Constants.VaiTro.GiaSu, gs.MaGS);

        new GiaSuService(Db, QuanLy()).Delete(gs.MaGS);

        Assert.Null(Db.GiaSus.Find(gs.MaGS));
        Assert.Null(Db.TaiKhoans.Find(tk.MaTK));
    }

    [Fact]
    [Trait("Luat", "HV-05")]
    public void HocVien_Xoa_CoLopHoatDong_BiChan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        TaoLop(gs, hv);

        Assert.Throws<InvalidOperationException>(() => new HocVienService(Db, QuanLy()).Delete(hv.MaHV));
    }

    [Fact]
    [Trait("Luat", "HV-05")]
    public void HocVien_Xoa_CoLichSuLop_BiChan()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        TaoLop(gs, hv, trangThai: Constants.LopHocStatus.DaKetThuc,
            batDau: DateTime.Now.AddDays(-10), ketThuc: DateTime.Now.AddDays(-1));

        Assert.Throws<InvalidOperationException>(() => new HocVienService(Db, QuanLy()).Delete(hv.MaHV));
    }

    [Fact]
    [Trait("Luat", "HV-05")]
    public void HocVien_Xoa_KhongCoLop_ThanhCong()
    {
        var hv = TaoHocVien(TenTest(), SdtTest());

        new HocVienService(Db, QuanLy()).Delete(hv.MaHV);

        Assert.Null(Db.HocViens.Find(hv.MaHV));
    }
}
