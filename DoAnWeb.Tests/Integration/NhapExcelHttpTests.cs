using DoAnWeb.Services.Excel;
using System.Net;
using System.IO.Compression;
using System.Text;
using ClosedXML.Excel;
using DoAnWeb.Data;
using DoAnWeb.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests.Integration;

public sealed class NhapExcelHttpTests : HttpTestBase
{
    [Fact]
    public async Task NhapExcel_dong_hop_le_duoc_nhap_va_dong_loi_co_ly_do()
    {
        var ql = await DangNhapQL();
        string ten = TenHoSoItg();
        string sdt = SdtTest();
        byte[] xlsx = TaoExcel(ws =>
        {
            string[] cot = { "HoTen", "SDT", "DiaChi", "TenPhuHuynh", "SDTPhuHuynh", "TruongLop", "TrinhDo", "GhiChu" };
            for (int i = 0; i < cot.Length; i++) ws.Cell(1, i + 1).Value = cot[i];
            ws.Cell(2, 1).Value = ten; ws.Cell(2, 2).Value = sdt;
            ws.Cell(3, 1).Value = ""; ws.Cell(3, 2).Value = "sai-sdt";
        });
        var xem = await TaiLen(ql, xlsx, "mau.xlsx");
        Assert.Equal(HttpStatusCode.OK, xem.StatusCode);
        string html = await xem.Content.ReadAsStringAsync();
        Assert.True(Text(html).Contains("Hợp lệ"), Text(html));
        Assert.Contains("Lỗi", Text(html));

        string maPhien = GiaTriInput(html, "maPhien");
        string token = GiaTriInput(html, "__RequestVerificationToken");
        using var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("maPhien", maPhien),
            new KeyValuePair<string, string>("__RequestVerificationToken", token)
        });
        var nhap = await ql.Client.PostAsync("/NhapExcel/Nhap", form);
        Assert.Equal(HttpStatusCode.Redirect, nhap.StatusCode);
        using var db = Moi();
        Assert.True(db.HocViens.Any(h => h.HoTen == ten && h.SDT == sdt));
    }

    [Fact]
    public async Task NhapExcel_bao_trung_SDT_trong_tap_tin_va_trong_CSDL()
    {
        var daCo = TaoGiaSu(TenItg());
        var ql = await DangNhapQL();
        byte[] xlsx = TaoExcel(ws =>
        {
            string[] cot = { "HoTen", "SDT", "ChuyenMon", "Email", "HocVan", "KinhNghiem", "GhiChu" };
            for (int i = 0; i < cot.Length; i++) ws.Cell(1, i + 1).Value = cot[i];
            ws.Cell(2, 1).Value = "Itg Mot"; ws.Cell(2, 2).Value = daCo.SDT; ws.Cell(2, 3).Value = "Toán";
            ws.Cell(3, 1).Value = "Itg Hai"; ws.Cell(3, 2).Value = daCo.SDT; ws.Cell(3, 3).Value = "Toán";
        });
        var xem = await TaiLen(ql, xlsx, "trung.xlsx", "GiaSu");
        string html = await xem.Content.ReadAsStringAsync();
        string text = Text(html);
        Assert.Contains("trùng trong tệp", text);
        Assert.Contains("đã tồn tại trong CSDL", text);
    }

    [Fact]
    public async Task NhapExcel_tim_trung_so_dien_thoai_hoc_vien_va_phu_huynh_trong_CSDL()
    {
        string sdtPhuHuynh = SdtTest();
        var hocVienDaCo = TaoHocVien(TenItg(), SdtTest());
        hocVienDaCo.SDTPhuHuynh = sdtPhuHuynh;
        Db.SaveChanges();
        var ql = await DangNhapQL();
        byte[] xlsx = TaoExcel(ws =>
        {
            string[] cot = { "HoTen", "SDT", "DiaChi", "TenPhuHuynh", "SDTPhuHuynh", "TruongLop", "TrinhDo", "GhiChu" };
            for (int i = 0; i < cot.Length; i++) ws.Cell(1, i + 1).Value = cot[i];
            ws.Cell(2, 1).Value = TenItg(); ws.Cell(2, 2).Value = SdtTest(); ws.Cell(2, 5).Value = sdtPhuHuynh;
        });

        var xem = await TaiLen(ql, xlsx, "hoc-vien-trung.xlsx");

        Assert.Contains("đã tồn tại trong CSDL", Text(await xem.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task NhapExcel_chan_gia_su_va_tu_choi_tep_khong_phai_xlsx()
    {
        var gs = await DangNhapGS();
        Assert.True((await gs.Get("/NhapExcel")).BiChan);

        var ql = await DangNhapQL();
        var bad = await TaiLen(ql, System.Text.Encoding.UTF8.GetBytes("khong phai excel"), "dulieu.csv");
        Assert.Equal(HttpStatusCode.OK, bad.StatusCode);
        Assert.Contains("Chỉ chấp nhận tệp .xlsx", Text(await bad.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task NhapExcel_tu_choi_archive_co_dung_luong_giai_nen_qua_lon()
    {
        var ql = await DangNhapQL();
        byte[] zipBomb;
        using (var stream = new MemoryStream())
        {
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            using (var writer = new StreamWriter(zip.CreateEntry("xl/sharedStrings.xml").Open(), Encoding.UTF8))
                writer.Write(new string('x', NhapExcelService.GioiHanGiaiNenByte + 1));
            zipBomb = stream.ToArray();
        }

        Assert.True(zipBomb.Length < NhapExcelService.GioiHanByte);
        var response = await TaiLen(ql, zipBomb, "qua-lon.xlsx");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("vượt giới hạn dung lượng giải nén", Text(await response.Content.ReadAsStringAsync()));
    }

    private static async Task<HttpResponseMessage> TaiLen(TrinhDuyet ql, byte[] noiDung, string tenTep, string loai = "HocVien")
    {
        var trang = await ql.Get("/NhapExcel");
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(trang.Token), "__RequestVerificationToken");
        form.Add(new StringContent(loai), "loaiHoSo");
        var tep = new ByteArrayContent(noiDung);
        tep.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.Add(tep, "tep", tenTep);
        return await ql.Client.PostAsync("/NhapExcel/XemTruoc", form);
    }

    private static byte[] TaoExcel(Action<IXLWorksheet> ghi)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("DuLieu");
        ghi(ws);
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static string GiaTriInput(string html, string name)
    {
        var doc = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(html);
        return doc.QuerySelector($"input[name='{name}']")?.GetAttribute("value");
    }

    private static string Text(string html) => new AngleSharp.Html.Parser.HtmlParser().ParseDocument(html).Body.TextContent;
}
