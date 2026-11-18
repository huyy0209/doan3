namespace DoAnWeb.ViewModels.SaoLuu;

public sealed class SaoLuuIndexVM
{
    public IReadOnlyList<SaoLuuFileVM> Tep { get; set; } = Array.Empty<SaoLuuFileVM>();
    public string GioTuDong { get; set; }
    public string Loi { get; set; }
}
