using DoAnWeb.Services.Excel;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>Xuất công nợ học phí ra .xlsx (đề xuất #4): mở lại bằng ClosedXML kiểm tiêu đề, dòng, tổng.</summary>
public class XuatExcelCongNoTests : XuatExcelTestBase
{
    [Fact]
    [Trait("Luat", "HP-01")]
    public async Task Xuat_excel_cong_no_co_tieu_de_du_dong_va_dong_tong()
    {
        var hv = TaoHocVien(TenItg(), sdt: "0901234567");
        var lop = TaoLop(GS, hv, hocPhi: 2000000m);
        Db.ThanhToans.Add(new ThanhToan
        {
            MaLop = lop.MaLop, SoTien = 500000, NgayDong = DateTime.Now, HinhThuc = "Tiền mặt"
        });
        Db.SaveChanges();
        var ql = await DangNhapQL();

        var r = await ql.Client.GetAsync("/HocPhi/XuatExcel?tinhTrang=" +
            Uri.EscapeDataString("Tất cả") + "&tuKhoa=" + Uri.EscapeDataString(lop.TenLop));

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(MimeXlsx, r.Content.Headers.ContentType?.MediaType);
        Assert.StartsWith("CongNo_", TenFile(r));
        Assert.EndsWith(".xlsx", TenFile(r));

        using var wb = MoLai(await r.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet("Công nợ");

        Assert.Equal("Lớp", ws.Cell(1, 1).GetString());
        Assert.Equal("Học viên", ws.Cell(1, 2).GetString());
        Assert.Equal("SĐT liên hệ", ws.Cell(1, 3).GetString());
        Assert.Equal("Môn", ws.Cell(1, 4).GetString());
        Assert.Equal("Gia sư", ws.Cell(1, 5).GetString());
        Assert.Equal("Học phí", ws.Cell(1, 6).GetString());
        Assert.Equal("Đã đóng", ws.Cell(1, 7).GetString());
        Assert.Equal("Còn nợ", ws.Cell(1, 8).GetString());
        Assert.Equal("Tình trạng", ws.Cell(1, 9).GetString());
        Assert.True(ws.Cell(1, 1).Style.Font.Bold);
        Assert.Equal(1, ws.SheetView.SplitRow);

        // 1 tiêu đề + 1 dòng dữ liệu + 1 dòng tổng
        Assert.Equal(3, ws.LastRowUsed().RowNumber());
        Assert.Equal(lop.TenLop, ws.Cell(2, 1).GetString());
        Assert.Equal(hv.HoTen, ws.Cell(2, 2).GetString());
        Assert.Equal(2000000d, ws.Cell(2, 6).GetDouble());
        Assert.Equal(500000d, ws.Cell(2, 7).GetDouble());
        Assert.Equal(1500000d, ws.Cell(2, 8).GetDouble());
        Assert.Equal("#,##0", ws.Cell(2, 6).Style.NumberFormat.Format);

        Assert.Equal("Tổng cộng", ws.Cell(3, 1).GetString());
        Assert.Equal("1 lớp", ws.Cell(3, 2).GetString());
        Assert.Equal(2000000d, ws.Cell(3, 6).GetDouble());
        Assert.Equal(500000d, ws.Cell(3, 7).GetDouble());
        Assert.Equal(1500000d, ws.Cell(3, 8).GetDouble());
        Assert.True(ws.Cell(3, 1).Style.Font.Bold);
    }

    [Fact]
    public async Task Xuat_excel_cong_no_giu_bo_loc_tinh_trang_cua_trang()
    {
        var hv = TaoHocVien(TenItg());
        var lop = TaoLop(GS, hv, hocPhi: 300000m);
        Db.ThanhToans.Add(new ThanhToan { MaLop = lop.MaLop, SoTien = 300000, NgayDong = DateTime.Now });
        Db.SaveChanges();
        var ql = await DangNhapQL();

        var r = await ql.Client.GetAsync("/HocPhi/XuatExcel?tinhTrang=" + Uri.EscapeDataString(HocPhiService.ConNo) +
            "&tuKhoa=" + Uri.EscapeDataString(lop.TenLop));

        using var wb = MoLai(await r.Content.ReadAsByteArrayAsync());
        var ws = wb.Worksheet("Công nợ");
        // Chỉ còn dòng tiêu đề và dòng tổng vì lớp đã đóng đủ
        Assert.Equal(2, ws.LastRowUsed().RowNumber());
        Assert.Equal("Tổng cộng", ws.Cell(2, 1).GetString());
        Assert.Equal(0d, ws.Cell(2, 8).GetDouble());
    }

    [Fact]
    public void O_bat_dau_bang_cong_thuc_duoc_ghi_dang_chuoi()
    {
        var svc = new XuatExcelService();
        var bytes = svc.CongNo(new[]
        {
            new CongNoLop
            {
                TenLop = "=HYPERLINK(\"http://x\")", HocVien = "+SUM(1,2)", SDTLienHe = "@cmd",
                MonHoc = "-1+1", GiaSu = "Gia sư", HocPhi = 100000, DaDong = 0, ConNo = 100000,
                TinhTrang = HocPhiService.ChuaDong
            }
        });

        using var wb = MoLai(bytes);
        var ws = wb.Worksheet("Công nợ");
        // Dấu nháy đơn chỉ là tiền tố an toàn khi ghi; ClosedXML lưu ô ở dạng chữ, không phải công thức.
        Assert.Equal("=HYPERLINK(\"http://x\")", ws.Cell(2, 1).GetString());
        Assert.Equal("+SUM(1,2)", ws.Cell(2, 2).GetString());
        Assert.Equal("@cmd", ws.Cell(2, 3).GetString());
        Assert.Equal("-1+1", ws.Cell(2, 4).GetString());
        Assert.False(ws.Cell(2, 1).HasFormula);
        Assert.False(ws.Cell(2, 2).HasFormula);
        Assert.False(ws.Cell(2, 3).HasFormula);
        Assert.False(ws.Cell(2, 4).HasFormula);
        Assert.Equal(XLDataType.Text, ws.Cell(2, 1).DataType);
    }
}
