using DoAnWeb.Services.Nen;
using System;
using System.Threading;
using System.Threading.Tasks;
using DoAnWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DoAnWeb.Services.Nen;

public sealed class NhatKyDonDepNen : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<NhatKyDonDepNen> _log;
    private readonly int _giuSoNgay;

    public NhatKyDonDepNen(
        IServiceScopeFactory scopes, ILogger<NhatKyDonDepNen> log, IConfiguration cauHinh)
    {
        _scopes = scopes;
        _log = log;
        _giuSoNgay = Math.Max(1, cauHinh.GetValue("NhatKy:GiuSoNgay", 365));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<QuanLyGiaSuContext>();
                int daXoa = await db.NhatKys
                    .Where(n => n.ThoiGian < DateTime.Now.AddDays(-_giuSoNgay))
                    .ExecuteDeleteAsync(stoppingToken);
                if (daXoa > 0) _log.LogInformation("Đã xóa {SoDong} nhật ký cũ hơn {SoNgay} ngày.", daXoa, _giuSoNgay);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Lỗi khi dọn nhật ký thao tác cũ.");
            }

            try { await Task.Delay(TimeSpan.FromDays(1), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
