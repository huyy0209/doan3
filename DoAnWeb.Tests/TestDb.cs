using System;
using System.IO;
using DoAnWeb.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// CSDL dùng để test: bản sao RIÊNG CHO TỪNG TIẾN TRÌNH test (QuanLyGiaSu_Web_Test_p{pid}), tạo bằng RESTORE
/// từ bản BACKUP của QuanLyGiaSu_Web (không ghi gì vào CSDL web thật hay CSDL của app) và tự xóa khi tiến trình kết thúc.
/// Lý do: trước đây mọi tiến trình dùng chung MỘT CSDL QuanLyGiaSu_Test, nên hai lần chạy cùng lúc
/// (hai worker, test + E2E, IDE + CLI…) xóa dữ liệu của nhau theo tiền tố, test lúc đỏ lúc xanh (F1).
/// Mọi dữ liệu test vẫn đặt tiền tố "TEST_"/"ITG_"/"E2E_" để dọn trong CÙNG tiến trình.
/// </summary>
public static class TestDb
{
    /// <summary>
    /// Phần chung của chuỗi kết nối (chưa có Database), đọc từ biến môi trường DOANWEB_TEST_SQL.
    /// Máy dev không đặt biến này thì giữ nguyên "Server=." + Windows Authentication như cũ;
    /// CI (Linux) đặt biến trỏ tới container SQL Server, ví dụ:
    ///   Server=localhost,1433;User Id=sa;Password=...;TrustServerCertificate=True
    /// Database được thêm riêng cho từng kết nối (QuanLyGiaSu_Test / master).
    /// </summary>
    private static readonly string ChuoiKetNoiGoc = LayChuoiKetNoiGoc();

    private static string LayChuoiKetNoiGoc()
    {
        string tuMoiTruong = Environment.GetEnvironmentVariable("DOANWEB_TEST_SQL");
        return string.IsNullOrWhiteSpace(tuMoiTruong)
            ? "Server=.;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
            : tuMoiTruong;
    }

    private static string VoiDatabase(string tenCsdl)
    {
        var builder = new SqlConnectionStringBuilder(ChuoiKetNoiGoc)
        {
            InitialCatalog = tenCsdl,
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        };
        return builder.ConnectionString;
    }

    /// <summary>Tiền tố tên mọi CSDL test; "QuanLyGiaSu" (app) và "QuanLyGiaSu_Web" (web thật) KHÔNG bao giờ khớp (xem <see cref="LaCsdlTest"/>).</summary>
    public const string TienToCsdlTest = "QuanLyGiaSu_Web_Test";

    /// <summary>CSDL web thật dùng làm nguồn sao chép (bản app WinForms có CSDL riêng "QuanLyGiaSu", test web không đụng tới).</summary>
    public const string NguonCsdl = "QuanLyGiaSu_Web";

    /// <summary>Tên CSDL test riêng của tiến trình này, ví dụ QuanLyGiaSu_Web_Test_p1234.</summary>
    public static readonly string TenCsdl = $"{TienToCsdlTest}_p{Environment.ProcessId}";

    /// <summary>Chốt an toàn: chỉ chấp nhận CSDL bắt đầu bằng QuanLyGiaSu_Web_Test (không bao giờ là QuanLyGiaSu của app hay QuanLyGiaSu_Web thật).</summary>
    public static bool LaCsdlTest(string ten) =>
        ten != null && ten.StartsWith(TienToCsdlTest, StringComparison.OrdinalIgnoreCase);

    /// <summary>Chuỗi kết nối tới CSDL test riêng của tiến trình (không phải CSDL web thật).</summary>
    public static string ConnStr => VoiDatabase(TenCsdl);

    private static string MasterConnStr => VoiDatabase("master");

    private static readonly object Khoa = new object();
    private static bool _daTao;

    /// <summary>Tạo context mới trỏ tới CSDL test.</summary>
    public static QuanLyGiaSuContext TaoContext() =>
        new(new DbContextOptionsBuilder<QuanLyGiaSuContext>().UseSqlServer(ConnStr).Options);

    /// <summary>
    /// Đảm bảo CSDL test riêng của tiến trình tồn tại: RESTORE từ file backup chung (tạo bằng BACKUP COPY_ONLY
    /// từ QuanLyGiaSu_Web nếu chưa có), dọn các CSDL test của tiến trình đã chết, và hẹn xóa CSDL khi thoát.
    /// </summary>
    public static void DamBaoDaTao()
    {
        if (_daTao) return;
        lock (Khoa)
        {
            if (_daTao) return;

            using var conn = new SqlConnection(MasterConnStr);
            conn.Open();

            DonCsdlMoCoi(conn);

            // Khóa ứng dụng theo tên: nhiều tiến trình khởi động cùng lúc không ghi đè file backup của nhau.
            using (var khoa = new SqlCommand(
                       "DECLARE @r int; EXEC @r = sp_getapplock @Resource = N'DoAnWeb_TestDb_Tao', @LockMode = N'Exclusive', " +
                       "@LockOwner = N'Session', @LockTimeout = 120000; SELECT @r;", conn))
            {
                if ((int)khoa.ExecuteScalar() < 0)
                    throw new InvalidOperationException("Không lấy được khóa tạo CSDL test (quá 120 giây).");
            }

            try
            {
                string dataDir = LayThuMucDuLieuMacDinh(conn);
                string bak = Path.Combine(dataDir, "QuanLyGiaSu_Web_Test.bak");
                string mdf = Path.Combine(dataDir, TenCsdl + ".mdf");
                string ldf = Path.Combine(dataDir, TenCsdl + "_log.ldf");

                if (!TonTaiBackup(conn, bak))
                {
                    using var cmd = new SqlCommand($"BACKUP DATABASE [{NguonCsdl}] TO DISK = @bak WITH COPY_ONLY, INIT", conn);
                    cmd.Parameters.AddWithValue("@bak", bak);
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();
                }

                // Tên logic của file dữ liệu/log lấy từ chính file backup (CSDL tạo bằng script có thể đặt tên khác nhau).
                string logicDuLieu = null, logicLog = null;
                using (var cmd = new SqlCommand("RESTORE FILELISTONLY FROM DISK = @bak", conn))
                {
                    cmd.Parameters.AddWithValue("@bak", bak);
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        string loai = rd["Type"].ToString();
                        if (loai == "D" && logicDuLieu == null) logicDuLieu = rd["LogicalName"].ToString();
                        else if (loai == "L" && logicLog == null) logicLog = rd["LogicalName"].ToString();
                    }
                }

                using (var cmd = new SqlCommand(
                           $"RESTORE DATABASE [{TenCsdl}] FROM DISK = @bak " +
                           $"WITH MOVE N'{logicDuLieu.Replace("'", "''")}' TO @mdf, MOVE N'{logicLog.Replace("'", "''")}' TO @ldf, REPLACE", conn))
                {
                    cmd.Parameters.AddWithValue("@bak", bak);
                    cmd.Parameters.AddWithValue("@mdf", mdf);
                    cmd.Parameters.AddWithValue("@ldf", ldf);
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand(
                           $"USE [{TenCsdl}]; " +
                           "IF OBJECT_ID(N'dbo.ThongBao', N'U') IS NULL " +
                           "BEGIN " +
                           "CREATE TABLE dbo.ThongBao (MaTB int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ThongBao PRIMARY KEY, " +
                           "MaTK int NOT NULL, TieuDe nvarchar(200) NOT NULL, NoiDung nvarchar(1000) NOT NULL, " +
                           "DuongDan nvarchar(300) NULL, DaDoc bit NOT NULL CONSTRAINT DF_ThongBao_DaDoc DEFAULT (0), " +
                           "ThoiGian datetime2 NOT NULL CONSTRAINT DF_ThongBao_ThoiGian DEFAULT (SYSDATETIME()), " +
                           "CONSTRAINT FK_ThongBao_TaiKhoan FOREIGN KEY (MaTK) REFERENCES dbo.TaiKhoan(MaTK)); " +
                           "CREATE INDEX IX_ThongBao_MaTK_DaDoc_ThoiGian ON dbo.ThongBao(MaTK, DaDoc, ThoiGian DESC); " +
                           "END; USE [master];", conn))
                    cmd.ExecuteNonQuery();
            }
            finally
            {
                using var mo = new SqlCommand(
                    "EXEC sp_releaseapplock @Resource = N'DoAnWeb_TestDb_Tao', @LockOwner = N'Session';", conn);
                mo.ExecuteNonQuery();
            }

            AppDomain.CurrentDomain.ProcessExit += (_, _) => XoaCsdl(TenCsdl);
            _daTao = true;
        }
    }

    /// <summary>File backup còn dùng được không (RESTORE VERIFYONLY: file có tồn tại và đọc được).</summary>
    private static bool TonTaiBackup(SqlConnection conn, string bak)
    {
        try
        {
            using var cmd = new SqlCommand("RESTORE VERIFYONLY FROM DISK = @bak", conn);
            cmd.Parameters.AddWithValue("@bak", bak);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (SqlException) { return false; }
    }

    /// <summary>Xóa CSDL test của các tiến trình đã không còn chạy (do bị kill, không kịp dọn).</summary>
    private static void DonCsdlMoCoi(SqlConnection conn)
    {
        var ten = new System.Collections.Generic.List<string>();
        using (var cmd = new SqlCommand(
                   $"SELECT name FROM sys.databases WHERE name LIKE N'{TienToCsdlTest}[_]p%'", conn))
        using (var rd = cmd.ExecuteReader())
            while (rd.Read()) ten.Add(rd.GetString(0));

        foreach (var t in ten)
        {
            if (!int.TryParse(t.Substring(TienToCsdlTest.Length + 2), out int pid) || pid == Environment.ProcessId) continue;
            bool conSong;
            try { conSong = !System.Diagnostics.Process.GetProcessById(pid).HasExited; }
            catch (ArgumentException) { conSong = false; }
            if (!conSong) XoaCsdl(t);
        }
    }

    /// <summary>Xóa một CSDL test (ngắt mọi kết nối còn lại); lỗi thì bỏ qua vì chỉ là dọn dẹp.</summary>
    private static void XoaCsdl(string ten)
    {
        if (!ten.StartsWith(TienToCsdlTest + "_p", StringComparison.OrdinalIgnoreCase)) return; // không bao giờ xóa DB khác
        try
        {
            SqlConnection.ClearAllPools();
            using var conn = new SqlConnection(MasterConnStr);
            conn.Open();
            using var cmd = new SqlCommand(
                $"IF DB_ID(N'{ten}') IS NOT NULL BEGIN ALTER DATABASE [{ten}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{ten}]; END", conn);
            cmd.ExecuteNonQuery();
        }
        catch { /* dọn dẹp thất bại không làm hỏng kết quả test */ }
    }

    /// <summary>
    /// Thư mục DATA mặc định của SQL Server (nơi service SQL có quyền ghi), lấy từ server
    /// nên chạy được cả trên Windows lẫn Linux: /var/opt/mssql/data hoặc ...\MSSQL\DATA.
    /// </summary>
    private static string LayThuMucDuLieuMacDinh(SqlConnection conn)
    {
        using (var cmd = new SqlCommand(
                   "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000))", conn))
        {
            string macDinh = cmd.ExecuteScalar() as string;
            if (!string.IsNullOrWhiteSpace(macDinh)) return macDinh.TrimEnd('\\', '/');
        }

        // SQL Server cũ không trả về InstanceDefaultDataPath: lấy thư mục chứa file .mdf của CSDL gốc.
        using (var cmd = new SqlCommand(
                   "SELECT physical_name FROM sys.master_files WHERE database_id = DB_ID('QuanLyGiaSu_Web') AND type = 0", conn))
            return Path.GetDirectoryName((string)cmd.ExecuteScalar());
    }

    /// <summary>Xóa mọi dữ liệu test (tiền tố TEST_) khỏi CSDL test, theo đúng thứ tự khóa ngoại.</summary>
    public static void DonDep(QuanLyGiaSuContext db)
    {
        db.Database.ExecuteSqlRaw(
            "DELETE FROM ThongBao WHERE MaTK IN (SELECT MaTK FROM TaiKhoan WHERE TenDangNhap LIKE 'TEST_%');" +
            "DELETE FROM BuoiHoc WHERE MaLop IN (SELECT MaLop FROM LopHoc WHERE MonHoc LIKE 'TEST_%' OR TenLop LIKE 'TEST_%');" +
            "DELETE FROM ThanhToan WHERE MaLop IN (SELECT MaLop FROM LopHoc WHERE MonHoc LIKE 'TEST_%' OR TenLop LIKE 'TEST_%');" +
            "DELETE FROM LopHoc WHERE MonHoc LIKE 'TEST_%' OR TenLop LIKE 'TEST_%';" +
            "DELETE FROM TaiKhoan WHERE TenDangNhap LIKE 'TEST_%' OR MaGS IN (SELECT MaGS FROM GiaSu WHERE HoTen LIKE 'TEST_%');" +
            "DELETE FROM GiaSu WHERE HoTen LIKE 'TEST_%';" +
            "DELETE FROM HocVien WHERE HoTen LIKE 'TEST_%';" +
            "DELETE FROM MonHoc WHERE TenMon LIKE 'TEST_%';");
    }
}

/// <summary>Collection fixture: đảm bảo CSDL test được tạo một lần cho cả nhóm test dùng DB.</summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}

/// <summary>Fixture dùng chung cho collection "Database".</summary>
public class DatabaseFixture : IDisposable
{
    public DatabaseFixture() => TestDb.DamBaoDaTao();
    public void Dispose() { }
}
