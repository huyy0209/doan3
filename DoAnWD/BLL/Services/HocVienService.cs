using System;
using System.Collections.Generic;
using DoAnWD.BLL.Validators;
using DoAnWD.DAL.Repositories;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    /// <summary>
    /// Business Logic cho Học Viên (KHÔNG CÓ EMAIL)
    /// </summary>
    public class HocVienService : IDisposable
    {
        private readonly GiaSuContextDB _context;
        private readonly HocVienRepository _hocVienRepo;

        public HocVienService()
        {
            _context = new GiaSuContextDB();
            _hocVienRepo = new HocVienRepository(_context);
        }

        public IEnumerable<HocVien> GetAll()
        {
            return _hocVienRepo.GetAll();
        }

        public HocVien GetById(int id)
        {
            return _hocVienRepo.GetById(id);
        }

        /// <summary>
        /// Thêm học viên mới (BỎ EMAIL)
        /// </summary>
        public void Add(string hoTen, string sdt, string diaChi)
        {
            // Validate
            string errorMessage;
            if (!HocVienValidator.Validate(hoTen, sdt, diaChi, out errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            // Create entity
            var hocVien = new HocVien
            {
                HoTen = hoTen.Trim(),
                SDT = sdt.Trim(),
                DiaChi = string.IsNullOrWhiteSpace(diaChi) ? null : diaChi.Trim()
            };

            _hocVienRepo.Add(hocVien);
            _hocVienRepo.SaveChanges();
        }

        /// <summary>
        /// Cập nhật học viên (BỎ EMAIL)
        /// </summary>
        public void Update(int maHV, string hoTen, string sdt, string diaChi)
        {
            // Validate
            string errorMessage;
            if (!HocVienValidator.Validate(hoTen, sdt, diaChi, out errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            // Find entity
            var hocVien = _hocVienRepo.GetById(maHV);
            if (hocVien == null)
            {
                throw new Exception("Không tìm thấy học viên.");
            }

            // Update
            hocVien.HoTen = hoTen.Trim();
            hocVien.SDT = sdt.Trim();
            hocVien.DiaChi = string.IsNullOrWhiteSpace(diaChi) ? null : diaChi.Trim();

            _hocVienRepo.Update(hocVien);
            _hocVienRepo.SaveChanges();
        }

        public void Delete(int maHV)
        {
            var hocVien = _hocVienRepo.GetById(maHV);
            if (hocVien == null)
            {
                throw new Exception("Không tìm thấy học viên.");
            }

            if (_hocVienRepo.HasActiveLopHoc(maHV))
            {
                throw new InvalidOperationException(
                    $"Không thể xóa!\n\nHọc viên '{hocVien.HoTen}' đang có lớp học.\n\nVui lòng hủy các lớp học trước.");
            }

            int tongLop = _hocVienRepo.CountAllLopHoc(maHV);
            if (tongLop > 0)
            {
                throw new InvalidOperationException(
                    $"Không thể xóa!\n\nHọc viên '{hocVien.HoTen}' đã có {tongLop} lớp trong lịch sử học.");
            }

            _hocVienRepo.Remove(hocVien);
            _hocVienRepo.SaveChanges();
        }

        public IEnumerable<HocVien> Search(string keyword)
        {
            return _hocVienRepo.Search(keyword);
        }

        public int CountAll()
        {
            return _hocVienRepo.Count();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}