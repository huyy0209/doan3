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
    /// Điểm danh và điểm bài test 15 phút cuối giờ.
    ///  - Mỗi lớp có tối đa 1 bản ghi cho mỗi ngày (khóa MaLop + NgayHoc).
    ///  - Không phải buổi nào cũng kiểm tra: Diem = null nghĩa là không test.
    ///  - Vắng thì không có điểm.
    /// Mỗi hàm dùng DbContext riêng nên luôn đọc dữ liệu mới nhất.
    /// </summary>
    public class HocTapService
    {
        /// <summary>Xếp loại theo điểm trung bình thang 10</summary>
        public static string XepLoai(decimal? diem)
        {
            if (!diem.HasValue) return "";
            if (diem.Value >= 8m) return "Giỏi";
            if (diem.Value >= 6.5m) return "Khá";
            if (diem.Value >= 5m) return "Trung bình";
            return "Yếu";
        }

        /// <summary>
        /// Kiểm tra dữ liệu một buổi. Trả về thông báo lỗi hoặc null nếu hợp lệ.
        /// </summary>
        public static string KiemTraBuoi(DateTime ngay, string diemDanh, decimal? diem, string nhanXet,
            DateTime? lopBatDau, DateTime? lopKetThuc)
        {
            if (!Constants.DiemDanh.TatCa.Contains(diemDanh))
                return "Trạng thái điểm danh không hợp lệ.";
            if (ngay.Date > DateTime.Now.Date)
                return "Không thể điểm danh cho ngày trong tương lai.";
            if (lopBatDau.HasValue && ngay.Date < lopBatDau.Value.Date)
                return $"Lớp bắt đầu từ {lopBatDau:dd/MM/yyyy}.";
            if (lopKetThuc.HasValue && ngay.Date > lopKetThuc.Value.Date)
                return $"Lớp đã kết thúc ngày {lopKetThuc:dd/MM/yyyy}.";
            if (diem.HasValue)
            {
                if (diem.Value < 0 || diem.Value > 10)
                    return "Điểm phải từ 0 đến 10.";
                if (!Constants.DiemDanh.CoHoc(diemDanh))
                    return "Học viên vắng thì không có điểm test.";
            }
            if (!string.IsNullOrEmpty(nhanXet) && nhanXet.Length > 300)
                return "Nhận xét tối đa 300 ký tự.";
            return null;
        }

        /// <summary>Đổi chữ người dùng gõ ("8,5" / "8.5" / "") sang điểm</summary>
        public static bool TryParseDiem(string text, out decimal? diem)
        {
            diem = null;
            if (string.IsNullOrWhiteSpace(text)) return true;
            decimal d;
            string chuan = text.Trim().Replace(',', '.');
            if (!decimal.TryParse(chuan, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out d))
                return false;
            diem = Math.Round(d, 2);
            return true;
        }

        /// <summary>
        /// Các lớp có buổi học vào ngày cho trước (bỏ lớp đã hủy), kèm điểm danh đã lưu nếu có.
        /// </summary>
        /// <param name="caHoc">null = mọi ca</param>
        /// <param name="maGS">null = mọi gia sư (quản lý); có giá trị = chỉ lớp của gia sư đó</param>
        public List<DiemDanhRow> GetLopTrongNgay(DateTime ngay, string caHoc = null, int? maGS = null)
        {
            var d = ngay.Date;
            var dSau = d.AddDays(1);
            using (var db = new GiaSuContextDB())
            {
                var lops = db.LopHocs
                    .Include(l => l.GiaSu)
                    .Include(l => l.HocVien)
                    .Where(l => l.TrangThai != Constants.LopHocStatus.DaHuy &&
                                l.ThoiGianBatDau < dSau && l.ThoiGianKetThuc >= d)
                    .ToList();

                if (!string.IsNullOrEmpty(caHoc)) lops = lops.Where(l => l.CaHoc == caHoc).ToList();
                if (maGS.HasValue) lops = lops.Where(l => l.MaGS == maGS.Value).ToList();

                var maLops = lops.Select(l => l.MaLop).ToList();
                var buoi = db.BuoiHocs
                    .Where(b => b.NgayHoc == d && maLops.Contains(b.MaLop))
                    .ToList()
                    .ToDictionary(b => b.MaLop);

                return lops
                    .OrderBy(l => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, l.CaHoc))
                    .ThenBy(l => l.HocVien?.HoTen)
                    .Select(l =>
                    {
                        BuoiHoc b;
                        buoi.TryGetValue(l.MaLop, out b);
                        return new DiemDanhRow
                        {
                            MaLop = l.MaLop,
                            HocVien = l.HocVien?.HoTen,
                            MonHoc = l.MonHoc,
                            GiaSu = l.GiaSu?.HoTen,
                            CaHoc = l.CaHoc,
                            DiemDanh = b?.DiemDanh ?? Constants.DiemDanh.CoMat,
                            Diem = b?.Diem,
                            NhanXet = b?.NhanXet,
                            DaLuu = b != null
                        };
                    })
                    .ToList();
            }
        }

        /// <summary>
        /// Lưu (thêm hoặc cập nhật) điểm danh + điểm của nhiều lớp trong cùng một ngày.
        /// Trả về danh sách lỗi theo từng dòng; dòng hợp lệ vẫn được lưu.
        /// </summary>
        public List<string> LuuNgay(DateTime ngay, IEnumerable<DiemDanhRow> rows, int? chiChoGiaSu = null)
        {
            var loi = new List<string>();
            var d = ngay.Date;

            using (var db = new GiaSuContextDB())
            {
                foreach (var r in rows)
                {
                    var lop = db.LopHocs.Include(l => l.HocVien).FirstOrDefault(l => l.MaLop == r.MaLop);
                    if (lop == null) { loi.Add($"Lớp {r.MaLop}: không tồn tại."); continue; }
                    if (chiChoGiaSu.HasValue && lop.MaGS != chiChoGiaSu.Value)
                    {
                        loi.Add($"Lớp {r.MaLop}: không phải lớp của bạn.");
                        continue;
                    }

                    string err = KiemTraBuoi(d, r.DiemDanh, r.Diem, r.NhanXet, lop.ThoiGianBatDau, lop.ThoiGianKetThuc);
                    if (err != null) { loi.Add($"{lop.HocVien?.HoTen} ({lop.MonHoc}): {err}"); continue; }

                    var b = db.BuoiHocs.FirstOrDefault(x => x.MaLop == r.MaLop && x.NgayHoc == d);
                    if (b == null)
                    {
                        b = new BuoiHoc { MaLop = r.MaLop, NgayHoc = d };
                        db.BuoiHocs.Add(b);
                    }
                    b.DiemDanh = r.DiemDanh;
                    b.Diem = r.Diem;
                    b.NhanXet = string.IsNullOrWhiteSpace(r.NhanXet) ? null : r.NhanXet.Trim();
                }
                db.SaveChanges();
            }
            return loi;
        }

        /// <summary>Kết quả học tập của học viên theo từng lớp (mới nhất trước)</summary>
        public List<KetQuaLop> GetKetQuaHocVien(int maHV, int? maGS = null)
        {
            using (var db = new GiaSuContextDB())
            {
                var lops = db.LopHocs
                    .Include(l => l.GiaSu)
                    .Include(l => l.BuoiHocs)
                    .Where(l => l.MaHV == maHV)
                    .ToList();
                if (maGS.HasValue) lops = lops.Where(l => l.MaGS == maGS.Value).ToList();

                return lops
                    .OrderByDescending(l => l.ThoiGianBatDau)
                    .Select(TongHop)
                    .ToList();
            }
        }

        private static KetQuaLop TongHop(LopHoc l)
        {
            var buoi = l.BuoiHocs.OrderByDescending(b => b.NgayHoc).ToList();
            var diem = buoi.Where(b => b.Diem.HasValue).Select(b => b.Diem.Value).ToList();
            decimal? tb = diem.Count > 0 ? Math.Round(diem.Average(), 2) : (decimal?)null;

            return new KetQuaLop
            {
                MaLop = l.MaLop,
                MonHoc = l.MonHoc,
                GiaSu = l.GiaSu?.HoTen,
                CaHoc = l.CaHoc,
                BatDau = l.ThoiGianBatDau,
                KetThuc = l.ThoiGianKetThuc,
                TrangThai = l.TrangThaiTheoThoiGian,
                SoBuoi = buoi.Count,
                SoBuoiCoMat = buoi.Count(b => Constants.DiemDanh.CoHoc(b.DiemDanh)),
                SoBuoiVang = buoi.Count(b => !Constants.DiemDanh.CoHoc(b.DiemDanh)),
                SoBaiTest = diem.Count,
                DiemTrungBinh = tb,
                XepLoai = XepLoai(tb),
                Buoi = buoi.Select(b => new BuoiHocRow
                {
                    NgayHoc = b.NgayHoc,
                    MonHoc = l.MonHoc,
                    DiemDanh = b.DiemDanh,
                    Diem = b.Diem,
                    NhanXet = b.NhanXet
                }).ToList()
            };
        }

        /// <summary>Học viên có lớp (để chọn xem kết quả); gia sư chỉ thấy học viên của mình</summary>
        public List<HocVien> GetHocVienCoLop(int? maGS = null)
        {
            using (var db = new GiaSuContextDB())
            {
                var q = db.LopHocs.Where(l => l.MaHV != null);
                if (maGS.HasValue) q = q.Where(l => l.MaGS == maGS.Value);
                var ids = q.Select(l => l.MaHV.Value).Distinct().ToList();
                return db.HocViens.Where(h => ids.Contains(h.MaHV)).OrderBy(h => h.HoTen).ToList();
            }
        }
    }
}
