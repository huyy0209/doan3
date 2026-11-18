using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DoAnWeb.Services;


/// <summary>Thông tin trung tâm, dùng ở tiêu đề và phiếu báo.</summary>
public class ThongTinTrungTam
{
    public string Ten { get; set; }
    public string DiaChi { get; set; }
    public string SDT { get; set; }

    /// <summary>Tên hiển thị mặc định khi chưa cấu hình.</summary>
    public string TenHienThi => string.IsNullOrWhiteSpace(Ten) ? CaiDatService.TenMacDinh : Ten;
}

/// <summary>Đọc và ghi thông tin trung tâm, cache trong bộ nhớ.</summary>
public class CaiDatService
{
    public const string TenMacDinh = "Trung tâm gia sư";
    private const string KTen = "TenTrungTam";
    private const string KDiaChi = "DiaChi";
    private const string KSDT = "SDT";
    private const string CacheKey = "CaiDat.TrungTam";

    private readonly QuanLyGiaSuContext _db;
    private readonly INguoiDungHienTai _nguoiDung;
    private readonly IMemoryCache _cache;

    public CaiDatService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung, IMemoryCache cache)
    {
        _db = db;
        _nguoiDung = nguoiDung;
        _cache = cache;
    }

    /// <summary>Thông tin trung tâm được nhớ trong bộ nhớ, chỉ đọc CSDL một lần.</summary>
    public ThongTinTrungTam TrungTam =>
        _cache.GetOrCreate(CacheKey, entry =>
        {
            var ds = _db.CaiDats.AsNoTracking()
                .ToDictionary(c => c.Khoa, c => c.GiaTri, StringComparer.OrdinalIgnoreCase);
            string Lay(string khoa) => ds.TryGetValue(khoa, out var giaTri) ? giaTri : null;
            return new ThongTinTrungTam { Ten = Lay(KTen), DiaChi = Lay(KDiaChi), SDT = Lay(KSDT) };
        });

    // Nguồn: DoAnWD/BLL/Services/CaiDatService.cs:49-66
    public void LuuTrungTam(string ten, string diaChi, string sdt)
    {
        _nguoiDung.YeuCauQuanLy();
        ten = (ten ?? "").Trim();
        if (ten.Length == 0) throw new ArgumentException("Nhập tên trung tâm.");
        if (ten.Length > 100) throw new ArgumentException("Tên trung tâm tối đa 100 ký tự.");
        if ((diaChi ?? "").Length > 300) throw new ArgumentException("Địa chỉ tối đa 300 ký tự.");
        if ((sdt ?? "").Length > 50) throw new ArgumentException("Số điện thoại tối đa 50 ký tự.");

        Ghi(KTen, ten);
        Ghi(KDiaChi, diaChi);
        Ghi(KSDT, sdt);
        _db.SaveChanges();
        _cache.Remove(CacheKey);
    }

    private void Ghi(string khoa, string giaTri)
    {
        giaTri = string.IsNullOrWhiteSpace(giaTri) ? null : giaTri.Trim();
        var c = _db.CaiDats.FirstOrDefault(x => x.Khoa == khoa);
        if (c == null) _db.CaiDats.Add(new CaiDat { Khoa = khoa, GiaTri = giaTri });
        else c.GiaTri = giaTri;
    }
}