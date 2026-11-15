using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>GET các trang đọc với dữ liệu thật (có lớp, buổi học, học phí): không được 500 – bắt lỗi view / truy vấn.</summary>
public class DocTrangHttpTests : HttpTestBase
{
    private async Task<int> TaoDuLieu()
    {
        var hv = TaoHocVien(TenItg());
        var lop = TaoLop(GS, hv);
        Db.BuoiHocs.Add(new DoAnWeb.Data.Entities.BuoiHoc { MaLop = lop.MaLop, NgayHoc = DateTime.Today.AddDays(-1), DiemDanh = "Có mặt", Diem = 8, CaHoc = lop.CaHoc });
        Db.ThanhToans.Add(new DoAnWeb.Data.Entities.ThanhToan { MaLop = lop.MaLop, SoTien = 50000, NgayDong = DateTime.Now, HinhThuc = "Tiền mặt" });
        Db.SaveChanges();
        await Task.CompletedTask;
        return lop.MaLop;
    }

    [Fact]
    public async Task Quan_ly_doc_cac_trang_khong_loi()
    {
        int maLop = await TaoDuLieu();
        var hv = Db.LopHocs.Find(maLop).MaHV;
        var ql = await DangNhapQL();
        string thang = $"nam={DateTime.Today.Year}&thang={DateTime.Today.Month}";
        foreach (var url in new[]
                 {
                     "/TongQuan", "/GiaSu", $"/GiaSu/Details/{GS.MaGS}", $"/GiaSu/Edit/{GS.MaGS}", "/HocVien", $"/HocVien/Details/{hv}",
                     $"/HocVien/Edit/{hv}", "/LopHoc", "/LopHoc?trangThai=T%E1%BA%A5t%20c%E1%BA%A3", $"/LopHoc/Details/{maLop}",
                     $"/LopHoc/Edit/{maLop}", "/LopHoc/GhepLop", "/MonHoc", "/HocPhi", $"/HocPhi?maLop={maLop}", "/CaiDat",
                     "/QuanLyTaiKhoan", $"/QuanLyTaiKhoan/DatLaiMatKhau/{QL.MaTK}", "/DiemDanh", "/HocTap", $"/HocTap/KetQua/{hv}",
                     $"/HocTap/PhieuBao/{hv}", $"/Lich?{thang}", $"/Lich?{thang}&maGS={GS.MaGS}", "/ThongBao/SapBatDau", "/TaiKhoan/DoiMatKhau"
                 })
        {
            var t = await ql.Get(url);
            Assert.True(t.Status == HttpStatusCode.OK, $"GET {url} → {(int)t.Status} {t.Location}\n{(t.Html.Length > 600 ? t.Html.Substring(0, 600) : t.Html)}");
        }
    }

    [Fact]
    public async Task Gia_su_doc_cac_trang_cua_minh_khong_loi()
    {
        int maLop = await TaoDuLieu();
        var hv = Db.LopHocs.Find(maLop).MaHV;
        var gs = await DangNhapGS();
        string thang = $"nam={DateTime.Today.Year}&thang={DateTime.Today.Month}";
        foreach (var url in new[]
                 {
                     "/HocVien", $"/HocVien/Details/{hv}", "/DiemDanh", "/HocTap", $"/HocTap/KetQua/{hv}", $"/HocTap/PhieuBao/{hv}",
                     $"/Lich?{thang}", $"/Lich?{thang}&maGS={GSKhac.MaGS}", "/ThongBao/SapBatDau", "/TaiKhoan/DoiMatKhau"
                 })
        {
            var t = await gs.Get(url);
            Assert.True(t.Status == HttpStatusCode.OK, $"GET {url} → {(int)t.Status} {t.Location}\n{(t.Html.Length > 600 ? t.Html.Substring(0, 600) : t.Html)}");
        }
    }

    [Fact]
    [Trait("Luat", "TK-19")]
    public async Task Gia_su_xem_lich_gia_su_khac_van_chi_thay_lich_cua_minh()
    {
        var hvToi = TaoHocVien(TenItg());
        var hvKhac = TaoHocVien(TenItg());
        var lopToi = TaoLop(GS, hvToi);
        var lopKhac = TaoLop(GSKhac, hvKhac);
        var gs = await DangNhapGS();

        var t = await gs.Get($"/Lich?nam={DateTime.Today.Year}&thang={DateTime.Today.Month}&maGS={GSKhac.MaGS}");

        Assert.Equal(HttpStatusCode.OK, t.Status);
        Assert.DoesNotContain(lopKhac.TenLop, t.Chu);
        Assert.DoesNotContain(hvKhac.HoTen, t.Chu);
    }
}
