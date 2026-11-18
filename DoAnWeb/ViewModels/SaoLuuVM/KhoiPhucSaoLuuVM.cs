using System.ComponentModel.DataAnnotations;

namespace DoAnWeb.ViewModels.SaoLuu;

public sealed class KhoiPhucSaoLuuVM
{
    public string TenTep { get; set; }
    [Required] public string XacNhanTenTep { get; set; }
    [Required, DataType(DataType.Password)] public string MatKhauHienTai { get; set; }
}
