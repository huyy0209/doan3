namespace DoAnWD.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("HocVien")]
    public partial class HocVien
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HocVien()
        {
            LopHocs = new HashSet<LopHoc>();
        }

        [Key]
        public int MaHV { get; set; }

        [StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(20)]
        public string SDT { get; set; }

        [StringLength(200)]
        public string DiaChi { get; set; }

        // ----- Hồ sơ (bổ sung ở 02_NangCap_v2.sql) -----
        [StringLength(100)]
        public string TenPhuHuynh { get; set; }

        [StringLength(20)]
        public string SDTPhuHuynh { get; set; }

        /// <summary>Trường / khối lớp, ví dụ "TH Lê Văn Tám, lớp 4"</summary>
        [StringLength(150)]
        public string TruongLop { get; set; }

        /// <summary>Trình độ: Vỡ lòng, Căn bản, Sơ cấp...</summary>
        [StringLength(50)]
        public string TrinhDo { get; set; }

        [StringLength(500)]
        public string GhiChu { get; set; }

        /// <summary>Số điện thoại để liên hệ: ưu tiên phụ huynh</summary>
        [NotMapped]
        public string SDTLienHe => !string.IsNullOrWhiteSpace(SDTPhuHuynh) ? SDTPhuHuynh : SDT;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LopHoc> LopHocs { get; set; }
    }
}
