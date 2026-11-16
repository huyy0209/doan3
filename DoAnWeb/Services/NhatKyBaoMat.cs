using System;

namespace DoAnWeb.Services;

/// <summary>Nhận diện và che các trường liên hệ, định danh trong nhật ký.</summary>
internal static class NhatKyBaoMat
{
    private static readonly string[] TuKhoaNhayCam =
    {
        "SoDienThoai", "SDT", "DienThoai", "Email", "DiaChi", "CCCD", "CMND"
    };

    public static bool LaTruongNhayCam(string tenTruong) =>
        !string.IsNullOrEmpty(tenTruong) && Array.Exists(TuKhoaNhayCam,
            tuKhoa => tenTruong.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase));

    public static string Che(string giaTri)
    {
        if (giaTri == null) return null;
        if (giaTri.Length <= 3) return "***";
        return new string('*', giaTri.Length - 3) + giaTri.Substring(giaTri.Length - 3);
    }
}
