using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services;


/// <summary>Một dòng gợi ý / kết quả tìm kiếm nhanh (đã kèm đường dẫn tới trang chi tiết).</summary>
public class GoiYTimKiem
{
    public int Id { get; set; }
    public string TieuDe { get; set; }
    public string Phu { get; set; }
    public string Url { get; set; }
}

/// <summary>Kết quả tìm kiếm nhanh toàn cục: ba nhóm Học viên – Gia sư – Lớp.</summary>
public class KetQuaTimKiemNhanh
{
    public string TuKhoa { get; set; } = "";
    public int TongHocVien { get; set; }
    public int TongGiaSu { get; set; }
    public int TongLop { get; set; }
    public List<GoiYTimKiem> HocViens { get; set; } = new();
    public List<GoiYTimKiem> GiaSus { get; set; } = new();
    public List<GoiYTimKiem> Lops { get; set; } = new();
}

/// <summary>
/// G9 – Tìm kiếm nhanh toàn cục (ô tìm trên thanh menu + trang /TimKiem).
/// Tìm không dấu / không phân biệt hoa thường bằng cách chuẩn hóa từ khóa (Common/TimKiem) và
/// chuẩn hóa cột ngay trên SQL (REPLACE "đ"→"d" + COLLATE Latin1_General_CI_AI), lọc bằng
/// <c>Where</c> + <c>Take</c> nên không tải cả bảng.
/// Phân quyền: gia sư chỉ thấy học viên mình dạy (đúng luật HocVienService.DuocXem) và lớp của mình,
/// không thấy nhóm Gia sư.
/// </summary>
public class TimKiemNhanhService
{
    /// <summary>Số gợi ý tối đa mỗi nhóm khi đang gõ.</summary>
    public const int SoGoiYMoiNhom = 5;

    /// <summary>Số dòng tối đa mỗi nhóm trên trang kết quả đầy đủ.</summary>
    public const int SoDongTrangDayDu = 50;

    private const string SapXepKhongDau = "Latin1_General_CI_AI";

    private readonly QuanLyGiaSuContext _db;
    private readonly INguoiDungHienTai _nguoiDung;

    public TimKiemNhanhService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung)
    {
        _db = db;
        _nguoiDung = nguoiDung;
    }

    /// <summary>Gợi ý khi đang gõ: tối đa <see cref="SoGoiYMoiNhom"/> dòng mỗi nhóm, không đếm tổng.</summary>
    public KetQuaTimKiemNhanh GoiY(string tuKhoa) => Tim(tuKhoa, SoGoiYMoiNhom, demTong: false);

    /// <summary>Trang kết quả đầy đủ: tối đa <see cref="SoDongTrangDayDu"/> dòng mỗi nhóm, có đếm tổng.</summary>
    public KetQuaTimKiemNhanh TimDayDu(string tuKhoa) => Tim(tuKhoa, SoDongTrangDayDu, demTong: true);

    private KetQuaTimKiemNhanh Tim(string tuKhoa, int soMoiNhom, bool demTong)
    {
        string goc = (tuKhoa ?? "").Trim();
        var kq = new KetQuaTimKiemNhanh { TuKhoa = goc };

        // Dưới 2 ký tự (sau khi bỏ dấu) thì không tìm: tránh quét cả bảng vì từ khóa quá ngắn.
        string chuan = TimKiem.ChuanHoa(goc).Trim();
        if (chuan.Length < 2) return kq;

        string[] tu = chuan.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string[] mau = tu
            .Select(t => "%" + GiaSuService.EscapeLike(t) + "%")
            .ToArray();

        var hocVien = LocHocVien(mau);
        if (demTong) kq.TongHocVien = hocVien.Count();
        kq.HocViens = hocVien
            .OrderBy(h => h.HoTen).ThenBy(h => h.MaHV)
            .Select(h => new { h.MaHV, h.HoTen, h.SDT })
            .Take(soMoiNhom)
            .ToList()
            .Select(h => new GoiYTimKiem
            {
                Id = h.MaHV,
                TieuDe = h.HoTen,
                Phu = h.SDT,
                Url = "/HocVien/Details/" + h.MaHV
            })
            .ToList();

        // Gia sư không được thấy danh sách gia sư khác.
        if (_nguoiDung.LaQuanLy)
        {
            var giaSu = LocGiaSu(mau);
            if (demTong) kq.TongGiaSu = giaSu.Count();
            kq.GiaSus = giaSu
                .OrderBy(g => g.HoTen).ThenBy(g => g.MaGS)
                .Select(g => new { g.MaGS, g.HoTen, g.SDT, g.ChuyenMon })
                .Take(soMoiNhom)
                .ToList()
                .Select(g => new GoiYTimKiem
                {
                    Id = g.MaGS,
                    TieuDe = g.HoTen,
                    Phu = Noi(g.SDT, g.ChuyenMon),
                    Url = "/GiaSu/Details/" + g.MaGS
                })
                .ToList();
        }

        var lop = LocLop(mau);
        if (demTong) kq.TongLop = lop.Count();
        bool laQuanLy = _nguoiDung.LaQuanLy;
        kq.Lops = lop
            .OrderBy(l => l.MonHoc).ThenBy(l => l.MaLop)
            .Select(l => new
            {
                l.MaLop,
                l.MaHV,
                l.MonHoc,
                l.TenLop,
                TenGiaSu = l.GiaSu.HoTen,
                TenHocVien = l.HocVien.HoTen
            })
            .Take(soMoiNhom)
            .ToList()
            .Select(l => new GoiYTimKiem
            {
                Id = l.MaLop,
                TieuDe = string.IsNullOrWhiteSpace(l.MonHoc) ? l.TenLop : l.MonHoc,
                Phu = Noi(
                    string.IsNullOrWhiteSpace(l.TenGiaSu) ? null : "GV: " + l.TenGiaSu,
                    string.IsNullOrWhiteSpace(l.TenHocVien) ? null : "HV: " + l.TenHocVien),
                // Quản lý mở trang chi tiết lớp; gia sư mở kết quả học tập của học viên trong lớp
                // (trang Lớp học chỉ dành cho quản lý).
                Url = laQuanLy
                    ? "/LopHoc/Details/" + l.MaLop
                    : (l.MaHV.HasValue ? "/HocTap/KetQua/" + l.MaHV.Value : "/Lich")
            })
            .ToList();

        return kq;
    }

    private static string Noi(params string[] phan) =>
        string.Join(" · ", phan.Where(s => !string.IsNullOrWhiteSpace(s)));

    /// <summary>Học viên người dùng hiện tại được xem (quản lý = tất cả; gia sư theo HocVienService.DuocXem).</summary>
    private IQueryable<HocVien> LocHocVien(string[] mau)
    {
        IQueryable<HocVien> q = _db.HocViens.AsNoTracking();
        int? gioiHan = _nguoiDung.GioiHanGiaSu;
        if (gioiHan.HasValue)
        {
            int maGS = gioiHan.Value;
            q = q.Where(h => h.LopHocs.Any(l => l.MaGS == maGS ||
                                                l.BuoiHocs.Any(b => b.MaGSDay == maGS)));
        }

        foreach (string p in mau)
        {
            string mauLike = p;
            q = q.Where(h => EF.Functions.Like(
                EF.Functions.Collate(
                    ((h.HoTen ?? "") + " " + (h.SDT ?? ""))
                        .Replace("đ", "d").Replace("Đ", "d"),
                    SapXepKhongDau),
                mauLike, "\\"));
        }
        return q;
    }

    /// <summary>Tìm gia sư theo tên, SĐT (chỉ quản lý dùng).</summary>
    private IQueryable<GiaSu> LocGiaSu(string[] mau)
    {
        IQueryable<GiaSu> q = _db.GiaSus.AsNoTracking();
        foreach (string p in mau)
        {
            string mauLike = p;
            q = q.Where(g => EF.Functions.Like(
                EF.Functions.Collate(
                    ((g.HoTen ?? "") + " " + (g.SDT ?? ""))
                        .Replace("đ", "d").Replace("Đ", "d"),
                    SapXepKhongDau),
                mauLike, "\\"));
        }
        return q;
    }

    /// <summary>Tìm lớp theo môn + tên gia sư + tên học viên; gia sư chỉ thấy lớp của mình.</summary>
    private IQueryable<LopHoc> LocLop(string[] mau)
    {
        IQueryable<LopHoc> q = _db.LopHocs.AsNoTracking();
        int? gioiHan = _nguoiDung.GioiHanGiaSu;
        if (gioiHan.HasValue)
        {
            int maGS = gioiHan.Value;
            q = q.Where(l => l.MaGS == maGS ||
                             l.BuoiHocs.Any(b => b.MaGSDay == maGS));
        }

        foreach (string p in mau)
        {
            string mauLike = p;
            q = q.Where(l => EF.Functions.Like(
                EF.Functions.Collate(
                    ((l.MonHoc ?? "") + " " + (l.GiaSu.HoTen ?? "") + " " + (l.HocVien.HoTen ?? ""))
                        .Replace("đ", "d").Replace("Đ", "d"),
                    SapXepKhongDau),
                mauLike, "\\"));
        }
        return q;
    }
}