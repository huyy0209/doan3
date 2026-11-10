using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DoAnWD.Models
{
    /// <summary>
    /// Danh mục môn học (bảng MonHoc, 05_NangCap_v5.sql).
    ///  - Môn chính: đánh giá bằng xếp loại theo điểm trung bình.
    ///  - Môn năng khiếu: đánh giá theo cấp độ; CapDo lưu danh sách cấp theo thứ tự, cách nhau bằng dấu ";".
    /// Lớp học vẫn lưu tên môn (LopHoc.MonHoc) nên đổi tên môn sẽ cập nhật theo.
    /// </summary>
    [Table("MonHoc")]
    public class MonHocDanhMuc
    {
        public const char NganCachCapDo = ';';

        [Key]
        public int MaMon { get; set; }

        [Required]
        [StringLength(50)]
        public string TenMon { get; set; }

        [Required]
        [StringLength(20)]
        public string LoaiMon { get; set; }

        [StringLength(500)]
        public string CapDo { get; set; }

        /// <summary>Ẩn khỏi danh sách khi ghép lớp mới; lớp cũ giữ nguyên</summary>
        public bool NgungDay { get; set; }

        [NotMapped]
        public bool LaNangKhieu => LoaiMon == Common.Constants.LoaiMon.NangKhieu;

        /// <summary>Các cấp độ theo thứ tự từ thấp đến cao (rỗng với môn chính)</summary>
        [NotMapped]
        public List<string> DanhSachCapDo =>
            !LaNangKhieu || string.IsNullOrWhiteSpace(CapDo)
                ? new List<string>()
                : CapDo.Split(NganCachCapDo).Select(c => c.Trim()).Where(c => c.Length > 0).ToList();

        public override string ToString() => TenMon;
    }
}
