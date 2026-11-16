using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DoAnWeb.Data.Entities;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>
/// Kiểm tra khả năng truy cập trên HTML thật: nhãn của ô nhập, phạm vi ô tiêu đề bảng,
/// tên truy cập của nút, alt của ảnh, thông báo có role và modal có tiêu đề.
/// </summary>
public class KhaNangTruyCapHttpTests : HttpTestBase
{
    private static readonly HashSet<string> LoaiInputKhongCanNhan = new(StringComparer.OrdinalIgnoreCase)
    {
        "hidden", "submit", "button", "reset", "image"
    };

    private async Task<(int MaLop, int MaHV, int MaNK)> TaoDuLieu()
    {
        var hv = TaoHocVien(TenItg());
        var lop = TaoLop(GS, hv);
        Db.BuoiHocs.Add(new BuoiHoc
        {
            MaLop = lop.MaLop, NgayHoc = DateTime.Today.AddDays(-1),
            DiemDanh = "Có mặt", Diem = 8, CaHoc = lop.CaHoc
        });
        Db.ThanhToans.Add(new ThanhToan
        {
            MaLop = lop.MaLop, SoTien = 50000, NgayDong = DateTime.Now, HinhThuc = "Tiền mặt"
        });
        var nk = new NhatKy
        {
            ThoiGian = DateTime.Now, MaTK = QL.MaTK, TenDangNhap = QL.TenDangNhap,
            HanhDong = "Tạo", Bang = "GiaSu", KhoaChinh = GS.MaGS.ToString(),
            ChiTiet = "{\"HoTen\":\"ITG\"}", DiaChiIP = "127.0.0.1"
        };
        Db.NhatKys.Add(nk);
        Db.SaveChanges();
        await Task.CompletedTask;
        return (lop.MaLop, hv.MaHV, nk.MaNK);
    }

    private static readonly string[] TrangQuanLy =
    {
        "/TongQuan", "/GiaSu", "/GiaSu/Create", "/HocVien", "/HocVien/Create",
        "/LopHoc", "/LopHoc/GhepLop", "/HocPhi", "/DiemDanh", "/HocTap",
        "/MonHoc", "/NhacNo", "/NhatKy", "/QuanLyTaiKhoan", "/QuanLyTaiKhoan/DatLaiMatKhau/{0}",
        "/CaiDat", "/SaoLuu", "/Lich", "/LichDay", "/NhapExcel", "/TimKiem?q=ITG", "/TaiKhoan/DoiMatKhau"
    };

    [Fact]
    public async Task Trang_quan_ly_dat_yeu_cau_kha_nang_truy_cap()
    {
        var (maLop, maHV, maNK) = await TaoDuLieu();
        var ql = await DangNhapQL();
        var urls = TrangQuanLy
            .Select(u => string.Format(u, QL.MaTK))
            .Concat(new[]
            {
                $"/GiaSu/Details/{GS.MaGS}", $"/HocVien/Details/{maHV}", $"/LopHoc/Details/{maLop}",
                $"/HocPhi/BangKe?maLop={maLop}", $"/HocTap/KetQua/{maHV}", $"/NhatKy/ChiTiet/{maNK}",
                $"/LichDay/Details/{maLop}"
            });

        foreach (var url in urls)
        {
            var t = await ql.Get(url);
            Assert.Equal(System.Net.HttpStatusCode.OK, t.Status);
            KiemTra(t);
        }

        var khach = Khach();
        var dangNhap = await khach.Get("/TaiKhoan/DangNhap");
        Assert.Equal(System.Net.HttpStatusCode.OK, dangNhap.Status);
        KiemTra(dangNhap);

        var khongTimThay = await khach.Get("/Loi/KhongTimThay");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, khongTimThay.Status);
        KiemTra(khongTimThay);
    }

    [Fact]
    public async Task Trang_gia_su_dat_yeu_cau_kha_nang_truy_cap()
    {
        var (_, maHV, _) = await TaoDuLieu();
        var gs = await DangNhapGS();

        foreach (var url in new[] { "/HocVien", $"/HocVien/Details/{maHV}", "/DiemDanh", "/HocTap", "/Lich", "/LichDay" })
        {
            var t = await gs.Get(url);
            Assert.Equal(System.Net.HttpStatusCode.OK, t.Status);
            KiemTra(t);
        }
    }

    private static void KiemTra(Trang t)
    {
        var loi = new List<string>();

        foreach (var el in t.Doc.QuerySelectorAll("input, select, textarea"))
        {
            if (el is IHtmlInputElement inp && LoaiInputKhongCanNhan.Contains(inp.Type ?? "text")) continue;
            if (el.HasAttribute("disabled")) continue;
            if (string.IsNullOrWhiteSpace(TenTruyCap(t.Doc, el)))
                loi.Add($"Ô nhập thiếu nhãn: <{el.TagName.ToLowerInvariant()} name='{el.GetAttribute("name")}' id='{el.GetAttribute("id")}'>");
        }

        foreach (var th in t.Doc.QuerySelectorAll("thead th"))
        {
            if (!string.Equals(th.GetAttribute("scope"), "col", StringComparison.OrdinalIgnoreCase))
                loi.Add($"Ô tiêu đề bảng thiếu scope=\"col\": {th.TextContent.Trim()}");
        }

        foreach (var th in t.Doc.QuerySelectorAll("tbody th"))
        {
            if (!string.Equals(th.GetAttribute("scope"), "row", StringComparison.OrdinalIgnoreCase))
                loi.Add($"Ô tiêu đề hàng thiếu scope=\"row\": {th.TextContent.Trim()}");
        }

        foreach (var nut in t.Doc.QuerySelectorAll("button"))
        {
            if (nut.HasAttribute("disabled")) continue;
            string ten = TenTruyCap(t.Doc, nut);
            if (string.IsNullOrWhiteSpace(ten))
                loi.Add($"Nút thiếu tên truy cập: class='{nut.GetAttribute("class")}'");
            bool chiCoIcon = nut.QuerySelector("svg, .vo-icon") != null && string.IsNullOrWhiteSpace(nut.TextContent);
            if (chiCoIcon && (string.IsNullOrWhiteSpace(nut.GetAttribute("aria-label"))
                || string.IsNullOrWhiteSpace(nut.GetAttribute("title"))))
                loi.Add($"Nút chỉ có icon cần aria-label và title: class='{nut.GetAttribute("class")}'");
        }

        foreach (var anh in t.Doc.QuerySelectorAll("img"))
        {
            if (!anh.HasAttribute("alt"))
                loi.Add($"Ảnh thiếu alt: src='{anh.GetAttribute("src")}'");
            else if (string.IsNullOrWhiteSpace(anh.GetAttribute("alt")))
                loi.Add($"Ảnh nội dung có alt rỗng: src='{anh.GetAttribute("src")}'");
        }

        foreach (var tb in t.Doc.QuerySelectorAll(".alert"))
        {
            if (!tb.HasAttribute("role") && !tb.HasAttribute("aria-live"))
                loi.Add($"Thông báo thiếu role/aria-live: class='{tb.GetAttribute("class")}'");
        }

        foreach (var modal in t.Doc.QuerySelectorAll(".modal"))
        {
            string dinh = modal.GetAttribute("aria-labelledby");
            if (string.IsNullOrWhiteSpace(dinh) || t.Doc.GetElementById(dinh) == null)
                loi.Add($"Modal thiếu aria-labelledby trỏ tới tiêu đề: id='{modal.GetAttribute("id")}'");
        }

        Assert.True(loi.Count == 0, $"{t.Url} có {loi.Count} lỗi khả năng truy cập:\n - " + string.Join("\n - ", loi.Distinct()));
    }

    /// <summary>Tên truy cập theo WCAG: aria-label, aria-labelledby, label[for], label bọc ngoài hoặc nội dung.</summary>
    private static string TenTruyCap(IHtmlDocument doc, IElement el)
    {
        string nhan = el.GetAttribute("aria-label");
        if (!string.IsNullOrWhiteSpace(nhan)) return nhan.Trim();

        string dinh = el.GetAttribute("aria-labelledby");
        if (!string.IsNullOrWhiteSpace(dinh))
        {
            var chu = string.Join(" ", dinh.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => doc.GetElementById(id)?.TextContent?.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x)));
            if (!string.IsNullOrWhiteSpace(chu)) return chu;
        }

        string id = el.GetAttribute("id");
        if (!string.IsNullOrWhiteSpace(id))
        {
            var lab = doc.QuerySelector($"label[for='{id.Replace("'", "\\'")}']");
            if (lab != null) return lab.TextContent;
        }

        var boc = el.Ancestors<IHtmlLabelElement>().FirstOrDefault();
        if (boc != null) return boc.TextContent;

        if (el is IHtmlButtonElement || el.TagName.Equals("button", StringComparison.OrdinalIgnoreCase)
            || el.TagName.Equals("a", StringComparison.OrdinalIgnoreCase))
            return el.TextContent;

        return null;
    }
}
