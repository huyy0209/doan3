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
                    TenLop = l.TenLop ?? (l.MonHoc + " - " + l.HocVien.HoTen),
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

    }
}