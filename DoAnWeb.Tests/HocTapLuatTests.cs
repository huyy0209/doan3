using System;
using DoAnWeb.Common;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

public class HocTapLuatTests
{
    [Fact]
    [Trait("Luat", "HT-02")]
    public void DiemDanh_NgayTuongLai_BaoLoi()
    {
        var loi = HocTapService.KiemTraBuoi(DateTime.Today.AddDays(1), Constants.DiemDanh.CoMat, null, null, null, null);

        Assert.Equal("Không thể điểm danh cho ngày trong tương lai.", loi);
    }

    [Fact]
    [Trait("Luat", "HT-02")]
    public void DiemDanh_NgoaiKhoangThoiGianLop_BaoLoi()
    {
        var ngay = DateTime.Today;
        Assert.Contains("Lớp bắt đầu từ", HocTapService.KiemTraBuoi(
            ngay, Constants.DiemDanh.CoMat, null, null, ngay.AddDays(1), ngay.AddDays(3)));
        Assert.Contains("Lớp đã kết thúc ngày", HocTapService.KiemTraBuoi(
            ngay, Constants.DiemDanh.CoMat, null, null, ngay.AddDays(-3), ngay.AddDays(-1)));
    }

    [Fact]
    [Trait("Luat", "HT-03")]
    public void DiemDanh_DiemNgoaiKhoang_BaoLoi()
    {
        var loi = HocTapService.KiemTraBuoi(DateTime.Today, Constants.DiemDanh.CoMat, 10.01m, null, null, null);

        Assert.Equal("Điểm phải từ 0 đến 10.", loi);
    }

    [Fact]
    [Trait("Luat", "HT-03")]
    public void DiemDanh_VangKhongDuocCoDiem_BaoLoi()
    {
        var loi = HocTapService.KiemTraBuoi(DateTime.Today, Constants.DiemDanh.VangCoPhep, 8m, null, null, null);

        Assert.Equal("Học viên vắng thì không có điểm test.", loi);
    }

    [Fact]
    [Trait("Luat", "HT-03")]
    public void DiemDanh_NhanXetQuaDai_BaoLoi()
    {
        var loi = HocTapService.KiemTraBuoi(
            DateTime.Today, Constants.DiemDanh.CoMat, null, new string('x', 301), null, null);

        Assert.Equal("Nhận xét tối đa 300 ký tự.", loi);
    }
}
