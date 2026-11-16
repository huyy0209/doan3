using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using DoAnWeb.Infrastructure;
using DoAnWeb.ViewModels.SaoLuuVM;

namespace DoAnWeb.Services;

public sealed class SaoLuuKhongTuongThichException : InvalidOperationException
{
    public SaoLuuKhongTuongThichException() : base("Bản sao lưu quá cũ, không tương thích; đã quay về dữ liệu trước khi khôi phục") { }
}

public sealed class SaoLuuService
{
    private static readonly Regex TenTepHopLe = new(
        @"^QuanLyGiaSu_\d{8}_\d{6}\.bak$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex TenCsdlHopLe = new(
        @"^[A-Za-z0-9_]+$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly SemaphoreSlim KhoaSaoLuu = new(1, 1);

    private readonly IConfiguration _cauHinh;
    private readonly ILogger<SaoLuuService> _logger;
    private readonly MocKhoiPhucPhien _mocKhoiPhuc;

    public SaoLuuService(IConfiguration cauHinh, ILogger<SaoLuuService> logger, MocKhoiPhucPhien mocKhoiPhuc = null)
    {
        _cauHinh = cauHinh;
        _logger = logger;
        _mocKhoiPhuc = mocKhoiPhuc;
    }

    public async Task<IReadOnlyList<SaoLuuFileVM>> LayDanhSachAsync(CancellationToken cancellationToken = default)
    {
        var thuMuc = await LayThuMucAsync(cancellationToken);
        Directory.CreateDirectory(thuMuc);
        return Directory.EnumerateFiles(thuMuc, "QuanLyGiaSu_*.bak", SearchOption.TopDirectoryOnly)
            .Select(duongDan => new FileInfo(duongDan))
            .Where(tep => TenTepHopLe.IsMatch(tep.Name))
            .OrderByDescending(tep => tep.LastWriteTimeUtc)
            .Select(tep => new SaoLuuFileVM
            {
                Ten = tep.Name,
                Ngay = tep.LastWriteTime,
                KichThuoc = tep.Length
            })
            .ToList();
    }

    public async Task<string> SaoLuuAsync(CancellationToken cancellationToken = default)
    {
        var chuoiKetNoi = _cauHinh.GetConnectionString("QuanLyGiaSu");
        if (string.IsNullOrWhiteSpace(chuoiKetNoi))
            throw new InvalidOperationException("Chưa cấu hình chuỗi kết nối QuanLyGiaSu.");

        var ketNoiBuilder = new SqlConnectionStringBuilder(chuoiKetNoi);
        var tenCsdl = ketNoiBuilder.InitialCatalog;
        if (!TenCsdlHopLe.IsMatch(tenCsdl))
            throw new InvalidOperationException("Tên CSDL trong chuỗi kết nối chỉ được gồm chữ, số và dấu gạch dưới.");

        var giuSoBan = _cauHinh.GetValue<int?>("SaoLuu:GiuSoBan") ?? 14;
        if (giuSoBan < 1)
            throw new InvalidOperationException("SaoLuu:GiuSoBan phải lớn hơn hoặc bằng 1.");

        var thuMuc = await LayThuMucAsync(cancellationToken);
        Directory.CreateDirectory(thuMuc);

        await KhoaSaoLuu.WaitAsync(cancellationToken);
        try
        {
            var thoiGian = DateTime.Now;
            var tenTep = "QuanLyGiaSu_" + thoiGian.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".bak";
            var duongDan = LayDuongDanAnToan(thuMuc, tenTep);
            while (File.Exists(duongDan))
            {
                thoiGian = thoiGian.AddSeconds(1);
                tenTep = "QuanLyGiaSu_" + thoiGian.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".bak";
                duongDan = LayDuongDanAnToan(thuMuc, tenTep);
            }

            await using var ketNoi = new SqlConnection(chuoiKetNoi);
            await ketNoi.OpenAsync(cancellationToken);
            await using (var lenh = ketNoi.CreateCommand())
            {
                lenh.CommandText = $"BACKUP DATABASE [{tenCsdl}] TO DISK = @duongDan WITH COPY_ONLY, INIT, CHECKSUM";
                lenh.CommandTimeout = 0;
                lenh.Parameters.Add("@duongDan", System.Data.SqlDbType.NVarChar, 4000).Value = duongDan;
                await lenh.ExecuteNonQueryAsync(cancellationToken);
            }

            _logger.LogInformation("Đã sao lưu CSDL {TenCsdl} vào {DuongDan}", tenCsdl, duongDan);
            XoaBanCu(thuMuc, giuSoBan, cancellationToken);
            return tenTep;
        }
        finally
        {
            KhoaSaoLuu.Release();
        }
    }

    public static void KiemTraTenTepCongKhai(string tenTep) => KiemTraTenTep(tenTep);

    public async Task<string> KhoiPhucAsync(string tenTep, CancellationToken cancellationToken = default)
    {
        KiemTraTenTep(tenTep);
        var cs = _cauHinh.GetConnectionString("QuanLyGiaSu");
        if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("Chưa cấu hình chuỗi kết nối.");
        var builder = new SqlConnectionStringBuilder(cs);
        string database = builder.InitialCatalog;
        if (!TenCsdlHopLe.IsMatch(database) || string.Equals(database, "QuanLyGiaSu", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("CSDL đích không được phép khôi phục.");
        var dir = await LayThuMucAsync(cancellationToken);
        string source = LayDuongDanAnToan(dir, tenTep);
        if (!File.Exists(source)) throw new FileNotFoundException("Không tìm thấy file sao lưu.", tenTep);
        await KhoaSaoLuu.WaitAsync(cancellationToken);
        try
        {
            string before = await SaoLuuDuoiKhoaAsync(cs, database, dir, cancellationToken);
            var master = new SqlConnectionStringBuilder(cs) { InitialCatalog = "master" };
            await using var conn = new SqlConnection(master.ConnectionString);
            await conn.OpenAsync(cancellationToken);
            bool singleUser = false;
            try
            {
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    await cmd.ExecuteNonQueryAsync(CancellationToken.None);
                    singleUser = true;
                }
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"RESTORE DATABASE [{database}] FROM DISK = @duongDan WITH REPLACE, CHECKSUM";
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add("@duongDan", System.Data.SqlDbType.NVarChar, 4000).Value = source;
                    await cmd.ExecuteNonQueryAsync(CancellationToken.None);
                }

                if (!await CoSchemaWebCanThietAsync(conn, database))
                {
                    await RestoreBackupAsync(conn, database, before);
                    throw new SaoLuuKhongTuongThichException();
                }

                // Ghi mốc bền vững ngoài DB ngay sau restore tương thích; nếu ghi thất bại thì rollback.
                try { _mocKhoiPhuc?.GhiMoc(DateTimeOffset.UtcNow); }
                catch
                {
                    await RestoreBackupAsync(conn, database, before);
                    throw new InvalidOperationException("Không lưu được mốc thu hồi phiên; đã quay về dữ liệu trước khi khôi phục");
                }
            }
            finally
            {
                try
                {
                    if (singleUser)
                    {
                        await using var cmd = conn.CreateCommand();
                        cmd.CommandText = $"ALTER DATABASE [{database}] SET MULTI_USER";
                        await cmd.ExecuteNonQueryAsync(CancellationToken.None);
                    }
                }
                finally { SqlConnection.ClearAllPools(); }
            }
            _logger.LogInformation("Khôi phục CSDL {TenCsdl} từ {Tep}; bản hiện tại trước khôi phục được lưu tại {BanTruoc}", database, source, before);
            return Path.GetFileName(before);
        }
        finally { KhoaSaoLuu.Release(); }
    }

    private static async Task RestoreBackupAsync(SqlConnection conn, string database, string backupPath)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"RESTORE DATABASE [{database}] FROM DISK = @duongDan WITH REPLACE, CHECKSUM";
        cmd.CommandTimeout = 0;
        cmd.Parameters.Add("@duongDan", System.Data.SqlDbType.NVarChar, 4000).Value = backupPath;
        await cmd.ExecuteNonQueryAsync(CancellationToken.None);
    }

    private static async Task<bool> CoSchemaWebCanThietAsync(SqlConnection conn, string database)
    {
        // Từ chối và rollback thay vì tự chạy scripts cũ: các file Database/* dùng USE QuanLyGiaSu,
        // có thể vô tình nâng cấp CSDL WinForms. Các cột/index v7/v8 cũng phải tồn tại đầy đủ.
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
SELECT CASE WHEN
    OBJECT_ID(N'[{database}].dbo.NhatKy', N'U') IS NOT NULL AND
    OBJECT_ID(N'[{database}].dbo.ThongBao', N'U') IS NOT NULL AND
    COL_LENGTH(N'[{database}].dbo.LopHoc', N'MaMon') IS NOT NULL AND
    COL_LENGTH(N'[{database}].dbo.TaiKhoan', N'MaGS') IS NOT NULL AND
    EXISTS (SELECT 1 FROM [{database}].sys.indexes WHERE object_id = OBJECT_ID(N'[{database}].dbo.TaiKhoan') AND name = N'UX_TaiKhoan_MaGS') AND
    NOT EXISTS (
        SELECT 1 FROM (VALUES
            (N'NhatKy', N'MaNK'), (N'NhatKy', N'ThoiGian'), (N'NhatKy', N'MaTK'), (N'NhatKy', N'TenDangNhap'), (N'NhatKy', N'HanhDong'), (N'NhatKy', N'Bang'), (N'NhatKy', N'KhoaChinh'), (N'NhatKy', N'ChiTiet'), (N'NhatKy', N'DiaChiIP'),
            (N'ThongBao', N'MaTB'), (N'ThongBao', N'MaTK'), (N'ThongBao', N'TieuDe'), (N'ThongBao', N'NoiDung'), (N'ThongBao', N'DuongDan'), (N'ThongBao', N'DaDoc'), (N'ThongBao', N'ThoiGian')
        ) required(TableName, ColumnName)
        WHERE COL_LENGTH(N'[{database}].dbo.' + required.TableName, required.ColumnName) IS NULL
    ) THEN 1 ELSE 0 END";
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 1;
    }

    private static async Task<string> SaoLuuDuoiKhoaAsync(string cs, string database, string dir, CancellationToken ct)
    {
        var time = DateTime.Now;
        string name, path;
        do
        {
            name = "QuanLyGiaSu_" + time.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".bak";
            path = LayDuongDanAnToan(dir, name);
            time = time.AddSeconds(1);
        } while (File.Exists(path));
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"BACKUP DATABASE [{database}] TO DISK = @duongDan WITH COPY_ONLY, INIT, CHECKSUM";
        cmd.CommandTimeout = 0;
        cmd.Parameters.Add("@duongDan", System.Data.SqlDbType.NVarChar, 4000).Value = path;
        await cmd.ExecuteNonQueryAsync(ct);
        return path;
    }

    public async Task<FileStream> MoTepAsync(string tenTep, CancellationToken cancellationToken = default)
    {
        KiemTraTenTep(tenTep);
        var thuMuc = await LayThuMucAsync(cancellationToken);
        var duongDan = LayDuongDanAnToan(thuMuc, tenTep);
        return new FileStream(duongDan, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public async Task XoaTepAsync(string tenTep, CancellationToken cancellationToken = default)
    {
        KiemTraTenTep(tenTep);
        var thuMuc = await LayThuMucAsync(cancellationToken);
        var duongDan = LayDuongDanAnToan(thuMuc, tenTep);
        if (!File.Exists(duongDan))
            throw new FileNotFoundException("Không tìm thấy file sao lưu.", tenTep);
        File.Delete(duongDan);
        _logger.LogInformation("Đã xóa file sao lưu cũ {DuongDan}", duongDan);
    }

    internal static string LayDuongDanAnToan(string thuMuc, string tenTep)
    {
        KiemTraTenTep(tenTep);

        var thuMucDayDu = Path.GetFullPath(thuMuc);
        var duongDan = Path.GetFullPath(Path.Combine(thuMucDayDu, tenTep));
        var tuongDoi = Path.GetRelativePath(thuMucDayDu, duongDan);
        if (Path.IsPathRooted(tuongDoi) || tuongDoi.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            string.Equals(tuongDoi, "..", StringComparison.Ordinal))
            throw new ArgumentException("File phải nằm trong thư mục sao lưu.", nameof(tenTep));

        return duongDan;
    }

    private static void KiemTraTenTep(string tenTep)
    {
        if (string.IsNullOrWhiteSpace(tenTep) || !TenTepHopLe.IsMatch(tenTep))
            throw new ArgumentException("Tên file sao lưu không hợp lệ.", nameof(tenTep));
    }

    private async Task<string> LayThuMucAsync(CancellationToken cancellationToken)
    {
        var thuMucCauHinh = _cauHinh["SaoLuu:ThuMuc"];
        if (!string.IsNullOrWhiteSpace(thuMucCauHinh))
            return Path.GetFullPath(thuMucCauHinh);

        var chuoiKetNoi = _cauHinh.GetConnectionString("QuanLyGiaSu");
        if (string.IsNullOrWhiteSpace(chuoiKetNoi))
            throw new InvalidOperationException("Chưa cấu hình chuỗi kết nối QuanLyGiaSu.");

        await using var ketNoi = new SqlConnection(chuoiKetNoi);
        await ketNoi.OpenAsync(cancellationToken);
        await using var lenh = ketNoi.CreateCommand();
        lenh.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000))";
        var giaTri = await lenh.ExecuteScalarAsync(cancellationToken);
        if (giaTri is not string thuMucMacDinh || string.IsNullOrWhiteSpace(thuMucMacDinh))
            throw new InvalidOperationException("SQL Server không trả về InstanceDefaultBackupPath. Hãy cấu hình SaoLuu:ThuMuc.");

        return Path.GetFullPath(thuMucMacDinh.Trim());
    }

    private void XoaBanCu(string thuMuc, int giuSoBan, CancellationToken cancellationToken)
    {
        var tepCu = Directory.EnumerateFiles(thuMuc, "QuanLyGiaSu_*.bak", SearchOption.TopDirectoryOnly)
            .Select(duongDan => new FileInfo(duongDan))
            .Where(tep => TenTepHopLe.IsMatch(tep.Name))
            .OrderByDescending(tep => tep.LastWriteTimeUtc)
            .Skip(giuSoBan)
            .ToList();

        foreach (var tep in tepCu)
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Delete(LayDuongDanAnToan(thuMuc, tep.Name));
            _logger.LogInformation("Đã xóa bản sao lưu quá hạn {DuongDan}", tep.FullName);
        }
    }
}
