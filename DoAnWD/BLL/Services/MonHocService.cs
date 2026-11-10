using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    /// <summary>
    /// Danh mục môn học (chỉ quản lý được thêm / sửa / xóa).
    ///  - Đổi tên môn: cập nhật luôn tên môn ở lớp học và chuyên môn gia sư đang dùng tên cũ.
    ///  - Không xóa môn đang có lớp (dùng "Ngừng dạy" để ẩn khỏi ghép lớp mới).
    ///  - Không bỏ cấp độ đang được lớp nào dùng (cấp đầu vào hoặc bài thi lên cấp).
    /// </summary>
    public class MonHocService
    {
        public List<MonHocDanhMuc> GetAll(bool kemNgungDay = true)
        {
            using (var db = new GiaSuContextDB())
            {
                var q = db.MonHocs.AsQueryable();
                if (!kemNgungDay) q = q.Where(m => !m.NgungDay);
                return q.ToList()
                    .OrderBy(m => m.LoaiMon == Constants.LoaiMon.NangKhieu)
                    .ThenBy(m => m.TenMon)
                    .ToList();
            }
        }

        /// <summary>Số lớp đang dùng mỗi môn (theo tên)</summary>
        public Dictionary<string, int> SoLopTheoMon()
        {
            using (var db = new GiaSuContextDB())
                return db.LopHocs.Where(l => l.MonHoc != null)
                    .GroupBy(l => l.MonHoc)
                    .Select(g => new { g.Key, SoLop = g.Count() })
                    .ToList()
                    .ToDictionary(x => x.Key.Trim(), x => x.SoLop, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Kiểm tra dữ liệu một môn. Trả về thông báo lỗi hoặc null.</summary>
        public static string KiemTra(string ten, string loai, IList<string> capDo)
        {
            ten = (ten ?? "").Trim();
            if (ten.Length == 0) return "Nhập tên môn học.";
            if (ten.Length > 50) return "Tên môn tối đa 50 ký tự.";
            if (!Constants.LoaiMon.TatCa.Contains(loai)) return "Chọn loại môn: Chính hoặc Năng khiếu.";
            if (loai == Constants.LoaiMon.NangKhieu)
            {
                var ds = (capDo ?? new List<string>()).Select(c => (c ?? "").Trim()).Where(c => c.Length > 0).ToList();
                if (ds.Count < 2) return "Môn năng khiếu cần ít nhất 2 cấp độ (mỗi dòng một cấp, từ thấp đến cao).";
                if (ds.Any(c => c.Length > 50)) return "Tên cấp độ tối đa 50 ký tự.";
                if (ds.Any(c => c.Contains(MonHocDanhMuc.NganCachCapDo))) return "Tên cấp độ không được chứa dấu chấm phẩy.";
                if (ds.Distinct(StringComparer.OrdinalIgnoreCase).Count() != ds.Count) return "Có cấp độ bị trùng tên.";
                if (string.Join(";", ds).Length > 500) return "Danh sách cấp độ quá dài.";
            }
            return null;
        }

        /// <summary>Thêm (maMon = null) hoặc sửa một môn. Trả về môn đã lưu.</summary>
        public MonHocDanhMuc Luu(int? maMon, string ten, string loai, IList<string> capDo, bool ngungDay)
        {
            PhienDangNhap.YeuCauQuanLy();
            string loi = KiemTra(ten, loai, capDo);
            if (loi != null) throw new ArgumentException(loi);
            ten = ten.Trim();
            var dsCap = loai == Constants.LoaiMon.NangKhieu
                ? capDo.Select(c => c.Trim()).Where(c => c.Length > 0).ToList()
                : new List<string>();

            using (var db = new GiaSuContextDB())
            {
                if (db.MonHocs.Any(m => m.TenMon == ten && m.MaMon != (maMon ?? 0)))
                    throw new ArgumentException($"Đã có môn \"{ten}\".");

                MonHocDanhMuc mon;
                if (maMon.HasValue)
                {
                    mon = db.MonHocs.FirstOrDefault(m => m.MaMon == maMon.Value);
                    if (mon == null) throw new InvalidOperationException("Môn học không còn tồn tại.");

                    // Cấp độ đang được dùng thì không bỏ được
                    var lopCuaMon = db.LopHocs.Where(l => l.MonHoc == mon.TenMon).Select(l => new { l.MaLop, l.CapDoDauVao }).ToList();
                    var maLops = lopCuaMon.Select(l => l.MaLop).ToList();
                    var capDangDung = lopCuaMon.Select(l => l.CapDoDauVao)
                        .Concat(db.BuoiHocs.Where(b => maLops.Contains(b.MaLop) && b.CapDoThi != null).Select(b => b.CapDoThi).ToList())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    var biBo = capDangDung.Where(c => !dsCap.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
                    if (biBo.Count > 0 && loai == Constants.LoaiMon.NangKhieu)
                        throw new InvalidOperationException($"Cấp độ {string.Join(", ", biBo)} đang được lớp học dùng, không bỏ được. Có thể đổi thứ tự hoặc thêm cấp mới.");

                    // Đổi tên => cập nhật lớp học + chuyên môn gia sư dùng tên cũ
                    if (!string.Equals(mon.TenMon, ten, StringComparison.Ordinal))
                    {
                        string cu = mon.TenMon;
                        foreach (var l in db.LopHocs.Where(l => l.MonHoc == cu).ToList()) l.MonHoc = ten;
                        foreach (var g in db.GiaSus.Where(g => g.ChuyenMon == cu).ToList()) g.ChuyenMon = ten;
                    }
                }
                else
                {
                    mon = new MonHocDanhMuc();
                    db.MonHocs.Add(mon);
                }

                mon.TenMon = ten;
                mon.LoaiMon = loai;
                mon.CapDo = dsCap.Count > 0 ? string.Join(MonHocDanhMuc.NganCachCapDo.ToString(), dsCap) : null;
                mon.NgungDay = ngungDay;
                db.SaveChanges();
                return mon;
            }
        }

        public void Xoa(int maMon)
        {
            PhienDangNhap.YeuCauQuanLy();
            using (var db = new GiaSuContextDB())
            {
                var mon = db.MonHocs.FirstOrDefault(m => m.MaMon == maMon);
                if (mon == null) return;
                int soLop = db.LopHocs.Count(l => l.MonHoc == mon.TenMon);
                if (soLop > 0)
                    throw new InvalidOperationException($"Môn {mon.TenMon} đang có {soLop} lớp nên không xóa được. Chọn \"Ngừng dạy\" để ẩn môn khỏi ghép lớp mới.");
                db.MonHocs.Remove(mon);
                db.SaveChanges();
            }
        }
    }

    /// <summary>Kết quả một bài thi lên cấp</summary>
    public class BaiThiLenCap
    {
        public DateTime Ngay { get; set; }
        public string CapThi { get; set; }
        public decimal? Diem { get; set; }
        public bool Dat => Diem.HasValue && Diem.Value >= Constants.LoaiMon.DiemDatThiLenCap;
        public override string ToString() =>
            $"{Ngay:dd/MM/yyyy}: thi lên {CapThi} - " + (Diem.HasValue ? $"{Diem:0.##} điểm, {(Dat ? "đạt" : "chưa đạt")}" : "chưa có điểm");
    }

    /// <summary>Cấp độ hiện tại của một lớp năng khiếu, tính từ cấp đầu vào + các bài thi lên cấp đã đạt</summary>
    public class TienTrinhCapDo
    {
        public List<string> CacCap { get; set; } = new List<string>();
        public string CapDauVao { get; set; }
        public string CapHienTai { get; set; }
        public string CapKeTiep { get; set; }
        public List<BaiThiLenCap> BaiThi { get; set; } = new List<BaiThiLenCap>();

        /// <summary>
        /// Tính tiến trình. Cấp hiện tại = cấp cao nhất trong (cấp đầu vào, các cấp đã thi đạt);
        /// không bao giờ tụt cấp khi thi trượt.
        /// </summary>
        /// <param name="denNgay">Chỉ tính bài thi trước ngày này (null = tất cả)</param>
        public static TienTrinhCapDo Tinh(IList<string> cacCap, string capDauVao, IEnumerable<BuoiHoc> buoi, DateTime? denNgay = null)
        {
            var kq = new TienTrinhCapDo { CacCap = cacCap.ToList(), CapDauVao = capDauVao };
            if (kq.CacCap.Count == 0) return kq;

            int viTri = Math.Max(0, kq.CacCap.FindIndex(c => string.Equals(c, capDauVao, StringComparison.OrdinalIgnoreCase)));
            kq.BaiThi = buoi.Where(b => b.ThiLenCap && !string.IsNullOrEmpty(b.CapDoThi) && (!denNgay.HasValue || b.NgayHoc.Date < denNgay.Value.Date))
                .OrderBy(b => b.NgayHoc)
                .Select(b => new BaiThiLenCap { Ngay = b.NgayHoc, CapThi = b.CapDoThi, Diem = b.Diem })
                .ToList();
            foreach (var t in kq.BaiThi.Where(t => t.Dat))
            {
                int i = kq.CacCap.FindIndex(c => string.Equals(c, t.CapThi, StringComparison.OrdinalIgnoreCase));
                if (i > viTri) viTri = i;
            }
            kq.CapHienTai = kq.CacCap[viTri];
            kq.CapKeTiep = viTri + 1 < kq.CacCap.Count ? kq.CacCap[viTri + 1] : null;
            return kq;
        }
    }
}
