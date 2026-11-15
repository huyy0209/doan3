using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>Review #2: lưu/xóa môn học, lưu cài đặt trung tâm, thêm/xóa lần đóng học phí + xuất CSV.</summary>
public class MonHocCaiDatHocPhiHttpTests : HttpTestBase
{
    private static string TenMon() => "ITG_mon" + Guid.NewGuid().ToString("N").Substring(0, 8);

    // ---------------- Môn học ----------------

    [Fact]
    [Trait("Luat", "MH-01")]
    public async Task Them_mon_chinh_ghi_DB_va_hien_thong_bao()
    {
        var ql = await DangNhapQL();
        string ten = TenMon();

        var kq = await ql.Gui((await ql.Get("/MonHoc")).Form("#form-mon-hoc").Dat("TenMon", ten).Dat("LoaiMon", "Chính"));

        Assert.True(kq.LaRedirect, $"được {(int)kq.Status}: {kq.Chu}");
        Assert.StartsWith("/MonHoc?maMon=", kq.Location);
        Assert.Equal("Đã thêm môn học.", (await ql.Theo(kq)).ThongBaoThanhCong);
        using var db = Moi();
        var mon = db.MonHocs.Single(m => m.TenMon == ten);
        Assert.Equal("Chính", mon.LoaiMon);
        Assert.False(mon.NgungDay);
        Assert.EndsWith("=" + mon.MaMon, kq.Location);
    }

    [Fact]
    [Trait("Luat", "MH-01")]
    public async Task Them_mon_nang_khieu_luu_danh_sach_cap_do()
    {
        var ql = await DangNhapQL();
        string ten = TenMon();

        var kq = await ql.Gui((await ql.Get("/MonHoc")).Form("#form-mon-hoc")
            .Dat("TenMon", ten).Dat("LoaiMon", "Năng khiếu").Dat("CapDoText", "Cấp 1\r\nCấp 2\r\n\r\n Cấp 3 "));

        Assert.True(kq.LaRedirect, $"được {(int)kq.Status}: {kq.Chu}");
        using var db = Moi();
        var mon = db.MonHocs.Single(m => m.TenMon == ten);
        Assert.Equal("Năng khiếu", mon.LoaiMon);
        Assert.Equal(new[] { "Cấp 1", "Cấp 2", "Cấp 3" }, mon.DanhSachCapDo);
    }

    [Fact]
    [Trait("Luat", "MH-01")]
    public async Task Them_mon_nang_khieu_chi_mot_cap_bi_tu_choi()
    {
        var ql = await DangNhapQL();
        string ten = TenMon();

        var kq = await ql.Gui((await ql.Get("/MonHoc")).Form("#form-mon-hoc")
            .Dat("TenMon", ten).Dat("LoaiMon", "Năng khiếu").Dat("CapDoText", "Cấp 1"));

        Assert.True(kq.LaRedirect);
        Assert.False(string.IsNullOrWhiteSpace((await ql.Theo(kq)).ThongBaoLoi));
        using var db = Moi();
        Assert.False(db.MonHocs.Any(m => m.TenMon == ten));
    }

    [Fact]
    [Trait("Luat", "MH-01")]
    public async Task Them_mon_trung_ten_bi_tu_choi()
    {
        var mon = TaoMon(TenMon());
        var ql = await DangNhapQL();

        var kq = await ql.Gui((await ql.Get("/MonHoc")).Form("#form-mon-hoc").Dat("TenMon", mon.TenMon.ToUpperInvariant()).Dat("LoaiMon", "Chính"));

        Assert.True(kq.LaRedirect);
        Assert.False(string.IsNullOrWhiteSpace((await ql.Theo(kq)).ThongBaoLoi));
        using var db = Moi();
        Assert.Equal(1, db.MonHocs.Count(m => m.TenMon == mon.TenMon));
    }

    [Fact]
    [Trait("Luat", "MH-02")]
    public async Task Sua_mon_doi_ten_cap_nhat_lop_va_chuyen_mon_gia_su_va_ngung_day()
    {
        var mon = TaoMon(TenMon());
        var hv = TaoHocVien(TenItg());
        var lop = TaoLop(GS, hv, monHoc: mon.TenMon);
        var gsMon = TaoGiaSu(TenItg(), chuyenMon: mon.TenMon);
        var ql = await DangNhapQL();
        string tenMoi = TenMon();

        var form = (await ql.Get($"/MonHoc?maMon={mon.MaMon}")).Form("#form-mon-hoc")
            .Dat("TenMon", tenMoi).Dat("NgungDay", "true");
        var kq = await ql.Gui(form);

        Assert.True(kq.LaRedirect, $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Equal("Đã lưu môn học.", (await ql.Theo(kq)).ThongBaoThanhCong);
        using var db = Moi();
        var sau = db.MonHocs.Single(m => m.MaMon == mon.MaMon);
        Assert.Equal(tenMoi, sau.TenMon);
        Assert.True(sau.NgungDay);
        Assert.Equal(tenMoi, db.LopHocs.Single(l => l.MaLop == lop.MaLop).MonHoc);
        Assert.Equal(tenMoi, db.GiaSus.Single(g => g.MaGS == gsMon.MaGS).ChuyenMon);
    }

    [Fact]
    [Trait("Luat", "MH-03")]
    public async Task Xoa_mon_chua_co_lop_thanh_cong()
    {
        var mon = TaoMon(TenMon());
        var ql = await DangNhapQL();

        var kq = await ql.Gui((await ql.Get($"/MonHoc?maMon={mon.MaMon}")).Form("form[action$='/MonHoc/Xoa']"));

        Assert.True(kq.RedirectToi("/MonHoc"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Equal(Constants.Messages.XoaThanhCong, (await ql.Theo(kq)).ThongBaoThanhCong);
        using var db = Moi();
        Assert.False(db.MonHocs.Any(m => m.MaMon == mon.MaMon));
    }

    [Fact]
    [Trait("Luat", "MH-03")]
    public async Task Xoa_mon_dang_co_lop_bi_chan()
    {
        var mon = TaoMon(TenMon());
        TaoLop(GS, TaoHocVien(TenItg()), monHoc: mon.TenMon);
        var ql = await DangNhapQL();

        var kq = await ql.Gui((await ql.Get($"/MonHoc?maMon={mon.MaMon}")).Form("form[action$='/MonHoc/Xoa']"));

        Assert.True(kq.RedirectToi("/MonHoc"));
        Assert.False(string.IsNullOrWhiteSpace((await ql.Theo(kq)).ThongBaoLoi));
        using var db = Moi();
        Assert.True(db.MonHocs.Any(m => m.MaMon == mon.MaMon));
    }

    // ---------------- Cài đặt trung tâm ----------------

    [Fact]
    [Trait("Luat", "CD-01")]
    public async Task Luu_cai_dat_ghi_DB_va_tra_ve_form_voi_gia_tri_moi()
    {
        var ql = await DangNhapQL();
        var goc = (await ql.Get("/CaiDat")).Form("form[action$='/CaiDat']");
        try
        {
            string ten = "ITG Trung tâm " + Guid.NewGuid().ToString("N").Substring(0, 6);
            var form = (await ql.Get("/CaiDat")).Form("form[action$='/CaiDat']")
                .Dat("Ten", ten).Dat("DiaChi", "12 Đường Itg").Dat("SDT", "0900000000");
            var kq = await ql.Gui(form);

            Assert.True(kq.RedirectToi("/CaiDat"), $"được {(int)kq.Status}: {kq.Chu}");
            var sau = await ql.Theo(kq);
            Assert.Equal("Đã lưu thông tin trung tâm.", sau.ThongBaoThanhCong);
            var f = sau.Form("form[action$='/CaiDat']");
            Assert.Equal(ten, f.Lay("Ten"));
            Assert.Equal("12 Đường Itg", f.Lay("DiaChi"));

            using var db = Moi();
            Assert.Equal(ten, db.CaiDats.Single(c => c.Khoa == "TenTrungTam").GiaTri);
            Assert.Equal("12 Đường Itg", db.CaiDats.Single(c => c.Khoa == "DiaChi").GiaTri);
            Assert.Contains(ten, (await ql.GetTheoRedirect("/")).Doc.Title);   // tên trung tâm hiện trên layout
        }
        finally
        {
            // Trả lại cài đặt cũ (CSDL test dùng chung)
            await ql.Gui((await ql.Get("/CaiDat")).Form("form[action$='/CaiDat']")
                .Dat("Ten", string.IsNullOrWhiteSpace(goc.Lay("Ten")) ? CaiDatMacDinh : goc.Lay("Ten"))
                .Dat("DiaChi", goc.Lay("DiaChi") ?? "").Dat("SDT", goc.Lay("SDT") ?? ""));
        }
    }

    private const string CaiDatMacDinh = "Trung tâm gia sư";

    [Fact]
    [Trait("Luat", "CD-01")]
    public async Task Luu_cai_dat_ten_qua_dai_bi_tu_choi_va_khong_doi()
    {
        var ql = await DangNhapQL();
        string truoc;
        using (var db = Moi()) truoc = db.CaiDats.FirstOrDefault(c => c.Khoa == "TenTrungTam")?.GiaTri;

        var kq = await ql.Gui((await ql.Get("/CaiDat")).Form("form[action$='/CaiDat']").Dat("Ten", new string('A', 101)));

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.Contains("tối đa 100", kq.Chu);
        using var db2 = Moi();
        Assert.Equal(truoc, db2.CaiDats.FirstOrDefault(c => c.Khoa == "TenTrungTam")?.GiaTri);
    }

    // ---------------- Học phí ----------------

    private LopHoc LopCoHocPhi(decimal hocPhi = 1000000m) => TaoLop(GS, TaoHocVien(TenItg()), hocPhi: hocPhi);

    private async Task<FormDuLieu> FormThanhToan(TrinhDuyet ql, int maLop) =>
        (await ql.Get($"/HocPhi?maLop={maLop}&tinhTrang=T%E1%BA%A5t%20c%E1%BA%A3")).Form("form.w6-payment-form");

    [Fact]
    [Trait("Luat", "HP-03")]
    public async Task Them_lan_dong_hoc_phi_ghi_DB_va_cap_nhat_cong_no()
    {
        var lop = LopCoHocPhi();
        var ql = await DangNhapQL();

        var form = (await FormThanhToan(ql, lop.MaLop)).Dat("ThanhToan.SoTien", "400.000")
            .Dat("ThanhToan.NgayDong", DateTime.Today.ToString("yyyy-MM-dd")).Dat("ThanhToan.GhiChu", "Đợt 1 itg");
        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi("/HocPhi"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Contains($"maLop={lop.MaLop}", kq.Location);
        var sau = await ql.Theo(kq);
        Assert.Contains("Đã ghi nhận 400.000", sau.ThongBaoThanhCong);
        Assert.Contains("Đợt 1 itg", sau.Chu);                                   // lịch sử đóng

        using var db = Moi();
        var tt = db.ThanhToans.Single(t => t.MaLop == lop.MaLop);
        Assert.Equal(400000m, tt.SoTien);
        Assert.Equal(DateTime.Today, tt.NgayDong.Date);
        Assert.Equal("Đợt 1 itg", tt.GhiChu);
    }

    [Fact]
    [Trait("Luat", "HP-03")]
    public async Task Them_lan_dong_vuot_so_no_hoac_ngay_tuong_lai_hoac_tien_sai_bi_tu_choi()
    {
        var lop = LopCoHocPhi();
        var ql = await DangNhapQL();

        var vuot = await ql.Gui((await FormThanhToan(ql, lop.MaLop)).Dat("ThanhToan.SoTien", "1.500.000"));
        Assert.Equal(HttpStatusCode.OK, vuot.Status);

        var tuongLai = await ql.Gui((await FormThanhToan(ql, lop.MaLop)).Dat("ThanhToan.SoTien", "100.000")
            .Dat("ThanhToan.NgayDong", DateTime.Today.AddDays(3).ToString("yyyy-MM-dd")));
        Assert.Equal(HttpStatusCode.OK, tuongLai.Status);

        var chu = await ql.Gui((await FormThanhToan(ql, lop.MaLop)).Dat("ThanhToan.SoTien", "abc"));
        Assert.Equal(HttpStatusCode.OK, chu.Status);
        Assert.Contains("Nhập số tiền", chu.Chu);

        var am = await ql.Gui((await FormThanhToan(ql, lop.MaLop)).Dat("ThanhToan.SoTien", "0"));
        Assert.Equal(HttpStatusCode.OK, am.Status);

        using var db = Moi();
        Assert.False(db.ThanhToans.Any(t => t.MaLop == lop.MaLop));
    }

    [Fact]
    [Trait("Luat", "HP-03")]
    public async Task Them_lan_dong_cho_lop_da_huy_bi_tu_choi()
    {
        var lop = TaoLop(GS, TaoHocVien(TenItg()), trangThai: Constants.LopHocStatus.DaHuy, hocPhi: 1000000m);
        var ql = await DangNhapQL();

        // Giao diện không hiện form cho lớp đã hủy → gửi thẳng POST như người dùng cố tình tạo
        var kq = await ql.Post("/HocPhi/ThemThanhToan", new KeyValuePair<string, string>[]
        {
            new("ThanhToan.MaLop", lop.MaLop.ToString()), new("ThanhToan.SoTien", "100.000"),
            new("ThanhToan.NgayDong", DateTime.Today.ToString("yyyy-MM-dd")), new("ThanhToan.HinhThuc", "Tiền mặt")
        });

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        using var db = Moi();
        Assert.False(db.ThanhToans.Any(t => t.MaLop == lop.MaLop));
    }

    [Fact]
    [Trait("Luat", "HP-04")]
    public async Task Xoa_lan_dong_hoc_phi_ghi_DB()
    {
        var lop = LopCoHocPhi();
        Db.ThanhToans.Add(new ThanhToan { MaLop = lop.MaLop, SoTien = 300000, NgayDong = DateTime.Now, HinhThuc = "Tiền mặt", GhiChu = "itg xoa" });
        Db.SaveChanges();
        int maTT = Db.ThanhToans.Single(t => t.MaLop == lop.MaLop).MaTT;
        var ql = await DangNhapQL();

        var trang = await ql.Get($"/HocPhi?maLop={lop.MaLop}&tinhTrang=T%E1%BA%A5t%20c%E1%BA%A3");
        var kq = await ql.Gui(trang.Form("form[action$='/HocPhi/XoaThanhToan']").Dat("maTT", maTT.ToString()));

        Assert.True(kq.RedirectToi("/HocPhi"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Equal("Đã xóa lần đóng học phí.", (await ql.Theo(kq)).ThongBaoThanhCong);
        using var db = Moi();
        Assert.False(db.ThanhToans.Any(t => t.MaTT == maTT));
    }

    [Fact]
    [Trait("Luat", "HP-04")]
    public async Task Xoa_lan_dong_khong_ton_tai_hien_loi()
    {
        var lop = LopCoHocPhi();
        Db.ThanhToans.Add(new ThanhToan { MaLop = lop.MaLop, SoTien = 100000, NgayDong = DateTime.Now, HinhThuc = "Tiền mặt" });
        Db.SaveChanges();
        var ql = await DangNhapQL();
        var trang = await ql.Get($"/HocPhi?maLop={lop.MaLop}&tinhTrang=T%E1%BA%A5t%20c%E1%BA%A3");
        var form = trang.Form("form[action$='/HocPhi/XoaThanhToan']");

        // Người khác đã xóa lần đóng này trước khi form được gửi
        Db.Database.ExecuteSqlRaw("DELETE FROM ThanhToan WHERE MaLop = {0}", lop.MaLop);
        var kq = await ql.Gui(form.Dat("maTT", "2147000000"));

        Assert.True(kq.RedirectToi("/HocPhi"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.False(string.IsNullOrWhiteSpace((await ql.Theo(kq)).ThongBaoLoi));
    }

    [Fact]
    [Trait("Luat", "HP-01")]
    public async Task Xuat_CSV_cong_no_co_header_va_dong_cua_lop()
    {
        var lop = LopCoHocPhi(2000000m);
        Db.ThanhToans.Add(new ThanhToan { MaLop = lop.MaLop, SoTien = 500000, NgayDong = DateTime.Now, HinhThuc = "Tiền mặt" });
        Db.SaveChanges();
        var ql = await DangNhapQL();

        var r = await ql.Client.GetAsync("/HocPhi/XuatCsv?tinhTrang=" + Uri.EscapeDataString("Tất cả") + "&tuKhoa=" + Uri.EscapeDataString(lop.TenLop));

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal("text/csv", r.Content.Headers.ContentType?.MediaType);
        Assert.StartsWith("CongNo_", r.Content.Headers.ContentDisposition?.FileName?.Trim('"'));
        string csv = Encoding.UTF8.GetString(await r.Content.ReadAsByteArrayAsync());
        Assert.Contains("Lớp", csv);
        Assert.Contains("Còn nợ", csv);
        Assert.Contains(lop.TenLop, csv);
        Assert.Contains("2000000", csv);
        Assert.Contains("500000", csv);
        Assert.Contains("1500000", csv);
    }

    [Fact]
    public async Task Trang_hoc_phi_hien_cong_no_cua_lop()
    {
        var lop = LopCoHocPhi();
        var ql = await DangNhapQL();
        var t = await ql.Get("/HocPhi?tinhTrang=" + Uri.EscapeDataString("Tất cả") + "&tuKhoa=" + Uri.EscapeDataString(lop.TenLop));
        Assert.Equal(HttpStatusCode.OK, t.Status);
        Assert.Contains(lop.TenLop, t.Chu);
    }
}
