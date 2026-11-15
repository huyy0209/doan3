using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>Review #2: lưu điểm danh một ngày, sửa buổi trong trang kết quả, xếp / hủy học bù – quản lý và gia sư.</summary>
public class DiemDanhHocTapHttpTests : HttpTestBase
{
    private readonly HocVien _hv;
    private readonly LopHoc _lop;     // lớp của GS (gia sư "của mình"), học hằng ngày, ca Sáng

    public DiemDanhHocTapHttpTests()
    {
        _hv = TaoHocVien(TenItg());
        _lop = TaoLop(GS, _hv);
    }

    private static string Ngay(int soNgay) => DateTime.Today.AddDays(soNgay).ToString("yyyy-MM-dd");

    /// <summary>Form điểm danh của một ngày (đã lọc theo gia sư để chỉ còn lớp của test)</summary>
    private static async Task<FormDuLieu> FormDiemDanh(TrinhDuyet t, int soNgay, int? maGS)
    {
        string url = $"/DiemDanh?ngay={Ngay(soNgay)}" + (maGS.HasValue ? $"&maGS={maGS}" : "");
        return (await t.Get(url)).Form("form[action$='/DiemDanh/Luu']");
    }

    private static int ChiSoDong(FormDuLieu f, int maLop)
    {
        var kv = f.Fields.First(x => x.Key.EndsWith("].MaLop") && x.Value == maLop.ToString());
        return int.Parse(kv.Key.Substring("Rows[".Length, kv.Key.IndexOf(']') - "Rows[".Length));
    }

    private List<BuoiHoc> Buoi(int maLop)
    {
        using var db = Moi();
        return db.BuoiHocs.Where(b => b.MaLop == maLop).ToList();
    }

    // ---------------- Điểm danh một ngày ----------------

    [Fact]
    [Trait("Luat", "HT-01")]
    public async Task Quan_ly_luu_diem_danh_hom_nay_ghi_buoi_hoc()
    {
        var ql = await DangNhapQL();
        var form = await FormDiemDanh(ql, 0, GS.MaGS);
        int i = ChiSoDong(form, _lop.MaLop);
        form.Dat($"Rows[{i}].DiemDanh", "Có mặt").Dat($"Rows[{i}].DiemText", "8,5").Dat($"Rows[{i}].NhanXet", "Làm bài tốt");

        var kq = await ql.Gui(form);

        Assert.True(kq.RedirectToi("/DiemDanh"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Contains($"Đã lưu điểm danh ngày {DateTime.Today:dd/MM/yyyy}", (await ql.Theo(kq)).ThongBaoThanhCong);
        var b = Assert.Single(Buoi(_lop.MaLop));
        Assert.Equal(DateTime.Today, b.NgayHoc);
        Assert.Equal("Có mặt", b.DiemDanh);
        Assert.Equal(8.5m, b.Diem);
        Assert.Equal("Làm bài tốt", b.NhanXet);
        Assert.Null(b.MaBuoiGoc);
    }

    [Fact]
    [Trait("Luat", "HT-04")]
    public async Task Luu_lai_cung_ngay_cap_nhat_khong_them_buoi_thu_hai()
    {
        var ql = await DangNhapQL();
        var form = await FormDiemDanh(ql, 0, GS.MaGS);
        int i = ChiSoDong(form, _lop.MaLop);
        await ql.Gui(form.Dat($"Rows[{i}].DiemDanh", "Có mặt").Dat($"Rows[{i}].DiemText", "7"));

        var form2 = await FormDiemDanh(ql, 0, GS.MaGS);
        int j = ChiSoDong(form2, _lop.MaLop);
        Assert.Equal("7", form2.Lay($"Rows[{j}].DiemText"));            // trang hiện lại điểm đã lưu
        var kq = await ql.Gui(form2.Dat($"Rows[{j}].DiemDanh", "Đi trễ").Dat($"Rows[{j}].DiemText", "9,25"));

        Assert.True(kq.RedirectToi("/DiemDanh"), $"được {(int)kq.Status}: {kq.Chu}");
        var b = Assert.Single(Buoi(_lop.MaLop));
        Assert.Equal("Đi trễ", b.DiemDanh);
        Assert.Equal(9.25m, b.Diem);
    }

    [Fact]
    [Trait("Luat", "HT-03")]
    public async Task Diem_ngoai_khoang_hoac_vang_co_diem_bi_tu_choi()
    {
        var ql = await DangNhapQL();
        var form = await FormDiemDanh(ql, 0, GS.MaGS);
        int i = ChiSoDong(form, _lop.MaLop);

        var diem11 = await ql.Gui(form.Dat($"Rows[{i}].DiemDanh", "Có mặt").Dat($"Rows[{i}].DiemText", "11"));
        Assert.Equal(HttpStatusCode.OK, diem11.Status);
        Assert.Contains("không hợp lệ", diem11.Chu);

        var diemChu = await ql.Gui(form.Dat($"Rows[{i}].DiemText", "giỏi"));
        Assert.Equal(HttpStatusCode.OK, diemChu.Status);

        var vangCoDiem = await ql.Gui(form.Dat($"Rows[{i}].DiemDanh", "Vắng không phép").Dat($"Rows[{i}].DiemText", "5"));
        Assert.Equal(HttpStatusCode.OK, vangCoDiem.Status);

        Assert.Empty(Buoi(_lop.MaLop));
    }

    [Fact]
    [Trait("Luat", "HT-02")]
    public async Task Diem_danh_ngay_tuong_lai_bi_tu_choi()
    {
        var ql = await DangNhapQL();
        var form = await FormDiemDanh(ql, 0, GS.MaGS);
        int i = ChiSoDong(form, _lop.MaLop);

        var kq = await ql.Gui(form.Dat("Ngay", Ngay(2)).Dat($"Rows[{i}].DiemDanh", "Có mặt"));

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.Contains("tương lai", kq.Chu);
        Assert.Empty(Buoi(_lop.MaLop));
    }

    [Fact]
    [Trait("Luat", "HT-02")]
    public async Task Diem_danh_ngay_truoc_khi_lop_bat_dau_bi_tu_choi()
    {
        var ql = await DangNhapQL();
        var form = await FormDiemDanh(ql, 0, GS.MaGS);
        int i = ChiSoDong(form, _lop.MaLop);

        var kq = await ql.Gui(form.Dat("Ngay", Ngay(-10)).Dat($"Rows[{i}].DiemDanh", "Có mặt"));

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.Empty(Buoi(_lop.MaLop));
    }

    [Fact]
    [Trait("Luat", "HT-06")]
    public async Task Thi_len_cap_chi_cho_mon_nang_khieu()
    {
        var ql = await DangNhapQL();
        var form = await FormDiemDanh(ql, 0, GS.MaGS);
        int i = ChiSoDong(form, _lop.MaLop);

        // Lớp môn thường: tick "thi lên cấp" (gửi tay vì ô bị disable) → bị từ chối hoặc bỏ qua, không được ghi cờ thi
        form.Dat($"Rows[{i}].DiemDanh", "Có mặt").Dat($"Rows[{i}].DiemText", "9").Dat($"Rows[{i}].ThiLenCap", "true");
        var kq = await ql.Gui(form);

        var buoi = Buoi(_lop.MaLop);
        Assert.False(buoi.Any(b => b.ThiLenCap), $"môn thường không được ghi cờ thi lên cấp (status {(int)kq.Status}: {kq.Chu})");
    }

    // ---------------- Gia sư ----------------

    [Fact]
    [Trait("Luat", "HT-01")]
    public async Task Gia_su_luu_diem_danh_lop_cua_minh_duoc()
    {
        var gs = await DangNhapGS();
        var form = await FormDiemDanh(gs, 0, null);
        int i = ChiSoDong(form, _lop.MaLop);

        var kq = await gs.Gui(form.Dat($"Rows[{i}].DiemDanh", "Có mặt").Dat($"Rows[{i}].DiemText", "6,5"));

        Assert.True(kq.RedirectToi("/DiemDanh"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Contains("Đã lưu điểm danh", (await gs.Theo(kq)).ThongBaoThanhCong);
        var b = Assert.Single(Buoi(_lop.MaLop));
        Assert.Equal(6.5m, b.Diem);
    }

    [Fact]
    [Trait("Luat", "HT-01")]
    public async Task Gia_su_khong_thay_lop_nguoi_khac_tren_trang_diem_danh()
    {
        var hvKhac = TaoHocVien(TenItg());
        var lopKhac = TaoLop(GSKhac, hvKhac);
        var gs = await DangNhapGS();

        var form = await FormDiemDanh(gs, 0, null);
        Assert.DoesNotContain(form.Fields, x => x.Key.EndsWith("].MaLop") && x.Value == lopKhac.MaLop.ToString());
        Assert.Contains(form.Fields, x => x.Key.EndsWith("].MaLop") && x.Value == _lop.MaLop.ToString());
    }

    [Fact]
    [Trait("Luat", "HT-01")]
    public async Task Gia_su_gui_diem_danh_cho_lop_nguoi_khac_bi_tu_choi_va_khong_ghi()
    {
        var hvKhac = TaoHocVien(TenItg());
        var lopKhac = TaoLop(GSKhac, hvKhac);
        var gs = await DangNhapGS();
        var form = await FormDiemDanh(gs, 0, null);

        // Giả mạo: thêm dòng của lớp người khác như khi sửa HTML bằng DevTools
        int n = form.Fields.Count(x => x.Key.EndsWith("].MaLop"));
        foreach (var (k, v) in new[]
                 {
                     ($"Rows[{n}].MaLop", lopKhac.MaLop.ToString()), ($"Rows[{n}].MaHV", hvKhac.MaHV.ToString()),
                     ($"Rows[{n}].HocVien", "x"), ($"Rows[{n}].DiemDanh", "Có mặt"), ($"Rows[{n}].DiemText", "10"),
                 })
            form.Fields.Add(new(k, v));
        var kq = await gs.Gui(form);

        Assert.True(kq.BiChan || kq.Status == HttpStatusCode.OK, $"được {(int)kq.Status} {kq.Location}");
        Assert.Empty(Buoi(lopKhac.MaLop));
    }

    // ---------------- Trang kết quả học viên: sửa buổi ----------------

    [Fact]
    [Trait("Luat", "HT-03")]
    public async Task Sua_buoi_trong_trang_ket_qua_hoc_vien()
    {
        var ql = await DangNhapQL();
        var form = await FormDiemDanh(ql, -1, GS.MaGS);
        int i = ChiSoDong(form, _lop.MaLop);
        await ql.Gui(form.Dat($"Rows[{i}].DiemDanh", "Có mặt").Dat($"Rows[{i}].DiemText", "5"));
        Assert.Single(Buoi(_lop.MaLop));

        var trang = await ql.Get($"/HocTap/KetQua/{_hv.MaHV}");
        var f = trang.Form("#luuBuoiForm");
        f.Dat("buoi[0].DiemText", "9").Dat("buoi[0].NhanXet", "Tiến bộ");
        var kq = await ql.Gui(f);

        Assert.True(kq.RedirectToi($"/HocTap/KetQua/{_hv.MaHV}"), $"được {(int)kq.Status}: {kq.Chu}");
        Assert.Equal("Đã lưu 1 buổi học.", (await ql.Theo(kq)).ThongBaoThanhCong);
        var b = Assert.Single(Buoi(_lop.MaLop));
        Assert.Equal(9m, b.Diem);
        Assert.Equal("Tiến bộ", b.NhanXet);
    }

    [Fact]
    [Trait("Luat", "HT-03")]
    public async Task Sua_buoi_gui_MaLop_khong_khop_bi_tu_choi()
    {
        var ql = await DangNhapQL();
        var form = await FormDiemDanh(ql, -1, GS.MaGS);
        int i = ChiSoDong(form, _lop.MaLop);
        await ql.Gui(form.Dat($"Rows[{i}].DiemDanh", "Có mặt").Dat($"Rows[{i}].DiemText", "5"));

        var f = (await ql.Get($"/HocTap/KetQua/{_hv.MaHV}")).Form("#luuBuoiForm");
        var kq = await ql.Gui(f.Dat("buoi[0].MaLop", (_lop.MaLop + 100000).ToString()).Dat("buoi[0].DiemText", "10"));

        Assert.Equal(HttpStatusCode.OK, kq.Status);
        Assert.Contains("không hợp lệ", kq.Chu);
        Assert.Equal(5m, Assert.Single(Buoi(_lop.MaLop)).Diem);
    }

    // ---------------- Học bù ----------------

    /// <summary>Tạo buổi vắng hôm qua qua form điểm danh; trả về MaBuoi</summary>
    private async Task<int> TaoBuoiVang(TrinhDuyet ql)
    {
        var form = await FormDiemDanh(ql, -1, GS.MaGS);
        int i = ChiSoDong(form, _lop.MaLop);
        var kq = await ql.Gui(form.Dat($"Rows[{i}].DiemDanh", "Vắng không phép").Dat($"Rows[{i}].DiemText", ""));
        Assert.True(kq.LaRedirect, $"lưu buổi vắng: {(int)kq.Status}: {kq.Chu}");
        return Assert.Single(Buoi(_lop.MaLop)).MaBuoi;
    }

    private static async Task<JsonElement> XepBu(TrinhDuyet t, Trang trang, int maBuoiVang, string ngay, string ca, int maGSDay, bool boQua = false)
    {
        var f = trang.Form("#xepHocBuForm").Dat("maBuoiVang", maBuoiVang.ToString()).Dat("ngay", ngay).Dat("ca", ca)
            .Dat("maGSDay", maGSDay.ToString()).Dat("boQuaCanhBao", boQua ? "true" : "false");
        var r = await t.Client.PostAsync("/HocTap/XepHocBu", new FormUrlEncodedContent(f.Fields));
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        return JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    private static bool Co(JsonElement e, string ten) =>
        e.EnumerateObject().Any(p => string.Equals(p.Name, ten, StringComparison.OrdinalIgnoreCase));

    [Fact]
    [Trait("Luat", "HT-10")]
    public async Task Xep_hoc_bu_roi_huy_hoc_bu_giu_nguyen_buoi_vang()
    {
        var ql = await DangNhapQL();
        int maVang = await TaoBuoiVang(ql);

        var trang = await ql.Get($"/HocTap/KetQua/{_hv.MaHV}");
        var kq = await XepBu(ql, trang, maVang, Ngay(1), Constants.CaHoc.Chieu, GSKhac.MaGS);
        Assert.True(Co(kq, "thanhCong"), kq.ToString());

        BuoiHoc bu;
        using (var db = Moi())
        {
            bu = db.BuoiHocs.Single(b => b.MaBuoiGoc == maVang);
            Assert.Equal(DateTime.Today.AddDays(1), bu.NgayHoc);
            Assert.Equal(Constants.CaHoc.Chieu, bu.CaHoc);
            Assert.Equal(GSKhac.MaGS, bu.MaGSDay);
            Assert.Equal(_lop.MaLop, bu.MaLop);
        }

        // Xếp lần hai cho cùng buổi vắng: bị từ chối (HT-11)
        var lan2 = await XepBu(ql, await ql.Get($"/HocTap/KetQua/{_hv.MaHV}"), maVang, Ngay(2), Constants.CaHoc.Chieu, GSKhac.MaGS);
        Assert.False(Co(lan2, "thanhCong"), lan2.ToString());
        using (var db = Moi()) Assert.Equal(1, db.BuoiHocs.Count(b => b.MaBuoiGoc == maVang));

        // Hủy học bù qua form ẩn
        var trang2 = await ql.Get($"/HocTap/KetQua/{_hv.MaHV}");
        var huy = await ql.Gui(trang2.Form("#huyHocBuForm").Dat("maBuoiBu", bu.MaBuoi.ToString()));
        Assert.True(huy.RedirectToi($"/HocTap/KetQua/{_hv.MaHV}"), $"được {(int)huy.Status}: {huy.Chu}");
        Assert.Contains("Đã hủy buổi học bù", (await ql.Theo(huy)).ThongBaoThanhCong);
        using (var db = Moi())
        {
            Assert.False(db.BuoiHocs.Any(b => b.MaBuoi == bu.MaBuoi));
            var vang = db.BuoiHocs.Single(b => b.MaBuoi == maVang);
            Assert.Equal("Vắng không phép", vang.DiemDanh);          // HT-12: buổi gốc giữ nguyên
        }
    }

    [Fact]
    [Trait("Luat", "HT-10")]
    public async Task Xep_hoc_bu_gia_su_ban_can_xac_nhan_roi_moi_xep()
    {
        var ql = await DangNhapQL();
        int maVang = await TaoBuoiVang(ql);
        // GSKhac đang có lớp Chiều hằng ngày với học viên khác → bận ca bù
        TaoLop(GSKhac, TaoHocVien(TenItg()), caHoc: Constants.CaHoc.Chieu);

        var trang = await ql.Get($"/HocTap/KetQua/{_hv.MaHV}");
        var canhBao = await XepBu(ql, trang, maVang, Ngay(1), Constants.CaHoc.Chieu, GSKhac.MaGS);
        Assert.True(Co(canhBao, "canhBao"), canhBao.ToString());
        using (var db = Moi()) Assert.False(db.BuoiHocs.Any(b => b.MaBuoiGoc == maVang));

        var ok = await XepBu(ql, trang, maVang, Ngay(1), Constants.CaHoc.Chieu, GSKhac.MaGS, boQua: true);
        Assert.True(Co(ok, "thanhCong"), ok.ToString());
        using (var db = Moi()) Assert.Equal(1, db.BuoiHocs.Count(b => b.MaBuoiGoc == maVang));
    }

    [Fact]
    [Trait("Luat", "HT-10")]
    public async Task Xep_hoc_bu_ngay_qua_xa_hoac_hoc_vien_ban_bi_tu_choi()
    {
        var ql = await DangNhapQL();
        int maVang = await TaoBuoiVang(ql);
        var trang = await ql.Get($"/HocTap/KetQua/{_hv.MaHV}");

        var xa = await XepBu(ql, trang, maVang, Ngay(90), Constants.CaHoc.Chieu, GSKhac.MaGS);
        Assert.False(Co(xa, "thanhCong"), xa.ToString());

        // Học viên đang học ca Sáng hằng ngày → bận
        var ban = await XepBu(ql, trang, maVang, Ngay(1), Constants.CaHoc.Sang, GSKhac.MaGS);
        Assert.False(Co(ban, "thanhCong"), ban.ToString());

        using var db = Moi();
        Assert.False(db.BuoiHocs.Any(b => b.MaBuoiGoc == maVang));
    }

    [Fact]
    [Trait("Luat", "HT-10")]
    public async Task Gia_su_xep_hoc_bu_cho_lop_cua_minh_duoc_con_lop_nguoi_khac_bi_chan()
    {
        var ql = await DangNhapQL();
        int maVang = await TaoBuoiVang(ql);
        var gs = await DangNhapGS();

        var ok = await XepBu(gs, await gs.Get($"/HocTap/KetQua/{_hv.MaHV}"), maVang, Ngay(1), Constants.CaHoc.Chieu, GSKhac.MaGS);
        Assert.True(Co(ok, "thanhCong"), ok.ToString());

        // Buổi vắng của lớp người khác (GSKhac) mà GS cố xếp bù
        var hvKhac = TaoHocVien(TenItg());
        var lopKhac = TaoLop(GSKhac, hvKhac);
        var form = await FormDiemDanh(ql, -1, GSKhac.MaGS);
        int i = ChiSoDong(form, lopKhac.MaLop);
        await ql.Gui(form.Dat($"Rows[{i}].DiemDanh", "Vắng có phép").Dat($"Rows[{i}].DiemText", ""));
        int maVangKhac = Buoi(lopKhac.MaLop).Single().MaBuoi;

        var r = await gs.Post("/HocTap/XepHocBu", new KeyValuePair<string, string>[]
        {
            new("maBuoiVang", maVangKhac.ToString()), new("ngay", Ngay(1)), new("ca", Constants.CaHoc.Chieu),
            new("maGSDay", GS.MaGS.ToString()), new("boQuaCanhBao", "false")
        });
        Assert.True(r.BiChan || (r.Status == HttpStatusCode.OK && !r.Html.Contains("thanhCong", StringComparison.OrdinalIgnoreCase)),
            $"được {(int)r.Status}: {r.Html}");
        using var db = Moi();
        Assert.False(db.BuoiHocs.Any(b => b.MaBuoiGoc == maVangKhac));
    }

    [Fact]
    [Trait("Luat", "HT-12")]
    public async Task Gia_su_huy_hoc_bu_lop_nguoi_khac_bi_chan()
    {
        var ql = await DangNhapQL();
        // Lớp của GSKhac có buổi vắng đã xếp bù, học viên của GSKhac
        var hvKhac = TaoHocVien(TenItg());
        var lopKhac = TaoLop(GSKhac, hvKhac);
        var vang = new BuoiHoc { MaLop = lopKhac.MaLop, NgayHoc = DateTime.Today.AddDays(-1), DiemDanh = "Vắng có phép", CaHoc = lopKhac.CaHoc };
        Db.BuoiHocs.Add(vang);
        Db.SaveChanges();
        var bu = new BuoiHoc { MaLop = lopKhac.MaLop, NgayHoc = DateTime.Today.AddDays(1), CaHoc = Constants.CaHoc.Chieu, MaGSDay = GSKhac.MaGS, MaBuoiGoc = vang.MaBuoi };
        Db.BuoiHocs.Add(bu);
        Db.SaveChanges();

        var gs = await DangNhapGS();
        var r = await gs.Post("/HocTap/HuyHocBu", new KeyValuePair<string, string>[]
        {
            new("maBuoiBu", bu.MaBuoi.ToString()), new("maHV", hvKhac.MaHV.ToString())
        });

        Assert.Equal(HttpStatusCode.Forbidden, r.Status);
        using var db = Moi();
        Assert.True(db.BuoiHocs.Any(b => b.MaBuoi == bu.MaBuoi));
    }
}
