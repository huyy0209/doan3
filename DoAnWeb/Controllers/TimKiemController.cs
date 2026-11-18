using System.Collections.Generic;
using DoAnWeb.Common;
using DoAnWeb.Services;
using DoAnWeb.ViewModels.TimKiem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;


/// <summary>
/// G9 – Tìm kiếm nhanh toàn cục: gợi ý JSON cho ô tìm trên thanh menu và trang kết quả đầy đủ.
/// Mặc định mọi controller đều phải đăng nhập (Program.cs) nên khách bị đưa về trang đăng nhập.
/// </summary>
[Authorize]
public class TimKiemController : Controller
{
    private readonly TimKiemNhanhService _timKiem;
    private readonly INguoiDungHienTai _nguoiDung;

    public TimKiemController(TimKiemNhanhService timKiem, INguoiDungHienTai nguoiDung)
    {
        _timKiem = timKiem;
        _nguoiDung = nguoiDung;
    }

    /// <summary>Trang kết quả đầy đủ, chia nhóm Học viên / Gia sư / Lớp.</summary>
    [HttpGet]
    public IActionResult Index(string q)
    {
        var kq = _timKiem.TimDayDu(q);
        bool duDai = kq.TuKhoa.Length > 0 &&
                     TimKiem.ChuanHoa(kq.TuKhoa).Trim().Length >= 2;

        var nhom = new List<NhomKetQuaVM>();
        if (duDai)
        {
            if (kq.TongHocVien > 0)
                nhom.Add(new NhomKetQuaVM
                {
                    Ma = "tk-nhom-hoc-vien",
                    TieuDe = "Học viên",
                    Items = kq.HocViens,
                    Tong = kq.TongHocVien
                });
            if (_nguoiDung.LaQuanLy && kq.TongGiaSu > 0)
                nhom.Add(new NhomKetQuaVM
                {
                    Ma = "tk-nhom-gia-su",
                    TieuDe = "Gia sư",
                    Items = kq.GiaSus,
                    Tong = kq.TongGiaSu
                });
            if (kq.TongLop > 0)
                nhom.Add(new NhomKetQuaVM
                {
                    Ma = "tk-nhom-lop",
                    TieuDe = "Lớp học",
                    Items = kq.Lops,
                    Tong = kq.TongLop
                });
        }

        return View(new TimKiemIndexVM { KetQua = kq, DuDai = duDai, Nhom = nhom });
    }

    /// <summary>
    /// Gợi ý khi đang gõ (JSON, tối đa 5 mỗi nhóm). Từ khóa rỗng / 1 ký tự trả ba mảng rỗng.
    /// </summary>
    [HttpGet]
    public IActionResult GoiY(string q)
    {
        var kq = _timKiem.GoiY(q);
        return Json(new
        {
            q = kq.TuKhoa,
            hocViens = kq.HocViens,
            giaSus = kq.GiaSus,
            lops = kq.Lops
        });
    }
}