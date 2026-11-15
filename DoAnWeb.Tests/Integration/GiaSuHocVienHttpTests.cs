using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>Review #2: luồng ghi gia sư và học viên (tạo / sửa / xóa) – gửi form như trình duyệt, kiểm redirect, thông báo, DB.</summary>
public class GiaSuHocVienHttpTests : HttpTestBase
{
    // ---------------- Gia sư ----------------

    [Fact]
    [Trait("Luat", "GS-01")]
    public async Task GiaSu_tao_moi_redirect_chi_tiet_va_ghi_DB()
    {
        var ql = await DangNhapQL();
        string ten = TenHoSoItg(), sdt = SdtTest();

        var form = (await ql.Get("/GiaSu/Create")).Form("form[action$='/GiaSu/Create']")
            .Dat("HoTen", ten).Dat("SDT", sdt).Dat("ChuyenMon", "Toán").Dat("Email", "itg@example.com")
            .Dat("KinhNghiem", "3").Dat("GhiChu", "ghi chú test");
        var kq = await ql.Gui(form);

        Assert.True(kq.LaRedirect, $"phải redirect, được {(int)kq.Status}: {kq.Chu}");
        Assert.StartsWith("/GiaSu/Details/", kq.Location);
        Assert.Contains($"Đã thêm gia sư {ten}", (await ql.Theo(kq)).ThongBaoThanhCong);

        using var db = Moi();
        var gs = db.GiaSus.Single(g => g.SDT == sdt);
        Assert.Equal(ten, gs.HoTen);
        Assert.Equal("Toán", gs.ChuyenMon);
        Assert.Equal("itg@example.com", gs.Email);
        Assert.Equal(3, gs.KinhNghiem);
        Assert.Equal("Rảnh", gs.TrangThai);
        Assert.EndsWith("/" + gs.MaGS, kq.Location);
    }

    [Fact]
    [Trait("Luat", "GS-05")]
    public async Task GiaSu_tao_trung_SDT_hien_loi_va_khong_them()
    {
        var ql = await DangNhapQL();
        var form = (await ql.Get("/GiaSu/Create")).Form("form[action$='/GiaSu/Create']")
            .Dat("HoTen", TenHoSoItg()).Dat("SDT", GS.SDT).Dat("ChuyenMon", "Toán");
        var kq = await ql.Gui(form);

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.Contains("đã được dùng", kq.Chu);
        using var db = Moi();
        Assert.Equal(1, db.GiaSus.Count(g => g.SDT == GS.SDT));
    }

    [Fact]
    [Trait("Luat", "GS-01")]
    public async Task GiaSu_tao_ten_co_so_bi_chan_boi_validation()
    {
        var ql = await DangNhapQL();
        string sdt = SdtTest();
        var form = (await ql.Get("/GiaSu/Create")).Form("form[action$='/GiaSu/Create']")
            .Dat("HoTen", "Itg Gia Su 5").Dat("SDT", sdt).Dat("ChuyenMon", "Toán");
        var kq = await ql.Gui(form);

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.Contains("không được chứa chữ số", kq.Chu);
        using var db = Moi();
        Assert.False(db.GiaSus.Any(g => g.SDT == sdt));
    }

    [Fact]
    [Trait("Luat", "GS-07")]
    public async Task GiaSu_sua_ho_so_va_tam_nghi_ghi_DB()
    {
        var ql = await DangNhapQL();
        string tenMoi = TenHoSoItg();

        var form = (await ql.Get($"/GiaSu/Edit/{GSKhac.MaGS}")).Form($"form[action$='/GiaSu/Edit/{GSKhac.MaGS}']")
            .Dat("HoTen", tenMoi).Dat("ChuyenMon", "Văn").Dat("GhiChu", "đã sửa").Dat("TamNghi", "true");
        var kq = await ql.Gui(form);

        Assert.True(kq.LaRedirect, $"phải redirect, được {(int)kq.Status}: {kq.Chu}");
        Assert.Equal($"/GiaSu/Details/{GSKhac.MaGS}", kq.Location);
        Assert.Contains($"Đã lưu hồ sơ {tenMoi}", (await ql.Theo(kq)).ThongBaoThanhCong);

        using var db = Moi();
        var gs = db.GiaSus.Single(g => g.MaGS == GSKhac.MaGS);
        Assert.Equal(tenMoi, gs.HoTen);
        Assert.Equal("Văn", gs.ChuyenMon);
        Assert.Equal("đã sửa", gs.GhiChu);
        Assert.Equal("Tạm nghỉ", gs.TrangThai);
    }

    [Fact]
    [Trait("Luat", "GS-06")]
    public async Task GiaSu_sua_chon_hai_ca_luu_ten_ngan()
    {
        var ql = await DangNhapQL();
        var trang = await ql.Get($"/GiaSu/Edit/{GSKhac.MaGS}");
        var form = trang.Form($"form[action$='/GiaSu/Edit/{GSKhac.MaGS}']").Dat("HoTen", TenHoSoItg()).Bo("CaNhanDay")
            .Dat("CaNhanDay", "Sáng");
        form.Fields.Add(new("CaNhanDay", "Tối"));
        var kq = await ql.Gui(form);

        Assert.True(kq.LaRedirect, $"được {(int)kq.Status}: {kq.Chu}");
        using var db = Moi();
        Assert.Equal("Sáng,Tối", db.GiaSus.Single(g => g.MaGS == GSKhac.MaGS).CaNhanDay);
    }

    [Fact]
    [Trait("Luat", "GS-09")]
    public async Task GiaSu_xoa_khong_lop_thanh_cong_va_xoa_kem_tai_khoan()
    {
        var gsXoa = TaoGiaSu(TenItg());
        var tk = TaoTaiKhoan("itg_xoa" + gsXoa.MaGS, "Mk123456", DoAnWeb.Common.Constants.VaiTro.GiaSu, gsXoa.MaGS);
        var ql = await DangNhapQL();

        var form = (await ql.Get("/GiaSu")).Form("form[action$='/GiaSu/Delete']").Dat("id", gsXoa.MaGS.ToString());
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi("/GiaSu"));
        Assert.Equal("Đã xóa gia sư.", (await ql.Theo(kq)).ThongBaoThanhCong);
        using var db = Moi();
        Assert.False(db.GiaSus.Any(g => g.MaGS == gsXoa.MaGS));
        Assert.False(db.TaiKhoans.Any(t => t.MaTK == tk.MaTK));
    }

    [Fact]
    [Trait("Luat", "GS-09")]
    public async Task GiaSu_xoa_khi_con_lop_hoat_dong_bi_chan()
    {
        var hv = TaoHocVien(TenItg());
        var gsCoLop = TaoGiaSu(TenItg());
        TaoLop(gsCoLop, hv);
        var ql = await DangNhapQL();

        var form = (await ql.Get("/GiaSu")).Form("form[action$='/GiaSu/Delete']").Dat("id", gsCoLop.MaGS.ToString());
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi("/GiaSu"));
        Assert.False(string.IsNullOrWhiteSpace((await ql.Theo(kq)).ThongBaoLoi));
        using var db = Moi();
        Assert.True(db.GiaSus.Any(g => g.MaGS == gsCoLop.MaGS));
    }

    // ---------------- Học viên ----------------

    [Fact]
    [Trait("Luat", "HV-01")]
    public async Task HocVien_tao_moi_redirect_chi_tiet_va_ghi_DB()
    {
        var ql = await DangNhapQL();
        string ten = TenHoSoItg(), sdt = SdtTest();

        var form = (await ql.Get("/HocVien/Create")).Form("form[action$='/HocVien/Create']")
            .Dat("HoTen", ten).Dat("SDT", sdt).Dat("TenPhuHuynh", "Phụ huynh Itg").Dat("TruongLop", "  THPT Itg  ")
            .Dat("TrinhDo", "Lớp 10").Dat("DiaChi", "");
        var kq = await ql.Gui(form);

        Assert.True(kq.LaRedirect, $"phải redirect, được {(int)kq.Status}: {kq.Chu}");
        Assert.StartsWith("/HocVien/Details/", kq.Location);
        Assert.Contains($"Đã thêm học viên {ten}", (await ql.Theo(kq)).ThongBaoThanhCong);

        using var db = Moi();
        var hv = db.HocViens.Single(h => h.SDT == sdt);
        Assert.Equal(ten, hv.HoTen);
        Assert.Equal("THPT Itg", hv.TruongLop);      // HV-04: trim
        Assert.Null(hv.DiaChi);                      // HV-04: rỗng → NULL
        Assert.Equal("Lớp 10", hv.TrinhDo);
    }

    [Fact]
    [Trait("Luat", "HV-02")]
    public async Task HocVien_tao_thieu_ca_hai_SDT_bi_chan()
    {
        var ql = await DangNhapQL();
        string ten = TenHoSoItg();
        var form = (await ql.Get("/HocVien/Create")).Form("form[action$='/HocVien/Create']")
            .Dat("HoTen", ten).Dat("SDT", "").Dat("SDTPhuHuynh", "");
        var kq = await ql.Gui(form);

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.Contains("ít nhất một số điện thoại", kq.Chu);
        using var db = Moi();
        Assert.False(db.HocViens.Any(h => h.HoTen == ten));
    }

    [Fact]
    [Trait("Luat", "HV-03")]
    public async Task HocVien_sua_ho_so_ghi_DB()
    {
        var hv = TaoHocVien(TenItg());
        var ql = await DangNhapQL();
        string tenMoi = TenHoSoItg();

        var form = (await ql.Get($"/HocVien/Edit/{hv.MaHV}")).Form($"form[action$='/HocVien/Edit/{hv.MaHV}']")
            .Dat("HoTen", tenMoi).Dat("SDTPhuHuynh", SdtTest()).Dat("GhiChu", "ghi chú mới");
        var kq = await ql.Gui(form);

        Assert.True(kq.LaRedirect, $"phải redirect, được {(int)kq.Status}: {kq.Chu}");
        Assert.Equal($"/HocVien/Details/{hv.MaHV}", kq.Location);
        Assert.Contains($"Đã lưu hồ sơ {tenMoi}", (await ql.Theo(kq)).ThongBaoThanhCong);

        using var db = Moi();
        var sau = db.HocViens.Single(h => h.MaHV == hv.MaHV);
        Assert.Equal(tenMoi, sau.HoTen);
        Assert.Equal("ghi chú mới", sau.GhiChu);
        Assert.NotNull(sau.SDTPhuHuynh);
    }

    [Fact]
    [Trait("Luat", "HV-05")]
    public async Task HocVien_xoa_khong_lop_thanh_cong()
    {
        var hv = TaoHocVien(TenItg());
        var ql = await DangNhapQL();

        var form = (await ql.Get("/HocVien")).Form("form[action$='/HocVien/Delete']").Dat("id", hv.MaHV.ToString());
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi("/HocVien"));
        Assert.Equal("Đã xóa học viên.", (await ql.Theo(kq)).ThongBaoThanhCong);
        using var db = Moi();
        Assert.False(db.HocViens.Any(h => h.MaHV == hv.MaHV));
    }

    [Fact]
    [Trait("Luat", "HV-05")]
    public async Task HocVien_xoa_khi_con_lop_bi_chan()
    {
        var hv = TaoHocVien(TenItg());
        TaoLop(GS, hv);
        var ql = await DangNhapQL();

        var form = (await ql.Get("/HocVien")).Form("form[action$='/HocVien/Delete']").Dat("id", hv.MaHV.ToString());
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi("/HocVien"));
        Assert.False(string.IsNullOrWhiteSpace((await ql.Theo(kq)).ThongBaoLoi));
        using var db = Moi();
        Assert.True(db.HocViens.Any(h => h.MaHV == hv.MaHV));
    }

    [Fact]
    public async Task HocVien_gia_su_xem_danh_sach_chi_thay_hoc_vien_cua_minh()
    {
        var hvToi = TaoHocVien(TenItg());
        var hvKhac = TaoHocVien(TenItg());
        TaoLop(GS, hvToi);
        TaoLop(GSKhac, hvKhac);
        var gs = await DangNhapGS();

        var trang = await gs.Get("/HocVien");
        Assert.Equal(HttpStatusCode.OK, trang.Status);
        Assert.Contains(hvToi.HoTen, trang.Chu);
        Assert.DoesNotContain(hvKhac.HoTen, trang.Chu);
    }
}
