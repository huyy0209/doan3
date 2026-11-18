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


/// <summary>
/// Nghiệp vụ Học viên. Nguồn: DoAnWD/BLL/Services/HocVienService.cs + DoAnWD/DAL/Repositories/HocVienRepository.cs
/// (luật HV-01..HV-07).
/// </summary>
public class HocVienService
{
    private readonly QuanLyGiaSuContext _db;
    private readonly INguoiDungHienTai _nguoiDung;

    public HocVienService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung)
    {
        _db = db;
        _nguoiDung = nguoiDung;
    }

    public List<HocVien> GetAll() => _db.HocViens.AsNoTracking().OrderBy(h => h.HoTen).ToList();

    public HocVien GetById(int id) => _db.HocViens.Find(id);

    /// <summary>
    /// Học viên người dùng hiện tại được xem: quản lý = tất cả; gia sư = học viên có lớp của mình
    /// hoặc lớp có buổi bù mình dạy (DoAnWD/BLL/Services/HocTapService.cs:455-468).
    /// </summary>
    public List<HocVien> GetDuocXem(string tuKhoa = null)
    {
        var ds = Search(tuKhoa);
        int? gioiHan = _nguoiDung.GioiHanGiaSu;
        if (!gioiHan.HasValue) return ds;

        var lopBu = _db.BuoiHocs.Where(b => b.MaGSDay == gioiHan.Value).Select(b => b.MaLop).Distinct().ToList();
        var ids = _db.LopHocs.Where(l => l.MaHV != null && (l.MaGS == gioiHan.Value || lopBu.Contains(l.MaLop)))
            .Select(l => l.MaHV.Value).Distinct().ToList();
        return ds.Where(h => ids.Contains(h.MaHV)).ToList();
    }

    /// <summary>Người dùng hiện tại có được xem học viên này không</summary>
    public bool DuocXem(int maHV)
    {
        int? gioiHan = _nguoiDung.GioiHanGiaSu;
        if (!gioiHan.HasValue) return true;
        return _db.LopHocs.Any(l => l.MaHV == maHV &&
            (l.MaGS == gioiHan.Value || l.BuoiHocs.Any(b => b.MaGSDay == gioiHan.Value)));
    }

    private static string Gon(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>Thêm mới (maHV = null) hoặc cập nhật hồ sơ học viên (HV-01..04). Nguồn: HocVienService.cs:38-67</summary>
    public HocVien LuuHoSo(int? maHV, HoSoHocVien hs)
    {
        _nguoiDung.YeuCauQuanLy();
        string loi = HocVienValidator.KiemTraHoSo(hs);
        if (loi != null) throw new ArgumentException(loi);

        HocVien hv;
        if (maHV.HasValue)
        {
            hv = _db.HocViens.Find(maHV.Value);
            if (hv == null) throw new InvalidOperationException("Không tìm thấy học viên.");
        }
        else
        {
            hv = new HocVien();
            _db.HocViens.Add(hv);
        }

        hv.HoTen = hs.HoTen.Trim();
        hv.SDT = Gon(hs.SDT);
        hv.DiaChi = Gon(hs.DiaChi);
        hv.TenPhuHuynh = Gon(hs.TenPhuHuynh);
        hv.SDTPhuHuynh = Gon(hs.SDTPhuHuynh);
        hv.TruongLop = Gon(hs.TruongLop);
        hv.TrinhDo = Gon(hs.TrinhDo);
        hv.GhiChu = Gon(hs.GhiChu);

        _db.SaveChanges();
        return hv;
    }

    /// <summary>HV-05: xóa (chặn khi còn lớp hoạt động / có lịch sử lớp). Nguồn: HocVienService.cs:69-94</summary>
    public void Delete(int maHV)
    {
        _nguoiDung.YeuCauQuanLy();
        var hocVien = _db.HocViens.Include(h => h.LopHocs).FirstOrDefault(h => h.MaHV == maHV);
        if (hocVien == null) throw new KeyNotFoundException("Không tìm thấy học viên.");

        if (hocVien.LopHocs.Any(l => l.DangHoatDong && Constants.LopHocStatus.LaHoatDong(l.TrangThaiTheoThoiGian)))
            throw new InvalidOperationException(
                $"Không thể xóa!\n\nHọc viên '{hocVien.HoTen}' đang có lớp học.\n\nVui lòng hủy các lớp học trước.");

        int tongLop = hocVien.LopHocs.Count;
        if (tongLop > 0)
            throw new InvalidOperationException(
                $"Không thể xóa!\n\nHọc viên '{hocVien.HoTen}' đã có {tongLop} lớp trong lịch sử học.");

        _db.HocViens.Remove(hocVien);
        _db.SaveChanges();
    }

    /// <summary>HV-06: tìm không dấu (HIGH-04: lọc thô trên SQL rồi lọc lại). Nguồn: HocVienRepository.cs:19-53</summary>
    public List<HocVien> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return GetAll();

        var tuKhoa = keyword.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string cot = "REPLACE(REPLACE((" +
            "ISNULL(HoTen, N'') + N' ' + ISNULL(SDT, N'') + N' ' + ISNULL(DiaChi, N'') + N' ' + " +
            "ISNULL(TenPhuHuynh, N'') + N' ' + ISNULL(SDTPhuHuynh, N'') + N' ' + " +
            "ISNULL(TruongLop, N'') + N' ' + ISNULL(TrinhDo, N'')) " +
            "COLLATE Latin1_General_CI_AI, N'đ', N'd'), N'Đ', N'd')";

        var sql = new StringBuilder("SELECT * FROM dbo.HocVien WHERE 1 = 1");
        var thamSo = new List<object>();
        for (int i = 0; i < tuKhoa.Length; i++)
        {
            sql.Append($" AND {cot} LIKE @p{i} ESCAPE '\\'");
            thamSo.Add(new SqlParameter($"@p{i}", "%" + GiaSuService.EscapeLike(TimKiem.ChuanHoa(tuKhoa[i])) + "%"));
        }

        return _db.HocViens.FromSqlRaw(sql.ToString(), thamSo.ToArray()).AsNoTracking()
            .ToList()
            .Where(h => TimKiem.Khop(keyword, h.HoTen, h.SDT, h.DiaChi, h.TenPhuHuynh, h.SDTPhuHuynh, h.TruongLop, h.TrinhDo))
            .OrderBy(h => h.HoTen)
            .ToList();
    }
}