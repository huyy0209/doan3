using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DoAnWeb.Common;

namespace DoAnWeb.Data.Entities;

// Phần nghiệp vụ của các entity khác, chép từ DoAnWD/Models/*.cs (không ánh xạ vào CSDL).

public partial class GiaSu
{
    /// <summary>Gia sư có nhận dạy ca này không (CaNhanDay rỗng = mọi ca) – GS-06</summary>
    public bool NhanCa(string caHoc)
    {
        if (string.IsNullOrWhiteSpace(CaNhanDay) || string.IsNullOrEmpty(caHoc)) return true;
        string ten = Constants.CaHoc.TenNgan(caHoc);
        foreach (var c in CaNhanDay.Split(','))
            if (c.Trim() == ten) return true;
        return false;
    }
}

public partial class HocVien
{
    /// <summary>SĐT liên hệ: ưu tiên SĐT phụ huynh – HV-07</summary>
    [NotMapped]
    public string SDTLienHe => !string.IsNullOrWhiteSpace(SDTPhuHuynh) ? SDTPhuHuynh : SDT;
}

public partial class BuoiHoc
{
    /// <summary>Buổi học bù (trỏ tới buổi vắng gốc)</summary>
    [NotMapped]
    public bool LaHocBu => MaBuoiGoc.HasValue;
}

public partial class MonHocDanhMuc
{
    /// <summary>Ký tự ngăn cách các cấp độ trong cột CapDo</summary>
    public const char NganCachCapDo = ';';

    [NotMapped]
    public bool LaNangKhieu => LoaiMon == Constants.LoaiMon.NangKhieu;

    /// <summary>Danh sách cấp độ từ thấp đến cao (chỉ môn năng khiếu)</summary>
    [NotMapped]
    public List<string> DanhSachCapDo =>
        !LaNangKhieu || string.IsNullOrWhiteSpace(CapDo)
            ? new List<string>()
            : CapDo.Split(NganCachCapDo).Select(c => c.Trim()).Where(c => c.Length > 0).ToList();

    public override string ToString() => TenMon;
}
