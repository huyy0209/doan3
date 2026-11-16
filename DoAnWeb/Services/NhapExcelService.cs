using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using DoAnWeb.Common;
using DoAnWeb.Data;
using DoAnWeb.Services.Validators;
using DoAnWeb.ViewModels.NhapExcelVM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DoAnWeb.Services;

public sealed class NhapExcelService
{
    public const int GioiHanByte = 2 * 1024 * 1024;
    public const int GioiHanDong = 1000;
    private readonly QuanLyGiaSuContext _db;
    private readonly HocVienService _hocVien;
    private readonly GiaSuService _giaSu;
    private readonly IMemoryCache _cache;
    private readonly INguoiDungHienTai _nguoiDung;

    public NhapExcelService(QuanLyGiaSuContext db, HocVienService hocVien, GiaSuService giaSu,
        IMemoryCache cache, INguoiDungHienTai nguoiDung)
    {
        _db = db;
        _hocVien = hocVien;
        _giaSu = giaSu;
        _cache = cache;
        _nguoiDung = nguoiDung;
    }

    public byte[] TaoMau(string loai)
    {
        KiemTraLoai(loai);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(loai == "HocVien" ? "HocVien" : "GiaSu");
        string[] cot = loai == "HocVien"
            ? new[] { "HoTen", "SDT", "DiaChi", "TenPhuHuynh", "SDTPhuHuynh", "TruongLop", "TrinhDo", "GhiChu" }
            : new[] { "HoTen", "SDT", "ChuyenMon", "Email", "HocVan", "KinhNghiem", "GhiChu" };
        for (int i = 0; i < cot.Length; i++)
        {
            ws.Cell(1, i + 1).SetValue(cot[i]);
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Column(i + 1).Style.NumberFormat.Format = "@";
        }
        ws.SheetView.FreezeRows(1);
        ws.Columns(1, cot.Length).AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public NhapExcelVM XemTruoc(string loai, Stream tep, string chuSoHuu)
    {
        _nguoiDung.YeuCauQuanLy();
        KiemTraLoai(loai);
        using var wb = new XLWorkbook(tep);
        var ws = wb.Worksheets.FirstOrDefault() ?? throw new InvalidDataException("Tệp Excel không có trang tính.");
        string[] cotMongDoi = loai == "HocVien"
            ? new[] { "HoTen", "SDT", "DiaChi", "TenPhuHuynh", "SDTPhuHuynh", "TruongLop", "TrinhDo", "GhiChu" }
            : new[] { "HoTen", "SDT", "ChuyenMon", "Email", "HocVan", "KinhNghiem", "GhiChu" };
        if (cotMongDoi.Where((ten, i) => !string.Equals(ws.Cell(1, i + 1).GetString().Trim(), ten, StringComparison.OrdinalIgnoreCase)).Any())
            throw new InvalidDataException("Tên hoặc thứ tự cột không đúng. Hãy tải và dùng tệp mẫu tương ứng.");
        int hangCuoi = ws.LastRowUsed()?.RowNumber() ?? 1;
        if (hangCuoi - 1 > GioiHanDong) throw new InvalidDataException("Tệp chỉ được có tối đa 1000 dòng dữ liệu.");
        var dong = new List<DongDuLieu>();
        var vm = new NhapExcelVM { LoaiHoSo = loai };
        var phones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dbPhones = loai == "HocVien"
            ? _db.HocViens.AsNoTracking().ToList().SelectMany(h => new[] { h.SDT, h.SDTPhuHuynh }).Where(s => s != null).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : _db.GiaSus.AsNoTracking().Select(g => g.SDT).Where(s => s != null).ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (int r = 2; r <= hangCuoi; r++)
        {
            var values = Enumerable.Range(1, loai == "HocVien" ? 8 : 7).Select(c => ws.Cell(r, c).GetFormattedString().Trim()).ToArray();
            if (values.All(string.IsNullOrWhiteSpace)) continue;
            var row = new DongDuLieu { SoDong = r, GiaTri = values };
            ValidateRow(loai, row, phones, dbPhones);
            dong.Add(row);
            vm.Dong.Add(new DongNhapExcelVM { SoDong = r, TomTat = values[0], Loi = row.Loi.ToList() });
        }

        vm.MaPhien = Guid.NewGuid().ToString("N");
        _cache.Set(Khoa(vm.MaPhien), new DuLieuCache(loai, chuSoHuu, dong), TimeSpan.FromMinutes(15));
        return vm;
    }

    public (int SoNhap, int SoBoQua) NhapCacDongHopLe(string maPhien, string chuSoHuu)
    {
        _nguoiDung.YeuCauQuanLy();
        if (string.IsNullOrWhiteSpace(maPhien) || !_cache.TryGetValue(Khoa(maPhien), out DuLieuCache duLieu) || duLieu.ChuSoHuu != chuSoHuu)
            throw new InvalidOperationException("Phiên xem trước đã hết hạn. Vui lòng tải lại tệp.");

        using var tx = _db.Database.BeginTransaction();
        int nhap = 0, boQua = 0;
        foreach (var row in duLieu.Dong.Where(x => x.Loi.Count == 0))
        {
            try
            {
                // Kiểm tra lại trùng CSDL trong transaction, tránh nhập trùng phát sinh sau lúc xem trước.
                if (TrungCSDL(duLieu.Loai, row.GiaTri)) { boQua++; continue; }
                if (duLieu.Loai == "HocVien")
                {
                    _hocVien.LuuHoSo(null, new HoSoHocVien
                    {
                        HoTen = row.GiaTri[0], SDT = RongNull(row.GiaTri[1]), DiaChi = RongNull(row.GiaTri[2]),
                        TenPhuHuynh = RongNull(row.GiaTri[3]), SDTPhuHuynh = RongNull(row.GiaTri[4]),
                        TruongLop = RongNull(row.GiaTri[5]), TrinhDo = RongNull(row.GiaTri[6]), GhiChu = RongNull(row.GiaTri[7])
                    });
                }
                else
                {
                    int? kinhNghiem = string.IsNullOrWhiteSpace(row.GiaTri[5]) ? null : int.Parse(row.GiaTri[5], CultureInfo.InvariantCulture);
                    _giaSu.LuuHoSo(null, new HoSoGiaSu
                    {
                        HoTen = row.GiaTri[0], SDT = row.GiaTri[1], ChuyenMon = row.GiaTri[2], Email = RongNull(row.GiaTri[3]),
                        HocVan = RongNull(row.GiaTri[4]), KinhNghiem = kinhNghiem, GhiChu = RongNull(row.GiaTri[6])
                    });
                }
                nhap++;
            }
            catch (ArgumentException)
            {
                boQua++;
            }
        }
        tx.Commit();
        _cache.Remove(Khoa(maPhien));
        return (nhap, boQua);
    }

    private bool TrungCSDL(string loai, string[] v) => loai == "HocVien"
        ? (!string.IsNullOrWhiteSpace(v[1]) && _db.HocViens.Any(h => h.SDT == v[1] || h.SDTPhuHuynh == v[1])) ||
          (!string.IsNullOrWhiteSpace(v[4]) && _db.HocViens.Any(h => h.SDT == v[4] || h.SDTPhuHuynh == v[4]))
        : _db.GiaSus.Any(g => g.SDT == v[1]);

    private static void ValidateRow(string loai, DongDuLieu row, HashSet<string> phones, HashSet<string> dbPhones)
    {
        var v = row.GiaTri;
        if (loai == "HocVien")
        {
            if (!HocVienValidator.ValidateHoTen(v[0], out var e)) row.Loi.Add("Họ tên: " + e);
            if (string.IsNullOrWhiteSpace(v[1]) && string.IsNullOrWhiteSpace(v[4])) row.Loi.Add("SĐT: Nhập ít nhất một số điện thoại.");
            foreach (int i in new[] { 1, 4 }) if (!string.IsNullOrWhiteSpace(v[i]) && !HocVienValidator.ValidateSDT(v[i], out e)) row.Loi.Add((i == 1 ? "SĐT" : "SĐT phụ huynh") + ": " + e);
            foreach (var (i, max, label) in new[] { (2, 200, "Địa chỉ"), (3, 100, "Tên phụ huynh"), (5, 150, "Trường/lớp"), (6, 50, "Trình độ"), (7, 500, "Ghi chú") })
                if (v[i].Length > max) row.Loi.Add($"{label}: tối đa {max} ký tự.");
            foreach (int i in new[] { 1, 4 }) AddDuplicate(v[i], i == 1 ? "SĐT" : "SĐT phụ huynh", phones, dbPhones, row.Loi);
        }
        else
        {
            if (!GiaSuValidator.ValidateHoTen(v[0], out var e)) row.Loi.Add("Họ tên: " + e);
            if (!GiaSuValidator.ValidateSDT(v[1], out e)) row.Loi.Add("SĐT: " + e);
            if (!GiaSuValidator.ValidateChuyenMon(v[2], out e)) row.Loi.Add("Chuyên môn: " + e);
            if (!string.IsNullOrWhiteSpace(v[3]) && (v[3].Length > 100 || !Regex.IsMatch(v[3], @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))) row.Loi.Add("Email: định dạng email không hợp lệ.");
            if (v[4].Length > 150) row.Loi.Add("Học vấn: tối đa 150 ký tự.");
            if (!string.IsNullOrWhiteSpace(v[5]) && (!int.TryParse(v[5], out int kn) || kn < 0 || kn > 60)) row.Loi.Add("Kinh nghiệm: nhập số nguyên từ 0 đến 60.");
            if (v[6].Length > 500) row.Loi.Add("Ghi chú: tối đa 500 ký tự.");
            AddDuplicate(v[1], "SĐT", phones, dbPhones, row.Loi);
        }
    }

    private static void AddDuplicate(string value, string label, HashSet<string> phones, HashSet<string> dbPhones, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        if (!phones.Add(value)) errors.Add($"{label}: trùng trong tệp.");
        if (dbPhones.Contains(value)) errors.Add($"{label}: đã tồn tại trong CSDL.");
    }

    private static string RongNull(string s) => string.IsNullOrWhiteSpace(s) ? null : s;
    private static void KiemTraLoai(string loai)
    {
        if (loai is not ("HocVien" or "GiaSu")) throw new ArgumentException("Loại hồ sơ không hợp lệ.");
    }
    private static string Khoa(string maPhien) => "NhapExcel:" + maPhien;
    private sealed class DongDuLieu { public int SoDong; public string[] GiaTri; public List<string> Loi = new(); }
    private sealed record DuLieuCache(string Loai, string ChuSoHuu, List<DongDuLieu> Dong);
}
