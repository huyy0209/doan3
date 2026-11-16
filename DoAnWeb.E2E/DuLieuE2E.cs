using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.E2E;

/// <summary>
/// Dữ liệu thử cho E2E: mọi tên đều có tiền tố E2E_ (tài khoản: e2e_) và được dọn trước + sau khi chạy.
/// Dùng tiền tố riêng, không dùng TEST_ / ITG_ vì CSDL test dùng chung với các nhóm test khác.
/// </summary>
public sealed class DuLieuE2E
{
    public const string MatKhau = "Mk123456";

    public string Hau { get; } = Guid.NewGuid().ToString("N").Substring(0, 8);

    public TaiKhoan QuanLy { get; private set; }
    public TaiKhoan TaiKhoanGS { get; private set; }

    public string TenMon { get; private set; }
    public HocVien HV1 { get; private set; }   // có lớp nền với GS_A, ca Sáng
    public HocVien HV2 { get; private set; }   // học viên ghép lớp mới ở kịch bản 2
    public HocVien HV3 { get; private set; }   // lớp điểm danh với GS_B
    public HocVien HV4 { get; private set; }   // lớp học phí
    public GiaSu GS_A { get; private set; }
    public GiaSu GS_B { get; private set; }    // có tài khoản – đăng nhập ở kịch bản 3, 5
    public LopHoc LopNen { get; private set; }       // GS_A + HV1, ca Sáng (gây xung đột)
    public LopHoc LopDiemDanh { get; private set; }  // GS_B + HV3, ca Sáng
    public LopHoc LopHocPhi { get; private set; }    // GS_A + HV4, ca Chiều, học phí 1.000.000

    /// <summary>Xóa mọi dữ liệu E2E_ theo đúng thứ tự khóa ngoại.</summary>
    public static void DonDep(QuanLyGiaSuContext db)
    {
        const string lop = "(SELECT MaLop FROM LopHoc WHERE MonHoc LIKE 'E2E[_]%' OR TenLop LIKE 'E2E[_]%' " +
                           "OR MaGS IN (SELECT MaGS FROM GiaSu WHERE HoTen LIKE 'E2E[_]%') " +
                           "OR MaHV IN (SELECT MaHV FROM HocVien WHERE HoTen LIKE 'E2E[_]%'))";
        db.Database.ExecuteSqlRaw(
            $"DELETE FROM BuoiHoc WHERE MaLop IN {lop};" +
            $"DELETE FROM ThanhToan WHERE MaLop IN {lop};" +
            $"DELETE FROM LopHoc WHERE MaLop IN {lop};" +
            "DELETE FROM TaiKhoan WHERE TenDangNhap LIKE 'e2e[_]%' OR MaGS IN (SELECT MaGS FROM GiaSu WHERE HoTen LIKE 'E2E[_]%');" +
            "DELETE FROM GiaSu WHERE HoTen LIKE 'E2E[_]%';" +
            "DELETE FROM HocVien WHERE HoTen LIKE 'E2E[_]%' OR GhiChu LIKE 'E2E[_]%';" +
            "DELETE FROM MonHoc WHERE TenMon LIKE 'E2E[_]%';");
    }

    public void Nap(QuanLyGiaSuContext db)
    {
        TenMon = "E2E_Mon_" + Hau;
        db.MonHocs.Add(new MonHocDanhMuc { TenMon = TenMon, LoaiMon = "Chính", NgungDay = false });

        HV1 = new HocVien { HoTen = "E2E_HV1_" + Hau, SDT = Sdt() };
        HV2 = new HocVien { HoTen = $"E2E_Học viên thử_{Hau}", SDT = Sdt() };
        HV3 = new HocVien { HoTen = "E2E_HV3_" + Hau, SDT = Sdt() };
        HV4 = new HocVien { HoTen = "E2E_HV4_" + Hau, SDT = Sdt() };
        db.HocViens.AddRange(HV1, HV2, HV3, HV4);

        GS_A = new GiaSu { HoTen = "E2E_GSA_" + Hau, SDT = Sdt(), ChuyenMon = TenMon, TrangThai = Constants.GiaSuStatus.Ranh };
        GS_B = new GiaSu { HoTen = "E2E_GSB_" + Hau, SDT = Sdt(), ChuyenMon = TenMon, TrangThai = Constants.GiaSuStatus.Ranh };
        db.GiaSus.AddRange(GS_A, GS_B);
        db.SaveChanges();

        QuanLy = new TaiKhoan
        {
            TenDangNhap = "e2e_ql_" + Hau, MatKhauHash = MatKhauBam(), VaiTro = Constants.VaiTro.QuanLy,
            HoatDong = true, NgayTao = DateTime.Now
        };
        TaiKhoanGS = new TaiKhoan
        {
            TenDangNhap = "e2e_gs_" + Hau, MatKhauHash = MatKhauBam(), VaiTro = Constants.VaiTro.GiaSu,
            MaGS = GS_B.MaGS, HoatDong = true, NgayTao = DateTime.Now
        };
        db.TaiKhoans.AddRange(QuanLy, TaiKhoanGS);

        LopNen = TaoLop(db, GS_A, HV1, "Nen", Constants.CaHoc.Sang, 500000);
        LopDiemDanh = TaoLop(db, GS_B, HV3, "DiemDanh", Constants.CaHoc.Sang, 500000);
        LopHocPhi = TaoLop(db, GS_A, HV4, "HocPhi", Constants.CaHoc.Chieu, 1000000);
        db.SaveChanges();
    }

    private LopHoc TaoLop(QuanLyGiaSuContext db, GiaSu gs, HocVien hv, string nhan, string ca, decimal hocPhi)
    {
        // Đang diễn ra từ 3 ngày trước tới 30 ngày sau, học hằng ngày (ThuHoc = null) → hôm nay luôn có buổi.
        var lop = new LopHoc
        {
            MaGS = gs.MaGS, MaHV = hv.MaHV, MonHoc = TenMon, TenLop = $"E2E_Lop_{nhan}_{Hau}", HocPhi = hocPhi,
            TrangThai = Constants.LopHocStatus.DangDienRa,
            ThoiGianBatDau = DateTime.Today.AddDays(-3),
            ThoiGianKetThuc = DateTime.Today.AddDays(30).AddHours(11),
            CaHoc = ca
        };
        db.LopHocs.Add(lop);
        return lop;
    }

    private static string MatKhauBam() => DoAnWeb.Services.MatKhau.Bam(MatKhau);

    private static string Sdt() =>
        "0" + (BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0) % 1000000000).ToString("D9");
}
