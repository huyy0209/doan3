// Nguồn: DoAnWD/BLL/Services/LopHocService.cs (luật LH-01..LH-25). Chuyển nguyên nghiệp vụ, đổi repository → DbContext.
using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Services.Validators;
using DoAnWeb.Common;
using DoAnWeb.Data;
using Microsoft.EntityFrameworkCore;
using DoAnWeb.Data.Entities;

namespace DoAnWeb.Services
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
    public class LopHocService
    {
        private readonly QuanLyGiaSuContext _db;
        private readonly INguoiDungHienTai _nguoiDung;

        public LopHocService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung)
        {
            _db = db;
            _nguoiDung = nguoiDung;
        }

        /// <summary>Lớp kèm gia sư + học viên trong một truy vấn (LopHocRepository.cs:22-25)</summary>
        public IQueryable<LopHoc> QueryDayDu() => _db.LopHocs.Include(l => l.GiaSu).Include(l => l.HocVien);

        /// <summary>Một lớp kèm gia sư, học viên (null nếu không có)</summary>
        public LopHoc GetById(int maLop) => QueryDayDu().FirstOrDefault(l => l.MaLop == maLop);

        /// <summary>
        /// Lớp còn hoạt động liên quan tới gia sư HOẶC học viên – dùng kiểm tra trùng lịch (LopHocRepository.cs:61-72)
        /// </summary>
        private List<LopHoc> GetLopHoatDongCuaGiaSuHoacHocVien(int maGS, int maHV)
        {
            var hoatDong = Constants.LopHocStatus.HoatDong;
            return QueryDayDu()
                .Where(l => (l.MaGS == maGS || l.MaHV == maHV) && hoatDong.Contains(l.TrangThai))
                .ToList()
                .Where(l => l.DangHoatDong)
                .ToList();
        }

        /// <summary>
        /// Lấy tất cả lớp học
        /// </summary>
        public IEnumerable<LopHoc> GetAll()
        {
            return QueryDayDu().ToList();
        }

        /// <summary>
        /// Lớp còn chiếm lịch (chưa bắt đầu / đang diễn ra), lọc ngay trong SQL
        /// </summary>
        public List<LopHoc> GetLopConHoatDong(int? maGS = null)
        {
            var hoatDong = Constants.LopHocStatus.HoatDong;
            var now = DateTime.Now;
            var q = QueryDayDu()
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
            var q = QueryDayDu()
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
            return ten.Length > 0 && _db.LopHocs.Any(l => l.TenLop == ten && l.MaLop != boQua);
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
            _nguoiDung.YeuCauQuanLy();
            // 1. Validate dữ liệu nhập
            string errorMessage;
            if (!LopHocValidator.Validate(y.MaHV, y.MaGS, y.MonHoc, y.HocPhi, out errorMessage))
                throw new ArgumentException(errorMessage);
            if (!LopHocValidator.ValidateLich(y.BatDau, y.KetThuc, y.CaHoc, out errorMessage))
                throw new ArgumentException(errorMessage);

            string monTen = y.MonHoc.Trim();
            var mon = _db.MonHocs.FirstOrDefault(m => m.TenMon == monTen);
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
            var giaSu = _db.GiaSus.Find(y.MaGS);
            if (giaSu == null)
                throw new InvalidOperationException("Không tìm thấy gia sư.");
            if (giaSu.TrangThai == Constants.GiaSuStatus.TamNghi)
                throw new InvalidOperationException(Constants.Messages.GiaSuTamNghi);

            var hocVien = _db.HocViens.Find(y.MaHV);
            if (hocVien == null)
                throw new InvalidOperationException("Không tìm thấy học viên.");

            // 3. Tên lớp
            string tenLop = string.IsNullOrWhiteSpace(y.TenLop)
                ? TenLopDuyNhat(GoiYTenLop(mon.TenMon, hocVien.HoTen, y.CaHoc))
                : y.TenLop.Trim();
            if (tenLop.Length > 150) throw new ArgumentException("Tên lớp tối đa 150 ký tự.");
            if (TrungTenLop(tenLop)) throw new ArgumentException($"Đã có lớp tên \"{tenLop}\". Hãy đặt tên khác.");

            string thuHoc = Constants.ThuHoc.Ghi(Constants.ThuHoc.Doc(y.ThuHoc));

            // Review #3 (LH-12, LH-13): kiểm trùng lịch + ghi lớp + đồng bộ gia sư trong CÙNG một transaction,
            // có khóa ứng dụng để hai yêu cầu ghép/sửa không chạy xen nhau. Lỗi → rollback (LH-18, CRIT-01).
            LopHoc lopHoc;
            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    LayKhoaGhepLop();

                    // 4. Kiểm tra trùng lịch (gia sư + học viên)
                    string xungDot = TimXungDot(y.MaGS, y.MaHV, mon.TenMon, y.CaHoc, y.BatDau, y.KetThuc, thuHoc);
                    if (xungDot != null)
                        throw new InvalidOperationException(xungDot);

                    // 5. Tạo lớp - trạng thái tính theo thời gian
                    lopHoc = new LopHoc
                    {
                        MaHV = y.MaHV,
                        MaGS = y.MaGS,
                        MonHoc = mon.TenMon,
                        MaMon = mon.MaMon,   // quyết định 6: ghi thêm khóa ngoại MaMon (v7)
                        TenLop = tenLop,
                        CapDoDauVao = capDauVao,
                        HocPhi = y.HocPhi,
                        ThoiGianBatDau = y.BatDau,
                        ThoiGianKetThuc = y.KetThuc,
                        CaHoc = y.CaHoc,
                        ThuHoc = thuHoc
                    };
                    if (lopHoc.DemSoBuoi() == 0)
                        throw new ArgumentException("Khoảng ngày đã chọn không có buổi nào rơi vào các thứ học.");
                    lopHoc.TrangThai = lopHoc.TrangThaiTheoThoiGian ?? Constants.LopHocStatus.ChuaBatDau;

                    _db.LopHocs.Add(lopHoc);
                    _db.SaveChanges();

                    // 6. Đồng bộ trạng thái gia sư
                    GiaSuService.DongBoTrangThai(_db, y.MaGS);
                    _db.SaveChanges();

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            return lopHoc;
        }

        /// <summary>
        /// Lấy khóa ứng dụng SQL 'GhepLop' (Exclusive, gắn với transaction hiện tại, tự nhả khi commit/rollback).
        /// Hai yêu cầu ghép/sửa lớp xếp hàng chờ nhau nên kiểm trùng lịch luôn thấy lớp của yêu cầu trước.
        /// Phải gọi sau khi đã mở transaction.
        /// </summary>
        private void LayKhoaGhepLop()
        {
            var ketQua = new Microsoft.Data.SqlClient.SqlParameter("@kq", System.Data.SqlDbType.Int)
            { Direction = System.Data.ParameterDirection.Output };
            _db.Database.ExecuteSqlRaw(
                "EXEC @kq = sp_getapplock @Resource = N'GhepLop', @LockMode = N'Exclusive', " +
                "@LockOwner = N'Transaction', @LockTimeout = 15000", ketQua);
            if ((int)ketQua.Value < 0)
                throw new InvalidOperationException("Hệ thống đang xử lý ghép lớp khác, vui lòng thử lại sau giây lát.");
        }

        /// <summary>Đổi tên lớp (không trùng, không rỗng)</summary>
        public void DoiTenLop(int maLop, string ten)
        {
            _nguoiDung.YeuCauQuanLy();
            ten = (ten ?? "").Trim();
            if (ten.Length == 0) throw new ArgumentException("Nhập tên lớp.");
            if (ten.Length > 150) throw new ArgumentException("Tên lớp tối đa 150 ký tự.");
            var lop = _db.LopHocs.Find(maLop);
            if (lop == null) throw new InvalidOperationException("Không tìm thấy lớp học.");
            if (TrungTenLop(ten, maLop)) throw new ArgumentException($"Đã có lớp tên \"{ten}\".");
            lop.TenLop = ten;
            _db.SaveChanges();
        }

        /// <summary>
        /// Sửa lớp đã ghép: tên lớp, học phí, ngày kết thúc (gia hạn / rút ngắn), thứ học, đổi gia sư.
        ///  - Không sửa lớp đã hủy.
        ///  - Học phí mới không nhỏ hơn số đã đóng.
        ///  - Ngày kết thúc không trước buổi đã điểm danh gần nhất, không trước ngày bắt đầu.
        ///  - Đổi gia sư / thứ / ngày kết thúc: kiểm tra trùng lịch như khi ghép lớp.
        /// </summary>
        public LopHoc SuaLop(int maLop, Dto.SuaLopYeuCau y)
        {
            _nguoiDung.YeuCauQuanLy();
            var lop = QueryDayDu().FirstOrDefault(l => l.MaLop == maLop);
            if (lop == null) throw new InvalidOperationException("Không tìm thấy lớp học.");
            if (lop.TrangThai == Constants.LopHocStatus.DaHuy) throw new InvalidOperationException("Lớp đã hủy, không sửa được.");
            if (!lop.ThoiGianBatDau.HasValue || !lop.MaHV.HasValue) throw new InvalidOperationException("Lớp thiếu thông tin, không sửa được.");

            string loi;
            if (!LopHocValidator.Validate(lop.MaHV, y.MaGS, lop.MonHoc, y.HocPhi, out loi)) throw new ArgumentException(loi);

            decimal daDong = _db.ThanhToans.Where(t => t.MaLop == maLop).Sum(t => (decimal?)t.SoTien) ?? 0;
            if (y.HocPhi < daDong)
                throw new ArgumentException($"Lớp đã đóng {daDong:N0} đ, học phí mới không được nhỏ hơn số đã đóng.");

            // Giữ giờ kết thúc theo ca
            int gioBD, gioKT;
            DateTime ketThuc = Constants.CaHoc.TryGetGio(lop.CaHoc, out gioBD, out gioKT)
                ? y.KetThuc.Date.AddHours(gioKT) : y.KetThuc.Date.AddDays(1).AddTicks(-1);
            if (ketThuc <= lop.ThoiGianBatDau.Value) throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");
            var buoiCuoi = _db.BuoiHocs.Where(b => b.MaLop == maLop && b.MaBuoiGoc == null)
                .OrderByDescending(b => b.NgayHoc).Select(b => (DateTime?)b.NgayHoc).FirstOrDefault();
            if (buoiCuoi.HasValue && ketThuc.Date < buoiCuoi.Value.Date)
                throw new ArgumentException($"Lớp đã điểm danh đến {buoiCuoi:dd/MM/yyyy}, ngày kết thúc không được trước ngày đó.");

            string tenMoi = y.TenLop == null ? null : y.TenLop.Trim();
            if (tenMoi != null && tenMoi != lop.TenLop)
            {
                if (tenMoi.Length == 0) throw new ArgumentException("Nhập tên lớp.");
                if (tenMoi.Length > 150) throw new ArgumentException("Tên lớp tối đa 150 ký tự.");
                if (TrungTenLop(tenMoi, maLop)) throw new ArgumentException($"Đã có lớp tên \"{tenMoi}\".");
            }

            string thuHoc = Constants.ThuHoc.Ghi(Constants.ThuHoc.Doc(y.ThuHoc));
            bool doiGiaSu = y.MaGS != lop.MaGS;
            int? gsCu = lop.MaGS;
            if (doiGiaSu)
            {
                var gs = _db.GiaSus.Find(y.MaGS);
                if (gs == null) throw new InvalidOperationException("Không tìm thấy gia sư.");
                if (gs.TrangThai == Constants.GiaSuStatus.TamNghi) throw new InvalidOperationException(Constants.Messages.GiaSuTamNghi);
            }

            // Lớp đã kết thúc mà không gia hạn / đổi lịch thì khỏi kiểm tra trùng
            bool doiLich = doiGiaSu || thuHoc != lop.ThuHoc || ketThuc != lop.ThoiGianKetThuc;

            // Review #3: kiểm trùng lịch nằm trong cùng transaction (và khóa ứng dụng) với bước ghi.
            // CRIT-01: sửa lớp + đồng bộ trạng thái (gia sư cũ/mới) cũng trong transaction này.
            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    LayKhoaGhepLop();

                    // Đã có khóa: nạp lại lớp từ CSDL (yêu cầu khác có thể vừa sửa xong khi ta chờ khóa)
                    // rồi tính lại các giá trị phụ thuộc vào trạng thái lớp, để không bỏ sót kiểm trùng lịch.
                    _db.Entry(lop).Reload();
                    if (lop.TrangThai == Constants.LopHocStatus.DaHuy) throw new InvalidOperationException("Lớp đã hủy, không sửa được.");
                    doiGiaSu = y.MaGS != lop.MaGS;
                    gsCu = lop.MaGS;
                    doiLich = doiGiaSu || thuHoc != lop.ThuHoc || ketThuc != lop.ThoiGianKetThuc;
                    if (doiGiaSu)
                    {
                        var gs = _db.GiaSus.Find(y.MaGS);
                        if (gs == null) throw new InvalidOperationException("Không tìm thấy gia sư.");
                        if (gs.TrangThai == Constants.GiaSuStatus.TamNghi) throw new InvalidOperationException(Constants.Messages.GiaSuTamNghi);
                    }

                    if (doiLich && ketThuc >= DateTime.Now)
                    {
                        // Chỉ xét phần lịch từ hôm nay trở đi (quá khứ đã xảy ra rồi)
                        DateTime tu = lop.ThoiGianBatDau.Value > DateTime.Now ? lop.ThoiGianBatDau.Value : DateTime.Today;
                        string xungDot = TimXungDot(y.MaGS, lop.MaHV.Value, null, lop.CaHoc, tu, ketThuc, thuHoc, maLop);
                        if (xungDot != null) throw new InvalidOperationException(xungDot);
                    }

                    // Thử trước trên bản sao: không còn buổi nào thì báo lỗi mà chưa đụng tới lớp thật
                    if (new LopHoc { ThoiGianBatDau = lop.ThoiGianBatDau, ThoiGianKetThuc = ketThuc, ThuHoc = thuHoc }.DemSoBuoi() == 0)
                        throw new ArgumentException("Khoảng ngày của lớp không có buổi nào rơi vào các thứ học.");

                    if (tenMoi != null) lop.TenLop = tenMoi;
                    lop.HocPhi = y.HocPhi;
                    lop.ThoiGianKetThuc = ketThuc;
                    lop.ThuHoc = thuHoc;
                    lop.MaGS = y.MaGS;
                    lop.TrangThai = lop.TrangThaiTheoThoiGian;   // gia hạn lớp đã kết thúc => học tiếp
                    _db.SaveChanges();

                    foreach (int ma in new[] { gsCu, (int?)y.MaGS }.Where(x => x.HasValue).Select(x => x.Value).Distinct())
                        GiaSuService.DongBoTrangThai(_db, ma);
                    _db.SaveChanges();

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            return lop;
        }

        /// <summary>
        /// Danh sách lớp cho trang Lớp học: thông tin lớp + đánh giá
        /// (xếp loại theo điểm TB với môn chính, cấp độ hiện tại với môn năng khiếu).
        /// Chỉ 3 truy vấn: lớp (kèm gia sư, học viên), buổi có điểm / bài thi, danh mục môn.
        /// </summary>
        public List<Dto.LopHocDong> GetDanhSachLop(int? maGS = null)
        {
            var q = QueryDayDu();
            if (maGS.HasValue) q = q.Where(l => l.MaGS == maGS.Value);
            var lops = q.ToList();

            var maLops = lops.Select(l => l.MaLop).ToList();
            var buoi = _db.BuoiHocs
                .Where(b => maLops.Contains(b.MaLop) && (b.Diem != null || b.ThiLenCap))
                .ToList()
                .ToLookup(b => b.MaLop);
            var danhMuc = _db.MonHocs.ToList()
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
                    ThuHoc = Constants.ThuHoc.MoTa(l.ThuHoc),
                    ThuHocGoc = l.ThuHoc,
                    SoBuoi = l.DemSoBuoi(),
                    BatDau = l.ThoiGianBatDau,
                    KetThuc = l.ThoiGianKetThuc,
                    TrangThai = l.TrangThaiTheoThoiGian,
                    HocPhi = l.HocPhi ?? 0,
                    DiemTB = tb,
                    DanhGia = nk ? "Cấp " + tt.CapHienTai : DanhGia.XepLoai(tb),
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
        /// <param name="thuHoc">Thứ học "2,4,6"; null = hằng ngày. Hai lớp cùng ca chỉ trùng khi có ngày cùng học.</param>
        /// <param name="boQuaMaLop">Lớp đang sửa (không tự so với chính nó)</param>
        public string TimXungDot(int maGS, int maHV, string monHoc, string caHoc,
            DateTime batDau, DateTime ketThuc, string thuHoc = null, int? boQuaMaLop = null)
        {
            var lopMoi = new LopHoc
            {
                MaGS = maGS,
                MaHV = maHV,
                CaHoc = caHoc,
                ThuHoc = thuHoc,
                ThoiGianBatDau = batDau,
                ThoiGianKetThuc = ketThuc
            };

            var lopLienQuan = GetLopHoatDongCuaGiaSuHoacHocVien(maGS, maHV)
                .Where(l => l.MaLop != (boQuaMaLop ?? 0)).ToList();

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

            // HIGH-01: kiểm tra thêm các buổi học bù đã xếp lịch (bảng BuoiHoc) mà TimXungDot trước đây bỏ sót.
            // Buổi bù là một ngày cụ thể nên chỉ trùng khi cùng ca, cùng ngày nằm trong khoảng của lớp mới
            // và ngày đó là ngày học của lớp mới.
            var tuNgay = lopMoi.ThoiGianBatDau.Value.Date;
            var denNgay = lopMoi.ThoiGianKetThuc.Value.Date;
            var buoiBu = _db.BuoiHocs
                .Where(b => b.MaBuoiGoc != null && b.CaHoc == caHoc &&
                            b.NgayHoc >= tuNgay && b.NgayHoc <= denNgay &&
                            b.MaLop != (boQuaMaLop ?? 0))
                .Select(b => new { b.MaLop, b.NgayHoc, b.MaGSDay, MaHV = b.LopHoc.MaHV })
                .ToList()
                .Where(b => Constants.ThuHoc.CoHoc(thuHoc, b.NgayHoc))
                .ToList();

            var buGS = buoiBu.FirstOrDefault(b => b.MaGSDay == maGS);
            if (buGS != null)
                return string.Format("Gia sư đã có buổi học bù ở ca {0} ngày {1:dd/MM/yyyy} (lớp {2}).",
                    caHoc, buGS.NgayHoc, buGS.MaLop);

            var buHV = buoiBu.FirstOrDefault(b => b.MaHV == maHV);
            if (buHV != null)
                return string.Format("Học viên đã có buổi học bù ở ca {0} ngày {1:dd/MM/yyyy} (lớp {2}).",
                    caHoc, buHV.NgayHoc, buHV.MaLop);

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
            var allClasses = _db.LopHocs.Where(l => hoatDong.Contains(l.TrangThai)).ToList();
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
                GiaSuService.DongBoTrangThai(_db, maGS);

            if (updatedCount > 0)
            {
                _db.SaveChanges();
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

            // Chỉ lấy 2 cột của các lớp đang trong thời gian học, rồi bỏ lớp không học vào thứ hôm nay
            var q = _db.LopHocs.Where(l => l.TrangThai != huy && l.CaHoc != null &&
                                                l.ThoiGianBatDau < ngayMai && l.ThoiGianKetThuc >= homNay);
            if (maGS.HasValue) q = q.Where(l => l.MaGS == maGS.Value);
            return q.Select(l => new { l.CaHoc, l.ThuHoc }).ToList()
                    .Where(x => Constants.ThuHoc.CoHoc(x.ThuHoc, homNay))
                    .GroupBy(x => x.CaHoc)
                    .ToDictionary(g => g.Key, g => g.Count());
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
            _nguoiDung.YeuCauQuanLy();
            var lopHoc = _db.LopHocs.Find(maLop);
            if (lopHoc == null)
                throw new InvalidOperationException("Không tìm thấy lớp học.");

            if (lopHoc.TrangThai == Constants.LopHocStatus.DaHuy)
                throw new InvalidOperationException("Lớp này đã được hủy trước đó.");

            if (lopHoc.TrangThaiTheoThoiGian == Constants.LopHocStatus.DaKetThuc)
                throw new InvalidOperationException("Lớp đã kết thúc, không thể hủy.");

            // CRIT-01: hủy lớp + xóa buổi bù tương lai + đồng bộ trạng thái gia sư trong 1 transaction.
            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    lopHoc.TrangThai = Constants.LopHocStatus.DaHuy;
                    // Buổi học bù đã xếp nhưng chưa tới ngày thì bỏ luôn (lớp đã hủy không học bù nữa)
                    var homNay = DateTime.Today;
                    foreach (var bu in _db.BuoiHocs.Where(b => b.MaLop == maLop && b.MaBuoiGoc != null && b.NgayHoc > homNay).ToList())
                        _db.BuoiHocs.Remove(bu);
                    _db.SaveChanges();

                    if (lopHoc.MaGS.HasValue)
                    {
                        GiaSuService.DongBoTrangThai(_db, lopHoc.MaGS.Value);
                        _db.SaveChanges();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Xóa hẳn lớp học khỏi CSDL
        /// </summary>
        public void Delete(int maLop)
        {
            _nguoiDung.YeuCauQuanLy();
            var lopHoc = _db.LopHocs.Find(maLop);
            if (lopHoc == null)
                throw new InvalidOperationException("Không tìm thấy lớp học.");

            int? maGS = lopHoc.MaGS;

            // Đã có điểm danh / đóng tiền => phải giữ lịch sử, chỉ được hủy
            if (_db.BuoiHocs.Any(b => b.MaLop == maLop) || _db.ThanhToans.Any(t => t.MaLop == maLop))
                throw new InvalidOperationException(
                    "Lớp đã có điểm danh hoặc đã đóng học phí nên không xóa hẳn được. Hãy dùng \"Hủy lớp\" để giữ lịch sử.");

            // CRIT-01: xóa lớp + đồng bộ trạng thái gia sư trong cùng một transaction.
            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.LopHocs.Remove(lopHoc);
                    _db.SaveChanges();

                    if (maGS.HasValue)
                    {
                        GiaSuService.DongBoTrangThai(_db, maGS.Value);
                        _db.SaveChanges();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

    }
}
