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
                var lops = db.LopHocs
                    .Include(l => l.HocVien)
                    .Include(l => l.GiaSu)
                    .Include(l => l.ThanhToans)
                    .ToList();

                return lops
                    .Where(l => !(l.TrangThai == Constants.LopHocStatus.DaHuy && !l.ThanhToans.Any()))
                    .Select(l =>
                    {
                        decimal hp = l.HocPhi ?? 0;
                        decimal da = l.ThanhToans.Sum(t => t.SoTien);
                        return new CongNoLop
                        {
                            MaLop = l.MaLop,
                            HocVien = l.HocVien?.HoTen,
                            SDTLienHe = l.HocVien?.SDTLienHe,
                            MonHoc = l.MonHoc,
                            GiaSu = l.GiaSu?.HoTen,
                            HocPhi = hp,
                            DaDong = da,
                            ConNo = Math.Max(0, hp - da),
                            TinhTrang = TinhTrang(hp, da),
                            TrangThaiLop = l.TrangThaiTheoThoiGian
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
