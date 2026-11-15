using System;
using System.IO;
using DoAnWeb.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// CSDL dùng để test: bản sao RIÊNG CHO TỪNG TIẾN TRÌNH test (QuanLyGiaSu_Test_p{pid}), tạo bằng RESTORE
/// từ bản BACKUP của QuanLyGiaSu (không ghi gì vào CSDL thật) và tự xóa khi tiến trình kết thúc.
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

    /// <summary>Tiền tố tên mọi CSDL test; "QuanLyGiaSu" thật KHÔNG bao giờ khớp (xem <see cref="LaCsdlTest"/>).</summary>
    public const string TienToCsdlTest = "QuanLyGiaSu_Test";

    /// <summary>Tên CSDL test riêng của tiến trình này, ví dụ QuanLyGiaSu_Test_p1234.</summary>
    public static readonly string TenCsdl = $"{TienToCsdlTest}_p{Environment.ProcessId}";

    /// <summary>Chốt an toàn: chỉ chấp nhận CSDL bắt đầu bằng QuanLyGiaSu_Test (không bao giờ là QuanLyGiaSu thật).</summary>
    public static bool LaCsdlTest(string ten) =>
        ten != null && ten.StartsWith(TienToCsdlTest, StringComparison.OrdinalIgnoreCase);

    /// <summary>Chuỗi kết nối tới CSDL test riêng của tiến trình (không phải QuanLyGiaSu thật).</summary>
    public static string ConnStr => VoiDatabase(TenCsdl);

    private static string MasterConnStr => VoiDatabase("master");

    private static readonly object Khoa = new object();
    private static bool _daTao;

    /// <summary>Tạo context mới trỏ tới CSDL test.</summary>
    public static QuanLyGiaSuContext TaoContext() =>
        new(new DbContextOptionsBuilder<QuanLyGiaSuContext>().UseSqlServer(ConnStr).Options);

    /// <summary>
    /// Đảm bảo CSDL test riêng của tiến trình tồn tại: RESTORE từ file backup chung (tạo bằng BACKUP COPY_ONLY
    /// từ QuanLyGiaSu nếu chưa có), dọn các CSDL test của tiến trình đã chết, và hẹn xóa CSDL khi thoát.
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
                string bak = Path.Combine(dataDir, "QuanLyGiaSu_W3Test.bak");
                string mdf = Path.Combine(dataDir, TenCsdl + ".mdf");
                string ldf = Path.Combine(dataDir, TenCsdl + "_log.ldf");

                if (!TonTaiBackup(conn, bak))
                {
                    using var cmd = new SqlCommand("BACKUP DATABASE QuanLyGiaSu TO DISK = @bak WITH COPY_ONLY, INIT", conn);
                    cmd.Parameters.AddWithValue("@bak", bak);
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand(
                           $"RESTORE DATABASE [{TenCsdl}] FROM DISK = @bak " +
                           "WITH MOVE 'QuanLyGiaSu' TO @mdf, MOVE 'QuanLyGiaSu_log' TO @ldf, REPLACE", conn))
                {
                    cmd.Parameters.AddWithValue("@bak", bak);
                    cmd.Parameters.AddWithValue("@mdf", mdf);
                    cmd.Parameters.AddWithValue("@ldf", ldf);
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();
                }
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
                   "SELECT physical_name FROM sys.master_files WHERE database_id = DB_ID('QuanLyGiaSu') AND type = 0", conn))
            return Path.GetDirectoryName((string)cmd.ExecuteScalar());
    }

    /// <summary>Xóa mọi dữ liệu test (tiền tố TEST_) khỏi CSDL test, theo đúng thứ tự khóa ngoại.</summary>
    public static void DonDep(QuanLyGiaSuContext db)
    {
        db.Database.ExecuteSqlRaw(
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
