using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Services.Dto;

namespace DoAnWeb.Services
{
    public class HomNayDuLieu
    {
        public List<DiemDanhRow> LopHomNay { get; set; } = new List<DiemDanhRow>();
        public List<HomNayBuoi> ChuaDiemDanhQuaGio { get; set; } = new List<HomNayBuoi>();
        public List<NhacNoDong> NoQuaHan { get; set; } = new List<NhacNoDong>();
    }

    public class HomNayGiaSuDuLieu
    {
        public int SoBuoiHomNay { get; set; }
        public List<HomNayBuoi> ChuaDiemDanh7Ngay { get; set; } = new List<HomNayBuoi>();
    }

    public class HomNayBuoi
    {
        public DateTime Ngay { get; set; }
        public DiemDanhRow Buoi { get; set; }
    }

    /// <summary>Dữ liệu lịch hôm nay và buổi chưa điểm danh trên trang Tổng quan / Điểm danh.</summary>
    public class HomNayService
    {
        private readonly HocTapService _hocTap;
        private readonly NhacNoService _nhacNo;
        private readonly INguoiDungHienTai _nguoiDung;

        public HomNayService(HocTapService hocTap, NhacNoService nhacNo, INguoiDungHienTai nguoiDung)
        {
            _hocTap = hocTap;
            _nhacNo = nhacNo;
            _nguoiDung = nguoiDung;
        }

        public HomNayDuLieu LayChoQuanLy(DateTime? thoiDiem = null)
        {
            _nguoiDung.YeuCauQuanLy();
            var now = thoiDiem ?? DateTime.Now;
            var lopHomNay = _hocTap.GetLopTrongNgay(now.Date);
            return new HomNayDuLieu
            {
                LopHomNay = lopHomNay,
                ChuaDiemDanhQuaGio = lopHomNay
                    .Where(b => ChuaDiemDanh(b) && DaQuaGio(b.CaHoc, now))
                    .Select(b => new HomNayBuoi { Ngay = now.Date, Buoi = b })
                    .ToList(),
                NoQuaHan = _nhacNo.GetDanhSach().Where(n => n.SoNgayChuaDong > 30).ToList()
            };
        }

        public HomNayGiaSuDuLieu LayChoGiaSu(DateTime? thoiDiem = null)
        {
            var maGS = _nguoiDung.GioiHanGiaSu;
            if (!maGS.HasValue) return new HomNayGiaSuDuLieu();

            var now = thoiDiem ?? DateTime.Now;
            var lichBayNgay = _hocTap.GetLopTrongKhoangNgay(now.Date.AddDays(-6), now.Date, maGS.Value);
            var homNay = lichBayNgay[now.Date];
            var chuaDiemDanh = new List<HomNayBuoi>();
            foreach (var (ngay, buoiTrongNgay) in lichBayNgay)
            {
                foreach (var buoi in buoiTrongNgay)
                {
                    if (ChuaDiemDanh(buoi) && (ngay < now.Date || DaQuaGio(buoi.CaHoc, now)))
                        chuaDiemDanh.Add(new HomNayBuoi { Ngay = ngay, Buoi = buoi });
                }
            }

            return new HomNayGiaSuDuLieu
            {
                SoBuoiHomNay = homNay.Count,
                ChuaDiemDanh7Ngay = chuaDiemDanh
                    .OrderByDescending(b => b.Ngay)
                    .ThenBy(b => b.Buoi.CaHoc)
                    .ThenBy(b => b.Buoi.HocVien)
                    .ToList()
            };
        }

        public static bool DaQuaGio(string caHoc, DateTime thoiDiem)
        {
            return Constants.CaHoc.TryGetGio(caHoc, out _, out var gioKetThuc)
                && thoiDiem >= thoiDiem.Date.AddHours(gioKetThuc);
        }

        private static bool ChuaDiemDanh(DiemDanhRow buoi) => !buoi.DaLuu && !buoi.LaHocBu;
    }
}
