using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services;


/// <summary>Business logic cho thống kê tổng quan.</summary>
public class ThongKeService
{
    private readonly QuanLyGiaSuContext _db;

    public ThongKeService(QuanLyGiaSuContext db)
    {
        _db = db;
    }

    // Nguồn: DoAnWD/BLL/Services/ThongKeService.cs:29-82
    public ThongKeStats GetStatistics()
    {
        var hoatDong = Constants.LopHocStatus.HoatDong;
        var theoTrangThai = _db.LopHocs.AsNoTracking().GroupBy(l => l.TrangThai)
            .Select(g => new { g.Key, SoLop = g.Count() }).ToList()
            .ToDictionary(x => x.Key ?? "", x => x.SoLop);
        int Dem(string tt) => theoTrangThai.TryGetValue(tt, out var n) ? n : 0;
        var dauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var dauThangSau = dauThang.AddMonths(1);

        return new ThongKeStats
        {
            TongGiaSu = _db.GiaSus.AsNoTracking().Count(),
            TongHocVien = _db.HocViens.AsNoTracking().Count(),
            TongLopHoc = _db.LopHocs.AsNoTracking().Count(),
            GiaSuRanh = _db.GiaSus.AsNoTracking().Count(g =>
                g.TrangThai == Constants.GiaSuStatus.Ranh ||
                g.TrangThai == Constants.GiaSuStatus.SanSang ||
                g.TrangThai == null),
            GiaSuTamNghi = _db.GiaSus.AsNoTracking().Count(g => g.TrangThai == Constants.GiaSuStatus.TamNghi),
            GiaSuDangDay = _db.GiaSus.AsNoTracking().Count(g =>
                g.TrangThai == Constants.GiaSuStatus.Ban ||
                g.TrangThai == Constants.GiaSuStatus.DangDay),

            LopChuaBatDau = Dem(Constants.LopHocStatus.ChuaBatDau),
            LopDangDienRa = Dem(Constants.LopHocStatus.DangDienRa) + Dem(Constants.LopHocStatus.DangHoc),
            LopDaKetThuc = Dem(Constants.LopHocStatus.DaKetThuc),
            LopDangHoc = Dem(Constants.LopHocStatus.DangDienRa) + Dem(Constants.LopHocStatus.DangHoc),
            LopDaHuy = Dem(Constants.LopHocStatus.DaHuy),
            HocVienDangHoc = _db.LopHocs.AsNoTracking()
                .Where(l => l.MaHV != null && hoatDong.Contains(l.TrangThai))
                .Select(l => l.MaHV).Distinct().Count(),

            // MED-03: Doanh thu theo dòng tiền thực thu (ThanhToan) thay vì học phí danh nghĩa.
            DoanhThuDangHoatDong = _db.ThanhToans.AsNoTracking()
                .Where(t => hoatDong.Contains(t.LopHoc.TrangThai))
                .Sum(t => (decimal?)t.SoTien) ?? 0,
            DoanhThuThangNay = _db.ThanhToans.AsNoTracking()
                .Where(t => t.NgayDong >= dauThang && t.NgayDong < dauThangSau)
                .Sum(t => (decimal?)t.SoTien) ?? 0,
            TongGiaTriLopKhaiGiangThangNay = _db.LopHocs.AsNoTracking()
                .Where(l => l.TrangThai != Constants.LopHocStatus.DaHuy &&
                            l.ThoiGianBatDau >= dauThang && l.ThoiGianBatDau < dauThangSau)
                .Sum(l => l.HocPhi) ?? 0
        };
    }

    // Nguồn: DoAnWD/BLL/Services/ThongKeService.cs:87-93
    public IEnumerable<object> GetGiaSuWithLopCount()
    {
        var hoatDong = Constants.LopHocStatus.HoatDong;
        return _db.GiaSus.AsNoTracking()
            .Select(g => new
            {
                g.MaGS,
                g.HoTen,
                g.ChuyenMon,
                g.TrangThai,
                SoLop = g.LopHocs.Count(l => hoatDong.Contains(l.TrangThai))
            })
            .OrderByDescending(g => g.SoLop)
            .ToList();
    }

    // Nguồn: DoAnWD/BLL/Services/ThongKeService.cs:97-103
    public IEnumerable<object> GetLopHocByGiaSu(int maGS)
    {
        return _db.LopHocs.AsNoTracking().Where(l => l.MaGS == maGS)
            .OrderByDescending(l => l.ThoiGianBatDau)
            .Select(l => new
            {
                l.MaLop,
                TenLop = l.TenLop ?? (l.MonHoc + " - " + l.HocVien.HoTen),
                HocVien = l.HocVien.HoTen ?? "Chưa có",
                l.MonHoc,
                l.CaHoc,
                BatDau = l.ThoiGianBatDau,
                KetThuc = l.ThoiGianKetThuc,
                l.HocPhi,
                l.TrangThai
            })
            .ToList();
    }

    // Nguồn: DoAnWD/BLL/Services/ThongKeService.cs:107-113
    public int CountActiveLopHocByGiaSu(int maGS)
    {
        var giaSu = _db.GiaSus.AsNoTracking().Include(g => g.LopHocs)
            .FirstOrDefault(g => g.MaGS == maGS);
        if (giaSu == null) return 0;

        return giaSu.LopHocs.Count(l => l.DangHoatDong &&
            Constants.LopHocStatus.LaHoatDong(l.TrangThaiTheoThoiGian));
    }
}

/// <summary>DTO cho thống kê.</summary>
public class ThongKeStats
{
    public int TongGiaSu { get; set; }
    public int TongHocVien { get; set; }
    public int TongLopHoc { get; set; }
    public int GiaSuRanh { get; set; }
    public int GiaSuDangDay { get; set; }
    public int LopDangHoc { get; set; }
    public int LopDaHuy { get; set; }
    public int LopChuaBatDau { get; set; }
    public int LopDangDienRa { get; set; }
    public int LopDaKetThuc { get; set; }
    public int HocVienDangHoc { get; set; }
    public int GiaSuTamNghi { get; set; }
    /// <summary>MED-03: Thực thu từ các lớp đang hoạt động theo dòng tiền thực tế.</summary>
    public decimal DoanhThuDangHoatDong { get; set; }
    /// <summary>MED-03: Dòng tiền thực thu trong tháng hiện tại (theo bảng ThanhToan).</summary>
    public decimal DoanhThuThangNay { get; set; }
    /// <summary>MED-03: Tổng học phí danh nghĩa các lớp khai giảng trong tháng hiện tại.</summary>
    public decimal TongGiaTriLopKhaiGiangThangNay { get; set; }
}