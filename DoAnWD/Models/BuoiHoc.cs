using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWD.Models
{
    /// <summary>
    /// Một buổi học của một lớp: điểm danh + điểm bài test 15 phút cuối giờ (nếu có).
    /// Mỗi lớp chỉ có 1 bản ghi cho mỗi ngày.
    /// </summary>
    [Table("BuoiHoc")]
    public partial class BuoiHoc
    {
        [Key]
        public int MaBuoi { get; set; }

        public int MaLop { get; set; }

        [Column(TypeName = "date")]
        public DateTime NgayHoc { get; set; }

        [Required]
        [StringLength(30)]
        public string DiemDanh { get; set; }

        /// <summary>Điểm thang 10; null = hôm đó không kiểm tra</summary>
        public decimal? Diem { get; set; }

        [StringLength(300)]
        public string NhanXet { get; set; }

        public virtual LopHoc LopHoc { get; set; }
    }
}
