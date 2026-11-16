using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services
{
    /// <summary>Một tháng trong biểu đồ doanh thu học phí.</summary>
    public class DoanhThuThang
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public string Nhan { get; set; }
        public decimal SoTien { get; set; }
    }

    /// <summary>Một tháng trong biểu đồ học viên mới.</summary>
    public class HocVienMoiThang
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public string Nhan { get; set; }
        public int SoLuong { get; set; }
    }

    /// <summary>Số lớp đang học của một môn.</summary>
    public class LopTheoMon
    {
        public string TenMon { get; set; }
        public int SoLop { get; set; }
    }

    /// <summary>Tỉ lệ chuyên cần của tháng hiện tại.</summary>
    public class TyLeChuyenCan
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public int CoMat { get; set; }
        public int TongBuoi { get; set; }
        public decimal TyLe { get; set; }
    }

    /// <summary>Toàn bộ số liệu cho các biểu đồ ở trang Tổng quan.</summary>
    public class BieuDoTongQuan
    {
        public List<DoanhThuThang> DoanhThu { get; set; } = new List<DoanhThuThang>();
        public List<LopTheoMon> LopTheoMon { get; set; } = new List<LopTheoMon>();
        public List<HocVienMoiThang> HocVienMoi { get; set; } = new List<HocVienMoiThang>();
        public TyLeChuyenCan ChuyenCan { get; set; } = new TyLeChuyenCan();
    }

    /// <summary>
    /// Số liệu biểu đồ trang Tổng quan: doanh thu 12 tháng, số lớp đang học theo môn,
    /// học viên mới 12 tháng và tỉ lệ chuyên cần tháng này.
    /// Mọi phép đếm/tổng đều gom nhóm bằng GroupBy để SQL Server làm, không tải cả bảng về bộ nhớ.
    /// </summary>
    public class ThongKeBieuDoService
    {
        public const int SoThang = 12;

        private readonly QuanLyGiaSuContext _db;

        public ThongKeBieuDoService(QuanLyGiaSuContext db)
        {
            _db = db;
        }

        public BieuDoTongQuan Lay()
        {
            var homNay = DateTime.Today;
            var dauThang = new DateTime(homNay.Year, homNay.Month, 1);
            var tu = dauThang.AddMonths(1 - SoThang);
            var den = dauThang.AddMonths(1);
            var kq = new BieuDoTongQuan();

            // 1. Doanh thu học phí theo tháng: tổng tiền đã đóng trong 12 tháng gần nhất.
            var thu = _db.ThanhToans.AsNoTracking()
                .Where(t => t.NgayDong >= tu && t.NgayDong < den)
                .GroupBy(t => new { t.NgayDong.Year, t.NgayDong.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, SoTien = g.Sum(x => x.SoTien) })
                .ToList();
            for (var i = 0; i < SoThang; i++)
            {
                var thang = tu.AddMonths(i);
                var dong = thu.FirstOrDefault(x => x.Year == thang.Year && x.Month == thang.Month);
                kq.DoanhThu.Add(new DoanhThuThang
                {
                    Nam = thang.Year,
                    Thang = thang.Month,
                    Nhan = NhanThang(thang),
                    SoTien = dong?.SoTien ?? 0
                });
            }

            // 2. Số lớp đang học theo môn (đang diễn ra / đang học), gom theo môn danh mục nếu có.
            var dangHoc = new[] { Constants.LopHocStatus.DangDienRa, Constants.LopHocStatus.DangHoc };
            var nhomMon = _db.LopHocs.AsNoTracking()
                .Where(l => dangHoc.Contains(l.TrangThai))
                .GroupBy(l => new { l.MaMon, l.MonHoc })
                .Select(g => new { g.Key.MaMon, g.Key.MonHoc, SoLop = g.Count() })
                .ToList();
            var maMons = nhomMon.Where(x => x.MaMon.HasValue).Select(x => x.MaMon.Value).Distinct().ToList();
            var tenMons = _db.MonHocs.AsNoTracking()
                .Where(m => maMons.Contains(m.MaMon))
                .Select(m => new { m.MaMon, m.TenMon })
                .ToDictionary(m => m.MaMon, m => m.TenMon);
            kq.LopTheoMon = nhomMon
                .Select(x => new
                {
                    Ten = x.MaMon.HasValue && tenMons.TryGetValue(x.MaMon.Value, out var ten) && !string.IsNullOrWhiteSpace(ten)
                        ? ten
                        : (string.IsNullOrWhiteSpace(x.MonHoc) ? "Chưa xác định" : x.MonHoc),
                    x.SoLop
                })
                .GroupBy(x => x.Ten)
                .Select(g => new LopTheoMon { TenMon = g.Key, SoLop = g.Sum(x => x.SoLop) })
                .OrderByDescending(x => x.SoLop)
                .ThenBy(x => x.TenMon)
                .ToList();

            // 3. Học viên mới theo tháng: học viên có lớp bắt đầu sớm nhất rơi vào tháng đó.
            var hocVienMoi = _db.LopHocs.AsNoTracking()
                .Where(l => l.MaHV != null && l.ThoiGianBatDau != null)
                .GroupBy(l => l.MaHV.Value)
                .Select(g => new { MaHV = g.Key, SomNhat = g.Min(x => x.ThoiGianBatDau) })
                .Where(x => x.SomNhat >= tu && x.SomNhat < den)
                .ToList();
            for (var i = 0; i < SoThang; i++)
            {
                var thang = tu.AddMonths(i);
                kq.HocVienMoi.Add(new HocVienMoiThang
                {
                    Nam = thang.Year,
                    Thang = thang.Month,
                    Nhan = NhanThang(thang),
                    SoLuong = hocVienMoi.Count(x =>
                        x.SomNhat.HasValue && x.SomNhat.Value.Year == thang.Year && x.SomNhat.Value.Month == thang.Month)
                });
            }

            // 4. Chuyên cần tháng này: có mặt (kể cả đi trễ) trên tổng số buổi đã điểm danh.
            var demDiemDanh = _db.BuoiHocs.AsNoTracking()
                .Where(b => b.NgayHoc >= dauThang && b.NgayHoc < den && b.DiemDanh != null)
                .GroupBy(b => b.DiemDanh)
                .Select(g => new { TrangThai = g.Key, SoBuoi = g.Count() })
                .ToList();
            var tongBuoi = demDiemDanh
                .Where(x => Constants.DiemDanh.TatCa.Contains(x.TrangThai))
                .Sum(x => x.SoBuoi);
            var coMat = demDiemDanh
                .Where(x => x.TrangThai == Constants.DiemDanh.CoMat || x.TrangThai == Constants.DiemDanh.DiTre)
                .Sum(x => x.SoBuoi);
            kq.ChuyenCan = new TyLeChuyenCan
            {
                Nam = homNay.Year,
                Thang = homNay.Month,
                CoMat = coMat,
                TongBuoi = tongBuoi,
                TyLe = tongBuoi == 0 ? 0 : Math.Round((decimal)coMat * 100 / tongBuoi, 1)
            };

            return kq;
        }

        private static string NhanThang(DateTime thang) => $"Tháng {thang.Month}/{thang.Year}";
    }
}
