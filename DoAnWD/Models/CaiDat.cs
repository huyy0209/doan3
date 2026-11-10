using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWD.Models
{
    /// <summary>Cài đặt dạng khóa - giá trị (thông tin trung tâm...). Bảng CaiDat, 06_NangCap_v6.sql.</summary>
    [Table("CaiDat")]
    public class CaiDat
    {
        [Key]
        [StringLength(50)]
        public string Khoa { get; set; }

        [StringLength(500)]
        public string GiaTri { get; set; }
    }
}
