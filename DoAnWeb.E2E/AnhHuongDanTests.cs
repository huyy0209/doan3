using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace DoAnWeb.E2E;

public class AnhHuongDanTests : GiaoDienTestBase
{
    private string[][] _duLieuCanAn = Array.Empty<string[]>();

    public AnhHuongDanTests(GiaoDienFixture f) : base(f) { }

    [Fact]
    [Trait("Loai", "ChupAnh")]
    public async Task ChupCacTrangChoTaiLieu()
    {
        _duLieuCanAn = LayDuLieuCanAn();
        ChiGiuDuLieuE2E();

        await Chay(nameof(ChupCacTrangChoTaiLieu), async trang =>
        {
            await trang.GotoAsync(Url("/TaiKhoan/DangNhap"));
            await Expect(trang.Locator(".vo-auth-title")).ToBeVisibleAsync();
            await ChupAnh(trang, "dang-nhap.png");

            await DangNhap(trang, Du.QuanLy.TenDangNhap);
            var cacTrang = new (string DuongDan, string TieuDe, string Tep)[]
            {
                ("/TongQuan", "Tổng quan", "tong-quan.png"),
                ("/GiaSu", "Gia sư", "gia-su.png"),
                ("/HocVien", "Học viên", "hoc-vien.png"),
                ("/LopHoc", "Lớp học", "lop-hoc.png"),
                ("/LopHoc/GhepLop", "Ghép lớp mới", "ghep-lop.png"),
                ("/MonHoc", "Môn học", "mon-hoc.png"),
                ("/Lich", "Lịch học", "lich-hoc.png"),
                ("/LichDay", "Thời khóa biểu", "thoi-khoa-bieu.png"),
                ("/DiemDanh", "Điểm danh", "diem-danh.png"),
                ($"/HocPhi?maLop={Du.LopHocPhi.MaLop}", "Học phí", "hoc-phi.png"),
                ("/NhacNo", "Nhắc nợ", "nhac-no.png"),
                ("/NhatKy", "Nhật ký thao tác", "nhat-ky.png"),
                ("/NhapExcel", "Nhập học viên / gia sư từ Excel", "nhap-excel.png"),
                ("/SaoLuu", "Sao lưu CSDL", "sao-luu.png"),
                ("/CaiDat", "Cài đặt trung tâm", "cai-dat.png"),
                ("/QuanLyTaiKhoan", "Quản lý tài khoản", "tai-khoan.png"),
                ("/HocTap", "Học viên & điểm", "hoc-tap.png"),
                ($"/HocTap/KetQua/{Du.HV3.MaHV}", "Kết quả học tập", "ket-qua.png"),
                ("/TimKiem?q=" + Uri.EscapeDataString(Du.HV2.HoTen), "Tìm kiếm", "tim-kiem.png")
            };

            foreach (var (duongDan, tieuDe, tep) in cacTrang)
            {
                await trang.GotoAsync(Url(duongDan));
                await Expect(trang.Locator(".vo-page-title")).ToHaveTextAsync(tieuDe);
                await ChupAnh(trang, tep);
            }

            await trang.GotoAsync(Url($"/HocPhi?maLop={Du.LopHocPhi.MaLop}"));
            await trang.FillAsync("#ThanhToan_SoTien", "100.000");
            await trang.FillAsync("#ThanhToan_HinhThuc", "Tiền mặt");
            await trang.ClickAsync("form.w6-payment-form button[type=submit]");
            await CoThongBao(trang, "Đã ghi nhận");
            string duongDanPhieu = await trang.Locator(".w6-history-scroll a:has-text('In phiếu thu')")
                .GetAttributeAsync("href");
            var maThanhToan = Regex.Match(duongDanPhieu, @"/PhieuThu/(\d+)$");
            Assert.True(maThanhToan.Success, "Không lấy được mã phiếu thu từ liên kết in.");
            await trang.GotoAsync(Url(duongDanPhieu));
            await Expect(trang.Locator(".vo-page-title")).ToHaveTextAsync("Phiếu thu học phí");
            await ChupAnh(trang, "phieu-thu.png");
            using (var db = DoAnWeb.Tests.TestDb.TaoContext())
            {
                var thanhToan = db.ThanhToans.Single(t => t.MaTT == int.Parse(maThanhToan.Groups[1].Value));
                db.ThanhToans.Remove(thanhToan);
                db.SaveChanges();
            }

            await trang.GotoAsync(Url("/TongQuan"));
            await trang.Locator("[data-theme-toggle]").ClickAsync();
            await trang.Locator("[data-theme-toggle]").ClickAsync();
            await Expect(trang.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark");
            await ChupAnh(trang, "giao-dien-toi.png");
        }, Desktop);

        await Chay(nameof(ChupCacTrangChoTaiLieu) + "-dien-thoai", async trang =>
        {
            await DangNhap(trang, Du.TaiKhoanGS.TenDangNhap);
            await Expect(trang.Locator(".vo-page-title")).ToHaveTextAsync("Điểm danh");
            await ChupAnh(trang, "gia-su-diem-danh.png");

            await trang.GotoAsync(Url("/LichDay"));
            await Expect(trang.Locator(".vo-page-title")).ToHaveTextAsync("Thời khóa biểu");
            await ChupAnh(trang, "gia-su-thoi-khoa-bieu.png");

            await trang.SetViewportSizeAsync(DienThoai.Width, DienThoai.Height);
            string homNay = DateTime.Today.ToString("yyyy-MM-dd");
            await trang.GotoAsync(Url($"/DiemDanh?ngay={homNay}"));
            await Expect(trang.Locator("table.dd-table thead")).ToBeHiddenAsync();
            await ChupAnh(trang, "diem-danh-dien-thoai.png");
        }, DienThoai);
    }

    private string[][] LayDuLieuCanAn()
    {
        using var db = DoAnWeb.Tests.TestDb.TaoContext();
        return db.CaiDats.AsNoTracking()
            .Where(c => c.GiaTri != null && c.GiaTri.Length >= 3)
            .Select(c => new[]
            {
                c.GiaTri,
                c.Khoa == "TenTrungTam" ? "Trung tâm gia sư" :
                    c.Khoa == "DiaChi" ? "Địa chỉ mẫu" : "0900000000"
            })
            .ToArray();
    }

    private static void ChiGiuDuLieuE2E()
    {
        using var db = DoAnWeb.Tests.TestDb.TaoContext();
        db.Database.ExecuteSqlRaw(
            "DELETE FROM NhatKy;" +
            "DELETE FROM BuoiHoc WHERE MaLop NOT IN (SELECT MaLop FROM LopHoc WHERE TenLop LIKE 'E2E[_]%');" +
            "DELETE FROM ThanhToan WHERE MaLop NOT IN (SELECT MaLop FROM LopHoc WHERE TenLop LIKE 'E2E[_]%');" +
            "DELETE FROM LopHoc WHERE TenLop NOT LIKE 'E2E[_]%';" +
            "DELETE FROM TaiKhoan WHERE TenDangNhap NOT LIKE 'e2e[_]%';" +
            "DELETE FROM GiaSu WHERE HoTen NOT LIKE 'E2E[_]%';" +
            "DELETE FROM HocVien WHERE HoTen NOT LIKE 'E2E[_]%';" +
            "DELETE FROM MonHoc WHERE TenMon NOT LIKE 'E2E[_]%';" +
            "DELETE FROM CaiDat;");
    }

    private async Task ChupAnh(IPage trang, string tenTep)
    {
        await trang.EvaluateAsync(@"giaTriCanAn => {
            const thay = text => {
                for (const [value, replacement] of giaTriCanAn) {
                    if (value) text = text.split(value).join(replacement);
                }
                return text;
            };
            const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
            const nodes = [];
            while (walker.nextNode()) nodes.push(walker.currentNode);
            for (const node of nodes) node.nodeValue = thay(node.nodeValue || '');
            for (const input of document.querySelectorAll('input, textarea'))
                input.value = thay(input.value || '');
        }", _duLieuCanAn);
        var thuMuc = Path.Combine(GocKhoMa(), "docs", "anh");
        Directory.CreateDirectory(thuMuc);
        var duongDan = Path.Combine(thuMuc, tenTep);
        var anh = await trang.ScreenshotAsync(new PageScreenshotOptions { Path = duongDan });

        Assert.True(anh.Length <= 200 * 1024, $"{tenTep} vượt quá 200 KB ({anh.Length} byte).");
        Assert.True(CoNoiDung(anh), $"{tenTep} là ảnh trắng hoặc không đọc được.");
    }

    private static string GocKhoMa()
    {
        var thuMuc = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        return thuMuc.Parent!.Parent!.Parent!.Parent!.FullName;
    }

    private static bool CoNoiDung(byte[] png)
    {
        byte[] chuKy = { 137, 80, 78, 71, 13, 10, 26, 10 };
        if (png.Length < chuKy.Length || !png.AsSpan(0, chuKy.Length).SequenceEqual(chuKy))
            return false;

        int rong = 0, cao = 0, bitDepth = 0, kieuMau = 0, interlace = 0;
        using var idat = new MemoryStream();
        int viTri = chuKy.Length;
        while (viTri + 12 <= png.Length)
        {
            int doDai = checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(viTri, 4)));
            if (doDai < 0 || viTri + 12L + doDai > png.Length)
                return false;

            string loai = Encoding.ASCII.GetString(png, viTri + 4, 4);
            int duLieu = viTri + 8;
            if (loai == "IHDR")
            {
                rong = checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(duLieu, 4)));
                cao = checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(duLieu + 4, 4)));
                bitDepth = png[duLieu + 8];
                kieuMau = png[duLieu + 9];
                interlace = png[duLieu + 12];
            }
            else if (loai == "IDAT")
            {
                idat.Write(png, duLieu, doDai);
            }
            else if (loai == "IEND")
            {
                break;
            }

            viTri += 12 + doDai;
        }

        if (rong <= 0 || cao <= 0 || bitDepth != 8 || interlace != 0 || kieuMau is not (2 or 6))
            return false;

        int soKenh = kieuMau == 6 ? 4 : 3;
        int doDaiHang = checked(rong * soKenh);
        var duLieuGiaiNen = new byte[checked(cao * (doDaiHang + 1))];
        idat.Position = 0;
        using (var zlib = new ZLibStream(idat, CompressionMode.Decompress, leaveOpen: true))
            zlib.ReadExactly(duLieuGiaiNen);

        var hangTruoc = new byte[doDaiHang];
        var hang = new byte[doDaiHang];
        int viTriDuLieu = 0;
        for (int y = 0; y < cao; y++)
        {
            int boLoc = duLieuGiaiNen[viTriDuLieu++];
            Array.Copy(duLieuGiaiNen, viTriDuLieu, hang, 0, doDaiHang);
            viTriDuLieu += doDaiHang;
            for (int i = 0; i < doDaiHang; i++)
            {
                int trai = i >= soKenh ? hang[i - soKenh] : 0;
                int tren = hangTruoc[i];
                int trenTrai = i >= soKenh ? hangTruoc[i - soKenh] : 0;
                int giaTri = hang[i];
                int giaTriBoLoc = boLoc switch
                {
                    0 => 0,
                    1 => trai,
                    2 => tren,
                    3 => (trai + tren) / 2,
                    4 => Paeth(trai, tren, trenTrai),
                    _ => throw new InvalidDataException("Bộ lọc PNG không hợp lệ.")
                };
                hang[i] = unchecked((byte)(giaTri + giaTriBoLoc));
            }

            for (int i = 0; i < doDaiHang; i += soKenh)
                if (hang[i] < 245 || hang[i + 1] < 245 || hang[i + 2] < 245)
                    return true;

            (hang, hangTruoc) = (hangTruoc, hang);
        }

        return false;
    }

    private static int Paeth(int trai, int tren, int trenTrai)
    {
        int p = trai + tren - trenTrai;
        int dTrai = Math.Abs(p - trai);
        int dTren = Math.Abs(p - tren);
        int dTrenTrai = Math.Abs(p - trenTrai);
        return dTrai <= dTren && dTrai <= dTrenTrai ? trai :
            dTren <= dTrenTrai ? tren : trenTrai;
    }
}
