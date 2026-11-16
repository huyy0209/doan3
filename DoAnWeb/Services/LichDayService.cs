using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services;

public class LichDayService
{
    private readonly QuanLyGiaSuContext _db;

    public LichDayService(QuanLyGiaSuContext db) => _db = db;

    public List<LopHoc> LayLopTrongTuan(DateTime batDauTuan, int? gioiHanGiaSu)
    {
        var ketThucTuan = batDauTuan.Date.AddDays(7);
        var query = _db.LopHocs.AsNoTracking()
            .Include(l => l.GiaSu)
            .Include(l => l.HocVien)
            .Where(l => l.TrangThai != Constants.LopHocStatus.DaHuy &&
                        l.ThoiGianBatDau.HasValue && l.ThoiGianKetThuc.HasValue &&
                        l.ThoiGianBatDau.Value < ketThucTuan &&
                        l.ThoiGianKetThuc.Value >= batDauTuan.Date);

        if (gioiHanGiaSu.HasValue)
            query = query.Where(l => l.MaGS == gioiHanGiaSu.Value);

        return query.ToList();
    }

    public LopHoc LayLop(int maLop, int? gioiHanGiaSu)
    {
        var query = _db.LopHocs.AsNoTracking()
            .Include(l => l.GiaSu)
            .Include(l => l.HocVien)
            .Where(l => l.MaLop == maLop);

        if (gioiHanGiaSu.HasValue)
            query = query.Where(l => l.MaGS == gioiHanGiaSu.Value);

        return query.FirstOrDefault();
    }
}
