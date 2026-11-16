using System.Net;
using DoAnWeb.Data.Entities;
using Xunit;

namespace DoAnWeb.Tests.Integration;

public class HocPhiPhieuThuHttpTests : HttpTestBase
{
    private (LopHoc lop, ThanhToan thanhToan) TaoPhieuThu()
    {
        var hocVien = TaoHocVien(TenItg());
        var lop = TaoLop(GS, hocVien, hocPhi: 500000);
        var thanhToan = new ThanhToan
        {
            MaLop = lop.MaLop,
            SoTien = 150000,
            NgayDong = DateTime.Today,
            HinhThuc = "Tiền mặt"
        };
        Db.ThanhToans.Add(thanhToan);
        Db.SaveChanges();
        return (lop, thanhToan);
    }

    [Fact]
    public async Task Quan_ly_xem_duoc_phieu_thu_va_bang_ke()
    {
        var (lop, thanhToan) = TaoPhieuThu();
        var ql = await DangNhapQL();

        var phieu = await ql.Get($"/HocPhi/PhieuThu/{thanhToan.MaTT}");
        Assert.Equal(HttpStatusCode.OK, phieu.Status);
        Assert.Contains("PHIẾU THU HỌC PHÍ", phieu.Html);
        Assert.Contains("một trăm năm mươi nghìn đồng", WebUtility.HtmlDecode(phieu.Html));
        Assert.Contains("Còn nợ", phieu.Html);

        var bangKe = await ql.Get($"/HocPhi/BangKe?maLop={lop.MaLop}");
        Assert.Equal(HttpStatusCode.OK, bangKe.Status);
        Assert.Contains("BẢNG KÊ CÁC LẦN ĐÓNG HỌC PHÍ", bangKe.Html);
        Assert.Contains("150.000 đ", bangKe.Html);
    }

    [Fact]
    public async Task Gia_su_khong_xem_duoc_phieu_thu()
    {
        var (_, thanhToan) = TaoPhieuThu();
        var gs = await DangNhapGS();

        var phieu = await gs.Get($"/HocPhi/PhieuThu/{thanhToan.MaTT}");
        Assert.True(phieu.Status == HttpStatusCode.Forbidden || phieu.RedirectToi("/TaiKhoan/DangNhap") || phieu.BiChan,
            $"Gia sư bị chặn nhưng nhận {(int)phieu.Status} {phieu.Location}");
    }

    [Fact]
    public async Task Phieu_thu_khong_ton_tai_tra_404()
    {
        var ql = await DangNhapQL();
        var phieu = await ql.Get("/HocPhi/PhieuThu/2147483647");
        Assert.Equal(HttpStatusCode.NotFound, phieu.Status);
    }
}
