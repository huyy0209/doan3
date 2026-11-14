using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD.DAL.Repositories
{
    /// <summary>
    /// Repository cho Gia Sư - Thêm methods đặc thù
    /// OOP: Inheritance - Kế thừa Repository generic
    /// </summary>
    public class GiaSuRepository : Repository<GiaSu>
    {
        public GiaSuRepository(GiaSuContextDB context) : base(context)
        {
        }

        /// <summary>
        /// Lấy gia sư có thể nhận thêm lớp: mọi gia sư trừ "Tạm nghỉ".
        /// Gia sư đang bận vẫn nhận được lớp ở ca khác - xung đột được kiểm tra theo ca.
        /// </summary>
        public IEnumerable<GiaSu> GetGiaSuNhanLop()
        {
            return _dbSet
                .Where(g => g.TrangThai == null || g.TrangThai != Constants.GiaSuStatus.TamNghi)
                .OrderBy(g => g.HoTen)
                .ToList();
        }

        /// <summary>
        /// Lấy gia sư với số lớp đang hoạt động
        /// </summary>
        public IEnumerable<object> GetGiaSuWithLopCount()
        {
            // Biến local để EF dịch thành SQL "IN (...)"
            var hoatDong = Constants.LopHocStatus.HoatDong;

            return _dbSet.Select(g => new
            {
                g.MaGS,
                g.HoTen,
                g.ChuyenMon,
                g.TrangThai,
                SoLop = g.LopHocs.Count(l => hoatDong.Contains(l.TrangThai))
            }).OrderByDescending(g => g.SoLop).ToList();
        }

        /// <summary>
        /// Nạp danh sách lớp của gia sư (null nếu không có gia sư)
        /// </summary>
        private GiaSu LoadWithLopHocs(int maGS)
        {
            var giaSu = _dbSet.Find(maGS);
            if (giaSu == null) return null;

            _context.Entry(giaSu).Collection(g => g.LopHocs).Load();
            return giaSu;
        }

        /// <summary>
        /// Kiểm tra gia sư có lớp đang hoạt động không
        /// </summary>
        public bool HasActiveLopHoc(int maGS)
        {
            return CountActiveLopHoc(maGS) > 0;
        }

        /// <summary>
        /// Đếm số lớp đang hoạt động (chưa bắt đầu / đang diễn ra) của gia sư
        /// </summary>
        public int CountActiveLopHoc(int maGS)
        {
            var giaSu = LoadWithLopHocs(maGS);
            if (giaSu == null) return 0;

            return giaSu.LopHocs.Count(l => l.DangHoatDong &&
                Constants.LopHocStatus.LaHoatDong(l.TrangThaiTheoThoiGian));
        }

        /// <summary>
        /// Đếm tất cả lớp (kể cả đã kết thúc/đã hủy) - dùng khi xóa
        /// </summary>
        public int CountAllLopHoc(int maGS)
        {
            var giaSu = LoadWithLopHocs(maGS);
            return giaSu?.LopHocs.Count ?? 0;
        }

        /// <summary>
        /// Đồng bộ trạng thái gia sư theo lịch dạy thực tế:
        ///  - Có lớp đang diễn ra      => "Đang dạy"
        ///  - Chỉ có lớp chưa bắt đầu => "Bận"
        ///  - Không còn lớp            => "Rảnh"
        ///  - "Tạm nghỉ" do quản lý đặt => giữ nguyên
        /// (Chỉ thay đổi entity, cần gọi SaveChanges sau đó)
        /// </summary>
        public void UpdateStatusBasedOnLopHoc(int maGS)
        {
            var giaSu = LoadWithLopHocs(maGS);
            if (giaSu == null) return;

            if (giaSu.TrangThai == Constants.GiaSuStatus.TamNghi)
                return;

            var trangThaiLop = giaSu.LopHocs
                .Where(l => l.DangHoatDong)
                .Select(l => l.TrangThaiTheoThoiGian)
                .ToList();

            if (trangThaiLop.Any(t => t == Constants.LopHocStatus.DangDienRa ||
                                      t == Constants.LopHocStatus.DangHoc))
                giaSu.TrangThai = Constants.GiaSuStatus.DangDay;
            else if (trangThaiLop.Any(t => t == Constants.LopHocStatus.ChuaBatDau))
                giaSu.TrangThai = Constants.GiaSuStatus.Ban;
            else
                giaSu.TrangThai = Constants.GiaSuStatus.Ranh;
        }

        /// <summary>
        /// Kiểm tra số điện thoại đã được gia sư khác dùng chưa
        /// </summary>
        public bool ExistsSDT(string sdt, int? boQuaMaGS = null)
        {
            if (string.IsNullOrWhiteSpace(sdt)) return false;
            sdt = sdt.Trim();

            return boQuaMaGS.HasValue
                ? _dbSet.Any(g => g.SDT == sdt && g.MaGS != boQuaMaGS.Value)
                : _dbSet.Any(g => g.SDT == sdt);
        }

        /// <summary>
        /// Tìm kiếm gia sư theo keyword
        /// </summary>
        public IEnumerable<GiaSu> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAll();

            // HIGH-04: lọc thô ngay trên SQL Server (chỉ nạp các dòng có khả năng khớp) thay vì
            // kéo toàn bộ bảng về RAM. Dùng collation không phân biệt dấu + đổi "đ" thành "d"
            // để tương đương cách TimKiem.ChuanHoa, rồi vẫn lọc lại bằng TimKiem.Khop cho chắc.
            var tuKhoa = keyword.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string cot = "REPLACE(REPLACE((" +
                "ISNULL(HoTen, N'') + N' ' + ISNULL(SDT, N'') + N' ' + " +
                "ISNULL(ChuyenMon, N'') + N' ' + ISNULL(TrangThai, N'')) " +
                "COLLATE Latin1_General_CI_AI, N'đ', N'd'), N'Đ', N'd')";

            var sql = new StringBuilder("SELECT * FROM dbo.GiaSu WHERE 1 = 1");
            var thamSo = new List<object>();
            for (int i = 0; i < tuKhoa.Length; i++)
            {
                sql.Append($" AND {cot} LIKE @p{i} ESCAPE '\\'");
                thamSo.Add(new SqlParameter($"@p{i}", "%" + EscapeLike(TimKiem.ChuanHoa(tuKhoa[i])) + "%"));
            }

            return _dbSet.SqlQuery(sql.ToString(), thamSo.ToArray())
                .ToList()
                .Where(g => TimKiem.Khop(keyword, g.HoTen, g.SDT, g.ChuyenMon, g.TrangThai))
                .ToList();
        }

        /// <summary>Thoát các ký tự đặc biệt của LIKE để từ khóa được hiểu theo nghĩa đen</summary>
        private static string EscapeLike(string s)
        {
            return s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
        }
    }
}
