using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Data.SqlClient;

namespace DoAnWeb.E2E;

/// <summary>
/// Chạy DoAnWeb.dll thật bằng Kestrel trong một tiến trình riêng, cổng trống do hệ điều hành cấp,
/// chuỗi kết nối trỏ CSDL QuanLyGiaSu_Test (ghi đè qua biến môi trường). KHÔNG BAO GIỜ dùng QuanLyGiaSu thật.
/// </summary>
public sealed class MayChuWeb : IDisposable
{
    private readonly Process _tienTrinh;
    private readonly StringBuilder _nhatKy = new();

    /// <summary>Địa chỉ gốc, ví dụ http://127.0.0.1:54321</summary>
    public string DiaChi { get; }

    private MayChuWeb(Process tienTrinh, string diaChi)
    {
        _tienTrinh = tienTrinh;
        DiaChi = diaChi;
    }

    /// <summary>Nhật ký stdout/stderr của web, đính vào thông báo lỗi khi web không lên được.</summary>
    public string NhatKy { get { lock (_nhatKy) return _nhatKy.ToString(); } }

    public static async Task<MayChuWeb> KhoiDong(string chuoiKetNoi)
    {
        // Chốt an toàn như WebAppFactory: chỉ chấp nhận CSDL QuanLyGiaSu_Test.
        string ten = new SqlConnectionStringBuilder(chuoiKetNoi).InitialCatalog;
        if (!DoAnWeb.Tests.TestDb.LaCsdlTest(ten))
            throw new InvalidOperationException($"E2E trỏ nhầm CSDL '{ten}' – chỉ được dùng CSDL test (QuanLyGiaSu_Test*).");

        string dll = TimDoAnWebDll(out string thuMucWeb);
        int cong = LayCongTrong();
        string diaChi = $"http://127.0.0.1:{cong}";

        var psi = new ProcessStartInfo("dotnet", $"\"{dll}\"")
        {
            WorkingDirectory = thuMucWeb,           // để tìm wwwroot / appsettings.json
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        psi.Environment["ASPNETCORE_URLS"] = diaChi;
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        psi.Environment["ConnectionStrings__QuanLyGiaSu"] = chuoiKetNoi;
        // Kịch bản E2E đăng nhập nhiều lần liên tiếp: nới giới hạn đăng nhập sai (G3) để không bị 429 giữa chừng
        psi.Environment["BaoMat__GioiHanDangNhap__SoLan"] = "1000";

        var tienTrinh = Process.Start(psi)!;
        var may = new MayChuWeb(tienTrinh, diaChi);
        tienTrinh.OutputDataReceived += (_, e) => may.Ghi(e.Data);
        tienTrinh.ErrorDataReceived += (_, e) => may.Ghi(e.Data);
        tienTrinh.BeginOutputReadLine();
        tienTrinh.BeginErrorReadLine();

        try { await may.ChoSanSang(); }
        catch { may.Dispose(); throw; }
        return may;
    }

    private void Ghi(string dong)
    {
        if (dong == null) return;
        lock (_nhatKy) _nhatKy.AppendLine(dong);
    }

    private async Task ChoSanSang()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var han = DateTime.UtcNow.AddSeconds(120);
        while (DateTime.UtcNow < han)
        {
            if (_tienTrinh.HasExited)
                throw new InvalidOperationException($"Web thoát sớm (mã {_tienTrinh.ExitCode}):\n{NhatKy}");
            try
            {
                var kq = await http.GetAsync(DiaChi + "/TaiKhoan/DangNhap");
                if (kq.StatusCode == HttpStatusCode.OK) return;
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) { }
            await Task.Delay(500);
        }
        throw new TimeoutException($"Web không sẵn sàng sau 120 giây:\n{NhatKy}");
    }

    /// <summary>
    /// Tìm DoAnWeb.dll đã build cùng cấu hình với project E2E
    /// (…/DoAnWeb.E2E/bin/Debug/net8.0 → …/DoAnWeb/bin/Debug/net8.0).
    /// </summary>
    private static string TimDoAnWebDll(out string thuMucWeb)
    {
        var thuMucBin = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        string khung = thuMucBin.Name;                       // net8.0
        string cauHinh = thuMucBin.Parent!.Name;             // Debug | Release
        var goc = thuMucBin.Parent!.Parent!.Parent!.Parent!; // thư mục solution
        thuMucWeb = Path.Combine(goc.FullName, "DoAnWeb");
        string dll = Path.Combine(thuMucWeb, "bin", cauHinh, khung, "DoAnWeb.dll");
        if (!File.Exists(dll))
            throw new FileNotFoundException("Chưa build DoAnWeb – chạy dotnet build DoAnWeb.E2E trước.", dll);
        return dll;
    }

    private static int LayCongTrong()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        try { return ((IPEndPoint)l.LocalEndpoint).Port; }
        finally { l.Stop(); }
    }

    public void Dispose()
    {
        try
        {
            if (!_tienTrinh.HasExited) _tienTrinh.Kill(entireProcessTree: true);
        }
        catch { /* đã thoát */ }
        _tienTrinh.Dispose();
    }
}
