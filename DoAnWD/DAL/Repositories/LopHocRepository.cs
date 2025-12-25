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
        /// Lấy lớp đang học của gia sư (với thông tin học viên)
        /// </summary>
        public IEnumerable<object> GetLopHocByGiaSu(int maGS)
        {
            return _dbSet
                .Include(l => l.HocVien)
                .Where(l => l.MaGS == maGS)
                .Select(l => new
                {
                    l.MaLop,
                    HocVien = l.HocVien.HoTen ?? "Chưa có",
                    l.MonHoc,
                    l.HocPhi,
                    l.TrangThai
                }).ToList();
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