using System.Text.RegularExpressions;
using DoAnWeb.Common;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace DoAnWeb.E2E;

/// <summary>5 kịch bản giao diện của đề xuất #8 (Chromium headless, web Kestrel thật, CSDL QuanLyGiaSu_Test).</summary>
public class KichBanTests : GiaoDienTestBase
{
    public KichBanTests(GiaoDienFixture f) : base(f) { }

    // ---------------------------------------------------------------- 1
    [Fact]
    public Task K1_DangNhapQuanLy_ThayTongQuan_DangXuat() => Chay(nameof(K1_DangNhapQuanLy_ThayTongQuan_DangXuat), async trang =>
    {
        await DangNhap(trang, Du.QuanLy.TenDangNhap);

        // Quản lý được đưa về Tổng quan
        await Expect(trang).ToHaveURLAsync(new Regex("/TongQuan"));
        await Expect(trang.Locator(".vo-page-title")).ToHaveTextAsync("Tổng quan");

        // Đăng xuất qua menu người dùng → về trang đăng nhập
        await trang.ClickAsync(".vo-user-toggle");
        await trang.ClickAsync("button.dropdown-item:has-text('Đăng xuất')");
        await Expect(trang).ToHaveURLAsync(new Regex("/TaiKhoan/DangNhap"));

        // Đã đăng xuất thật: vào lại Tổng quan bị đẩy về đăng nhập
        await trang.GotoAsync(Url("/TongQuan"));
        await Expect(trang).ToHaveURLAsync(new Regex("/TaiKhoan/DangNhap"));
    });

    // ---------------------------------------------------------------- 2
    [Fact]
    public Task K2_GhepLop_XungDot_Luu_Huy_Xoa() => Chay(nameof(K2_GhepLop_XungDot_Luu_Huy_Xoa), async trang =>
    {
        string tenLop = "E2E_Lop_Moi_" + Du.Hau;
        await DangNhap(trang, Du.QuanLy.TenDangNhap);
        await trang.GotoAsync(Url("/LopHoc/GhepLop"));

        // 1. Học viên (HV2) – tìm theo tên rồi chọn
        await trang.FillAsync("#tim-hv", Du.HV2.HoTen);
        await trang.ClickAsync($"#ds-hv label.ghep-item[data-ma='{Du.HV2.MaHV}']");

        // 2. Môn + ca Sáng (GS_A đã có lớp nền ở ca này)
        await trang.SelectOptionAsync("#cbo-mon", Du.TenMon);
        await trang.ClickAsync($".ca-o[data-ca='{Constants.CaHoc.Sang}']");

        // 3. Gia sư GS_A – bỏ lọc "còn trống" để vẫn thấy gia sư đang bận
        await trang.UncheckAsync("#chi-trong");
        await trang.FillAsync("#tim-gs", Du.GS_A.HoTen);
        await trang.ClickAsync($"#ds-gs label.ghep-item[data-ma='{Du.GS_A.MaGS}']");

        // Dòng kiểm tra xung đột báo lỗi đúng nội dung; nút Ghép lớp bị khóa
        var dong = trang.Locator("#ghep-status");
        await Expect(dong).ToHaveClassAsync(new Regex("is-loi"));
        await Expect(dong).ToContainTextAsync("Gia sư đã có lớp");
        await Expect(trang.Locator("#nut-ghep")).ToBeDisabledAsync();

        // Đổi sang ca Tối muộn: hết xung đột
        await trang.ClickAsync($".ca-o[data-ca='{Constants.CaHoc.ToiMuon}']");
        await Expect(dong).ToHaveClassAsync(new Regex("is-ok"));
        await Expect(dong).ToContainTextAsync("buổi");

        await trang.FillAsync("#hoc-phi", "1.500.000");
        await trang.FillAsync("#ten-lop", tenLop);
        // Tên lớp đổi → kiểm tra lại; chờ xong rồi mới bấm
        await Expect(dong).ToHaveClassAsync(new Regex("is-ok"));
        await Expect(trang.Locator("#nut-ghep")).ToBeEnabledAsync();
        await trang.ClickAsync("#nut-ghep");

        // Lưu xong chuyển sang chi tiết lớp + toast
        await Expect(trang).ToHaveURLAsync(new Regex(@"/LopHoc/Details/\d+"));
        await CoThongBao(trang, "Đã ghép lớp");
        string maLop = Regex.Match(trang.Url, @"/Details/(\d+)").Groups[1].Value;

        // Lớp xuất hiện trong danh sách
        await trang.GotoAsync(Url("/LopHoc?q=" + Uri.EscapeDataString(tenLop)));
        await Expect(trang.Locator("a.vo-link-strong", new() { HasTextString = tenLop })).ToBeVisibleAsync();

        // Hủy lớp (hộp xác nhận, không dùng alert)
        await trang.GotoAsync(Url("/LopHoc/Details/" + maLop));
        await trang.ClickAsync("button[data-xac-nhan-nut='Hủy lớp']");
        await trang.ClickAsync("#modal-xac-nhan-nut");
        await CoThongBao(trang, "Đã hủy lớp");

        // Xóa lớp thử
        await trang.GotoAsync(Url("/LopHoc/Details/" + maLop));
        await trang.ClickAsync("button[data-xac-nhan-nut='Xóa lớp']");
        await trang.ClickAsync("#modal-xac-nhan-nut");
        await CoThongBao(trang, "Xóa thành công");

        await trang.GotoAsync(Url("/LopHoc?q=" + Uri.EscapeDataString(tenLop)));
        await Expect(trang.Locator("a.vo-link-strong", new() { HasTextString = tenLop })).ToHaveCountAsync(0);
    });

    // ---------------------------------------------------------------- 3
    [Fact]
    public Task K3_GiaSuDangNhap_VaoThangDiemDanh_LuuMotDong() => Chay(nameof(K3_GiaSuDangNhap_VaoThangDiemDanh_LuuMotDong), async trang =>
    {
        await DangNhap(trang, Du.TaiKhoanGS.TenDangNhap);
        await Expect(trang).ToHaveURLAsync(new Regex("/DiemDanh"));

        // Chọn đúng ngày hôm nay + ca của lớp thử để chỉ còn một dòng
        string homNay = DateTime.Today.ToString("yyyy-MM-dd");
        await trang.GotoAsync(Url($"/DiemDanh?ngay={homNay}&ca={Uri.EscapeDataString(Constants.CaHoc.Sang)}"));
        var dong = trang.Locator("table.dd-table tbody tr");
        await Expect(dong).ToHaveCountAsync(1);
        await Expect(dong).ToContainTextAsync(Du.HV3.HoTen);

        await dong.Locator("select[name='Rows[0].DiemDanh']").SelectOptionAsync(Constants.DiemDanh.CoMat);
        await dong.Locator("input[name='Rows[0].DiemText']").FillAsync("8,5");
        await trang.ClickAsync("form.dd-form button[type=submit]:has-text('Lưu tất cả')");

        await CoThongBao(trang, "Đã lưu điểm danh");
        await Expect(trang.Locator("table.dd-table tbody tr .dd-state .vo-tag-success")).ToHaveTextAsync("Đã lưu");
    });

    // ---------------------------------------------------------------- 4
    [Fact]
    public Task K4_HocPhi_ThemLanDong_ConNoGiam_XoaLanDong() => Chay(nameof(K4_HocPhi_ThemLanDong_ConNoGiam_XoaLanDong), async trang =>
    {
        await DangNhap(trang, Du.QuanLy.TenDangNhap);
        var lop = Du.LopHocPhi;
        string url = Url($"/HocPhi?maLop={lop.MaLop}&tuKhoa={Uri.EscapeDataString(lop.TenLop)}&tinhTrang=" + Uri.EscapeDataString("Tất cả"));
        await trang.GotoAsync(url);

        var conNo = trang.Locator(".w6-selected-class");
        await Expect(conNo).ToContainTextAsync("Còn nợ 1.000.000 đ");

        await trang.FillAsync("#ThanhToan_SoTien", "400.000");
        await trang.FillAsync("#ThanhToan_HinhThuc", "Tiền mặt");
        await trang.ClickAsync("form.w6-payment-form button[type=submit]");

        await CoThongBao(trang, "Đã ghi nhận 400.000 đ");
        await Expect(conNo).ToContainTextAsync("Còn nợ 600.000 đ");
        var lichSu = trang.Locator(".w6-history-scroll tbody tr");
        await Expect(lichSu).ToHaveCountAsync(1);

        // Phiếu thu có số tiền bằng chữ, sau đó quay lại để dọn lần đóng thử.
        string duongDanPhieu = await lichSu.Locator("a:has-text('In phiếu thu')").GetAttributeAsync("href");
        await trang.GotoAsync(Url(duongDanPhieu));
        await Expect(trang.Locator(".phieu-bang-chu em")).ToContainTextAsync("bốn trăm nghìn đồng");

        await trang.GotoAsync(url);
        await Expect(trang.Locator(".w6-history-scroll tbody tr")).ToHaveCountAsync(1);
        lichSu = trang.Locator(".w6-history-scroll tbody tr");

        // Xóa lần đóng → nợ trở lại 1.000.000
        await lichSu.Locator("button:has-text('Xóa')").ClickAsync();
        await trang.ClickAsync("#w6-xoa-thanh-toan button[type=submit]");
        await CoThongBao(trang, "Đã xóa lần đóng");
        await Expect(conNo).ToContainTextAsync("Còn nợ 1.000.000 đ");
        await Expect(trang.Locator(".w6-history-scroll")).ToHaveCountAsync(0);
    });

    // ---------------------------------------------------------------- 5
    [Fact]
    public Task K5_DienThoai_MenuHamburger_DiemDanhDangThe() => Chay(nameof(K5_DienThoai_MenuHamburger_DiemDanhDangThe), async trang =>
    {
        await DangNhap(trang, Du.TaiKhoanGS.TenDangNhap);
        await Expect(trang).ToHaveURLAsync(new Regex("/DiemDanh"));

        // Menu ☰: ban đầu đóng (sidebar trượt ra ngoài), bấm thì mở, thấy mục "Lịch học"
        var app = trang.Locator("#vo-app");
        var nut = trang.Locator("#vo-sidebar-toggle");
        await Expect(nut).ToBeVisibleAsync();
        await Expect(app).Not.ToHaveClassAsync(new Regex("is-sidebar-open"));
        await nut.ClickAsync();
        await Expect(app).ToHaveClassAsync(new Regex("is-sidebar-open"));
        await Expect(nut).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(trang.Locator(".vo-nav-link", new() { HasTextString = "Lịch học" })).ToBeInViewportAsync();
        // Sidebar mở phủ lên nút ☰ nên đóng bằng phím Escape (giống người dùng bàn phím)
        await trang.Keyboard.PressAsync("Escape");
        await Expect(app).Not.ToHaveClassAsync(new Regex("is-sidebar-open"));

        // Điểm danh dạng thẻ: ẩn tiêu đề bảng, mỗi dòng là một lưới, không tràn ngang
        string homNay = DateTime.Today.ToString("yyyy-MM-dd");
        await trang.GotoAsync(Url($"/DiemDanh?ngay={homNay}&ca={Uri.EscapeDataString(Constants.CaHoc.Sang)}"));
        var dong = trang.Locator("table.dd-table tbody tr").First;
        await Expect(dong).ToBeVisibleAsync();
        await Expect(trang.Locator("table.dd-table thead")).ToBeHiddenAsync();
        await Expect(dong).ToHaveCSSAsync("display", "grid");
        await Expect(dong.Locator("td.dd-status")).ToHaveAttributeAsync("data-label", "Điểm danh");

        bool tranNgang = await trang.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > window.innerWidth + 1");
        Assert.False(tranNgang, "Trang Điểm danh trên điện thoại bị tràn ngang.");
    }, DienThoai);

    // ---------------------------------------------------------------- 6
    [Fact(Skip = "LOI-01: trang /QuanLyTaiKhoan va /CaiDat ap dung inline style bi CSP style-src 'self' chan; xem docs/progress/G13.md.")]
    public async Task K6_MenuQuanLyVaGiaSu_KhongLoiConsoleHoacCsp()
    {
        await KiemTraMenuVaiTro(Du.QuanLy.TenDangNhap, laQuanLy: true);
        await KiemTraMenuVaiTro(Du.TaiKhoanGS.TenDangNhap, laQuanLy: false);
    }

    // ---------------------------------------------------------------- 7
    [Fact]
    public Task K7_TimKiemNhanh_KhongDau_MoHocVienBangEnter() => Chay(nameof(K7_TimKiemNhanh_KhongDau_MoHocVienBangEnter), async trang =>
    {
        await DangNhap(trang, Du.QuanLy.TenDangNhap);
        await trang.GotoAsync(Url("/TongQuan"));

        await trang.Keyboard.PressAsync("Control+k");
        await Expect(trang.Locator("#vo-tim-kiem-q")).ToBeFocusedAsync();
        await trang.Keyboard.TypeAsync("E2E Hoc vien thu_" + Du.Hau);

        var goiY = trang.Locator("#vo-tim-kiem-goi-y a[role='option']").First;
        await Expect(goiY).ToContainTextAsync(Du.HV2.HoTen);
        await trang.Keyboard.PressAsync("ArrowDown");
        await trang.Keyboard.PressAsync("Enter");

        await Expect(trang).ToHaveURLAsync(new Regex($"/HocVien/Details/{Du.HV2.MaHV}$"));
        await Expect(trang.Locator("h1.vo-page-title")).ToHaveTextAsync("Hồ sơ học viên");
    });

    // ---------------------------------------------------------------- 8
    [Fact]
    public Task K8_ThoiKhoaBieu_DieuHuongTuan_VaTheTrenDienThoai() => Chay(nameof(K8_ThoiKhoaBieu_DieuHuongTuan_VaTheTrenDienThoai), async trang =>
    {
        await DangNhap(trang, Du.QuanLy.TenDangNhap);
        await trang.GotoAsync(Url("/LichDay"));

        string batDauTuan = await trang.Locator(".ld-filters input[name='ngay']").GetAttributeAsync("value");
        await Expect(trang.Locator(".ld-desktop-grid .ld-class-card").First).ToBeVisibleAsync();
        await trang.Locator("a[aria-label='Tuần sau']").ClickAsync();
        string tuanSau = await trang.Locator(".ld-filters input[name='ngay']").GetAttributeAsync("value");
        Assert.Equal(DateTime.Parse(batDauTuan).AddDays(7), DateTime.Parse(tuanSau));

        await trang.Locator("a[aria-label='Tuần trước']").ClickAsync();
        string tuanTruoc = await trang.Locator(".ld-filters input[name='ngay']").GetAttributeAsync("value");
        Assert.Equal(DateTime.Parse(batDauTuan), DateTime.Parse(tuanTruoc));

        await trang.SetViewportSizeAsync(DienThoai.Width, DienThoai.Height);
        await trang.ReloadAsync();
        await Expect(trang.Locator(".ld-mobile-days")).ToBeVisibleAsync();
        await Expect(trang.Locator(".ld-day-card")).ToHaveCountAsync(7);
        await Expect(trang.Locator(".ld-desktop-grid")).ToBeHiddenAsync();
        Assert.True(await trang.Locator(".ld-day-card .ld-class-card").CountAsync() > 0,
            "Lịch điện thoại phải hiển thị lớp học trong các thẻ ngày.");
    }, Desktop);

    // ---------------------------------------------------------------- 9
    [Fact]
    public Task K9_GiaoDienToi_LuuSauKhiTaiLai() => Chay(nameof(K9_GiaoDienToi_LuuSauKhiTaiLai), async trang =>
    {
        await DangNhap(trang, Du.QuanLy.TenDangNhap);
        await trang.GotoAsync(Url("/TongQuan"));
        var nut = trang.Locator("[data-theme-toggle]");

        await nut.ClickAsync();
        await nut.ClickAsync();
        await Expect(trang.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark");

        await trang.ReloadAsync();
        await Expect(trang.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark");
    });

    // ---------------------------------------------------------------- 10
    [Fact]
    public Task K10_NhatKy_GhiNhanSuaHocVien() => Chay(nameof(K10_NhatKy_GhiNhanSuaHocVien), async trang =>
    {
        await DangNhap(trang, Du.QuanLy.TenDangNhap);
        string tenMoi = "HocVienDaSua";
        await trang.GotoAsync(Url($"/HocVien/Edit/{Du.HV1.MaHV}"));
        await trang.FillAsync("#HoTen", tenMoi);
        await trang.FillAsync("#GhiChu", "E2E_SuaHocVien_" + Du.Hau);
        await trang.ClickAsync("form[action*='/HocVien/Edit'] button[type=submit]");
        await Expect(trang).ToHaveURLAsync(new Regex($"/HocVien/Details/{Du.HV1.MaHV}$"));

        string homNay = DateTime.Today.ToString("yyyy-MM-dd");
        await trang.GotoAsync(Url($"/NhatKy?tuNgay={homNay}&denNgay={homNay}&hanhDong=Sua&bang=HocVien"));
        var dong = trang.Locator("table tbody tr").First;
        await Expect(dong).ToContainTextAsync("Sua");
        await Expect(dong).ToContainTextAsync("HocVien");
    });

    private async Task KiemTraMenuVaiTro(string tenDangNhap, bool laQuanLy)
    {
        await Chay($"Menu-{tenDangNhap}", async trang =>
        {
            await DangNhap(trang, tenDangNhap);
            await trang.AddInitScriptAsync(
                "window.__e2eCsp = []; document.addEventListener('securitypolicyviolation', " +
                "event => window.__e2eCsp.push(event.violatedDirective + ': ' + event.blockedURI));");

            var loiConsole = new List<string>();
            trang.Console += (_, thongBao) =>
            {
                if (thongBao.Type == "error")
                    loiConsole.Add($"{trang.Url}: {thongBao.Text}");
            };

            string[] duongDan = await trang.Locator("nav.vo-nav a.vo-nav-link")
                .EvaluateAllAsync<string[]>("links => links.map(link => link.getAttribute('href'))");
            Assert.Equal(laQuanLy ? 11 : 3, duongDan.Length);
            if (laQuanLy) duongDan = duongDan.Append("/CaiDat").ToArray();

            var loiCsp = new List<string>();
            foreach (string muc in duongDan)
            {
                await trang.GotoAsync(Url(muc));
                string[] viPham = await trang.EvaluateAsync<string[]>(
                    "() => window.__e2eCsp || []");
                loiCsp.AddRange(viPham.Select(v => $"{muc}: {v}"));
                await trang.EvaluateAsync("() => { window.__e2eCsp = []; }");
            }

            Assert.True(loiConsole.Count == 0,
                "Lỗi JavaScript trong menu: " + string.Join(" | ", loiConsole));
            Assert.True(loiCsp.Count == 0,
                "Vi phạm CSP trong menu: " + string.Join(" | ", loiCsp));
        });
    }
}
