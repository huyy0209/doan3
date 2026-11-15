using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>Xuất danh sách lớp ra .xlsx theo bộ lọc hiện tại của trang Lớp học.</summary>
public class XuatExcelLopHocTests : XuatExcelTestBase
{
    [Fact]
    public async Task Xuat_excel_danh_sach_lop_mo_lai_duoc_va_dung_bo_loc()
    {
        var hv = TaoHocVien(TenItg());
        var lop = TaoLop(GS, hv, monHoc: "ITG_Toan", hocPhi: 1500000m);
        TaoLop(GS, TaoHocVien(TenItg()), monHoc: "ITG_Ly", hocPhi: 900000m);
        var ql = await DangNhapQL();

        var r = await ql.Client.GetAsync("/LopHoc/XuatExcel?q=" + Uri.EscapeDataString(lop.TenLop) +
            "&trangThai=" + Uri.EscapeDataString("Tất cả"));

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(MimeXlsx, r.Content.Headers.ContentType?.MediaType);
        Assert.StartsWith("DanhSachLop_", TenFile(r));
        Assert.EndsWith(".xlsx", TenFile(r));

        using var wb = MoLai(await r.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet("Danh sách lớp");

        Assert.Equal("Lớp", ws.Cell(1, 1).GetString());
        Assert.Equal("Học viên", ws.Cell(1, 2).GetString());
        Assert.Equal("Gia sư", ws.Cell(1, 3).GetString());
        Assert.Equal("Môn", ws.Cell(1, 4).GetString());
        Assert.Equal("Học phí", ws.Cell(1, 12).GetString());
        Assert.True(ws.Cell(1, 1).Style.Font.Bold);
        Assert.Equal(1, ws.SheetView.SplitRow);

        // Chỉ đúng lớp khớp từ khóa: 1 tiêu đề + 1 dòng
        Assert.Equal(2, ws.LastRowUsed().RowNumber());
        Assert.Equal(lop.TenLop, ws.Cell(2, 1).GetString());
        Assert.Equal(hv.HoTen, ws.Cell(2, 2).GetString());
        Assert.Equal("ITG_Toan", ws.Cell(2, 4).GetString());
        Assert.Equal(1500000d, ws.Cell(2, 12).GetDouble());
        Assert.Equal("#,##0", ws.Cell(2, 12).Style.NumberFormat.Format);
    }

    [Fact]
    public async Task Xuat_excel_danh_sach_lop_loc_theo_gia_su()
    {
        var hv = TaoHocVien(TenItg());
        var lop = TaoLop(GS, hv, monHoc: "ITG_Toan");
        TaoLop(GSKhac, TaoHocVien(TenItg()), monHoc: "ITG_Toan");
        var ql = await DangNhapQL();

        var r = await ql.Client.GetAsync("/LopHoc/XuatExcel?maGS=" + GS.MaGS +
            "&trangThai=" + Uri.EscapeDataString("Tất cả"));

        using var wb = MoLai(await r.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet("Danh sách lớp");
        int cuoi = ws.LastRowUsed().RowNumber();
        Assert.True(cuoi >= 2);
        bool thayLop = false;
        for (int i = 2; i <= cuoi; i++)
        {
            Assert.Equal(GS.HoTen, ws.Cell(i, 3).GetString());
            Assert.NotEqual(GSKhac.HoTen, ws.Cell(i, 3).GetString());
            if (ws.Cell(i, 1).GetString() == lop.TenLop) thayLop = true;
        }
        Assert.True(thayLop, "Không thấy lớp của gia sư được lọc trong file.");
    }
}
