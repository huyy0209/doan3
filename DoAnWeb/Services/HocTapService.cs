using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services.Dto;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services;


/// <summary>
/// Điểm danh, điểm bài test 15 phút cuối giờ và học bù.
/// Buổi thường: mỗi lớp tối đa 1 bản ghi / ngày, ca + gia sư theo lớp.
/// Buổi vắng giữ nguyên trạng thái và trỏ tới buổi bù; buổi bù vẫn tính cho lớp gốc.
/// </summary>
public class HocTapService
{
    private readonly QuanLyGiaSuContext _db;
    private readonly INguoiDungHienTai _nguoiDung;

    public HocTapService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung)
    {
        _db = db;
        _nguoiDung = nguoiDung;
    }

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:22-32
    public void KiemTraCauTrucCSDL()
    {
        _db.BuoiHocs.AsNoTracking().Select(b => new { b.MaBuoiGoc, b.CaHoc, b.MaGSDay, b.ThiLenCap, b.CapDoThi }).Take(1).ToList();
        _db.LopHocs.AsNoTracking().Select(l => new { l.TenLop, l.CapDoDauVao, l.ThuHoc }).Take(1).ToList();
        _db.MonHocs.AsNoTracking().Take(1).ToList();
        _db.TaiKhoans.AsNoTracking().Select(t => new { t.SoLanSai, t.KhoaDen }).Take(1).ToList();
        _db.CaiDats.AsNoTracking().Take(1).ToList();
    }

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:35-42
    public static string XepLoai(decimal? diem) => DanhGia.XepLoai(diem);

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:47-68
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

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:71-82
    public static bool TryParseDiem(string text, out decimal? diem)
    {
        diem = null;
        if (string.IsNullOrWhiteSpace(text)) return true;
        string chuan = text.Trim().Replace(',', '.');
        if (!decimal.TryParse(chuan, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal d))
            return false;
        diem = Math.Round(d, 2);
        return true;
    }

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:94-203
    public List<DiemDanhRow> GetLopTrongNgay(DateTime ngay, string caHoc = null, int? maGS = null)
    {
        var d = ngay.Date;
        var dSau = d.AddDays(1);
        var huy = Constants.LopHocStatus.DaHuy;
        var q = _db.LopHocs.AsNoTracking()
            .Include(l => l.GiaSu)
            .Include(l => l.HocVien)
            .Where(l => l.TrangThai != huy && l.ThoiGianBatDau < dSau && l.ThoiGianKetThuc >= d);
        if (!string.IsNullOrEmpty(caHoc)) q = q.Where(l => l.CaHoc == caHoc);
        if (maGS.HasValue) q = q.Where(l => l.MaGS == maGS.Value);
        var lops = q.ToList().Where(l => Constants.ThuHoc.CoHoc(l.ThuHoc, d)).ToList();

        var maLops = lops.Select(l => l.MaLop).ToList();
        var nangKhieu = MonNangKhieu(_db);
        var buoi = _db.BuoiHocs.AsNoTracking()
            .Where(b => b.NgayHoc == d && b.MaBuoiGoc == null && maLops.Contains(b.MaLop))
            .ToList()
            .ToDictionary(b => b.MaLop);
        var maBuoiHomNay = buoi.Values.Select(b => b.MaBuoi).ToList();
        var buCuaHomNay = _db.BuoiHocs.AsNoTracking()
            .Where(b => b.MaBuoiGoc != null && maBuoiHomNay.Contains(b.MaBuoiGoc.Value))
            .ToList()
            .ToDictionary(b => b.MaBuoiGoc.Value);

        var ketQua = lops.Select(l =>
        {
            buoi.TryGetValue(l.MaLop, out var b);
            BuoiHoc bu = null;
            if (b != null) buCuaHomNay.TryGetValue(b.MaBuoi, out bu);
            return new DiemDanhRow
            {
                MaLop = l.MaLop,
                TenLop = l.TenHienThi,
                LaNangKhieu = l.MonHoc != null && nangKhieu.Contains(l.MonHoc.Trim()),
                ThiLenCap = b?.ThiLenCap ?? false,
                CapDoThi = b?.CapDoThi,
                MaBuoi = b?.MaBuoi,
                MaHV = l.MaHV ?? 0,
                HocVien = l.HocVien?.HoTen,
                MonHoc = l.MonHoc,
                GiaSu = l.GiaSu?.HoTen,
                CaHoc = l.CaHoc,
                DiemDanh = b?.DiemDanh ?? Constants.DiemDanh.CoMat,
                Diem = b?.Diem,
                NhanXet = b?.NhanXet,
                DaLuu = b != null,
                DaXepBu = bu != null,
                GhiChu = bu == null ? null : $"Đã xếp bù {bu.NgayHoc:dd/MM}, {Constants.CaHoc.TenNgan(bu.CaHoc)}"
            };
        }).ToList();

        var qBu = _db.BuoiHocs.AsNoTracking().Where(b => b.NgayHoc == d && b.MaBuoiGoc != null);
        if (!string.IsNullOrEmpty(caHoc)) qBu = qBu.Where(b => b.CaHoc == caHoc);
        if (maGS.HasValue) qBu = qBu.Where(b => b.MaGSDay == maGS.Value);
        var dsBu = qBu.ToList();
        if (dsBu.Count > 0)
        {
            var lopIds = dsBu.Select(b => b.MaLop).Distinct().ToList();
            var lopBu = _db.LopHocs.AsNoTracking().Include(l => l.HocVien)
                .Where(l => lopIds.Contains(l.MaLop)).ToList().ToDictionary(l => l.MaLop);
            var gsIds = dsBu.Select(b => b.MaGSDay ?? 0).Distinct().ToList();
            var tenGS = _db.GiaSus.AsNoTracking().Where(g => gsIds.Contains(g.MaGS))
                .ToList().ToDictionary(g => g.MaGS, g => g.HoTen);
            var gocIds = dsBu.Select(b => b.MaBuoiGoc.Value).ToList();
            var ngayGoc = _db.BuoiHocs.AsNoTracking().Where(b => gocIds.Contains(b.MaBuoi))
                .ToList().ToDictionary(b => b.MaBuoi, b => b.NgayHoc);

            foreach (var b in dsBu)
            {
                if (!lopBu.TryGetValue(b.MaLop, out var l) || l.TrangThai == huy) continue;
                tenGS.TryGetValue(b.MaGSDay ?? 0, out var ten);
                // MED-02: buổi gốc có thể đã bị xóa => không hiển thị nhầm ngày 01/01.
                DateTime goc;
                string ngayGocText = ngayGoc.TryGetValue(b.MaBuoiGoc.Value, out goc)
                    ? goc.ToString("dd/MM") : "không rõ";
                ketQua.Add(new DiemDanhRow
                {
                    MaLop = b.MaLop,
                    TenLop = l.TenHienThi,
                    LaNangKhieu = l.MonHoc != null && nangKhieu.Contains(l.MonHoc.Trim()),
                    ThiLenCap = b.ThiLenCap,
                    CapDoThi = b.CapDoThi,
                    MaBuoi = b.MaBuoi,
                    MaHV = l.MaHV ?? 0,
                    HocVien = l.HocVien?.HoTen,
                    MonHoc = l.MonHoc,
                    GiaSu = ten,
                    CaHoc = b.CaHoc,
                    DiemDanh = b.DiemDanh,
                    Diem = b.Diem,
                    NhanXet = b.NhanXet,
                    DaLuu = true,
                    LaHocBu = true,
                    GhiChu = $"Học bù buổi {ngayGocText}"
                });
            }
        }

        return ketQua
            .OrderBy(r => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, r.CaHoc))
            .ThenBy(r => r.LaHocBu)
            .ThenBy(r => r.HocVien)
            .ToList();
    }

    /// <summary>Lấy lịch điểm danh cho nhiều ngày bằng một lượt đọc lớp trong khoảng ngày.</summary>
    public Dictionary<DateTime, List<DiemDanhRow>> GetLopTrongKhoangNgay(DateTime tuNgay, DateTime denNgay,
        int? maGS = null)
    {
        var batDau = tuNgay.Date;
        var ngayCuoi = denNgay.Date;
        if (ngayCuoi < batDau) throw new ArgumentException("Khoảng ngày không hợp lệ.");

        var cacNgay = Enumerable.Range(0, (ngayCuoi - batDau).Days + 1)
            .Select(i => batDau.AddDays(i)).ToList();
        var ketThuc = ngayCuoi.AddDays(1);
        var huy = Constants.LopHocStatus.DaHuy;
        var qLop = _db.LopHocs.AsNoTracking()
            .Include(l => l.GiaSu)
            .Include(l => l.HocVien)
            .Where(l => l.TrangThai != huy && l.ThoiGianBatDau < ketThuc && l.ThoiGianKetThuc >= batDau);
        if (maGS.HasValue) qLop = qLop.Where(l => l.MaGS == maGS.Value);
        var lops = qLop.ToList();
        var maLops = lops.Select(l => l.MaLop).ToArray();

        var buoiThuong = _db.BuoiHocs.AsNoTracking()
            .Where(b => b.NgayHoc >= batDau && b.NgayHoc < ketThuc && b.MaBuoiGoc == null && maLops.Contains(b.MaLop))
            .ToList()
            .GroupBy(b => b.NgayHoc.Date)
            .ToDictionary(g => g.Key, g => g.ToDictionary(b => b.MaLop));

        var maBuoiGoc = buoiThuong.Values.SelectMany(x => x.Values).Select(b => b.MaBuoi).ToArray();
        var buDaXep = maBuoiGoc.Length == 0
            ? new Dictionary<int, BuoiHoc>()
            : _db.BuoiHocs.AsNoTracking().Where(b => b.MaBuoiGoc != null && maBuoiGoc.Contains(b.MaBuoiGoc.Value))
                .ToList().ToDictionary(b => b.MaBuoiGoc.Value);

        var qBu = _db.BuoiHocs.AsNoTracking()
            .Where(b => b.NgayHoc >= batDau && b.NgayHoc < ketThuc && b.MaBuoiGoc != null);
        if (maGS.HasValue) qBu = qBu.Where(b => b.MaGSDay == maGS.Value);
        var buoiBu = qBu.ToList().GroupBy(b => b.NgayHoc.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
        var maLopBu = buoiBu.Values.SelectMany(x => x).Select(b => b.MaLop).Distinct().ToArray();
        var lopBu = maLopBu.Length == 0
            ? new Dictionary<int, LopHoc>()
            : _db.LopHocs.AsNoTracking().Include(l => l.HocVien).Where(l => maLopBu.Contains(l.MaLop))
                .ToList().ToDictionary(l => l.MaLop);
        var maGsBu = buoiBu.Values.SelectMany(x => x).Where(b => b.MaGSDay.HasValue)
            .Select(b => b.MaGSDay.Value).Distinct().ToArray();
        var tenGsBu = maGsBu.Length == 0
            ? new Dictionary<int, string>()
            : _db.GiaSus.AsNoTracking().Where(g => maGsBu.Contains(g.MaGS))
                .ToList().ToDictionary(g => g.MaGS, g => g.HoTen);
        var maBuoiGocBu = buoiBu.Values.SelectMany(x => x).Where(b => b.MaBuoiGoc.HasValue)
            .Select(b => b.MaBuoiGoc.Value).Distinct().ToArray();
        var ngayBuoiGoc = maBuoiGocBu.Length == 0
            ? new Dictionary<int, DateTime>()
            : _db.BuoiHocs.AsNoTracking().Where(b => maBuoiGocBu.Contains(b.MaBuoi))
                .ToList().ToDictionary(b => b.MaBuoi, b => b.NgayHoc);
        var nangKhieu = MonNangKhieu(_db);
        var ketQua = cacNgay.ToDictionary(d => d, _ => new List<DiemDanhRow>());

        foreach (var ngay in cacNgay)
        {
            var dsLop = lops.Where(l => Constants.ThuHoc.CoHoc(l.ThuHoc, ngay));
            buoiThuong.TryGetValue(ngay, out var buoiTheoLop);
            foreach (var lop in dsLop)
            {
                BuoiHoc b = null;
                buoiTheoLop?.TryGetValue(lop.MaLop, out b);
                BuoiHoc bu = null;
                if (b != null) buDaXep.TryGetValue(b.MaBuoi, out bu);
                ketQua[ngay].Add(new DiemDanhRow
                {
                    MaLop = lop.MaLop,
                    TenLop = lop.TenHienThi,
                    LaNangKhieu = lop.MonHoc != null && nangKhieu.Contains(lop.MonHoc.Trim()),
                    ThiLenCap = b?.ThiLenCap ?? false,
                    CapDoThi = b?.CapDoThi,
                    MaBuoi = b?.MaBuoi,
                    MaHV = lop.MaHV ?? 0,
                    HocVien = lop.HocVien?.HoTen,
                    MonHoc = lop.MonHoc,
                    GiaSu = lop.GiaSu?.HoTen,
                    CaHoc = lop.CaHoc,
                    DiemDanh = b?.DiemDanh ?? Constants.DiemDanh.CoMat,
                    Diem = b?.Diem,
                    NhanXet = b?.NhanXet,
                    DaLuu = b != null,
                    DaXepBu = bu != null,
                    GhiChu = bu == null ? null : $"Đã xếp bù {bu.NgayHoc:dd/MM}, {Constants.CaHoc.TenNgan(bu.CaHoc)}"
                });
            }

            if (!buoiBu.TryGetValue(ngay, out var dsBu)) continue;
            foreach (var b in dsBu)
            {
                if (!lopBu.TryGetValue(b.MaLop, out var lop) || lop.TrangThai == huy) continue;
                tenGsBu.TryGetValue(b.MaGSDay ?? 0, out var tenGs);
                string ngayGocText = ngayBuoiGoc.TryGetValue(b.MaBuoiGoc.Value, out var ngayGoc)
                    ? ngayGoc.ToString("dd/MM") : "không rõ";
                ketQua[ngay].Add(new DiemDanhRow
                {
                    MaLop = b.MaLop,
                    TenLop = lop.TenHienThi,
                    LaNangKhieu = lop.MonHoc != null && nangKhieu.Contains(lop.MonHoc.Trim()),
                    ThiLenCap = b.ThiLenCap,
                    CapDoThi = b.CapDoThi,
                    MaBuoi = b.MaBuoi,
                    MaHV = lop.MaHV ?? 0,
                    HocVien = lop.HocVien?.HoTen,
                    MonHoc = lop.MonHoc,
                    GiaSu = tenGs,
                    CaHoc = b.CaHoc,
                    DiemDanh = b.DiemDanh,
                    Diem = b.Diem,
                    NhanXet = b.NhanXet,
                    DaLuu = true,
                    LaHocBu = true,
                    GhiChu = $"Học bù buổi {ngayGocText}"
                });
            }
        }

        foreach (var ngay in cacNgay)
            ketQua[ngay] = ketQua[ngay]
                .OrderBy(r => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, r.CaHoc))
                .ThenBy(r => r.LaHocBu)
                .ThenBy(r => r.HocVien)
                .ToList();
        return ketQua;
    }

    private static HashSet<string> MonNangKhieu(QuanLyGiaSuContext db)
    {
        var nk = Constants.LoaiMon.NangKhieu;
        return new HashSet<string>(db.MonHocs.AsNoTracking().Where(m => m.LoaiMon == nk)
            .Select(m => m.TenMon).ToList(), StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, MonHocDanhMuc> DanhMucMon(QuanLyGiaSuContext db) =>
        db.MonHocs.AsNoTracking().ToList()
            .GroupBy(m => m.TenMon.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:222-240
    public List<string> LuuNgay(DateTime ngay, IEnumerable<DiemDanhRow> rows, int? chiChoGiaSu = null)
    {
        var d = ngay.Date;
        return LuuCacBuoi(rows.Select(r => new BuoiHocRow
        {
            MaBuoi = r.MaBuoi ?? 0,
            MaLop = r.MaLop,
            NgayHoc = d,
            DiemDanh = r.DiemDanh,
            Diem = r.Diem,
            NhanXet = r.NhanXet,
            ThiLenCap = r.ThiLenCap,
            CapDoThi = r.CapDoThi
        }), chiChoGiaSu);
    }

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:243-336
    public List<string> LuuCacBuoi(IEnumerable<BuoiHocRow> rows, int? chiChoGiaSu = null)
    {
        // HIGH-02: không tin tham số do tầng UI truyền xuống, tự đọc giới hạn từ người dùng hiện tại.
        int? gioiHan = _nguoiDung.GioiHanGiaSu;
        var loi = new List<string>();
        var danhMuc = DanhMucMon(_db);
        foreach (var r in rows)
        {
            var lop = _db.LopHocs.Include(l => l.HocVien).FirstOrDefault(l => l.MaLop == r.MaLop);
            if (lop == null) { loi.Add($"Lớp {r.MaLop}: không tồn tại."); continue; }
            string ten = $"{lop.HocVien?.HoTen} ({lop.MonHoc}, {r.NgayHoc:dd/MM})";

            BuoiHoc b = null;
            if (r.MaBuoi > 0)
            {
                b = _db.BuoiHocs.FirstOrDefault(x => x.MaBuoi == r.MaBuoi);
                if (b == null || b.MaLop != r.MaLop) { loi.Add($"{ten}: buổi học không còn tồn tại."); continue; }
            }
            else
            {
                var d = r.NgayHoc.Date;
                b = _db.BuoiHocs.FirstOrDefault(x => x.MaLop == r.MaLop && x.NgayHoc == d && x.MaBuoiGoc == null);
            }
            bool laBu = b != null && b.MaBuoiGoc.HasValue;

            if (gioiHan.HasValue && lop.MaGS != gioiHan.Value && !(laBu && b.MaGSDay == gioiHan.Value))
            {
                loi.Add($"{ten}: không phải lớp của bạn.");
                continue;
            }

            string err = laBu
                ? KiemTraBuoi(b.NgayHoc, r.DiemDanh, r.Diem, r.NhanXet, null, null)
                : KiemTraBuoi(r.NgayHoc, r.DiemDanh, r.Diem, r.NhanXet, lop.ThoiGianBatDau, lop.ThoiGianKetThuc);
            if (err != null) { loi.Add($"{ten}: {err}"); continue; }

            if (b != null && !laBu && Constants.DiemDanh.CoHoc(r.DiemDanh))
            {
                int maGoc = b.MaBuoi;
                if (_db.BuoiHocs.AsNoTracking().Any(x => x.MaBuoiGoc == maGoc))
                {
                    loi.Add($"{ten}: buổi này đã xếp học bù. Hủy học bù trước nếu muốn đổi thành có mặt.");
                    continue;
                }
            }

            string capThi = null;
            if (r.ThiLenCap)
            {
                danhMuc.TryGetValue((lop.MonHoc ?? "").Trim(), out var mon);
                if (mon == null || !mon.LaNangKhieu || mon.DanhSachCapDo.Count == 0)
                { loi.Add($"{ten}: chỉ môn năng khiếu mới có bài thi lên cấp."); continue; }
                if (!r.Diem.HasValue)
                { loi.Add($"{ten}: bài thi lên cấp cần có điểm."); continue; }

                int maBuoiNay = b?.MaBuoi ?? 0;
                var buoiKhac = _db.BuoiHocs.AsNoTracking()
                    .Where(x => x.MaLop == lop.MaLop && x.MaBuoi != maBuoiNay).ToList();
                var tt = TienTrinhCapDo.Tinh(mon.DanhSachCapDo, lop.CapDoDauVao, buoiKhac, r.NgayHoc);
                capThi = !string.IsNullOrEmpty(r.CapDoThi) && mon.DanhSachCapDo.Contains(r.CapDoThi, StringComparer.OrdinalIgnoreCase)
                    ? r.CapDoThi
                    : b != null && b.ThiLenCap && !string.IsNullOrEmpty(b.CapDoThi) ? b.CapDoThi : tt.CapKeTiep;
                if (capThi == null)
                { loi.Add($"{ten}: học viên đã ở cấp cao nhất ({tt.CapHienTai})."); continue; }
            }

            if (b == null)
            {
                b = new BuoiHoc { MaLop = r.MaLop, NgayHoc = r.NgayHoc.Date };
                _db.BuoiHocs.Add(b);
            }
            b.DiemDanh = r.DiemDanh;
            b.Diem = r.Diem;
            b.NhanXet = string.IsNullOrWhiteSpace(r.NhanXet) ? null : r.NhanXet.Trim();
            b.ThiLenCap = r.ThiLenCap;
            b.CapDoThi = capThi;
        }
        _db.SaveChanges();
        return loi;
    }

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:339-370
    public List<KetQuaLop> GetKetQuaHocVien(int maHV, int? maGS = null)
    {
        var lops = _db.LopHocs.AsNoTracking().Include(l => l.GiaSu).Where(l => l.MaHV == maHV).ToList();
        var maLops = lops.Select(l => l.MaLop).ToList();
        var buoi = _db.BuoiHocs.AsNoTracking().Where(b => maLops.Contains(b.MaLop)).ToList();

        if (maGS.HasValue)
        {
            var lopCoBuMinhDay = buoi.Where(b => b.MaGSDay == maGS.Value).Select(b => b.MaLop).ToList();
            lops = lops.Where(l => l.MaGS == maGS.Value || lopCoBuMinhDay.Contains(l.MaLop)).ToList();
        }

        var gsIds = buoi.Where(b => b.MaGSDay.HasValue).Select(b => b.MaGSDay.Value).Distinct().ToList();
        var tenGS = _db.GiaSus.AsNoTracking().Where(g => gsIds.Contains(g.MaGS))
            .ToList().ToDictionary(g => g.MaGS, g => g.HoTen);
        var danhMuc = DanhMucMon(_db);

        return lops.OrderByDescending(l => l.ThoiGianBatDau)
            .Select(l =>
            {
                danhMuc.TryGetValue((l.MonHoc ?? "").Trim(), out var mon);
                return TongHop(l, buoi.Where(b => b.MaLop == l.MaLop).ToList(), tenGS, mon);
            })
            .ToList();
    }

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:373-452
    public static KetQuaLop TongHop(LopHoc l, List<BuoiHoc> buoiCuaLop, Dictionary<int, string> tenGS, MonHocDanhMuc mon = null)
    {
        bool nangKhieu = mon != null && mon.LaNangKhieu && mon.DanhSachCapDo.Count > 0;
        var bu = buoiCuaLop.Where(b => b.MaBuoiGoc.HasValue).ToDictionary(b => b.MaBuoiGoc.Value);
        var thuong = buoiCuaLop.Where(b => !b.MaBuoiGoc.HasValue).ToList();
        var ngayCuaBuoi = buoiCuaLop.ToDictionary(b => b.MaBuoi, b => b.NgayHoc);
        var homNay = DateTime.Today;

        int coMat = 0, vang = 0, daBu = 0;
        foreach (var b in thuong)
        {
            bu.TryGetValue(b.MaBuoi, out var bb);
            bool coBuXong = bb != null && bb.NgayHoc.Date <= homNay;
            string thucTe = coBuXong ? bb.DiemDanh : b.DiemDanh;
            if (Constants.DiemDanh.CoHoc(thucTe)) coMat++; else vang++;
            if (coBuXong && Constants.DiemDanh.CoHoc(bb.DiemDanh)) daBu++;
        }

        var diem = buoiCuaLop.Where(b => b.Diem.HasValue).Select(b => b.Diem.Value).ToList();
        decimal? tb = diem.Count > 0 ? Math.Round(diem.Average(), 2) : (decimal?)null;

        string TenGiaSu(int? ma)
        {
            return ma.HasValue && tenGS != null && tenGS.TryGetValue(ma.Value, out var t) ? t : l.GiaSu?.HoTen;
        }

        var rows = buoiCuaLop.Select(b =>
        {
            BuoiHoc bb = null;
            if (!b.MaBuoiGoc.HasValue) bu.TryGetValue(b.MaBuoi, out bb);
            DateTime ngayGoc;
            return new BuoiHocRow
            {
                MaBuoi = b.MaBuoi,
                MaLop = l.MaLop,
                NgayHoc = b.NgayHoc,
                MonHoc = l.MonHoc,
                CaHoc = b.MaBuoiGoc.HasValue ? b.CaHoc : l.CaHoc,
                GiaSu = b.MaBuoiGoc.HasValue ? TenGiaSu(b.MaGSDay) : l.GiaSu?.HoTen,
                MaGSDay = b.MaGSDay ?? l.MaGS ?? 0,
                DiemDanh = b.DiemDanh,
                Diem = b.Diem,
                NhanXet = b.NhanXet,
                ThiLenCap = b.ThiLenCap,
                CapDoThi = b.CapDoThi,
                LaHocBu = b.MaBuoiGoc.HasValue,
                NgayVangGoc = b.MaBuoiGoc.HasValue && ngayCuaBuoi.TryGetValue(b.MaBuoiGoc.Value, out ngayGoc) ? ngayGoc : (DateTime?)null,
                MaBuoiBu = bb?.MaBuoi,
                NgayBu = bb?.NgayHoc,
                CaBu = bb?.CaHoc
            };
        })
        .OrderByDescending(r => r.NgayHoc)
        .ThenByDescending(r => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, r.CaHoc))
        .ToList();

        return new KetQuaLop
        {
            MaLop = l.MaLop,
            TenLop = l.TenHienThi,
            LoaiMon = nangKhieu ? Constants.LoaiMon.NangKhieu : Constants.LoaiMon.Chinh,
            TienTrinh = nangKhieu ? TienTrinhCapDo.Tinh(mon.DanhSachCapDo, l.CapDoDauVao, buoiCuaLop) : null,
            MaGS = l.MaGS ?? 0,
            MonHoc = l.MonHoc,
            GiaSu = l.GiaSu?.HoTen,
            CaHoc = l.CaHoc,
            BatDau = l.ThoiGianBatDau,
            KetThuc = l.ThoiGianKetThuc,
            TrangThai = l.TrangThaiTheoThoiGian,
            SoBuoi = thuong.Count,
            SoBuoiCoMat = coMat,
            SoBuoiVang = vang,
            SoBuoiHocBu = daBu,
            SoBaiTest = diem.Count,
            DiemTrungBinh = tb,
            XepLoai = nangKhieu ? null : XepLoai(tb),
            Buoi = rows
        };
    }

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:455-469
    public List<HocVien> GetHocVienCoLop(int? maGS = null)
    {
        var q = _db.LopHocs.AsNoTracking().Where(l => l.MaHV != null);
        if (maGS.HasValue)
        {
            var lopBu = _db.BuoiHocs.AsNoTracking().Where(b => b.MaGSDay == maGS.Value)
                .Select(b => b.MaLop).Distinct().ToList();
            q = q.Where(l => l.MaGS == maGS.Value || lopBu.Contains(l.MaLop));
        }
        var ids = q.Select(l => l.MaHV.Value).Distinct().ToList();
        return _db.HocViens.AsNoTracking().Where(h => ids.Contains(h.MaHV))
            .OrderBy(h => h.HoTen).ToList();
    }

    private class LichBan
    {
        public int? MaHV, MaGS, MaLop, MaBuoi;
        public string MoTa;
    }

    private static List<LichBan> LichTrongCa(QuanLyGiaSuContext db, DateTime ngay, string ca)
    {
        var d = ngay.Date;
        var dSau = d.AddDays(1);
        var huy = Constants.LopHocStatus.DaHuy;
        var ds = db.LopHocs.AsNoTracking().Include(l => l.HocVien)
            .Where(l => l.TrangThai != huy && l.CaHoc == ca && l.ThoiGianBatDau < dSau && l.ThoiGianKetThuc >= d)
            .ToList()
            .Where(l => Constants.ThuHoc.CoHoc(l.ThuHoc, d))
            .Select(l => new LichBan
            {
                MaHV = l.MaHV,
                MaGS = l.MaGS,
                MaLop = l.MaLop,
                MoTa = $"lớp {l.MonHoc} của {l.HocVien?.HoTen}"
            })
            .ToList();

        var bu = db.BuoiHocs.AsNoTracking().Where(b => b.NgayHoc == d && b.MaBuoiGoc != null && b.CaHoc == ca).ToList();
        if (bu.Count > 0)
        {
            var lopIds = bu.Select(b => b.MaLop).Distinct().ToList();
            var lop = db.LopHocs.AsNoTracking().Include(l => l.HocVien)
                .Where(l => lopIds.Contains(l.MaLop)).ToList().ToDictionary(l => l.MaLop);
            foreach (var b in bu)
            {
                lop.TryGetValue(b.MaLop, out var l);
                ds.Add(new LichBan
                {
                    MaHV = l?.MaHV,
                    MaGS = b.MaGSDay,
                    MaLop = b.MaLop,
                    MaBuoi = b.MaBuoi,
                    MoTa = $"buổi học bù {l?.MonHoc} của {l?.HocVien?.HoTen}"
                });
            }
        }
        return ds;
    }

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:517-538
    public List<GiaSuHocBu> GetGiaSuChoHocBu(DateTime ngay, string ca)
    {
        var tamNghi = Constants.GiaSuStatus.TamNghi;
        var lich = string.IsNullOrEmpty(ca) ? new List<LichBan>() : LichTrongCa(_db, ngay, ca);
        return _db.GiaSus.AsNoTracking().Where(g => g.TrangThai != tamNghi).OrderBy(g => g.HoTen).ToList()
            .Select(g => new GiaSuHocBu
            {
                MaGS = g.MaGS,
                HoTen = g.HoTen,
                ChuyenMon = g.ChuyenMon,
                Ban = lich.Where(x => x.MaGS == g.MaGS).Select(x => x.MoTa).FirstOrDefault()
            })
            .ToList();
    }

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:541-596
    public KetQuaHocBu XepHocBu(int maBuoiVang, DateTime ngay, string ca, int maGSDay,
        bool boQuaCanhBao = false, int? chiChoGiaSu = null)
    {
        // HIGH-02: tự xác thực bằng người dùng hiện tại thay vì tin tham số chiChoGiaSu từ UI.
        int? gioiHan = _nguoiDung.GioiHanGiaSu;
        var goc = _db.BuoiHocs.AsNoTracking().FirstOrDefault(b => b.MaBuoi == maBuoiVang);
        if (goc == null) return new KetQuaHocBu { Loi = "Không tìm thấy buổi vắng." };
        if (goc.MaBuoiGoc.HasValue) return new KetQuaHocBu { Loi = "Buổi này đã là buổi học bù." };
        if (Constants.DiemDanh.CoHoc(goc.DiemDanh)) return new KetQuaHocBu { Loi = "Chỉ xếp học bù cho buổi vắng." };
        if (_db.BuoiHocs.AsNoTracking().Any(b => b.MaBuoiGoc == maBuoiVang))
            return new KetQuaHocBu { Loi = "Buổi vắng này đã có lịch học bù. Hủy lịch cũ trước nếu muốn đổi." };

        var lop = _db.LopHocs.AsNoTracking().FirstOrDefault(l => l.MaLop == goc.MaLop);
        if (lop == null) return new KetQuaHocBu { Loi = "Lớp của buổi này không còn tồn tại." };
        if (gioiHan.HasValue && lop.MaGS != gioiHan.Value)
            return new KetQuaHocBu { Loi = "Chỉ xếp học bù cho học viên lớp của bạn." };
        if (!Constants.CaHoc.TatCaCaHoc.Contains(ca)) return new KetQuaHocBu { Loi = "Chọn ca học bù." };
        if (lop.ThoiGianBatDau.HasValue && ngay.Date < lop.ThoiGianBatDau.Value.Date)
            return new KetQuaHocBu { Loi = $"Ngày học bù phải từ ngày lớp bắt đầu ({lop.ThoiGianBatDau:dd/MM/yyyy})." };
        if (ngay.Date > DateTime.Today.AddDays(60))
            return new KetQuaHocBu { Loi = "Chỉ xếp học bù trong vòng 60 ngày tới." };

        var gs = _db.GiaSus.AsNoTracking().FirstOrDefault(g => g.MaGS == maGSDay);
        if (gs == null) return new KetQuaHocBu { Loi = "Chọn gia sư dạy bù." };
        if (gs.TrangThai == Constants.GiaSuStatus.TamNghi)
            return new KetQuaHocBu { Loi = $"Gia sư {gs.HoTen} đang tạm nghỉ." };

        var lich = LichTrongCa(_db, ngay, ca);
        var hvBan = lich.FirstOrDefault(x => x.MaHV.HasValue && x.MaHV == lop.MaHV);
        if (hvBan != null)
            return new KetQuaHocBu { Loi = $"Học viên đã có {hvBan.MoTa} vào {Constants.CaHoc.TenNgan(ca)} ngày {ngay:dd/MM}." };

        var gsBan = lich.FirstOrDefault(x => x.MaGS == maGSDay);
        if (gsBan != null && !boQuaCanhBao)
            return new KetQuaHocBu
            {
                CanhBao = $"Gia sư {gs.HoTen} đang có {gsBan.MoTa} vào {Constants.CaHoc.TenNgan(ca)} ngày {ngay:dd/MM}.\n" +
                          "Vẫn xếp học bù (học chung buổi đó)?"
            };

        var moi = new BuoiHoc
        {
            MaLop = lop.MaLop,
            NgayHoc = ngay.Date,
            CaHoc = ca,
            MaGSDay = maGSDay,
            MaBuoiGoc = maBuoiVang,
            DiemDanh = Constants.DiemDanh.CoMat
        };
        _db.BuoiHocs.Add(moi);
        _db.SaveChanges();
        return new KetQuaHocBu { MaBuoi = moi.MaBuoi };
    }

    // Nguồn: DoAnWD/BLL/Services/HocTapService.cs:599-617
    public void HuyHocBu(int maBuoiBu, int? chiChoGiaSu = null)
    {
        // HIGH-02: tự xác thực bằng người dùng hiện tại thay vì tin tham số chiChoGiaSu từ UI.
        int? gioiHan = _nguoiDung.GioiHanGiaSu;
        var b = _db.BuoiHocs.FirstOrDefault(x => x.MaBuoi == maBuoiBu);
        if (b == null) return;
        if (!b.MaBuoiGoc.HasValue) throw new InvalidOperationException("Đây là buổi học thường, không phải buổi học bù.");
        if (gioiHan.HasValue)
        {
            var lop = _db.LopHocs.AsNoTracking().FirstOrDefault(l => l.MaLop == b.MaLop);
            if (lop?.MaGS != gioiHan.Value && b.MaGSDay != gioiHan.Value)
                throw new InvalidOperationException("Chỉ hủy được buổi bù của lớp bạn hoặc buổi bạn dạy.");
        }
        _db.BuoiHocs.Remove(b);
        _db.SaveChanges();
    }
}