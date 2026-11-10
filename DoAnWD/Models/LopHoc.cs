using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnWD.Models
{
    [Table("LopHoc")]
    public partial class LopHoc
    {
        public LopHoc()
        {
            BuoiHocs = new System.Collections.Generic.HashSet<BuoiHoc>();
            ThanhToans = new System.Collections.Generic.HashSet<ThanhToan>();
        }

        public virtual System.Collections.Generic.ICollection<BuoiHoc> BuoiHocs { get; set; }

        public virtual System.Collections.Generic.ICollection<ThanhToan> ThanhToans { get; set; }

        [Key]
        public int MaLop { get; set; }

        public int? MaGS { get; set; }

        public int? MaHV { get; set; }

        [StringLength(50)]
        public string MonHoc { get; set; }

        public decimal? HocPhi { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; }

        // ✨ CÁC THUỘC TÍNH MỚI CHO QUẢN LÝ CA HỌC
        public DateTime? ThoiGianBatDau { get; set; }

        public DateTime? ThoiGianKetThuc { get; set; }

        [StringLength(50)]
        public string CaHoc { get; set; }

        /// <summary>Tên lớp do người ghép lớp đặt (không trùng). Bổ sung ở 05_NangCap_v5.sql.</summary>
        [StringLength(150)]
        public string TenLop { get; set; }

        /// <summary>Cấp độ lúc vào lớp (môn năng khiếu)</summary>
        [StringLength(50)]
        public string CapDoDauVao { get; set; }

        /// <summary>Tên để hiển thị: tên lớp nếu đã đặt, không thì "Môn - Học viên"</summary>
        [NotMapped]
        public string TenHienThi => !string.IsNullOrWhiteSpace(TenLop) ? TenLop : $"{MonHoc} - {HocVien?.HoTen}";

        public virtual GiaSu GiaSu { get; set; }

        public virtual HocVien HocVien { get; set; }

        // ===================================================================
        // ✨ COMPUTED PROPERTIES - MỚI CHO OPTION 2
        // ===================================================================

        /// <summary>
        /// Tính trạng thái lớp học dựa trên thời gian hiện tại
        /// </summary>
        [NotMapped]
        public string TrangThaiTheoThoiGian
        {
            get
            {
                // Lớp đã hủy giữ nguyên trạng thái, không tính theo thời gian
                if (TrangThai == Common.Constants.LopHocStatus.DaHuy)
                    return TrangThai;

                if (!ThoiGianBatDau.HasValue || !ThoiGianKetThuc.HasValue)
                    return TrangThai; // Trả về trạng thái thủ công nếu không có thời gian

                DateTime now = DateTime.Now;

                if (now < ThoiGianBatDau.Value)
                    return Common.Constants.LopHocStatus.ChuaBatDau;
                else if (now >= ThoiGianBatDau.Value && now <= ThoiGianKetThuc.Value)
                    return Common.Constants.LopHocStatus.DangDienRa;
                else
                    return Common.Constants.LopHocStatus.DaKetThuc;
            }
        }

        /// <summary>
        /// Kiểm tra xem lớp có đang hoạt động không (chưa kết thúc)
        /// </summary>
        [NotMapped]
        public bool DangHoatDong
        {
            get
            {
                if (TrangThai == Common.Constants.LopHocStatus.DaHuy)
                    return false; // Lớp đã hủy không còn chiếm lịch

                if (!ThoiGianKetThuc.HasValue)
                    return Common.Constants.LopHocStatus.LaHoatDong(TrangThai);

                return DateTime.Now <= ThoiGianKetThuc.Value;
            }
        }

        /// <summary>
        /// Kiểm tra xem lớp có đã bắt đầu chưa
        /// </summary>
        [NotMapped]
        public bool DaBatDau
        {
            get
            {
                if (!ThoiGianBatDau.HasValue)
                    return false;

                return DateTime.Now >= ThoiGianBatDau.Value;
            }
        }

        /// <summary>
        /// Số ngày còn lại đến khi lớp bắt đầu (âm nếu đã bắt đầu)
        /// </summary>
        [NotMapped]
        public int SoNgayConLai
        {
            get
            {
                if (!ThoiGianBatDau.HasValue)
                    return 0;

                return (ThoiGianBatDau.Value.Date - DateTime.Now.Date).Days;
            }
        }

        /// <summary>
        /// Hiển thị thông tin ca học đầy đủ
        /// </summary>
        [NotMapped]
        public string ThongTinCaHoc
        {
            get
            {
                if (!ThoiGianBatDau.HasValue || !ThoiGianKetThuc.HasValue)
                    return CaHoc ?? "Chưa xác định";

                return string.Format("{0} | {1:dd/MM/yyyy} - {2:dd/MM/yyyy}",
                    CaHoc ?? "Chưa xác định",
                    ThoiGianBatDau.Value,
                    ThoiGianKetThuc.Value);
            }
        }

        /// <summary>
        /// Hiển thị thông tin đầy đủ cho DataGridView
        /// </summary>
        [NotMapped]
        public string ThongTinDayDu
        {
            get
            {
                return string.Format("Lớp {0} - {1} - GS: {2} - HV: {3}",
                    MaLop,
                    MonHoc ?? "N/A",
                    GiaSu?.HoTen ?? "N/A",
                    HocVien?.HoTen ?? "N/A");
            }
        }

        // ===================================================================
        // ✨ HELPER METHODS - MỚI CHO OPTION 2
        // ===================================================================

        /// <summary>
        /// Kiểm tra xem lớp có trùng thời gian với lớp khác không
        /// </summary>
        /// <param name="other">Lớp học khác cần so sánh</param>
        /// <returns>True nếu trùng thời gian</returns>
        public bool TrungThoiGian(LopHoc other)
        {
            if (other == null || !ThoiGianBatDau.HasValue || !ThoiGianKetThuc.HasValue ||
                !other.ThoiGianBatDau.HasValue || !other.ThoiGianKetThuc.HasValue)
                return false;

            // Kiểm tra overlap: (StartA <= EndB) AND (EndA >= StartB)
            return ThoiGianBatDau.Value <= other.ThoiGianKetThuc.Value &&
                   ThoiGianKetThuc.Value >= other.ThoiGianBatDau.Value;
        }

        /// <summary>
        /// Kiểm tra xem lớp có trùng ca học với lớp khác không
        /// </summary>
        /// <param name="other">Lớp học khác cần so sánh</param>
        /// <returns>True nếu trùng ca học</returns>
        public bool TrungCaHoc(LopHoc other)
        {
            if (other == null || string.IsNullOrEmpty(CaHoc) || string.IsNullOrEmpty(other.CaHoc))
                return false;

            return CaHoc.Equals(other.CaHoc, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Trùng lịch = cùng ca học VÀ khoảng ngày học giao nhau.
        /// (Một gia sư/học viên có thể học nhiều lớp song song nếu khác ca.)
        /// </summary>
        public bool TrungLich(LopHoc other)
        {
            return TrungCaHoc(other) && TrungThoiGian(other);
        }

        /// <summary>
        /// Validate thời gian của lớp học
        /// </summary>
        /// <returns>Thông báo lỗi (null nếu hợp lệ)</returns>
        public string ValidateThoiGian()
        {
            if (!ThoiGianBatDau.HasValue || !ThoiGianKetThuc.HasValue)
                return "Vui lòng nhập đầy đủ thời gian bắt đầu và kết thúc.";

            if (ThoiGianBatDau.Value >= ThoiGianKetThuc.Value)
                return Common.Constants.Messages.ThoiGianKhongHopLe;

            if (ThoiGianBatDau.Value < DateTime.Now.Date)
                return Common.Constants.Messages.ThoiGianQuaKhu;

            return null; // Hợp lệ
        }

        /// <summary>
        /// Cập nhật trạng thái tự động dựa trên thời gian
        /// </summary>
        /// <returns>True nếu trạng thái thay đổi</returns>
        public bool CapNhatTrangThaiTuDong()
        {
            if (!ThoiGianBatDau.HasValue || !ThoiGianKetThuc.HasValue)
                return false;

            if (TrangThai == Common.Constants.LopHocStatus.DaHuy)
                return false;

            string trangThaiMoi = TrangThaiTheoThoiGian;

            if (TrangThai != trangThaiMoi)
            {
                TrangThai = trangThaiMoi;
                return true; // Trạng thái đã thay đổi
            }

            return false; // Không thay đổi
        }
    }
}