using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWD.Models
{
    /// <summary>
    /// Một buổi học của một lớp: điểm danh + điểm bài test 15 phút cuối giờ (nếu có).
    ///  - Buổi thường: mỗi lớp tối đa 1 bản ghi / ngày; ca và gia sư lấy theo lớp.
    ///  - Buổi học bù (MaBuoiGoc có giá trị): bù cho một buổi vắng của cùng lớp,
    ///    học vào ngày + ca khác (CaHoc), có thể với gia sư khác (MaGSDay). Bổ sung ở 04_NangCap_v4.sql.
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

        /// <summary>Ca của buổi học bù; null với buổi thường (theo ca của lớp)</summary>
        [StringLength(50)]
        public string CaHoc { get; set; }

        /// <summary>Gia sư dạy buổi học bù; null với buổi thường (gia sư của lớp)</summary>
        public int? MaGSDay { get; set; }

        /// <summary>Buổi vắng mà buổi này học bù cho; null = buổi thường</summary>
        public int? MaBuoiGoc { get; set; }

        /// <summary>Buổi này là bài thi lên cấp (môn năng khiếu). Bổ sung ở 05_NangCap_v5.sql.</summary>
        public bool ThiLenCap { get; set; }

        /// <summary>Cấp thi lên; đạt khi điểm từ 8 trở lên</summary>
        [StringLength(50)]
        public string CapDoThi { get; set; }

        [NotMapped]
        public bool LaHocBu => MaBuoiGoc.HasValue;

        public virtual LopHoc LopHoc { get; set; }
    }
}
