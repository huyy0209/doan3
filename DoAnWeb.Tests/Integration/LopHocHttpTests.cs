using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>Review #2: ghép lớp → sửa lớp → hủy lớp → xóa lớp qua form HTTP, kiểm thông báo + DB.</summary>
public class LopHocHttpTests : HttpTestBase
{
    private readonly HocVien _hv;
    private readonly MonHocDanhMuc _mon;

    public LopHocHttpTests()
    {
        _hv = TaoHocVien(TenItg());
        _mon = TaoMon("ITG_mon" + Guid.NewGuid().ToString("N").Substring(0, 6));
    }

    private static string Ngay(int soNgayTuHomNay) => DateTime.Today.AddDays(soNgayTuHomNay).ToString("yyyy-MM-dd");

    /// <summary>Điền form ghép lớp như người dùng: chọn học viên, gia sư, môn, ca, ngày, học phí</summary>
    private static FormDuLieu DienGhepLop(Trang trang, int maHV, int maGS, string mon, string ca = Constants.CaHoc.Sang,
        int tu = 1, int den = 8, string hocPhi = "1.500.000", string tenLop = "")
    {
        return trang.Form("#form-ghep-lop")
            .Dat("MaHV", maHV.ToString()).Dat("MaGS", maGS.ToString()).Dat("MonHoc", mon)
            .Dat("CaHoc", ca).Dat("BatDau", Ngay(tu)).Dat("KetThuc", Ngay(den))
            .Dat("HocPhi", hocPhi).Dat("TenLop", tenLop);
    }

    private async Task<int> GhepLopThanhCong(TrinhDuyet ql, int maGS, int maHV = 0)
    {
        var form = DienGhepLop(await ql.Get("/LopHoc/GhepLop"), maHV == 0 ? _hv.MaHV : maHV, maGS, _mon.TenMon);
        var kq = await ql.Gui(form);
        Assert.True(kq.LaRedirect, $"ghép lớp phải redirect, được {(int)kq.Status}: {kq.Chu}");
        return int.Parse(kq.Location.Split('/').Last());
    }

    [Fact]
    [Trait("Luat", "LH-18")]
    public async Task Ghep_lop_thanh_cong_ghi_lop_va_dong_bo_trang_thai_gia_su()
    {
        var ql = await DangNhapQL();
        var form = DienGhepLop(await ql.Get("/LopHoc/GhepLop"), _hv.MaHV, GS.MaGS, _mon.TenMon);
        var kq = await ql.Gui(form);

        Assert.True(kq.LaRedirect, $"phải redirect, được {(int)kq.Status}: {kq.Chu}");
        Assert.StartsWith("/LopHoc/Details/", kq.Location);
        var chiTiet = await ql.Theo(kq);
        Assert.Equal(HttpStatusCode.OK, chiTiet.Status);
        Assert.Contains("Đã ghép lớp", chiTiet.ThongBaoThanhCong);

        using var db = Moi();
        var lop = db.LopHocs.Single(l => l.MaHV == _hv.MaHV);
        Assert.EndsWith("/" + lop.MaLop, kq.Location);
        Assert.Equal(GS.MaGS, lop.MaGS);
        Assert.Equal(_mon.TenMon, lop.MonHoc);
        Assert.Equal(1500000m, lop.HocPhi);
        Assert.Equal(Constants.CaHoc.Sang, lop.CaHoc);
        Assert.Equal(Constants.LopHocStatus.ChuaBatDau, lop.TrangThai);
        Assert.Equal(DateTime.Today.AddDays(1).AddHours(7), lop.ThoiGianBatDau);   // LH-09: giờ đầu ca
        Assert.Equal(DateTime.Today.AddDays(8).AddHours(9), lop.ThoiGianKetThuc);   // LH-09: giờ cuối ca
        Assert.False(string.IsNullOrWhiteSpace(lop.TenLop));                         // LH-07: tự gợi ý tên
        Assert.Contains("Sáng", lop.TenLop);
        Assert.Equal(Constants.GiaSuStatus.Ban, db.GiaSus.Single(g => g.MaGS == GS.MaGS).TrangThai);  // GS-08
    }

    [Fact]
    [Trait("Luat", "LH-12")]
    public async Task Ghep_lop_trung_lich_gia_su_bi_tu_choi_va_khong_tao_lop_thu_hai()
    {
        var ql = await DangNhapQL();
        await GhepLopThanhCong(ql, GS.MaGS);
        var hv2 = TaoHocVien(TenItg());

        var kq = await ql.Gui(DienGhepLop(await ql.Get("/LopHoc/GhepLop"), hv2.MaHV, GS.MaGS, _mon.TenMon, den: 5));

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.False(string.IsNullOrWhiteSpace(kq.Chu));
        using var db = Moi();
        Assert.Equal(1, db.LopHocs.Count(l => l.MaGS == GS.MaGS));
        Assert.False(db.LopHocs.Any(l => l.MaHV == hv2.MaHV));
    }

    [Fact]
    [Trait("Luat", "LH-02")]
    public async Task Ghep_lop_thieu_hoc_vien_hoac_hoc_phi_sai_hien_loi()
    {
        var ql = await DangNhapQL();
        var trang = await ql.Get("/LopHoc/GhepLop");

        var thieuHV = await ql.Gui(DienGhepLop(trang, _hv.MaHV, GS.MaGS, _mon.TenMon).Bo("MaHV"));
        Assert.Equal(HttpStatusCode.OK, thieuHV.Status);

        var hocPhiChu = await ql.Gui(DienGhepLop(trang, _hv.MaHV, GS.MaGS, _mon.TenMon, hocPhi: "abc"));
        Assert.Equal(HttpStatusCode.OK, hocPhiChu.Status);
        Assert.Contains("Học phí phải là số", hocPhiChu.Chu);

        var thieuCa = await ql.Gui(DienGhepLop(trang, _hv.MaHV, GS.MaGS, _mon.TenMon).Bo("CaHoc"));
        Assert.Equal(HttpStatusCode.OK, thieuCa.Status);

        using var db = Moi();
        Assert.False(db.LopHocs.Any(l => l.MaHV == _hv.MaHV));
    }

    [Fact]
    [Trait("Luat", "LH-03")]
    public async Task Ghep_lop_ngay_bat_dau_trong_qua_khu_bi_tu_choi()
    {
        var ql = await DangNhapQL();
        var kq = await ql.Gui(DienGhepLop(await ql.Get("/LopHoc/GhepLop"), _hv.MaHV, GS.MaGS, _mon.TenMon, tu: -5, den: 5));

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        using var db = Moi();
        Assert.False(db.LopHocs.Any(l => l.MaHV == _hv.MaHV));
    }

    [Fact]
    [Trait("Luat", "LH-19")]
    public async Task Sua_lop_doi_ten_hoc_phi_ngay_ket_thuc_va_gia_su()
    {
        var ql = await DangNhapQL();
        int maLop = await GhepLopThanhCong(ql, GS.MaGS);
        string tenMoi = "ITG_lop_moi" + Guid.NewGuid().ToString("N").Substring(0, 6);

        var form = (await ql.Get($"/LopHoc/Edit/{maLop}")).Form("#form-sua-lop")
            .Dat("TenLop", tenMoi).Dat("HocPhi", "2.000.000").Dat("KetThuc", Ngay(15)).Dat("MaGS", GSKhac.MaGS.ToString());
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi($"/LopHoc/Details/{maLop}"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Equal("Đã lưu thay đổi của lớp.", (await ql.Theo(kq)).ThongBaoThanhCong);

        using var db = Moi();
        var lop = db.LopHocs.Single(l => l.MaLop == maLop);
        Assert.Equal(tenMoi, lop.TenLop);
        Assert.Equal(2000000m, lop.HocPhi);
        Assert.Equal(GSKhac.MaGS, lop.MaGS);
        Assert.Equal(DateTime.Today.AddDays(15).AddHours(9), lop.ThoiGianKetThuc);                    // giữ giờ cuối ca
        Assert.Equal(Constants.GiaSuStatus.Ban, db.GiaSus.Single(g => g.MaGS == GSKhac.MaGS).TrangThai);  // LH-21: đồng bộ GS mới
        Assert.Equal(Constants.GiaSuStatus.Ranh, db.GiaSus.Single(g => g.MaGS == GS.MaGS).TrangThai);     // và GS cũ
    }

    [Fact]
    [Trait("Luat", "LH-19")]
    public async Task Sua_lop_ngay_ket_thuc_truoc_ngay_bat_dau_bi_tu_choi()
    {
        var ql = await DangNhapQL();
        int maLop = await GhepLopThanhCong(ql, GS.MaGS);

        var form = (await ql.Get($"/LopHoc/Edit/{maLop}")).Form("#form-sua-lop").Dat("KetThuc", Ngay(0));
        var kq = await ql.Gui(form);

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        using var db = Moi();
        Assert.Equal(DateTime.Today.AddDays(8).AddHours(9), db.LopHocs.Single(l => l.MaLop == maLop).ThoiGianKetThuc);
    }

    [Fact]
    [Trait("Luat", "LH-22")]
    public async Task Huy_lop_dat_trang_thai_da_huy_va_tra_lich_gia_su()
    {
        var ql = await DangNhapQL();
        int maLop = await GhepLopThanhCong(ql, GS.MaGS);

        var chiTiet = await ql.Get($"/LopHoc/Details/{maLop}");
        var form = chiTiet.Form("#form-xac-nhan");
        form.Action = chiTiet.Thuoc("[data-xac-nhan-title='Hủy lớp']", "data-xac-nhan-action");   // JS của modal gán action này
        Assert.Equal($"/LopHoc/Huy/{maLop}", form.Action);
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi($"/LopHoc/Details/{maLop}"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Equal("Đã hủy lớp.", (await ql.Theo(kq)).ThongBaoThanhCong);
        using (var db = Moi())
        {
            Assert.Equal(Constants.LopHocStatus.DaHuy, db.LopHocs.Single(l => l.MaLop == maLop).TrangThai);
            Assert.Equal(Constants.GiaSuStatus.Ranh, db.GiaSus.Single(g => g.MaGS == GS.MaGS).TrangThai);
        }

        // Hủy lần hai: bị chặn, hiện lỗi
        var lai = await ql.Post($"/LopHoc/Huy/{maLop}", Array.Empty<System.Collections.Generic.KeyValuePair<string, string>>());
        Assert.True(lai.RedirectToi($"/LopHoc/Details/{maLop}"));
        Assert.False(string.IsNullOrWhiteSpace((await ql.Theo(lai)).ThongBaoLoi));
    }

    [Fact]
    [Trait("Luat", "LH-23")]
    public async Task Xoa_lop_chua_co_diem_danh_hoac_hoc_phi_thanh_cong()
    {
        var ql = await DangNhapQL();
        int maLop = await GhepLopThanhCong(ql, GS.MaGS);

        var chiTiet = await ql.Get($"/LopHoc/Details/{maLop}");
        var form = chiTiet.Form("#form-xac-nhan");
        form.Action = chiTiet.Thuoc("[data-xac-nhan-title='Xóa lớp']", "data-xac-nhan-action");
        Assert.Equal($"/LopHoc/Delete/{maLop}", form.Action);
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi("/LopHoc"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Equal(Constants.Messages.XoaThanhCong, (await ql.Theo(kq)).ThongBaoThanhCong);
        using var db = Moi();
        Assert.False(db.LopHocs.Any(l => l.MaLop == maLop));
        Assert.Equal(Constants.GiaSuStatus.Ranh, db.GiaSus.Single(g => g.MaGS == GS.MaGS).TrangThai);
    }

    [Fact]
    [Trait("Luat", "LH-23")]
    public async Task Xoa_lop_da_dong_hoc_phi_bi_chan()
    {
        var ql = await DangNhapQL();
        int maLop = await GhepLopThanhCong(ql, GS.MaGS);
        Db.ThanhToans.Add(new ThanhToan { MaLop = maLop, SoTien = 100000, NgayDong = DateTime.Now, HinhThuc = "Tiền mặt" });
        Db.SaveChanges();

        var kq = await ql.Post($"/LopHoc/Delete/{maLop}", Array.Empty<System.Collections.Generic.KeyValuePair<string, string>>());

        Assert.True(kq.RedirectToi($"/LopHoc/Details/{maLop}"));
        Assert.False(string.IsNullOrWhiteSpace((await ql.Theo(kq)).ThongBaoLoi));
        using var db = Moi();
        Assert.True(db.LopHocs.Any(l => l.MaLop == maLop));
    }

    [Fact]
    public async Task Danh_sach_lop_hien_lop_vua_ghep_va_loc_theo_ten()
    {
        var ql = await DangNhapQL();
        int maLop = await GhepLopThanhCong(ql, GS.MaGS);
        string tenLop;
        using (var db = Moi()) tenLop = db.LopHocs.Single(l => l.MaLop == maLop).TenLop;

        var ds = await ql.Get("/LopHoc?q=" + Uri.EscapeDataString(_mon.TenMon));
        Assert.Equal(HttpStatusCode.OK, ds.Status);
        Assert.Contains(tenLop, ds.Chu);
    }

    [Fact]
    public async Task Ajax_kiem_tra_va_goi_y_ten_tra_JSON()
    {
        var ql = await DangNhapQL();
        string q = $"/LopHoc/KiemTra?maHV={_hv.MaHV}&maGS={GS.MaGS}&monHoc={Uri.EscapeDataString(_mon.TenMon)}" +
                   $"&caHoc={Uri.EscapeDataString(Constants.CaHoc.Sang)}&batDau={Ngay(1)}&ketThuc={Ngay(8)}&thuHoc=2,3,4,5,6,7,8";
        var r = await ql.Client.GetAsync(q);
        var thanKiemTra = await r.Content.ReadAsStringAsync();
        Assert.True(r.StatusCode == HttpStatusCode.OK, thanKiemTra.Substring(0, Math.Min(1200, thanKiemTra.Length)));
        string json = await r.Content.ReadAsStringAsync();
        Assert.Contains("\"mucDo\"", json);

        var ten = await ql.Client.GetAsync($"/LopHoc/GoiYTen?monHoc={Uri.EscapeDataString(_mon.TenMon)}&maHV={_hv.MaHV}&caHoc={Uri.EscapeDataString(Constants.CaHoc.Sang)}");
        Assert.Equal(HttpStatusCode.OK, ten.StatusCode);
        Assert.Contains(_mon.TenMon, await ten.Content.ReadAsStringAsync());
    }
}
