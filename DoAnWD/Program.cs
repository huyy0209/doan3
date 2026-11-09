using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DoAnWD.Models;

namespace DoAnWD
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Kiểm tra kết nối CSDL trước khi mở giao diện chính
            if (!KiemTraKetNoiCSDL())
                return;

            // Đăng nhập; "Đăng xuất" trong ứng dụng quay lại màn hình này
            bool vuaTaoAdmin;
            try
            {
                vuaTaoAdmin = new BLL.Services.TaiKhoanService().DamBaoTaiKhoanMacDinh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "CSDL chưa được nâng cấp lên phiên bản mới.\n\n" +
                    "Hãy chạy script Database\\NangCap_v2.sql trong SQL Server Management Studio rồi mở lại ứng dụng.\n\n" +
                    "Chi tiết: " + ex.GetBaseException().Message,
                    "Cần nâng cấp CSDL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            while (true)
            {
                using (var dangNhap = new frmDangNhap(vuaTaoAdmin))
                {
                    if (dangNhap.ShowDialog() != DialogResult.OK) return;
                }
                vuaTaoAdmin = false;

                var trangChu = new TrangChu();
                Application.Run(trangChu);
                if (!trangChu.DangXuat) return;
                BLL.Services.PhienDangNhap.KetThuc();
            }
        }

        /// <summary>
        /// Kiểm tra CSDL QuanLyGiaSu có tồn tại và kết nối được không.
        /// Nếu lỗi: hiển thị hướng dẫn chạy script Database/QuanLyGiaSu.sql
        /// và sửa connection string trong App.config.
        /// </summary>
        private static bool KiemTraKetNoiCSDL()
        {
            try
            {
                using (var db = new GiaSuContextDB())
                {
                    if (db.Database.Exists())
                        return true;
                }

                MessageBox.Show(
                    "Chưa tìm thấy CSDL 'QuanLyGiaSu'.\n\n" +
                    "Hãy chạy script Database\\QuanLyGiaSu.sql trong SQL Server Management Studio, " +
                    "sau đó mở lại ứng dụng.",
                    "Chưa có CSDL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không kết nối được SQL Server.\n\n" +
                    "Kiểm tra lại 'data source' trong App.config (mặc định là '.', " +
                    "nếu dùng SQL Express thì đổi thành '.\\SQLEXPRESS').\n\n" +
                    "Chi tiết: " + ex.GetBaseException().Message,
                    "Lỗi kết nối CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }
    }
}
