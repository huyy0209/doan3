using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>
/// Cơ sở cho test tích hợp HTTP: app thật trong bộ nhớ + CSDL QuanLyGiaSu_Test.
/// Dữ liệu dùng tiền tố riêng "ITG_" (không dùng "TEST_") và dọn riêng, vì CSDL test được dùng chung với
/// các nhóm test khác / worker khác – <see cref="TestDb.DonDep"/> của họ xóa mọi hàng TEST_ bất kỳ lúc nào.
/// Mỗi test có sẵn 1 tài khoản quản lý, 1 gia sư (có tài khoản) và 1 gia sư khác để thử quyền.
/// </summary>
[Collection("Database")]
public abstract class HttpTestBase : IDisposable
{
    protected const string MatKhauTest = "Mk123456";

    private static readonly Lazy<WebAppFactory> _factory = new(() => new WebAppFactory());
    protected static WebAppFactory Factory => _factory.Value;

    protected readonly QuanLyGiaSuContext Db;
    protected readonly TaiKhoan QL;
    protected readonly GiaSu GS;          // gia sư "của mình"
    protected readonly TaiKhoan TKGS;     // tài khoản của GS
    protected readonly GiaSu GSKhac;      // gia sư người khác (không có tài khoản)

    private readonly List<TrinhDuyet> _trinhDuyets = new();

    protected HttpTestBase()
    {
        TestDb.DamBaoDaTao();
        Db = TestDb.TaoContext();
        DonDepItg(Db);
        string hau = Guid.NewGuid().ToString("N").Substring(0, 8);
        QL = TaoTaiKhoan("itg_ql" + hau, MatKhauTest);
        GS = TaoGiaSu(TenItg(), SdtTest());
        TKGS = TaoTaiKhoan("itg_gs" + hau, MatKhauTest, Constants.VaiTro.GiaSu, GS.MaGS);
        GSKhac = TaoGiaSu(TenItg(), SdtTest());
    }

    public void Dispose()
    {
        foreach (var t in _trinhDuyets) t.Dispose();
        try { DonDepItg(Db); }
        finally { Db.Dispose(); }
    }

    /// <summary>Xóa mọi dữ liệu tiền tố ITG_ (tên gia sư/học viên/môn/lớp/tài khoản), đúng thứ tự khóa ngoại.</summary>
    public static void DonDepItg(QuanLyGiaSuContext db)
    {
        db.Database.ExecuteSqlRaw(
            "DELETE FROM ThongBao WHERE MaTK IN (SELECT MaTK FROM TaiKhoan WHERE TenDangNhap LIKE 'itg[_]%');" +
            "DELETE FROM NhatKy WHERE MaTK IN (SELECT MaTK FROM TaiKhoan WHERE TenDangNhap LIKE 'itg[_]%') OR TenDangNhap LIKE 'itg[_]%';" +
            "DELETE FROM BuoiHoc WHERE MaLop IN (SELECT MaLop FROM LopHoc WHERE MonHoc LIKE 'ITG[_]%' OR TenLop LIKE 'ITG[_]%' OR MaGS IN (SELECT MaGS FROM GiaSu WHERE HoTen LIKE 'ITG[_]%' OR HoTen LIKE 'Itg %') OR MaHV IN (SELECT MaHV FROM HocVien WHERE HoTen LIKE 'ITG[_]%' OR HoTen LIKE 'Itg %'));" +
            "DELETE FROM ThanhToan WHERE MaLop IN (SELECT MaLop FROM LopHoc WHERE MonHoc LIKE 'ITG[_]%' OR TenLop LIKE 'ITG[_]%' OR MaGS IN (SELECT MaGS FROM GiaSu WHERE HoTen LIKE 'ITG[_]%' OR HoTen LIKE 'Itg %') OR MaHV IN (SELECT MaHV FROM HocVien WHERE HoTen LIKE 'ITG[_]%' OR HoTen LIKE 'Itg %'));" +
            "DELETE FROM LopHoc WHERE MonHoc LIKE 'ITG[_]%' OR TenLop LIKE 'ITG[_]%' OR MaGS IN (SELECT MaGS FROM GiaSu WHERE HoTen LIKE 'ITG[_]%' OR HoTen LIKE 'Itg %') OR MaHV IN (SELECT MaHV FROM HocVien WHERE HoTen LIKE 'ITG[_]%' OR HoTen LIKE 'Itg %');" +
            "DELETE FROM TaiKhoan WHERE TenDangNhap LIKE 'itg[_]%' OR MaGS IN (SELECT MaGS FROM GiaSu WHERE HoTen LIKE 'ITG[_]%' OR HoTen LIKE 'Itg %');" +
            "DELETE FROM GiaSu WHERE HoTen LIKE 'ITG[_]%' OR HoTen LIKE 'Itg %';" +
            "DELETE FROM HocVien WHERE HoTen LIKE 'ITG[_]%' OR HoTen LIKE 'Itg %';" +
            "DELETE FROM MonHoc WHERE TenMon LIKE 'ITG[_]%';");
    }

    // ---------- Dữ liệu mẫu (tiền tố ITG_) ----------

    /// <summary>Tên dùng khi chèn thẳng vào CSDL: ITG_ + 10 ký tự hex</summary>
    protected static string TenItg() => "ITG_" + Guid.NewGuid().ToString("N").Substring(0, 10);

    /// <summary>Tên hợp lệ khi gửi qua form (không có chữ số): "Itg " + chữ cái</summary>
    protected static string TenHoSoItg() =>
        "Itg " + string.Concat(Guid.NewGuid().ToByteArray().Select(b => (char)('a' + b % 26)));

    protected static string SdtTest() =>
        "0" + (BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0) % 1000000000).ToString("D9");

    /// <summary>CSDL test, context mới (không dính cache tracking của <c>Db</c>)</summary>
    protected static QuanLyGiaSuContext Moi() => TestDb.TaoContext();

    protected GiaSu TaoGiaSu(string hoTen, string sdt = null, string chuyenMon = "Toán", string trangThai = null)
    {
        var gs = new GiaSu
        {
            HoTen = hoTen, SDT = sdt ?? SdtTest(), ChuyenMon = chuyenMon,
            TrangThai = trangThai ?? Constants.GiaSuStatus.Ranh
        };
        Db.GiaSus.Add(gs);
        Db.SaveChanges();
        return gs;
    }

    protected HocVien TaoHocVien(string hoTen, string sdt = null)
    {
        var hv = new HocVien { HoTen = hoTen, SDT = sdt ?? SdtTest() };
        Db.HocViens.Add(hv);
        Db.SaveChanges();
        return hv;
    }

    protected MonHocDanhMuc TaoMon(string ten, string loai = "Chính", string capDo = null)
    {
        var mon = new MonHocDanhMuc { TenMon = ten, LoaiMon = loai, CapDo = capDo, NgungDay = false };
        Db.MonHocs.Add(mon);
        Db.SaveChanges();
        return mon;
    }

    protected TaiKhoan TaoTaiKhoan(string ten, string matKhau, string vaiTro = Constants.VaiTro.QuanLy, int? maGS = null)
    {
        var tk = new TaiKhoan
        {
            TenDangNhap = ten, MatKhauHash = MatKhau.Bam(matKhau), VaiTro = vaiTro, MaGS = maGS,
            HoatDong = true, NgayTao = DateTime.Now
        };
        Db.TaiKhoans.Add(tk);
        Db.SaveChanges();
        return tk;
    }

    /// <summary>Lớp đang diễn ra từ hôm qua tới +30 ngày, học hằng ngày (ThuHoc = null), ca Sáng</summary>
    protected LopHoc TaoLop(GiaSu gs, HocVien hv, string monHoc = "ITG_Mon", string trangThai = null,
        DateTime? batDau = null, DateTime? ketThuc = null, string caHoc = Constants.CaHoc.Sang, decimal hocPhi = 100000)
    {
        var lop = new LopHoc
        {
            MaGS = gs?.MaGS, MaHV = hv?.MaHV, MonHoc = monHoc, TenLop = TenItg(), HocPhi = hocPhi,
            TrangThai = trangThai ?? Constants.LopHocStatus.DangDienRa,
            ThoiGianBatDau = batDau ?? DateTime.Today.AddDays(-3),
            ThoiGianKetThuc = ketThuc ?? DateTime.Today.AddDays(30).AddHours(11),
            CaHoc = caHoc
        };
        Db.LopHocs.Add(lop);
        Db.SaveChanges();
        return lop;
    }

    // ---------- Trình duyệt giả lập ----------

    protected TrinhDuyet Khach()
    {
        var t = new TrinhDuyet(Factory);
        _trinhDuyets.Add(t);
        return t;
    }

    protected async Task<TrinhDuyet> DangNhapQL()
    {
        var t = Khach();
        var kq = await t.DangNhap(QL.TenDangNhap, MatKhauTest);
        Assert.True(kq.RedirectToi("/"), $"Đăng nhập quản lý lỗi: {(int)kq.Status} {kq.Location} {kq.Chu}");
        return t;
    }

    protected async Task<TrinhDuyet> DangNhapGS()
    {
        var t = Khach();
        var kq = await t.DangNhap(TKGS.TenDangNhap, MatKhauTest);
        Assert.True(kq.RedirectToi("/"), $"Đăng nhập gia sư lỗi: {(int)kq.Status} {kq.Location} {kq.Chu}");
        return t;
    }
}
