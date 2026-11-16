namespace DoAnWeb.Services;

public sealed class ThongBaoNen : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ThongBaoNen> _logger;

    public ThongBaoNen(IServiceScopeFactory scopeFactory, ILogger<ThongBaoNen> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime hienTai = DateTime.Now;
            DateTime lanChayTiep = hienTai.Date.AddDays(1).AddMinutes(10);
            try
            {
                await Task.Delay(lanChayTiep - hienTai, stoppingToken);
                using var scope = _scopeFactory.CreateScope();
                var thongBao = scope.ServiceProvider.GetRequiredService<ThongBaoService>();
                await thongBao.TaoNhacNoQuaHanAsync(stoppingToken);
                await thongBao.XoaThongBaoCuAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tạo thông báo nhắc nợ hoặc dọn thông báo cũ.");
            }
        }
    }
}
