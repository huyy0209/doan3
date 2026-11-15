using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DoAnWeb.Common;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>Review #2: tạo tài khoản, đặt lại mật khẩu, khóa/mở, đổi mật khẩu của mình (kể cả bắt đổi mật khẩu mặc định).</summary>
public class TaiKhoanHttpTests : HttpTestBase
{
    private static KeyValuePair<string, string>[] Khong => Array.Empty<KeyValuePair<string, string>>();

    // ---------------- Quản lý tài khoản ----------------

    [Fact]
    [Trait("Luat", "TK-15")]
    public async Task Tao_tai_khoan_gia_su_ghi_DB_va_dang_nhap_duoc()
    {
        var ql = await DangNhapQL();
        string ten = "itg_new" + Guid.NewGuid().ToString("N").Substring(0, 6);

        var form = (await ql.Get("/QuanLyTaiKhoan")).Form("form[action$='/QuanLyTaiKhoan']")
            .Dat("TaoMoi.VaiTro", "GiaSu").Dat("TaoMoi.MaGS", GSKhac.MaGS.ToString())
            .Dat("TaoMoi.TenDangNhap", ten.ToUpperInvariant()).Dat("TaoMoi.MatKhau", "MatKhau789");
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi("/QuanLyTaiKhoan"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Contains($"Đã tạo tài khoản {ten}", (await ql.Theo(kq)).ThongBaoThanhCong);   // TK-04: lưu chữ thường

        using (var db = Moi())
        {
            var tk = db.TaiKhoans.Single(t => t.TenDangNhap == ten);
            Assert.Equal("GiaSu", tk.VaiTro);
            Assert.Equal(GSKhac.MaGS, tk.MaGS);
            Assert.True(tk.HoatDong);
            Assert.True(MatKhau.KiemTra("MatKhau789", tk.MatKhauHash));
        }
        var moi = await Khach().DangNhap(ten, "MatKhau789");
        Assert.True(moi.RedirectToi("/"));
    }

    [Fact]
    [Trait("Luat", "TK-15")]
    public async Task Tao_tai_khoan_quan_ly_khong_gan_gia_su()
    {
        var ql = await DangNhapQL();
        string ten = "itg_ql2" + Guid.NewGuid().ToString("N").Substring(0, 6);

        var form = (await ql.Get("/QuanLyTaiKhoan")).Form("form[action$='/QuanLyTaiKhoan']")
            .Dat("TaoMoi.VaiTro", "QuanLy").Dat("TaoMoi.MaGS", "")
            .Dat("TaoMoi.TenDangNhap", ten).Dat("TaoMoi.MatKhau", "MatKhau789");
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi("/QuanLyTaiKhoan"), $"được {(int)kq.Status}: {kq.Chu}");
        using var db = Moi();
        var tk = db.TaiKhoans.Single(t => t.TenDangNhap == ten);
        Assert.Equal("QuanLy", tk.VaiTro);
        Assert.Null(tk.MaGS);
    }

    [Fact]
    [Trait("Luat", "TK-15")]
    public async Task Tao_tai_khoan_trung_ten_hien_loi_va_khong_them()
    {
        var ql = await DangNhapQL();
        var form = (await ql.Get("/QuanLyTaiKhoan")).Form("form[action$='/QuanLyTaiKhoan']")
            .Dat("TaoMoi.VaiTro", "GiaSu").Dat("TaoMoi.MaGS", GSKhac.MaGS.ToString())
            .Dat("TaoMoi.TenDangNhap", QL.TenDangNhap).Dat("TaoMoi.MatKhau", "MatKhau789");
        var kq = await ql.Gui(form);

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        using var db = Moi();
        Assert.Equal(1, db.TaiKhoans.Count(t => t.TenDangNhap == QL.TenDangNhap));
        Assert.False(db.TaiKhoans.Any(t => t.MaGS == GSKhac.MaGS));
    }

    [Fact]
    [Trait("Luat", "TK-15")]
    public async Task Tao_tai_khoan_gia_su_thieu_MaGS_bi_validation_chan()
    {
        var ql = await DangNhapQL();
        string ten = "itg_thieu" + Guid.NewGuid().ToString("N").Substring(0, 6);
        var form = (await ql.Get("/QuanLyTaiKhoan")).Form("form[action$='/QuanLyTaiKhoan']")
            .Dat("TaoMoi.VaiTro", "GiaSu").Dat("TaoMoi.MaGS", "")
            .Dat("TaoMoi.TenDangNhap", ten).Dat("TaoMoi.MatKhau", "MatKhau789");
        var kq = await ql.Gui(form);

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.Contains("Chọn gia sư", kq.Chu);
        using var db = Moi();
        Assert.False(db.TaiKhoans.Any(t => t.TenDangNhap == ten));
    }

    [Fact]
    [Trait("Luat", "TK-16")]
    public async Task Dat_lai_mat_khau_doi_hash_go_tam_khoa_va_dang_nhap_bang_mat_khau_moi()
    {
        using (var db = Moi())
        {
            var tk = db.TaiKhoans.Single(t => t.MaTK == TKGS.MaTK);
            tk.SoLanSai = 3;
            tk.KhoaDen = DateTime.Now.AddMinutes(10);
            db.SaveChanges();
        }
        var ql = await DangNhapQL();

        var form = (await ql.Get($"/QuanLyTaiKhoan/DatLaiMatKhau/{TKGS.MaTK}"))
            .Form($"form[action$='/QuanLyTaiKhoan/DatLaiMatKhau/{TKGS.MaTK}']")
            .Dat("MatKhauMoi", "DatLai999").Dat("XacNhan", "DatLai999");
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi("/QuanLyTaiKhoan"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Contains($"Đã đặt lại mật khẩu cho {TKGS.TenDangNhap}", (await ql.Theo(kq)).ThongBaoThanhCong);

        using (var db = Moi())
        {
            var tk = db.TaiKhoans.Single(t => t.MaTK == TKGS.MaTK);
            Assert.True(MatKhau.KiemTra("DatLai999", tk.MatKhauHash));
            Assert.False(MatKhau.KiemTra(MatKhauTest, tk.MatKhauHash));
            Assert.Equal(0, tk.SoLanSai);
            Assert.Null(tk.KhoaDen);
        }
        Assert.Equal(HttpStatusCode.OK, (await Khach().DangNhap(TKGS.TenDangNhap, MatKhauTest)).Status);   // mật khẩu cũ bị từ chối
        Assert.True((await Khach().DangNhap(TKGS.TenDangNhap, "DatLai999")).RedirectToi("/"));
    }

    [Fact]
    [Trait("Luat", "TK-16")]
    public async Task Dat_lai_mat_khau_xac_nhan_khong_khop_khong_doi()
    {
        var ql = await DangNhapQL();
        var form = (await ql.Get($"/QuanLyTaiKhoan/DatLaiMatKhau/{TKGS.MaTK}"))
            .Form($"form[action$='/QuanLyTaiKhoan/DatLaiMatKhau/{TKGS.MaTK}']")
            .Dat("MatKhauMoi", "DatLai999").Dat("XacNhan", "KhacHan123");
        var kq = await ql.Gui(form);

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.Contains("không khớp", kq.Chu);
        using var db = Moi();
        Assert.True(MatKhau.KiemTra(MatKhauTest, db.TaiKhoans.Single(t => t.MaTK == TKGS.MaTK).MatKhauHash));
    }

    [Fact]
    [Trait("Luat", "TK-17")]
    public async Task Khoa_roi_mo_khoa_tai_khoan_qua_form_modal()
    {
        var ql = await DangNhapQL();

        var khoa = await ql.Gui((await ql.Get("/QuanLyTaiKhoan")).Form("form[action$='/QuanLyTaiKhoan/DoiTrangThai']")
            .Dat("id", TKGS.MaTK.ToString()));
        Assert.True(khoa.RedirectToi("/QuanLyTaiKhoan"));
        Assert.Equal("Đã khóa tài khoản.", (await ql.Theo(khoa)).ThongBaoThanhCong);
        using (var db = Moi()) Assert.False(db.TaiKhoans.Single(t => t.MaTK == TKGS.MaTK).HoatDong);

        var mo = await ql.Gui((await ql.Get("/QuanLyTaiKhoan")).Form("form[action$='/QuanLyTaiKhoan/DoiTrangThai']")
            .Dat("id", TKGS.MaTK.ToString()));
        Assert.Equal("Đã mở khóa tài khoản.", (await ql.Theo(mo)).ThongBaoThanhCong);
        using (var db = Moi()) Assert.True(db.TaiKhoans.Single(t => t.MaTK == TKGS.MaTK).HoatDong);
    }

    [Fact]
    [Trait("Luat", "TK-17")]
    public async Task Khong_khoa_duoc_chinh_minh()
    {
        var ql = await DangNhapQL();
        var kq = await ql.Gui((await ql.Get("/QuanLyTaiKhoan")).Form("form[action$='/QuanLyTaiKhoan/DoiTrangThai']")
            .Dat("id", QL.MaTK.ToString()));

        Assert.True(kq.RedirectToi("/QuanLyTaiKhoan"));
        Assert.Contains("Không thể khóa tài khoản đang đăng nhập", (await ql.Theo(kq)).ThongBaoLoi);
        using var db = Moi();
        Assert.True(db.TaiKhoans.Single(t => t.MaTK == QL.MaTK).HoatDong);
    }

    // ---------------- Đổi mật khẩu của mình ----------------

    [Fact]
    [Trait("Luat", "TK-14")]
    public async Task Doi_mat_khau_cua_minh_thanh_cong()
    {
        var gs = await DangNhapGS();
        var form = (await gs.Get("/TaiKhoan/DoiMatKhau")).Form("form[action$='/TaiKhoan/DoiMatKhau']")
            .Dat("MatKhauCu", MatKhauTest).Dat("MatKhauMoi", "MoiMoi456").Dat("XacNhan", "MoiMoi456");
        var kq = await gs.Gui(form);

        Assert.True(kq.RedirectToi("/"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Contains("Đã đổi mật khẩu", (await gs.GetTheoRedirect(kq.Location)).Chu);
        using (var db = Moi())
            Assert.True(MatKhau.KiemTra("MoiMoi456", db.TaiKhoans.Single(t => t.MaTK == TKGS.MaTK).MatKhauHash));
        Assert.Equal(HttpStatusCode.OK, (await Khach().DangNhap(TKGS.TenDangNhap, MatKhauTest)).Status);
        Assert.True((await Khach().DangNhap(TKGS.TenDangNhap, "MoiMoi456")).RedirectToi("/"));
    }

    [Theory]
    [InlineData("SaiSai999", "MoiMoi456", "MoiMoi456", "mật khẩu")]           // sai mật khẩu hiện tại
    [InlineData("Mk123456", "Mk123456", "Mk123456", "")]                        // mới trùng cũ (TK-14)
    [InlineData("Mk123456", "admin123", "admin123", "")]                        // mới = mật khẩu mặc định (TK-14)
    [InlineData("Mk123456", "abc", "abc", "")]                                   // quá ngắn (TK-12)
    [InlineData("Mk123456", "MoiMoi456", "KhacHan456", "không khớp")]           // xác nhận lệch
    [Trait("Luat", "TK-14")]
    public async Task Doi_mat_khau_sai_thi_hien_loi_va_khong_doi(string cu, string moi, string xacNhan, string cauLoi)
    {
        var gs = await DangNhapGS();
        var form = (await gs.Get("/TaiKhoan/DoiMatKhau")).Form("form[action$='/TaiKhoan/DoiMatKhau']")
            .Dat("MatKhauCu", cu).Dat("MatKhauMoi", moi).Dat("XacNhan", xacNhan);
        var kq = await gs.Gui(form);

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.Contains(cauLoi, kq.Chu);
        using var db = Moi();
        Assert.True(MatKhau.KiemTra(MatKhauTest, db.TaiKhoans.Single(t => t.MaTK == TKGS.MaTK).MatKhauHash));
    }

    [Fact]
    [Trait("Luat", "TK-11")]
    public async Task Dang_nhap_bang_mat_khau_mac_dinh_bi_bat_doi_truoc_khi_vao()
    {
        var tk = TaoTaiKhoan("itg_def" + Guid.NewGuid().ToString("N").Substring(0, 6), TaiKhoanService.MatKhauMacDinh);
        var t = Khach();
        var dn = await t.DangNhap(tk.TenDangNhap, TaiKhoanService.MatKhauMacDinh);
        Assert.True(dn.RedirectToi("/TaiKhoan/DoiMatKhau"), $"được {(int)dn.Status} {dn.Location}");

        // Mọi trang khác bị đẩy về trang đổi mật khẩu
        foreach (var url in new[] { "/", "/GiaSu", "/LopHoc", "/QuanLyTaiKhoan" })
            Assert.True((await t.Get(url)).RedirectToi("/TaiKhoan/DoiMatKhau"), $"{url} phải chuyển về đổi mật khẩu");

        // Form ẩn ô mật khẩu cũ; mật khẩu mới không được là mặc định
        var trang = await t.Get("/TaiKhoan/DoiMatKhau");
        Assert.Equal(HttpStatusCode.OK, trang.Status);
        var form = trang.Form("form[action$='/TaiKhoan/DoiMatKhau']");
        Assert.Null(form.Lay("MatKhauCu"));
        Assert.Equal("True", form.Lay("BatBuoc"), ignoreCase: true);

        var trungMacDinh = await t.Gui(trang.Form("form[action$='/TaiKhoan/DoiMatKhau']")
            .Dat("MatKhauMoi", "admin123").Dat("XacNhan", "admin123"));
        Assert.Equal(HttpStatusCode.OK, trungMacDinh.Status);
        Assert.True((await t.Get("/GiaSu")).RedirectToi("/TaiKhoan/DoiMatKhau"));

        // Đổi sang mật khẩu riêng → vào được
        var ok = await t.Gui(trang.Form("form[action$='/TaiKhoan/DoiMatKhau']")
            .Dat("MatKhauMoi", "Rieng12345").Dat("XacNhan", "Rieng12345"));
        Assert.True(ok.RedirectToi("/"), $"được {(int)ok.Status}: {ok.Chu}");
        Assert.Equal(HttpStatusCode.OK, (await t.Get("/GiaSu")).Status);
        using var db = Moi();
        Assert.True(MatKhau.KiemTra("Rieng12345", db.TaiKhoans.Single(x => x.MaTK == tk.MaTK).MatKhauHash));
    }

    [Fact]
    [Trait("Luat", "TK-11")]
    public async Task Tai_khoan_bat_doi_mat_khau_van_dang_xuat_duoc()
    {
        var tk = TaoTaiKhoan("itg_def" + Guid.NewGuid().ToString("N").Substring(0, 6), TaiKhoanService.MatKhauMacDinh);
        var t = Khach();
        await t.DangNhap(tk.TenDangNhap, TaiKhoanService.MatKhauMacDinh);

        var kq = await t.Post("/TaiKhoan/DangXuat", Khong);
        Assert.True(kq.RedirectToi("/TaiKhoan/DangNhap"), $"được {(int)kq.Status} {kq.Location}");
        Assert.True((await t.Get("/")).VeDangNhap);
    }

    [Fact]
    public async Task Dang_xuat_xoa_phien_va_quay_ve_dang_nhap()
    {
        var gs = await DangNhapGS();
        Assert.Equal(HttpStatusCode.OK, (await gs.Get("/DiemDanh")).Status);

        var kq = await gs.Post("/TaiKhoan/DangXuat", Khong);
        Assert.True(kq.RedirectToi("/TaiKhoan/DangNhap"));
        Assert.True((await gs.Get("/DiemDanh")).VeDangNhap);
    }

    [Fact]
    [Trait("Luat", "TK-06")]
    public async Task Dang_nhap_sai_hien_cung_mot_cau_loi()
    {
        var saiMk = await Khach().DangNhap(QL.TenDangNhap, "SaiSai999");
        var saiTen = await Khach().DangNhap("itg_khong_ton_tai", "SaiSai999");
        Assert.Equal(HttpStatusCode.OK, saiMk.Status);
        Assert.Contains("Sai tên đăng nhập hoặc mật khẩu.", saiMk.Chu);
        Assert.Contains("Sai tên đăng nhập hoặc mật khẩu.", saiTen.Chu);
    }

    [Fact]
    public async Task Dang_nhap_returnUrl_cuc_bo_duoc_theo_returnUrl_ngoai_bi_bo_qua()
    {
        var t = Khach();
        var form = (await t.Get("/TaiKhoan/DangNhap?returnUrl=%2FGiaSu")).Form("form[action*='/TaiKhoan/DangNhap']")
            .Dat("TenDangNhap", QL.TenDangNhap).Dat("MatKhau", MatKhauTest);
        Assert.True((await t.Gui(form)).RedirectToi("/GiaSu"));

        var t2 = Khach();
        var form2 = (await t2.Get("/TaiKhoan/DangNhap")).Form("form[action*='/TaiKhoan/DangNhap']")
            .Dat("TenDangNhap", QL.TenDangNhap).Dat("MatKhau", MatKhauTest).Dat("ReturnUrl", "https://evil.example/x");
        Assert.True((await t2.Gui(form2)).RedirectToi("/"));
    }
}
