using DoAnWeb.Services.Excel;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using DoAnWeb.Data.Entities;

namespace DoAnWeb.Services.Excel;

public sealed class NhatKyExcelService
{
    public byte[] Tao(IEnumerable<DoAnWeb.Data.Entities.NhatKy> danhSach)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Nhật ký");
        var tieuDe = new[] { "Thời gian", "Tài khoản", "Hành động", "Bảng", "Khóa chính", "Chi tiết", "Địa chỉ IP" };
        for (int i = 0; i < tieuDe.Length; i++)
        {
            ws.Cell(1, i + 1).Value = tieuDe[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }
        int dong = 2;
        foreach (var muc in danhSach)
        {
            ws.Cell(dong, 1).Value = muc.ThoiGian;
            ws.Cell(dong, 2).Value = Chuoi(muc.TenDangNhap);
            ws.Cell(dong, 3).Value = Chuoi(muc.HanhDong);
            ws.Cell(dong, 4).Value = Chuoi(muc.Bang);
            ws.Cell(dong, 5).Value = Chuoi(muc.KhoaChinh);
            ws.Cell(dong, 6).Value = Chuoi(muc.ChiTiet);
            ws.Cell(dong, 7).Value = Chuoi(muc.DiaChiIP);
            dong++;
        }
        ws.SheetView.FreezeRows(1);
        ws.Columns(1, tieuDe.Length).AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static string Chuoi(string value)
    {
        value ??= "";
        return value.Length > 0 && value[0] is '=' or '+' or '-' or '@' or '\t' or '\r'
            ? "'" + value
            : value;
    }
}
