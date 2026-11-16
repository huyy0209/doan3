using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Infrastructure;
using DoAnWeb.ViewModels.NhatKyVM;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Services;

public sealed class NhatKyService
{
    private readonly QuanLyGiaSuContext _db;
    private readonly IHttpContextAccessor _http;

    public NhatKyService(QuanLyGiaSuContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public NhatKyIndexVM TaoTrang(
        DateTime? tuNgay, DateTime? denNgay, string nguoi, string hanhDong, string bang,
        int trang, int soDong)
    {
        var q = Loc(tuNgay, denNgay, nguoi, hanhDong, bang);
        var danhSach = q.OrderByDescending(n => n.ThoiGian).ThenByDescending(n => n.MaNK).ToList();
        var thamSo = new Dictionary<string, string>();
        if (tuNgay.HasValue) thamSo["tuNgay"] = tuNgay.Value.ToString("yyyy-MM-dd");
        if (denNgay.HasValue) thamSo["denNgay"] = denNgay.Value.ToString("yyyy-MM-dd");
        if (!string.IsNullOrWhiteSpace(nguoi)) thamSo["nguoi"] = nguoi;
        if (!string.IsNullOrWhiteSpace(hanhDong)) thamSo["hanhDong"] = hanhDong;
        if (!string.IsNullOrWhiteSpace(bang)) thamSo["bang"] = bang;

        return new NhatKyIndexVM
        {
            KetQua = PhanTrang.Tao(danhSach, trang, soDong, thamSo: thamSo),
            TuNgay = tuNgay,
            DenNgay = denNgay,
            Nguoi = nguoi,
            HanhDong = hanhDong,
            Bang = bang,
            DanhSachNguoi = _db.NhatKys.AsNoTracking().Where(n => n.TenDangNhap != null)
                .Select(n => n.TenDangNhap).Distinct().OrderBy(n => n).ToList(),
            DanhSachBang = _db.NhatKys.AsNoTracking().Where(n => n.Bang != null)
                .Select(n => n.Bang).Distinct().OrderBy(n => n).ToList()
        };
    }

    public IReadOnlyList<NhatKy> Xuat(
        DateTime? tuNgay, DateTime? denNgay, string nguoi, string hanhDong, string bang) =>
        Loc(tuNgay, denNgay, nguoi, hanhDong, bang)
            .OrderByDescending(n => n.ThoiGian).ThenByDescending(n => n.MaNK).ToList();

    public NhatKyChiTietTrangVM ChiTiet(int maNK)
    {
        var muc = _db.NhatKys.AsNoTracking().SingleOrDefault(n => n.MaNK == maNK);
        if (muc == null) return null;

        var vm = new NhatKyChiTietTrangVM { NhatKy = muc };
        if (string.IsNullOrWhiteSpace(muc.ChiTiet)) return vm;
        try
        {
            using var json = JsonDocument.Parse(muc.ChiTiet);
            foreach (var truong in json.RootElement.EnumerateObject())
            {
                string cu = null, moi = null;
                if (truong.Value.ValueKind == JsonValueKind.Object)
                {
                    if (truong.Value.TryGetProperty("cu", out var cuValue)) cu = HienThi(cuValue);
                    if (truong.Value.TryGetProperty("moi", out var moiValue)) moi = HienThi(moiValue);
                    if (NhatKyBaoMat.LaTruongNhayCam(truong.Name))
                    {
                        cu = NhatKyBaoMat.Che(cu);
                        moi = NhatKyBaoMat.Che(moi);
                    }
                }
                vm.CacTruong.Add(new NhatKyTruongVM { Ten = truong.Name, Cu = cu, Moi = moi });
            }
        }
        catch (JsonException)
        {
            vm.JsonKhongHopLe = muc.ChiTiet;
        }
        return vm;
    }

    public async Task GhiSuKienAsync(string hanhDong, string tenDangNhap, int? maTK = null)
    {
        var muc = new NhatKy
        {
            ThoiGian = DateTime.Now,
            MaTK = maTK,
            TenDangNhap = GioiHan(tenDangNhap, 50),
            HanhDong = hanhDong,
            DiaChiIP = GioiHan(_http.HttpContext?.Connection.RemoteIpAddress?.ToString(), 45)
        };
        _db.NhatKys.Add(muc);
        await _db.SaveChangesAsync();
    }

    private IQueryable<NhatKy> Loc(
        DateTime? tuNgay, DateTime? denNgay, string nguoi, string hanhDong, string bang)
    {
        var q = _db.NhatKys.AsNoTracking().AsQueryable();
        if (tuNgay.HasValue) q = q.Where(n => n.ThoiGian >= tuNgay.Value.Date);
        if (denNgay.HasValue)
        {
            var ngaySau = denNgay.Value.Date.AddDays(1);
            q = q.Where(n => n.ThoiGian < ngaySau);
        }
        if (!string.IsNullOrWhiteSpace(nguoi)) q = q.Where(n => n.TenDangNhap == nguoi);
        if (!string.IsNullOrWhiteSpace(hanhDong)) q = q.Where(n => n.HanhDong == hanhDong);
        if (!string.IsNullOrWhiteSpace(bang)) q = q.Where(n => n.Bang == bang);
        return q;
    }

    private static string HienThi(JsonElement value) =>
        value.ValueKind == JsonValueKind.Null ? null :
        value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();

    private static string GioiHan(string value, int doDai) =>
        string.IsNullOrEmpty(value) ? null : value.Substring(0, Math.Min(value.Length, doDai));
}
