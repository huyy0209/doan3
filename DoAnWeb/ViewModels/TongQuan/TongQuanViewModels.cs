using System.Collections.Generic;
using DoAnWeb.Services;

namespace DoAnWeb.ViewModels.TongQuan;


public class TongQuanIndexViewModel
{
    public TongQuanDuLieu DuLieu { get; set; }
    public HomNayDuLieu HomNay { get; set; }
    public Dictionary<string, int> SoLopTheoCa { get; set; }
    public string TenQuanLy { get; set; }
    public string CaHienTai { get; set; }
    public List<ViecTongQuanViewModel> Viec { get; set; }
}

public class ViecTongQuanViewModel
{
    public ViecCanLam Viec { get; set; }
    public string Url { get; set; }
}