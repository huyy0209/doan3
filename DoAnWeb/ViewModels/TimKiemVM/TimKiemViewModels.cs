using System.Collections.Generic;
using DoAnWeb.Services;

namespace DoAnWeb.ViewModels.TimKiemVM
{
    /// <summary>Một nhóm kết quả trên trang /TimKiem (Học viên / Gia sư / Lớp).</summary>
    public class NhomKetQuaVM
    {
        /// <summary>Id dùng cho aria-labelledby</summary>
        public string Ma { get; set; }
        public string TieuDe { get; set; }
        public List<GoiYTimKiem> Items { get; set; } = new();
        /// <summary>Tổng số kết quả khớp (có thể nhiều hơn <see cref="Items"/> vì trang chỉ lấy tối đa mỗi nhóm 50).</summary>
        public int Tong { get; set; }
    }

    /// <summary>Dữ liệu trang tìm kiếm đầy đủ.</summary>
    public class TimKiemIndexVM
    {
        public KetQuaTimKiemNhanh KetQua { get; set; } = new();
        /// <summary>Từ khóa đã đủ dài (≥ 2 ký tự sau khi bỏ dấu) để tìm hay chưa.</summary>
        public bool DuDai { get; set; }
        public List<NhomKetQuaVM> Nhom { get; set; } = new();
    }
}
