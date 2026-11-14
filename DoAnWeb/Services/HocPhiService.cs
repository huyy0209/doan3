using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services.Dto;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services
{
    /// <summary>
    /// Theo dõi học phí: mỗi lớp có học phí cả khóa, phụ huynh đóng một hoặc nhiều lần.
    /// Còn nợ = học phí - tổng đã đóng. Không cho đóng vượt số còn nợ.
    /// </summary>
    public class HocPhiService
    {
        public const string DaDu = "Đã đủ";
        public const string ConNo = "Còn nợ";
        public const string ChuaDong = "Chưa đóng";
        public const string DaHuy = "Đã hủy";

        private readonly QuanLyGiaSuContext _db;
        private readonly INguoiDungHienTai _nguoiDung;

        public HocPhiService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung)
        {
            _db = db;
            _nguoiDung = nguoiDung;
        }

        // Nguồn: DoAnWD/BLL/Services/HocPhiService.cs:22-26
        public static string TinhTrang(decimal hocPhi, decimal daDong)
        {
            if (daDong <= 0) return ChuaDong;
            return daDong >= hocPhi ? DaDu : ConNo;
        }

        // Nguồn: DoAnWD/BLL/Services/HocPhiService.cs:29-73
        public List<CongNoLop> GetCongNo()
        {
            var huy = Constants.LopHocStatus.DaHuy;
            var dong = _db.LopHocs.AsNoTracking()
                .Select(l => new
                {
                    l.MaLop,
                    l.TenLop,
                    l.MonHoc,
                    l.HocPhi,
                    l.TrangThai,
                    l.ThoiGianBatDau,
                    l.ThoiGianKetThuc,
                    HocVien = l.HocVien.HoTen,
                    l.HocVien.SDT,
                    l.HocVien.SDTPhuHuynh,
                    GiaSu = l.GiaSu.HoTen,
                    DaDong = l.ThanhToans.Sum(t => (decimal?)t.SoTien) ?? 0
                })
                .Where(x => !(x.TrangThai == huy && x.DaDong == 0))
                .ToList();

            return dong.Select(x =>
                {
                    decimal hp = x.HocPhi ?? 0;
                    var lopTam = new LopHoc
                    {
                        TrangThai = x.TrangThai,
                        ThoiGianBatDau = x.ThoiGianBatDau,
                        ThoiGianKetThuc = x.ThoiGianKetThuc
                    };
                    bool laHuy = x.TrangThai == huy || lopTam.TrangThaiTheoThoiGian == huy;
                    return new CongNoLop
                    {
                        MaLop = x.MaLop,
                        TenLop = x.TenLop ?? $"{x.MonHoc} - {x.HocVien}",
                        HocVien = x.HocVien,
                        SDTLienHe = string.IsNullOrWhiteSpace(x.SDTPhuHuynh) ? x.SDT : x.SDTPhuHuynh,
                        MonHoc = x.MonHoc,
                        GiaSu = x.GiaSu,
                        HocPhi = hp,
                        DaDong = x.DaDong,
                        ConNo = laHuy ? 0 : Math.Max(0, hp - x.DaDong),
                        TinhTrang = laHuy ? DaHuy : TinhTrang(hp, x.DaDong),
                        TrangThaiLop = lopTam.TrangThaiTheoThoiGian
                    };
                })
                .OrderByDescending(c => c.ConNo)
                .ThenBy(c => c.HocVien)
                .ToList();
        }

        // Nguồn: DoAnWD/BLL/Services/HocPhiService.cs:76-81
        public List<ThanhToan> GetLichSu(int maLop)
        {
            return _db.ThanhToans.AsNoTracking().Where(t => t.MaLop == maLop)
                .OrderByDescending(t => t.NgayDong).ToList();
        }

        // Nguồn: DoAnWD/BLL/Services/HocPhiService.cs:84-121
        public ThanhToan ThemThanhToan(int maLop, decimal soTien, DateTime ngayDong, string hinhThuc, string ghiChu)
        {
            _nguoiDung.YeuCauQuanLy();
            if (soTien <= 0)
                throw new ArgumentException("Số tiền phải lớn hơn 0.");
            if (ngayDong.Date > DateTime.Now.Date)
                throw new ArgumentException("Ngày đóng không được ở tương lai.");
            if (!string.IsNullOrEmpty(ghiChu) && ghiChu.Length > 200)
                throw new ArgumentException("Ghi chú tối đa 200 ký tự.");

            var lop = _db.LopHocs.Include(l => l.ThanhToans).FirstOrDefault(l => l.MaLop == maLop);
            if (lop == null)
                throw new InvalidOperationException("Không tìm thấy lớp học.");
            if (lop.TrangThai == Constants.LopHocStatus.DaHuy)
                throw new InvalidOperationException("Không thể ghi nhận học phí cho lớp đã hủy.");

            decimal conNo = (lop.HocPhi ?? 0) - lop.ThanhToans.Sum(t => t.SoTien);
            if (conNo <= 0)
                throw new InvalidOperationException("Lớp này đã đóng đủ học phí.");
            if (soTien > conNo)
                throw new ArgumentException($"Số tiền vượt quá số còn nợ ({conNo:N0} đ).");

            var tt = new ThanhToan
            {
                MaLop = maLop,
                SoTien = soTien,
                NgayDong = ngayDong,
                HinhThuc = string.IsNullOrWhiteSpace(hinhThuc) ? null : hinhThuc.Trim(),
                GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim()
            };
            _db.ThanhToans.Add(tt);
            _db.SaveChanges();
            return tt;
        }

        // Nguồn: DoAnWD/BLL/Services/HocPhiService.cs:123-133
        public void XoaThanhToan(int maTT)
        {
            _nguoiDung.YeuCauQuanLy();
            var tt = _db.ThanhToans.Find(maTT);
            if (tt == null) throw new InvalidOperationException("Không tìm thấy lần đóng tiền này.");
            _db.ThanhToans.Remove(tt);
            _db.SaveChanges();
        }

        // Nguồn: DoAnWD/BLL/Services/HocPhiService.cs:136-140
        public decimal TongConNo()
        {
            return GetCongNo().Where(c => c.TrangThaiLop != Constants.LopHocStatus.DaHuy).Sum(c => c.ConNo);
        }
    }
}
