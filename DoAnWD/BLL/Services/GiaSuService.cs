using System;
using System.Collections.Generic;
using DoAnWD.BLL.Validators;
using DoAnWD.Common;
using DoAnWD.DAL.Repositories;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    /// <summary>
    /// ✨ Business Logic cho Gia Sư - CẬP NHẬT CHO OPTION 2
    /// OOP: Single Responsibility - Chỉ xử lý business logic
    /// OOP: Encapsulation - Đóng gói context và repository
    /// </summary>
    public class GiaSuService : IDisposable
    {
        private readonly GiaSuContextDB _context;
        private readonly GiaSuRepository _giaSuRepo;

        public GiaSuService()
        {
            _context = new GiaSuContextDB();
            _giaSuRepo = new GiaSuRepository(_context);
        }

        /// <summary>
        /// Lấy tất cả gia sư
        /// </summary>
        public IEnumerable<GiaSu> GetAll()
        {
            return _giaSuRepo.GetAll();
        }

        /// <summary>
        /// Lấy gia sư theo ID
        /// </summary>
        public GiaSu GetById(int id)
        {
            return _giaSuRepo.GetById(id);
        }

        /// <summary>
        /// Lấy gia sư rảnh (để ghép lớp)
        /// </summary>
        public IEnumerable<GiaSu> GetAvailable()
        {
            return _giaSuRepo.GetAvailableGiaSu();
        }

        /// <summary>
        /// Lấy gia sư có thể nhận lớp mới (mọi gia sư trừ "Tạm nghỉ")
        /// </summary>
        public IEnumerable<GiaSu> GetForGhepLop()
        {
            return _giaSuRepo.GetGiaSuNhanLop();
        }

        /// <summary>
        /// Lấy gia sư với số lớp đang dạy
        /// </summary>
        public IEnumerable<object> GetWithLopCount()
        {
            return _giaSuRepo.GetGiaSuWithLopCount();
        }

        /// <summary>
        /// ✨ Thêm gia sư mới - LUÔN ĐẶT "RẢNH" KHI MỚI TẠO
        /// </summary>
        public void Add(string hoTen, string sdt, string chuyenMon, string trangThai)
        {
            // Validate
            string errorMessage;
            if (!GiaSuValidator.Validate(hoTen, sdt, chuyenMon, out errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            if (_giaSuRepo.ExistsSDT(sdt))
                throw new ArgumentException(Constants.Messages.SDTDaTonTai);

            // Gia sư mới chưa có lớp => "Rảnh", trừ khi quản lý chọn "Tạm nghỉ"
            var giaSu = new GiaSu
            {
                HoTen = hoTen.Trim(),
                SDT = sdt.Trim(),
                ChuyenMon = chuyenMon.Trim(),
                TrangThai = trangThai == Constants.GiaSuStatus.TamNghi
                    ? Constants.GiaSuStatus.TamNghi
                    : Constants.GiaSuStatus.Ranh
            };

            // Save
            _giaSuRepo.Add(giaSu);
            _giaSuRepo.SaveChanges();
        }

        /// <summary>
        /// ✨ Cập nhật gia sư - TỰ ĐỘNG ĐIỀU CHỈNH TRẠNG THÁI
        /// </summary>
        public void Update(int maGS, string hoTen, string sdt, string chuyenMon, string trangThai)
        {
            // Validate
            string errorMessage;
            if (!GiaSuValidator.Validate(hoTen, sdt, chuyenMon, out errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            // Find entity
            var giaSu = _giaSuRepo.GetById(maGS);
            if (giaSu == null)
            {
                throw new Exception("Không tìm thấy gia sư.");
            }

            if (_giaSuRepo.ExistsSDT(sdt, maGS))
                throw new ArgumentException(Constants.Messages.SDTDaTonTai);

            // Update thông tin cơ bản
            giaSu.HoTen = hoTen.Trim();
            giaSu.SDT = sdt.Trim();
            giaSu.ChuyenMon = chuyenMon.Trim();

            // Trạng thái: quản lý chỉ chủ động chọn "Tạm nghỉ" (ngừng nhận lớp mới).
            // Các trạng thái khác (Rảnh / Bận / Đang dạy) tính tự động theo lịch dạy.
            if (trangThai == Constants.GiaSuStatus.TamNghi)
            {
                giaSu.TrangThai = Constants.GiaSuStatus.TamNghi;
            }
            else
            {
                giaSu.TrangThai = Constants.GiaSuStatus.Ranh;
                _giaSuRepo.UpdateStatusBasedOnLopHoc(maGS);
            }

            _giaSuRepo.Update(giaSu);
            _giaSuRepo.SaveChanges();
        }

        /// <summary>
        /// Xóa gia sư (kiểm tra ràng buộc)
        /// </summary>
        public void Delete(int maGS)
        {
            var giaSu = _giaSuRepo.GetById(maGS);
            if (giaSu == null)
            {
                throw new Exception("Không tìm thấy gia sư.");
            }

            // Kiểm tra có lớp học không
            if (_giaSuRepo.HasActiveLopHoc(maGS))
            {
                var lopCount = _giaSuRepo.CountActiveLopHoc(maGS);
                throw new InvalidOperationException(
                    $"Không thể xóa!\n\nGia sư '{giaSu.HoTen}' đang có {lopCount} lớp học.\n\nVui lòng xóa các lớp học trước.");
            }

            // Có lịch sử lớp (đã kết thúc/đã hủy) => không xóa để giữ dữ liệu thống kê
            int tongLop = _giaSuRepo.CountAllLopHoc(maGS);
            if (tongLop > 0)
            {
                throw new InvalidOperationException(
                    $"Không thể xóa!\n\nGia sư '{giaSu.HoTen}' đã có {tongLop} lớp trong lịch sử.\n\n" +
                    $"Hãy chuyển trạng thái sang \"{Constants.GiaSuStatus.TamNghi}\" thay vì xóa.");
            }

            _giaSuRepo.Remove(giaSu);
            _giaSuRepo.SaveChanges();
        }

        /// <summary>
        /// Tìm kiếm gia sư
        /// </summary>
        public IEnumerable<GiaSu> Search(string keyword)
        {
            return _giaSuRepo.Search(keyword);
        }

        /// <summary>
        /// Đếm tổng gia sư
        /// </summary>
        public int CountAll()
        {
            return _giaSuRepo.Count();
        }

        /// <summary>
        /// Đếm gia sư rảnh
        /// </summary>
        public int CountAvailable()
        {
            return _giaSuRepo.Count(g =>
                g.TrangThai == Constants.GiaSuStatus.Ranh ||
                g.TrangThai == Constants.GiaSuStatus.SanSang ||
                g.TrangThai == null
            );
        }

        /// <summary>
        /// ✨ Đếm gia sư bận - CẬP NHẬT CHO OPTION 2
        /// </summary>
        public int CountBusy()
        {
            return _giaSuRepo.Count(g =>
                g.TrangThai == Constants.GiaSuStatus.Ban ||
                g.TrangThai == Constants.GiaSuStatus.DangDay);
        }

        /// <summary>
        /// Đếm gia sư tạm nghỉ
        /// </summary>
        public int CountTamNghi()
        {
            return _giaSuRepo.Count(g => g.TrangThai == Constants.GiaSuStatus.TamNghi);
        }

        /// <summary>
        /// Đếm gia sư đang dạy (backward compatible)
        /// </summary>
        public int CountTeaching()
        {
            return _giaSuRepo.Count(g => g.TrangThai == Constants.GiaSuStatus.DangDay);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}