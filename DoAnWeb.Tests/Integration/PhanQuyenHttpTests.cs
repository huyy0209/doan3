using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using DoAnWeb.Infrastructure;
using DoAnWeb.Services;
using System;

namespace DoAnWeb.Tests.Integration;

/// <summary>
/// Review #10: phân quyền thật qua HTTP (cookie + filter + [Authorize]) – gia sư không vào được trang quản lý,
/// chưa đăng nhập bị đẩy về đăng nhập, POST thiếu antiforgery bị 400, tài khoản khóa giữa phiên mất quyền.
/// </summary>
public class PhanQuyenHttpTests : HttpTestBase
{
    [Fact]
    public async Task Moc_khoi_phuc_thu_hoi_cookie_cua_phien_khac()
    {
        var phien1 = await DangNhapQL();
        var phien2 = await DangNhapQL();
        Assert.Equal(HttpStatusCode.OK, (await phien2.Get("/GiaSu")).Status);
        var backupFolder = TestDb.ThuMucBackupTest;
        var backupsBefore = new System.Collections.Generic.HashSet<string>(System.IO.Directory.EnumerateFiles(backupFolder, "QuanLyGiaSu_*.bak"), StringComparer.OrdinalIgnoreCase);
        using var scope = Factory.Services.CreateScope();
        var saoLuu = scope.ServiceProvider.GetRequiredService<SaoLuuService>();
        var tenTep = await saoLuu.SaoLuuAsync();
        try
        {
            var trangXacNhan = await phien1.Get("/SaoLuu/KhoiPhuc?tenTep=" + Uri.EscapeDataString(tenTep));
            Assert.Equal(HttpStatusCode.OK, trangXacNhan.Status);
            var form = trangXacNhan.Form("form[action$='/SaoLuu/KhoiPhuc']")
                .Dat("XacNhanTenTep", tenTep)
                .Dat("MatKhauHienTai", MatKhauTest);
            Assert.True((await phien1.Gui(form)).RedirectToi("/SaoLuu"));
        }
        finally
        {
            foreach (var file in System.IO.Directory.EnumerateFiles(backupFolder, "QuanLyGiaSu_*.bak").Where(file => !backupsBefore.Contains(file)))
                System.IO.File.Delete(file);
        }
        Assert.True((await phien2.Get("/GiaSu")).VeDangNhap);
    }

    public static IEnumerable<object[]> TrangChiQuanLy => new[]
    {
        "/GiaSu", "/GiaSu/Create", "/LopHoc", "/LopHoc/GhepLop", "/MonHoc", "/HocPhi", "/HocPhi/XuatCsv",
        "/TongQuan", "/CaiDat", "/QuanLyTaiKhoan", "/HocVien/Create", "/SaoLuu", "/NhatKy"
    }.Select(u => new object[] { u });

    [Theory]
    [MemberData(nameof(TrangChiQuanLy))]
    [Trait("Luat", "TK-19")]
    public async Task GiaSu_GET_trang_chi_quan_ly_bi_chan(string url)
    {
        var gs = await DangNhapGS();
        var t = await gs.Get(url);
        Assert.True(t.BiChan, $"GET {url} phải bị chặn, nhưng được {(int)t.Status} {t.Location}");
    }

    [Theory]
    [MemberData(nameof(TrangChiQuanLy))]
    [Trait("Luat", "TK-19")]
    public async Task Quan_ly_vao_duoc_trang_quan_ly(string url)
    {
        var ql = await DangNhapQL();
        var t = await ql.Get(url);
        Assert.Equal(HttpStatusCode.OK, t.Status);
    }

    [Fact]
    [Trait("Luat", "TK-19")]
    public async Task GiaSu_POST_cac_hanh_dong_quan_ly_bi_chan_va_khong_doi_du_lieu()
    {
        var hv = TaoHocVien("ITG_hv_quyen");
        var gsKhac = TaoGiaSu(TenItg(), SdtTest());
        var gs = await DangNhapGS();

        var cacPost = new (string Url, KeyValuePair<string, string>[] Fields)[]
        {
            ("/GiaSu/Create", new KeyValuePair<string, string>[] { new("HoTen", TenHoSoItg()), new("SDT", SdtTest()), new("ChuyenMon", "Toán") }),
            ($"/GiaSu/Edit/{gsKhac.MaGS}", new KeyValuePair<string, string>[] { new("HoTen", TenHoSoItg()), new("SDT", SdtTest()), new("ChuyenMon", "Toán") }),
            ($"/GiaSu/Delete/{gsKhac.MaGS}", new KeyValuePair<string, string>[0]),
            ("/HocVien/Create", new KeyValuePair<string, string>[] { new("HoTen", TenHoSoItg()), new("SDT", SdtTest()) }),
            ($"/HocVien/Edit/{hv.MaHV}", new KeyValuePair<string, string>[] { new("HoTen", TenHoSoItg()), new("SDT", SdtTest()) }),
            ($"/HocVien/Delete/{hv.MaHV}", new KeyValuePair<string, string>[0]),
            ("/LopHoc/GhepLop", new KeyValuePair<string, string>[] { new("MaHV", hv.MaHV.ToString()), new("MaGS", GS.MaGS.ToString()) }),
            ("/LopHoc/Huy/1", new KeyValuePair<string, string>[0]),
            ("/LopHoc/Delete/1", new KeyValuePair<string, string>[0]),
            ("/MonHoc/Luu", new KeyValuePair<string, string>[] { new("TenMon", "ITG_mon_quyen"), new("LoaiMon", "Chính") }),
            ("/MonHoc/Xoa", new KeyValuePair<string, string>[] { new("maMon", "1") }),
            ("/HocPhi/ThemThanhToan", new KeyValuePair<string, string>[] { new("ThanhToan.MaLop", "1"), new("ThanhToan.SoTien", "1000") }),
            ("/HocPhi/XoaThanhToan", new KeyValuePair<string, string>[] { new("maTT", "1"), new("maLop", "1") }),
            ("/CaiDat", new KeyValuePair<string, string>[] { new("Ten", "ITG_trung_tam") }),
            ("/QuanLyTaiKhoan", new KeyValuePair<string, string>[] { new("TaoMoi.VaiTro", "QuanLy"), new("TaoMoi.TenDangNhap", "itg_gs_tao"), new("TaoMoi.MatKhau", "Mk123456") }),
            ($"/QuanLyTaiKhoan/DoiTrangThai/{QL.MaTK}", new KeyValuePair<string, string>[0]),
            ("/QuanLyTaiKhoan/DatLaiMatKhau", new KeyValuePair<string, string>[] { new("MaTK", QL.MaTK.ToString()), new("MatKhauMoi", "Hack123456"), new("XacNhan", "Hack123456") }),
        };

        foreach (var (url, fields) in cacPost)
        {
            var t = await gs.Post(url, fields);
            Assert.True(t.BiChan, $"POST {url} phải bị chặn, nhưng được {(int)t.Status} {t.Location}");
        }

        using var db = Moi();
        Assert.True(db.GiaSus.Any(g => g.MaGS == gsKhac.MaGS), "gia sư không được bị xóa");
        Assert.True(db.HocViens.Any(h => h.MaHV == hv.MaHV && h.HoTen == "ITG_hv_quyen"), "học viên không được bị sửa/xóa");
        Assert.False(db.MonHocs.Any(m => m.TenMon == "ITG_mon_quyen"));
        Assert.False(db.TaiKhoans.Any(t => t.TenDangNhap == "itg_gs_tao"));
        Assert.True(db.TaiKhoans.Single(t => t.MaTK == QL.MaTK).HoatDong, "tài khoản quản lý không được bị khóa");
        Assert.True(Doan.Giong(QL.MatKhauHash, db.TaiKhoans.Single(t => t.MaTK == QL.MaTK).MatKhauHash), "mật khẩu quản lý không được bị đổi");
        Assert.False(db.LopHocs.Any(l => l.MaHV == hv.MaHV), "không được tạo lớp");
    }

    [Fact]
    [Trait("Luat", "TK-19")]
    public async Task GiaSu_xem_ket_qua_hoc_vien_cua_nguoi_khac_bi_403()
    {
        var hvCuaToi = TaoHocVien("ITG_hv_cuatoi");
        var hvNguoiKhac = TaoHocVien("ITG_hv_nguoikhac");
        TaoLop(GS, hvCuaToi);
        TaoLop(GSKhac, hvNguoiKhac);
        var gs = await DangNhapGS();

        Assert.Equal(HttpStatusCode.OK, (await gs.Get($"/HocTap/KetQua/{hvCuaToi.MaHV}")).Status);
        Assert.Equal(HttpStatusCode.OK, (await gs.Get($"/HocTap/PhieuBao/{hvCuaToi.MaHV}")).Status);

        Assert.Equal(HttpStatusCode.Forbidden, (await gs.Get($"/HocTap/KetQua/{hvNguoiKhac.MaHV}")).Status);
        Assert.Equal(HttpStatusCode.Forbidden, (await gs.Get($"/HocTap/PhieuBao/{hvNguoiKhac.MaHV}")).Status);
        var post = await gs.Post($"/HocTap/KetQua/{hvNguoiKhac.MaHV}", new KeyValuePair<string, string>[0]);
        Assert.Equal(HttpStatusCode.Forbidden, post.Status);
        var huy = await gs.Post("/HocTap/HuyHocBu", new KeyValuePair<string, string>[] { new("maBuoiBu", "1"), new("maHV", hvNguoiKhac.MaHV.ToString()) });
        Assert.Equal(HttpStatusCode.Forbidden, huy.Status);
        // Học viên chi tiết của người khác cũng bị chặn
        Assert.True((await gs.Get($"/HocVien/Details/{hvNguoiKhac.MaHV}")).BiChan);
    }

    [Fact]
    [Trait("Luat", "TK-19")]
    public async Task Quan_ly_xem_duoc_ket_qua_moi_hoc_vien()
    {
        var hv = TaoHocVien("ITG_hv_ql_xem");
        TaoLop(GSKhac, hv);
        var ql = await DangNhapQL();
        Assert.Equal(HttpStatusCode.OK, (await ql.Get($"/HocTap/KetQua/{hv.MaHV}")).Status);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/GiaSu")]
    [InlineData("/LopHoc")]
    [InlineData("/HocVien")]
    [InlineData("/HocPhi")]
    [InlineData("/DiemDanh")]
    [InlineData("/HocTap")]
    [InlineData("/Lich")]
    [InlineData("/TongQuan")]
    [InlineData("/CaiDat")]
    [InlineData("/QuanLyTaiKhoan")]
    [InlineData("/TaiKhoan/DoiMatKhau")]
    public async Task Chua_dang_nhap_chuyen_ve_trang_dang_nhap(string url)
    {
        var t = await Khach().Get(url);
        Assert.True(t.VeDangNhap, $"GET {url} chưa đăng nhập phải về trang đăng nhập, nhưng được {(int)t.Status} {t.Location}");
    }

    [Fact]
    public async Task Gia_su_bi_chan_trang_sao_luu_va_quan_ly_xem_duoc()
    {
        var gs = await DangNhapGS();
        Assert.True((await gs.Get("/SaoLuu")).BiChan);
        Assert.True((await gs.Get("/SaoLuu/KhoiPhuc?tenTep=QuanLyGiaSu_20261115_230000.bak")).BiChan);

        var ql = await DangNhapQL();
        var trang = await ql.Get("/SaoLuu");
        Assert.Equal(HttpStatusCode.OK, trang.Status);
        Assert.Contains("Sao lưu ngay", trang.Chu);
        Assert.Contains("Các file sao lưu", trang.Chu);
    }

    [Fact]
    public async Task Chua_dang_nhap_POST_bi_chuyen_ve_dang_nhap_hoac_chan()
    {
        var khach = Khach();
        // Lấy token từ trang đăng nhập (công khai) để chỉ kiểm tra phần xác thực, không kiểm tra antiforgery
        var token = (await khach.Get("/TaiKhoan/DangNhap")).Token;
        var t = await khach.Client.PostAsync("/GiaSu/Delete/1", new FormUrlEncodedContent(
            new[] { new KeyValuePair<string, string>("__RequestVerificationToken", token) }));
        Assert.True((int)t.StatusCode is 302 or 400 or 401 or 403, $"được {(int)t.StatusCode}");
        using var db = Moi();
        Assert.NotEqual(0, db.GiaSus.Count());
    }

    [Theory]
    [InlineData("/GiaSu/Create")]
    [InlineData("/HocVien/Create")]
    [InlineData("/LopHoc/GhepLop")]
    [InlineData("/MonHoc/Luu")]
    [InlineData("/HocPhi/ThemThanhToan")]
    [InlineData("/CaiDat")]
    [InlineData("/QuanLyTaiKhoan")]
    [InlineData("/QuanLyTaiKhoan/DatLaiMatKhau")]
    [InlineData("/DiemDanh/Luu")]
    [InlineData("/TaiKhoan/DoiMatKhau")]
    [InlineData("/TaiKhoan/DangXuat")]
    public async Task POST_thieu_antiforgery_bi_400(string url)
    {
        var ql = await DangNhapQL();
        var t = await ql.Post(url, new KeyValuePair<string, string>[] { new("TenMon", "ITG_khong_token") }, kemToken: false);
        Assert.Equal(HttpStatusCode.BadRequest, t.Status);
        using var db = Moi();
        Assert.False(db.MonHocs.Any(m => m.TenMon == "ITG_khong_token"));
    }

    [Fact]
    public async Task POST_dang_nhap_thieu_antiforgery_bi_400()
    {
        var t = await Khach().Post("/TaiKhoan/DangNhap",
            new KeyValuePair<string, string>[] { new("TenDangNhap", QL.TenDangNhap), new("MatKhau", MatKhauTest) }, kemToken: false);
        Assert.Equal(HttpStatusCode.BadRequest, t.Status);
    }

    [Fact]
    [Trait("Luat", "TK-08")]
    public async Task Tai_khoan_bi_khoa_giua_phien_mat_quyen_ngay()
    {
        var gs = await DangNhapGS();
        Assert.Equal(HttpStatusCode.OK, (await gs.Get("/DiemDanh")).Status);   // đưa trạng thái vào cache

        var ql = await DangNhapQL();
        var khoa = await ql.Post($"/QuanLyTaiKhoan/DoiTrangThai/{TKGS.MaTK}", new KeyValuePair<string, string>[0]);
        Assert.True(khoa.RedirectToi("/QuanLyTaiKhoan"));
        Assert.Contains("Đã khóa", (await ql.Theo(khoa)).ThongBaoThanhCong);

        var sau = await gs.Get("/DiemDanh");
        Assert.True(sau.VeDangNhap, $"sau khi bị khóa phải về đăng nhập, nhưng được {(int)sau.Status} {sau.Location}");
        // đăng nhập lại cũng bị từ chối
        var lai = await Khach().DangNhap(TKGS.TenDangNhap, MatKhauTest);
        Assert.Equal(HttpStatusCode.OK, lai.Status);
        Assert.Contains("Tài khoản đã bị khóa", lai.Chu);

        // mở khóa → đăng nhập lại được
        await ql.Post($"/QuanLyTaiKhoan/DoiTrangThai/{TKGS.MaTK}", new KeyValuePair<string, string>[0]);
        var moLai = await Khach().DangNhap(TKGS.TenDangNhap, MatKhauTest);
        Assert.True(moLai.RedirectToi("/"));
    }

    [Fact]
    [Trait("Luat", "TK-08")]
    public async Task Tai_khoan_bi_khoa_thang_trong_CSDL_truoc_request_dau_cung_mat_quyen()
    {
        var gs = await DangNhapGS();
        using (var db = Moi())
        {
            db.TaiKhoans.Single(t => t.MaTK == TKGS.MaTK).HoatDong = false;
            db.SaveChanges();
        }
        Assert.True((await gs.Get("/DiemDanh")).VeDangNhap);
    }

    [Fact]
    [Trait("Luat", "TK-17")]
    public async Task Doi_vai_tro_giua_phien_lam_cookie_cu_het_hieu_luc()
    {
        var gs = await DangNhapGS();
        using (var db = Moi())
        {
            var tk = db.TaiKhoans.Single(t => t.MaTK == TKGS.MaTK);
            tk.VaiTro = "QuanLy";
            tk.MaGS = null;
            db.SaveChanges();
        }
        // Cookie còn claim GiaSu → không được dùng tiếp để vào trang quản lý
        var t = await gs.Get("/GiaSu");
        Assert.True(t.BiChan, $"được {(int)t.Status} {t.Location}");
    }
}

/// <summary>So sánh nhỏ dùng chung trong test</summary>
internal static class Doan
{
    public static bool Giong(string a, string b) => string.Equals(a, b, System.StringComparison.Ordinal);
}
