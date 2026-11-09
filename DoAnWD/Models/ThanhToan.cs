using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWD.Models
{
    /// <summary>Một lần đóng học phí cho một lớp</summary>
    [Table("ThanhToan")]
    public partial class ThanhToan
    {
        [Key]
        public int MaTT { get; set; }

        public int MaLop { get; set; }

        public decimal SoTien { get; set; }

        public DateTime NgayDong { get; set; }

        [StringLength(30)]
        public string HinhThuc { get; set; }

        [StringLength(200)]
        public string GhiChu { get; set; }

        public virtual LopHoc LopHoc { get; set; }
    }
}
