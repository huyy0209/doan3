using DoAnWeb.Services.Nen;
using System.Globalization;

namespace DoAnWeb.Services.Nen;

public sealed class TuSaoLuuNen : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _cauHinh;
    private readonly ILogger<TuSaoLuuNen> _logger;

    public TuSaoLuuNen(IServiceScopeFactory scopeFactory, IConfiguration cauHinh, ILogger<TuSaoLuuNen> logger)
    {
        _scopeFactory = scopeFactory;
        _cauHinh = cauHinh;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var gioCauHinh = _cauHinh["SaoLuu:GioTuDong"] ?? "23:00";
        if (string.Equals(gioCauHinh, "Tat", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Đã tắt lịch tự sao lưu.");
            return;
        }

        if (!TimeOnly.TryParseExact(gioCauHinh, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var gio))
        {
            _logger.LogError("SaoLuu:GioTuDong không hợp lệ: {GioTuDong}. Dùng HH:mm hoặc Tat.", gioCauHinh);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var hienTai = DateTime.Now;
            var lanChayTiep = hienTai.Date.Add(gio.ToTimeSpan());
            if (lanChayTiep <= hienTai)
                lanChayTiep = lanChayTiep.AddDays(1);

            try
            {
                await Task.Delay(lanChayTiep - hienTai, stoppingToken);
                using var scope = _scopeFactory.CreateScope();
                var saoLuu = scope.ServiceProvider.GetRequiredService<SaoLuuService>();
                var tenTep = await saoLuu.SaoLuuAsync(stoppingToken);
                _logger.LogInformation("Tự sao lưu CSDL thành công: {TenTep}", tenTep);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tự sao lưu CSDL thất bại.");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var thongBao = scope.ServiceProvider.GetRequiredService<ThongBaoService>();
                    await thongBao.TaoThongBaoSaoLuuThatBaiAsync(stoppingToken);
                }
                catch (Exception loiThongBao)
                {
                    _logger.LogError(loiThongBao, "Không thể gửi thông báo về lần sao lưu tự động thất bại.");
                }
            }
        }
    }
}
