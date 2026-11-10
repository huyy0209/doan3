using System;
using System.Collections.Generic;

namespace DoAnWD.BLL.Dto
{
    /// <summary>
    /// Một dòng trên màn hình điểm danh theo ca: một lớp có buổi học hôm đó,
    /// hoặc một học viên đến học bù vào ca đó (LaHocBu = true, MaBuoi có giá trị).
    /// </summary>
    public class DiemDanhRow
    {
        public int MaLop { get; set; }
        /// <summary>Buổi đã có trong CSDL (buổi học bù luôn có sẵn từ lúc xếp lịch)</summary>
        public int? MaBuoi { get; set; }
        public int MaHV { get; set; }
        public string HocVien { get; set; }
        public string MonHoc { get; set; }
        public string GiaSu { get; set; }
        public string CaHoc { get; set; }
        public string DiemDanh { get; set; }
        public decimal? Diem { get; set; }
        public string NhanXet { get; set; }
        /// <summary>Đã có bản ghi trong CSDL cho ngày này chưa</summary>
        public bool DaLuu { get; set; }
        public bool LaHocBu { get; set; }
        /// <summary>"Học bù buổi 10/11" hoặc "Đã xếp bù 12/11, Chiều"</summary>
        public string GhiChu { get; set; }
        /// <summary>Buổi thường đã có lịch học bù => không đổi sang có mặt được</summary>
        public bool DaXepBu { get; set; }
    }

    /// <summary>Một buổi học của học viên (buổi thường hoặc buổi học bù)</summary>
    public class BuoiHocRow
    {
        /// <summary>0 = buổi mới chưa lưu</summary>
        public int MaBuoi { get; set; }
        public int MaLop { get; set; }
        public DateTime NgayHoc { get; set; }
        public string MonHoc { get; set; }
        /// <summary>Ca thực tế: ca học bù hoặc ca của lớp</summary>
        public string CaHoc { get; set; }
        /// <summary>Gia sư thực tế dạy buổi đó</summary>
        public string GiaSu { get; set; }
        public int MaGSDay { get; set; }
        public string DiemDanh { get; set; }
        public decimal? Diem { get; set; }
        public string NhanXet { get; set; }

        // ----- Học bù -----
        public bool LaHocBu { get; set; }
        /// <summary>Với buổi học bù: ngày của buổi vắng được bù</summary>
        public DateTime? NgayVangGoc { get; set; }
        /// <summary>Với buổi vắng: buổi học bù đã xếp (nếu có)</summary>
        public int? MaBuoiBu { get; set; }
        public DateTime? NgayBu { get; set; }
        public string CaBu { get; set; }

        /// <summary>Buổi học bù đã xếp nhưng chưa tới ngày</summary>
        public bool ChuaToiNgay => NgayHoc.Date > DateTime.Today;

        public bool Vang => !Common.Constants.DiemDanh.CoHoc(DiemDanh);

        /// <summary>Buổi vắng chưa có lịch bù => xếp học bù được</summary>
        public bool CoTheXepBu => !LaHocBu && MaBuoi > 0 && Vang && !MaBuoiBu.HasValue;

        /// <summary>Chữ hiện ở cột điểm danh (có thông tin học bù)</summary>
        public string DiemDanhHienThi
        {
            get
            {
                if (LaHocBu)
                    return ChuaToiNgay ? $"Học bù (đã xếp lịch)" : $"{DiemDanh} (học bù)";
                if (MaBuoiBu.HasValue)
                    return $"{DiemDanh}, bù {NgayBu:dd/MM} {Common.Constants.CaHoc.TenNgan(CaBu)}";
                return DiemDanh;
            }
        }
    }

    /// <summary>Tổng hợp kết quả một lớp của học viên</summary>
    public class KetQuaLop
    {
        public int MaLop { get; set; }
        public int MaGS { get; set; }
        public string MonHoc { get; set; }
        public string GiaSu { get; set; }
        public string CaHoc { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public string TrangThai { get; set; }
        /// <summary>Số buổi theo lịch lớp đã điểm danh (buổi học bù không cộng thêm)</summary>
        public int SoBuoi { get; set; }
        /// <summary>Có mặt, tính cả buổi vắng đã học bù xong</summary>
        public int SoBuoiCoMat { get; set; }
        public int SoBuoiVang { get; set; }
        public int SoBuoiHocBu { get; set; }
        public int SoBaiTest { get; set; }
        public decimal? DiemTrungBinh { get; set; }
        public string XepLoai { get; set; }
        /// <summary>Mọi buổi (thường + học bù), mới nhất trước</summary>
        public List<BuoiHocRow> Buoi { get; set; } = new List<BuoiHocRow>();

        public string ChuyenCan => SoBuoi == 0 ? "" : $"{SoBuoiCoMat}/{SoBuoi} buổi";
    }

    /// <summary>Gia sư để chọn dạy học bù, kèm lý do bận ở ngày + ca đó (nếu có)</summary>
    public class GiaSuHocBu
    {
        public int MaGS { get; set; }
        public string HoTen { get; set; }
        public string ChuyenMon { get; set; }
        /// <summary>null = rảnh; có giá trị = đang dạy lớp khác ở ca đó (vẫn xếp được nếu chấp nhận học chung)</summary>
        public string Ban { get; set; }
        public override string ToString() =>
            $"{HoTen} ({ChuyenMon})" + (Ban == null ? "" : "  - bận: " + Ban);
    }

    /// <summary>Kết quả xếp học bù: lỗi (không xếp được), cảnh báo (cần xác nhận) hoặc mã buổi mới</summary>
    public class KetQuaHocBu
    {
        public string Loi { get; set; }
        public string CanhBao { get; set; }
        public int? MaBuoi { get; set; }
        public bool ThanhCong => MaBuoi.HasValue;
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
