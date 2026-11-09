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

            // Lọc trong bộ nhớ để không phân biệt dấu tiếng Việt
            return _dbSet.ToList().Where(h => Common.TimKiem.Khop(keyword,
                h.HoTen, h.SDT, h.DiaChi, h.TenPhuHuynh, h.SDTPhuHuynh, h.TruongLop, h.TrinhDo)).ToList();
        }

        /// <summary>
        /// Học viên còn lớp đang hoạt động (chưa bắt đầu / đang diễn ra) không.
        /// Trước đây chỉ kiểm tra "Đang học" nên vẫn xóa được học viên có lớp sắp học.
        /// </summary>
        public bool HasActiveLopHoc(int maHV)
        {
            var hocVien = _dbSet.Find(maHV);
            if (hocVien == null) return false;

            _context.Entry(hocVien).Collection(h => h.LopHocs).Load();
            return hocVien.LopHocs.Any(l => l.DangHoatDong &&
                Common.Constants.LopHocStatus.LaHoatDong(l.TrangThaiTheoThoiGian));
        }

        /// <summary>
        /// Đếm tất cả lớp (kể cả đã kết thúc/đã hủy) của học viên
        /// </summary>
        public int CountAllLopHoc(int maHV)
        {
            var hocVien = _dbSet.Find(maHV);
            if (hocVien == null) return 0;

            _context.Entry(hocVien).Collection(h => h.LopHocs).Load();
            return hocVien.LopHocs.Count;
        }
    }
}