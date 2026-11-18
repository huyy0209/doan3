using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services.Validators;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services;


/// <summary>Gia sư kèm số lớp đang hoạt động (dòng bảng trang Gia sư)</summary>
public class GiaSuDong
{
    public GiaSu GiaSu { get; set; }
    public int SoLop { get; set; }
}

/// <summary>
/// Nghiệp vụ Gia sư. Nguồn: DoAnWD/BLL/Services/GiaSuService.cs + DoAnWD/DAL/Repositories/GiaSuRepository.cs
/// (luật GS-01..GS-11).
/// </summary>
public class GiaSuService
{
    private readonly QuanLyGiaSuContext _db;
    private readonly INguoiDungHienTai _nguoiDung;

    public GiaSuService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung)
    {
        _db = db;
        _nguoiDung = nguoiDung;
    }

    public List<GiaSu> GetAll() => _db.GiaSus.AsNoTracking().OrderBy(g => g.HoTen).ToList();

    public GiaSu GetById(int id) => _db.GiaSus.Find(id);

    /// <summary>GS-10: gia sư nhận được lớp mới = mọi gia sư trừ "Tạm nghỉ" (GiaSuRepository.cs:26-32)</summary>
    public List<GiaSu> GetForGhepLop() =>
        _db.GiaSus.AsNoTracking()
            .Where(g => g.TrangThai == null || g.TrangThai != Constants.GiaSuStatus.TamNghi)
            .OrderBy(g => g.HoTen)
            .ToList();

    /// <summary>Danh sách cho trang Gia sư: kèm số lớp đang hoạt động (chưa bắt đầu / đang diễn ra)</summary>
    public List<GiaSuDong> GetDanhSach(string tuKhoa = null)
    {
        var ds = Search(tuKhoa);
        var ma = ds.Select(g => g.MaGS).ToList();
        var hoatDong = Constants.LopHocStatus.HoatDong;
        var lops = _db.LopHocs.AsNoTracking()
            .Where(l => l.MaGS != null && ma.Contains(l.MaGS.Value) && hoatDong.Contains(l.TrangThai))
            .ToList()
            .Where(l => l.DangHoatDong && Constants.LopHocStatus.LaHoatDong(l.TrangThaiTheoThoiGian))
            .GroupBy(l => l.MaGS.Value)
            .ToDictionary(g => g.Key, g => g.Count());
        return ds.Select(g => new GiaSuDong { GiaSu = g, SoLop = lops.TryGetValue(g.MaGS, out var n) ? n : 0 }).ToList();
    }

    private static string Gon(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>
    /// Thêm mới (maGS = null) hoặc cập nhật hồ sơ gia sư (GS-01..07). Nguồn: GiaSuService.cs:60-100.
    /// Trạng thái: chỉ "Tạm nghỉ" do người dùng chọn; Rảnh / Bận / Đang dạy tính theo lịch.
    /// </summary>
    public GiaSu LuuHoSo(int? maGS, HoSoGiaSu hs)
    {
        _nguoiDung.YeuCauQuanLy();
        string loi = GiaSuValidator.KiemTraHoSo(hs);
        if (loi != null) throw new ArgumentException(loi);
        if (ExistsSDT(hs.SDT, maGS))
            throw new ArgumentException(Constants.Messages.SDTDaTonTai);

        GiaSu gs;
        if (maGS.HasValue)
        {
            gs = _db.GiaSus.Find(maGS.Value);
            if (gs == null) throw new InvalidOperationException("Không tìm thấy gia sư.");
        }
        else
        {
            gs = new GiaSu();
            _db.GiaSus.Add(gs);
        }

        gs.HoTen = hs.HoTen.Trim();
        gs.SDT = hs.SDT.Trim();
        gs.ChuyenMon = hs.ChuyenMon.Trim();
        gs.Email = Gon(hs.Email);
        gs.HocVan = Gon(hs.HocVan);
        gs.KinhNghiem = hs.KinhNghiem;
        gs.GhiChu = Gon(hs.GhiChu);
        bool moiCa = hs.CaNhanDay == null || hs.CaNhanDay.Count == 0 ||
                     hs.CaNhanDay.Count == Constants.CaHoc.TatCaCaHoc.Length;
        gs.CaNhanDay = moiCa ? null : string.Join(",", hs.CaNhanDay);

        if (hs.TamNghi) gs.TrangThai = Constants.GiaSuStatus.TamNghi;
        else
        {
            gs.TrangThai = Constants.GiaSuStatus.Ranh;
            if (maGS.HasValue) DongBoTrangThai(_db, maGS.Value);
        }

        _db.SaveChanges();
        return gs;
    }

    /// <summary>GS-09: xóa gia sư (chặn khi còn lớp / có lịch sử / từng dạy bù). Nguồn: GiaSuService.cs:102-145</summary>
    public void Delete(int maGS)
    {
        _nguoiDung.YeuCauQuanLy();
        var giaSu = _db.GiaSus.Include(g => g.LopHocs).FirstOrDefault(g => g.MaGS == maGS);
        if (giaSu == null) throw new KeyNotFoundException("Không tìm thấy gia sư.");

        int lopCount = giaSu.LopHocs.Count(l => l.DangHoatDong && Constants.LopHocStatus.LaHoatDong(l.TrangThaiTheoThoiGian));
        if (lopCount > 0)
            throw new InvalidOperationException(
                $"Không thể xóa!\n\nGia sư '{giaSu.HoTen}' đang có {lopCount} lớp học.\n\nVui lòng xóa các lớp học trước.");

        int tongLop = giaSu.LopHocs.Count;
        if (tongLop > 0)
            throw new InvalidOperationException(
                $"Không thể xóa!\n\nGia sư '{giaSu.HoTen}' đã có {tongLop} lớp trong lịch sử.\n\n" +
                $"Hãy chuyển trạng thái sang \"{Constants.GiaSuStatus.TamNghi}\" thay vì xóa.");

        // CRIT-02: chặn xóa nếu gia sư đã từng dạy buổi học bù (FK_BuoiHoc_GiaSuDay)
        if (_db.BuoiHocs.Any(b => b.MaGSDay == maGS))
            throw new InvalidOperationException(
                $"Không thể xóa!\n\nGia sư '{giaSu.HoTen}' đã từng dạy các buổi học bù trong hệ thống.\n\n" +
                $"Hãy chuyển trạng thái sang \"{Constants.GiaSuStatus.TamNghi}\" để lưu giữ lịch sử giảng dạy.");

        // Gia sư chưa có lớp nào nhưng có tài khoản đăng nhập => xóa luôn tài khoản
        foreach (var tk in _db.TaiKhoans.Where(t => t.MaGS == maGS).ToList())
            _db.TaiKhoans.Remove(tk);

        _db.GiaSus.Remove(giaSu);
        _db.SaveChanges();
    }

    /// <summary>GS-05: số điện thoại đã được gia sư khác dùng chưa (GiaSuRepository.cs:126-134)</summary>
    public bool ExistsSDT(string sdt, int? boQuaMaGS = null)
    {
        if (string.IsNullOrWhiteSpace(sdt)) return false;
        sdt = sdt.Trim();
        return boQuaMaGS.HasValue
            ? _db.GiaSus.Any(g => g.SDT == sdt && g.MaGS != boQuaMaGS.Value)
            : _db.GiaSus.Any(g => g.SDT == sdt);
    }

    /// <summary>
    /// GS-11: tìm không dấu, nhiều từ. Lọc thô trên SQL Server (HIGH-04) rồi lọc lại bằng TimKiem.Khop.
    /// Nguồn: GiaSuRepository.cs:139-171
    /// </summary>
    public List<GiaSu> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return GetAll();

        var tuKhoa = keyword.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string cot = "REPLACE(REPLACE((" +
            "ISNULL(HoTen, N'') + N' ' + ISNULL(SDT, N'') + N' ' + " +
            "ISNULL(ChuyenMon, N'') + N' ' + ISNULL(TrangThai, N'')) " +
            "COLLATE Latin1_General_CI_AI, N'đ', N'd'), N'Đ', N'd')";

        var sql = new StringBuilder("SELECT * FROM dbo.GiaSu WHERE 1 = 1");
        var thamSo = new List<object>();
        for (int i = 0; i < tuKhoa.Length; i++)
        {
            sql.Append($" AND {cot} LIKE @p{i} ESCAPE '\\'");
            thamSo.Add(new SqlParameter($"@p{i}", "%" + EscapeLike(TimKiem.ChuanHoa(tuKhoa[i])) + "%"));
        }

        return _db.GiaSus.FromSqlRaw(sql.ToString(), thamSo.ToArray()).AsNoTracking()
            .ToList()
            .Where(g => TimKiem.Khop(keyword, g.HoTen, g.SDT, g.ChuyenMon, g.TrangThai))
            .OrderBy(g => g.HoTen)
            .ToList();
    }

    /// <summary>Thoát ký tự đặc biệt của LIKE để từ khóa được hiểu theo nghĩa đen</summary>
    internal static string EscapeLike(string s) =>
        s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");

    /// <summary>
    /// GS-08: đồng bộ trạng thái gia sư theo lịch dạy (GiaSuRepository.cs:101-121). Chỉ đổi entity, người gọi SaveChanges.
    ///  - Có lớp đang diễn ra => "Đang dạy"; chỉ có lớp chưa bắt đầu => "Bận"; không còn lớp => "Rảnh"; "Tạm nghỉ" giữ nguyên.
    /// </summary>
    public static void DongBoTrangThai(QuanLyGiaSuContext db, int maGS)
    {
        var giaSu = db.GiaSus.Find(maGS);
        if (giaSu == null) return;
        if (giaSu.TrangThai == Constants.GiaSuStatus.TamNghi) return;

        db.Entry(giaSu).Collection(g => g.LopHocs).Load();
        var trangThaiLop = giaSu.LopHocs
            .Where(l => l.DangHoatDong)
            .Select(l => l.TrangThaiTheoThoiGian)
            .ToList();

        if (trangThaiLop.Any(t => t == Constants.LopHocStatus.DangDienRa || t == Constants.LopHocStatus.DangHoc))
            giaSu.TrangThai = Constants.GiaSuStatus.DangDay;
        else if (trangThaiLop.Any(t => t == Constants.LopHocStatus.ChuaBatDau))
            giaSu.TrangThai = Constants.GiaSuStatus.Ban;
        else
            giaSu.TrangThai = Constants.GiaSuStatus.Ranh;
    }
}