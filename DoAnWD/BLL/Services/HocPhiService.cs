using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DoAnWD.BLL.Dto;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
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

        public static string TinhTrang(decimal hocPhi, decimal daDong)
        {
            if (daDong <= 0) return ChuaDong;
            return daDong >= hocPhi ? DaDu : ConNo;
        }

        /// <summary>Công nợ từng lớp (bỏ lớp đã hủy chưa đóng đồng nào)</summary>
        public List<CongNoLop> GetCongNo()
        {
            using (var db = new GiaSuContextDB())
            {
                var huy = Constants.LopHocStatus.DaHuy;

                // Một câu SQL: chỉ lấy đúng các cột cần + tổng đã đóng (không nạp từng lần đóng)
                var dong = db.LopHocs
                    .Select(l => new
                    {
                        l.MaLop, l.MonHoc, l.HocPhi, l.TrangThai, l.ThoiGianBatDau, l.ThoiGianKetThuc,
                        HocVien = l.HocVien.HoTen, l.HocVien.SDT, l.HocVien.SDTPhuHuynh,
                        GiaSu = l.GiaSu.HoTen,
                        DaDong = l.ThanhToans.Sum(t => (decimal?)t.SoTien) ?? 0
                    })
                    .Where(x => !(x.TrangThai == huy && x.DaDong == 0))
                    .ToList();

                return dong
                    .Select(x =>
                    {
                        decimal hp = x.HocPhi ?? 0;
                        var lopTam = new LopHoc { TrangThai = x.TrangThai, ThoiGianBatDau = x.ThoiGianBatDau, ThoiGianKetThuc = x.ThoiGianKetThuc };
                        return new CongNoLop
                        {
                            MaLop = x.MaLop,
                            HocVien = x.HocVien,
                            SDTLienHe = string.IsNullOrWhiteSpace(x.SDTPhuHuynh) ? x.SDT : x.SDTPhuHuynh,
                            MonHoc = x.MonHoc,
                            GiaSu = x.GiaSu,
                            HocPhi = hp,
                            DaDong = x.DaDong,
                            ConNo = Math.Max(0, hp - x.DaDong),
                            TinhTrang = TinhTrang(hp, x.DaDong),
                            TrangThaiLop = lopTam.TrangThaiTheoThoiGian
                        };
                    })
                    .OrderByDescending(c => c.ConNo)
                    .ThenBy(c => c.HocVien)
                    .ToList();
            }
        }

        public List<ThanhToan> GetLichSu(int maLop)
        {
            using (var db = new GiaSuContextDB())
            {
                return db.ThanhToans.Where(t => t.MaLop == maLop).OrderByDescending(t => t.NgayDong).ToList();
            }
        }

        /// <summary>Ghi nhận một lần đóng học phí</summary>
        public ThanhToan ThemThanhToan(int maLop, decimal soTien, DateTime ngayDong, string hinhThuc, string ghiChu)
        {
            if (soTien <= 0)
                throw new ArgumentException("Số tiền phải lớn hơn 0.");
            if (ngayDong.Date > DateTime.Now.Date)
                throw new ArgumentException("Ngày đóng không được ở tương lai.");
            if (!string.IsNullOrEmpty(ghiChu) && ghiChu.Length > 200)
                throw new ArgumentException("Ghi chú tối đa 200 ký tự.");

            using (var db = new GiaSuContextDB())
            {
                var lop = db.LopHocs.Include(l => l.ThanhToans).FirstOrDefault(l => l.MaLop == maLop);
                if (lop == null)
                    throw new InvalidOperationException("Không tìm thấy lớp học.");

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
                db.ThanhToans.Add(tt);
                db.SaveChanges();
                return tt;
            }
        }

        public void XoaThanhToan(int maTT)
        {
            using (var db = new GiaSuContextDB())
            {
                var tt = db.ThanhToans.Find(maTT);
                if (tt == null) throw new InvalidOperationException("Không tìm thấy lần đóng tiền này.");
                db.ThanhToans.Remove(tt);
                db.SaveChanges();
            }
        }

        /// <summary>Tổng số tiền còn phải thu (lớp chưa hủy)</summary>
        public decimal TongConNo()
        {
            return GetCongNo().Where(c => c.TrangThaiLop != Constants.LopHocStatus.DaHuy).Sum(c => c.ConNo);
        }
    }
}
