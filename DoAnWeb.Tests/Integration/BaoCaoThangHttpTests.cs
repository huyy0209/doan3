using System;
using System.Linq;
using System.Net;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests.Integration;

public class BaoCaoThangHttpTests : XuatExcelTestBase
{
    [Fact]
    public async System.Threading.Tasks.Task Bao_cao_thang_tong_hop_dung_du_lieu_TEST_va_don_dep()
    {
        var ql = await DangNhapQL();
        var thang = DateTime.Today.Month;
        var nam = DateTime.Today.Year;
        var truoc = new BaoCaoThangService(Db, NguoiDungCoDinh.QuanLy()).Lay(thang, nam);
        var ten = "TEST_G18_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var tenMon = ten + "_Mon";
        var hocVienA = ten + "_HV1";
        var hocVienB = ten + "_HV2";

        try
        {
            var gs = TaoGiaSu(ten);
            var mon = TaoMon(tenMon);
            var hv1 = TaoHocVien(hocVienA);
            var hv2 = TaoHocVien(hocVienB);
            var dauThang = new DateTime(nam, thang, 1);
            var hetThang = dauThang.AddMonths(1).AddDays(-1);
            var lop1 = TaoLop(gs, hv1, tenMon, batDau: dauThang, ketThuc: hetThang, hocPhi: 100000);
            var lop2 = TaoLop(gs, hv2, tenMon, batDau: dauThang, ketThuc: hetThang, hocPhi: 100000);
            lop1.MaMon = mon.MaMon;
            lop2.MaMon = mon.MaMon;

            Db.BuoiHocs.AddRange(
                new BuoiHoc { MaLop = lop1.MaLop, NgayHoc = dauThang, DiemDanh = Constants.DiemDanh.CoMat },
                new BuoiHoc { MaLop = lop1.MaLop, NgayHoc = dauThang.AddDays(1), DiemDanh = Constants.DiemDanh.VangCoPhep },
                new BuoiHoc { MaLop = lop1.MaLop, NgayHoc = dauThang.AddDays(2), DiemDanh = Constants.DiemDanh.DiTre },
                new BuoiHoc { MaLop = lop2.MaLop, NgayHoc = dauThang.AddDays(3), DiemDanh = Constants.DiemDanh.CoMat });
            Db.ThanhToans.Add(new ThanhToan
            {
                MaLop = lop1.MaLop,
                SoTien = 35000,
                NgayDong = dauThang.AddDays(2)
            });
            Db.SaveChanges();

            var sau = new BaoCaoThangService(Db, NguoiDungCoDinh.QuanLy()).Lay(thang, nam);
            var dongGiaSu = Assert.Single(sau.TheoGiaSu, g => g.MaGS == gs.MaGS);
            var dongMon = Assert.Single(sau.TheoMon, m => m.TenMon == tenMon);
            Assert.Equal(2, dongGiaSu.SoLopDangDay);
            Assert.Equal(4, dongGiaSu.SoBuoiDaDiemDanh);
            Assert.Equal(1, dongGiaSu.SoBuoiVang);
            Assert.Equal(75m, dongGiaSu.TyLeChuyenCan);
            Assert.Equal(2, dongMon.SoLop);
            Assert.Equal(2, dongMon.SoHocVien);
            Assert.Equal(35000m, dongMon.HocPhiThu);
            Assert.Equal(truoc.HocPhiThu + 35000m, sau.HocPhiThu);
            Assert.Equal(truoc.HocVienMoi + 2, sau.HocVienMoi);
            Assert.Equal(truoc.NoCuoiThang + 165000m, sau.NoCuoiThang);

            var trang = await ql.Get($"/BaoCao?thang={thang}&nam={nam}");
            Assert.Equal(HttpStatusCode.OK, trang.Status);
            Assert.Contains(ten, trang.Html);

            using var excel = await ql.Client.GetAsync($"/BaoCao/XuatExcel?thang={thang}&nam={nam}");
            Assert.Equal(HttpStatusCode.OK, excel.StatusCode);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                excel.Content.Headers.ContentType?.MediaType);
            using var workbook = MoLai(await excel.Content.ReadAsByteArrayAsync());
            Assert.Equal(3, workbook.Worksheets.Count);
            var excelGiaSu = workbook.Worksheet("Theo gia sư");
            Assert.Contains(excelGiaSu.RowsUsed().Skip(1), row => row.Cell(1).GetString() == ten);
            var excelMon = workbook.Worksheet("Theo môn");
            Assert.Contains(excelMon.RowsUsed().Skip(1), row =>
                row.Cell(1).GetString() == tenMon && row.Cell(4).GetDouble() == 35000d);
        }
        finally
        {
            Db.BuoiHocs.Where(b => b.LopHoc.MonHoc == tenMon).ExecuteDelete();
            Db.ThanhToans.Where(t => t.LopHoc.MonHoc == tenMon).ExecuteDelete();
            Db.LopHocs.Where(l => l.MonHoc == tenMon).ExecuteDelete();
            Db.HocViens.Where(h => h.HoTen == hocVienA || h.HoTen == hocVienB).ExecuteDelete();
            Db.GiaSus.Where(g => g.HoTen == ten).ExecuteDelete();
            Db.MonHocs.Where(m => m.TenMon == tenMon).ExecuteDelete();
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task Gia_su_bi_chan_va_thang_trong_khong_co_du_lieu_tra_so_0()
    {
        var gs = await DangNhapGS();
        var biChan = await gs.Get("/BaoCao");
        Assert.True(biChan.Status == HttpStatusCode.Forbidden || biChan.BiChan,
            $"Gia sư phải bị chặn, nhưng nhận {(int)biChan.Status} {biChan.Location}");

        var ql = await DangNhapQL();
        var trang = await ql.Get("/BaoCao?thang=1&nam=1900");
        Assert.Equal(HttpStatusCode.OK, trang.Status);
        Assert.Contains("Học phí thu trong tháng</span><strong>0 đ</strong>", trang.Html);
        Assert.Contains("Học viên mới</span><strong>0</strong>", trang.Html);
        Assert.Contains("Nợ cuối tháng</span><strong>0 đ</strong>", trang.Html);

        using var excel = await ql.Client.GetAsync("/BaoCao/XuatExcel?thang=1&nam=1900");
        Assert.Equal(HttpStatusCode.OK, excel.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            excel.Content.Headers.ContentType?.MediaType);
    }
}
