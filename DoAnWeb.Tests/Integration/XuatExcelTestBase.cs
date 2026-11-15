using System;
using System.IO;
using System.Net.Http;
using ClosedXML.Excel;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>
/// Cơ sở cho test xuất Excel: mở lại file .xlsx trả về bằng ClosedXML để kiểm nội dung
/// (tiêu đề, số dòng, dòng tổng), thay vì chỉ so chuỗi như CSV.
/// </summary>
[Collection("Database")]
public abstract class XuatExcelTestBase : HttpTestBase
{
    protected const string MimeXlsx =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    protected static XLWorkbook MoLai(byte[] noiDung) => new(new MemoryStream(noiDung));

    /// <summary>Tên file trong Content-Disposition (bỏ dấu nháy bao quanh)</summary>
    protected static string TenFile(HttpResponseMessage r) =>
        (r.Content.Headers.ContentDisposition?.FileName ?? "").Trim('"');
}
