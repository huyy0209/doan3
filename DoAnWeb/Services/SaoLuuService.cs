using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using DoAnWeb.ViewModels.SaoLuuVM;

namespace DoAnWeb.Services;

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

    public SaoLuuService(IConfiguration cauHinh, ILogger<SaoLuuService> logger)
    {
        _cauHinh = cauHinh;
        _logger = logger;
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
