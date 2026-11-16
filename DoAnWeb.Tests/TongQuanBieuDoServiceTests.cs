using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm tra số liệu biểu đồ trang Tổng quan ở tầng service: trả đủ 12 tháng liên tiếp,
/// gom đúng doanh thu / học viên mới / số lớp theo môn và tỉ lệ chuyên cần.
/// Dữ liệu tạo với tiền tố TEST_ và được <see cref="DbTestBase"/> tự dọn.
/// </summary>
public class TongQuanBieuDoServiceTests : DbTestBase
{
    [Fact]
    [Trait("Luat", "TQ-BD")]
    public void Lay_tra_du_12_thang_va_gom_dung_du_lieu_thang_nay()
    {
        var truoc = new ThongKeBieuDoService(Db).Lay();

        var khuVuc = TenTest();
        var mon = TaoMon(khuVuc);
        var gs = TaoGiaSu(khuVuc, SdtTest());
        var hv1 = TaoHocVien(khuVuc + "a", SdtTest());
        var hv2 = TaoHocVien(khuVuc + "b", SdtTest());
        var dauThang = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var lop1 = TaoLop(gs, hv1, monHoc: khuVuc, maMon: mon.MaMon, batDau: dauThang.AddHours(8));
        var lop2 = TaoLop(gs, hv2, monHoc: khuVuc, maMon: mon.MaMon, batDau: dauThang.AddHours(9));
        Db.ThanhToans.Add(new ThanhToan { MaLop = lop1.MaLop, SoTien = 300000, NgayDong = dauThang.AddDays(1), HinhThuc = "Tiền mặt" });
        Db.ThanhToans.Add(new ThanhToan { MaLop = lop2.MaLop, SoTien = 200000, NgayDong = dauThang.AddDays(2), HinhThuc = "Tiền mặt" });
        Db.BuoiHocs.Add(new BuoiHoc { MaLop = lop1.MaLop, NgayHoc = dauThang.AddDays(1), CaHoc = lop1.CaHoc, DiemDanh = Constants.DiemDanh.CoMat });
        Db.BuoiHocs.Add(new BuoiHoc { MaLop = lop1.MaLop, NgayHoc = dauThang.AddDays(2), CaHoc = lop1.CaHoc, DiemDanh = Constants.DiemDanh.DiTre });
        Db.BuoiHocs.Add(new BuoiHoc { MaLop = lop1.MaLop, NgayHoc = dauThang.AddDays(3), CaHoc = lop1.CaHoc, DiemDanh = Constants.DiemDanh.VangKhongPhep });
        // Đi trễ vẫn tính là có mặt; vắng không phép chỉ tính vào tổng buổi
        Db.BuoiHocs.Add(new BuoiHoc { MaLop = lop2.MaLop, NgayHoc = dauThang.AddDays(4), CaHoc = lop2.CaHoc, DiemDanh = Constants.DiemDanh.CoMat });
        Db.SaveChanges();

        var sau = new ThongKeBieuDoService(Db).Lay();

        Assert.Equal(ThongKeBieuDoService.SoThang, sau.DoanhThu.Count);
        Assert.Equal(ThongKeBieuDoService.SoThang, sau.HocVienMoi.Count);
        Assert.Equal(ThongKeBieuDoService.SoThang,
            sau.DoanhThu.Select(x => new { x.Nam, x.Thang }).Distinct().Count());
        Assert.Equal($"Tháng {DateTime.Today.Month}/{DateTime.Today.Year}", sau.DoanhThu.Last().Nhan);

        Assert.Equal(truoc.DoanhThu.Last().SoTien + 500000m, sau.DoanhThu.Last().SoTien);
        Assert.Equal(truoc.HocVienMoi.Last().SoLuong + 2, sau.HocVienMoi.Last().SoLuong);

        var monTruoc = truoc.LopTheoMon.FirstOrDefault(x => x.TenMon == khuVuc)?.SoLop ?? 0;
        var monSau = sau.LopTheoMon.FirstOrDefault(x => x.TenMon == khuVuc)?.SoLop ?? 0;
        Assert.Equal(monTruoc + 2, monSau);

        // Có mặt gồm cả đi trễ; tổng buổi chỉ tính buổi đã điểm danh
        Assert.Equal(truoc.ChuyenCan.CoMat + 3, sau.ChuyenCan.CoMat);
        Assert.Equal(truoc.ChuyenCan.TongBuoi + 4, sau.ChuyenCan.TongBuoi);
    }
}
