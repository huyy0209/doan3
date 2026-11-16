using System;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// G9 – Tìm kiếm nhanh: không dấu, giới hạn gợi ý, từ khóa quá ngắn và phân quyền gia sư.
/// Dữ liệu dùng tiền tố TEST_, DbTestBase tự dọn trước và sau mỗi test.
/// </summary>
public class TimKiemNhanhServiceTests : DbTestBase
{
    private TimKiemNhanhService Tao(INguoiDungHienTai nguoiDung) => new(Db, nguoiDung);

    [Fact]
    [Trait("Luat", "G9")]
    public void Tim_khong_dau_van_ra_co_dau()
    {
        var hv = TaoHocVien("TEST_Đỗ Bảo Ngọc", SdtTest());

        var kq = Tao(QuanLy()).GoiY("do bao ngoc");

        Assert.Contains(kq.HocViens, x => x.Id == hv.MaHV);
        Assert.Contains(kq.HocViens, x => x.Url == "/HocVien/Details/" + hv.MaHV);
    }

    [Fact]
    [Trait("Luat", "G9")]
    public void Tim_theo_so_dien_thoai()
    {
        var hv = TaoHocVien("TEST_Học Viên Số Điện Thoại", "0987654321");

        var kq = Tao(QuanLy()).GoiY("0987 654");

        Assert.Contains(kq.HocViens, x => x.Id == hv.MaHV);
    }

    [Theory]
    [Trait("Luat", "G9")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a")]
    [InlineData("T")]
    [InlineData("Đ")]
    public void Tu_khoa_rong_hoac_mot_ky_tu_tra_rong(string q)
    {
        TaoHocVien("TEST_Một Học Viên Bất Kỳ", SdtTest());
        var svc = Tao(QuanLy());

        var kq = svc.TimDayDu(q);

        Assert.Empty(kq.HocViens);
        Assert.Empty(kq.GiaSus);
        Assert.Empty(kq.Lops);
        Assert.Equal(0, kq.TongHocVien + kq.TongGiaSu + kq.TongLop);
    }

    [Fact]
    [Trait("Luat", "G9")]
    public void Goi_y_toi_da_nam_dong_moi_nhom()
    {
        for (int i = 0; i < 8; i++)
            TaoHocVien("TEST_GoiY Nhiều " + i.ToString("00"), SdtTest());

        var kq = Tao(QuanLy()).GoiY("TEST_GoiY");

        Assert.True(kq.HocViens.Count <= TimKiemNhanhService.SoGoiYMoiNhom,
            $"Gợi ý phải tối đa {TimKiemNhanhService.SoGoiYMoiNhom} dòng, nhưng có {kq.HocViens.Count}.");
        Assert.NotEmpty(kq.HocViens);
    }

    [Fact]
    [Trait("Luat", "G9")]
    public void Tim_lop_theo_mon_va_ten_hoc_vien()
    {
        var gs = TaoGiaSu("TEST_Gia Sư Lớp", SdtTest());
        var hv = TaoHocVien("TEST_Phạm Minh Khôi", SdtTest());
        var lop = TaoLop(gs, hv, "TEST_Toán Nâng Cao");
        var svc = Tao(QuanLy());

        var theoMon = svc.GoiY("toan nang cao");
        var theoHocVien = svc.GoiY("pham minh khoi");

        Assert.Contains(theoMon.Lops, x => x.Id == lop.MaLop);
        Assert.Contains(theoHocVien.Lops, x => x.Id == lop.MaLop);
        Assert.Contains(theoHocVien.Lops, x => x.Url == "/LopHoc/Details/" + lop.MaLop);
    }

    [Fact]
    [Trait("Luat", "G9")]
    public void Gia_su_chi_thay_hoc_vien_va_lop_cua_minh()
    {
        var gsMinh = TaoGiaSu("TEST_Gia Sư Của Mình", SdtTest());
        var gsKhac = TaoGiaSu("TEST_Gia Sư Người Khác", SdtTest());
        var hvCuaMinh = TaoHocVien("TEST_Học Viên Của Mình", SdtTest());
        var hvNguoiKhac = TaoHocVien("TEST_Học Viên Người Khác", SdtTest());
        var lopCuaMinh = TaoLop(gsMinh, hvCuaMinh, "TEST_Toán");
        var lopNguoiKhac = TaoLop(gsKhac, hvNguoiKhac, "TEST_Lý");

        var kq = Tao(GiaSu(11, gsMinh.MaGS)).TimDayDu("TEST_");

        Assert.Contains(kq.HocViens, x => x.Id == hvCuaMinh.MaHV);
        Assert.DoesNotContain(kq.HocViens, x => x.Id == hvNguoiKhac.MaHV);
        Assert.Contains(kq.Lops, x => x.Id == lopCuaMinh.MaLop);
        Assert.DoesNotContain(kq.Lops, x => x.Id == lopNguoiKhac.MaLop);
        Assert.Empty(kq.GiaSus);
        Assert.Equal(0, kq.TongGiaSu);
        Assert.Contains(kq.Lops, x => x.Url == "/HocTap/KetQua/" + hvCuaMinh.MaHV);
    }

    [Fact]
    [Trait("Luat", "G9")]
    public void Quan_ly_thay_du_ba_nhom()
    {
        var gs = TaoGiaSu("TEST_Gia Sư Ba Nhóm", SdtTest());
        var hv = TaoHocVien("TEST_Học Viên Ba Nhóm", SdtTest());
        var lop = TaoLop(gs, hv, "TEST_Hóa");

        var kq = Tao(QuanLy()).TimDayDu("TEST_");

        Assert.Contains(kq.HocViens, x => x.Id == hv.MaHV);
        Assert.Contains(kq.GiaSus, x => x.Id == gs.MaGS);
        Assert.Contains(kq.Lops, x => x.Id == lop.MaLop);
        Assert.Contains(kq.GiaSus, x => x.Url == "/GiaSu/Details/" + gs.MaGS);
        Assert.True(kq.TongHocVien >= 1);
        Assert.True(kq.TongGiaSu >= 1);
        Assert.True(kq.TongLop >= 1);
    }

    [Fact]
    [Trait("Luat", "G9")]
    public void Gia_su_day_bu_cung_thay_hoc_vien_cua_lop_do()
    {
        var gsDayBu = TaoGiaSu("TEST_Gia Sư Dạy Bù", SdtTest());
        var gsChinh = TaoGiaSu("TEST_Gia Sư Chính", SdtTest());
        var hv = TaoHocVien("TEST_Học Viên Học Bù", SdtTest());
        var lop = TaoLop(gsChinh, hv, "TEST_Văn");
        Db.BuoiHocs.Add(new DoAnWeb.Data.Entities.BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today,
            CaHoc = lop.CaHoc,
            MaGSDay = gsDayBu.MaGS,
            DiemDanh = Constants.DiemDanh.CoMat
        });
        Db.SaveChanges();

        var kq = Tao(GiaSu(12, gsDayBu.MaGS)).TimDayDu("TEST_Học Viên Học Bù");

        Assert.Contains(kq.HocViens, x => x.Id == hv.MaHV);
        Assert.Contains(kq.Lops, x => x.Id == lop.MaLop);
    }
}
