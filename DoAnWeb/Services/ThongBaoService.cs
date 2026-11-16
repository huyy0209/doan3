using System.Data;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DoAnWeb.Services;

public sealed class ThongBaoService
{
    private const string TieuDeNhacNo = "Học viên còn nợ học phí quá 30 ngày";
    private static readonly TimeSpan ThoiGianCache = TimeSpan.FromSeconds(20);

    private readonly QuanLyGiaSuContext _db;
    private readonly IMemoryCache _cache;

    public ThongBaoService(QuanLyGiaSuContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ThongBaoMenuViewModel> LayMenuAsync(int maTK, CancellationToken cancellationToken = default)
    {
        string khoa = KhoaCache(maTK);
        if (_cache.TryGetValue(khoa, out ThongBaoMenuViewModel daCache))
            return daCache;

        var thongBao = await _db.ThongBaos.AsNoTracking()
            .Where(x => x.MaTK == maTK)
            .OrderByDescending(x => x.ThoiGian)
            .ThenByDescending(x => x.MaTB)
            .Take(10)
            .ToListAsync(cancellationToken);
        int chuaDoc = await _db.ThongBaos.CountAsync(x => x.MaTK == maTK && !x.DaDoc, cancellationToken);
        var ketQua = new ThongBaoMenuViewModel { ChuaDoc = chuaDoc, ThongBao = thongBao };
        _cache.Set(khoa, ketQua, ThoiGianCache);
        return ketQua;
    }

    public async Task<KetQuaTrang<ThongBao>> LayDanhSachAsync(
        int maTK, int trang, int soDong, CancellationToken cancellationToken = default)
    {
        if (soDong is not (20 or 50 or 100))
            soDong = PhanTrang.SoDongMacDinh;

        var truyVan = _db.ThongBaos.AsNoTracking().Where(x => x.MaTK == maTK);
        int tongSo = await truyVan.CountAsync(cancellationToken);
        int soTrang = Math.Max(1, (int)Math.Ceiling(tongSo / (double)soDong));
        trang = Math.Clamp(trang, 1, soTrang);
        var items = await truyVan.OrderByDescending(x => x.ThoiGian).ThenByDescending(x => x.MaTB)
            .Skip((trang - 1) * soDong).Take(soDong)
            .ToListAsync(cancellationToken);

        return new KetQuaTrang<ThongBao>
        {
            Items = items,
            Trang = trang,
            SoTrang = soTrang,
            SoDong = soDong,
            TongSo = tongSo,
            TuDong = tongSo == 0 ? 0 : (trang - 1) * soDong + 1,
            DenDong = Math.Min(trang * soDong, tongSo),
            ThamSo = new Dictionary<string, string>
            {
                ["trang"] = trang.ToString(),
                ["soDong"] = soDong.ToString()
            }
        };
    }

    public void TaoChoGiaSu(int? maGS, string tieuDe, string noiDung, string duongDan)
    {
        if (!maGS.HasValue)
            return;

        int? maTK = _db.TaiKhoans.AsNoTracking()
            .Where(x => x.MaGS == maGS.Value && x.HoatDong)
            .Select(x => (int?)x.MaTK)
            .FirstOrDefault();
        if (!maTK.HasValue)
            return;

        _db.ThongBaos.Add(new ThongBao
        {
            MaTK = maTK.Value,
            TieuDe = tieuDe,
            NoiDung = noiDung,
            DuongDan = duongDan
        });
    }

    public void LamMoiMenuGiaSu(int? maGS)
    {
        if (!maGS.HasValue)
            return;
        int? maTK = _db.TaiKhoans.AsNoTracking()
            .Where(x => x.MaGS == maGS.Value)
            .Select(x => (int?)x.MaTK)
            .FirstOrDefault();
        if (maTK.HasValue)
            XoaCache(maTK.Value);
    }

    public async Task<string> DanhDauDaDocAsync(int maTK, int maTB, CancellationToken cancellationToken = default)
    {
        var thongBao = await _db.ThongBaos
            .FirstOrDefaultAsync(x => x.MaTB == maTB && x.MaTK == maTK, cancellationToken);
        if (thongBao == null)
            return null;

        if (!thongBao.DaDoc)
        {
            thongBao.DaDoc = true;
            await _db.SaveChangesAsync(cancellationToken);
            XoaCache(maTK);
        }
        return thongBao.DuongDan;
    }

    public async Task DanhDauTatCaDaDocAsync(int maTK, CancellationToken cancellationToken = default)
    {
        await _db.ThongBaos.Where(x => x.MaTK == maTK && !x.DaDoc)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.DaDoc, true), cancellationToken);
        XoaCache(maTK);
    }

    public async Task TaoNhacNoQuaHanAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var lopNo = await _db.LopHocs.AsNoTracking()
            .Include(x => x.ThanhToans)
            .Include(x => x.HocVien)
            .Where(x => x.TrangThai != Constants.LopHocStatus.DaHuy)
            .ToListAsync(cancellationToken);
        DateTime mocQuaHan = DateTime.Today.AddDays(-30);
        var quaHan = lopNo
            .Where(x => x.TrangThaiTheoThoiGian != Constants.LopHocStatus.DaHuy &&
                        (x.HocPhi ?? 0) > x.ThanhToans.Sum(t => t.SoTien))
            .Select(x => new
            {
                HocVien = x.HocVien?.HoTen ?? "Học viên chưa rõ",
                MocNgay = x.ThanhToans.Count > 0
                    ? x.ThanhToans.Max(t => t.NgayDong).Date
                    : x.ThoiGianBatDau?.Date
            })
            .Where(x => x.MocNgay.HasValue && x.MocNgay.Value < mocQuaHan)
            .ToList();

        if (quaHan.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var taiKhoanQuanLy = await _db.TaiKhoans.AsNoTracking()
            .Where(x => x.VaiTro == Constants.VaiTro.QuanLy && x.HoatDong)
            .Select(x => x.MaTK)
            .ToListAsync(cancellationToken);
        if (taiKhoanQuanLy.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        DateTime dauNgay = DateTime.Today;
        DateTime ngayMai = dauNgay.AddDays(1);
        string hocVien = string.Join(", ", quaHan.Select(x => x.HocVien).Distinct().Take(8));
        string noiDung = $"{quaHan.Count} học viên còn nợ quá 30 ngày: {hocVien}" +
                         (quaHan.Select(x => x.HocVien).Distinct().Count() > 8 ? ", …" : ".");
        foreach (int maTK in taiKhoanQuanLy)
        {
            bool daTaoHomNay = await _db.ThongBaos.AnyAsync(
                x => x.MaTK == maTK && x.TieuDe == TieuDeNhacNo &&
                     x.ThoiGian >= dauNgay && x.ThoiGian < ngayMai,
                cancellationToken);
            if (daTaoHomNay)
                continue;

            _db.ThongBaos.Add(new ThongBao
            {
                MaTK = maTK,
                TieuDe = TieuDeNhacNo,
                NoiDung = noiDung,
                DuongDan = "/NhacNo?quaNgay=31"
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        foreach (int maTK in taiKhoanQuanLy)
            XoaCache(maTK);
    }

    public async Task TaoThongBaoSaoLuuThatBaiAsync(CancellationToken cancellationToken = default)
    {
        var taiKhoanQuanLy = await _db.TaiKhoans
            .Where(x => x.VaiTro == Constants.VaiTro.QuanLy && x.HoatDong)
            .Select(x => x.MaTK)
            .ToListAsync(cancellationToken);
        foreach (int maTK in taiKhoanQuanLy)
        {
            _db.ThongBaos.Add(new ThongBao
            {
                MaTK = maTK,
                TieuDe = "Sao lưu CSDL tự động thất bại",
                NoiDung = $"Lần sao lưu lúc {DateTime.Now:dd/MM/yyyy HH:mm} không thành công. Hãy kiểm tra log và cấu hình sao lưu.",
                DuongDan = "/SaoLuu"
            });
        }
        await _db.SaveChangesAsync(cancellationToken);
        foreach (int maTK in taiKhoanQuanLy)
            XoaCache(maTK);
    }

    public async Task XoaThongBaoCuAsync(CancellationToken cancellationToken = default)
    {
        DateTime moc = DateTime.Now.AddDays(-90);
        var taiKhoan = await _db.ThongBaos.AsNoTracking()
            .Where(x => x.DaDoc && x.ThoiGian < moc)
            .Select(x => x.MaTK)
            .Distinct()
            .ToListAsync(cancellationToken);
        await _db.ThongBaos.Where(x => x.DaDoc && x.ThoiGian < moc)
            .ExecuteDeleteAsync(cancellationToken);
        foreach (int maTK in taiKhoan)
            XoaCache(maTK);
    }

    private static string KhoaCache(int maTK) => $"ThongBao:Menu:{maTK}";
    private void XoaCache(int maTK) => _cache.Remove(KhoaCache(maTK));
}

public sealed class ThongBaoMenuViewModel
{
    public int ChuaDoc { get; set; }
    public IReadOnlyList<ThongBao> ThongBao { get; set; } = Array.Empty<ThongBao>();
}

public sealed class ThongBaoDanhSachViewModel
{
    public KetQuaTrang<ThongBao> KetQua { get; set; }
    public int ChuaDoc { get; set; }
}
