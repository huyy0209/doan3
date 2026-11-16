using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace DoAnWeb.Tests.Integration;

/// <summary>Đo thời gian các trang nặng trên bộ dữ liệu PERF_ trong CSDL test riêng.</summary>
[Trait("Loai", "HieuNang")]
public sealed class HieuNangWebHttpTests : HttpTestBase
{
    private readonly ITestOutputHelper _output;

    public HieuNangWebHttpTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task Do_thoi_gian_cac_trang_tren_du_lieu_lon()
    {
        string tienTo = "PERF_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        try
        {
            TaoDuLieuLon(tienTo);
            GhiThongKeIo();
            var ql = await DangNhapQL();
            var cacTrang = new Dictionary<string, string>
            {
                ["Tổng quan"] = "/TongQuan",
                ["Biểu đồ"] = "/TongQuan/BieuDo",
                ["Học phí"] = "/HocPhi",
                ["Nhắc nợ"] = "/NhacNo",
                ["Lịch dạy"] = "/LichDay",
                ["Tìm kiếm gợi ý"] = "/TimKiem/GoiY?q=PERF",
                ["Nhật ký"] = "/NhatKy",
                ["Báo cáo"] = $"/BaoCao?thang={DateTime.Today.Month}&nam={DateTime.Today.Year}",
                ["Điểm danh"] = $"/DiemDanh?ngay={DateTime.Today:yyyy-MM-dd}"
            };

            foreach (var trang in cacTrang)
            {
                var thoiGian = new List<double>();
                for (int lan = 0; lan < 4; lan++)
                {
                    var dongHo = Stopwatch.StartNew();
                    var ketQua = await ql.Get(trang.Value);
                    dongHo.Stop();
                    Assert.True((int)ketQua.Status == 200,
                        $"GET {trang.Value} → {(int)ketQua.Status} {ketQua.Location}");
                    if (lan > 0) thoiGian.Add(dongHo.Elapsed.TotalMilliseconds);
                }

                thoiGian.Sort();
                _output.WriteLine($"{trang.Key}: median {thoiGian[1]:F1} ms; " +
                                  $"min {thoiGian[0]:F1} ms; max {thoiGian[2]:F1} ms");
            }
        }
        finally
        {
            DonDuLieuLon(tienTo);
        }
    }

    private void TaoDuLieuLon(string tienTo)
    {
        const string sql = @"
SET NOCOUNT ON;
DECLARE @homNay date = CONVERT(date, GETDATE());

;WITH n AS
(
    SELECT TOP (5000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS i
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT dbo.HocVien (HoTen)
SELECT @tienTo + N'_HV_' + RIGHT(N'00000' + CONVERT(nvarchar(5), i), 5)
FROM n;

;WITH n AS
(
    SELECT TOP (1000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS i
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
), hv AS
(
    SELECT MaHV, ROW_NUMBER() OVER (ORDER BY MaHV) - 1 AS i
    FROM dbo.HocVien
    WHERE HoTen LIKE @tienTo + N'[_]HV[_]%'
)
INSERT dbo.LopHoc
    (MaGS, MaHV, MonHoc, HocPhi, TrangThai, ThoiGianBatDau, ThoiGianKetThuc, CaHoc, TenLop, ThuHoc)
SELECT @maGS, hv.MaHV, @tienTo + N'_Mon', 1000000, N'Đang diễn ra',
       DATEADD(day, -365, @homNay), DATEADD(day, 30, @homNay), N'Sáng',
       @tienTo + N'_LOP_' + RIGHT(N'0000' + CONVERT(nvarchar(4), n.i), 4), N'2,4,6'
FROM n
JOIN hv ON hv.i = (n.i * 5) % 5000;

;WITH n AS
(
    SELECT TOP (1000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS i
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
SELECT l.MaLop, ROW_NUMBER() OVER (ORDER BY l.TenLop) - 1 AS i
INTO #perfLop
FROM dbo.LopHoc l
WHERE l.TenLop LIKE @tienTo + N'[_]LOP[_]%';

;WITH n AS
(
    SELECT TOP (100) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS i
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT dbo.BuoiHoc (MaLop, NgayHoc, DiemDanh, Diem, CaHoc, MaGSDay, MaBuoiGoc, ThiLenCap)
SELECT l.MaLop, DATEADD(day, -n.i, @homNay), N'Có mặt', 8.5, N'Sáng', @maGS, NULL, 0
FROM #perfLop l CROSS JOIN n;

;WITH n AS
(
    SELECT TOP (20) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS i
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT dbo.ThanhToan (MaLop, SoTien, NgayDong, HinhThuc)
SELECT l.MaLop, 2500, DATEADD(day, -n.i, @homNay), N'Tiền mặt'
FROM #perfLop l CROSS JOIN n;

;WITH n AS
(
    SELECT TOP (20000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS i
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT dbo.NhatKy (ThoiGian, TenDangNhap, HanhDong, Bang, KhoaChinh)
SELECT DATEADD(minute, -i, SYSDATETIME()), @tienTo + N'_user', N'Xem', N'LopHoc',
       CONVERT(nvarchar(100), i)
FROM n;";

        var conn = (SqlConnection)Db.Database.GetDbConnection();
        bool mo = conn.State != ConnectionState.Open;
        if (mo) conn.Open();
        try
        {
            using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 300 };
            cmd.Parameters.AddWithValue("@tienTo", tienTo);
            cmd.Parameters.AddWithValue("@maGS", GS.MaGS);
            cmd.ExecuteNonQuery();
        }
        finally
        {
            if (mo) conn.Close();
        }
    }

    private void DonDuLieuLon(string tienTo)
    {
        Db.Database.ExecuteSqlRaw(
            "DELETE FROM dbo.BuoiHoc WHERE MaLop IN (SELECT MaLop FROM dbo.LopHoc WHERE TenLop LIKE {0} + N'[_]LOP[_]%');" +
            "DELETE FROM dbo.ThanhToan WHERE MaLop IN (SELECT MaLop FROM dbo.LopHoc WHERE TenLop LIKE {0} + N'[_]LOP[_]%');" +
            "DELETE FROM dbo.LopHoc WHERE TenLop LIKE {0} + N'[_]LOP[_]%';" +
            "DELETE FROM dbo.HocVien WHERE HoTen LIKE {0} + N'[_]HV[_]%';" +
            "DELETE FROM dbo.NhatKy WHERE TenDangNhap = {0} + N'_user';",
            tienTo);
    }

    private void GhiThongKeIo()
    {
        const string sql = @"
SET STATISTICS IO, TIME ON;
SELECT COUNT_BIG(*) FROM dbo.BuoiHoc
WHERE NgayHoc >= DATEADD(day, -30, CONVERT(date, GETDATE())) AND DiemDanh IS NOT NULL;
SELECT MaLop, SUM(SoTien) FROM dbo.ThanhToan GROUP BY MaLop;
SELECT TOP (20) MaNK, ThoiGian, TenDangNhap, HanhDong, Bang, KhoaChinh, DiaChiIP
FROM dbo.NhatKy
WHERE ThoiGian >= DATEADD(day, -30, CONVERT(date, GETDATE()))
ORDER BY ThoiGian DESC, MaNK DESC;
SET STATISTICS IO, TIME OFF;";
        var conn = (SqlConnection)Db.Database.GetDbConnection();
        bool mo = conn.State != ConnectionState.Open;
        if (mo) conn.Open();
        SqlInfoMessageEventHandler handler = (_, e) => _output.WriteLine(e.Message);
        conn.InfoMessage += handler;
        conn.FireInfoMessageEventOnUserErrors = true;
        try
        {
            using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 300 };
            cmd.ExecuteNonQuery();
        }
        finally
        {
            conn.InfoMessage -= handler;
            if (mo) conn.Close();
        }
    }
}
