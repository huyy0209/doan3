using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DoAnWD.BLL.Dto;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    /// <summary>Loại việc tồn hiện ở trang Tổng quan (trang dựa vào đây để biết bấm vào thì mở đâu)</summary>
    public enum LoaiViec { ChuaDiemDanh, VangChuaBu, ConNo, SapKetThuc, SapBatDau }

    public class ViecCanLam
    {
        public LoaiViec Loai { get; set; }
        public string TieuDe { get; set; }
        public string ChiTiet { get; set; }
        /// <summary>Học viên / lớp liên quan (nếu việc gắn với một người cụ thể)</summary>
        public int? MaHV { get; set; }
        public int? MaLop { get; set; }
        public DateTime? Ngay { get; set; }
    }

    public class ThuThang
    {
        public DateTime Thang { get; set; }
        public decimal SoTien { get; set; }
    }

    public class LopSapHet
    {
        public int MaLop { get; set; }
        public string TenLop { get; set; }
        public string GiaSu { get; set; }
        public DateTime KetThuc { get; set; }
        public int ConNgay => Math.Max(0, (KetThuc.Date - DateTime.Today).Days);
    }

    /// <summary>Toàn bộ số liệu của trang Tổng quan, nạp một lần</summary>
    public class TongQuanDuLieu
    {
        public int LopDangHoc { get; set; }
        public int LopChuaBatDau { get; set; }
        public int HocVienDangHoc { get; set; }
        public int GiaSuDangDay { get; set; }
        public int TongGiaSu { get; set; }
        public decimal ThuThangNay { get; set; }
        public decimal ThuThangTruoc { get; set; }
        public decimal ConNo { get; set; }
        public int SoLopConNo { get; set; }

        /// <summary>Lớp học hôm nay (kèm buổi bù), đã có cờ DaLuu = đã điểm danh</summary>
        public List<DiemDanhRow> HomNay { get; set; } = new List<DiemDanhRow>();
        public List<ViecCanLam> Viec { get; set; } = new List<ViecCanLam>();
        /// <summary>Học phí đã thu 6 tháng gần nhất, cũ trước mới sau</summary>
        public List<ThuThang> ThuTheoThang { get; set; } = new List<ThuThang>();
        public List<LopSapHet> SapKetThuc { get; set; } = new List<LopSapHet>();
    }

    /// <summary>
    /// Số liệu trang Tổng quan: hôm nay dạy gì, việc nào còn tồn, tiền thu ra sao.
    /// Việc tồn gồm: hôm qua chưa điểm danh, buổi vắng chưa xếp học bù (30 ngày), lớp còn nợ học phí,
    /// lớp kết thúc / bắt đầu trong 7 ngày tới.
    /// </summary>
    public class TongQuanService
    {
        public const int SoNgayXetVang = 30;
        public const int SoNgayBaoTruoc = 7;
        private const int SoDongVangToiDa = 3;

        public TongQuanDuLieu Lay()
        {
            var kq = new TongQuanDuLieu();
            DateTime homNay = DateTime.Today, mocVang = homNay.AddDays(-SoNgayXetVang), mocBao = homNay.AddDays(SoNgayBaoTruoc + 1);
            string huy = Constants.LopHocStatus.DaHuy;

            var hocTap = new HocTapService();
            kq.HomNay = hocTap.GetLopTrongNgay(homNay);
            int chuaDiemDanh = hocTap.GetLopTrongNgay(homNay.AddDays(-1)).Count(r => !r.DaLuu && !r.LaHocBu);

            var congNo = new HocPhiService().GetCongNo().Where(c => c.TrangThaiLop != huy && c.ConNo > 0).ToList();
            kq.ConNo = congNo.Sum(c => c.ConNo);
            kq.SoLopConNo = congNo.Count;

            using (var db = new GiaSuContextDB())
            {
                // Lớp chưa hủy còn chạy hoặc mới kết thúc gần đây (đủ để xét buổi vắng 30 ngày)
                var lops = db.LopHocs.Include(l => l.HocVien).Include(l => l.GiaSu)
                    .Where(l => l.TrangThai != huy && (l.ThoiGianKetThuc == null || l.ThoiGianKetThuc >= mocVang))
                    .ToList();
                var dangHoc = lops.Where(l => l.TrangThaiTheoThoiGian == Constants.LopHocStatus.DangDienRa
                                           || l.TrangThaiTheoThoiGian == Constants.LopHocStatus.DangHoc).ToList();
                var chuaBatDau = lops.Where(l => l.TrangThaiTheoThoiGian == Constants.LopHocStatus.ChuaBatDau).ToList();

                kq.LopDangHoc = dangHoc.Count;
                kq.LopChuaBatDau = chuaBatDau.Count;
                kq.HocVienDangHoc = dangHoc.Where(l => l.MaHV.HasValue).Select(l => l.MaHV).Distinct().Count();
                kq.GiaSuDangDay = dangHoc.Where(l => l.MaGS.HasValue).Select(l => l.MaGS).Distinct().Count();
                kq.TongGiaSu = db.GiaSus.Count();

                // Học phí đã thu theo tháng
                var dauThang = new DateTime(homNay.Year, homNay.Month, 1);
                var tu = dauThang.AddMonths(-5);
                var thu = db.ThanhToans.Where(t => t.NgayDong >= tu).Select(t => new { t.NgayDong, t.SoTien }).ToList();
                for (int i = 0; i < 6; i++)
                {
                    var thang = tu.AddMonths(i);
                    kq.ThuTheoThang.Add(new ThuThang
                    {
                        Thang = thang,
                        SoTien = thu.Where(t => t.NgayDong.Year == thang.Year && t.NgayDong.Month == thang.Month).Sum(t => t.SoTien)
                    });
                }
                kq.ThuThangNay = kq.ThuTheoThang[5].SoTien;
                kq.ThuThangTruoc = kq.ThuTheoThang[4].SoTien;

                kq.SapKetThuc = dangHoc
                    .Where(l => l.ThoiGianKetThuc.HasValue && l.ThoiGianKetThuc.Value < mocBao)
                    .OrderBy(l => l.ThoiGianKetThuc)
                    .Select(l => new LopSapHet { MaLop = l.MaLop, TenLop = l.TenHienThi, GiaSu = l.GiaSu?.HoTen, KetThuc = l.ThoiGianKetThuc.Value })
                    .ToList();
                int sapBatDau = chuaBatDau.Count(l => l.ThoiGianBatDau.HasValue && l.ThoiGianBatDau.Value < mocBao);

                // Buổi vắng trong 30 ngày chưa có buổi bù
                var theoMa = lops.ToDictionary(l => l.MaLop);
                var maLops = theoMa.Keys.ToList();
                string vangCoPhep = Constants.DiemDanh.VangCoPhep, vangKhongPhep = Constants.DiemDanh.VangKhongPhep;
                var vang = db.BuoiHocs
                    .Where(b => b.MaBuoiGoc == null && b.NgayHoc >= mocVang && maLops.Contains(b.MaLop)
                             && (b.DiemDanh == vangCoPhep || b.DiemDanh == vangKhongPhep))
                    .Select(b => new { b.MaBuoi, b.MaLop, b.NgayHoc }).ToList();
                var maVang = vang.Select(v => v.MaBuoi).ToList();
                var daBu = new HashSet<int>(db.BuoiHocs
                    .Where(b => b.MaBuoiGoc != null && maVang.Contains(b.MaBuoiGoc.Value))
                    .Select(b => b.MaBuoiGoc.Value).ToList());
                var chuaBu = vang.Where(v => !daBu.Contains(v.MaBuoi)).OrderByDescending(v => v.NgayHoc).ToList();

                // ---- Xếp việc theo độ gấp ----
                if (chuaDiemDanh > 0)
                    kq.Viec.Add(new ViecCanLam
                    {
                        Loai = LoaiViec.ChuaDiemDanh, Ngay = homNay.AddDays(-1),
                        TieuDe = $"{chuaDiemDanh} lớp hôm qua chưa điểm danh",
                        ChiTiet = "Điểm danh bù để sổ chuyên cần đúng"
                    });

                foreach (var v in chuaBu.Take(chuaBu.Count > SoDongVangToiDa + 1 ? SoDongVangToiDa : chuaBu.Count))
                {
                    var l = theoMa[v.MaLop];
                    kq.Viec.Add(new ViecCanLam
                    {
                        Loai = LoaiViec.VangChuaBu, MaHV = l.MaHV, MaLop = l.MaLop, Ngay = v.NgayHoc,
                        TieuDe = $"{l.HocVien?.HoTen ?? "Học viên"} vắng ngày {v.NgayHoc:dd/MM}",
                        ChiTiet = $"{l.TenHienThi} - chưa xếp học bù"
                    });
                }
                if (chuaBu.Count > SoDongVangToiDa + 1)
                    kq.Viec.Add(new ViecCanLam
                    {
                        Loai = LoaiViec.VangChuaBu,
                        TieuDe = $"{chuaBu.Count - SoDongVangToiDa} buổi vắng khác chưa xếp học bù",
                        ChiTiet = $"Trong {SoNgayXetVang} ngày gần đây"
                    });

                if (kq.SoLopConNo > 0)
                    kq.Viec.Add(new ViecCanLam
                    {
                        Loai = LoaiViec.ConNo,
                        TieuDe = $"{kq.SoLopConNo} lớp còn nợ học phí",
                        ChiTiet = $"Tổng còn phải thu {kq.ConNo:N0} đ"
                    });
                if (kq.SapKetThuc.Count > 0)
                    kq.Viec.Add(new ViecCanLam
                    {
                        Loai = LoaiViec.SapKetThuc,
                        TieuDe = $"{kq.SapKetThuc.Count} lớp kết thúc trong {SoNgayBaoTruoc} ngày tới",
                        ChiTiet = "Hỏi phụ huynh có học tiếp không để gia hạn"
                    });
                if (sapBatDau > 0)
                    kq.Viec.Add(new ViecCanLam
                    {
                        Loai = LoaiViec.SapBatDau,
                        TieuDe = $"{sapBatDau} lớp bắt đầu trong {SoNgayBaoTruoc} ngày tới",
                        ChiTiet = "Nhắc gia sư và học viên lịch buổi đầu"
                    });
            }
            return kq;
        }
    }
}
