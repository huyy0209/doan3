using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DoAnWD.BLL.Dto;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    /// <summary>
    /// Điểm danh, điểm bài test 15 phút cuối giờ và học bù.
    ///  - Buổi thường: mỗi lớp tối đa 1 bản ghi / ngày, ca + gia sư theo lớp.
    ///  - Không phải buổi nào cũng kiểm tra: Diem = null nghĩa là không test. Vắng thì không có điểm.
    ///  - Học bù: buổi vắng được bù bằng một buổi khác (ngày + ca khác, có thể gia sư khác),
    ///    vẫn tính cho lớp gốc. Buổi vắng giữ nguyên trạng thái, chỉ trỏ tới buổi bù.
    /// Mỗi hàm dùng DbContext riêng nên luôn đọc dữ liệu mới nhất.
    /// </summary>
    public class HocTapService
    {
        /// <summary>Ném lỗi nếu CSDL chưa chạy 04_NangCap_v4.sql (thiếu cột học bù)</summary>
        public static void KiemTraCauTrucCSDL()
        {
            using (var db = new GiaSuContextDB())
                db.BuoiHocs.Select(b => new { b.MaBuoiGoc, b.CaHoc, b.MaGSDay }).Take(1).ToList();
        }

        /// <summary>Xếp loại theo điểm trung bình thang 10</summary>
        public static string XepLoai(decimal? diem)
        {
            if (!diem.HasValue) return "";
            if (diem.Value >= 8m) return "Giỏi";
            if (diem.Value >= 6.5m) return "Khá";
            if (diem.Value >= 5m) return "Trung bình";
            return "Yếu";
        }

        /// <summary>
        /// Kiểm tra dữ liệu một buổi. Trả về thông báo lỗi hoặc null nếu hợp lệ.
        /// </summary>
        public static string KiemTraBuoi(DateTime ngay, string diemDanh, decimal? diem, string nhanXet,
            DateTime? lopBatDau, DateTime? lopKetThuc)
        {
            if (!Constants.DiemDanh.TatCa.Contains(diemDanh))
                return "Trạng thái điểm danh không hợp lệ.";
            if (ngay.Date > DateTime.Now.Date)
                return "Không thể điểm danh cho ngày trong tương lai.";
            if (lopBatDau.HasValue && ngay.Date < lopBatDau.Value.Date)
                return $"Lớp bắt đầu từ {lopBatDau:dd/MM/yyyy}.";
            if (lopKetThuc.HasValue && ngay.Date > lopKetThuc.Value.Date)
                return $"Lớp đã kết thúc ngày {lopKetThuc:dd/MM/yyyy}.";
            if (diem.HasValue)
            {
                if (diem.Value < 0 || diem.Value > 10)
                    return "Điểm phải từ 0 đến 10.";
                if (!Constants.DiemDanh.CoHoc(diemDanh))
                    return "Học viên vắng thì không có điểm test.";
            }
            if (!string.IsNullOrEmpty(nhanXet) && nhanXet.Length > 300)
                return "Nhận xét tối đa 300 ký tự.";
            return null;
        }

        /// <summary>Đổi chữ người dùng gõ ("8,5" / "8.5" / "") sang điểm</summary>
        public static bool TryParseDiem(string text, out decimal? diem)
        {
            diem = null;
            if (string.IsNullOrWhiteSpace(text)) return true;
            decimal d;
            string chuan = text.Trim().Replace(',', '.');
            if (!decimal.TryParse(chuan, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out d))
                return false;
            diem = Math.Round(d, 2);
            return true;
        }

        // ===================================================================
        // ĐIỂM DANH THEO CA
        // ===================================================================

        /// <summary>
        /// Danh sách điểm danh của một ngày: các lớp có buổi hôm đó (bỏ lớp đã hủy)
        /// + học viên đến học bù vào ngày đó. Kèm điểm danh đã lưu nếu có.
        /// </summary>
        /// <param name="caHoc">null = mọi ca</param>
        /// <param name="maGS">null = mọi gia sư (quản lý); có giá trị = lớp của gia sư đó + buổi bù gia sư đó dạy</param>
        public List<DiemDanhRow> GetLopTrongNgay(DateTime ngay, string caHoc = null, int? maGS = null)
        {
            var d = ngay.Date;
            var dSau = d.AddDays(1);
            using (var db = new GiaSuContextDB())
            {
                var huy = Constants.LopHocStatus.DaHuy;
                var q = db.LopHocs
                    .Include(l => l.GiaSu)
                    .Include(l => l.HocVien)
                    .Where(l => l.TrangThai != huy && l.ThoiGianBatDau < dSau && l.ThoiGianKetThuc >= d);
                if (!string.IsNullOrEmpty(caHoc)) q = q.Where(l => l.CaHoc == caHoc);
                if (maGS.HasValue) q = q.Where(l => l.MaGS == maGS.Value);
                var lops = q.ToList();

                var maLops = lops.Select(l => l.MaLop).ToList();
                var buoi = db.BuoiHocs
                    .Where(b => b.NgayHoc == d && b.MaBuoiGoc == null && maLops.Contains(b.MaLop))
                    .ToList()
                    .ToDictionary(b => b.MaLop);
                // Buổi bù đã xếp cho các buổi vắng hôm nay
                var maBuoiHomNay = buoi.Values.Select(b => b.MaBuoi).ToList();
                var buCuaHomNay = db.BuoiHocs
                    .Where(b => b.MaBuoiGoc != null && maBuoiHomNay.Contains(b.MaBuoiGoc.Value))
                    .ToList()
                    .ToDictionary(b => b.MaBuoiGoc.Value);

                var ketQua = lops.Select(l =>
                {
                    BuoiHoc b;
                    buoi.TryGetValue(l.MaLop, out b);
                    BuoiHoc bu = null;
                    if (b != null) buCuaHomNay.TryGetValue(b.MaBuoi, out bu);
                    return new DiemDanhRow
                    {
                        MaLop = l.MaLop,
                        MaBuoi = b?.MaBuoi,
                        MaHV = l.MaHV ?? 0,
                        HocVien = l.HocVien?.HoTen,
                        MonHoc = l.MonHoc,
                        GiaSu = l.GiaSu?.HoTen,
                        CaHoc = l.CaHoc,
                        DiemDanh = b?.DiemDanh ?? Constants.DiemDanh.CoMat,
                        Diem = b?.Diem,
                        NhanXet = b?.NhanXet,
                        DaLuu = b != null,
                        DaXepBu = bu != null,
                        GhiChu = bu == null ? null : $"Đã xếp bù {bu.NgayHoc:dd/MM}, {Constants.CaHoc.TenNgan(bu.CaHoc)}"
                    };
                }).ToList();

                // Học viên đến học bù vào ngày này
                var qBu = db.BuoiHocs.Where(b => b.NgayHoc == d && b.MaBuoiGoc != null);
                if (!string.IsNullOrEmpty(caHoc)) qBu = qBu.Where(b => b.CaHoc == caHoc);
                if (maGS.HasValue) qBu = qBu.Where(b => b.MaGSDay == maGS.Value);
                var dsBu = qBu.ToList();
                if (dsBu.Count > 0)
                {
                    var lopIds = dsBu.Select(b => b.MaLop).Distinct().ToList();
                    var lopBu = db.LopHocs.Include(l => l.HocVien).Where(l => lopIds.Contains(l.MaLop)).ToList().ToDictionary(l => l.MaLop);
                    var gsIds = dsBu.Select(b => b.MaGSDay ?? 0).Distinct().ToList();
                    var tenGS = db.GiaSus.Where(g => gsIds.Contains(g.MaGS)).ToList().ToDictionary(g => g.MaGS, g => g.HoTen);
                    var gocIds = dsBu.Select(b => b.MaBuoiGoc.Value).ToList();
                    var ngayGoc = db.BuoiHocs.Where(b => gocIds.Contains(b.MaBuoi)).ToList().ToDictionary(b => b.MaBuoi, b => b.NgayHoc);

                    foreach (var b in dsBu)
                    {
                        LopHoc l;
                        if (!lopBu.TryGetValue(b.MaLop, out l)) continue;
                        string ten; tenGS.TryGetValue(b.MaGSDay ?? 0, out ten);
                        DateTime goc; ngayGoc.TryGetValue(b.MaBuoiGoc.Value, out goc);
                        ketQua.Add(new DiemDanhRow
                        {
                            MaLop = b.MaLop,
                            MaBuoi = b.MaBuoi,
                            MaHV = l.MaHV ?? 0,
                            HocVien = l.HocVien?.HoTen,
                            MonHoc = l.MonHoc,
                            GiaSu = ten,
                            CaHoc = b.CaHoc,
                            DiemDanh = b.DiemDanh,
                            Diem = b.Diem,
                            NhanXet = b.NhanXet,
                            DaLuu = true,
                            LaHocBu = true,
                            GhiChu = $"Học bù buổi {goc:dd/MM}"
                        });
                    }
                }

                return ketQua
                    .OrderBy(r => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, r.CaHoc))
                    .ThenBy(r => r.LaHocBu)
                    .ThenBy(r => r.HocVien)
                    .ToList();
            }
        }

        /// <summary>
        /// Lưu điểm danh + điểm của nhiều dòng trong cùng một ngày (buổi thường và buổi học bù).
        /// Trả về danh sách lỗi theo từng dòng; dòng hợp lệ vẫn được lưu.
        /// </summary>
        public List<string> LuuNgay(DateTime ngay, IEnumerable<DiemDanhRow> rows, int? chiChoGiaSu = null)
        {
            var d = ngay.Date;
            return LuuCacBuoi(rows.Select(r => new BuoiHocRow
            {
                MaBuoi = r.MaBuoi ?? 0,
                MaLop = r.MaLop,
                NgayHoc = d,
                DiemDanh = r.DiemDanh,
                Diem = r.Diem,
                NhanXet = r.NhanXet
            }), chiChoGiaSu);
        }

        /// <summary>
        /// Lưu nhiều buổi (dùng chung cho điểm danh theo ca và sửa buổi ở trang học viên).
        ///  - MaBuoi > 0: cập nhật buổi đó.
        ///  - MaBuoi = 0: buổi thường của lớp vào NgayHoc (thêm mới hoặc cập nhật nếu đã có).
        /// </summary>
        public List<string> LuuCacBuoi(IEnumerable<BuoiHocRow> rows, int? chiChoGiaSu = null)
        {
            var loi = new List<string>();
            using (var db = new GiaSuContextDB())
            {
                foreach (var r in rows)
                {
                    var lop = db.LopHocs.Include(l => l.HocVien).FirstOrDefault(l => l.MaLop == r.MaLop);
                    if (lop == null) { loi.Add($"Lớp {r.MaLop}: không tồn tại."); continue; }
                    string ten = $"{lop.HocVien?.HoTen} ({lop.MonHoc}, {r.NgayHoc:dd/MM})";

                    BuoiHoc b = null;
                    if (r.MaBuoi > 0)
                    {
                        b = db.BuoiHocs.FirstOrDefault(x => x.MaBuoi == r.MaBuoi);
                        if (b == null || b.MaLop != r.MaLop) { loi.Add($"{ten}: buổi học không còn tồn tại."); continue; }
                    }
                    else
                    {
                        var d = r.NgayHoc.Date;
                        b = db.BuoiHocs.FirstOrDefault(x => x.MaLop == r.MaLop && x.NgayHoc == d && x.MaBuoiGoc == null);
                    }
                    bool laBu = b != null && b.MaBuoiGoc.HasValue;

                    if (chiChoGiaSu.HasValue && lop.MaGS != chiChoGiaSu.Value && !(laBu && b.MaGSDay == chiChoGiaSu.Value))
                    {
                        loi.Add($"{ten}: không phải lớp của bạn.");
                        continue;
                    }

                    // Buổi bù có thể sau ngày kết thúc lớp => không kiểm tra khoảng ngày của lớp
                    string err = laBu
                        ? KiemTraBuoi(b.NgayHoc, r.DiemDanh, r.Diem, r.NhanXet, null, null)
                        : KiemTraBuoi(r.NgayHoc, r.DiemDanh, r.Diem, r.NhanXet, lop.ThoiGianBatDau, lop.ThoiGianKetThuc);
                    if (err != null) { loi.Add($"{ten}: {err}"); continue; }

                    // Buổi vắng đã có lịch bù thì không đổi sang có mặt (sẽ bị tính 2 lần)
                    if (b != null && !laBu && Constants.DiemDanh.CoHoc(r.DiemDanh))
                    {
                        int maGoc = b.MaBuoi;
                        if (db.BuoiHocs.Any(x => x.MaBuoiGoc == maGoc))
                        {
                            loi.Add($"{ten}: buổi này đã xếp học bù. Hủy học bù trước nếu muốn đổi thành có mặt.");
                            continue;
                        }
                    }

                    if (b == null)
                    {
                        b = new BuoiHoc { MaLop = r.MaLop, NgayHoc = r.NgayHoc.Date };
                        db.BuoiHocs.Add(b);
                    }
                    b.DiemDanh = r.DiemDanh;
                    b.Diem = r.Diem;
                    b.NhanXet = string.IsNullOrWhiteSpace(r.NhanXet) ? null : r.NhanXet.Trim();
                }
                db.SaveChanges();
            }
            return loi;
        }

        // ===================================================================
        // KẾT QUẢ HỌC VIÊN
        // ===================================================================

        /// <summary>
        /// Kết quả học tập của học viên theo từng lớp (mới nhất trước), mỗi lớp kèm mọi buổi
        /// (buổi thường + học bù). Gia sư chỉ thấy lớp của mình và lớp có buổi bù mình dạy.
        /// </summary>
        public List<KetQuaLop> GetKetQuaHocVien(int maHV, int? maGS = null)
        {
            using (var db = new GiaSuContextDB())
            {
                var lops = db.LopHocs.Include(l => l.GiaSu).Where(l => l.MaHV == maHV).ToList();
                var maLops = lops.Select(l => l.MaLop).ToList();
                var buoi = db.BuoiHocs.Where(b => maLops.Contains(b.MaLop)).ToList();

                if (maGS.HasValue)
                {
                    var lopCoBuMinhDay = buoi.Where(b => b.MaGSDay == maGS.Value).Select(b => b.MaLop).ToList();
                    lops = lops.Where(l => l.MaGS == maGS.Value || lopCoBuMinhDay.Contains(l.MaLop)).ToList();
                }

                var gsIds = buoi.Where(b => b.MaGSDay.HasValue).Select(b => b.MaGSDay.Value).Distinct().ToList();
                var tenGS = db.GiaSus.Where(g => gsIds.Contains(g.MaGS)).ToList().ToDictionary(g => g.MaGS, g => g.HoTen);

                return lops
                    .OrderByDescending(l => l.ThoiGianBatDau)
                    .Select(l => TongHop(l, buoi.Where(b => b.MaLop == l.MaLop).ToList(), tenGS))
                    .ToList();
            }
        }

        /// <summary>
        /// Tổng hợp một lớp. Buổi vắng đã học bù xong được tính là có mặt (theo điểm danh của buổi bù);
        /// buổi bù không cộng thêm vào số buổi của lớp.
        /// </summary>
        public static KetQuaLop TongHop(LopHoc l, List<BuoiHoc> buoiCuaLop, Dictionary<int, string> tenGS)
        {
            var bu = buoiCuaLop.Where(b => b.MaBuoiGoc.HasValue).ToDictionary(b => b.MaBuoiGoc.Value);
            var thuong = buoiCuaLop.Where(b => !b.MaBuoiGoc.HasValue).ToList();
            var ngayCuaBuoi = buoiCuaLop.ToDictionary(b => b.MaBuoi, b => b.NgayHoc);
            var homNay = DateTime.Today;

            int coMat = 0, vang = 0, daBu = 0;
            foreach (var b in thuong)
            {
                BuoiHoc bb;
                bool coBuXong = bu.TryGetValue(b.MaBuoi, out bb) && bb.NgayHoc.Date <= homNay;
                string thucTe = coBuXong ? bb.DiemDanh : b.DiemDanh;
                if (Constants.DiemDanh.CoHoc(thucTe)) coMat++; else vang++;
                if (coBuXong && Constants.DiemDanh.CoHoc(bb.DiemDanh)) daBu++;
            }

            var diem = buoiCuaLop.Where(b => b.Diem.HasValue).Select(b => b.Diem.Value).ToList();
            decimal? tb = diem.Count > 0 ? Math.Round(diem.Average(), 2) : (decimal?)null;

            Func<int?, string> ten = ma =>
            {
                string t;
                return ma.HasValue && tenGS != null && tenGS.TryGetValue(ma.Value, out t) ? t : l.GiaSu?.HoTen;
            };

            var rows = buoiCuaLop.Select(b =>
            {
                BuoiHoc bb = null;
                if (!b.MaBuoiGoc.HasValue) bu.TryGetValue(b.MaBuoi, out bb);
                DateTime ngayGoc;
                return new BuoiHocRow
                {
                    MaBuoi = b.MaBuoi,
                    MaLop = l.MaLop,
                    NgayHoc = b.NgayHoc,
                    MonHoc = l.MonHoc,
                    CaHoc = b.MaBuoiGoc.HasValue ? b.CaHoc : l.CaHoc,
                    GiaSu = b.MaBuoiGoc.HasValue ? ten(b.MaGSDay) : l.GiaSu?.HoTen,
                    MaGSDay = b.MaGSDay ?? l.MaGS ?? 0,
                    DiemDanh = b.DiemDanh,
                    Diem = b.Diem,
                    NhanXet = b.NhanXet,
                    LaHocBu = b.MaBuoiGoc.HasValue,
                    NgayVangGoc = b.MaBuoiGoc.HasValue && ngayCuaBuoi.TryGetValue(b.MaBuoiGoc.Value, out ngayGoc) ? ngayGoc : (DateTime?)null,
                    MaBuoiBu = bb?.MaBuoi,
                    NgayBu = bb?.NgayHoc,
                    CaBu = bb?.CaHoc
                };
            })
            .OrderByDescending(r => r.NgayHoc)
            .ThenByDescending(r => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, r.CaHoc))
            .ToList();

            return new KetQuaLop
            {
                MaLop = l.MaLop,
                MaGS = l.MaGS ?? 0,
                MonHoc = l.MonHoc,
                GiaSu = l.GiaSu?.HoTen,
                CaHoc = l.CaHoc,
                BatDau = l.ThoiGianBatDau,
                KetThuc = l.ThoiGianKetThuc,
                TrangThai = l.TrangThaiTheoThoiGian,
                SoBuoi = thuong.Count,
                SoBuoiCoMat = coMat,
                SoBuoiVang = vang,
                SoBuoiHocBu = daBu,
                SoBaiTest = diem.Count,
                DiemTrungBinh = tb,
                XepLoai = XepLoai(tb),
                Buoi = rows
            };
        }

        /// <summary>Học viên có lớp; gia sư chỉ thấy học viên của mình (kể cả học viên mình dạy bù)</summary>
        public List<HocVien> GetHocVienCoLop(int? maGS = null)
        {
            using (var db = new GiaSuContextDB())
            {
                var q = db.LopHocs.Where(l => l.MaHV != null);
                if (maGS.HasValue)
                {
                    var lopBu = db.BuoiHocs.Where(b => b.MaGSDay == maGS.Value).Select(b => b.MaLop).Distinct().ToList();
                    q = q.Where(l => l.MaGS == maGS.Value || lopBu.Contains(l.MaLop));
                }
                var ids = q.Select(l => l.MaHV.Value).Distinct().ToList();
                return db.HocViens.Where(h => ids.Contains(h.MaHV)).OrderBy(h => h.HoTen).ToList();
            }
        }

        // ===================================================================
        // HỌC BÙ
        // ===================================================================

        /// <summary>Ai đang bận ở một ngày + ca: lớp đang học ca đó và các buổi bù đã xếp vào ca đó</summary>
        private class LichBan
        {
            public int? MaHV, MaGS, MaLop, MaBuoi;
            public string MoTa;
        }

        private static List<LichBan> LichTrongCa(GiaSuContextDB db, DateTime ngay, string ca)
        {
            var d = ngay.Date;
            var dSau = d.AddDays(1);
            var huy = Constants.LopHocStatus.DaHuy;
            var ds = db.LopHocs.Include(l => l.HocVien)
                .Where(l => l.TrangThai != huy && l.CaHoc == ca && l.ThoiGianBatDau < dSau && l.ThoiGianKetThuc >= d)
                .ToList()
                .Select(l => new LichBan
                {
                    MaHV = l.MaHV, MaGS = l.MaGS, MaLop = l.MaLop,
                    MoTa = $"lớp {l.MonHoc} của {l.HocVien?.HoTen}"
                })
                .ToList();

            var bu = db.BuoiHocs.Where(b => b.NgayHoc == d && b.MaBuoiGoc != null && b.CaHoc == ca).ToList();
            if (bu.Count > 0)
            {
                var lopIds = bu.Select(b => b.MaLop).Distinct().ToList();
                var lop = db.LopHocs.Include(l => l.HocVien).Where(l => lopIds.Contains(l.MaLop)).ToList().ToDictionary(l => l.MaLop);
                foreach (var b in bu)
                {
                    LopHoc l;
                    lop.TryGetValue(b.MaLop, out l);
                    ds.Add(new LichBan
                    {
                        MaHV = l?.MaHV, MaGS = b.MaGSDay, MaLop = b.MaLop, MaBuoi = b.MaBuoi,
                        MoTa = $"buổi học bù {l?.MonHoc} của {l?.HocVien?.HoTen}"
                    });
                }
            }
            return ds;
        }

        /// <summary>Gia sư có thể dạy bù (bỏ người tạm nghỉ), kèm lý do bận ở ngày + ca đó</summary>
        public List<GiaSuHocBu> GetGiaSuChoHocBu(DateTime ngay, string ca)
        {
            using (var db = new GiaSuContextDB())
            {
                var tamNghi = Constants.GiaSuStatus.TamNghi;
                var lich = string.IsNullOrEmpty(ca) ? new List<LichBan>() : LichTrongCa(db, ngay, ca);
                return db.GiaSus.Where(g => g.TrangThai != tamNghi).OrderBy(g => g.HoTen).ToList()
                    .Select(g => new GiaSuHocBu
                    {
                        MaGS = g.MaGS,
                        HoTen = g.HoTen,
                        ChuyenMon = g.ChuyenMon,
                        Ban = lich.Where(x => x.MaGS == g.MaGS).Select(x => x.MoTa).FirstOrDefault()
                    })
                    .ToList();
            }
        }

        /// <summary>
        /// Xếp lịch học bù cho một buổi vắng.
        ///  - Lỗi (không xếp): buổi không phải buổi vắng / đã có lịch bù, ca không hợp lệ,
        ///    gia sư tạm nghỉ, học viên đã có lớp hoặc buổi bù khác cùng ngày + ca.
        ///  - Cảnh báo (cần xác nhận, gọi lại với boQuaCanhBao = true): gia sư đang dạy ca đó (học chung).
        /// </summary>
        public KetQuaHocBu XepHocBu(int maBuoiVang, DateTime ngay, string ca, int maGSDay,
            bool boQuaCanhBao = false, int? chiChoGiaSu = null)
        {
            using (var db = new GiaSuContextDB())
            {
                var goc = db.BuoiHocs.FirstOrDefault(b => b.MaBuoi == maBuoiVang);
                if (goc == null) return new KetQuaHocBu { Loi = "Không tìm thấy buổi vắng." };
                if (goc.MaBuoiGoc.HasValue) return new KetQuaHocBu { Loi = "Buổi này đã là buổi học bù." };
                if (Constants.DiemDanh.CoHoc(goc.DiemDanh)) return new KetQuaHocBu { Loi = "Chỉ xếp học bù cho buổi vắng." };
                if (db.BuoiHocs.Any(b => b.MaBuoiGoc == maBuoiVang))
                    return new KetQuaHocBu { Loi = "Buổi vắng này đã có lịch học bù. Hủy lịch cũ trước nếu muốn đổi." };

                var lop = db.LopHocs.FirstOrDefault(l => l.MaLop == goc.MaLop);
                if (lop == null) return new KetQuaHocBu { Loi = "Lớp của buổi này không còn tồn tại." };
                if (chiChoGiaSu.HasValue && lop.MaGS != chiChoGiaSu.Value)
                    return new KetQuaHocBu { Loi = "Chỉ xếp học bù cho học viên lớp của bạn." };
                if (!Constants.CaHoc.TatCaCaHoc.Contains(ca)) return new KetQuaHocBu { Loi = "Chọn ca học bù." };
                if (lop.ThoiGianBatDau.HasValue && ngay.Date < lop.ThoiGianBatDau.Value.Date)
                    return new KetQuaHocBu { Loi = $"Ngày học bù phải từ ngày lớp bắt đầu ({lop.ThoiGianBatDau:dd/MM/yyyy})." };
                if (ngay.Date > DateTime.Today.AddDays(60))
                    return new KetQuaHocBu { Loi = "Chỉ xếp học bù trong vòng 60 ngày tới." };

                var gs = db.GiaSus.FirstOrDefault(g => g.MaGS == maGSDay);
                if (gs == null) return new KetQuaHocBu { Loi = "Chọn gia sư dạy bù." };
                if (gs.TrangThai == Constants.GiaSuStatus.TamNghi)
                    return new KetQuaHocBu { Loi = $"Gia sư {gs.HoTen} đang tạm nghỉ." };

                var lich = LichTrongCa(db, ngay, ca);
                var hvBan = lich.FirstOrDefault(x => x.MaHV.HasValue && x.MaHV == lop.MaHV);
                if (hvBan != null)
                    return new KetQuaHocBu { Loi = $"Học viên đã có {hvBan.MoTa} vào {Constants.CaHoc.TenNgan(ca)} ngày {ngay:dd/MM}." };

                var gsBan = lich.FirstOrDefault(x => x.MaGS == maGSDay);
                if (gsBan != null && !boQuaCanhBao)
                    return new KetQuaHocBu
                    {
                        CanhBao = $"Gia sư {gs.HoTen} đang có {gsBan.MoTa} vào {Constants.CaHoc.TenNgan(ca)} ngày {ngay:dd/MM}.\n" +
                                  "Vẫn xếp học bù (học chung buổi đó)?"
                    };

                var moi = new BuoiHoc
                {
                    MaLop = lop.MaLop,
                    NgayHoc = ngay.Date,
                    CaHoc = ca,
                    MaGSDay = maGSDay,
                    MaBuoiGoc = maBuoiVang,
                    DiemDanh = Constants.DiemDanh.CoMat
                };
                db.BuoiHocs.Add(moi);
                db.SaveChanges();
                return new KetQuaHocBu { MaBuoi = moi.MaBuoi };
            }
        }

        /// <summary>Hủy lịch học bù (xóa buổi bù); buổi vắng gốc giữ nguyên</summary>
        public void HuyHocBu(int maBuoiBu, int? chiChoGiaSu = null)
        {
            using (var db = new GiaSuContextDB())
            {
                var b = db.BuoiHocs.FirstOrDefault(x => x.MaBuoi == maBuoiBu);
                if (b == null) return;
                if (!b.MaBuoiGoc.HasValue) throw new InvalidOperationException("Đây là buổi học thường, không phải buổi học bù.");
                if (chiChoGiaSu.HasValue)
                {
                    var lop = db.LopHocs.FirstOrDefault(l => l.MaLop == b.MaLop);
                    if (lop?.MaGS != chiChoGiaSu.Value && b.MaGSDay != chiChoGiaSu.Value)
                        throw new InvalidOperationException("Chỉ hủy được buổi bù của lớp bạn hoặc buổi bạn dạy.");
                }
                db.BuoiHocs.Remove(b);
                db.SaveChanges();
            }
        }
    }
}
