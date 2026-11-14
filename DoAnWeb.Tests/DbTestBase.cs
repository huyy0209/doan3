using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Lớp cơ sở cho các test cần CSDL: mỗi test có context riêng, tự dọn dữ liệu TEST_ trước và sau.
/// Các test dùng lớp này đều nằm trong collection "Database" (chạy tuần tự, tránh đụng nhau).
/// </summary>
[Collection("Database")]
public abstract class DbTestBase : IDisposable
{
    protected readonly QuanLyGiaSuContext Db;

    protected DbTestBase()
    {
        Db = TestDb.TaoContext();
        TestDb.DonDep(Db);
    }

    public void Dispose()
    {
        try { TestDb.DonDep(Db); }
        finally { Db.Dispose(); }
    }

    /// <summary>Người dùng hiện tại đóng vai trò Quản lý.</summary>
    protected static INguoiDungHienTai QuanLy(int maTK = 1, string ten = "admin") =>
        NguoiDungCoDinh.QuanLy(maTK, ten);

    /// <summary>Người dùng hiện tại đóng vai trò Gia sư.</summary>
    protected static INguoiDungHienTai GiaSu(int maTK, int maGS) =>
        NguoiDungCoDinh.GiaSuDay(maTK, maGS);

    // ---------- Hàm tạo dữ liệu mẫu (tiền tố TEST_) ----------

    protected GiaSu TaoGiaSu(string hoTen, string sdt = "0901000001", string trangThai = null, string chuyenMon = "Toán")
    {
        var gs = new GiaSu
        {
            HoTen = hoTen,
            SDT = sdt,
            ChuyenMon = chuyenMon,
            TrangThai = trangThai ?? Constants.GiaSuStatus.Ranh
        };
        Db.GiaSus.Add(gs);
        Db.SaveChanges();
        return gs;
    }

    protected HocVien TaoHocVien(string hoTen, string sdt = "0902000002")
    {
        var hv = new HocVien { HoTen = hoTen, SDT = sdt };
        Db.HocViens.Add(hv);
        Db.SaveChanges();
        return hv;
    }

    /// <summary>Tạo môn học; loai = "Chính" hoặc "Năng khiếu".</summary>
    protected MonHocDanhMuc TaoMon(string ten, string loai = "Chính", string capDo = null, bool ngungDay = false)
    {
        var mon = new MonHocDanhMuc
        {
            TenMon = ten,
            LoaiMon = loai,
            CapDo = capDo,
            NgungDay = ngungDay
        };
        Db.MonHocs.Add(mon);
        Db.SaveChanges();
        return mon;
    }

    protected TaiKhoan TaoTaiKhoan(string ten, string matKhau, string vaiTro = Constants.VaiTro.QuanLy, int? maGS = null, bool hoatDong = true)
    {
        var tk = new TaiKhoan
        {
            TenDangNhap = ten,
            MatKhauHash = MatKhau.Bam(matKhau),
            VaiTro = vaiTro,
            MaGS = maGS,
            HoatDong = hoatDong,
            NgayTao = DateTime.Now
        };
        Db.TaiKhoans.Add(tk);
        Db.SaveChanges();
        return tk;
    }

    protected static string TenTest() => "TEST_" + Guid.NewGuid().ToString("N").Substring(0, 10);
    protected static string TenHoSoTest() =>
        "Test " + string.Concat(Guid.NewGuid().ToByteArray().Select(b => (char)('a' + b % 26)));
    protected static string SdtTest() =>
        "0" + (BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0) % 1000000000).ToString("D9");

    protected LopHoc TaoLop(
        GiaSu gs,
        HocVien hv,
        string monHoc = "TEST_Mon",
        string trangThai = null,
        DateTime? batDau = null,
        DateTime? ketThuc = null,
        string caHoc = Constants.CaHoc.Sang,
        string thuHoc = null,
        int? maMon = null)
    {
        var lop = new LopHoc
        {
            MaGS = gs?.MaGS,
            MaHV = hv?.MaHV,
            MonHoc = monHoc,
            MaMon = maMon,
            TenLop = TenTest(),
            HocPhi = 100000,
            TrangThai = trangThai ?? Constants.LopHocStatus.DangDienRa,
            ThoiGianBatDau = batDau ?? DateTime.Now.AddDays(-1),
            ThoiGianKetThuc = ketThuc ?? DateTime.Now.AddDays(30),
            CaHoc = caHoc,
            ThuHoc = thuHoc
        };
        Db.LopHocs.Add(lop);
        Db.SaveChanges();
        return lop;
    }
}
