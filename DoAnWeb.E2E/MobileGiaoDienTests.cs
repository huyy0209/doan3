using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace DoAnWeb.E2E;

/// <summary>Kiểm tra các màn hình chính ở hai kích thước điện thoại thường dùng.</summary>
[Collection("E2E")]
public sealed class MobileGiaoDienTests : GiaoDienTestBase
{
    public MobileGiaoDienTests(GiaoDienFixture f) : base(f) { }

    [Fact]
    public async Task QuanLyVaGiaSu_MoiTrangChinh_HienThiTotTrenDienThoai()
    {
        int maPhieu;
        using (var db = DoAnWeb.Tests.TestDb.TaoContext())
        {
            var phieu = new DoAnWeb.Data.Entities.ThanhToan
            {
                MaLop = Du.LopHocPhi.MaLop,
                SoTien = 100000,
                NgayDong = DateTime.Today,
                HinhThuc = "Tiền mặt",
                GhiChu = "E2E_mobile"
            };
            db.ThanhToans.Add(phieu);
            db.SaveChanges();
            maPhieu = phieu.MaTT;
        }

        try
        {
            await KiemTraVaiTro(Du.QuanLy.TenDangNhap, true, new ViewportSize { Width = 390, Height = 844 }, maPhieu);
            await KiemTraVaiTro(Du.QuanLy.TenDangNhap, true, new ViewportSize { Width = 360, Height = 740 }, maPhieu);
            await KiemTraVaiTro(Du.TaiKhoanGS.TenDangNhap, false, new ViewportSize { Width = 390, Height = 844 }, maPhieu);
            await KiemTraVaiTro(Du.TaiKhoanGS.TenDangNhap, false, new ViewportSize { Width = 360, Height = 740 }, maPhieu);
        }
        finally
        {
            using var db = DoAnWeb.Tests.TestDb.TaoContext();
            var phieu = db.ThanhToans.Find(maPhieu);
            if (phieu != null)
            {
                db.ThanhToans.Remove(phieu);
                db.SaveChanges();
            }
        }
    }

    private async Task KiemTraVaiTro(string taiKhoan, bool quanLy, ViewportSize khung, int maPhieu)
    {
        await Chay($"mobile-{(quanLy ? "quan-ly" : "gia-su")}-{khung.Width}", async trang =>
        {
            await DangNhap(trang, taiKhoan);
            var menu = await trang.Locator("nav.vo-nav a.vo-nav-link")
                .EvaluateAllAsync<string[]>("links => links.map(link => link.getAttribute('href'))");
            Assert.NotEmpty(menu);

            var cacTrang = new List<string>(menu);
            if (quanLy)
            {
                cacTrang.AddRange(new[]
                {
                    "/CaiDat", "/HocVien/Create", $"/HocVien/Edit/{Du.HV1.MaHV}",
                    "/GiaSu/Create", $"/GiaSu/Edit/{Du.GS_A.MaGS}", "/LopHoc/GhepLop",
                    $"/LopHoc/Details/{Du.LopNen.MaLop}", $"/LopHoc/Edit/{Du.LopNen.MaLop}",
                    "/HocTap", $"/HocTap/KetQua/{Du.HV1.MaHV}", "/HocPhi", $"/HocPhi/PhieuThu/{maPhieu}", "/NhacNo",
                    "/BaoCao", "/NhatKy", "/NhapExcel", "/SaoLuu", "/TimKiem", "/MonHoc"
                });
            }
            else
            {
                cacTrang.AddRange(new[] { "/DiemDanh", "/HocTap", "/Lich", "/LichDay" });
            }

            foreach (string duongDan in cacTrang.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                await trang.SetViewportSizeAsync(khung.Width, khung.Height);
                await trang.GotoAsync(Url(duongDan), new() { WaitUntil = WaitUntilState.DOMContentLoaded });
                await Expect(trang.Locator("body")).ToBeVisibleAsync();

                bool tranNgang = await trang.EvaluateAsync<bool>(
                    "() => document.documentElement.scrollWidth > window.innerWidth + 1");
                Assert.False(tranNgang, $"{duongDan} bị cuộn ngang ở khung {khung.Width}px.");

                string[] nutQuaNho = await trang.Locator("button:visible, .btn:visible, .vo-nav-link:visible, .vo-foot-link:visible, .vo-pager a:visible")
                    .EvaluateAllAsync<string[]>("items => items.filter(e => { const r=e.getBoundingClientRect(); return r.width < 40 || r.height < 40; }).map(e => `${e.tagName}.${e.className}: ${Math.round(e.getBoundingClientRect().width)}x${Math.round(e.getBoundingClientRect().height)}`)");
                Assert.True(nutQuaNho.Length == 0, $"{duongDan} có nút/link nhỏ hơn 40px: {string.Join(", ", nutQuaNho)}");

                string[] inputQuaNho = await trang.Locator("input:not([type=hidden]):not([type=checkbox]):not([type=radio]):visible, select:visible, textarea:visible")
                    .EvaluateAllAsync<string[]>("items => items.filter(e => parseFloat(getComputedStyle(e).fontSize) < 16).map(e => `${e.tagName}#${e.id}: ${getComputedStyle(e).fontSize}`)");
                Assert.True(inputQuaNho.Length == 0, $"{duongDan} có ô nhập chữ nhỏ hơn 16px: {string.Join(", ", inputQuaNho)}");
            }
        }, khung, "mobile");
    }
}
