using System;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DoAnWeb.Data;
using DoAnWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DoAnWeb.Infrastructure;


/// <summary>
/// TK-08, TK-17: cookie đăng nhập sống 8 giờ nên mỗi request phải kiểm lại bảng TaiKhoan –
/// tài khoản bị xóa / khóa / đổi vai trò thì cookie cũ bị từ chối ngay.
/// Kết quả được cache ngắn theo MaTK để không truy vấn CSDL ở mọi request.
/// </summary>
public static class KiemTraPhienDangNhap
{
    /// <summary>Thời gian cache trạng thái tài khoản – khóa tài khoản có hiệu lực chậm nhất sau chừng này</summary>
    public static readonly TimeSpan ThoiGianCache = TimeSpan.FromSeconds(30);

    /// <summary>Trạng thái tài khoản lưu trong cache (null trong cache = tài khoản không còn)</summary>
    private sealed record TrangThaiTK(bool HoatDong, string VaiTro);

    private static string Khoa(int maTK) => "PhienTK:" + maTK;

    /// <summary>Xóa cache của một tài khoản (gọi khi khóa / mở khóa / đặt lại mật khẩu) để thay đổi có hiệu lực ngay</summary>
    public static void XoaCache(IMemoryCache cache, int maTK) => cache?.Remove(Khoa(maTK));

    /// <summary>
    /// Principal trong cookie còn hợp lệ không? Không còn tài khoản, đã khóa, hoặc vai trò khác claim → false.
    /// </summary>
    public static bool HopLe(ClaimsPrincipal user, QuanLyGiaSuContext db, IMemoryCache cache)
    {
        if (!int.TryParse(user?.FindFirst(LoaiClaim.MaTK)?.Value, out int maTK) || maTK <= 0)
            return false;

        var tt = cache.GetOrCreate(Khoa(maTK), e =>
        {
            e.AbsoluteExpirationRelativeToNow = ThoiGianCache;
            return db.TaiKhoans.Where(t => t.MaTK == maTK)
                .Select(t => new { t.HoatDong, t.VaiTro })
                .AsEnumerable()
                .Select(t => new TrangThaiTK(t.HoatDong, t.VaiTro))
                .FirstOrDefault();
        });

        return tt != null && tt.HoatDong &&
               string.Equals(tt.VaiTro, user.FindFirst(ClaimTypes.Role)?.Value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Như <see cref="HopLe"/> nhưng CSDL lỗi (mất kết nối…) thì ghi log và coi là không hợp lệ,
    /// để request chuyển về trang đăng nhập gọn thay vì lỗi 500 (kể cả khi bấm Đăng xuất).
    /// </summary>
    public static bool HopLeHoacLoi(ClaimsPrincipal user, QuanLyGiaSuContext db, IMemoryCache cache, ILogger logger)
    {
        try { return HopLe(user, db, cache); }
        catch (DbException ex)
        {
            logger?.LogError(ex, "Không kiểm tra được phiên đăng nhập vì lỗi CSDL");
            return false;
        }
    }

    /// <summary>Gắn vào CookieAuthenticationEvents.OnValidatePrincipal</summary>
    public static async Task KiemTra(CookieValidatePrincipalContext context)
    {
        var dv = context.HttpContext.RequestServices;
        var logger = dv.GetRequiredService<ILoggerFactory>().CreateLogger("KiemTraPhienDangNhap");
        var cache = dv.GetRequiredService<IMemoryCache>();
        var moc = dv.GetRequiredService<MocKhoiPhucPhien>().RestoredUtc;
        var maTKClaim = context.Principal?.FindFirst(LoaiClaim.MaTK)?.Value;
        var issuedUtc = context.Properties?.IssuedUtc;
        var issuedPrecisely = context.Principal?.FindFirst(LoaiClaim.PhatHanhPhienUtc)?.Value;
        // CookieAuthentication serializes IssuedUtc to whole seconds. The signed claim preserves
        // sub-second order so a login immediately after restore is not mistaken for an old ticket.
        bool daPhatHanhTruocMoc = !issuedUtc.HasValue ||
            !DateTimeOffset.TryParse(issuedPrecisely, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out var phatHanhUtc) || phatHanhUtc < moc;
        if (moc.HasValue && daPhatHanhTruocMoc)
        {
            if (int.TryParse(maTKClaim, out int maTK)) XoaCache(cache, maTK);
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }
        if (HopLeHoacLoi(context.Principal, dv.GetRequiredService<QuanLyGiaSuContext>(), cache, logger))
            return;

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}