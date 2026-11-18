using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.ViewModels.BaoCao;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services;


public class BaoCaoThangService
{
    private readonly QuanLyGiaSuContext _db;
    private readonly INguoiDungHienTai _nguoiDung;

    public BaoCaoThangService(QuanLyGiaSuContext db, INguoiDungHienTai nguoiDung)
    {
        _db = db;
        _nguoiDung = nguoiDung;
    }

    public BaoCaoThangViewModel Lay(int thang, int nam)
    {
        _nguoiDung.YeuCauQuanLy();

        var tuNgay = new DateTime(nam, thang, 1);
        var denNgay = tuNgay.AddMonths(1);
        var lopTrongThang = _db.LopHocs.AsNoTracking()
            .Where(l => l.TrangThai != Constants.LopHocStatus.DaHuy &&
                (!l.ThoiGianBatDau.HasValue || l.ThoiGianBatDau < denNgay) &&
                (!l.ThoiGianKetThuc.HasValue || l.ThoiGianKetThuc >= tuNgay));

        var lopTheoGiaSu = lopTrongThang
            .Where(l => l.MaGS.HasValue)
            .GroupBy(l => l.MaGS.Value)
            .Select(g => new { MaGS = g.Key, SoLop = g.Count() })
            .ToList()
            .ToDictionary(x => x.MaGS, x => x.SoLop);

        var vangCoPhep = Constants.DiemDanh.VangCoPhep;
        var vangKhongPhep = Constants.DiemDanh.VangKhongPhep;
        var coMat = Constants.DiemDanh.CoMat;
        var diTre = Constants.DiemDanh.DiTre;
        var buoiTheoGiaSu = _db.BuoiHocs.AsNoTracking()
            .Where(b => b.NgayHoc >= tuNgay && b.NgayHoc < denNgay &&
                b.DiemDanh != null && b.LopHoc.TrangThai != Constants.LopHocStatus.DaHuy)
            .Select(b => new
            {
                MaGS = b.MaBuoiGoc.HasValue
                    ? (b.MaGSDay ?? b.LopHoc.MaGS)
                    : b.LopHoc.MaGS,
                b.DiemDanh
            })
            .Where(b => b.MaGS.HasValue)
            .GroupBy(b => b.MaGS.Value)
            .Select(g => new
            {
                MaGS = g.Key,
                DaDiemDanh = g.Count(b => Constants.DiemDanh.TatCa.Contains(b.DiemDanh)),
                Vang = g.Count(b => b.DiemDanh == vangCoPhep || b.DiemDanh == vangKhongPhep),
                CoMat = g.Count(b => b.DiemDanh == coMat || b.DiemDanh == diTre)
            })
            .ToList()
            .ToDictionary(x => x.MaGS);

        var maGiaSus = lopTheoGiaSu.Keys.Concat(buoiTheoGiaSu.Keys).Distinct().ToList();
        var giaSus = _db.GiaSus.AsNoTracking()
            .Where(g => maGiaSus.Contains(g.MaGS))
            .Select(g => new { g.MaGS, g.HoTen })
            .ToList();
        var theoGiaSu = giaSus.Select(g =>
        {
            lopTheoGiaSu.TryGetValue(g.MaGS, out int soLop);
            buoiTheoGiaSu.TryGetValue(g.MaGS, out var buoi);
            int tongBuoi = buoi?.DaDiemDanh ?? 0;
            int coMatTrongThang = buoi?.CoMat ?? 0;
            return new BaoCaoGiaSuViewModel
            {
                MaGS = g.MaGS,
                HoTen = g.HoTen,
                SoLopDangDay = soLop,
                SoBuoiDaDiemDanh = tongBuoi,
                SoBuoiVang = buoi?.Vang ?? 0,
                TyLeChuyenCan = tongBuoi == 0 ? 0 : Math.Round((decimal)coMatTrongThang * 100 / tongBuoi, 1)
            };
        })
            .OrderBy(g => g.HoTen)
            .ToList();

        var lopTheoMon = lopTrongThang
            .GroupBy(l => l.MonHoc ?? "Chưa xác định")
            .Select(g => new { TenMon = g.Key, SoLop = g.Count() })
            .ToList();

        var hocVienTheoMon = lopTrongThang
            .Where(l => l.MaHV.HasValue)
            .Select(l => new { l.MaHV, TenMon = l.MonHoc ?? "Chưa xác định" })
            .Distinct()
            .GroupBy(l => l.TenMon)
            .Select(g => new { TenMon = g.Key, SoHocVien = g.Count() })
            .ToList()
            .ToDictionary(x => x.TenMon, x => x.SoHocVien);

        var thuTheoMon = _db.ThanhToans.AsNoTracking()
            .Where(t => t.NgayDong >= tuNgay && t.NgayDong < denNgay)
            .GroupBy(t => t.LopHoc.MonHoc ?? "Chưa xác định")
            .Select(g => new { TenMon = g.Key, HocPhiThu = g.Sum(t => t.SoTien) })
            .ToList()
            .ToDictionary(x => x.TenMon, x => x.HocPhiThu);

        var tenMons = lopTheoMon.Select(x => x.TenMon)
            .Concat(hocVienTheoMon.Keys)
            .Concat(thuTheoMon.Keys)
            .Distinct()
            .ToList();
        var theoMon = tenMons.Select(tenMon =>
        {
            hocVienTheoMon.TryGetValue(tenMon, out int soHocVien);
            thuTheoMon.TryGetValue(tenMon, out decimal hocPhiThu);
            return new BaoCaoMonViewModel
            {
                TenMon = tenMon,
                SoLop = lopTheoMon.FirstOrDefault(x => x.TenMon == tenMon)?.SoLop ?? 0,
                SoHocVien = soHocVien,
                HocPhiThu = hocPhiThu
            };
        })
            .OrderBy(x => x.TenMon)
            .ToList();

        int hocVienMoi = _db.LopHocs.AsNoTracking()
            .Where(l => l.MaHV.HasValue && l.ThoiGianBatDau.HasValue)
            .GroupBy(l => l.MaHV.Value)
            .Select(g => g.Min(l => l.ThoiGianBatDau))
            .Count(ngay => ngay >= tuNgay && ngay < denNgay);

        decimal hocPhiThuTong = _db.ThanhToans.AsNoTracking()
            .Where(t => t.NgayDong >= tuNgay && t.NgayDong < denNgay)
            .Sum(t => (decimal?)t.SoTien) ?? 0;

        var daThuTheoLop = _db.ThanhToans.AsNoTracking()
            .Where(t => t.NgayDong < denNgay)
            .GroupBy(t => t.MaLop)
            .Select(g => new { MaLop = g.Key, DaThu = g.Sum(t => t.SoTien) })
            .ToList()
            .ToDictionary(x => x.MaLop, x => x.DaThu);
        var cacLop = _db.LopHocs.AsNoTracking()
            .Where(l => !l.ThoiGianBatDau.HasValue || l.ThoiGianBatDau < denNgay)
            .Select(l => new { l.MaLop, l.HocPhi, l.TrangThai })
            .ToList();
        decimal noCuoiThang = cacLop.Sum(l =>
        {
            if (l.TrangThai == Constants.LopHocStatus.DaHuy) return 0m;
            daThuTheoLop.TryGetValue(l.MaLop, out decimal daThu);
            return Math.Max(0m, (l.HocPhi ?? 0m) - daThu);
        });

        return new BaoCaoThangViewModel
        {
            Thang = thang,
            Nam = nam,
            HocPhiThu = hocPhiThuTong,
            HocVienMoi = hocVienMoi,
            // Bảng Học viên hiện chưa có cột trạng thái nghỉ để truy vấn số liệu này.
            HocVienNghi = 0,
            NoCuoiThang = noCuoiThang,
            TheoGiaSu = theoGiaSu,
            TheoMon = theoMon
        };
    }
}