using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD.DAL.Repositories
{
    /// <summary>
    /// Repository cho Lớp Học
    /// </summary>
    public class LopHocRepository : Repository<LopHoc>
    {
        public LopHocRepository(GiaSuContextDB context) : base(context)
        {
        }

        /// <summary>
        /// Lớp kèm gia sư + học viên trong MỘT câu truy vấn
        /// (trước đây mỗi lần đọc l.GiaSu / l.HocVien lại gọi thêm 1 truy vấn - lỗi N+1)
        /// </summary>
        public IQueryable<LopHoc> QueryDayDu()
        {
            return _dbSet.Include(l => l.GiaSu).Include(l => l.HocVien);
        }

        public override IEnumerable<LopHoc> GetAll()
        {
            return QueryDayDu().ToList();
        }

        /// <summary>
        /// Lấy lớp đang học của gia sư (với thông tin học viên)
        /// </summary>
        public IEnumerable<object> GetLopHocByGiaSu(int maGS, bool chiConHoatDong = false)
        {
            var hoatDong = Constants.LopHocStatus.HoatDong;
            return _dbSet
                .Where(l => !chiConHoatDong || hoatDong.Contains(l.TrangThai))
                .Include(l => l.HocVien)
                .Where(l => l.MaGS == maGS)
                .OrderByDescending(l => l.ThoiGianBatDau)
                .Select(l => new
                {
                    l.MaLop,
                    HocVien = l.HocVien.HoTen ?? "Chưa có",
                    l.MonHoc,
                    l.CaHoc,
                    BatDau = l.ThoiGianBatDau,
                    KetThuc = l.ThoiGianKetThuc,
                    l.HocPhi,
                    l.TrangThai
                }).ToList();
        }

        /// <summary>
        /// Lấy các lớp còn hoạt động có liên quan tới gia sư HOẶC học viên
        /// (dùng để kiểm tra trùng lịch khi ghép lớp)
        /// </summary>
        public List<LopHoc> GetLopHoatDongCuaGiaSuHoacHocVien(int maGS, int maHV)
        {
            var hoatDong = Constants.LopHocStatus.HoatDong;

            return _dbSet
                .Include(l => l.GiaSu)
                .Include(l => l.HocVien)
                .Where(l => (l.MaGS == maGS || l.MaHV == maHV) && hoatDong.Contains(l.TrangThai))
                .ToList()
                .Where(l => l.DangHoatDong)
                .ToList();
        }

        /// <summary>
        /// Lấy lớp đang học của gia sư
        /// </summary>
        public IEnumerable<LopHoc> GetActiveLopHocByGiaSu(int maGS)
        {
            return _dbSet
                .Include(l => l.HocVien)
                .Where(l => l.MaGS == maGS && l.TrangThai == Constants.LopHocStatus.DangHoc)
                .ToList();
        }

        /// <summary>
        /// Kiểm tra trùng lặp lớp học
        /// </summary>
        public bool IsDuplicateLopHoc(int maHV, int maGS, string monHoc)
        {
            return _dbSet.Any(l =>
                l.MaHV == maHV &&
                l.MaGS == maGS &&
                l.MonHoc == monHoc &&
                l.TrangThai == Constants.LopHocStatus.DangHoc
            );
        }

        /// <summary>
        /// Lấy tất cả lớp với thông tin gia sư và học viên
        /// </summary>
        public IEnumerable<object> GetAllLopHocWithDetails()
        {
            return _dbSet
                .Include(l => l.GiaSu)
                .Include(l => l.HocVien)
                .Select(l => new
                {
                    l.MaLop,
                    GiaSu = l.GiaSu.HoTen ?? "Chưa có",
                    HocVien = l.HocVien.HoTen ?? "Chưa có",
                    l.MonHoc,
                    l.HocPhi,
                    l.TrangThai
                }).ToList();
        }
    }
}