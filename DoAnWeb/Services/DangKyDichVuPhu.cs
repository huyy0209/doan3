using Microsoft.Extensions.DependencyInjection;

namespace DoAnWeb.Services
{
    /// <summary>Đăng ký các dịch vụ nghiệp vụ phụ của ứng dụng web.</summary>
    public static class DangKyDichVuPhu
    {
        public static IServiceCollection AddDichVuPhu(this IServiceCollection s)
        {
            s.AddScoped<HocTapService>();
            s.AddScoped<HocPhiService>();
            s.AddScoped<MonHocService>();
            s.AddScoped<TongQuanService>();
            s.AddScoped<ThongBaoService>();
            s.AddScoped<CaiDatService>();
            s.AddScoped<ThongKeService>();
            s.AddScoped<BaoCaoService>();
            s.AddScoped<HomNayService>();
            // Xuất Excel .xlsx (đề xuất #4) – không giữ trạng thái giữa các lần gọi
            s.AddScoped<XuatExcelService>();
            return s;
        }
    }
}
