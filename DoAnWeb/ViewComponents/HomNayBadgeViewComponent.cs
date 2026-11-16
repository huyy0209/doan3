using System;
using DoAnWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DoAnWeb.ViewComponents
{
    public class HomNayBadgeViewComponent : ViewComponent
    {
        private readonly HomNayService _homNay;
        private readonly INguoiDungHienTai _nguoiDung;
        private readonly IMemoryCache _cache;

        public HomNayBadgeViewComponent(HomNayService homNay, INguoiDungHienTai nguoiDung, IMemoryCache cache)
        {
            _homNay = homNay;
            _nguoiDung = nguoiDung;
            _cache = cache;
        }

        public IViewComponentResult Invoke(string loai)
        {
            if (!_nguoiDung.DaDangNhap || (loai == "nhacNo" && !_nguoiDung.LaQuanLy))
                return Content(string.Empty);
            if (loai != "diemDanh" && loai != "nhacNo")
                throw new ArgumentOutOfRangeException(nameof(loai));

            string khoa = $"hom-nay-badge:{_nguoiDung.MaTK}:{_nguoiDung.VaiTro}:{_nguoiDung.MaGS}";
            var soLieu = _cache.GetOrCreate(khoa, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                if (_nguoiDung.LaQuanLy)
                {
                    var ql = _homNay.LayChoQuanLy();
                    return new HomNayBadgeSoLieu
                    {
                        DiemDanh = ql.ChuaDiemDanhQuaGio.Count,
                        NhacNo = ql.NoQuaHan.Count
                    };
                }

                return new HomNayBadgeSoLieu
                {
                    DiemDanh = _homNay.LayChoGiaSu().ChuaDiemDanh7Ngay.Count
                };
            });

            int count = loai == "nhacNo" ? soLieu.NhacNo : soLieu.DiemDanh;
            if (count == 0) return Content(string.Empty);
            return View(new HomNayBadgeViewModel
            {
                SoLuong = count,
                Nhan = loai == "nhacNo" ? $"{count} lớp nợ quá 30 ngày" : $"{count} buổi chưa điểm danh"
            });
        }
    }

    public class HomNayBadgeSoLieu
    {
        public int DiemDanh { get; set; }
        public int NhacNo { get; set; }
    }

    public class HomNayBadgeViewModel
    {
        public int SoLuong { get; set; }
        public string Nhan { get; set; }
    }
}
