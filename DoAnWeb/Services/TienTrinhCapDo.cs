using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;

namespace DoAnWeb.Services;


/// <summary>Xếp loại học tập dùng chung (HT-08), chép từ DoAnWD/BLL/Services/HocTapService.cs:35-42</summary>
public static class DanhGia
{
    /// <summary>Xếp loại theo điểm trung bình thang 10</summary>
    public static string XepLoai(decimal? diem)
    {
        if (!diem.HasValue) return "";
        if (diem.Value >= 8m) return "Giỏi";
        if (diem.Value >= 6.5m) return "Khá";
        if (diem.Value >= 5m) return "Trung bình";
        return "Yếu";
    }
}

/// <summary>Kết quả một bài thi lên cấp (DoAnWD/BLL/Services/MonHocService.cs:134-142)</summary>
public class BaiThiLenCap
{
    public DateTime Ngay { get; set; }
    public string CapThi { get; set; }
    public decimal? Diem { get; set; }
    public bool Dat => Diem.HasValue && Diem.Value >= Constants.LoaiMon.DiemDatThiLenCap;
    public override string ToString() =>
        $"{Ngay:dd/MM/yyyy}: thi lên {CapThi} - " + (Diem.HasValue ? $"{Diem:0.##} điểm, {(Dat ? "đạt" : "chưa đạt")}" : "chưa có điểm");
}

/// <summary>
/// Cấp độ hiện tại của một lớp năng khiếu (HT-07), chép từ DoAnWD/BLL/Services/MonHocService.cs:145-177.
/// Cấp hiện tại = cấp cao nhất trong (cấp đầu vào, các cấp đã thi đạt); không bao giờ tụt cấp.
/// </summary>
public class TienTrinhCapDo
{
    public List<string> CacCap { get; set; } = new List<string>();
    public string CapDauVao { get; set; }
    public string CapHienTai { get; set; }
    public string CapKeTiep { get; set; }
    public List<BaiThiLenCap> BaiThi { get; set; } = new List<BaiThiLenCap>();

    /// <param name="denNgay">Chỉ tính bài thi trước ngày này (null = tất cả)</param>
    public static TienTrinhCapDo Tinh(IList<string> cacCap, string capDauVao, IEnumerable<BuoiHoc> buoi, DateTime? denNgay = null)
    {
        var kq = new TienTrinhCapDo { CacCap = cacCap.ToList(), CapDauVao = capDauVao };
        if (kq.CacCap.Count == 0) return kq;

        int viTri = Math.Max(0, kq.CacCap.FindIndex(c => string.Equals(c, capDauVao, StringComparison.OrdinalIgnoreCase)));
        kq.BaiThi = buoi.Where(b => b.ThiLenCap && !string.IsNullOrEmpty(b.CapDoThi) && (!denNgay.HasValue || b.NgayHoc.Date < denNgay.Value.Date))
            .OrderBy(b => b.NgayHoc)
            .Select(b => new BaiThiLenCap { Ngay = b.NgayHoc, CapThi = b.CapDoThi, Diem = b.Diem })
            .ToList();
        foreach (var t in kq.BaiThi.Where(t => t.Dat))
        {
            int i = kq.CacCap.FindIndex(c => string.Equals(c, t.CapThi, StringComparison.OrdinalIgnoreCase));
            if (i > viTri) viTri = i;
        }
        kq.CapHienTai = kq.CacCap[viTri];
        kq.CapKeTiep = viTri + 1 < kq.CacCap.Count ? kq.CacCap[viTri + 1] : null;
        return kq;
    }
}