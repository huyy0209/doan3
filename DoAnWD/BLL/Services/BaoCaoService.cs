using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using DoAnWD.BLL.Dto;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    /// <summary>
    /// Xuất báo cáo không cần thư viện ngoài:
    ///  - Phiếu báo kết quả học tập (HTML, mở bằng trình duyệt, in ra PDF bằng Ctrl+P)
    ///  - Bảng dữ liệu ra CSV (UTF-8 có BOM => Excel đọc đúng tiếng Việt)
    /// </summary>
    public class BaoCaoService
    {
        private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

        private static string H(string s) => WebUtility.HtmlEncode(s ?? "");

        /// <summary>Tạo phiếu báo kết quả và trả về đường dẫn file HTML</summary>
        public string TaoPhieuBao(HocVien hv, List<KetQuaLop> ketQua, string thuMuc = null)
        {
            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html lang=""vi""><head><meta charset=""utf-8"">
<title>Phiếu báo kết quả học tập</title>
<style>
  body{font-family:'Be Vietnam Pro','Segoe UI',Arial,sans-serif;color:#1B2A26;background:#F2F5F3;margin:0;padding:32px}
  .page{max-width:820px;margin:auto;background:#fff;padding:36px 48px 40px;border:1px solid #DFE6E2;border-top:6px solid #1D4A40}
  .tt{display:flex;justify-content:space-between;align-items:flex-end;gap:24px;border-bottom:1px solid #DFE6E2;padding-bottom:14px;margin-bottom:22px}
  .tt b{font-size:18px;font-weight:600;color:#1D4A40}.tt span{color:#5C6C66;font-size:13px;text-align:right}
  h1{font-weight:600;font-size:24px;margin:0 0 4px;color:#1B7A5E}
  .sub{color:#5C6C66;margin:0 0 24px}
  table{border-collapse:collapse;width:100%;margin:8px 0 20px}
  th{text-align:left;color:#1B7A5E;font-weight:600;font-size:13px;border-bottom:2px solid #DFE6E2;padding:8px 6px}
  td{border-bottom:1px solid #E9EEEB;padding:7px 6px;font-size:14px}
  .info td{border:none;padding:3px 12px 3px 0}
  .info td:first-child{color:#5C6C66;width:150px}
  h2{font-size:17px;font-weight:600;margin:28px 0 4px;padding-left:10px;border-left:3px solid #1B7A5E}
  .tong{display:flex;gap:28px;margin:8px 0 6px;flex-wrap:wrap}
  .tong div{min-width:120px}.tong b{display:block;font-size:22px;font-weight:600;color:#1B7A5E}
  .tong span{color:#5C6C66;font-size:13px}
  .vang{color:#C4403C}.num{text-align:right}
  .ky{display:flex;justify-content:space-between;margin-top:40px;color:#5C6C66}
  @media print{body{background:#fff;padding:0}.page{border:none}}
</style></head><body><div class=""page"">");

            // Đầu phiếu: tên trung tâm + liên hệ (lấy từ Cài đặt trung tâm)
            var tt = CaiDatService.TrungTam;
            sb.AppendFormat("<div class=\"tt\"><b>{0}</b><span>{1}</span></div>", H(tt.TenHienThi),
                string.Join("<br>", new[] { tt.DiaChi, tt.SDT }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(H)));

            sb.Append("<h1>Phiếu báo kết quả học tập</h1>");
            sb.AppendFormat("<p class=\"sub\">Ngày lập: {0:dd/MM/yyyy}</p>", DateTime.Now);

            sb.Append("<table class=\"info\">");
            Dong(sb, "Học viên", hv.HoTen);
            Dong(sb, "Trường / lớp", hv.TruongLop);
            Dong(sb, "Trình độ", hv.TrinhDo);
            Dong(sb, "Phụ huynh", string.Join(", ", new[] { hv.TenPhuHuynh, hv.SDTPhuHuynh }.Where(x => !string.IsNullOrWhiteSpace(x))));
            sb.Append("</table>");

            if (ketQua.Count == 0)
                sb.Append("<p>Học viên chưa có lớp học nào.</p>");

            foreach (var kq in ketQua)
            {
                sb.AppendFormat("<h2>{0}</h2>", H(kq.TenLop ?? kq.MonHoc));
                sb.AppendFormat("<p class=\"sub\">Môn {0} ({1}), gia sư {2}</p>", H(kq.MonHoc), H(kq.LoaiMon?.ToLower()), H(kq.GiaSu));
                sb.AppendFormat("<p class=\"sub\">{0}, từ {1:dd/MM/yyyy} đến {2:dd/MM/yyyy}, {3}</p>",
                    H(kq.CaHoc), kq.BatDau, kq.KetThuc, H(kq.TrangThai?.ToLower()));

                sb.Append("<div class=\"tong\">");
                sb.AppendFormat("<div><b>{0}</b><span>điểm trung bình</span></div>",
                    kq.DiemTrungBinh.HasValue ? kq.DiemTrungBinh.Value.ToString("0.##", Vi) : "–");
                if (kq.TienTrinh != null)
                    sb.AppendFormat("<div><b>{0}</b><span>cấp độ hiện tại{1}</span></div>", H(kq.TienTrinh.CapHienTai),
                        kq.TienTrinh.CapKeTiep != null ? H(", tiếp theo: " + kq.TienTrinh.CapKeTiep) : "");
                else
                    sb.AppendFormat("<div><b>{0}</b><span>xếp loại</span></div>", H(string.IsNullOrEmpty(kq.XepLoai) ? "–" : kq.XepLoai));
                sb.AppendFormat("<div><b>{0}</b><span>bài test</span></div>", kq.SoBaiTest);
                sb.AppendFormat("<div><b>{0}/{1}</b><span>buổi có mặt</span></div>", kq.SoBuoiCoMat, kq.SoBuoi);
                if (kq.SoBuoiHocBu > 0)
                    sb.AppendFormat("<div><b>{0}</b><span>buổi đã học bù</span></div>", kq.SoBuoiHocBu);
                sb.Append("</div>");

                sb.Append(BieuDo(kq.Buoi));

                // Bỏ buổi bù chưa tới ngày; buổi vắng hiện kèm ngày học bù
                var coNoiDung = kq.Buoi.Where(b => !(b.LaHocBu && b.ChuaToiNgay))
                    .Where(b => b.Diem.HasValue || !string.IsNullOrEmpty(b.NhanXet) || b.Vang || b.LaHocBu || b.ThiLenCap).ToList();
                if (coNoiDung.Count > 0)
                {
                    sb.Append("<table><tr><th>Ngày</th><th>Điểm danh</th><th class=\"num\">Điểm test</th><th>Nhận xét của gia sư</th></tr>");
                    foreach (var b in coNoiDung)
                    {
                        bool vang = b.Vang && !b.MaBuoiBu.HasValue;
                        sb.AppendFormat("<tr><td>{0:dd/MM/yyyy}</td><td{1}>{2}</td><td class=\"num\">{3}</td><td>{4}</td></tr>",
                            b.NgayHoc, vang ? " class=\"vang\"" : "", H(b.DiemDanhHienThi + (b.ThiLenCap ? " · " + b.KetQuaThi : "")),
                            b.Diem.HasValue ? b.Diem.Value.ToString("0.##", Vi) : "", H(b.NhanXet));
                    }
                    sb.Append("</table>");
                }
            }

            sb.Append("<div class=\"ky\"><span>Gia sư</span><span>Phụ huynh</span></div>");
            sb.Append("</div></body></html>");

            string dir = thuMuc ?? Path.Combine(Path.GetTempPath(), "QuanLyGiaSu");
            Directory.CreateDirectory(dir);
            string ten = $"PhieuBao_{BoDau(hv.HoTen)}_{DateTime.Now:yyyyMMdd_HHmm}.html";
            string path = Path.Combine(dir, ten);
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
            return path;
        }

        private static void Dong(StringBuilder sb, string nhan, string giaTri)
        {
            if (string.IsNullOrWhiteSpace(giaTri)) return;
            sb.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", H(nhan), H(giaTri));
        }

        /// <summary>Biểu đồ đường điểm test theo thời gian (SVG), cần ít nhất 2 bài</summary>
        private static string BieuDo(List<BuoiHocRow> buoi)
        {
            var diem = buoi.Where(b => b.Diem.HasValue).OrderBy(b => b.NgayHoc).ToList();
            if (diem.Count < 2) return "";

            const int w = 700, h = 140, pad = 24;
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture,
                "<svg width=\"100%\" viewBox=\"0 0 {0} {1}\" role=\"img\" aria-label=\"Điểm test theo ngày\">", w, h + 22);
            foreach (int muc in new[] { 0, 5, 10 })
            {
                double y = pad + (10 - muc) / 10.0 * (h - 2 * pad);
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "<line x1=\"30\" x2=\"{0}\" y1=\"{1:0.#}\" y2=\"{1:0.#}\" stroke=\"#E9EEEB\"/><text x=\"0\" y=\"{2:0.#}\" font-size=\"11\" fill=\"#93A192\">{3}</text>",
                    w, y, y + 4, muc);
            }
            var pts = new List<string>();
            for (int i = 0; i < diem.Count; i++)
            {
                double x = 40 + i * (w - 60.0) / (diem.Count - 1);
                double y = pad + (10 - (double)diem[i].Diem.Value) / 10.0 * (h - 2 * pad);
                pts.Add(string.Format(CultureInfo.InvariantCulture, "{0:0.#},{1:0.#}", x, y));
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "<circle cx=\"{0:0.#}\" cy=\"{1:0.#}\" r=\"4\" fill=\"#1B7A5E\"><title>{2:dd/MM}: {3}</title></circle>",
                    x, y, diem[i].NgayHoc, diem[i].Diem.Value.ToString("0.##", Vi));
                if (i == 0 || i == diem.Count - 1)
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "<text x=\"{0:0.#}\" y=\"{1}\" font-size=\"11\" fill=\"#5C6C66\" text-anchor=\"middle\">{2:dd/MM}</text>", x, h + 16, diem[i].NgayHoc);
            }
            sb.AppendFormat("<polyline points=\"{0}\" fill=\"none\" stroke=\"#1B7A5E\" stroke-width=\"2\"/>", string.Join(" ", pts));
            sb.Append("</svg>");
            return sb.ToString();
        }

        /// <summary>Xuất các cột đang hiện của DataGridView ra CSV</summary>
        public void XuatCsv(DataGridView dgv, string path)
        {
            var cols = dgv.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToList();
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.HeaderText))));
            foreach (DataGridViewRow r in dgv.Rows)
            {
                if (r.IsNewRow) continue;
                sb.AppendLine(string.Join(",", cols.Select(c => Csv(r.Cells[c.Index].FormattedValue?.ToString()))));
            }
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        }

        private static string Csv(string s)
        {
            s = s ?? "";
            if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                s = "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        /// <summary>"Đỗ Bảo Ngọc" =&gt; "Do_Bao_Ngoc" (dùng cho tên file)</summary>
        public static string BoDau(string s)
        {
            if (string.IsNullOrEmpty(s)) return "HocVien";
            var norm = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in norm)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (cat == UnicodeCategory.NonSpacingMark) continue;
                if (ch == 'đ') sb.Append('d');
                else if (ch == 'Đ') sb.Append('D');
                else if (char.IsLetterOrDigit(ch)) sb.Append(ch);
                else if (ch == ' ') sb.Append('_');
            }
            return sb.ToString();
        }
    }
}
