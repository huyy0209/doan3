using System;
using System.IO;
using DoAnWeb.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// CSDL dùng để test: bản sao riêng QuanLyGiaSu_Test, tạo một lần bằng BACKUP/RESTORE
/// từ QuanLyGiaSu (không ghi gì vào CSDL thật). Mọi dữ liệu test đặt tiền tố "TEST_" để dọn được.
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

    /// <summary>Chuỗi kết nối tới CSDL test (không phải QuanLyGiaSu thật).</summary>
    public static string ConnStr => VoiDatabase("QuanLyGiaSu_Test");

    private static string MasterConnStr => VoiDatabase("master");

    private static readonly object Khoa = new object();
    private static bool _daTao;

    /// <summary>Tạo context mới trỏ tới CSDL test.</summary>
    public static QuanLyGiaSuContext TaoContext() =>
        new(new DbContextOptionsBuilder<QuanLyGiaSuContext>().UseSqlServer(ConnStr).Options);

    /// <summary>Đảm bảo CSDL QuanLyGiaSu_Test tồn tại (BACKUP/RESTORE nếu chưa có).</summary>
    public static void DamBaoDaTao()
    {
        if (_daTao) return;
        lock (Khoa)
        {
            if (_daTao) return;

            using var conn = new SqlConnection(MasterConnStr);
            conn.Open();

            bool tonTai = false;
            using (var cmd = new SqlCommand(
                       "SELECT 1 FROM sys.databases WHERE name = N'QuanLyGiaSu_Test'", conn))
                tonTai = cmd.ExecuteScalar() != null;

            if (!tonTai)
            {
                // Thư mục DATA mặc định của SQL Server, không hard-code đường dẫn Windows:
                // /var/opt/mssql/data trên Linux (CI) và ...\MSSQL\DATA trên Windows.
                string dataDir = LayThuMucDuLieuMacDinh(conn);

                string bak = Path.Combine(dataDir, "QuanLyGiaSu_W3Test.bak");
                string mdf = Path.Combine(dataDir, "QuanLyGiaSu_Test.mdf");
                string ldf = Path.Combine(dataDir, "QuanLyGiaSu_Test_log.ldf");

                using (var cmd = new SqlCommand("BACKUP DATABASE QuanLyGiaSu TO DISK = @bak WITH COPY_ONLY, INIT", conn))
                {
                    cmd.Parameters.AddWithValue("@bak", bak);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand(
                           "RESTORE DATABASE QuanLyGiaSu_Test FROM DISK = @bak " +
                           "WITH MOVE 'QuanLyGiaSu' TO @mdf, MOVE 'QuanLyGiaSu_log' TO @ldf, REPLACE", conn))
                {
                    cmd.Parameters.AddWithValue("@bak", bak);
                    cmd.Parameters.AddWithValue("@mdf", mdf);
                    cmd.Parameters.AddWithValue("@ldf", ldf);
                    cmd.ExecuteNonQuery();
                }
            }

            _daTao = true;
        }
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
