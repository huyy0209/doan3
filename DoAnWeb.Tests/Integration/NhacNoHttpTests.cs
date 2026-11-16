using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ClosedXML.Excel;
using DoAnWeb.Data.Entities;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>
/// G6: trang /NhacNo – chỉ quản lý, số liệu khớp trang Học phí, lọc theo môn/mức nợ hoạt động,
/// nút sao chép tin nhắn có dữ liệu cho file js và Excel xuất đúng danh sách đang lọc.
/// Dữ liệu dùng tiền tố ITG_ của <see cref="HttpTestBase"/>.
/// </summary>
public class NhacNoHttpTests : HttpTestBase
{
    private const string MimeXlsx =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>Lớp còn nợ 90.000đ (học phí 200.000, đã đóng 110.000 cách đây 3 ngày).</summary>
    private (HocVien Hv, LopHoc Lop, string Mon) TaoLopConNo(string mon = null)
    {
        var hv = TaoHocVien(TenItg());
        var lop = TaoLop(GS, hv, mon ?? ("ITG_Mon" + Guid.NewGuid().ToString("N").Substring(0, 6)),
            hocPhi: 200000);
        Db.ThanhToans.Add(new ThanhToan
        {
            MaLop = lop.MaLop,
            SoTien = 110000,
            NgayDong = DateTime.Today.AddDays(-3),
            HinhThuc = "Tiền mặt"
        });
        Db.SaveChanges();
        return (hv, lop, lop.MonHoc);
    }

    [Fact]
    [Trait("Luat", "NO-05")]
    public async Task GiaSu_bi_chan_va_khach_bi_ve_dang_nhap()
    {
        var gs = await DangNhapGS();
        Assert.True((await gs.Get("/NhacNo")).BiChan);
        Assert.True((await gs.Get("/NhacNo/XuatExcel")).BiChan);

        var khach = Khach();
        Assert.True((await khach.Get("/NhacNo")).VeDangNhap);
    }

    [Fact]
    [Trait("Luat", "NO-05")]
    public async Task QuanLy_thay_dung_so_no_va_du_lieu_sao_chep()
    {
        var (hv, lop, mon) = TaoLopConNo();
        var ql = await DangNhapQL();

        var trang = await ql.Get("/NhacNo?mon=" + Uri.EscapeDataString(mon));

        Assert.Equal(HttpStatusCode.OK, trang.Status);
        Assert.Contains(hv.HoTen, trang.Chu);
        Assert.Contains("90.000", trang.Chu);
        Assert.Contains("200.000", trang.Chu);
        Assert.Contains("110.000", trang.Chu);
        Assert.Contains(DateTime.Today.AddDays(-3).ToString("dd/MM/yyyy"), trang.Chu);
        Assert.Contains("3 ngày", trang.Chu);

        var nut = trang.Doc.QuerySelectorAll(".js-nhac-no-copy")
            .OfType<IHtmlButtonElement>()
            .Single(b => b.GetAttribute("data-hoc-vien") == hv.HoTen);
        Assert.Equal("90.000", nut.GetAttribute("data-so-tien"));
        Assert.Equal(lop.MonHoc, nut.GetAttribute("data-mon"));
        Assert.False(string.IsNullOrWhiteSpace(nut.GetAttribute("data-ten-trung-tam")));
        Assert.NotNull(trang.Doc.QuerySelector("script[src*='nhac-no.js']"));
    }

    [Fact]
    [Trait("Luat", "NO-06")]
    public async Task Loc_theo_mon_va_muc_no_hoat_dong()
    {
        var (hvA, _, monA) = TaoLopConNo();
        var (hvB, lopB, monB) = TaoLopConNo();
        lopB.HocPhi = 500000;
        Db.SaveChanges();

        var ql = await DangNhapQL();

        var trangA = await ql.Get("/NhacNo?mon=" + Uri.EscapeDataString(monA));
        Assert.Contains(hvA.HoTen, trangA.Chu);
        Assert.DoesNotContain(hvB.HoTen, trangA.Chu);

        var trangB = await ql.Get("/NhacNo?mon=" + Uri.EscapeDataString(monB) + "&conNoToiThieu=300000");
        Assert.Contains(hvB.HoTen, trangB.Chu);
        Assert.DoesNotContain(hvA.HoTen, trangB.Chu);

        // Nâng mức nợ tối thiểu lên trên số nợ của B: B bị loại.
        var trangB2 = await ql.Get("/NhacNo?mon=" + Uri.EscapeDataString(monB) + "&conNoToiThieu=500000");
        Assert.DoesNotContain(hvB.HoTen, trangB2.Chu);
    }

    [Fact]
    [Trait("Luat", "NO-07")]
    public async Task XuatExcel_dung_danh_sach_dang_loc_va_co_cot_lan_dong_so_ngay()
    {
        var (hvA, _, monA) = TaoLopConNo();
        var (hvB, _, monB) = TaoLopConNo();
        var ql = await DangNhapQL();

        HttpResponseMessage r = await ql.Client.GetAsync("/NhacNo/XuatExcel?mon=" + Uri.EscapeDataString(monA));

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(MimeXlsx, r.Content.Headers.ContentType?.MediaType);
        byte[] bytes = await r.Content.ReadAsByteArrayAsync();
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(1);

        var tieuDe = ws.Row(1).CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Contains("Lần đóng gần nhất", tieuDe);
        Assert.Contains("Số ngày chưa đóng", tieuDe);

        var tenHocVien = ws.CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Contains(hvA.HoTen, tenHocVien);
        Assert.DoesNotContain(hvB.HoTen, tenHocVien);
        Assert.DoesNotContain(monB, tenHocVien);

        // Dòng của học viên A: lần đóng gần nhất = 3 ngày trước, số ngày chưa đóng = 3.
        int dongA = ws.RowsUsed().First(x => x.CellsUsed().Any(c => c.GetString() == hvA.HoTen)).RowNumber();
        Assert.Equal(DateTime.Today.AddDays(-3), ws.Cell(dongA, 8).GetDateTime().Date);
        Assert.Equal(3, ws.Cell(dongA, 9).GetValue<int>());
    }

    [Fact]
    [Trait("Luat", "NO-07")]
    public async Task XuatExcel_hoc_vien_chua_dong_lan_nao_ghi_chu_thich()
    {
        var hv = TaoHocVien(TenItg());
        string mon = "ITG_Mon" + Guid.NewGuid().ToString("N").Substring(0, 6);
        TaoLop(GS, hv, mon, batDau: DateTime.Today.AddDays(-40), hocPhi: 150000);
        var ql = await DangNhapQL();

        HttpResponseMessage r = await ql.Client.GetAsync("/NhacNo/XuatExcel?mon=" + Uri.EscapeDataString(mon));

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        using var wb = new XLWorkbook(new MemoryStream(await r.Content.ReadAsByteArrayAsync()));
        var ws = wb.Worksheet(1);
        int dong = ws.RowsUsed().First(x => x.CellsUsed().Any(c => c.GetString() == hv.HoTen)).RowNumber();
        Assert.Equal("Chưa đóng lần nào", ws.Cell(dong, 8).GetString());
        Assert.Equal(40, ws.Cell(dong, 9).GetValue<int>());
    }
}
