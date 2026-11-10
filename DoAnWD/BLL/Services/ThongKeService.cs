using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWD.Common;
using DoAnWD.DAL.Repositories;

namespace DoAnWD.BLL.Services
{
    /// <summary>
    /// ✨ Business Logic cho Thống Kê - CẬP NHẬT CHO OPTION 2
    /// </summary>
    public class ThongKeService : IDisposable
    {
        private readonly Models.GiaSuContextDB _context;
        private readonly GiaSuRepository _giaSuRepo;
        private readonly HocVienRepository _hocVienRepo;
        private readonly LopHocRepository _lopHocRepo;

        public ThongKeService()
        {
            _context = new Models.GiaSuContextDB();
            _giaSuRepo = new GiaSuRepository(_context);
            _hocVienRepo = new HocVienRepository(_context);
            _lopHocRepo = new LopHocRepository(_context);
        }

        /// <summary>
        /// ✨ Lấy số liệu thống kê tổng quan - CẬP NHẬT CHO OPTION 2
        /// </summary>
        public ThongKeStats GetStatistics()
        {
            var hoatDong = Constants.LopHocStatus.HoatDong;
            // Đếm lớp theo trạng thái bằng một truy vấn GROUP BY thay vì đếm từng trạng thái
            var theoTrangThai = _context.LopHocs.GroupBy(l => l.TrangThai)
                .Select(g => new { g.Key, SoLop = g.Count() }).ToList()
                .ToDictionary(x => x.Key ?? "", x => x.SoLop);
            Func<string, int> dem = tt => { int n; return theoTrangThai.TryGetValue(tt, out n) ? n : 0; };
            var dauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var dauThangSau = dauThang.AddMonths(1);

            return new ThongKeStats
            {
                TongGiaSu = _giaSuRepo.Count(),
                TongHocVien = _hocVienRepo.Count(),
                TongLopHoc = _lopHocRepo.Count(),

                // ✅ SỬA: Dùng trạng thái mới
                GiaSuRanh = _giaSuRepo.Count(g =>
                    g.TrangThai == Constants.GiaSuStatus.Ranh ||
                    g.TrangThai == Constants.GiaSuStatus.SanSang ||
                    g.TrangThai == null),
                GiaSuTamNghi = _giaSuRepo.Count(g => g.TrangThai == Constants.GiaSuStatus.TamNghi),
                GiaSuDangDay = _giaSuRepo.Count(g =>
                    g.TrangThai == Constants.GiaSuStatus.Ban ||
                    g.TrangThai == Constants.GiaSuStatus.DangDay),

                LopChuaBatDau = dem(Constants.LopHocStatus.ChuaBatDau),
                LopDangDienRa = dem(Constants.LopHocStatus.DangDienRa) + dem(Constants.LopHocStatus.DangHoc),
                LopDaKetThuc = dem(Constants.LopHocStatus.DaKetThuc),
                LopDangHoc = dem(Constants.LopHocStatus.DangDienRa) + dem(Constants.LopHocStatus.DangHoc),
                LopDaHuy = dem(Constants.LopHocStatus.DaHuy),
                // Học viên đang có lớp chưa kết thúc
                HocVienDangHoc = _context.LopHocs.Where(l => l.MaHV != null && hoatDong.Contains(l.TrangThai))
                    .Select(l => l.MaHV).Distinct().Count(),

                // Doanh thu = tổng học phí (không tính lớp đã hủy)
                DoanhThuDangHoatDong = _context.LopHocs
                    .Where(l => hoatDong.Contains(l.TrangThai))
                    .Sum(l => l.HocPhi) ?? 0,
                DoanhThuThangNay = _context.LopHocs
                    .Where(l => l.TrangThai != Constants.LopHocStatus.DaHuy &&
                                l.ThoiGianBatDau >= dauThang && l.ThoiGianBatDau < dauThangSau)
                    .Sum(l => l.HocPhi) ?? 0
            };
        }

        /// <summary>
        /// Lấy danh sách gia sư với số lớp
        /// </summary>
        public IEnumerable<object> GetGiaSuWithLopCount()
        {
            return _giaSuRepo.GetGiaSuWithLopCount();
        }

        /// <summary>
        /// Lấy lớp học theo gia sư
        /// </summary>
        public IEnumerable<object> GetLopHocByGiaSu(int maGS)
        {
            return _lopHocRepo.GetLopHocByGiaSu(maGS);
        }

        /// <summary>
        /// Đếm số lớp đang dạy của gia sư
        /// </summary>
        public int CountActiveLopHocByGiaSu(int maGS)
        {
            return _giaSuRepo.CountActiveLopHoc(maGS);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

    /// <summary>
    /// DTO cho thống kê
    /// OOP: Encapsulation - Đóng gói dữ liệu thống kê
    /// </summary>
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

        /// <summary>Tổng học phí các lớp đang hoạt động</summary>
        public decimal DoanhThuDangHoatDong { get; set; }

        /// <summary>Tổng học phí các lớp khai giảng trong tháng hiện tại</summary>
        public decimal DoanhThuThangNay { get; set; }
    }
}