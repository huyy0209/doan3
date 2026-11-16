using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace DoAnWeb.Services;

public sealed class NhatKySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly INguoiDungHienTai _nguoiDung;
    private readonly IHttpContextAccessor _http;
    private readonly List<MucNhatKy> _choGhi = new();
    private IDbContextTransaction _giaoDich;
    private bool _tuMoGiaoDich;

    public NhatKySaveChangesInterceptor(INguoiDungHienTai nguoiDung, IHttpContextAccessor http)
    {
        _nguoiDung = nguoiDung;
        _http = http;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        BatDauGhi(eventData.Context);
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await BatDauGhiAsync(eventData.Context, cancellationToken);
        return result;
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        try
        {
            GhiCacMuc(eventData.Context);
            if (_tuMoGiaoDich) _giaoDich.Commit();
            return result;
        }
        catch
        {
            if (_tuMoGiaoDich) _giaoDich.Rollback();
            throw;
        }
        finally
        {
            KetThucGiaoDich();
            _choGhi.Clear();
        }
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        try
        {
            await GhiCacMucAsync(eventData.Context, cancellationToken);
            if (_tuMoGiaoDich) await _giaoDich.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            if (_tuMoGiaoDich) await _giaoDich.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            KetThucGiaoDich();
            _choGhi.Clear();
        }
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        HuyGiaoDich();
        _choGhi.Clear();
    }

    public override async Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        await HuyGiaoDichAsync();
        _choGhi.Clear();
    }

    private void BatDauGhi(DbContext db)
    {
        if (db == null) return;
        ChupCacThayDoi(db);
        if (_choGhi.Count == 0 || db.Database.CurrentTransaction != null) return;
        _giaoDich = db.Database.BeginTransaction();
        _tuMoGiaoDich = true;
    }

    private async Task BatDauGhiAsync(DbContext db, CancellationToken cancellationToken)
    {
        if (db == null) return;
        ChupCacThayDoi(db);
        if (_choGhi.Count == 0 || db.Database.CurrentTransaction != null) return;
        _giaoDich = await db.Database.BeginTransactionAsync(cancellationToken);
        _tuMoGiaoDich = true;
    }

    private void ChupCacThayDoi(DbContext db)
    {
        _choGhi.Clear();
        foreach (var entry in db.ChangeTracker.Entries()
                     .Where(e => e.Entity is not NhatKy &&
                         e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var bang = entry.Metadata.GetTableName();
            if (string.IsNullOrWhiteSpace(bang)) continue;

            var thayDoi = new Dictionary<string, object>();
            foreach (var p in entry.Properties)
            {
                if (LaThongTinMat(p.Metadata.Name)) continue;
                if (entry.State == EntityState.Modified && !p.IsModified) continue;

                object cu = entry.State == EntityState.Added ? null : p.OriginalValue;
                object moi = entry.State == EntityState.Deleted ? null : p.CurrentValue;
                if (NhatKyBaoMat.LaTruongNhayCam(p.Metadata.Name))
                {
                    cu = NhatKyBaoMat.Che(cu?.ToString());
                    moi = NhatKyBaoMat.Che(moi?.ToString());
                }
                thayDoi[p.Metadata.Name] = new { cu, moi };
            }

            var khoa = entry.Metadata.FindPrimaryKey()?.Properties ?? Array.Empty<Microsoft.EntityFrameworkCore.Metadata.IProperty>();
            _choGhi.Add(new MucNhatKy
            {
                Entry = entry,
                Bang = bang,
                Khoa = khoa,
                HanhDong = entry.State switch
                {
                    EntityState.Added => "Them",
                    EntityState.Modified => "Sua",
                    _ => "Xoa"
                },
                ChiTiet = JsonSerializer.Serialize(thayDoi),
                MaTK = _nguoiDung.DaDangNhap && _nguoiDung.MaTK > 0 ? _nguoiDung.MaTK : (int?)null,
                TenDangNhap = GioiHan(_nguoiDung.TenDangNhap, 50),
                DiaChiIP = GioiHan(_http.HttpContext?.Connection.RemoteIpAddress?.ToString(), 45)
            });
        }
    }

    private static bool LaThongTinMat(string ten) =>
        ten.Contains("MatKhau", StringComparison.OrdinalIgnoreCase) ||
        ten.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
        ten.Contains("Hash", StringComparison.OrdinalIgnoreCase);

    private static string GioiHan(string giaTri, int doDai) =>
        string.IsNullOrEmpty(giaTri) ? null : giaTri.Substring(0, Math.Min(giaTri.Length, doDai));

    private void GhiCacMuc(DbContext db)
    {
        foreach (var muc in _choGhi)
            db.Database.ExecuteSqlRaw(
                "INSERT INTO dbo.NhatKy (MaTK, TenDangNhap, HanhDong, Bang, KhoaChinh, ChiTiet, DiaChiIP) " +
                "VALUES (@MaTK, @TenDangNhap, @HanhDong, @Bang, @KhoaChinh, @ChiTiet, @DiaChiIP)",
                ThamSo(muc));
    }

    private async Task GhiCacMucAsync(DbContext db, CancellationToken cancellationToken)
    {
        foreach (var muc in _choGhi)
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO dbo.NhatKy (MaTK, TenDangNhap, HanhDong, Bang, KhoaChinh, ChiTiet, DiaChiIP) " +
                "VALUES (@MaTK, @TenDangNhap, @HanhDong, @Bang, @KhoaChinh, @ChiTiet, @DiaChiIP)",
                ThamSo(muc), cancellationToken);
    }

    private static object[] ThamSo(MucNhatKy muc)
    {
        var khoaChinh = string.Join(", ", muc.Khoa.Select(p =>
            $"{p.Name}={muc.Entry.Property(p.Name).CurrentValue ?? muc.Entry.Property(p.Name).OriginalValue}"));
        return new object[]
        {
            new SqlParameter("@MaTK", SqlDbType.Int) { Value = muc.MaTK.HasValue ? muc.MaTK.Value : DBNull.Value },
            new SqlParameter("@TenDangNhap", SqlDbType.NVarChar, 50) { Value = (object)muc.TenDangNhap ?? DBNull.Value },
            new SqlParameter("@HanhDong", SqlDbType.NVarChar, 20) { Value = muc.HanhDong },
            new SqlParameter("@Bang", SqlDbType.NVarChar, 50) { Value = muc.Bang },
            new SqlParameter("@KhoaChinh", SqlDbType.NVarChar, 100) { Value = (object)khoaChinh ?? DBNull.Value },
            new SqlParameter("@ChiTiet", SqlDbType.NVarChar, -1) { Value = muc.ChiTiet },
            new SqlParameter("@DiaChiIP", SqlDbType.NVarChar, 45) { Value = (object)muc.DiaChiIP ?? DBNull.Value }
        };
    }

    private void HuyGiaoDich()
    {
        try
        {
            if (_tuMoGiaoDich) _giaoDich?.Rollback();
        }
        finally
        {
            KetThucGiaoDich();
        }
    }

    private async Task HuyGiaoDichAsync()
    {
        try
        {
            if (_tuMoGiaoDich && _giaoDich != null)
                await _giaoDich.RollbackAsync(CancellationToken.None);
        }
        finally
        {
            KetThucGiaoDich();
        }
    }

    private void KetThucGiaoDich()
    {
        if (_tuMoGiaoDich) _giaoDich?.Dispose();
        _giaoDich = null;
        _tuMoGiaoDich = false;
    }

    private sealed class MucNhatKy
    {
        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; set; }
        public IReadOnlyList<Microsoft.EntityFrameworkCore.Metadata.IProperty> Khoa { get; set; }
        public string HanhDong { get; set; }
        public string Bang { get; set; }
        public string ChiTiet { get; set; }
        public int? MaTK { get; set; }
        public string TenDangNhap { get; set; }
        public string DiaChiIP { get; set; }
    }
}
