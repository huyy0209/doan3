using System;
using System.Linq;
using System.Threading.Tasks;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>Giữ khóa ứng dụng 'GhepLop' bằng một kết nối riêng (mô phỏng yêu cầu khác đang ghép/sửa lớp).</summary>
internal sealed class GiuKhoaGhepLop : IDisposable
{
    private readonly SqlConnection _conn;
    private readonly SqlTransaction _tx;

    public GiuKhoaGhepLop()
    {
        _conn = new SqlConnection(TestDb.ConnStr);
        _conn.Open();
        _tx = _conn.BeginTransaction();
        using var cmd = new SqlCommand(
            "DECLARE @kq int; EXEC @kq = sp_getapplock @Resource=N'GhepLop', @LockMode=N'Exclusive', " +
            "@LockOwner=N'Transaction', @LockTimeout=5000; SELECT @kq;", _conn, _tx);
        if ((int)cmd.ExecuteScalar() < 0) throw new InvalidOperationException("Không giữ được khóa GhepLop");
    }

    /// <summary>Nhả khóa (rollback transaction đang giữ khóa)</summary>
    public void Dispose()
    {
        _tx.Rollback();
        _tx.Dispose();
        _conn.Dispose();
    }
}

/// <summary>Review #3 (LH-12, LH-13): kiểm trùng lịch và ghi lớp cùng transaction + khóa ứng dụng.</summary>
[Collection("Database")]
public class GhepLopSongSongTests : DbTestBase
{
    private static GhepLopYeuCau YeuCau(GiaSu gs, HocVien hv, string mon, DateTime batDau) => new()
    {
        MaGS = gs.MaGS,
        MaHV = hv.MaHV,
        MonHoc = mon,
        HocPhi = 500000,
        BatDau = batDau,
        KetThuc = batDau.AddDays(10),
        CaHoc = Constants.CaHoc.Sang,
        TenLop = TenTest()
    };

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
            var y = YeuCau(gs, hv, mon.TenMon, batDau);
            barrier.SignalAndWait();
            try { new LopHocService(db, QuanLy()).GhepLop(y); return (Exception)null; }
            catch (Exception ex) { return ex; }
        })).ToArray();
        await Task.WhenAll(tasks);

        Assert.Equal(1, tasks.Count(t => t.Result == null));
        Assert.All(tasks.Where(t => t.Result != null), t => Assert.IsType<InvalidOperationException>(t.Result));
        Assert.Equal(1, Db.LopHocs.Count(l => l.MaGS == gs.MaGS));
    }

    /// <summary>Chứng minh có khóa thật: kết nối khác đang giữ 'GhepLop' thì hai GhepLop đều phải chờ; nhả khóa mới chạy, chỉ 1 lớp.</summary>
    [Fact]
    [Trait("Luat", "LH-12")]
    public async Task GhepLop_ChoKhoaGhepLop_NhaKhoaMoiChay_ChiMotLop()
    {
        var gs = TaoGiaSu(TenTest(), SdtTest());
        var mon = TaoMon(TenTest());
        var hvs = new[] { TaoHocVien(TenTest(), SdtTest()), TaoHocVien(TenTest(), SdtTest()) };
        var batDau = DateTime.Today.AddDays(2);

        Task<Exception>[] tasks;
        var giu = new GiuKhoaGhepLop();
        try
        {
            tasks = hvs.Select(hv => Task.Run(() =>
            {
                using var db = TestDb.TaoContext();
                try { new LopHocService(db, QuanLy()).GhepLop(YeuCau(gs, hv, mon.TenMon, batDau)); return (Exception)null; }
                catch (Exception ex) { return ex; }
            })).ToArray();

            await Task.Delay(2000);
            Assert.All(tasks, t => Assert.False(t.IsCompleted));      // đang chờ khóa
            Assert.Equal(0, Db.LopHocs.Count(l => l.MaGS == gs.MaGS));
        }
        finally { giu.Dispose(); }

        await Task.WhenAll(tasks);
        Assert.Equal(1, tasks.Count(t => t.Result == null));
        Assert.Equal(1, Db.LopHocs.Count(l => l.MaGS == gs.MaGS));
    }

    /// <summary>
    /// SuaLop đọc lớp trước khi lấy khóa; trong lúc chờ khóa yêu cầu khác đổi gia sư của lớp.
    /// Sau khi có khóa phải nạp lại lớp và kiểm trùng lịch, không ghi đè gia sư cũ.
    /// </summary>
    [Fact]
    [Trait("Luat", "LH-13")]
    public async Task SuaLop_LopBiDoiGiaSuTrongLucChoKhoa_VanKiemTrungLich()
    {
        var gs1 = TaoGiaSu(TenTest(), SdtTest());
        var gs2 = TaoGiaSu(TenTest(), SdtTest());
        var hv1 = TaoHocVien(TenTest(), SdtTest());
        var hv2 = TaoHocVien(TenTest(), SdtTest());
        Constants.CaHoc.TryGetGio(Constants.CaHoc.Sang, out _, out int gioKT);
        var ketThuc = DateTime.Today.AddDays(10).AddHours(gioKT);
        string thuHoc = Constants.ThuHoc.Ghi(Constants.ThuHoc.Doc(null));
        var lop = TaoLop(gs1, hv1, batDau: DateTime.Today.AddDays(1), ketThuc: ketThuc, thuHoc: thuHoc);

        Task<Exception> sua;
        var giu = new GiuKhoaGhepLop();
        try
        {
            sua = Task.Run(() =>
            {
                using var db = TestDb.TaoContext();
                try
                {
                    // Chỉ đổi học phí: theo bản đọc cũ (gia sư gs1) thì không cần kiểm trùng lịch
                    new LopHocService(db, QuanLy()).SuaLop(lop.MaLop, new SuaLopYeuCau
                    { MaGS = gs1.MaGS, HocPhi = 200000, KetThuc = ketThuc.Date, ThuHoc = thuHoc, TenLop = null });
                    return (Exception)null;
                }
                catch (Exception ex) { return ex; }
            });
            await Task.Delay(2000);
            Assert.False(sua.IsCompleted);

            // Yêu cầu khác: lớp chuyển sang gs2, rồi gs1 nhận lớp mới cùng ca (nếu lớp quay lại gs1 sẽ trùng)
            using (var db2 = TestDb.TaoContext())
                db2.Database.ExecuteSqlRaw("UPDATE LopHoc SET MaGS = {0} WHERE MaLop = {1}", gs2.MaGS, lop.MaLop);
            TaoLop(gs1, hv2, batDau: DateTime.Today.AddDays(1), ketThuc: ketThuc, thuHoc: thuHoc);
        }
        finally { giu.Dispose(); }

        var loi = await sua;
        Assert.IsType<InvalidOperationException>(loi);   // gs1 đã có lớp khác cùng ca
        Db.ChangeTracker.Clear();
        Assert.Equal(gs2.MaGS, Db.LopHocs.Single(l => l.MaLop == lop.MaLop).MaGS);
    }
}
