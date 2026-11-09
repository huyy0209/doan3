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
  body{font-family:'Segoe UI',Arial,sans-serif;color:#23233A;background:#F6F7FB;margin:0;padding:32px}
  .page{max-width:820px;margin:auto;background:#fff;padding:36px 44px 40px 64px;position:relative;border:1px solid #DCDDEB}
  .page:before{content:'';position:absolute;left:36px;top:0;bottom:0;width:2px;background:#C8414B}
  h1{font-weight:600;font-size:24px;margin:0 0 4px;color:#4B3F9E}
  .sub{color:#6B6F86;margin:0 0 24px}
  table{border-collapse:collapse;width:100%;margin:8px 0 20px}
  th{text-align:left;color:#4B3F9E;font-weight:600;font-size:13px;border-bottom:2px solid #DCDDEB;padding:8px 6px}
  td{border-bottom:1px solid #ECEDF4;padding:7px 6px;font-size:14px}
  .info td{border:none;padding:3px 12px 3px 0}
  .info td:first-child{color:#6B6F86;width:150px}
  h2{font-size:17px;font-weight:600;margin:28px 0 4px;padding-left:10px;border-left:3px solid #4B3F9E}
  .tong{display:flex;gap:28px;margin:8px 0 6px;flex-wrap:wrap}
  .tong div{min-width:120px}.tong b{display:block;font-size:22px;font-weight:600;color:#4B3F9E}
  .tong span{color:#6B6F86;font-size:13px}
  .vang{color:#C8414B}.num{text-align:right}
  .ky{display:flex;justify-content:space-between;margin-top:40px;color:#6B6F86}
  @media print{body{background:#fff;padding:0}.page{border:none}}
</style></head><body><div class=""page"">");

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
                sb.AppendFormat("<h2>{0} với gia sư {1}</h2>", H(kq.MonHoc), H(kq.GiaSu));
                sb.AppendFormat("<p class=\"sub\">{0}, từ {1:dd/MM/yyyy} đến {2:dd/MM/yyyy}, {3}</p>",
                    H(kq.CaHoc), kq.BatDau, kq.KetThuc, H(kq.TrangThai?.ToLower()));

                sb.Append("<div class=\"tong\">");
                sb.AppendFormat("<div><b>{0}</b><span>điểm trung bình</span></div>",
                    kq.DiemTrungBinh.HasValue ? kq.DiemTrungBinh.Value.ToString("0.##", Vi) : "–");
                sb.AppendFormat("<div><b>{0}</b><span>xếp loại</span></div>", H(string.IsNullOrEmpty(kq.XepLoai) ? "–" : kq.XepLoai));
                sb.AppendFormat("<div><b>{0}</b><span>bài test</span></div>", kq.SoBaiTest);
                sb.AppendFormat("<div><b>{0}/{1}</b><span>buổi có mặt</span></div>", kq.SoBuoiCoMat, kq.SoBuoi);
                sb.Append("</div>");

                sb.Append(BieuDo(kq.Buoi));

                var coNoiDung = kq.Buoi.Where(b => b.Diem.HasValue || !string.IsNullOrEmpty(b.NhanXet)
                                                   || !Common.Constants.DiemDanh.CoHoc(b.DiemDanh)).ToList();
                if (coNoiDung.Count > 0)
                {
                    sb.Append("<table><tr><th>Ngày</th><th>Điểm danh</th><th class=\"num\">Điểm test</th><th>Nhận xét của gia sư</th></tr>");
                    foreach (var b in coNoiDung)
                    {
                        bool vang = !Common.Constants.DiemDanh.CoHoc(b.DiemDanh);
                        sb.AppendFormat("<tr><td>{0:dd/MM/yyyy}</td><td{1}>{2}</td><td class=\"num\">{3}</td><td>{4}</td></tr>",
                            b.NgayHoc, vang ? " class=\"vang\"" : "", H(b.DiemDanh),
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
                    "<line x1=\"30\" x2=\"{0}\" y1=\"{1:0.#}\" y2=\"{1:0.#}\" stroke=\"#ECEDF4\"/><text x=\"0\" y=\"{2:0.#}\" font-size=\"11\" fill=\"#9A9DB2\">{3}</text>",
                    w, y, y + 4, muc);
            }
            var pts = new List<string>();
            for (int i = 0; i < diem.Count; i++)
            {
                double x = 40 + i * (w - 60.0) / (diem.Count - 1);
                double y = pad + (10 - (double)diem[i].Diem.Value) / 10.0 * (h - 2 * pad);
                pts.Add(string.Format(CultureInfo.InvariantCulture, "{0:0.#},{1:0.#}", x, y));
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "<circle cx=\"{0:0.#}\" cy=\"{1:0.#}\" r=\"4\" fill=\"#4B3F9E\"><title>{2:dd/MM}: {3}</title></circle>",
                    x, y, diem[i].NgayHoc, diem[i].Diem.Value.ToString("0.##", Vi));
                if (i == 0 || i == diem.Count - 1)
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "<text x=\"{0:0.#}\" y=\"{1}\" font-size=\"11\" fill=\"#6B6F86\" text-anchor=\"middle\">{2:dd/MM}</text>", x, h + 16, diem[i].NgayHoc);
            }
            sb.AppendFormat("<polyline points=\"{0}\" fill=\"none\" stroke=\"#4B3F9E\" stroke-width=\"2\"/>", string.Join(" ", pts));
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
