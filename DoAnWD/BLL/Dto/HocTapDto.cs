using System;
using System.Collections.Generic;

namespace DoAnWD.BLL.Dto
{
    /// <summary>Một dòng trên màn hình điểm danh theo ngày (một lớp có buổi học hôm đó)</summary>
    public class DiemDanhRow
    {
        public int MaLop { get; set; }
        public string HocVien { get; set; }
        public string MonHoc { get; set; }
        public string GiaSu { get; set; }
        public string CaHoc { get; set; }
        public string DiemDanh { get; set; }
        public decimal? Diem { get; set; }
        public string NhanXet { get; set; }
        /// <summary>Đã có bản ghi trong CSDL cho ngày này chưa</summary>
        public bool DaLuu { get; set; }
    }

    /// <summary>Lịch sử một buổi học (dùng cho kết quả học viên)</summary>
    public class BuoiHocRow
    {
        public DateTime NgayHoc { get; set; }
        public string MonHoc { get; set; }
        public string DiemDanh { get; set; }
        public decimal? Diem { get; set; }
        public string NhanXet { get; set; }
    }

    /// <summary>Tổng hợp kết quả một lớp của học viên</summary>
    public class KetQuaLop
    {
        public int MaLop { get; set; }
        public string MonHoc { get; set; }
        public string GiaSu { get; set; }
        public string CaHoc { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public string TrangThai { get; set; }
        public int SoBuoi { get; set; }
        public int SoBuoiCoMat { get; set; }
        public int SoBuoiVang { get; set; }
        public int SoBaiTest { get; set; }
        public decimal? DiemTrungBinh { get; set; }
        public string XepLoai { get; set; }
        public List<BuoiHocRow> Buoi { get; set; } = new List<BuoiHocRow>();

        public string ChuyenCan => SoBuoi == 0 ? "" : $"{SoBuoiCoMat}/{SoBuoi} buổi";
    }

    /// <summary>Công nợ học phí của một lớp</summary>
    public class CongNoLop
    {
        public int MaLop { get; set; }
        public string HocVien { get; set; }
        public string SDTLienHe { get; set; }
        public string MonHoc { get; set; }
        public string GiaSu { get; set; }
        public decimal HocPhi { get; set; }
        public decimal DaDong { get; set; }
        public decimal ConNo { get; set; }
        /// <summary>Đã đủ / Còn nợ / Chưa đóng</summary>
        public string TinhTrang { get; set; }
        public string TrangThaiLop { get; set; }
    }
}
