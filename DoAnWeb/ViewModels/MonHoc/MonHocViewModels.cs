using System;
using System.Collections.Generic;
using DoAnWeb.Data.Entities;

namespace DoAnWeb.ViewModels.MonHoc;


/// <summary>Danh mục môn học (dlgMonHoc.cs): danh sách bên trái, biểu mẫu bên phải.</summary>
public class MonHocVM
{
    public List<MonHocDanhMuc> DanhSach { get; set; } = new List<MonHocDanhMuc>();
    public Dictionary<string, int> SoLopTheoMon { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public int? MaMon { get; set; }
    public string TenMon { get; set; }
    public string LoaiMon { get; set; } = Common.Constants.LoaiMon.Chinh;
    /// <summary>Mỗi dòng một cấp, từ thấp đến cao (môn năng khiếu).</summary>
    public string CapDoText { get; set; }
    public bool NgungDay { get; set; }

    /// <summary>Số lớp đang dùng môn đang chọn.</summary>
    public int SoLopCuaMon { get; set; }

    public int SoLop(MonHocDanhMuc mon)
    {
        if (mon?.TenMon == null) return 0;
        return SoLopTheoMon.TryGetValue(mon.TenMon.Trim(), out int n) ? n : 0;
    }
}