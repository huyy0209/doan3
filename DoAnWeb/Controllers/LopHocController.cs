using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Infrastructure;
using DoAnWeb.Services;
using DoAnWeb.Services.Dto;
using DoAnWeb.ViewModels.LopHoc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnWeb.Controllers;


/// <summary>
/// Trang Lớp học (frmLopHoc.cs), ghép lớp (frmChucNangGhepLop.cs),
/// sửa lớp (dlgSuaLop.cs), hủy / xóa lớp. Chỉ quản lý.
/// Mọi nghiệp vụ nằm trong LopHocService – controller chỉ chuẩn bị dữ liệu và bắt lỗi.
/// </summary>
[Authorize(Roles = "QuanLy")]
public class LopHocController : Controller
{
    private readonly LopHocService _lopHoc;
    private readonly GiaSuService _giaSu;
    private readonly HocVienService _hocVien;
    private readonly MonHocService _monHoc;
    private readonly XuatExcelService _excel;
    private readonly Microsoft.Extensions.Logging.ILogger<LopHocController> _logger;

    public LopHocController(LopHocService lopHoc, GiaSuService giaSu,
        HocVienService hocVien, MonHocService monHoc, XuatExcelService excel,
        Microsoft.Extensions.Logging.ILogger<LopHocController> logger)
    {
        _lopHoc = lopHoc;
        _giaSu = giaSu;
        _hocVien = hocVien;
        _monHoc = monHoc;
        _excel = excel;
        _logger = logger;
    }

    // ===================================================================
    // DANH SÁCH + CHI TIẾT
    // ===================================================================

    /// <summary>Danh sách lớp, tìm không dấu và lọc trạng thái / ca / môn / gia sư.</summary>
    [HttpGet]
    public IActionResult Index(string q, string trangThai, string ca, string mon, int? maGS, int? chon,
        int trang = 1, int soDong = PhanTrang.SoDongMacDinh, string sapXep = null, string chieu = null)
    {
        var tatCa = _lopHoc.GetDanhSachLop();
        var vm = new LopHocIndexVM
        {
            TuKhoa = q,
            TrangThai = string.IsNullOrWhiteSpace(trangThai) ? "Còn hoạt động" : trangThai,
            CaHoc = ca,
            MonHoc = mon,
            MaGS = maGS,
            ChonMaLop = chon,
            TongSoTatCa = tatCa.Count,
            GiaSus = _giaSu.GetAll(),
            MonHocs = tatCa.Select(l => l.MonHoc).Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m => m).ToList()
        };

        var danhSach = LocDanhSach(tatCa, vm.TuKhoa, vm.TrangThai, vm.CaHoc, vm.MonHoc, vm.MaGS);
        bool giamDan = chieu == "desc";
        IEnumerable<LopHocDong> sapXepTheo = sapXep switch
        {
            "tenLop" => (giamDan ? danhSach.OrderByDescending(l => l.TenLop) : danhSach.OrderBy(l => l.TenLop)).ThenBy(l => l.MaLop),
            "hocVien" => (giamDan ? danhSach.OrderByDescending(l => l.HocVien) : danhSach.OrderBy(l => l.HocVien)).ThenBy(l => l.MaLop),
            "giaSu" => (giamDan ? danhSach.OrderByDescending(l => l.GiaSu) : danhSach.OrderBy(l => l.GiaSu)).ThenBy(l => l.MaLop),
            "monHoc" => (giamDan ? danhSach.OrderByDescending(l => l.MonHoc) : danhSach.OrderBy(l => l.MonHoc)).ThenBy(l => l.MaLop),
            "loaiMon" => (giamDan ? danhSach.OrderByDescending(l => l.LoaiMon) : danhSach.OrderBy(l => l.LoaiMon)).ThenBy(l => l.MaLop),
            "caHoc" => (giamDan ? danhSach.OrderByDescending(l => l.CaHoc) : danhSach.OrderBy(l => l.CaHoc)).ThenBy(l => l.MaLop),
            "thuHoc" => (giamDan ? danhSach.OrderByDescending(l => l.ThuHoc) : danhSach.OrderBy(l => l.ThuHoc)).ThenBy(l => l.MaLop),
            "soBuoi" => (giamDan ? danhSach.OrderByDescending(l => l.SoBuoi) : danhSach.OrderBy(l => l.SoBuoi)).ThenBy(l => l.MaLop),
            "batDau" => (giamDan ? danhSach.OrderByDescending(l => l.BatDau) : danhSach.OrderBy(l => l.BatDau)).ThenBy(l => l.MaLop),
            "ketThuc" => (giamDan ? danhSach.OrderByDescending(l => l.KetThuc) : danhSach.OrderBy(l => l.KetThuc)).ThenBy(l => l.MaLop),
            "trangThaiLop" => (giamDan ? danhSach.OrderByDescending(l => l.TrangThai) : danhSach.OrderBy(l => l.TrangThai)).ThenBy(l => l.MaLop),
            "hocPhi" => (giamDan ? danhSach.OrderByDescending(l => l.HocPhi) : danhSach.OrderBy(l => l.HocPhi)).ThenBy(l => l.MaLop),
            "danhGia" => (giamDan ? danhSach.OrderByDescending(l => l.DanhGia) : danhSach.OrderBy(l => l.DanhGia)).ThenBy(l => l.MaLop),
            "diemTB" => (giamDan ? danhSach.OrderByDescending(l => l.DiemTB) : danhSach.OrderBy(l => l.DiemTB)).ThenBy(l => l.MaLop),
            _ => danhSach
        };
        var ketQuaTrang = PhanTrang.Tao(sapXepTheo, trang, soDong, sapXep, chieu,
            new Dictionary<string, string>
            {
                ["q"] = q ?? "",
                ["trangThai"] = vm.TrangThai ?? "",
                ["ca"] = ca ?? "",
                ["mon"] = mon ?? "",
                ["maGS"] = maGS?.ToString() ?? "",
                ["chon"] = chon?.ToString() ?? ""
            });
        ViewBag.PhanTrang = ketQuaTrang;
        vm.DanhSach = ketQuaTrang.Items;
        vm.TongSo = ketQuaTrang.TongSo;
        return View(vm);
    }

    /// <summary>Xuất danh sách lớp ra Excel (.xlsx) theo đúng bộ lọc của trang.</summary>
    [HttpGet]
    public IActionResult XuatExcel(string q, string trangThai, string ca, string mon, int? maGS)
    {
        string loc = string.IsNullOrWhiteSpace(trangThai) ? "Còn hoạt động" : trangThai;
        var ds = LocDanhSach(_lopHoc.GetDanhSachLop(), q, loc, ca, mon, maGS);
        return File(_excel.DanhSachLop(ds),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"DanhSachLop_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    /// <summary>Lọc danh sách lớp theo trạng thái, ca, môn, gia sư và từ khóa không dấu.</summary>
    private static List<LopHocDong> LocDanhSach(IEnumerable<LopHocDong> tatCa,
        string tuKhoa, string trangThai, string caHoc, string monHoc, int? maGS)
    {
        IEnumerable<LopHocDong> ds = tatCa;
        if (trangThai == "Còn hoạt động")
            ds = ds.Where(l => Constants.LopHocStatus.LaHoatDong(l.TrangThai));
        else if (trangThai != "Tất cả")
            ds = ds.Where(l => l.TrangThai == trangThai);

        if (!string.IsNullOrWhiteSpace(caHoc)) ds = ds.Where(l => l.CaHoc == caHoc);
        if (!string.IsNullOrWhiteSpace(monHoc))
            ds = ds.Where(l => string.Equals(l.MonHoc, monHoc, StringComparison.OrdinalIgnoreCase));
        if (maGS.HasValue) ds = ds.Where(l => l.MaGS == maGS.Value);

        return ds
            .Where(l => TimKiem.Khop(tuKhoa, l.TenLop, l.HocVien, l.GiaSu, l.MonHoc))
            .ToList();
    }

    /// <summary>Chi tiết một lớp: thông tin, đánh giá / tiến trình cấp độ, nút sửa – hủy – xóa.</summary>
    [HttpGet]
    public IActionResult Details(int id)
    {
        var lop = _lopHoc.GetDanhSachLop().FirstOrDefault(l => l.MaLop == id);
        if (lop == null)
        {
            TempData["Loi"] = "Không tìm thấy lớp học.";
            return RedirectToAction(nameof(Index));
        }
        return View(lop);
    }

    // ===================================================================
    // GHÉP LỚP
    // ===================================================================

    [HttpGet]
    public IActionResult GhepLop()
    {
        var vm = new GhepLopVM
        {
            BatDau = DateTime.Today.AddDays(1),
            KetThuc = DateTime.Today.AddDays(1).AddMonths(1),
            ThuHoc = string.Join(",", Constants.ThuHoc.TatCa)
        };
        NapGhepLop(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GhepLop(GhepLopVM vm)
    {
        if (!vm.BatDau.HasValue || !vm.KetThuc.HasValue)
            ModelState.AddModelError(string.Empty, "Chọn ngày bắt đầu và ngày kết thúc.");
        if (string.IsNullOrWhiteSpace(vm.CaHoc))
            ModelState.AddModelError(string.Empty, Constants.Messages.VuiLongChonCaHoc);
        if (!SoTien.TryParse(vm.HocPhi, out decimal hocPhi))
            ModelState.AddModelError(nameof(vm.HocPhi), SoTien.ThongBaoDinhDang);

        if (!ModelState.IsValid)
        {
            NapGhepLop(vm);
            return View(vm);
        }

        try
        {
            // Giờ bắt đầu / kết thúc lấy theo ca như app (LH-09).
            LopHocGhepLopLogic.LayKhoang(vm.CaHoc, vm.BatDau.Value, vm.KetThuc.Value, out var bd, out var kt);
            var lop = _lopHoc.GhepLop(new GhepLopYeuCau
            {
                MaHV = vm.MaHV ?? 0,
                MaGS = vm.MaGS ?? 0,
                MonHoc = vm.MonHoc,
                HocPhi = hocPhi,
                BatDau = bd,
                KetThuc = kt,
                CaHoc = vm.CaHoc,
                ThuHoc = vm.ThuHoc,
                TenLop = vm.TenLop,
                CapDoDauVao = vm.CapDoDauVao
            });
            TempData["ThongBao"] = $"Đã ghép lớp \"{lop.TenLop}\".";
            return RedirectToAction(nameof(Details), new { id = lop.MaLop });
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, LoiHeThong.GhiLogVaLayThongBao(_logger, ex, "ghép lớp"));
        }

        NapGhepLop(vm);
        return View(vm);
    }

    /// <summary>
    /// Kiểm tra real-time cho dòng trạng thái ở form ghép lớp.
    /// Trả JSON: ok, mucDo (loi | canhbao | ok | trong), thongBao, soBuoi, moTa, gio.
    /// </summary>
    [HttpGet]
    public IActionResult KiemTra(int? maHV, int? maGS, string monHoc, string caHoc,
        DateTime? batDau, DateTime? ketThuc, string thuHoc, string tenLop)
    {
        JsonResult Kq(string mucDo, string thongBao, int soBuoi = 0, string moTa = null, string gio = null) =>
            Json(new
            {
                ok = mucDo == "ok",
                mucDo,
                thongBao,
                soBuoi,
                moTa = moTa ?? Constants.ThuHoc.MoTa(thuHoc),
                gio
            });

        if (string.IsNullOrWhiteSpace(caHoc) || !Constants.CaHoc.TatCaCaHoc.Contains(caHoc))
            return Kq("trong", "Chọn một ca học.");
        if (!batDau.HasValue || !ketThuc.HasValue)
            return Kq("loi", "Chọn ngày bắt đầu và ngày kết thúc.");
        if (ketThuc.Value.Date <= batDau.Value.Date)
            return Kq("loi", "Ngày kết thúc phải sau ngày bắt đầu.");

        LopHocGhepLopLogic.LayKhoang(caHoc, batDau.Value, ketThuc.Value, out var bd, out var kt);
        string gio = $"{bd:HH:mm}–{kt:HH:mm}";
        int soBuoi = new Data.Entities.LopHoc
        {
            ThoiGianBatDau = bd,
            ThoiGianKetThuc = kt,
            ThuHoc = thuHoc
        }.DemSoBuoi();
        if (soBuoi == 0)
            return Kq("loi", "Khoảng ngày này không có buổi nào rơi vào các thứ đã chọn.", soBuoi, gio: gio);

        if (!string.IsNullOrWhiteSpace(tenLop) && _lopHoc.TrungTenLop(tenLop))
            return Kq("loi", $"Đã có lớp tên \"{tenLop.Trim()}\". Hãy đặt tên khác.", soBuoi, gio: gio);

        // Khi đã chọn đủ người dùng TimXungDot của service (đúng luật LH-11..LH-15);
        // mới chọn một bên thì chỉ kiểm tra bên đó như app.
        string xungDot = null;
        if (maHV.HasValue && maHV.Value > 0 && maGS.HasValue && maGS.Value > 0)
        {
            xungDot = _lopHoc.TimXungDot(maGS.Value, maHV.Value, monHoc, caHoc, bd, kt, thuHoc);
        }
        else
        {
            var lopHoatDong = _lopHoc.GetLopConHoatDong();
            if (maHV.HasValue && maHV.Value > 0)
            {
                var trung = LopHocGhepLopLogic.TimLopTrung(lopHoatDong, l => l.MaHV == maHV.Value,
                    caHoc, thuHoc, batDau.Value, ketThuc.Value);
                if (trung != null)
                    xungDot = $"Học viên đã có lớp {trung.MonHoc} ở ca {trung.CaHoc}, đến {trung.ThoiGianKetThuc:dd/MM/yyyy}.";
            }
            if (xungDot == null && maGS.HasValue && maGS.Value > 0)
            {
                var trung = LopHocGhepLopLogic.TimLopTrung(lopHoatDong, l => l.MaGS == maGS.Value,
                    caHoc, thuHoc, batDau.Value, ketThuc.Value);
                if (trung != null)
                    xungDot = $"Gia sư đã có lớp {trung.MonHoc} ở ca {trung.CaHoc}, đến {trung.ThoiGianKetThuc:dd/MM/yyyy}.";
            }
        }

        if (xungDot != null) return Kq("loi", xungDot, soBuoi, gio: gio);

        if (maGS.HasValue && maGS.Value > 0)
        {
            var gs = _giaSu.GetById(maGS.Value);
            if (gs != null && !gs.NhanCa(caHoc))
                return Kq("canhbao",
                    $"Lưu ý: {gs.HoTen} chỉ nhận ca {gs.CaNhanDay?.Replace(",", ", ")}. Vẫn ghép được nếu đã thống nhất.",
                    soBuoi, gio: gio);
        }

        if (!maHV.HasValue || maHV.Value <= 0 || !maGS.HasValue || maGS.Value <= 0)
            return Kq("trong", "Chọn học viên và gia sư để kiểm tra trùng lịch.", soBuoi, gio: gio);

        return Kq("ok", $"{soBuoi} buổi ({Constants.ThuHoc.MoTa(thuHoc)}), {gio}.", soBuoi, gio: gio);
    }

    /// <summary>Gợi ý tên lớp "Môn - Học viên - Ca", tự thêm (2), (3)… nếu trùng.</summary>
    [HttpGet]
    public IActionResult GoiYTen(string monHoc, int? maHV, string caHoc)
    {
        var hv = maHV.HasValue && maHV.Value > 0 ? _hocVien.GetById(maHV.Value) : null;
        var daCo = new HashSet<string>(
            _lopHoc.GetAll().Where(l => !string.IsNullOrWhiteSpace(l.TenLop)).Select(l => l.TenLop),
            StringComparer.OrdinalIgnoreCase);
        string ten = LopHocGhepLopLogic.GoiYTenLop(monHoc, hv?.HoTen, caHoc, daCo);
        return Json(new { ten });
    }

    // ===================================================================
    // SỬA LỚP
    // ===================================================================

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var lop = _lopHoc.GetDanhSachLop().FirstOrDefault(l => l.MaLop == id);
        if (lop == null)
        {
            TempData["Loi"] = "Không tìm thấy lớp học.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new SuaLopVM
        {
            MaLop = lop.MaLop,
            TenLop = lop.TenLop,
            HocPhi = lop.HocPhi.ToString("0"),
            KetThuc = lop.KetThuc?.Date,
            ThuHoc = lop.ThuHocGoc,
            MaGS = lop.MaGS ?? 0,
            Lop = lop
        };
        NapSuaLop(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, SuaLopVM vm)
    {
        vm.MaLop = id;
        if (vm.KetThuc == null)
            ModelState.AddModelError(string.Empty, "Chọn ngày kết thúc.");
        if (!SoTien.TryParse(vm.HocPhi, out decimal hocPhi))
            ModelState.AddModelError(nameof(vm.HocPhi), SoTien.ThongBaoDinhDang);
        if (vm.MaGS <= 0)
            ModelState.AddModelError(string.Empty, Constants.Messages.VuiLongChonGiaSu);

        if (!ModelState.IsValid)
        {
            NapSuaLop(vm);
            return View(vm);
        }

        try
        {
            _lopHoc.SuaLop(id, new SuaLopYeuCau
            {
                TenLop = vm.TenLop,
                HocPhi = hocPhi,
                KetThuc = vm.KetThuc.Value.Date,
                ThuHoc = vm.ThuHoc,
                MaGS = vm.MaGS
            });
            TempData["ThongBao"] = "Đã lưu thay đổi của lớp.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, LoiHeThong.GhiLogVaLayThongBao(_logger, ex, "sửa lớp"));
        }

        NapSuaLop(vm);
        return View(vm);
    }

    // ===================================================================
    // HỦY / XÓA
    // ===================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Huy(int id)
    {
        try
        {
            _lopHoc.HuyLop(id);
            TempData["ThongBao"] = "Đã hủy lớp.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            TempData["Loi"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        try
        {
            _lopHoc.Delete(id);
            TempData["ThongBao"] = Constants.Messages.XoaThanhCong;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            TempData["Loi"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // ===================================================================
    // NẠP DỮ LIỆU CHO BIỂU MẪU
    // ===================================================================

    private void NapGhepLop(GhepLopVM vm)
    {
        vm.HocViens = _hocVien.GetAll();
        vm.GiaSus = _giaSu.GetForGhepLop();
        vm.MonHocs = _monHoc.GetAll(kemNgungDay: false);
        vm.LopHoatDong = _lopHoc.GetLopConHoatDong();
        vm.TenLopDaCo = new HashSet<string>(
            _lopHoc.GetAll().Where(l => !string.IsNullOrWhiteSpace(l.TenLop)).Select(l => l.TenLop),
            StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(vm.ThuHoc))
            vm.ThuHoc = string.Join(",", Constants.ThuHoc.TatCa);
    }

    private void NapSuaLop(SuaLopVM vm)
    {
        var lop = _lopHoc.GetDanhSachLop().FirstOrDefault(l => l.MaLop == vm.MaLop);
        vm.Lop = lop;
        if (lop == null) return;

        var ds = _giaSu.GetForGhepLop().OrderBy(g => g.HoTen).ToList();
        // Gia sư hiện tại đang tạm nghỉ vẫn phải hiện để giữ nguyên được.
        if (lop.MaGS.HasValue && ds.All(g => g.MaGS != lop.MaGS.Value))
        {
            var gsCu = _giaSu.GetById(lop.MaGS.Value);
            if (gsCu != null) ds.Insert(0, gsCu);
        }
        vm.GiaSus = ds;
        if (vm.MaGS <= 0 && lop.MaGS.HasValue) vm.MaGS = lop.MaGS.Value;
        if (vm.KetThuc == null) vm.KetThuc = lop.KetThuc?.Date;
        if (vm.ThuHoc == null) vm.ThuHoc = lop.ThuHocGoc;
        if (string.IsNullOrWhiteSpace(vm.TenLop)) vm.TenLop = lop.TenLop;
        if (string.IsNullOrWhiteSpace(vm.HocPhi)) vm.HocPhi = lop.HocPhi.ToString("0");
    }
}