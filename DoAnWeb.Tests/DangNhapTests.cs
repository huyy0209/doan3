using DoAnWeb.Services.NhatKy;
using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Controllers;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.TaiKhoan;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm thử đăng nhập và quản lý tài khoản trên CSDL test (TK-04..TK-17, TK-19).
/// Mỗi test tự tạo tài khoản TEST_ rồi dọn sau khi chạy.
/// </summary>
[Collection("Database")]
public class DangNhapTests : DbTestBase
{
    private const string Mk = "MatKhau@123";

    private TaiKhoanService TaoDichVu(INguoiDungHienTai nd = null) =>
        new(Db, nd ?? QuanLy());

    // ============ TK-04: tên đăng nhập trim + ToLowerInvariant ============

    [Fact]
    [Trait("Luat", "TK-04")]
    public void DangNhap_TrimVaToLower_TimDuocTaiKhoan()
    {
        TaoTaiKhoan("test_user1", Mk);
        var tk = TaoDichVu().DangNhap("  TEST_USER1  ", Mk);
        Assert.NotNull(tk);
        Assert.Equal("test_user1", tk.TenDangNhap);
    }

    [Fact]
    [Trait("Luat", "TK-04")]
    public void TaoTaiKhoan_LuuTenDaLowerVaTrim()
    {
        TaoDichVu().TaoTaiKhoan("  Test_User2  ", Mk, Constants.VaiTro.QuanLy, null);
        var tk = Db.TaiKhoans.Single(t => t.TenDangNhap == "test_user2");
        Assert.NotNull(tk);
    }

    // ============ TK-05: thiếu tên/mật khẩu ============

    [Theory]
    [Trait("Luat", "TK-05")]
    [InlineData("", "x")]
    [InlineData("   ", "x")]
    [InlineData(null, "x")]
    [InlineData("ten", "")]
    [InlineData("ten", null)]
    public void DangNhap_ThieuThongTin_Loi(string ten, string mk)
    {
        var ex = Assert.Throws<ArgumentException>(() => TaoDichVu().DangNhap(ten, mk));
        Assert.Equal("Nhập tên đăng nhập và mật khẩu.", ex.Message);
    }

    // ============ TK-06: sai tên / sai mk / tạm khóa → cùng một câu ============

    [Fact]
    [Trait("Luat", "TK-06")]
    public void DangNhap_SaiTen_CungCauLoi()
    {
        var ex = Assert.Throws<UnauthorizedAccessException>(() => TaoDichVu().DangNhap("khong_ton_tai", Mk));
        Assert.Equal("Sai tên đăng nhập hoặc mật khẩu.", ex.Message);
    }

    [Fact]
    [Trait("Luat", "TK-06")]
    public void DangNhap_SaiMatKhau_CungCauLoi()
    {
        TaoTaiKhoan("test_sai", Mk);
        var ex = Assert.Throws<UnauthorizedAccessException>(() => TaoDichVu().DangNhap("test_sai", "sai"));
        Assert.Equal("Sai tên đăng nhập hoặc mật khẩu.", ex.Message);
    }

    [Fact]
    [Trait("Luat", "TK-06")]
    public void DangNhap_DangTamKhoa_CungCauLoi()
    {
        var tk = TaoTaiKhoan("test_khoa", Mk);
        tk.KhoaDen = DateTime.Now.AddMinutes(5);
        Db.SaveChanges();

        var ex = Assert.Throws<UnauthorizedAccessException>(() => TaoDichVu().DangNhap("test_khoa", Mk));
        Assert.Equal("Sai tên đăng nhập hoặc mật khẩu.", ex.Message);
    }

    // ============ TK-07: sai 5 lần → KhoaDen = now + 5 phút, reset SoLanSai = 0 ============

    [Fact]
    [Trait("Luat", "TK-07")]
    public void DangNhap_Sai5Lan_KhoaTamThoi()
    {
        var tk = TaoTaiKhoan("test_5lan", Mk);
        var dv = TaoDichVu();
        for (int i = 0; i < 4; i++)
            Assert.Throws<UnauthorizedAccessException>(() => dv.DangNhap("test_5lan", "sai"));

        // Lần sai thứ 5 → tạm khóa.
        Assert.Throws<UnauthorizedAccessException>(() => dv.DangNhap("test_5lan", "sai"));

        Db.Entry(tk).Reload();
        Assert.Equal(0, tk.SoLanSai);
        Assert.NotNull(tk.KhoaDen);
        Assert.InRange(tk.KhoaDen.Value, DateTime.Now.AddMinutes(4), DateTime.Now.AddMinutes(6));
    }

    // ============ TK-08: đúng mk nhưng HoatDong = false ============

    [Fact]
    [Trait("Luat", "TK-08")]
    public void DangNhap_TaiKhoanBiKhoa_LoiRieng()
    {
        TaoTaiKhoan("test_bikhoa", Mk, Constants.VaiTro.QuanLy, null, hoatDong: false);
        var ex = Assert.Throws<UnauthorizedAccessException>(() => TaoDichVu().DangNhap("test_bikhoa", Mk));
        Assert.Equal("Tài khoản đã bị khóa. Liên hệ quản lý.", ex.Message);
    }

    // ============ TK-09: đăng nhập đúng → xóa SoLanSai, KhoaDen ============

    [Fact]
    [Trait("Luat", "TK-09")]
    public void DangNhap_Dung_XoaDemSaiVaTamKhoa()
    {
        var tk = TaoTaiKhoan("test_dung", Mk);
        tk.SoLanSai = 3;
        tk.KhoaDen = DateTime.Now.AddMinutes(-1); // quá hạn, không còn chặn
        Db.SaveChanges();

        var kq = TaoDichVu().DangNhap("test_dung", Mk);
        Assert.NotNull(kq);

        Db.Entry(tk).Reload();
        Assert.Equal(0, tk.SoLanSai);
        Assert.Null(tk.KhoaDen);
    }

    // ============ TK-10: bảng rỗng → tạo admin/admin123 (QuanLy) ============

    [Fact]
    [Trait("Luat", "TK-10")]
    public void DamBaoTaiKhoanMacDinh_BangRong_TaoAdmin()
    {
        using var tx = Db.Database.BeginTransaction();
        Db.Database.ExecuteSqlRaw("DELETE FROM NhatKy; DELETE FROM TaiKhoan;");

        var dv = TaoDichVu();
        Assert.True(dv.DamBaoTaiKhoanMacDinh());

        var admin = Db.TaiKhoans.Single(t => t.TenDangNhap == "admin");
        Assert.Equal(Constants.VaiTro.QuanLy, admin.VaiTro);
        Assert.True(MatKhau.KiemTra("admin123", admin.MatKhauHash));

        tx.Rollback(); // hoàn nguyên dữ liệu tài khoản gốc
    }

    [Fact]
    [Trait("Luat", "TK-10")]
    public void DamBaoTaiKhoanMacDinh_DaCoTaiKhoan_KhongTao()
    {
        TaoTaiKhoan("test_co", Mk);
        Assert.False(TaoDichVu().DamBaoTaiKhoanMacDinh());
    }

    [Fact]
    [Trait("Luat", "TK-10")]
    public void DangNhap_AdminMacDinhDuyNhat_HienGoiY()
    {
        using var tx = Db.Database.BeginTransaction();
        Db.Database.ExecuteSqlRaw("DELETE FROM NhatKy; DELETE FROM TaiKhoan;");
        TaoTaiKhoan(TaiKhoanService.AdminMacDinh, TaiKhoanService.MatKhauMacDinh);

        Assert.True(((DangNhapVM)TaoController().Model).LanDau);

        tx.Rollback();
    }

    [Fact]
    [Trait("Luat", "TK-10")]
    public void DangNhap_NhieuTaiKhoan_KhongHienGoiY()
    {
        using var tx = Db.Database.BeginTransaction();
        Db.Database.ExecuteSqlRaw("DELETE FROM NhatKy; DELETE FROM TaiKhoan;");
        TaoTaiKhoan(TaiKhoanService.AdminMacDinh, TaiKhoanService.MatKhauMacDinh);
        TaoTaiKhoan("other", Mk);

        Assert.False(((DangNhapVM)TaoController().Model).LanDau);

        tx.Rollback();
    }

    [Fact]
    [Trait("Luat", "TK-10")]
    public void DangNhap_AdminDaDoiMatKhau_KhongHienGoiY()
    {
        using var tx = Db.Database.BeginTransaction();
        Db.Database.ExecuteSqlRaw("DELETE FROM NhatKy; DELETE FROM TaiKhoan;");
        TaoTaiKhoan(TaiKhoanService.AdminMacDinh, Mk);

        Assert.False(((DangNhapVM)TaoController().Model).LanDau);

        tx.Rollback();
    }

    private ViewResult TaoController()
    {
        var controller = new TaiKhoanController(TaoDichVu(), QuanLy(), new NhatKyService(Db, new HttpContextAccessor()));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return Assert.IsType<ViewResult>(controller.DangNhap());
    }

    // ============ TK-14: đổi mật khẩu (mới ≠ cũ, ≠ admin123, đúng mk hiện tại) ============

    [Fact]
    [Trait("Luat", "TK-14")]
    public void DoiMatKhau_MoiTrungCu_Loi()
    {
        var tk = TaoTaiKhoan("test_doi1", Mk);
        var ex = Assert.Throws<ArgumentException>(() => TaoDichVu().DoiMatKhau(tk.MaTK, Mk, Mk));
        Assert.Equal("Mật khẩu mới phải khác mật khẩu hiện tại.", ex.Message);
    }

    [Fact]
    [Trait("Luat", "TK-14")]
    public void DoiMatKhau_MoiLaMacDinh_Loi()
    {
        var tk = TaoTaiKhoan("test_doi2", Mk);
        var ex = Assert.Throws<ArgumentException>(
            () => TaoDichVu().DoiMatKhau(tk.MaTK, Mk, TaiKhoanService.MatKhauMacDinh));
        Assert.Equal("Không dùng lại mật khẩu mặc định.", ex.Message);
    }

    [Fact]
    [Trait("Luat", "TK-14")]
    public void DoiMatKhau_SaiMatKhauCu_Loi()
    {
        var tk = TaoTaiKhoan("test_doi3", Mk);
        Assert.Throws<UnauthorizedAccessException>(
            () => TaoDichVu().DoiMatKhau(tk.MaTK, "sai", "MatKhauMoi@1"));
    }

    [Fact]
    [Trait("Luat", "TK-14")]
    public void DoiMatKhau_HopLe_DoiDuoc()
    {
        var tk = TaoTaiKhoan("test_doi4", Mk);
        TaoDichVu().DoiMatKhau(tk.MaTK, Mk, "MatKhauMoi@1");

        Db.Entry(tk).Reload();
        Assert.True(MatKhau.KiemTra("MatKhauMoi@1", tk.MatKhauHash));
    }

    // ============ TK-15: tạo tài khoản (chỉ QL) ============

    [Fact]
    [Trait("Luat", "TK-15")]
    public void TaoTaiKhoan_VaiTroKhongHopLe_Loi()
    {
        Assert.Throws<ArgumentException>(() => TaoDichVu().TaoTaiKhoan("test_vt", Mk, "SinhVien", null));
    }

    [Fact]
    [Trait("Luat", "TK-15")]
    public void TaoTaiKhoan_GiaSuThieuMaGS_Loi()
    {
        Assert.Throws<ArgumentException>(() => TaoDichVu().TaoTaiKhoan("test_gs", Mk, Constants.VaiTro.GiaSu, null));
    }

    [Fact]
    [Trait("Luat", "TK-15")]
    public void TaoTaiKhoan_TenTrung_Loi()
    {
        TaoTaiKhoan("test_trung", Mk);
        Assert.Throws<ArgumentException>(() => TaoDichVu().TaoTaiKhoan("test_trung", Mk, Constants.VaiTro.QuanLy, null));
    }

    [Fact]
    [Trait("Luat", "TK-15")]
    public void TaoTaiKhoan_GiaSuDaCoTaiKhoan_Loi()
    {
        var gs = TaoGiaSu("TEST_GS_co_tk");
        TaoDichVu().TaoTaiKhoan("test_gs1", Mk, Constants.VaiTro.GiaSu, gs.MaGS);
        Assert.Throws<ArgumentException>(() => TaoDichVu().TaoTaiKhoan("test_gs2", Mk, Constants.VaiTro.GiaSu, gs.MaGS));
    }

    [Fact]
    [Trait("Luat", "TK-15")]
    public void TaoTaiKhoan_QuanLy_MaGSLuonNull()
    {
        var gs = TaoGiaSu("TEST_GS_ql");
        TaoDichVu().TaoTaiKhoan("test_ql", Mk, Constants.VaiTro.QuanLy, gs.MaGS);
        var tk = Db.TaiKhoans.Single(t => t.TenDangNhap == "test_ql");
        Assert.Null(tk.MaGS);
    }

    // ============ TK-16: đặt lại mật khẩu → gỡ tạm khóa ============

    [Fact]
    [Trait("Luat", "TK-16")]
    public void DatLaiMatKhau_GoTamKhoaVaDemSai()
    {
        var tk = TaoTaiKhoan("test_reset", Mk);
        tk.SoLanSai = 4;
        tk.KhoaDen = DateTime.Now.AddMinutes(5);
        Db.SaveChanges();

        TaoDichVu().DatLaiMatKhau(tk.MaTK, "MatKhauMoi@2");

        Db.Entry(tk).Reload();
        Assert.True(MatKhau.KiemTra("MatKhauMoi@2", tk.MatKhauHash));
        Assert.Equal(0, tk.SoLanSai);
        Assert.Null(tk.KhoaDen);
    }

    // ============ TK-17: khóa/mở (chỉ QL) ============

    [Fact]
    [Trait("Luat", "TK-17")]
    public void DoiTrangThai_KhoaChinhMinh_Loi()
    {
        var tk = TaoTaiKhoan("test_minh", Mk, Constants.VaiTro.QuanLy);
        var dv = TaoDichVu(QuanLy(maTK: tk.MaTK));
        Assert.Throws<InvalidOperationException>(() => dv.DoiTrangThai(tk.MaTK));
    }

    [Fact]
    [Trait("Luat", "TK-17")]
    public void DoiTrangThai_KhoaGiaSu_ThanhCong()
    {
        var gs = TaoGiaSu("TEST_GS_kgs");
        var tk = TaoTaiKhoan("test_kgs", Mk, Constants.VaiTro.GiaSu, gs.MaGS);
        bool moi = TaoDichVu().DoiTrangThai(tk.MaTK);
        Assert.False(moi);
        Db.Entry(tk).Reload();
        Assert.False(tk.HoatDong);
    }

    [Fact]
    [Trait("Luat", "TK-17")]
    public void DoiTrangThai_QuanLyCuoiCung_Loi()
    {
        using var tx = Db.Database.BeginTransaction();
        // Vô hiệu mọi QL hiện có, để lại duy nhất một QL test.
        Db.Database.ExecuteSqlRaw("UPDATE TaiKhoan SET HoatDong=0 WHERE VaiTro='QuanLy'");
        var ql = TaoTaiKhoan("test_ql_cuoi", Mk, Constants.VaiTro.QuanLy);

        var dv = TaoDichVu(QuanLy(maTK: 999));
        Assert.Throws<InvalidOperationException>(() => dv.DoiTrangThai(ql.MaTK));

        tx.Rollback();
    }

    // ============ TK-19: thao tác quản lý khi không phải QL → từ chối ============

    [Fact]
    [Trait("Luat", "TK-19")]
    public void GiaSu_TaoTaiKhoan_BiTuChoi()
    {
        var dv = TaoDichVu(GiaSu(10, 99));
        Assert.Throws<UnauthorizedAccessException>(
            () => dv.TaoTaiKhoan("test_gstaotk", Mk, Constants.VaiTro.QuanLy, null));
    }

    [Fact]
    [Trait("Luat", "TK-19")]
    public void GiaSu_DatLaiMatKhau_BiTuChoi()
    {
        var tk = TaoTaiKhoan("test_gs_reset", Mk);
        var dv = TaoDichVu(GiaSu(10, 99));
        Assert.Throws<UnauthorizedAccessException>(() => dv.DatLaiMatKhau(tk.MaTK, "x123456"));
    }
}
