using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWD.Models
{
    /// <summary>
    /// Tài khoản đăng nhập. VaiTro = "QuanLy" (toàn quyền) hoặc "GiaSu" (chỉ lớp của mình).
    /// Mật khẩu lưu dạng băm PBKDF2, không lưu mật khẩu gốc.
    /// </summary>
    [Table("TaiKhoan")]
    public partial class TaiKhoan
    {
        [Key]
        public int MaTK { get; set; }

        [Required]
        [StringLength(50)]
        public string TenDangNhap { get; set; }

        [Required]
        [StringLength(200)]
        public string MatKhauHash { get; set; }

        [Required]
        [StringLength(20)]
        public string VaiTro { get; set; }

        public int? MaGS { get; set; }

        public bool HoatDong { get; set; }

        public DateTime NgayTao { get; set; }

        /// <summary>Số lần nhập sai mật khẩu liên tiếp (06_NangCap_v6.sql)</summary>
        public int SoLanSai { get; set; }

        /// <summary>Tạm khóa đăng nhập đến thời điểm này sau nhiều lần nhập sai</summary>
        public DateTime? KhoaDen { get; set; }

        public virtual GiaSu GiaSu { get; set; }
    }
}
