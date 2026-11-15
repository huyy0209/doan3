namespace DoAnWeb.Infrastructure;

public interface IThongTinTrang
{
    int Trang { get; }
    int SoTrang { get; }
    int SoDong { get; }
    int TongSo { get; }
    int TuDong { get; }
    int DenDong { get; }
    string SapXep { get; }
    string Chieu { get; }
    Dictionary<string, string> ThamSo { get; }
}

public class KetQuaTrang<T> : IThongTinTrang
{
    public List<T> Items { get; set; } = new();
    public int Trang { get; set; }
    public int SoTrang { get; set; }
    public int SoDong { get; set; }
    public int TongSo { get; set; }
    public int TuDong { get; set; }
    public int DenDong { get; set; }
    public string SapXep { get; set; }
    public string Chieu { get; set; }
    public Dictionary<string, string> ThamSo { get; set; } = new();
}

public static class PhanTrang
{
    public const int SoDongMacDinh = 20;
    private static readonly int[] CacLuaChonSoDong = { 20, 50, 100 };

    public static KetQuaTrang<T> Tao<T>(
        IEnumerable<T> danhSach,
        int trang,
        int soDong,
        string sapXep = null,
        string chieu = null,
        IDictionary<string, string> thamSo = null)
    {
        ArgumentNullException.ThrowIfNull(danhSach);

        var items = danhSach.ToList();
        int tongSo = items.Count;
        if (!CacLuaChonSoDong.Contains(soDong))
            soDong = SoDongMacDinh;
        int soTrang = Math.Max(1, (int)Math.Ceiling(tongSo / (double)soDong));
        trang = Math.Clamp(trang, 1, soTrang);

        var thamSoTrang = thamSo == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(thamSo);
        thamSoTrang["trang"] = trang.ToString();
        thamSoTrang["soDong"] = soDong.ToString();
        if (!string.IsNullOrWhiteSpace(sapXep))
            thamSoTrang["sapXep"] = sapXep;
        if (chieu == "asc" || chieu == "desc")
            thamSoTrang["chieu"] = chieu;

        int tuDong = tongSo == 0 ? 0 : (trang - 1) * soDong + 1;
        int denDong = Math.Min(trang * soDong, tongSo);
        return new KetQuaTrang<T>
        {
            Items = items.Skip((trang - 1) * soDong).Take(soDong).ToList(),
            Trang = trang,
            SoTrang = soTrang,
            SoDong = soDong,
            TongSo = tongSo,
            TuDong = tuDong,
            DenDong = denDong,
            SapXep = sapXep,
            Chieu = chieu == "desc" ? "desc" : "asc",
            ThamSo = thamSoTrang
        };
    }

    public static Dictionary<string, string> TaoThamSoSapXep(
        IThongTinTrang trangHienTai, string sapXep)
    {
        var thamSo = new Dictionary<string, string>(trangHienTai.ThamSo);
        thamSo["trang"] = "1";
        thamSo["soDong"] = trangHienTai.SoDong.ToString();
        thamSo["sapXep"] = sapXep;
        thamSo["chieu"] = trangHienTai.SapXep == sapXep && trangHienTai.Chieu == "asc"
            ? "desc"
            : "asc";
        return thamSo;
    }

    public static string BieuTuongSapXep(IThongTinTrang trang, string sapXep)
    {
        if (trang.SapXep != sapXep)
            return "";
        return trang.Chieu == "desc" ? "▼" : "▲";
    }
}
