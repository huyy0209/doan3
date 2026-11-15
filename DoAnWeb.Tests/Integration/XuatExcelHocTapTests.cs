using System;
using System.Net;
using System.Threading.Tasks;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>
/// Xuất kết quả học tập ra .xlsx: mỗi lớp một sheet + sheet tổng hợp;
/// gia sư chỉ được xuất học viên mình dạy (ngoài phạm vi 403).
/// </summary>
public class XuatExcelHocTapTests : XuatExcelTestBase
{
    private LopHoc TaoLopCoBuoi(HocVien hv, GiaSu gs, string tenMon = "ITG_Toan")
    {
        var lop = TaoLop(gs, hv, monHoc: tenMon);
        Db.BuoiHocs.Add(new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today.AddDays(-1),
            CaHoc = Constants.CaHoc.Sang,
            DiemDanh = Constants.DiemDanh.CoMat,
            Diem = 8.5m,
            NhanXet = "Học tốt"
        });
        Db.BuoiHocs.Add(new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = DateTime.Today,
            CaHoc = Constants.CaHoc.Sang,
            DiemDanh = Constants.DiemDanh.VangCoPhep,
            NhanXet = "Nghỉ ốm"
        });
        Db.SaveChanges();
        return lop;
    }

    [Fact]
    public async Task Quan_ly_xuat_ket_qua_moi_lop_mot_sheet_va_co_sheet_tong_hop()
    {
        var hv = TaoHocVien(TenItg());
        var lop = TaoLopCoBuoi(hv, GS);
        var ql = await DangNhapQL();

        var r = await ql.Client.GetAsync($"/HocTap/XuatExcel/{hv.MaHV}");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(MimeXlsx, r.Content.Headers.ContentType?.MediaType);
        Assert.StartsWith("KetQuaHocTap_", TenFile(r));
        Assert.EndsWith(".xlsx", TenFile(r));

        using var wb = MoLai(await r.Content.ReadAsByteArrayAsync());
        Assert.Equal(2, wb.Worksheets.Count);
        Assert.NotNull(wb.Worksheet("Tổng hợp"));

        var tong = wb.Worksheet("Tổng hợp");
        Assert.Equal("Học viên", tong.Cell(1, 1).GetString());
        Assert.Equal("Điểm TB", tong.Cell(1, 11).GetString());
        Assert.Equal(2, tong.LastRowUsed().RowNumber());
        Assert.Equal(hv.HoTen, tong.Cell(2, 1).GetString());
        Assert.Equal(lop.TenLop, tong.Cell(2, 2).GetString());
        Assert.Equal(2d, tong.Cell(2, 6).GetDouble());
        Assert.Equal(1d, tong.Cell(2, 7).GetDouble());
        Assert.Equal(1d, tong.Cell(2, 8).GetDouble());
        Assert.Equal(8.5d, tong.Cell(2, 11).GetDouble());

        var lopSheet = wb.Worksheet(lop.TenLop);
        Assert.Equal("Buổi", lopSheet.Cell(1, 1).GetString());
        Assert.Equal("Điểm danh", lopSheet.Cell(1, 2).GetString());
        Assert.Equal("Điểm", lopSheet.Cell(1, 3).GetString());
        Assert.Equal("Nhận xét", lopSheet.Cell(1, 4).GetString());
        Assert.Equal(3, lopSheet.LastRowUsed().RowNumber());
        Assert.Contains("Có mặt", lopSheet.Cell(2, 2).GetString());
        Assert.Equal(8.5d, lopSheet.Cell(2, 3).GetDouble());
        Assert.Equal("Học tốt", lopSheet.Cell(2, 4).GetString());
        Assert.Equal("Nghỉ ốm", lopSheet.Cell(3, 4).GetString());
    }

    [Fact]
    public async Task Gia_su_xuat_duoc_ket_qua_hoc_vien_minh_day()
    {
        var hv = TaoHocVien(TenItg());
        TaoLopCoBuoi(hv, GS);
        var gs = await DangNhapGS();

        var r = await gs.Client.GetAsync($"/HocTap/XuatExcel/{hv.MaHV}");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        using var wb = MoLai(await r.Content.ReadAsByteArrayAsync());
        Assert.True(wb.Worksheets.Count >= 2);
    }

    [Fact]
    public async Task Gia_su_xuat_ket_qua_hoc_vien_nguoi_khac_bi_403()
    {
        var hv = TaoHocVien(TenItg());
        TaoLopCoBuoi(hv, GSKhac);
        var gs = await DangNhapGS();

        var r = await gs.Client.GetAsync($"/HocTap/XuatExcel/{hv.MaHV}");

        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Quan_ly_xuat_ket_qua_hoc_vien_chua_co_lop_van_hop_le()
    {
        var hv = TaoHocVien(TenItg());
        var ql = await DangNhapQL();

        var r = await ql.Client.GetAsync($"/HocTap/XuatExcel/{hv.MaHV}");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        using var wb = MoLai(await r.Content.ReadAsByteArrayAsync());
        Assert.Single(wb.Worksheets);
        Assert.NotNull(wb.Worksheet("Tổng hợp"));
        Assert.Equal(1, wb.Worksheet("Tổng hợp").LastRowUsed().RowNumber());
    }
}
