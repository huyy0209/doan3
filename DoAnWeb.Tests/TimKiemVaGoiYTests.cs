using DoAnWeb.Common;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm thử tìm kiếm không dấu (GS-11) và gợi ý tên lớp (LH-07) – không cần CSDL.
/// </summary>
public class TimKiemVaGoiYTests
{
    // ============ GS-11: tìm không dấu, nhiều từ AND, "đ"→"d" ============

    [Fact]
    [Trait("Luat", "GS-11")]
    public void ChuanHoa_BoDauVaDoiD()
    {
        Assert.Equal("do bao ngoc", TimKiem.ChuanHoa("Đỗ Bảo Ngọc"));
        Assert.Equal("gia su", TimKiem.ChuanHoa("Gia sư"));
    }

    [Fact]
    [Trait("Luat", "GS-11")]
    public void Khop_KhongDau_VaKhongPhanBietHoaThuong()
    {
        Assert.True(TimKiem.Khop("ngoc", "Đỗ Bảo Ngọc"));
        Assert.True(TimKiem.Khop("DO BAO", "Đỗ Bảo Ngọc"));
    }

    [Fact]
    [Trait("Luat", "GS-11")]
    public void Khop_NhieuTu_PhaiCoDu()
    {
        // Mọi từ khóa phải xuất hiện (AND).
        Assert.True(TimKiem.Khop("ngoc 0981", "Đỗ Bảo Ngọc", "0981111111"));
        Assert.False(TimKiem.Khop("ngoc 0999", "Đỗ Bảo Ngọc", "0981111111"));
    }

    [Fact]
    [Trait("Luat", "GS-11")]
    public void Khop_TrenNhieuTruong()
    {
        // Tìm trên HoTen/SDT/ChuyenMon/TrangThai.
        Assert.True(TimKiem.Khop("toan", "Đỗ Bảo Ngọc", "0981111111", "Toán, Lý", "Rảnh"));
        Assert.True(TimKiem.Khop("ranh", "Đỗ Bảo Ngọc", "0981111111", "Toán", "Rảnh"));
        Assert.False(TimKiem.Khop("hoa", "Đỗ Bảo Ngọc", "0981111111", "Toán, Lý", "Rảnh"));
    }

    [Fact]
    [Trait("Luat", "GS-11")]
    public void Khop_TuKhoaRong_LuonDung()
    {
        Assert.True(TimKiem.Khop("", "Bất kỳ"));
        Assert.True(TimKiem.Khop(null, "Bất kỳ"));
        Assert.True(TimKiem.Khop("   ", "Bất kỳ"));
    }

    // ============ LH-07: gợi ý tên lớp ============

    [Fact]
    [Trait("Luat", "LH-07")]
    public void GoiYTenLop_MonHocVienCa()
    {
        Assert.Equal("Toán - Nguyễn Văn A - Tối",
            LopHocService.GoiYTenLop("Toán", "Nguyễn Văn A", "Tối (18h-20h)"));
    }

    [Fact]
    [Trait("Luat", "LH-07")]
    public void GoiYTenLop_BoPhanRong()
    {
        // Ca rỗng → không nối phần ca.
        Assert.Equal("Toán - Nguyễn Văn A", LopHocService.GoiYTenLop("Toán", "Nguyễn Văn A", null));
    }

    [Fact]
    [Trait("Luat", "LH-07")]
    public void GoiYTenLop_Cat140KyTu()
    {
        string dai = new string('x', 200);
        string ten = LopHocService.GoiYTenLop(dai, "Học viên", "Tối (18h-20h)");
        Assert.Equal(140, ten.Length);
    }

    [Fact]
    [Trait("Luat", "LH-07")]
    public void GoiYTenLop_TrungThemHauTo()
    {
        // GhepLopLogic.GoiYTenLop thêm " (2)", " (3)" khi tên đã có.
        var daCo = new System.Collections.Generic.HashSet<string> { "Toán - A - Tối" };
        Assert.Equal("Toán - A - Tối (2)", LopHocGhepLopLogic.GoiYTenLop("Toán", "A", "Tối (18h-20h)", daCo));

        daCo.Add("Toán - A - Tối (2)");
        Assert.Equal("Toán - A - Tối (3)", LopHocGhepLopLogic.GoiYTenLop("Toán", "A", "Tối (18h-20h)", daCo));
    }

    [Fact]
    [Trait("Luat", "LH-07")]
    public void GoiYTenLop_KhongTrung_KhongThemHauTo()
    {
        var daCo = new System.Collections.Generic.HashSet<string> { "Lý - B - Sáng" };
        Assert.Equal("Toán - A - Tối", LopHocGhepLopLogic.GoiYTenLop("Toán", "A", "Tối (18h-20h)", daCo));
    }
}
