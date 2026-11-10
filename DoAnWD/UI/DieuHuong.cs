using System;
using System.Windows.Forms;

namespace DoAnWD.UI
{
    /// <summary>
    /// Chuyển trang từ bên trong một trang (ví dụ bấm ô ở Tổng quan => trang Gia sư đã lọc sẵn).
    /// TrangChu đăng ký hàm mở trang lúc khởi động; trang khác chỉ cần gọi DieuHuong.Mo&lt;T&gt;()
    /// rồi dùng trang trả về để lọc / chọn sẵn.
    /// </summary>
    public static class DieuHuong
    {
        /// <summary>Do TrangChu gán: mở trang theo kiểu, đánh dấu menu, trả về trang đã mở</summary>
        internal static Func<Type, Form> MoTrang { get; set; }

        /// <summary>Mở trang T; trả về null nếu người dùng không có quyền vào trang đó</summary>
        public static T Mo<T>() where T : Form
        {
            return MoTrang?.Invoke(typeof(T)) as T;
        }
    }
}
