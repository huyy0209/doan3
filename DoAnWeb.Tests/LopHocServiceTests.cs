using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace DoAnWeb.Tests;

[Collection("Database")]
public class LopHocServiceTests : DbTestBase
{
    private GhepLopYeuCau YeuCau(GiaSu gs, HocVien hv, string mon, DateTime? batDau = null, DateTime? ketThuc = null) =>
        new()
        {
            MaGS = gs.MaGS,
            MaHV = hv.MaHV,
            MonHoc = mon,
            HocPhi = 500000,
            BatDau = batDau ?? DateTime.Today.AddDays(2),
            KetThuc = ketThuc ?? DateTime.Today.AddDays(12),
            CaHoc = Constants.CaHoc.Sang
        };

    [Fact]
    [Trait("Luat", "LH-04")]
    public void GhepLop_MonChuaCoTrongDanhMuc_BaoLoi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var y = YeuCau(gs, hv, TenTest());

        Assert.Throws<ArgumentException>(() => new LopHocService(Db, QuanLy()).GhepLop(y));
    }

    [Fact]
    [Trait("Luat", "LH-04")]
    public void GhepLop_MonNgungDay_BaoLoi()
    {
        var mon = TaoMon(TenTest(), ngungDay: true);
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());

        Assert.Throws<ArgumentException>(() => new LopHocService(Db, QuanLy()).GhepLop(YeuCau(gs, hv, mon.TenMon)));
    }

    [Fact]
    [Trait("Luat", "LH-05")]
    public void GhepLop_MonNangKhieu_BoTrongCapChonCapThapNhat()
    {
        var mon = TaoMon(TenTest(), Constants.LoaiMon.NangKhieu, "Co ban;Nang cao");
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());

        var lop = new LopHocService(Db, QuanLy()).GhepLop(YeuCau(gs, hv, mon.TenMon));

        Assert.Equal("Co ban", lop.CapDoDauVao);
    }

    [Fact]
    [Trait("Luat", "LH-05")]
    public void GhepLop_MonNangKhieu_CapKhongHopLe_BaoLoi()
    {
        var mon = TaoMon(TenTest(), Constants.LoaiMon.NangKhieu, "Co ban;Nang cao");
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var y = YeuCau(gs, hv, mon.TenMon);
        y.CapDoDauVao = "Chuyen nghiep";

        Assert.Throws<ArgumentException>(() => new LopHocService(Db, QuanLy()).GhepLop(y));
    }

    [Fact]
    [Trait("Luat", "LH-06")]
    public void GhepLop_GiaSuTamNghi_BaoLoi()
    {
        var mon = TaoMon(TenTest());
        var gs = TaoGiaSu(TenTest(), SdtTest(), Constants.GiaSuStatus.TamNghi);
        var hv = TaoHocVien(TenTest(), SdtTest());

        Assert.Throws<InvalidOperationException>(() => new LopHocService(Db, QuanLy()).GhepLop(YeuCau(gs, hv, mon.TenMon)));
    }

    [Fact]
    [Trait("Luat", "LH-06")]
    public void GhepLop_HocVienKhongTonTai_BaoLoi()
    {
        var mon = TaoMon(TenTest());
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var y = YeuCau(gs, new HocVien { MaHV = int.MaxValue }, mon.TenMon);

        Assert.Throws<InvalidOperationException>(() => new LopHocService(Db, QuanLy()).GhepLop(y));
    }

    [Fact]
    [Trait("Luat", "LH-07")]
    public void GhepLop_TenTrung_TuThemHauToVaLuuMaMon()
    {
        var mon = TaoMon(TenTest());
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var service = new LopHocService(Db, QuanLy());
        var batDau1 = DateTime.Today.AddDays(2);
        var lop1 = service.GhepLop(YeuCau(gs, hv, mon.TenMon, batDau1, batDau1.AddDays(2)));
        var batDau2 = batDau1.AddDays(4);
        var lop2 = service.GhepLop(YeuCau(gs, hv, mon.TenMon, batDau2, batDau2.AddDays(2)));

        Assert.Equal(lop1.TenLop + " (2)", lop2.TenLop);
        Assert.Equal(mon.MaMon, lop1.MaMon);
        Assert.Equal(mon.MaMon, lop2.MaMon);
    }

    [Fact]
    [Trait("Luat", "LH-12")]
    public void GhepLop_GiaSuTrungLich_BaoLoi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hvCu = TaoHocVien(TenTest(), SdtTest());
        var hvMoi = TaoHocVien(TenTest(), SdtTest());
        var mon = TaoMon(TenTest());
        var batDau = DateTime.Today.AddDays(5);
        TaoLop(gs, hvCu, mon.TenMon, batDau: batDau, ketThuc: batDau.AddDays(8));

        var loi = Assert.Throws<InvalidOperationException>(() =>
            new LopHocService(Db, QuanLy()).GhepLop(YeuCau(gs, hvMoi, mon.TenMon, batDau, batDau.AddDays(8))));

        Assert.Contains("Gia sư đã có lớp", loi.Message);
    }

    [Fact]
    [Trait("Luat", "LH-13")]
    public void GhepLop_HocVienTrungLich_BaoLoi()
    {
        var gsCu = TaoGiaSu(TenTest(), SdtTest());
        var gsMoi = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var mon = TaoMon(TenTest());
        var batDau = DateTime.Today.AddDays(5);
        TaoLop(gsCu, hv, mon.TenMon, batDau: batDau, ketThuc: batDau.AddDays(8));

        var loi = Assert.Throws<InvalidOperationException>(() =>
            new LopHocService(Db, QuanLy()).GhepLop(YeuCau(gsMoi, hv, mon.TenMon, batDau, batDau.AddDays(8))));

        Assert.Contains("Học viên đã có lớp", loi.Message);
    }

    [Fact]
    [Trait("Luat", "LH-14")]
    public void GhepLop_GiaSuDaCoBuoiBuCungCa_BaoLoi()
    {
        var gsBu = TaoGiaSu(TenTest(), SdtTest());
        var gsLop = TaoGiaSu(TenTest(), SdtTest());
        var hvCu = TaoHocVien(TenTest(), SdtTest());
        var hvMoi = TaoHocVien(TenTest(), SdtTest());
        var mon = TaoMon(TenTest());
        var batDau = DateTime.Today.AddDays(4);
        var lop = TaoLop(gsLop, hvCu, mon.TenMon, batDau: batDau, ketThuc: batDau.AddDays(8));
        var goc = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = batDau.AddDays(1),
            DiemDanh = Constants.DiemDanh.VangKhongPhep
        };
        Db.BuoiHocs.Add(goc);
        Db.SaveChanges();
        Db.BuoiHocs.Add(new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = batDau.AddDays(1),
            DiemDanh = Constants.DiemDanh.CoMat,
            CaHoc = Constants.CaHoc.Sang,
            MaGSDay = gsBu.MaGS,
            MaBuoiGoc = goc.MaBuoi
        });
        Db.SaveChanges();

        var loi = Assert.Throws<InvalidOperationException>(() =>
            new LopHocService(Db, QuanLy()).GhepLop(YeuCau(gsBu, hvMoi, mon.TenMon, batDau, batDau.AddDays(8))));

        Assert.Contains("buổi học bù", loi.Message);
    }

    [Fact]
    [Trait("Luat", "LH-15")]
    public void GhepLop_CungGiaSuHocVienVaMonTrungThoiGianKhacCa_BaoLoi()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var mon = TaoMon(TenTest());
        var batDau = DateTime.Today.AddDays(5);
        TaoLop(gs, hv, mon.TenMon, batDau: batDau, ketThuc: batDau.AddDays(8), caHoc: Constants.CaHoc.Sang);
        var yeuCau = YeuCau(gs, hv, mon.TenMon, batDau, batDau.AddDays(8));
        yeuCau.CaHoc = Constants.CaHoc.Chieu;

        var loi = Assert.Throws<InvalidOperationException>(() => new LopHocService(Db, QuanLy()).GhepLop(yeuCau));

        Assert.Contains("đang học môn", loi.Message);
    }

    [Fact]
    [Trait("Luat", "LH-18")]
    public void GhepLop_LoiDongBoTrangThai_RollbackLopVuaThem()
    {
        var mon = TaoMon(TenTest());
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var hv = TaoHocVien(TenTest(), SdtTest());
        var tenLop = TenTest();
        var y = YeuCau(gs, hv, mon.TenMon);
        y.TenLop = tenLop;

        var interceptor = new LoiTaiLanLuuThuHai();
        var options = new DbContextOptionsBuilder<QuanLyGiaSuContext>()
            .UseSqlServer(TestDb.ConnStr)
            .AddInterceptors(interceptor)
            .Options;

        using (var db = new QuanLyGiaSuContext(options))
            Assert.Throws<InvalidOperationException>(() => new LopHocService(db, QuanLy()).GhepLop(y));

        using var verify = TestDb.TaoContext();
        Assert.False(verify.LopHocs.Any(l => l.TenLop == tenLop));
    }

    private sealed class LoiTaiLanLuuThuHai : SaveChangesInterceptor
    {
        private int _soLanLuu;

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (++_soLanLuu == 2) throw new InvalidOperationException("Lỗi đồng bộ trạng thái giả lập.");
            return result;
        }
    }
}

/// <summary>Review #3 (LH-12, LH-13): kiểm trùng lịch và ghi lớp cùng transaction + khóa ứng dụng.</summary>
[Collection("Database")]
public class GhepLopSongSongTests : DbTestBase
{
    [Fact]
    [Trait("Luat", "LH-12")]
    public async Task GhepLopSongSong_CungGiaSuCaNgay_ChiMotLopDuocTao()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var mon = TaoMon(TenTest());
        const int soYeuCau = 4;
        var hvs = Enumerable.Range(0, soYeuCau).Select(_ => TaoHocVien(TenTest(), SdtTest())).ToList();
        var batDau = DateTime.Today.AddDays(2);
        var barrier = new System.Threading.Barrier(soYeuCau);

        var tasks = hvs.Select(hv => Task.Run(() =>
        {
            using var db = TestDb.TaoContext();   // mỗi yêu cầu một DbContext như request thật
            var y = new GhepLopYeuCau
            {
                MaGS = gs.MaGS, MaHV = hv.MaHV, MonHoc = mon.TenMon, HocPhi = 500000,
                BatDau = batDau, KetThuc = batDau.AddDays(10), CaHoc = Constants.CaHoc.Sang,
                TenLop = TenTest()
            };
            barrier.SignalAndWait();
            try { new LopHocService(db, QuanLy()).GhepLop(y); return (Exception)null; }
            catch (Exception ex) { return ex; }
        })).ToArray();
        await Task.WhenAll(tasks);

        Assert.Single(tasks, t => t.Result == null);
        Assert.All(tasks.Where(t => t.Result != null), t => Assert.IsType<InvalidOperationException>(t.Result));
        Assert.Equal(1, Db.LopHocs.Count(l => l.MaGS == gs.MaGS));
    }
}
