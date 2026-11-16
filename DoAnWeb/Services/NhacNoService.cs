using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Services.Dto;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services
{
    /// <summary>
    /// Một lớp còn nợ học phí, kèm thông tin lần đóng gần nhất để nhắc nợ.
    /// Mọi con số tiền lấy nguyên từ <see cref="CongNoLop"/> của trang Học phí –
    /// không tự tính lại công thức nợ ở đây.
    /// </summary>
    public class NhacNoDong
    {
        /// <summary>Dòng công nợ gốc (đúng số liệu trang Học phí).</summary>
        public CongNoLop CongNo { get; set; }

        public int MaLop => CongNo.MaLop;
        public string TenLop => CongNo.TenLop;
        public string HocVien => CongNo.HocVien;
        public string SDTLienHe => CongNo.SDTLienHe;
        public string MonHoc => CongNo.MonHoc;

        /// <summary>Tổng phải đóng (học phí cả khóa của lớp).</summary>
        public decimal PhaiDong => CongNo.HocPhi;
        public decimal DaDong => CongNo.DaDong;
        public decimal ConNo => CongNo.ConNo;

        /// <summary>Ngày đóng của lần gần nhất; null khi chưa đóng lần nào.</summary>
        public DateTime? LanDongGanNhat { get; set; }

        /// <summary>Số lần đã đóng tiền của lớp.</summary>
        public int SoLanDong { get; set; }

        /// <summary>Mốc để đếm số ngày: lần đóng gần nhất, chưa đóng lần nào thì ngày bắt đầu lớp.</summary>
        public DateTime? MocNgay { get; set; }

        /// <summary>Số ngày tính từ <see cref="MocNgay"/> tới hôm nay (không âm).</summary>
        public int SoNgayChuaDong { get; set; }

        /// <summary>Lớp chưa có lần đóng nào.</summary>
        public bool ChuaDongLanNao => !LanDongGanNhat.HasValue;
    }

    /// <summary>
    /// Danh sách nhắc nợ học phí: lọc lớp còn nợ từ công nợ của trang Học phí
    /// (<see cref="HocPhiService.GetCongNo"/>) rồi bổ sung lần đóng gần nhất.
    /// Chỉ quản lý được dùng.
    /// </summary>
    public class NhacNoService
    {
        public const string SapXepConNo = "conNo";
        public const string SapXepSoNgay = "soNgay";

        private readonly QuanLyGiaSuContext _db;
        private readonly HocPhiService _hocPhi;
        private readonly INguoiDungHienTai _nguoiDung;

        public NhacNoService(QuanLyGiaSuContext db, HocPhiService hocPhi, INguoiDungHienTai nguoiDung)
        {
            _db = db;
            _hocPhi = hocPhi;
            _nguoiDung = nguoiDung;
        }

        /// <summary>
        /// Các lớp còn nợ (còn nợ &gt; 0, bỏ lớp đã hủy) kèm lần đóng gần nhất.
        /// Không lọc/sắp xếp – controller gọi <see cref="Loc"/> và <see cref="SapXep"/>.
        /// </summary>
        public List<NhacNoDong> GetDanhSach()
        {
            _nguoiDung.YeuCauQuanLy();

            var congNo = _hocPhi.GetCongNo()
                .Where(c => c.ConNo > 0 && c.TrangThaiLop != Constants.LopHocStatus.DaHuy)
                .ToList();
            if (congNo.Count == 0) return new List<NhacNoDong>();

            var maLops = congNo.Select(c => c.MaLop).ToList();

            // Lần đóng gần nhất + số lần đóng của từng lớp: gom nhóm ngay trên SQL Server.
            var lanDong = _db.ThanhToans.AsNoTracking()
                .Where(t => maLops.Contains(t.MaLop))
                .GroupBy(t => t.MaLop)
                .Select(g => new { MaLop = g.Key, NgayDong = g.Max(t => t.NgayDong), SoLan = g.Count() })
                .ToList()
                .ToDictionary(x => x.MaLop);

            // Lớp chưa đóng lần nào: đếm từ ngày bắt đầu lớp để vẫn xếp được "quá N ngày".
            var batDau = _db.LopHocs.AsNoTracking()
                .Where(l => maLops.Contains(l.MaLop))
                .Select(l => new { l.MaLop, l.ThoiGianBatDau })
                .ToList()
                .ToDictionary(x => x.MaLop, x => x.ThoiGianBatDau);

            var homNay = DateTime.Today;
            var ketQua = new List<NhacNoDong>(congNo.Count);
            foreach (var c in congNo)
            {
                lanDong.TryGetValue(c.MaLop, out var lan);
                DateTime? moc = lan?.NgayDong.Date;
                if (!moc.HasValue && batDau.TryGetValue(c.MaLop, out var ngayBatDau))
                    moc = ngayBatDau?.Date;
                ketQua.Add(new NhacNoDong
                {
                    CongNo = c,
                    LanDongGanNhat = lan?.NgayDong.Date,
                    SoLanDong = lan?.SoLan ?? 0,
                    MocNgay = moc,
                    SoNgayChuaDong = moc.HasValue
                        ? Math.Max(0, (homNay - moc.Value).Days)
                        : 0
                });
            }
            return ketQua;
        }

        /// <summary>Lọc theo còn nợ tối thiểu, số ngày chưa đóng tối thiểu và môn học.</summary>
        public static List<NhacNoDong> Loc(IEnumerable<NhacNoDong> danhSach, decimal? conNoToiThieu,
            int? quaNgay, string mon)
        {
            IEnumerable<NhacNoDong> ds = danhSach ?? Enumerable.Empty<NhacNoDong>();
            if (conNoToiThieu.HasValue && conNoToiThieu.Value > 0)
                ds = ds.Where(c => c.ConNo >= conNoToiThieu.Value);
            if (quaNgay.HasValue && quaNgay.Value > 0)
                ds = ds.Where(c => c.SoNgayChuaDong >= quaNgay.Value);
            if (!string.IsNullOrWhiteSpace(mon))
                ds = ds.Where(c => string.Equals(c.MonHoc, mon, StringComparison.OrdinalIgnoreCase));
            return ds.ToList();
        }

        /// <summary>Mặc định còn nợ giảm dần; chọn "soNgay" để xếp theo số ngày chưa đóng.</summary>
        public static List<NhacNoDong> SapXep(IEnumerable<NhacNoDong> danhSach, string sapXep, bool giamDan)
        {
            IEnumerable<NhacNoDong> ds = danhSach ?? Enumerable.Empty<NhacNoDong>();
            IOrderedEnumerable<NhacNoDong> daSapXep;
            if (sapXep == SapXepSoNgay)
            {
                daSapXep = giamDan
                    ? ds.OrderByDescending(c => c.SoNgayChuaDong)
                    : ds.OrderBy(c => c.SoNgayChuaDong);
            }
            else
            {
                daSapXep = giamDan
                    ? ds.OrderByDescending(c => c.ConNo)
                    : ds.OrderBy(c => c.ConNo);
            }
            return daSapXep.ThenBy(c => c.HocVien, StringComparer.CurrentCulture).ThenBy(c => c.MaLop).ToList();
        }

        /// <summary>Các môn có trong danh sách (đã sắp xếp) để đổ vào ô lọc.</summary>
        public static List<string> DanhSachMon(IEnumerable<NhacNoDong> danhSach)
        {
            return (danhSach ?? Enumerable.Empty<NhacNoDong>())
                .Select(c => c.MonHoc)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(m => m, StringComparer.CurrentCulture)
                .ToList();
        }
    }
}
