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

/// <summary>
/// G9 – HTTP cho tìm kiếm nhanh: JSON gợi ý, trang kết quả đầy đủ và phân quyền gia sư.
/// Dữ liệu tiền tố ITG_ do HttpTestBase dọn riêng.
/// </summary>
public class TimKiemHttpTests : HttpTestBase
{
    [Fact]
    [Trait("Luat", "G9")]
    public async Task Chua_dang_nhap_bi_chan()
    {
        var khach = Khach();

        var goiY = await khach.Get("/TimKiem/GoiY?q=abc");
        var trang = await khach.Get("/TimKiem?q=abc");

        Assert.True(goiY.VeDangNhap, $"Gợi ý phải đưa khách về đăng nhập, nhưng được {(int)goiY.Status} {goiY.Location}");
        Assert.True(trang.VeDangNhap, $"Trang tìm kiếm phải đưa khách về đăng nhập, nhưng được {(int)trang.Status} {trang.Location}");
    }

    [Fact]
    [Trait("Luat", "G9")]
    public async Task Goi_y_gia_su_chi_tra_du_lieu_cua_minh()
    {
        var hvCuaMinh = TaoHocVien(TenItg(), SdtTest());
        var hvNguoiKhac = TaoHocVien(TenItg(), SdtTest());
        var lopCuaMinh = TaoLop(GS, hvCuaMinh, "ITG_Toán");
        TaoLop(GSKhac, hvNguoiKhac, "ITG_Lý");

        try
        {
            var gs = await DangNhapGS();
            var trang = await gs.Get("/TimKiem/GoiY?q=" + Uri.EscapeDataString("ITG_"));

            Assert.Equal(HttpStatusCode.OK, trang.Status);
            using var doc = JsonDocument.Parse(trang.Html);
            var root = doc.RootElement;

            var idsHocVien = root.GetProperty("hocViens").EnumerateArray()
                .Select(e => e.GetProperty("id").GetInt32()).ToList();
            var idsLop = root.GetProperty("lops").EnumerateArray()
                .Select(e => e.GetProperty("id").GetInt32()).ToList();

            Assert.Contains(hvCuaMinh.MaHV, idsHocVien);
            Assert.DoesNotContain(hvNguoiKhac.MaHV, idsHocVien);
            Assert.Contains(lopCuaMinh.MaLop, idsLop);
            Assert.Equal(0, root.GetProperty("giaSus").GetArrayLength());

            var urlLop = root.GetProperty("lops").EnumerateArray()
                .First(e => e.GetProperty("id").GetInt32() == lopCuaMinh.MaLop)
                .GetProperty("url").GetString();
            Assert.Equal("/HocTap/KetQua/" + hvCuaMinh.MaHV, urlLop);
        }
        finally
        {
            DonDepItg(Db);
        }
    }

    [Fact]
    [Trait("Luat", "G9")]
    public async Task Quan_ly_thay_ca_ba_nhom_va_trang_day_du()
    {
        var gs = TaoGiaSu(TenItg(), SdtTest());
        var hv = TaoHocVien(TenItg(), SdtTest());
        var lop = TaoLop(gs, hv, "ITG_Hóa");

        try
        {
            var ql = await DangNhapQL();
            var goiY = await ql.Get("/TimKiem/GoiY?q=" + Uri.EscapeDataString("ITG_"));

            Assert.Equal(HttpStatusCode.OK, goiY.Status);
            using var doc = JsonDocument.Parse(goiY.Html);
            var root = doc.RootElement;
            Assert.Contains(root.GetProperty("hocViens").EnumerateArray(),
                e => e.GetProperty("id").GetInt32() == hv.MaHV);
            Assert.Contains(root.GetProperty("giaSus").EnumerateArray(),
                e => e.GetProperty("id").GetInt32() == gs.MaGS);
            Assert.Contains(root.GetProperty("lops").EnumerateArray(),
                e => e.GetProperty("id").GetInt32() == lop.MaLop);

            var trang = await ql.Get("/TimKiem?q=" + Uri.EscapeDataString("ITG_"));
            Assert.Equal(HttpStatusCode.OK, trang.Status);
            Assert.Contains(hv.HoTen, trang.Chu);
            Assert.Contains(gs.HoTen, trang.Chu);
            Assert.Contains("/HocVien/Details/" + hv.MaHV, trang.Html);
            Assert.Contains("/LopHoc/Details/" + lop.MaLop, trang.Html);
        }
        finally
        {
            DonDepItg(Db);
        }
    }

    [Fact]
    [Trait("Luat", "G9")]
    public async Task Tu_khoa_qua_ngan_tra_rong()
    {
        var hv = TaoHocVien(TenItg(), SdtTest());
        TaoLop(GS, hv, "ITG_Toán");

        try
        {
            var ql = await DangNhapQL();
            var trang = await ql.Get("/TimKiem/GoiY?q=a");

            Assert.Equal(HttpStatusCode.OK, trang.Status);
            using var doc = JsonDocument.Parse(trang.Html);
            var root = doc.RootElement;
            Assert.Equal(0, root.GetProperty("hocViens").GetArrayLength());
            Assert.Equal(0, root.GetProperty("giaSus").GetArrayLength());
            Assert.Equal(0, root.GetProperty("lops").GetArrayLength());
        }
        finally
        {
            DonDepItg(Db);
        }
    }

    [Fact]
    [Trait("Luat", "G9")]
    public async Task O_tim_kiem_co_tren_thanh_menu_va_nap_js_css()
    {
        var ql = await DangNhapQL();

        var trang = await ql.Get("/TimKiem?q=" + Uri.EscapeDataString("ITG_"));

        Assert.Equal(HttpStatusCode.OK, trang.Status);
        Assert.NotNull(trang.Doc.QuerySelector("#vo-tim-kiem-form"));
        Assert.NotNull(trang.Doc.QuerySelector("#vo-tim-kiem-q"));
        Assert.NotNull(trang.Doc.QuerySelector("link[href*='/css/tim-kiem.css']"));
        Assert.NotNull(trang.Doc.QuerySelector("script[src*='/js/tim-kiem.js']"));
    }
}
