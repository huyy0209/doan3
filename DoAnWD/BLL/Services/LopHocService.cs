using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWD.BLL.Validators;
using DoAnWD.Common;
using DoAnWD.DAL.Repositories;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    /// <summary>
    /// Business Logic cho Lớp Học.
    ///
    /// Luật ghép lớp:
    ///  1. Gia sư không được "Tạm nghỉ".
    ///  2. Gia sư KHÔNG có lớp khác cùng ca trong khoảng ngày giao nhau.
    ///  3. Học viên KHÔNG có lớp khác cùng ca trong khoảng ngày giao nhau.
    ///  4. Học viên không học trùng môn với cùng gia sư khi lớp cũ còn hoạt động.
    ///  5. Trạng thái lớp tính theo thời gian (Chưa bắt đầu / Đang diễn ra / Đã kết thúc).
    ///  6. Sau mỗi thay đổi, trạng thái gia sư được đồng bộ theo lịch dạy.
    /// </summary>
    public class LopHocService : IDisposable
    {
        private readonly GiaSuContextDB _context;
        private readonly LopHocRepository _lopHocRepo;
        private readonly GiaSuRepository _giaSuRepo;
        private readonly HocVienRepository _hocVienRepo;

        public LopHocService()
        {
            _context = new GiaSuContextDB();
            _lopHocRepo = new LopHocRepository(_context);
            _giaSuRepo = new GiaSuRepository(_context);
            _hocVienRepo = new HocVienRepository(_context);
        }

        /// <summary>
        /// Lấy tất cả lớp học
        /// </summary>
        public IEnumerable<LopHoc> GetAll()
        {
            return _lopHocRepo.GetAll();
        }

        /// <summary>
        /// Lấy lớp học theo gia sư
        /// </summary>
        public IEnumerable<object> GetByGiaSu(int maGS)
        {
            return _lopHocRepo.GetLopHocByGiaSu(maGS);
        }

        /// <summary>
        /// Lấy tất cả lớp với thông tin chi tiết
        /// </summary>
        public IEnumerable<object> GetAllWithDetails()
        {
            return _lopHocRepo.GetAllLopHocWithDetails();
        }

        // ===================================================================
        // GHÉP LỚP
        // ===================================================================

        /// <summary>
        /// Ghép lớp mới (không có thời gian) - Deprecated, giữ để tương thích
        /// </summary>
        public void GhepLop(int maHV, int maGS, string monHoc, decimal hocPhi, string trangThai)
        {
            DateTime batDau = DateTime.Now.Date.AddDays(1); // Ngày mai
            DateTime ketThuc = batDau.AddMonths(1);         // Sau 1 tháng
            GhepLop(maHV, maGS, monHoc, hocPhi, trangThai, batDau, ketThuc, Constants.CaHoc.Sang);
        }

        /// <summary>
        /// Ghép lớp mới với thời gian và ca học.
        /// Tham số trangThai được bỏ qua: trạng thái luôn tính theo thời gian.
        /// </summary>
        public LopHoc GhepLop(int maHV, int maGS, string monHoc, decimal hocPhi, string trangThai,
            DateTime thoiGianBatDau, DateTime thoiGianKetThuc, string caHoc)
        {
            // 1. Validate dữ liệu nhập
            string errorMessage;
            if (!LopHocValidator.Validate(maHV, maGS, monHoc, hocPhi, out errorMessage))
                throw new ArgumentException(errorMessage);

            if (!LopHocValidator.ValidateLich(thoiGianBatDau, thoiGianKetThuc, caHoc, out errorMessage))
                throw new ArgumentException(errorMessage);

            // 2. Kiểm tra gia sư / học viên tồn tại
            var giaSu = _giaSuRepo.GetById(maGS);
            if (giaSu == null)
                throw new InvalidOperationException("Không tìm thấy gia sư.");
            if (giaSu.TrangThai == Constants.GiaSuStatus.TamNghi)
                throw new InvalidOperationException(Constants.Messages.GiaSuTamNghi);

            var hocVien = _hocVienRepo.GetById(maHV);
            if (hocVien == null)
                throw new InvalidOperationException("Không tìm thấy học viên.");

            // 3. Kiểm tra trùng lịch (gia sư + học viên)
            string xungDot = TimXungDot(maGS, maHV, monHoc, caHoc, thoiGianBatDau, thoiGianKetThuc);
            if (xungDot != null)
                throw new InvalidOperationException(xungDot);

            // 4. Tạo lớp - trạng thái tính theo thời gian
            var lopHoc = new LopHoc
            {
                MaHV = maHV,
                MaGS = maGS,
                MonHoc = monHoc.Trim(),
                HocPhi = hocPhi,
                ThoiGianBatDau = thoiGianBatDau,
                ThoiGianKetThuc = thoiGianKetThuc,
                CaHoc = caHoc
            };
            lopHoc.TrangThai = lopHoc.TrangThaiTheoThoiGian ?? Constants.LopHocStatus.ChuaBatDau;

            _lopHocRepo.Add(lopHoc);
            _lopHocRepo.SaveChanges();

            // 5. Đồng bộ trạng thái gia sư
            _giaSuRepo.UpdateStatusBasedOnLopHoc(maGS);
            _giaSuRepo.SaveChanges();

            return lopHoc;
        }

        /// <summary>
        /// Tìm xung đột lịch khi ghép lớp. Trả về thông báo lỗi, hoặc null nếu hợp lệ.
        /// Dùng chung cho Service (khi lưu) và UI (kiểm tra real-time).
        /// </summary>
        public string TimXungDot(int maGS, int maHV, string monHoc, string caHoc,
            DateTime batDau, DateTime ketThuc)
        {
            var lopMoi = new LopHoc
            {
                MaGS = maGS,
                MaHV = maHV,
                CaHoc = caHoc,
                ThoiGianBatDau = batDau,
                ThoiGianKetThuc = ketThuc
            };

            var lopLienQuan = _lopHocRepo.GetLopHoatDongCuaGiaSuHoacHocVien(maGS, maHV);

            var trungGiaSu = lopLienQuan.FirstOrDefault(l => l.MaGS == maGS && l.TrungLich(lopMoi));
            if (trungGiaSu != null)
                return string.Format("Gia sư đã có lớp {0} (học viên {1}) ở ca {2}, từ {3:dd/MM/yyyy} đến {4:dd/MM/yyyy}.",
                    trungGiaSu.MonHoc, trungGiaSu.HocVien?.HoTen ?? "?", trungGiaSu.CaHoc,
                    trungGiaSu.ThoiGianBatDau, trungGiaSu.ThoiGianKetThuc);

            var trungHocVien = lopLienQuan.FirstOrDefault(l => l.MaHV == maHV && l.TrungLich(lopMoi));
            if (trungHocVien != null)
                return string.Format("Học viên đã có lớp {0} (gia sư {1}) ở ca {2}, từ {3:dd/MM/yyyy} đến {4:dd/MM/yyyy}.",
                    trungHocVien.MonHoc, trungHocVien.GiaSu?.HoTen ?? "?", trungHocVien.CaHoc,
                    trungHocVien.ThoiGianBatDau, trungHocVien.ThoiGianKetThuc);

            if (!string.IsNullOrWhiteSpace(monHoc))
            {
                string mon = monHoc.Trim();
                var trungMon = lopLienQuan.FirstOrDefault(l =>
                    l.MaGS == maGS && l.MaHV == maHV &&
                    string.Equals(l.MonHoc?.Trim(), mon, StringComparison.OrdinalIgnoreCase) &&
                    l.TrungThoiGian(lopMoi));
                if (trungMon != null)
                    return string.Format("Học viên đang học môn {0} với gia sư này (lớp {1}, {2}).",
                        trungMon.MonHoc, trungMon.MaLop, trungMon.CaHoc);
            }

            return null;
        }

        /// <summary>
        /// Các ca đã bị chiếm (của gia sư hoặc học viên) trong khoảng ngày cho trước.
        /// UI dùng để tô đỏ ca không chọn được.
        /// </summary>
        public HashSet<string> GetCaDaBan(int maGS, int maHV, DateTime batDau, DateTime ketThuc)
        {
            var khoang = new LopHoc { ThoiGianBatDau = batDau, ThoiGianKetThuc = ketThuc };

            return new HashSet<string>(
                _lopHocRepo.GetLopHoatDongCuaGiaSuHoacHocVien(maGS, maHV)
                    .Where(l => !string.IsNullOrEmpty(l.CaHoc) && l.TrungThoiGian(khoang))
                    .Select(l => l.CaHoc),
                StringComparer.OrdinalIgnoreCase);
        }

        // ===================================================================
        // TỰ ĐỘNG CẬP NHẬT TRẠNG THÁI
        // ===================================================================

        /// <summary>
        /// Cập nhật trạng thái tất cả lớp theo thời gian, sau đó đồng bộ trạng thái
        /// của các gia sư bị ảnh hưởng.
        /// </summary>
        /// <returns>Số lớp đã thay đổi trạng thái</returns>
        public int AutoUpdateStatus()
        {
            var allClasses = _lopHocRepo.GetAll().ToList();
            var giaSuCanDongBo = new HashSet<int>();
            int updatedCount = 0;

            foreach (var lop in allClasses)
            {
                if (lop.CapNhatTrangThaiTuDong())
                {
                    updatedCount++;
                    if (lop.MaGS.HasValue)
                        giaSuCanDongBo.Add(lop.MaGS.Value);
                }
            }

            foreach (int maGS in giaSuCanDongBo)
                _giaSuRepo.UpdateStatusBasedOnLopHoc(maGS);

            if (updatedCount > 0)
            {
                _lopHocRepo.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"[AUTO UPDATE] Đã cập nhật {updatedCount} lớp học.");
            }

            return updatedCount;
        }

        // ===================================================================
        // TRUY VẤN
        // ===================================================================

        /// <summary>
        /// Lấy các lớp đang diễn ra
        /// </summary>
        public IEnumerable<LopHoc> GetActiveLopHoc()
        {
            return _lopHocRepo.GetAll()
                .Where(l => l.TrangThaiTheoThoiGian == Constants.LopHocStatus.DangDienRa)
                .ToList();
        }

        /// <summary>
        /// Lấy các lớp sắp bắt đầu (trong vòng N ngày), bỏ qua lớp đã hủy
        /// </summary>
        public IEnumerable<LopHoc> GetUpcomingLopHoc(int soNgay = 7)
        {
            var now = DateTime.Now;
            var future = now.AddDays(soNgay);

            return _lopHocRepo.GetAll()
                .Where(l => l.TrangThai != Constants.LopHocStatus.DaHuy &&
                            l.ThoiGianBatDau.HasValue &&
                            l.ThoiGianBatDau.Value >= now &&
                            l.ThoiGianBatDau.Value <= future)
                .ToList();
        }

        /// <summary>
        /// Số lớp có buổi học hôm nay theo từng ca (bỏ lớp đã hủy)
        /// </summary>
        public Dictionary<string, int> GetSoLopHomNayTheoCa()
        {
            var homNay = DateTime.Now.Date;

            return _lopHocRepo.GetAll()
                .Where(l => l.TrangThai != Constants.LopHocStatus.DaHuy &&
                            !string.IsNullOrEmpty(l.CaHoc) &&
                            l.ThoiGianBatDau.HasValue && l.ThoiGianKetThuc.HasValue &&
                            l.ThoiGianBatDau.Value.Date <= homNay &&
                            l.ThoiGianKetThuc.Value.Date >= homNay)
                .GroupBy(l => l.CaHoc)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Lấy lớp học theo ca
        /// </summary>
        public IEnumerable<LopHoc> GetLopHocByCa(string caHoc)
        {
            return _lopHocRepo.GetAll()
                .Where(l => l.CaHoc == caHoc && l.DangHoatDong)
                .ToList();
        }

        // ===================================================================
        // HỦY / XÓA
        // ===================================================================

        /// <summary>
        /// Hủy lớp: giữ lại bản ghi (lịch sử, thống kê) nhưng giải phóng lịch
        /// cho gia sư và học viên.
        /// </summary>
        public void HuyLop(int maLop)
        {
            var lopHoc = _lopHocRepo.GetById(maLop);
            if (lopHoc == null)
                throw new InvalidOperationException("Không tìm thấy lớp học.");

            if (lopHoc.TrangThai == Constants.LopHocStatus.DaHuy)
                throw new InvalidOperationException("Lớp này đã được hủy trước đó.");

            if (lopHoc.TrangThaiTheoThoiGian == Constants.LopHocStatus.DaKetThuc)
                throw new InvalidOperationException("Lớp đã kết thúc, không thể hủy.");

            lopHoc.TrangThai = Constants.LopHocStatus.DaHuy;
            _lopHocRepo.SaveChanges();

            if (lopHoc.MaGS.HasValue)
            {
                _giaSuRepo.UpdateStatusBasedOnLopHoc(lopHoc.MaGS.Value);
                _giaSuRepo.SaveChanges();
            }
        }

        /// <summary>
        /// Xóa hẳn lớp học khỏi CSDL
        /// </summary>
        public void Delete(int maLop)
        {
            var lopHoc = _lopHocRepo.GetById(maLop);
            if (lopHoc == null)
                throw new InvalidOperationException("Không tìm thấy lớp học.");

            int? maGS = lopHoc.MaGS;

            // Đã có điểm danh / đóng tiền => phải giữ lịch sử, chỉ được hủy
            if (_context.BuoiHocs.Any(b => b.MaLop == maLop) || _context.ThanhToans.Any(t => t.MaLop == maLop))
                throw new InvalidOperationException(
                    "Lớp đã có điểm danh hoặc đã đóng học phí nên không xóa hẳn được. Hãy dùng \"Hủy lớp\" để giữ lịch sử.");

            _lopHocRepo.Remove(lopHoc);
            _lopHocRepo.SaveChanges();

            if (maGS.HasValue)
            {
                _giaSuRepo.UpdateStatusBasedOnLopHoc(maGS.Value);
                _giaSuRepo.SaveChanges();
            }
        }

        // ===================================================================
        // ĐẾM
        // ===================================================================

        /// <summary>
        /// Đếm tổng lớp học
        /// </summary>
        public int CountAll()
        {
            return _lopHocRepo.Count();
        }

        /// <summary>
        /// Đếm lớp đang học (bao gồm cả đang diễn ra)
        /// </summary>
        public int CountActive()
        {
            return _lopHocRepo.Count(l =>
                l.TrangThai == Constants.LopHocStatus.DangHoc ||
                l.TrangThai == Constants.LopHocStatus.DangDienRa);
        }

        /// <summary>
        /// Đếm lớp theo trạng thái
        /// </summary>
        public int CountByStatus(string trangThai)
        {
            return _lopHocRepo.Count(l => l.TrangThai == trangThai);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
