using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.UI.Controls;

namespace DoAnWD.UI.Pages
{
    /// <summary>
    /// Trang Tổng quan của quản lý, trả lời ba câu: hôm nay dạy gì (bảng 6 ca),
    /// việc nào còn tồn (danh sách bấm được), tiền thu ra sao (4 ô số + biểu đồ 6 tháng).
    /// Mọi ô đều bấm được để mở đúng trang liên quan.
    /// </summary>
    public class frmTongQuan : Form, ITuGiaoDien, ILamMoi
    {
        private readonly TongQuanService _service = new TongQuanService();
        private TongQuanDuLieu _duLieu = new TongQuanDuLieu();

        private Label lblChao, lblTom, lblViecTrong, lblHetTrong;
        private BangHomNay bang;
        private TheSo theLop, theHocVien, theThu, theNo;
        private BieuDoThu bieuDo;
        private ListBox lstViec, lstSapHet;

        public frmTongQuan()
        {
            BackColor = Theme.Paper;
            Font = Theme.Body;
            Padding = new Padding(22, 16, 8, 16);
            DungGiaoDien();
            Load += (s, e) => LamMoi();
        }

        // ===================================================================
        // GIAO DIỆN
        // ===================================================================

        private void DungGiaoDien()
        {
            // ---- Lời chào + thao tác nhanh ----
            lblChao = Ui.Title("Tổng quan");
            lblTom = Ui.Text("", true);
            lblTom.Margin = new Padding(1, 0, 0, 0);
            var chu = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Dock = DockStyle.Fill, Margin = Padding.Empty };
            chu.Controls.Add(lblChao);
            chu.Controls.Add(lblTom);

            var btnDiemDanh = Ui.Button("Điểm danh hôm nay", ButtonKind.Secondary, Glyph.Clipboard);
            var btnGhep = Ui.Button("Ghép lớp mới", ButtonKind.Primary, Glyph.Plus);
            btnGhep.Margin = new Padding(0, 4, 14, 4);
            btnDiemDanh.Click += (s, e) => DieuHuong.Mo<frmHocVien>()?.MoDiemDanh(DateTime.Today, null);
            btnGhep.Click += (s, e) => DieuHuong.Mo<frmLopHoc>()?.GhepLopMoi();
            var nut = Ui.Row(btnDiemDanh, btnGhep);
            nut.Dock = DockStyle.None;
            nut.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

            var dau = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true, Margin = new Padding(0, 0, 0, 12) };
            dau.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            dau.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dau.Controls.Add(chu, 0, 0);
            dau.Controls.Add(nut, 1, 0);

            // ---- Bảng hôm nay + việc cần xử lý ----
            bang = new BangHomNay();
            bang.ChonCa += ca => DieuHuong.Mo<frmHocVien>()?.MoDiemDanh(DateTime.Today, ca);

            lstViec = Ui.List(56, VeViec);
            lblViecTrong = NhanTrong("Không còn việc tồn.\nSổ sách hôm nay đang gọn.");
            var oViec = Ui.Section("Cần xử lý");
            oViec.Controls.Add(lstViec);
            oViec.Controls.Add(lblViecTrong);
            BamVaoDong(lstViec, i => MoViec(_duLieu.Viec[i]));

            // ---- Bốn ô số ----
            theLop = new TheSo("Lớp đang học", Glyph.Book);
            theHocVien = new TheSo("Học viên đang học", Glyph.Users);
            theThu = new TheSo("Đã thu tháng này", Glyph.Money);
            theNo = new TheSo("Còn phải thu", Glyph.Clock);
            theLop.Click += (s, e) => DieuHuong.Mo<frmLopHoc>()?.LocTrangThai(Constants.LopHocStatus.DangDienRa);
            theHocVien.Click += (s, e) => DieuHuong.Mo<frmHocVien>()?.LocTinhTrang("Đang học");
            theThu.Click += (s, e) => DieuHuong.Mo<frmHocPhi>();
            theNo.Click += (s, e) => DieuHuong.Mo<frmHocPhi>();
            var so = Luoi(4, theLop, theHocVien, theThu, theNo);
            so.Margin = new Padding(0, 14, 0, 14);

            // ---- Biểu đồ thu + lớp sắp kết thúc ----
            bieuDo = new BieuDoThu();
            var oThu = Ui.Section("Học phí đã thu 6 tháng gần đây");
            oThu.Controls.Add(bieuDo);

            lstSapHet = Ui.List(52, VeSapHet);
            lblHetTrong = NhanTrong($"Không có lớp nào kết thúc\ntrong {TongQuanService.SoNgayBaoTruoc} ngày tới.");
            var oHet = Ui.Section("Lớp sắp kết thúc");
            oHet.Controls.Add(lstSapHet);
            oHet.Controls.Add(lblHetTrong);
            BamVaoDong(lstSapHet, i => DieuHuong.Mo<frmLopHoc>()?.ChonLop(_duLieu.SapKetThuc[i].MaLop));

            var tren = HaiCot(bang, oViec);
            var duoi = HaiCot(oThu, oHet);

            var khung = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = Padding.Empty };
            khung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            khung.RowStyles.Add(new RowStyle(SizeType.Percent, 57));
            khung.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            khung.RowStyles.Add(new RowStyle(SizeType.Percent, 43));
            khung.Controls.Add(dau, 0, 0);
            khung.Controls.Add(tren, 0, 1);
            khung.Controls.Add(so, 0, 2);
            khung.Controls.Add(duoi, 0, 3);
            ThemeManager.EnableDoubleBuffer(khung);
            Controls.Add(khung);
        }

        /// <summary>Hai cột 64% / 36%</summary>
        private static TableLayoutPanel HaiCot(Control trai, Control phai)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            t.Controls.Add(trai, 0, 0);
            t.Controls.Add(phai, 1, 0);
            ThemeManager.EnableDoubleBuffer(t);
            return t;
        }

        private static TableLayoutPanel Luoi(int soCot, params Control[] cacO)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = soCot, RowCount = 1, AutoSize = true };
            for (int i = 0; i < soCot; i++) t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / soCot));
            for (int i = 0; i < cacO.Length; i++) t.Controls.Add(cacO[i], i, 0);
            ThemeManager.EnableDoubleBuffer(t);
            return t;
        }

        private static Label NhanTrong(string chu)
        {
            return new Label
            {
                Text = chu, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = Theme.Body,
                ForeColor = Theme.TextFaint, BackColor = Theme.Surface, Visible = false, UseMnemonic = false
            };
        }

        /// <summary>Danh sách dạng "bấm vào dòng để mở": không giữ dòng chọn, con trỏ hình bàn tay</summary>
        private static void BamVaoDong(ListBox lst, Action<int> mo)
        {
            lst.Cursor = Cursors.Hand;
            lst.MouseClick += (s, e) =>
            {
                int i = lst.IndexFromPoint(e.Location);
                lst.ClearSelected();
                if (i >= 0 && i < lst.Items.Count) mo(i);
            };
        }

        // ===================================================================
        // DỮ LIỆU
        // ===================================================================

        public void LamMoi()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _duLieu = _service.Lay();
            }
            catch (Exception ex)
            {
                lblTom.Text = "Không tải được số liệu: " + ex.GetBaseException().Message;
                lblTom.ForeColor = Theme.Margin;
                return;
            }
            finally { Cursor = Cursors.Default; }

            var d = _duLieu;
            int gio = DateTime.Now.Hour;
            string buoi = gio < 11 ? "buổi sáng" : gio < 13 ? "buổi trưa" : gio < 18 ? "buổi chiều" : "buổi tối";
            string ten = PhienDangNhap.DaDangNhap ? PhienDangNhap.TenDangNhap : "bạn";
            lblChao.Text = $"Chào {buoi}, {ten}";
            lblTom.ForeColor = Theme.TextMuted;
            lblTom.Text = (d.HomNay.Count == 0 ? "Hôm nay không có lớp nào" : $"Hôm nay có {d.HomNay.Count} lớp")
                        + (d.Viec.Count == 0 ? ", không còn việc tồn." : $", {d.Viec.Count} việc cần xử lý.");

            bang.Nap(d.HomNay);

            theLop.Dat(d.LopDangHoc.ToString(), d.LopChuaBatDau > 0 ? $"{d.LopChuaBatDau} lớp sắp khai giảng" : "Chưa có lớp chờ khai giảng");
            theHocVien.Dat(d.HocVienDangHoc.ToString(), $"{d.GiaSuDangDay}/{d.TongGiaSu} gia sư đang dạy");
            string soVoiThangTruoc = d.ThuThangTruoc == 0 ? "Tháng trước chưa có khoản thu"
                : d.ThuThangNay >= d.ThuThangTruoc ? $"Tháng trước {Tien.Gon(d.ThuThangTruoc)}, đang cao hơn"
                : $"Tháng trước {Tien.Gon(d.ThuThangTruoc)}";
            theThu.Dat(Tien.Gon(d.ThuThangNay), soVoiThangTruoc, d.ThuThangTruoc > 0 && d.ThuThangNay >= d.ThuThangTruoc ? Theme.Success : (Color?)null);
            theNo.Dat(Tien.Gon(d.ConNo), d.SoLopConNo > 0 ? $"{d.SoLopConNo} lớp còn nợ học phí" : "Không lớp nào còn nợ",
                d.SoLopConNo > 0 ? Theme.Warning : Theme.Success);

            bieuDo.Nap(d.ThuTheoThang);
            NapDanhSach(lstViec, lblViecTrong, d.Viec.Cast<object>());
            NapDanhSach(lstSapHet, lblHetTrong, d.SapKetThuc.Cast<object>());
        }

        private static void NapDanhSach(ListBox lst, Label trong, IEnumerable<object> ds)
        {
            lst.BeginUpdate();
            lst.Items.Clear();
            lst.Items.AddRange(ds.ToArray());
            lst.EndUpdate();
            trong.Visible = lst.Items.Count == 0;
            lst.Visible = lst.Items.Count > 0;
        }

        /// <summary>Bấm vào một việc: mở đúng trang, lọc / chọn sẵn thứ cần làm</summary>
        private static void MoViec(ViecCanLam v)
        {
            switch (v.Loai)
            {
                case LoaiViec.ChuaDiemDanh:
                    DieuHuong.Mo<frmHocVien>()?.MoDiemDanh(v.Ngay ?? DateTime.Today.AddDays(-1), null);
                    break;
                case LoaiViec.VangChuaBu:
                    var trang = DieuHuong.Mo<frmHocVien>();
                    if (trang == null) break;
                    if (v.MaHV.HasValue) trang.ChonHocVien(v.MaHV.Value, v.MaLop);
                    else trang.LocTinhTrang("Đang học");
                    break;
                case LoaiViec.ConNo:
                    DieuHuong.Mo<frmHocPhi>();
                    break;
                case LoaiViec.SapKetThuc:
                    DieuHuong.Mo<frmLopHoc>()?.LocTrangThai(Constants.LopHocStatus.DangDienRa);
                    break;
                case LoaiViec.SapBatDau:
                    DieuHuong.Mo<frmLopHoc>()?.LocTrangThai(Constants.LopHocStatus.ChuaBatDau);
                    break;
            }
        }

        // ===================================================================
        // VẼ DÒNG
        // ===================================================================

        private const TextFormatFlags MotDong = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                                                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

        private static void NenDong(DrawItemEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var b = new SolidBrush(Theme.Surface)) e.Graphics.FillRectangle(b, e.Bounds);
            using (var p = new Pen(Theme.Rule)) e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void VeViec(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var v = (ViecCanLam)lstViec.Items[e.Index];
            var g = e.Graphics;
            NenDong(e);

            Color mau; Glyph glyph;
            switch (v.Loai)
            {
                case LoaiViec.ChuaDiemDanh: mau = Theme.Warning; glyph = Glyph.Clipboard; break;
                case LoaiViec.VangChuaBu: mau = Theme.Margin; glyph = Glyph.User; break;
                case LoaiViec.ConNo: mau = Theme.Warning; glyph = Glyph.Money; break;
                case LoaiViec.SapKetThuc: mau = Theme.Info; glyph = Glyph.Clock; break;
                default: mau = Theme.Success; glyph = Glyph.Calendar; break;
            }
            var o = new RectangleF(e.Bounds.Left + 2, e.Bounds.Top + (e.Bounds.Height - 36) / 2f, 36, 36);
            using (var b = new SolidBrush(Theme.Blend(mau, Color.White, 0.87f))) g.FillEllipse(b, o);
            Icons.Draw(g, glyph, new RectangleF(o.X + 9, o.Y + 9, 18, 18), mau);

            int x = e.Bounds.Left + 50, rong = e.Bounds.Width - 50 - 22;
            TextRenderer.DrawText(g, v.TieuDe, Theme.BodyStrong, new Rectangle(x, e.Bounds.Top + 7, rong, 22), Theme.Text, MotDong);
            TextRenderer.DrawText(g, v.ChiTiet, Theme.Small, new Rectangle(x, e.Bounds.Top + 28, rong, 20), Theme.TextMuted, MotDong);
            Icons.Draw(g, Glyph.ChevronRight, new RectangleF(e.Bounds.Right - 18, e.Bounds.Top + (e.Bounds.Height - 14) / 2f, 14, 14), Theme.TextFaint);
        }

        private void VeSapHet(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var l = (LopSapHet)lstSapHet.Items[e.Index];
            var g = e.Graphics;
            NenDong(e);

            string con = l.ConNgay == 0 ? "Hôm nay" : $"Còn {l.ConNgay} ngày";
            bool gap = l.ConNgay <= 2;
            int rongNhan = Ui.DrawTag(g, con, new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 2, e.Bounds.Height),
                gap ? Theme.Warning : Theme.TextMuted, gap ? Theme.WarningSoft : Theme.NeutralSoft);
            int rong = e.Bounds.Width - rongNhan - 8;
            TextRenderer.DrawText(g, l.TenLop, Theme.BodyStrong, new Rectangle(e.Bounds.Left + 2, e.Bounds.Top + 6, rong, 22), Theme.Text, MotDong);
            TextRenderer.DrawText(g, $"Đến {l.KetThuc:dd/MM}" + (string.IsNullOrEmpty(l.GiaSu) ? "" : ", gia sư " + l.GiaSu), Theme.Small,
                new Rectangle(e.Bounds.Left + 2, e.Bounds.Top + 27, rong, 20), Theme.TextMuted, MotDong);
        }
    }
}
