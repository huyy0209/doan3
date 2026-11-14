using System;
using System.Threading;
using System.Threading.Tasks;
using DoAnWeb.Common;
using DoAnWeb.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DoAnWeb.Services
{
    /// <summary>
    /// LH-24: tự cập nhật trạng thái lớp theo thời gian + đồng bộ trạng thái gia sư, mỗi 60 giây
    /// (thay timer của TrangChu – DoAnWD/UI/TrangChu.cs:291, Constants.Timer.AutoUpdateInterval).
    /// </summary>
    public class CapNhatTrangThaiNen : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<CapNhatTrangThaiNen> _log;

        public CapNhatTrangThaiNen(IServiceScopeFactory scopes, ILogger<CapNhatTrangThaiNen> log)
        {
            _scopes = scopes;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var chuKy = TimeSpan.FromMilliseconds(Constants.Timer.AutoUpdateInterval);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopes.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<QuanLyGiaSuContext>();
                    // Tác vụ hệ thống: không cần quyền quản lý (AutoUpdateStatus không gọi YeuCauQuanLy)
                    var service = new LopHocService(db, new NguoiDungCoDinh());
                    int n = service.AutoUpdateStatus();
                    if (n > 0) _log.LogInformation("Đã tự cập nhật trạng thái {SoLop} lớp học.", n);
                }
                catch (Exception ex)
                {
                    // Không để lỗi tạm thời (mất kết nối CSDL…) làm dừng tác vụ nền
                    _log.LogWarning(ex, "Lỗi khi tự cập nhật trạng thái lớp.");
                }
                try { await Task.Delay(chuKy, stoppingToken); } catch (TaskCanceledException) { }
            }
        }
    }
}
