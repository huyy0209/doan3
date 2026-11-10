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
        /// Lớp còn chiếm lịch (chưa bắt đầu / đang diễn ra), lọc ngay trong SQL
        /// </summary>
        public List<LopHoc> GetLopConHoatDong(int? maGS = null)
        {
            var hoatDong = Constants.LopHocStatus.HoatDong;
            var now = DateTime.Now;
            var q = _lopHocRepo.QueryDayDu()
                .Where(l => hoatDong.Contains(l.TrangThai) && (l.ThoiGianKetThuc == null || l.ThoiGianKetThuc >= now));
            if (maGS.HasValue) q = q.Where(l => l.MaGS == maGS.Value);
            return q.ToList();
        }

        /// <summary>
        /// Lớp (chưa hủy) có ít nhất một buổi trong khoảng [tu, den], lọc trong SQL
        /// </summary>
        public List<LopHoc> GetLopTrongKhoang(DateTime tu, DateTime den, int? maGS = null)
        {
            var huy = Constants.LopHocStatus.DaHuy;
            var q = _lopHocRepo.QueryDayDu()
                .Where(l => l.TrangThai != huy && l.ThoiGianBatDau <= den && l.ThoiGianKetThuc >= tu);
            if (maGS.HasValue) q = q.Where(l => l.MaGS == maGS.Value);
            return q.ToList();
        }

        // ===================================================================
        // GHÉP LỚP
        // ===================================================================

        /// <summary>Tên lớp gợi ý: "Môn - Học viên - Ca"</summary>
        public static string GoiYTenLop(string monHoc, string hoTenHocVien, string caHoc)
        {
            var phan = new[] { monHoc?.Trim(), hoTenHocVien?.Trim(), Constants.CaHoc.TenNgan(caHoc) }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            string ten = string.Join(" - ", phan);
            return ten.Length > 140 ? ten.Substring(0, 140) : ten;
        }

        /// <summary>Tên lớp đã có (bỏ qua lớp đang sửa)?</summary>
        public bool TrungTenLop(string ten, int? boQuaMaLop = null)
        {
            ten = (ten ?? "").Trim();
            int boQua = boQuaMaLop ?? 0;
            return ten.Length > 0 && _context.LopHocs.Any(l => l.TenLop == ten && l.MaLop != boQua);
        }

        /// <summary>Gợi ý không trùng: thêm " (2)", " (3)"... nếu tên đã có</summary>
        public string TenLopDuyNhat(string goiY)
        {
            string ten = goiY;
            for (int i = 2; TrungTenLop(ten) && i < 100; i++) ten = $"{goiY} ({i})";
            return ten;
        }

        /// <summary>
        /// Ghép lớp mới. Trạng thái lớp luôn tính theo thời gian.
        ///  - Môn phải có trong danh mục và chưa ngừng dạy.
        ///  - Tên lớp: bỏ trống thì tự đặt theo gợi ý; không được trùng.
        ///  - Môn năng khiếu: cấp độ đầu vào phải thuộc danh sách cấp của môn (bỏ trống = cấp thấp nhất).
        /// </summary>
        public LopHoc GhepLop(Dto.GhepLopYeuCau y)
        {
            // 1. Validate dữ liệu nhập
            string errorMessage;
            if (!LopHocValidator.Validate(y.MaHV, y.MaGS, y.MonHoc, y.HocPhi, out errorMessage))
                throw new ArgumentException(errorMessage);
            if (!LopHocValidator.ValidateLich(y.BatDau, y.KetThuc, y.CaHoc, out errorMessage))
                throw new ArgumentException(errorMessage);

            string monTen = y.MonHoc.Trim();
            var mon = _context.MonHocs.FirstOrDefault(m => m.TenMon == monTen);
            if (mon == null) throw new ArgumentException($"Môn \"{monTen}\" chưa có trong danh mục môn học.");
            if (mon.NgungDay) throw new ArgumentException($"Môn {mon.TenMon} đã ngừng dạy.");

            string capDauVao = null;
            if (mon.LaNangKhieu && mon.DanhSachCapDo.Count > 0)
            {
                capDauVao = string.IsNullOrWhiteSpace(y.CapDoDauVao) ? mon.DanhSachCapDo[0] : y.CapDoDauVao.Trim();
                if (!mon.DanhSachCapDo.Contains(capDauVao, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException($"Cấp độ \"{capDauVao}\" không có trong môn {mon.TenMon}.");
            }

            // 2. Kiểm tra gia sư / học viên tồn tại
            var giaSu = _giaSuRepo.GetById(y.MaGS);
            if (giaSu == null)
                throw new InvalidOperationException("Không tìm thấy gia sư.");
            if (giaSu.TrangThai == Constants.GiaSuStatus.TamNghi)
                throw new InvalidOperationException(Constants.Messages.GiaSuTamNghi);

            var hocVien = _hocVienRepo.GetById(y.MaHV);
            if (hocVien == null)
                throw new InvalidOperationException("Không tìm thấy học viên.");

            // 3. Tên lớp
            string tenLop = string.IsNullOrWhiteSpace(y.TenLop)
                ? TenLopDuyNhat(GoiYTenLop(mon.TenMon, hocVien.HoTen, y.CaHoc))
                : y.TenLop.Trim();
            if (tenLop.Length > 150) throw new ArgumentException("Tên lớp tối đa 150 ký tự.");
            if (TrungTenLop(tenLop)) throw new ArgumentException($"Đã có lớp tên \"{tenLop}\". Hãy đặt tên khác.");

            // 4. Kiểm tra trùng lịch (gia sư + học viên)
            string xungDot = TimXungDot(y.MaGS, y.MaHV, mon.TenMon, y.CaHoc, y.BatDau, y.KetThuc);
            if (xungDot != null)
                throw new InvalidOperationException(xungDot);

            // 5. Tạo lớp - trạng thái tính theo thời gian
            var lopHoc = new LopHoc
            {
                MaHV = y.MaHV,
                MaGS = y.MaGS,
                MonHoc = mon.TenMon,
                TenLop = tenLop,
                CapDoDauVao = capDauVao,
                HocPhi = y.HocPhi,
                ThoiGianBatDau = y.BatDau,
                ThoiGianKetThuc = y.KetThuc,
                CaHoc = y.CaHoc
            };
            lopHoc.TrangThai = lopHoc.TrangThaiTheoThoiGian ?? Constants.LopHocStatus.ChuaBatDau;

            _lopHocRepo.Add(lopHoc);
            _lopHocRepo.SaveChanges();

            // 6. Đồng bộ trạng thái gia sư
            _giaSuRepo.UpdateStatusBasedOnLopHoc(y.MaGS);
            _giaSuRepo.SaveChanges();

            return lopHoc;
        }

        /// <summary>Đổi tên lớp (không trùng, không rỗng)</summary>
        public void DoiTenLop(int maLop, string ten)
        {
            ten = (ten ?? "").Trim();
            if (ten.Length == 0) throw new ArgumentException("Nhập tên lớp.");
            if (ten.Length > 150) throw new ArgumentException("Tên lớp tối đa 150 ký tự.");
            var lop = _lopHocRepo.GetById(maLop);
            if (lop == null) throw new InvalidOperationException("Không tìm thấy lớp học.");
            if (TrungTenLop(ten, maLop)) throw new ArgumentException($"Đã có lớp tên \"{ten}\".");
            lop.TenLop = ten;
            _lopHocRepo.SaveChanges();
        }

        /// <summary>
        /// Danh sách lớp cho trang Lớp học: thông tin lớp + đánh giá
        /// (xếp loại theo điểm TB với môn chính, cấp độ hiện tại với môn năng khiếu).
        /// Chỉ 3 truy vấn: lớp (kèm gia sư, học viên), buổi có điểm / bài thi, danh mục môn.
        /// </summary>
        public List<Dto.LopHocDong> GetDanhSachLop(int? maGS = null)
        {
            var q = _lopHocRepo.QueryDayDu();
            if (maGS.HasValue) q = q.Where(l => l.MaGS == maGS.Value);
            var lops = q.ToList();

            var maLops = lops.Select(l => l.MaLop).ToList();
            var buoi = _context.BuoiHocs
                .Where(b => maLops.Contains(b.MaLop) && (b.Diem != null || b.ThiLenCap))
                .ToList()
                .ToLookup(b => b.MaLop);
            var danhMuc = _context.MonHocs.ToList()
                .GroupBy(m => m.TenMon.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            return lops.Select(l =>
            {
                MonHocDanhMuc mon;
                danhMuc.TryGetValue((l.MonHoc ?? "").Trim(), out mon);
                var cuaLop = buoi[l.MaLop].ToList();
                var diem = cuaLop.Where(b => b.Diem.HasValue).Select(b => b.Diem.Value).ToList();
                decimal? tb = diem.Count > 0 ? Math.Round(diem.Average(), 2) : (decimal?)null;
                bool nk = mon != null && mon.LaNangKhieu && mon.DanhSachCapDo.Count > 0;
                var tt = nk ? TienTrinhCapDo.Tinh(mon.DanhSachCapDo, l.CapDoDauVao, cuaLop) : null;
                return new Dto.LopHocDong
                {
                    MaLop = l.MaLop,
                    MaHV = l.MaHV,
                    MaGS = l.MaGS,
                    TenLop = l.TenHienThi,
                    HocVien = l.HocVien?.HoTen,
                    GiaSu = l.GiaSu?.HoTen,
                    MonHoc = l.MonHoc,
                    LoaiMon = nk ? Constants.LoaiMon.NangKhieu : Constants.LoaiMon.Chinh,
                    CaHoc = l.CaHoc,
                    BatDau = l.ThoiGianBatDau,
                    KetThuc = l.ThoiGianKetThuc,
                    TrangThai = l.TrangThaiTheoThoiGian,
                    HocPhi = l.HocPhi ?? 0,
                    DiemTB = tb,
                    DanhGia = nk ? "Cấp " + tt.CapHienTai : HocTapService.XepLoai(tb),
                    TienTrinh = tt
                };
            })
            .OrderBy(x => x.TrangThai == Constants.LopHocStatus.DaHuy)
            .ThenBy(x => x.TrangThai == Constants.LopHocStatus.DaKetThuc)
            .ThenBy(x => x.TenLop)
            .ToList();
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
            // Chỉ lớp còn có thể đổi trạng thái theo thời gian (bỏ lớp đã kết thúc / đã hủy)
            var hoatDong = Constants.LopHocStatus.HoatDong;
            var allClasses = _lopHocRepo.Find(l => hoatDong.Contains(l.TrangThai)).ToList();
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
        /// Số lớp có buổi học hôm nay theo từng ca (bỏ lớp đã hủy)
        /// </summary>
        public Dictionary<string, int> GetSoLopHomNayTheoCa(int? maGS = null)
        {
            var homNay = DateTime.Now.Date;
            var ngayMai = homNay.AddDays(1);
            var huy = Constants.LopHocStatus.DaHuy;

            // Đếm ngay trong SQL, chỉ trả về 6 dòng (ca, số lớp)
            var q = _context.LopHocs.Where(l => l.TrangThai != huy && l.CaHoc != null &&
                                                l.ThoiGianBatDau < ngayMai && l.ThoiGianKetThuc >= homNay);
            if (maGS.HasValue) q = q.Where(l => l.MaGS == maGS.Value);
            return q.GroupBy(l => l.CaHoc)
                    .Select(g => new { Ca = g.Key, SoLop = g.Count() })
                    .ToList()
                    .ToDictionary(x => x.Ca, x => x.SoLop);
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

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
