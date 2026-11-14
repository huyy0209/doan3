using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services
{
    /// <summary>
    /// Danh mục môn học (chỉ quản lý được thêm / sửa / xóa).
    /// Đổi tên môn sẽ cập nhật lớp học và chuyên môn gia sư đang dùng tên cũ.
    /// Môn đang có lớp không thể xóa; cấp độ đang dùng trong lớp không thể bỏ.
    /// </summary>
    public class MonHocService
    {
        private readonly QuanLyGiaSuContext _db;
        private readonly INguoiDungHienTai _nguoiDung;

        public MonHocService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung)
        {
            _db = db;
            _nguoiDung = nguoiDung;
        }

        // Nguồn: DoAnWD/BLL/Services/MonHocService.cs:18-30
        public List<MonHocDanhMuc> GetAll(bool kemNgungDay = true)
        {
            var q = _db.MonHocs.AsNoTracking().AsQueryable();
            if (!kemNgungDay) q = q.Where(m => !m.NgungDay);
            return q.ToList()
                .OrderBy(m => m.LoaiMon == Constants.LoaiMon.NangKhieu)
                .ThenBy(m => m.TenMon)
                .ToList();
        }

        // Nguồn: DoAnWD/BLL/Services/MonHocService.cs:33-42
        public Dictionary<string, int> SoLopTheoMon()
        {
            return _db.LopHocs.AsNoTracking().Where(l => l.MonHoc != null)
                .GroupBy(l => l.MonHoc)
                .Select(g => new { g.Key, SoLop = g.Count() })
                .ToList()
                .ToDictionary(x => x.Key.Trim(), x => x.SoLop, StringComparer.OrdinalIgnoreCase);
        }

        // Nguồn: DoAnWD/BLL/Services/MonHocService.cs:45-62
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

        // Nguồn: DoAnWD/BLL/Services/MonHocService.cs:65-114
        public MonHocDanhMuc Luu(int? maMon, string ten, string loai, IList<string> capDo, bool ngungDay)
        {
            _nguoiDung.YeuCauQuanLy();
            string loi = KiemTra(ten, loai, capDo);
            if (loi != null) throw new ArgumentException(loi);
            ten = ten.Trim();
            var dsCap = loai == Constants.LoaiMon.NangKhieu
                ? capDo.Select(c => c.Trim()).Where(c => c.Length > 0).ToList()
                : new List<string>();

            if (_db.MonHocs.Any(m => m.TenMon == ten && m.MaMon != (maMon ?? 0)))
                throw new ArgumentException($"Đã có môn \"{ten}\".");

            MonHocDanhMuc mon;
            if (maMon.HasValue)
            {
                mon = _db.MonHocs.FirstOrDefault(m => m.MaMon == maMon.Value);
                if (mon == null) throw new InvalidOperationException("Môn học không còn tồn tại.");

                var lopCuaMon = _db.LopHocs.AsNoTracking().Where(l => l.MonHoc == mon.TenMon)
                    .Select(l => new { l.MaLop, l.CapDoDauVao }).ToList();
                var maLops = lopCuaMon.Select(l => l.MaLop).ToList();
                var capDangDung = lopCuaMon.Select(l => l.CapDoDauVao)
                    .Concat(_db.BuoiHocs.AsNoTracking()
                        .Where(b => maLops.Contains(b.MaLop) && b.CapDoThi != null)
                        .Select(b => b.CapDoThi).ToList())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var biBo = capDangDung.Where(c => !dsCap.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
                if (biBo.Count > 0 && loai == Constants.LoaiMon.NangKhieu)
                    throw new InvalidOperationException($"Cấp độ {string.Join(", ", biBo)} đang được lớp học dùng, không bỏ được. Có thể đổi thứ tự hoặc thêm cấp mới.");

                if (!string.Equals(mon.TenMon, ten, StringComparison.Ordinal))
                {
                    string cu = mon.TenMon;
                    foreach (var l in _db.LopHocs.Where(l => l.MonHoc == cu).ToList()) l.MonHoc = ten;
                    foreach (var g in _db.GiaSus.Where(g => g.ChuyenMon == cu).ToList()) g.ChuyenMon = ten;
                }
            }
            else
            {
                mon = new MonHocDanhMuc();
                _db.MonHocs.Add(mon);
            }

            mon.TenMon = ten;
            mon.LoaiMon = loai;
            mon.CapDo = dsCap.Count > 0 ? string.Join(MonHocDanhMuc.NganCachCapDo.ToString(), dsCap) : null;
            mon.NgungDay = ngungDay;
            _db.SaveChanges();
            return mon;
        }

        // Nguồn: DoAnWD/BLL/Services/MonHocService.cs:117-133
        public void Xoa(int maMon)
        {
            _nguoiDung.YeuCauQuanLy();
            var mon = _db.MonHocs.FirstOrDefault(m => m.MaMon == maMon);
            if (mon == null) return;
            int soLop = _db.LopHocs.Count(l => l.MonHoc == mon.TenMon);
            if (soLop > 0)
                throw new InvalidOperationException($"Môn {mon.TenMon} đang có {soLop} lớp nên không xóa được. Chọn \"Ngừng dạy\" để ẩn môn khỏi ghép lớp mới.");
            _db.MonHocs.Remove(mon);
            _db.SaveChanges();
        }
    }
}
