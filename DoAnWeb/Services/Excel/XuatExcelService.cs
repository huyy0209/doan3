using DoAnWeb.Services.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using DoAnWeb.Services.Dto;
using DoAnWeb.ViewModels.BaoCao;

namespace DoAnWeb.Services.Excel;


/// <summary>
/// Xuất báo cáo ra Excel (.xlsx) bằng ClosedXML (đề xuất #4):
/// công nợ học phí, nhắc nợ học phí, danh sách lớp, kết quả học tập của một học viên.
/// Dịch vụ thuần dữ liệu – controller quyết định bộ lọc và quyền xem.
/// </summary>
public class XuatExcelService
{
    private const string DinhDangTien = "#,##0";

    /// <summary>Xuất báo cáo tháng thành các trang tính riêng theo từng phần.</summary>
    public byte[] BaoCaoThang(BaoCaoThangViewModel baoCao)
    {
        using var wb = new XLWorkbook();

        var tong = wb.Worksheets.Add("Tổng hợp");
        TieuDe(tong, "Chỉ tiêu", "Giá trị");
        tong.Cell(2, 1).Value = "Tháng";
        tong.Cell(2, 2).Value = $"{baoCao.Thang:D2}/{baoCao.Nam}";
        tong.Cell(3, 1).Value = "Học phí thu trong tháng";
        tong.Cell(3, 2).Value = baoCao.HocPhiThu;
        tong.Cell(4, 1).Value = "Học viên mới";
        tong.Cell(4, 2).Value = baoCao.HocVienMoi;
        tong.Cell(5, 1).Value = "Học viên nghỉ";
        tong.Cell(5, 2).Value = baoCao.HocVienNghi;
        tong.Cell(6, 1).Value = "Nợ cuối tháng";
        tong.Cell(6, 2).Value = baoCao.NoCuoiThang;
        tong.Range(3, 2, 3, 2).Style.NumberFormat.Format = DinhDangTien;
        tong.Range(6, 2, 6, 2).Style.NumberFormat.Format = DinhDangTien;
        HoanThien(tong, 2);

        var giaSu = wb.Worksheets.Add("Theo gia sư");
        TieuDe(giaSu, "Gia sư", "Lớp đang dạy", "Buổi đã điểm danh", "Buổi vắng/nghỉ", "Chuyên cần (%)");
        int dong = 2;
        foreach (var g in baoCao.TheoGiaSu)
        {
            giaSu.Cell(dong, 1).Value = Chuoi(g.HoTen);
            giaSu.Cell(dong, 2).Value = g.SoLopDangDay;
            giaSu.Cell(dong, 3).Value = g.SoBuoiDaDiemDanh;
            giaSu.Cell(dong, 4).Value = g.SoBuoiVang;
            giaSu.Cell(dong, 5).Value = g.TyLeChuyenCan;
            dong++;
        }
        HoanThien(giaSu, 5);

        var mon = wb.Worksheets.Add("Theo môn");
        TieuDe(mon, "Môn học", "Số lớp", "Số học viên", "Học phí thu trong tháng");
        dong = 2;
        foreach (var m in baoCao.TheoMon)
        {
            mon.Cell(dong, 1).Value = Chuoi(m.TenMon);
            mon.Cell(dong, 2).Value = m.SoLop;
            mon.Cell(dong, 3).Value = m.SoHocVien;
            mon.Cell(dong, 4).Value = m.HocPhiThu;
            dong++;
        }
        if (baoCao.TheoMon.Count > 0)
            mon.Range(2, 4, dong - 1, 4).Style.NumberFormat.Format = DinhDangTien;
        HoanThien(mon, 4);

        return Luu(wb);
    }

    // ===================================================================
    // 1. Công nợ học phí
    // ===================================================================

    /// <summary>Công nợ theo lớp: tiêu đề in đậm, tiền dạng #,##0, dòng tổng cuối bảng.</summary>
    public byte[] CongNo(IEnumerable<CongNoLop> danhSach)
    {
        var ds = (danhSach ?? Enumerable.Empty<CongNoLop>()).ToList();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Công nợ");

        TieuDe(ws, "Lớp", "Học viên", "SĐT liên hệ", "Môn", "Gia sư",
            "Học phí", "Đã đóng", "Còn nợ", "Tình trạng");

        int dong = 2;
        foreach (var c in ds)
        {
            ws.Cell(dong, 1).Value = Chuoi(c.TenLop);
            ws.Cell(dong, 2).Value = Chuoi(c.HocVien);
            ws.Cell(dong, 3).Value = Chuoi(c.SDTLienHe);
            ws.Cell(dong, 4).Value = Chuoi(c.MonHoc);
            ws.Cell(dong, 5).Value = Chuoi(c.GiaSu);
            ws.Cell(dong, 6).Value = c.HocPhi;
            ws.Cell(dong, 7).Value = c.DaDong;
            ws.Cell(dong, 8).Value = c.ConNo;
            ws.Cell(dong, 9).Value = Chuoi(c.TinhTrang);
            dong++;
        }

        int dongTong = dong;
        ws.Cell(dongTong, 1).Value = "Tổng cộng";
        ws.Cell(dongTong, 2).Value = $"{ds.Count} lớp";
        ws.Cell(dongTong, 6).Value = ds.Sum(c => c.HocPhi);
        ws.Cell(dongTong, 7).Value = ds.Sum(c => c.DaDong);
        ws.Cell(dongTong, 8).Value = ds.Sum(c => c.ConNo);
        ws.Range(dongTong, 1, dongTong, 9).Style.Font.Bold = true;

        ws.Range(2, 6, dongTong, 8).Style.NumberFormat.Format = DinhDangTien;
        HoanThien(ws, 9);
        return Luu(wb);
    }

    // ===================================================================
    // 2. Nhắc nợ học phí
    // ===================================================================

    /// <summary>
    /// Danh sách nhắc nợ: giống công nợ nhưng thêm "Lần đóng gần nhất" và "Số ngày chưa đóng".
    /// Số tiền do <see cref="NhacNoDong"/> mang sẵn từ trang Học phí – service này không tính lại.
    /// </summary>
    public byte[] NhacNo(IEnumerable<NhacNoDong> danhSach)
    {
        var ds = (danhSach ?? Enumerable.Empty<NhacNoDong>()).ToList();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Nhắc nợ");

        TieuDe(ws, "Lớp", "Học viên", "SĐT liên hệ", "Môn",
            "Tổng phải đóng", "Đã đóng", "Còn nợ", "Lần đóng gần nhất", "Số ngày chưa đóng");

        int dong = 2;
        foreach (var c in ds)
        {
            ws.Cell(dong, 1).Value = Chuoi(c.TenLop);
            ws.Cell(dong, 2).Value = Chuoi(c.HocVien);
            ws.Cell(dong, 3).Value = Chuoi(c.SDTLienHe);
            ws.Cell(dong, 4).Value = Chuoi(c.MonHoc);
            ws.Cell(dong, 5).Value = c.PhaiDong;
            ws.Cell(dong, 6).Value = c.DaDong;
            ws.Cell(dong, 7).Value = c.ConNo;
            if (c.LanDongGanNhat.HasValue)
            {
                ws.Cell(dong, 8).Value = c.LanDongGanNhat.Value.Date;
                ws.Cell(dong, 8).Style.NumberFormat.Format = "dd/MM/yyyy";
            }
            else
            {
                ws.Cell(dong, 8).Value = "Chưa đóng lần nào";
            }
            ws.Cell(dong, 9).Value = c.SoNgayChuaDong;
            dong++;
        }

        int dongTong = dong;
        ws.Cell(dongTong, 1).Value = "Tổng cộng";
        ws.Cell(dongTong, 2).Value = $"{ds.Count} lớp";
        ws.Cell(dongTong, 5).Value = ds.Sum(c => c.PhaiDong);
        ws.Cell(dongTong, 6).Value = ds.Sum(c => c.DaDong);
        ws.Cell(dongTong, 7).Value = ds.Sum(c => c.ConNo);
        ws.Range(dongTong, 1, dongTong, 9).Style.Font.Bold = true;

        ws.Range(2, 5, dongTong, 7).Style.NumberFormat.Format = DinhDangTien;
        HoanThien(ws, 9);
        return Luu(wb);
    }

    // ===================================================================
    // 3. Danh sách lớp
    // ===================================================================

    /// <summary>Danh sách lớp theo bộ lọc hiện tại của trang Lớp học.</summary>
    public byte[] DanhSachLop(IEnumerable<LopHocDong> danhSach)
    {
        var ds = (danhSach ?? Enumerable.Empty<LopHocDong>()).ToList();
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Danh sách lớp");

        TieuDe(ws, "Lớp", "Học viên", "Gia sư", "Môn", "Loại môn", "Ca",
            "Thứ học", "Số buổi", "Bắt đầu", "Kết thúc", "Trạng thái", "Học phí", "Điểm TB", "Đánh giá");

        int dong = 2;
        foreach (var l in ds)
        {
            ws.Cell(dong, 1).Value = Chuoi(l.TenLop);
            ws.Cell(dong, 2).Value = Chuoi(l.HocVien);
            ws.Cell(dong, 3).Value = Chuoi(l.GiaSu);
            ws.Cell(dong, 4).Value = Chuoi(l.MonHoc);
            ws.Cell(dong, 5).Value = Chuoi(l.LoaiMon);
            ws.Cell(dong, 6).Value = Chuoi(l.CaHoc);
            ws.Cell(dong, 7).Value = Chuoi(l.ThuHoc);
            ws.Cell(dong, 8).Value = l.SoBuoi;
            ws.Cell(dong, 9).Value = l.BatDau.HasValue ? l.BatDau.Value.ToString("dd/MM/yyyy") : "";
            ws.Cell(dong, 10).Value = l.KetThuc.HasValue ? l.KetThuc.Value.ToString("dd/MM/yyyy") : "";
            ws.Cell(dong, 11).Value = Chuoi(l.TrangThai);
            ws.Cell(dong, 12).Value = l.HocPhi;
            if (l.DiemTB.HasValue) ws.Cell(dong, 13).Value = l.DiemTB.Value;
            ws.Cell(dong, 14).Value = Chuoi(l.DanhGia);
            dong++;
        }

        if (ds.Count > 0)
            ws.Range(2, 12, dong - 1, 12).Style.NumberFormat.Format = DinhDangTien;
        HoanThien(ws, 14);
        return Luu(wb);
    }

    // ===================================================================
    // 4. Kết quả học tập
    // ===================================================================

    /// <summary>
    /// Kết quả học tập một học viên: sheet "Tổng hợp" trước, mỗi lớp một sheet
    /// (buổi, điểm danh, điểm, nhận xét).
    /// </summary>
    public byte[] KetQuaHocTap(string tenHocVien, IEnumerable<KetQuaLop> cacLop)
    {
        var ds = (cacLop ?? Enumerable.Empty<KetQuaLop>()).ToList();
        using var wb = new XLWorkbook();

        var tong = wb.Worksheets.Add("Tổng hợp");
        TieuDe(tong, "Học viên", "Lớp", "Môn", "Gia sư", "Ca", "Số buổi", "Có mặt",
            "Vắng", "Học bù", "Bài test", "Điểm TB", "Đánh giá", "Bắt đầu", "Kết thúc", "Trạng thái");
        int dong = 2;
        foreach (var k in ds)
        {
            tong.Cell(dong, 1).Value = Chuoi(tenHocVien);
            tong.Cell(dong, 2).Value = Chuoi(k.TenLop);
            tong.Cell(dong, 3).Value = Chuoi(k.MonHoc);
            tong.Cell(dong, 4).Value = Chuoi(k.GiaSu);
            tong.Cell(dong, 5).Value = Chuoi(k.CaHoc);
            tong.Cell(dong, 6).Value = k.SoBuoi;
            tong.Cell(dong, 7).Value = k.SoBuoiCoMat;
            tong.Cell(dong, 8).Value = k.SoBuoiVang;
            tong.Cell(dong, 9).Value = k.SoBuoiHocBu;
            tong.Cell(dong, 10).Value = k.SoBaiTest;
            if (k.DiemTrungBinh.HasValue) tong.Cell(dong, 11).Value = k.DiemTrungBinh.Value;
            tong.Cell(dong, 12).Value = Chuoi(k.DanhGia);
            tong.Cell(dong, 13).Value = k.BatDau.HasValue ? k.BatDau.Value.ToString("dd/MM/yyyy") : "";
            tong.Cell(dong, 14).Value = k.KetThuc.HasValue ? k.KetThuc.Value.ToString("dd/MM/yyyy") : "";
            tong.Cell(dong, 15).Value = Chuoi(k.TrangThai);
            dong++;
        }
        HoanThien(tong, 15);

        var daDung = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Tổng hợp" };
        foreach (var k in ds)
        {
            var ws = wb.Worksheets.Add(TenSheet(k.TenLop ?? k.MonHoc, daDung));
            TieuDe(ws, "Buổi", "Điểm danh", "Điểm", "Nhận xét");
            int r = 2;
            foreach (var b in (k.Buoi ?? new List<BuoiHocRow>()).OrderBy(b => b.NgayHoc).ThenBy(b => b.MaBuoi))
            {
                ws.Cell(r, 1).Value = $"{b.NgayHoc:dd/MM/yyyy}" +
                    (string.IsNullOrWhiteSpace(b.CaHoc) ? "" : " · " + b.CaHoc);
                ws.Cell(r, 2).Value = Chuoi(b.DiemDanhHienThi);
                if (b.Diem.HasValue) ws.Cell(r, 3).Value = b.Diem.Value;
                ws.Cell(r, 4).Value = Chuoi(b.NhanXet);
                r++;
            }
            HoanThien(ws, 4);
        }

        return Luu(wb);
    }

    // ===================================================================
    // Phần dùng chung
    // ===================================================================

    /// <summary>Ghi tiêu đề in đậm, cố định hàng đầu.</summary>
    private static void TieuDe(IXLWorksheet ws, params string[] ten)
    {
        for (int i = 0; i < ten.Length; i++)
        {
            var o = ws.Cell(1, i + 1);
            o.Value = ten[i];
            o.Style.Font.Bold = true;
            o.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
        ws.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E9EEEB");
        ws.SheetView.FreezeRows(1);
    }

    /// <summary>Tự giãn độ rộng cột theo nội dung.</summary>
    private static void HoanThien(IXLWorksheet ws, int soCot)
    {
        ws.Columns(1, soCot).AdjustToContents();
        ws.Column(1).Width = Math.Max(ws.Column(1).Width, 12);
    }

    private static byte[] Luu(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Chặn CSV/Excel formula injection như phần xuất CSV: ô bắt đầu bằng =, +, -, @, tab, CR
    /// được ghi thành chuỗi, thêm dấu nháy đơn ở đầu để Excel không hiểu là công thức.
    /// </summary>
    private static string Chuoi(string s)
    {
        s = s ?? "";
        if (s.Length > 0 && s[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
            s = "'" + s;
        return s;
    }

    /// <summary>Tên sheet hợp lệ (≤ 31 ký tự, không trùng), tự thêm (2), (3)…</summary>
    private static string TenSheet(string ten, HashSet<string> daDung)
    {
        ten = string.IsNullOrWhiteSpace(ten) ? "Lớp" : ten.Trim();
        var sb = new StringBuilder(ten.Length);
        foreach (var ch in ten)
            sb.Append(@"\/?*[]:".IndexOf(ch) >= 0 ? ' ' : ch);
        string goc = sb.ToString().Trim();
        if (goc.Length == 0) goc = "Lớp";
        if (goc.Length > 31) goc = goc.Substring(0, 31).TrimEnd();

        string kq = goc;
        int i = 2;
        while (!daDung.Add(kq))
        {
            string hau = $" ({i++})";
            kq = goc.Length + hau.Length > 31
                ? goc.Substring(0, 31 - hau.Length) + hau
                : goc + hau;
        }
        return kq;
    }
}