using System;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm thử logic của entity LopHoc: DemSoBuoi (LH-10), TrungLich (LH-11),
/// TrangThaiTheoThoiGian (LH-17). Không cần CSDL.
/// </summary>
public class LopHocLogicTests
{
    // ============ LH-10: số buổi = số ngày trong khoảng rơi vào thứ học ============

    private static LopHoc Lop(DateTime? bd, DateTime? kt, string thu, string ca = null) =>
        new LopHoc { ThoiGianBatDau = bd, ThoiGianKetThuc = kt, ThuHoc = thu, CaHoc = ca };

    [Fact]
    [Trait("Luat", "LH-10")]
    public void DemSoBuoi_ThieuNgay_Bang0()
    {
        Assert.Equal(0, Lop(null, null, "2,4").DemSoBuoi());
        Assert.Equal(0, Lop(new DateTime(2026, 11, 9), null, "2,4").DemSoBuoi());
    }

    [Fact]
    [Trait("Luat", "LH-10")]
    public void DemSoBuoi_KetThucTruocBatDau_Bang0()
    {
        Assert.Equal(0, Lop(new DateTime(2026, 11, 15), new DateTime(2026, 11, 9), "2,4").DemSoBuoi());
    }

    [Fact]
    [Trait("Luat", "LH-10")]
    public void DemSoBuoi_HangNgay_DemHetCacNgay()
    {
        // 09/11 (T2) → 15/11 (CN) = 7 ngày, học hằng ngày.
        Assert.Equal(7, Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 15), null).DemSoBuoi());
        Assert.Equal(7, Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 15), "").DemSoBuoi());
    }

    [Fact]
    [Trait("Luat", "LH-10")]
    public void DemSoBuoi_TheoThu_DemDungSoBuoi()
    {
        // 09/11 (T2) → 13/11 (T6), học T2-T4-T6 = 3 buổi.
        Assert.Equal(3, Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 13), "2,4,6").DemSoBuoi());
    }

    [Fact]
    [Trait("Luat", "LH-10")]
    public void DemSoBuoi_NhieuTuan()
    {
        // 09/11 → 22/11 (14 ngày), học T2-T4-T6 = 2 tuần × 3 = 6 buổi.
        Assert.Equal(6, Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 22), "2,4,6").DemSoBuoi());
    }

    // ============ LH-11: trùng lịch ============

    [Fact]
    [Trait("Luat", "LH-11")]
    public void TrungLich_KhacCa_KhongTrung()
    {
        var a = Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 15), "2,4,6", Constants.CaHoc.Sang);
        var b = Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 15), "2,4,6", Constants.CaHoc.Toi);
        Assert.False(a.TrungLich(b));
    }

    [Fact]
    [Trait("Luat", "LH-11")]
    public void TrungLich_CungCa_ThieuNgay_Trung()
    {
        // MED-01: cùng ca nhưng thiếu ngày → coi là trùng.
        var a = Lop(null, new DateTime(2026, 11, 15), "2,4", Constants.CaHoc.Toi);
        var b = Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 15), "2,4", Constants.CaHoc.Toi);
        Assert.True(a.TrungLich(b));
    }

    [Fact]
    [Trait("Luat", "LH-11")]
    public void TrungLich_CungCa_KhongGiaoThoiGian_KhongTrung()
    {
        var a = Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 15), "2,4,6", Constants.CaHoc.Toi);
        var b = Lop(new DateTime(2026, 12, 1), new DateTime(2026, 12, 15), "2,4,6", Constants.CaHoc.Toi);
        Assert.False(a.TrungLich(b));
    }

    [Fact]
    [Trait("Luat", "LH-11")]
    public void TrungLich_CungCa_GiaoNgayNhungKhacThu_KhongTrung()
    {
        // Cùng ca Tối, giao khoảng ngày nhưng thứ học khác nhau (không có ngày chung).
        var a = Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 15), "2,4", Constants.CaHoc.Toi);
        var b = Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 15), "3,5", Constants.CaHoc.Toi);
        Assert.False(a.TrungLich(b));
    }

    [Fact]
    [Trait("Luat", "LH-11")]
    public void TrungLich_CungCa_GiaoNgayCoNgayChung_Trung()
    {
        // Cùng ca Tối, giao khoảng và có ngày thứ Tư (4) chung → trùng.
        var a = Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 15), "2,4", Constants.CaHoc.Toi);
        var b = Lop(new DateTime(2026, 11, 9), new DateTime(2026, 11, 15), "4,6", Constants.CaHoc.Toi);
        Assert.True(a.TrungLich(b));
    }

    // ============ LH-17: trạng thái theo thời gian ============

    [Fact]
    [Trait("Luat", "LH-17")]
    public void TrangThaiTheoThoiGian_DaHuy_GiuNguyen()
    {
        var lop = Lop(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-5), null);
        lop.TrangThai = Constants.LopHocStatus.DaHuy;
        Assert.Equal(Constants.LopHocStatus.DaHuy, lop.TrangThaiTheoThoiGian);
    }

    [Fact]
    [Trait("Luat", "LH-17")]
    public void TrangThaiTheoThoiGian_TruocBatDau_ChuaBatDau()
    {
        var lop = Lop(DateTime.Now.AddDays(5), DateTime.Now.AddDays(30), null);
        lop.TrangThai = Constants.LopHocStatus.DangDienRa;
        Assert.Equal(Constants.LopHocStatus.ChuaBatDau, lop.TrangThaiTheoThoiGian);
    }

    [Fact]
    [Trait("Luat", "LH-17")]
    public void TrangThaiTheoThoiGian_TrongKhoang_DangDienRa()
    {
        var lop = Lop(DateTime.Now.AddDays(-5), DateTime.Now.AddDays(30), null);
        lop.TrangThai = Constants.LopHocStatus.ChuaBatDau;
        Assert.Equal(Constants.LopHocStatus.DangDienRa, lop.TrangThaiTheoThoiGian);
    }

    [Fact]
    [Trait("Luat", "LH-17")]
    public void TrangThaiTheoThoiGian_SauKetThuc_DaKetThuc()
    {
        var lop = Lop(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(-5), null);
        lop.TrangThai = Constants.LopHocStatus.DangDienRa;
        Assert.Equal(Constants.LopHocStatus.DaKetThuc, lop.TrangThaiTheoThoiGian);
    }

    [Fact]
    [Trait("Luat", "LH-17")]
    public void TrangThaiTheoThoiGian_ThieuNgay_GiuTrangThaiThuCong()
    {
        var lop = Lop(null, DateTime.Now.AddDays(5), null);
        lop.TrangThai = Constants.LopHocStatus.DangHoc;
        Assert.Equal(Constants.LopHocStatus.DangHoc, lop.TrangThaiTheoThoiGian);
    }
}
