using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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

            // HIGH-04: lọc thô ngay trên SQL Server (chỉ nạp các dòng có khả năng khớp) thay vì
            // kéo toàn bộ bảng về RAM. Dùng collation không phân biệt dấu + đổi "đ" thành "d"
            // để tương đương cách TimKiem.ChuanHoa, rồi vẫn lọc lại bằng TimKiem.Khop cho chắc.
            var tuKhoa = keyword.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string cot = "REPLACE(REPLACE((" +
                "ISNULL(HoTen, N'') + N' ' + ISNULL(SDT, N'') + N' ' + ISNULL(DiaChi, N'') + N' ' + " +
                "ISNULL(TenPhuHuynh, N'') + N' ' + ISNULL(SDTPhuHuynh, N'') + N' ' + " +
                "ISNULL(TruongLop, N'') + N' ' + ISNULL(TrinhDo, N'')) " +
                "COLLATE Latin1_General_CI_AI, N'đ', N'd'), N'Đ', N'd')";

            var sql = new StringBuilder("SELECT * FROM dbo.HocVien WHERE 1 = 1");
            var thamSo = new List<object>();
            for (int i = 0; i < tuKhoa.Length; i++)
            {
                sql.Append($" AND {cot} LIKE @p{i} ESCAPE '\\'");
                thamSo.Add(new SqlParameter($"@p{i}", "%" + EscapeLike(Common.TimKiem.ChuanHoa(tuKhoa[i])) + "%"));
            }

            return _dbSet.SqlQuery(sql.ToString(), thamSo.ToArray())
                .ToList()
                .Where(h => Common.TimKiem.Khop(keyword,
                    h.HoTen, h.SDT, h.DiaChi, h.TenPhuHuynh, h.SDTPhuHuynh, h.TruongLop, h.TrinhDo))
                .ToList();
        }

        /// <summary>Thoát các ký tự đặc biệt của LIKE để từ khóa được hiểu theo nghĩa đen</summary>
        private static string EscapeLike(string s)
        {
            return s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
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