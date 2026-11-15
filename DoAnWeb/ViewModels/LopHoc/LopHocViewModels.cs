using System;
using System.Collections.Generic;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services.Dto;

namespace DoAnWeb.ViewModels.LopHocVM
{
    /// <summary>Dữ liệu trang danh sách lớp (frmLopHoc.cs).</summary>
    public class LopHocIndexVM
    {
        public List<LopHocDong> DanhSach { get; set; } = new List<LopHocDong>();

        public string TuKhoa { get; set; }
        /// <summary>"Còn hoạt động" (mặc định) / "Tất cả" / một trạng thái cụ thể.</summary>
        public string TrangThai { get; set; } = "Còn hoạt động";
        public string CaHoc { get; set; }
        public string MonHoc { get; set; }
        public int? MaGS { get; set; }
        /// <summary>Lớp cần chọn sẵn sau khi ghép / sửa.</summary>
        public int? ChonMaLop { get; set; }

        public List<DoAnWeb.Data.Entities.GiaSu> GiaSus { get; set; } = new List<DoAnWeb.Data.Entities.GiaSu>();
        public List<string> MonHocs { get; set; } = new List<string>();

        /// <summary>Tổng số lớp khớp bộ lọc (để hiện dòng đếm như app).</summary>
        public int TongSo { get; set; }
        public int TongSoTatCa { get; set; }
    }

    /// <summary>Biểu mẫu ghép lớp mới (frmChucNangGhepLop.cs).</summary>
    public class GhepLopVM
    {
        public int? MaHV { get; set; }
        public int? MaGS { get; set; }
        public string MonHoc { get; set; }
        /// <summary>Nhập dạng "1.500.000" (SoTien.TryParse đọc phần chữ số).</summary>
        public string HocPhi { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public string CaHoc { get; set; }
        /// <summary>Chuỗi "2,4,6"; đủ 7 thứ hoặc rỗng = hằng ngày.</summary>
        public string ThuHoc { get; set; }
        public string TenLop { get; set; }
        public string CapDoDauVao { get; set; }

        public List<DoAnWeb.Data.Entities.HocVien> HocViens { get; set; } = new List<DoAnWeb.Data.Entities.HocVien>();
        public List<DoAnWeb.Data.Entities.GiaSu> GiaSus { get; set; } = new List<DoAnWeb.Data.Entities.GiaSu>();
        public List<MonHocDanhMuc> MonHocs { get; set; } = new List<MonHocDanhMuc>();
        public List<Data.Entities.LopHoc> LopHoatDong { get; set; } = new List<Data.Entities.LopHoc>();
        public HashSet<string> TenLopDaCo { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Biểu mẫu sửa lớp (dlgSuaLop.cs).</summary>
    public class SuaLopVM
    {
        public int MaLop { get; set; }
        public string TenLop { get; set; }
        public string HocPhi { get; set; }
        public DateTime? KetThuc { get; set; }
        public string ThuHoc { get; set; }
        public int MaGS { get; set; }

        /// <summary>Thông tin lớp đang sửa (để hiện phần cố định và kiểm tra).</summary>
        public LopHocDong Lop { get; set; }
        public List<DoAnWeb.Data.Entities.GiaSu> GiaSus { get; set; } = new List<DoAnWeb.Data.Entities.GiaSu>();
    }
}
