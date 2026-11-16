namespace DoAnWeb.Data.Entities;

public partial class ThongBao
{
    public int MaTB { get; set; }
    public int MaTK { get; set; }
    public string TieuDe { get; set; }
    public string NoiDung { get; set; }
    public string DuongDan { get; set; }
    public bool DaDoc { get; set; }
    public DateTime ThoiGian { get; set; }
    public virtual TaiKhoan TaiKhoan { get; set; }
}
