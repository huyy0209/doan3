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
    /// Business Logic cho Lớp Học - ✨ CẬP NHẬT CHO OPTION 2
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
        // ✨ PHƯƠNG THỨC GHÉP LỚP CŨ - GIỮ LẠI ĐỂ BACKWARD COMPATIBLE
        // ===================================================================
        /// <summary>
        /// Ghép lớp mới (không có thời gian) - Deprecated
        /// </summary>
        public void GhepLop(int maHV, int maGS, string monHoc, decimal hocPhi, string trangThai)
        {
            // Gọi overload mới với thời gian mặc định
            DateTime batDau = DateTime.Now.Date.AddDays(1); // Ngày mai
            DateTime ketThuc = batDau.AddMonths(1); // Sau 1 tháng
            string caHoc = Constants.CaHoc.Sang; // Ca mặc định

            GhepLop(maHV, maGS, monHoc, hocPhi, trangThai, batDau, ketThuc, caHoc);
        }

        // ===================================================================
        // ✨ PHƯƠNG THỨC GHÉP LỚP MỚI - CHO OPTION 2
        // ===================================================================
        /// <summary>
        /// Ghép lớp mới với thời gian và ca học
        /// </summary>
        public void GhepLop(int maHV, int maGS, string monHoc, decimal hocPhi, string trangThai,
            DateTime thoiGianBatDau, DateTime thoiGianKetThuc, string caHoc)
        {
            // ✅ Validate cơ bản
            string errorMessage;
            if (!LopHocValidator.Validate(maHV, maGS, monHoc, hocPhi, out errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            // ✅ Validate thời gian
            if (thoiGianBatDau >= thoiGianKetThuc)
            {
                throw new ArgumentException(Constants.Messages.ThoiGianKhongHopLe);
            }

            if (thoiGianBatDau < DateTime.Now.Date)
            {
                throw new ArgumentException(Constants.Messages.ThoiGianQuaKhu);
            }

            // ✅ Validate ca học
            if (string.IsNullOrWhiteSpace(caHoc))
            {
                throw new ArgumentException(Constants.Messages.VuiLongChonCaHoc);
            }

            // ✅ Kiểm tra gia sư có rảnh trong thời gian này không
            if (IsGiaSuBusy(maGS, thoiGianBatDau, thoiGianKetThuc))
            {
                var giaSu = _giaSuRepo.GetById(maGS);
                throw new InvalidOperationException(
                    $"Gia sư '{giaSu?.HoTen}' đã có lớp trong thời gian này.\n{Constants.Messages.GiaSuKhongRanh}");
            }

            // ✅ Kiểm tra trùng ca học (nếu cùng gia sư, cùng ca, cùng thời gian)
            if (IsCaHocConflict(maGS, caHoc, thoiGianBatDau, thoiGianKetThuc))
            {
                throw new InvalidOperationException(Constants.Messages.CaHocBiTrung);
            }

            // ✅ Tạo lớp học mới
            var lopHoc = new LopHoc
            {
                MaHV = maHV,
                MaGS = maGS,
                MonHoc = monHoc.Trim(),
                HocPhi = hocPhi,
                TrangThai = string.IsNullOrWhiteSpace(trangThai) ? Constants.LopHocStatus.ChuaBatDau : trangThai,
                ThoiGianBatDau = thoiGianBatDau,
                ThoiGianKetThuc = thoiGianKetThuc,
                CaHoc = caHoc
            };

            // ✅ Validate lần cuối bằng model method
            string validationError = lopHoc.ValidateThoiGian();
            if (validationError != null)
            {
                throw new ArgumentException(validationError);
            }

            // ✅ Lưu vào DB
            _lopHocRepo.Add(lopHoc);

            // ✅ Cập nhật trạng thái gia sư
            var giaSuEntity = _giaSuRepo.GetById(maGS);
            if (giaSuEntity != null)
            {
                // Nếu lớp sắp bắt đầu, đánh dấu GS là "Bận"
                if (lopHoc.TrangThaiTheoThoiGian == Constants.LopHocStatus.ChuaBatDau)
                    giaSuEntity.TrangThai = Constants.GiaSuStatus.Ban;
                else if (lopHoc.TrangThaiTheoThoiGian == Constants.LopHocStatus.DangDienRa)
                    giaSuEntity.TrangThai = Constants.GiaSuStatus.DangDay;

                _giaSuRepo.Update(giaSuEntity);
            }

            _lopHocRepo.SaveChanges();
        }

        // ===================================================================
        // ✨ PHƯƠNG THỨC TỰ ĐỘNG CẬP NHẬT TRẠNG THÁI - CORE FEATURE
        // ===================================================================
        /// <summary>
        /// Tự động cập nhật trạng thái tất cả lớp học dựa trên thời gian
        /// </summary>
        /// <returns>Số lượng lớp đã cập nhật</returns>
        public int AutoUpdateStatus()
        {
            var allClasses = _lopHocRepo.GetAll().ToList();
            int updatedCount = 0;

            foreach (var lop in allClasses)
            {
                // Sử dụng method từ Model
                if (lop.CapNhatTrangThaiTuDong())
                {
                    updatedCount++;

                    // ✅ Cập nhật trạng thái gia sư tương ứng
                    if (lop.MaGS.HasValue)
                    {
                        var giaSu = _giaSuRepo.GetById(lop.MaGS.Value);
                        if (giaSu != null)
                        {
                            if (lop.TrangThai == Constants.LopHocStatus.DangDienRa)
                                giaSu.TrangThai = Constants.GiaSuStatus.DangDay;
                            else if (lop.TrangThai == Constants.LopHocStatus.DaKetThuc)
                            {
                                // Kiểm tra xem GS còn lớp nào đang dạy không
                                _giaSuRepo.UpdateStatusBasedOnLopHoc(lop.MaGS.Value);
                            }

                            _giaSuRepo.Update(giaSu);
                        }
                    }
                }
            }

            if (updatedCount > 0)
            {
                _lopHocRepo.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"[AUTO UPDATE] Đã cập nhật {updatedCount} lớp học.");
            }

            return updatedCount;
        }

        // ===================================================================
        // ✨ HELPER METHODS - KIỂM TRA XUNG ĐỘT
        // ===================================================================

        /// <summary>
        /// Kiểm tra gia sư có bận trong khoảng thời gian này không
        /// </summary>
        private bool IsGiaSuBusy(int maGS, DateTime batDau, DateTime ketThuc)
        {
            var existingClasses = _lopHocRepo.GetAll()
                .Where(l => l.MaGS == maGS && l.DangHoatDong)
                .ToList();

            var newClass = new LopHoc
            {
                ThoiGianBatDau = batDau,
                ThoiGianKetThuc = ketThuc
            };

            return existingClasses.Any(l => l.TrungThoiGian(newClass));
        }

        /// <summary>
        /// Kiểm tra xung đột ca học
        /// </summary>
        private bool IsCaHocConflict(int maGS, string caHoc, DateTime batDau, DateTime ketThuc)
        {
            var existingClasses = _lopHocRepo.GetAll()
                .Where(l => l.MaGS == maGS &&
                            l.CaHoc == caHoc &&
                            l.DangHoatDong)
                .ToList();

            var newClass = new LopHoc
            {
                ThoiGianBatDau = batDau,
                ThoiGianKetThuc = ketThuc,
                CaHoc = caHoc
            };

            return existingClasses.Any(l => l.TrungThoiGian(newClass) && l.TrungCaHoc(newClass));
        }

        // ===================================================================
        // ✨ QUERY METHODS MỚI
        // ===================================================================

        /// <summary>
        /// Lấy các lớp đang diễn ra
        /// </summary>
        public IEnumerable<LopHoc> GetActiveLopHoc()
        {
            return _lopHocRepo.GetAll()
                .Where(l => l.TrangThai == Constants.LopHocStatus.DangDienRa)
                .ToList();
        }

        /// <summary>
        /// Lấy các lớp sắp bắt đầu (trong vòng N ngày)
        /// </summary>
        public IEnumerable<LopHoc> GetUpcomingLopHoc(int soNgay = 7)
        {
            var now = DateTime.Now;
            var future = now.AddDays(soNgay);

            return _lopHocRepo.GetAll()
                .Where(l => l.ThoiGianBatDau.HasValue &&
                            l.ThoiGianBatDau.Value >= now &&
                            l.ThoiGianBatDau.Value <= future)
                .ToList();
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
        // PHƯƠNG THỨC CŨ - GIỮ NGUYÊN
        // ===================================================================

        /// <summary>
        /// Xóa lớp học
        /// </summary>
        public void Delete(int maLop)
        {
            var lopHoc = _lopHocRepo.GetById(maLop);
            if (lopHoc == null)
            {
                throw new Exception("Không tìm thấy lớp học.");
            }

            int? maGS = lopHoc.MaGS;

            // Xóa lớp
            _lopHocRepo.Remove(lopHoc);
            _lopHocRepo.SaveChanges();

            // Cập nhật trạng thái gia sư
            if (maGS.HasValue)
            {
                _giaSuRepo.UpdateStatusBasedOnLopHoc(maGS.Value);
                _giaSuRepo.SaveChanges();
            }
        }

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
        /// ✨ Đếm lớp theo trạng thái
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