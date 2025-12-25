using System.Collections.Generic;
using System.Linq;
using DoAnWD.Models;

namespace DoAnWD.DAL.Repositories
{
    public class HocVienRepository : Repository<HocVien>
    {
        public HocVienRepository(GiaSuContextDB context) : base(context)
        {
        }

        /// <summary>
        /// Tìm kiếm học viên (BỎ EMAIL)
        /// </summary>
        public IEnumerable<HocVien> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAll();

            keyword = keyword.ToLower();
            return _dbSet.Where(h =>
                (h.HoTen != null && h.HoTen.ToLower().Contains(keyword)) ||
                (h.SDT != null && h.SDT.Contains(keyword)) ||
                (h.DiaChi != null && h.DiaChi.ToLower().Contains(keyword))
            ).ToList();
        }

        public bool HasActiveLopHoc(int maHV)
        {
            var hocVien = _dbSet.Find(maHV);
            if (hocVien == null) return false;

            _context.Entry(hocVien).Collection(h => h.LopHocs).Load();
            return hocVien.LopHocs.Any(l => l.TrangThai == Common.Constants.LopHocStatus.DangHoc);
        }
    }
}