namespace DoAnWD.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GiaSu")]
    public partial class GiaSu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GiaSu()
        {
            LopHocs = new HashSet<LopHoc>();
        }

        [Key]
        public int MaGS { get; set; }

        [StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(20)]
        public string SDT { get; set; }

        [StringLength(100)]
        public string ChuyenMon { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; }

        // ----- Hồ sơ (bổ sung ở NangCap_v3.sql) -----
        [StringLength(100)]
        public string Email { get; set; }

        /// <summary>Bằng cấp / trình độ, ví dụ "SV năm 4 HUTECH", "Kiện tướng FIDE"</summary>
        [StringLength(150)]
        public string HocVan { get; set; }

        /// <summary>Số năm kinh nghiệm giảng dạy</summary>
        public int? KinhNghiem { get; set; }

        /// <summary>Các ca gia sư nhận dạy, tên ngắn cách nhau dấu phẩy: "Sáng,Tối". Rỗng = nhận mọi ca</summary>
        [StringLength(200)]
        public string CaNhanDay { get; set; }

        [StringLength(500)]
        public string GhiChu { get; set; }

        /// <summary>Gia sư có nhận dạy ca này không (CaNhanDay rỗng = nhận mọi ca)</summary>
        public bool NhanCa(string caHoc)
        {
            if (string.IsNullOrWhiteSpace(CaNhanDay) || string.IsNullOrEmpty(caHoc)) return true;
            string ten = Common.Constants.CaHoc.TenNgan(caHoc);
            foreach (var c in CaNhanDay.Split(','))
                if (c.Trim() == ten) return true;
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LopHoc> LopHocs { get; set; }
    }
}
