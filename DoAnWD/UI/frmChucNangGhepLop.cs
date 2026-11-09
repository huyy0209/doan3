using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;

namespace DoAnWD
{
    /// <summary>
    /// Ghép lớp mới theo 3 bước:
    ///   1. Tìm và chọn học viên (gõ tên, SĐT, tên phụ huynh, trường... không cần dấu)
    ///   2. Chọn ca + khoảng ngày (ca học viên đã bận bị đánh dấu)
    ///   3. Chọn gia sư: lọc theo môn, mặc định chỉ hiện người còn trống ca đã chọn
    /// Kiểm tra trùng lịch được tính ngay trong bộ nhớ; khi lưu, LopHocService kiểm tra lại lần cuối.
    /// </summary>
    public partial class frmChucNangGhepLop : Form, ITuGiaoDien
    {
        private readonly LopHocService _lopHocService = new LopHocService();

        private List<HocVien> _hocViens = new List<HocVien>();
        private List<GiaSu> _giaSus = new List<GiaSu>();
        private List<LopHoc> _lopHoatDong = new List<LopHoc>();

        // Bước 1
        private TextBox txtTimHV;
        private ListBox lstHV;
        private Label lblHVInfo;
        // Bước 2
        private ListBox lstCa;
        private DateTimePicker dtpBatDau, dtpKetThuc;
        private TextBox txtHocPhi;
        private Label lblCanhBao;
        // Bước 3
        private TextBox txtTimGS;
        private ComboBox cboMon;
        private CheckBox chkChiTrong;
        private ListBox lstGS;
        private Label lblLichGS;
        // Chân
        private Label lblTomTat;
        private ThemedButton btnGhepLop, btnHuy;

        public frmChucNangGhepLop()
        {
            InitializeComponent();

            // Màn hình nhỏ (laptop 1366x768): thu hộp thoại vừa 92% vùng làm việc
            var vung = Screen.PrimaryScreen.WorkingArea;
            Size = new Size(Math.Min(Width, (int)(vung.Width * 0.92)), Math.Min(Height, (int)(vung.Height * 0.92)));
            MinimumSize = new Size(Math.Min(MinimumSize.Width, Width), Math.Min(MinimumSize.Height, Height));

            BuildUi();
            Load += (s, e) => NapDuLieu();
            FormClosed += (s, e) => _lopHocService.Dispose();
        }

        // ===================================================================
        // DỰNG GIAO DIỆN
        // ===================================================================

        private void BuildUi()
        {
            BackColor = Theme.Paper;
            Font = Theme.Body;
            Padding = new Padding(24, 18, 24, 18);

            // ---- Bước 1: học viên ----
            txtTimHV = Ui.Input("Tìm tên, SĐT, phụ huynh, trường...");
            txtTimHV.TextChanged += (s, e) => LocHocVien();
            lstHV = Ui.List(52, VeHocVien);
            lstHV.SelectedIndexChanged += (s, e) => { CapNhatCa(); LocGiaSu(); CapNhat(); };
            lblHVInfo = Ui.Text("", true);
            lblHVInfo.MaximumSize = new Size(330, 0);
            var sec1 = Ui.Section("1. Học viên");
            sec1.Controls.Add(Ui.Stack(false, txtTimHV, KhungList(lstHV), lblHVInfo));
            ((TableLayoutPanel)sec1.Controls[0]).RowStyles[1] = new RowStyle(SizeType.Percent, 100);

            // ---- Bước 2: ca và thời gian ----
            lstCa = Ui.List(40, VeCa);
            lstCa.Items.AddRange(Constants.CaHoc.TatCaCaHoc);
            lstCa.Height = 40 * 6 + 2;
            lstCa.Dock = DockStyle.Top;
            lstCa.SelectedIndexChanged += (s, e) => { LocGiaSu(); CapNhat(); };

            dtpBatDau = Ui.Date(140);
            dtpKetThuc = Ui.Date(140);
            dtpBatDau.MinDate = DateTime.Now.Date;
            dtpBatDau.Value = DateTime.Now.Date.AddDays(1);
            dtpKetThuc.Value = dtpBatDau.Value.AddMonths(1);
            dtpBatDau.ValueChanged += (s, e) => { if (dtpKetThuc.Value <= dtpBatDau.Value) dtpKetThuc.Value = dtpBatDau.Value.AddMonths(1); CapNhatCa(); LocGiaSu(); CapNhat(); };
            dtpKetThuc.ValueChanged += (s, e) => { CapNhatCa(); LocGiaSu(); CapNhat(); };

            var nhanh = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0), WrapContents = false };
            foreach (int thang in new[] { 1, 2, 3 })
            {
                var b = Ui.Button($"{thang} tháng", ButtonKind.Ghost, Glyph.None, 82);
                b.Height = 32;
                int t = thang;
                b.Click += (s, e) => dtpKetThuc.Value = dtpBatDau.Value.AddMonths(t);
                nhanh.Controls.Add(b);
            }

            txtHocPhi = Ui.Input("Ví dụ: 1.500.000", 200);
            txtHocPhi.TextChanged += (s, e) => CapNhat();
            txtHocPhi.Leave += (s, e) =>
            {
                decimal hp;
                if (TryParseHocPhi(txtHocPhi.Text, out hp))
                    txtHocPhi.Text = hp.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
            };

            lblCanhBao = Ui.Text("", true);
            lblCanhBao.MaximumSize = new Size(300, 0);

            var sec2 = Ui.Section("2. Ca và thời gian");
            sec2.Controls.Add(Ui.Stack(false,
                Ui.FieldLabel("Ca học"), lstCa,
                Ui.FieldLabel("Từ ngày – đến ngày"), Ui.Row(dtpBatDau, dtpKetThuc), nhanh,
                Ui.FieldLabel("Học phí cả khóa (đ)"), txtHocPhi,
                lblCanhBao));

            // ---- Bước 3: gia sư ----
            txtTimGS = Ui.Input("Tìm tên, SĐT gia sư...");
            txtTimGS.TextChanged += (s, e) => LocGiaSu();
            cboMon = Ui.Combo();
            cboMon.SelectedIndexChanged += (s, e) => LocGiaSu();
            chkChiTrong = new CheckBox
            {
                Text = "Chỉ hiện gia sư nhận và còn trống ca này", Checked = true, AutoSize = true,
                Font = Theme.Body, ForeColor = Theme.Text, Margin = new Padding(0, 4, 0, 6)
            };
            chkChiTrong.CheckedChanged += (s, e) => LocGiaSu();
            lstGS = Ui.List(52, VeGiaSu);
            lstGS.SelectedIndexChanged += (s, e) => { HienLichGiaSu(); CapNhat(); };
            lblLichGS = Ui.Text("", true);
            lblLichGS.MaximumSize = new Size(380, 0);

            var sec3 = Ui.Section("3. Gia sư");
            var stack3 = Ui.Stack(false, txtTimGS, cboMon, chkChiTrong, KhungList(lstGS), lblLichGS);
            stack3.RowStyles[3] = new RowStyle(SizeType.Percent, 100);
            sec3.Controls.Add(stack3);

            // ---- Ba cột ----
            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(0, 12, 0, 12) };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            cot.Controls.Add(sec1, 0, 0);
            cot.Controls.Add(sec2, 1, 0);
            cot.Controls.Add(sec3, 2, 0);
            sec3.Margin = new Padding(0);

            // ---- Tiêu đề + chân ----
            var header = Ui.Stack(false, Ui.Title("Ghép lớp mới"),
                Ui.Text("Chọn học viên, ca học rồi gia sư còn trống. Môn học lấy theo chuyên môn của gia sư.", true));

            lblTomTat = new Label { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = Theme.BodyStrong, ForeColor = Theme.Text };
            btnHuy = Ui.Button("Hủy", ButtonKind.Secondary, Glyph.Close, 110);
            btnHuy.Click += (s, e) => Close();
            btnGhepLop = Ui.Button("Ghép lớp", ButtonKind.Primary, Glyph.Check, 150);
            btnGhepLop.Margin = new Padding(0, 4, 0, 4);
            btnGhepLop.Click += BtnGhepLop_Click;
            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoSize = true };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.Controls.Add(lblTomTat, 0, 0);
            footer.Controls.Add(btnHuy, 1, 0);
            footer.Controls.Add(btnGhepLop, 2, 0);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(header, 0, 0);
            root.Controls.Add(cot, 0, 1);
            root.Controls.Add(footer, 0, 2);
            Controls.Add(root);

            AcceptButton = btnGhepLop;
            CancelButton = btnHuy;
            ThemeManager.EnableDoubleBuffer(this);
        }

        /// <summary>Bọc ListBox trong khung viền mảnh</summary>
        private static Control KhungList(ListBox l)
        {
            var p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = Theme.Rule, Margin = new Padding(0, 4, 0, 4), MinimumSize = new Size(0, 160) };
            p.Controls.Add(l);
            return p;
        }

        // ===================================================================
        // DỮ LIỆU
        // ===================================================================

        private void NapDuLieu()
        {
            try
            {
                using (var hv = new HocVienService()) _hocViens = hv.GetAll().OrderBy(h => h.HoTen).ToList();
                using (var gs = new GiaSuService()) _giaSus = gs.GetForGhepLop().ToList();
                _lopHoatDong = _lopHocService.GetLopConHoatDong();

                cboMon.Items.Clear();
                cboMon.Items.Add("Tất cả môn");
                cboMon.Items.AddRange(_giaSus.Select(g => g.ChuyenMon).Where(m => !string.IsNullOrWhiteSpace(m))
                    .Distinct().OrderBy(m => m).ToArray());
                cboMon.SelectedIndex = 0;

                LocHocVien();
                LocGiaSu();
                CapNhat();
                txtTimHV.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tải được dữ liệu: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private HocVien HocVienChon => lstHV.SelectedItem as HocVien;
        private GiaSu GiaSuChon => lstGS.SelectedItem as GiaSu;
        private string CaChon => lstCa.SelectedItem as string;

        private void LayKhoang(string ca, out DateTime bd, out DateTime kt)
        {
            int gioBD, gioKT;
            if (Constants.CaHoc.TryGetGio(ca, out gioBD, out gioKT))
            {
                bd = dtpBatDau.Value.Date.AddHours(gioBD);
                kt = dtpKetThuc.Value.Date.AddHours(gioKT);
            }
            else
            {
                bd = dtpBatDau.Value.Date;
                kt = dtpKetThuc.Value.Date.AddDays(1).AddTicks(-1);
            }
        }

        /// <summary>Lớp đang hoạt động trùng ca + trùng ngày với khoảng đang chọn</summary>
        private LopHoc LopTrung(Func<LopHoc, bool> cuaAi, string ca)
        {
            if (ca == null) return null;
            DateTime bd, kt;
            LayKhoang(ca, out bd, out kt);
            var moi = new LopHoc { CaHoc = ca, ThoiGianBatDau = bd, ThoiGianKetThuc = kt };
            return _lopHoatDong.FirstOrDefault(l => cuaAi(l) && l.TrungLich(moi));
        }

        private void LocHocVien()
        {
            var chon = HocVienChon;
            string tk = txtTimHV.Text;
            var ds = _hocViens.Where(h => TimKiem.Khop(tk, h.HoTen, h.SDT, h.TenPhuHuynh, h.SDTPhuHuynh, h.TruongLop, h.TrinhDo)).ToList();

            lstHV.BeginUpdate();
            lstHV.Items.Clear();
            lstHV.Items.AddRange(ds.Cast<object>().ToArray());
            if (chon != null && ds.Contains(chon)) lstHV.SelectedItem = chon;
            else if (ds.Count == 1) lstHV.SelectedIndex = 0;
            lstHV.EndUpdate();
            if (lstHV.SelectedIndex < 0) { CapNhatCa(); CapNhat(); }
        }

        private void LocGiaSu()
        {
            var chon = GiaSuChon;
            string tk = txtTimGS.Text;
            string mon = cboMon.SelectedIndex > 0 ? cboMon.SelectedItem as string : null;
            string ca = CaChon;

            var ds = _giaSus
                .Where(g => mon == null || g.ChuyenMon == mon)
                .Where(g => TimKiem.Khop(tk, g.HoTen, g.SDT, g.ChuyenMon))
                .Where(g => !chkChiTrong.Checked || ca == null || (g.NhanCa(ca) && LopTrung(l => l.MaGS == g.MaGS, ca) == null))
                .OrderBy(g => ca != null && (!g.NhanCa(ca) || LopTrung(l => l.MaGS == g.MaGS, ca) != null)) // người trống lên trước
                .ThenBy(g => _lopHoatDong.Count(l => l.MaGS == g.MaGS))               // ít lớp lên trước
                .ThenBy(g => g.HoTen)
                .ToList();

            lstGS.BeginUpdate();
            lstGS.Items.Clear();
            lstGS.Items.AddRange(ds.Cast<object>().ToArray());
            if (chon != null && ds.Contains(chon)) lstGS.SelectedItem = chon;
            lstGS.EndUpdate();
            HienLichGiaSu();
            CapNhat();
        }

        /// <summary>Thông tin học viên + vẽ lại danh sách ca (ca học viên đã bận)</summary>
        private void CapNhatCa()
        {
            var hv = HocVienChon;
            if (hv == null)
                lblHVInfo.Text = _hocViens.Count == 0 ? "Chưa có học viên. Thêm học viên ở trang Học viên trước." : "Chọn một học viên trong danh sách.";
            else
            {
                var lop = _lopHoatDong.Where(l => l.MaHV == hv.MaHV).ToList();
                lblHVInfo.Text = lop.Count == 0
                    ? "Chưa có lớp nào đang học."
                    : "Đang học: " + string.Join("; ", lop.Select(l => $"{l.MonHoc} ({Theme.ShiftShortName(l.CaHoc)})"));
            }
            lstCa.Invalidate();
        }

        private void HienLichGiaSu()
        {
            var gs = GiaSuChon;
            if (gs == null) { lblLichGS.Text = ""; return; }
            var lop = _lopHoatDong.Where(l => l.MaGS == gs.MaGS)
                .OrderBy(l => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, l.CaHoc)).ToList();
            lblLichGS.Text = lop.Count == 0
                ? $"{gs.HoTen} chưa dạy lớp nào."
                : $"Lịch dạy: " + string.Join("; ", lop.Select(l => $"{Theme.ShiftShortName(l.CaHoc)} {l.MonHoc} đến {l.ThoiGianKetThuc:dd/MM}"));
        }

        /// <summary>Tóm tắt + cảnh báo + bật/tắt nút Ghép lớp</summary>
        private void CapNhat()
        {
            var hv = HocVienChon;
            var gs = GiaSuChon;
            string ca = CaChon;
            decimal hp;
            bool coHocPhi = TryParseHocPhi(txtHocPhi.Text, out hp);

            string loi = null;
            if (ca != null && hv != null)
            {
                var trung = LopTrung(l => l.MaHV == hv.MaHV, ca);
                if (trung != null) loi = $"{hv.HoTen} đã học {trung.MonHoc} ca này đến {trung.ThoiGianKetThuc:dd/MM/yyyy}.";
            }
            if (loi == null && ca != null && gs != null)
            {
                var trung = LopTrung(l => l.MaGS == gs.MaGS, ca);
                if (trung != null) loi = $"{gs.HoTen} đã dạy {trung.HocVien?.HoTen} ca này đến {trung.ThoiGianKetThuc:dd/MM/yyyy}.";
            }
            bool khongNhanCa = loi == null && ca != null && gs != null && !gs.NhanCa(ca);
            if (loi == null && dtpKetThuc.Value.Date <= dtpBatDau.Value.Date)
                loi = "Ngày kết thúc phải sau ngày bắt đầu.";
            if (loi == null && !string.IsNullOrWhiteSpace(txtHocPhi.Text) && !coHocPhi)
                loi = "Học phí phải là số, ví dụ 1.500.000.";

            if (loi != null)
            {
                lblCanhBao.Text = loi;
                lblCanhBao.ForeColor = Theme.Margin;
            }
            else if (khongNhanCa)
            {
                lblCanhBao.Text = $"Lưu ý: {gs.HoTen} chỉ nhận ca {gs.CaNhanDay.Replace(",", ", ")}. Vẫn ghép được nếu đã thống nhất.";
                lblCanhBao.ForeColor = Theme.Warning;
            }
            else if (ca != null)
            {
                DateTime bd, kt;
                LayKhoang(ca, out bd, out kt);
                int soNgay = (kt.Date - bd.Date).Days + 1;
                lblCanhBao.Text = $"{soNgay} buổi, {bd:HH:mm}–{kt:HH:mm} mỗi ngày.";
                lblCanhBao.ForeColor = Theme.Success;
            }
            else
            {
                lblCanhBao.Text = "Chọn một ca học.";
                lblCanhBao.ForeColor = Theme.TextMuted;
            }

            // Tóm tắt cho người dùng kiểm tra trước khi bấm
            var thieu = new List<string>();
            if (hv == null) thieu.Add("học viên");
            if (ca == null) thieu.Add("ca học");
            if (gs == null) thieu.Add("gia sư");
            if (!coHocPhi) thieu.Add("học phí");

            if (thieu.Count > 0)
            {
                lblTomTat.Text = "Còn thiếu: " + string.Join(", ", thieu) + ".";
                lblTomTat.ForeColor = Theme.TextMuted;
            }
            else
            {
                lblTomTat.Text = $"{hv.HoTen} học {gs.ChuyenMon} với {gs.HoTen}, ca {Theme.ShiftShortName(ca)}, " +
                                 $"{dtpBatDau.Value:dd/MM} – {dtpKetThuc.Value:dd/MM/yyyy}, {hp.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} đ";
                lblTomTat.ForeColor = loi == null ? Theme.Text : Theme.Margin;
            }

            btnGhepLop.Enabled = thieu.Count == 0 && loi == null;
        }

        private static bool TryParseHocPhi(string text, out decimal hocPhi)
        {
            hocPhi = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            string so = new string(text.Where(char.IsDigit).ToArray());
            return so.Length > 0 && so.Length <= 12 && decimal.TryParse(so, out hocPhi) && hocPhi > 0;
        }

        // ===================================================================
        // VẼ DANH SÁCH
        // ===================================================================

        private void VeHocVien(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var hv = (HocVien)lstHV.Items[e.Index];
            var r = Ui.DrawListItemBackground(e);
            var g = e.Graphics;

            int soLop = _lopHoatDong.Count(l => l.MaHV == hv.MaHV);
            int tagW = soLop > 0 ? Ui.DrawTag(g, $"{soLop} lớp", r, Theme.Ink, Theme.InkSoft) : 0;

            var line1 = new Rectangle(r.X, r.Y, r.Width - tagW, r.Height / 2);
            var line2 = new Rectangle(r.X, r.Y + r.Height / 2, r.Width - tagW, r.Height / 2);
            TextRenderer.DrawText(g, hv.HoTen, Theme.BodyStrong, line1, Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            var phu = new List<string>();
            if (!string.IsNullOrWhiteSpace(hv.TenPhuHuynh)) phu.Add("PH " + hv.TenPhuHuynh);
            if (!string.IsNullOrWhiteSpace(hv.SDTLienHe)) phu.Add(hv.SDTLienHe);
            if (!string.IsNullOrWhiteSpace(hv.TrinhDo)) phu.Add(hv.TrinhDo);
            TextRenderer.DrawText(g, string.Join(", ", phu), Theme.Small, line2, Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void VeCa(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            string ca = (string)lstCa.Items[e.Index];
            var hv = HocVienChon;
            var trung = hv != null ? LopTrung(l => l.MaHV == hv.MaHV, ca) : null;

            var r = Ui.DrawListItemBackground(e, trung != null);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var b = new SolidBrush(trung != null ? Theme.Rule : Theme.ShiftColor(ca)))
                g.FillEllipse(b, r.X, r.Y + r.Height / 2f - 5, 10, 10);

            int tagW = trung != null ? Ui.DrawTag(g, "HV đã có lớp", r, Theme.Margin, Theme.MarginSoft) : 0;
            TextRenderer.DrawText(g, ca, Theme.Body, new Rectangle(r.X + 18, r.Y, r.Width - 18 - tagW, r.Height),
                trung != null ? Theme.TextFaint : Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void VeGiaSu(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var gs = (GiaSu)lstGS.Items[e.Index];
            string ca = CaChon;
            bool trung = ca != null && LopTrung(l => l.MaGS == gs.MaGS, ca) != null;

            var r = Ui.DrawListItemBackground(e, trung);
            var g = e.Graphics;
            int tagW = 0;
            if (ca != null)
                tagW = trung ? Ui.DrawTag(g, "Trùng ca", r, Theme.Margin, Theme.MarginSoft)
                     : !gs.NhanCa(ca) ? Ui.DrawTag(g, "Không nhận ca này", r, Theme.Neutral, Theme.NeutralSoft)
                     : Ui.DrawTag(g, "Trống ca", r, Theme.Success, Theme.SuccessSoft);

            int soLop = _lopHoatDong.Count(l => l.MaGS == gs.MaGS);
            var line1 = new Rectangle(r.X, r.Y, r.Width - tagW, r.Height / 2);
            var line2 = new Rectangle(r.X, r.Y + r.Height / 2, r.Width - tagW, r.Height / 2);
            TextRenderer.DrawText(g, gs.HoTen, Theme.BodyStrong, line1, trung ? Theme.TextMuted : Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, $"{gs.ChuyenMon}, {(soLop == 0 ? "chưa có lớp" : soLop + " lớp đang dạy")}",
                Theme.Small, line2, Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        // ===================================================================
        // GHÉP LỚP
        // ===================================================================

        private void BtnGhepLop_Click(object sender, EventArgs e)
        {
            var hv = HocVienChon;
            var gs = GiaSuChon;
            string ca = CaChon;
            decimal hp;
            if (hv == null || gs == null || ca == null || !TryParseHocPhi(txtHocPhi.Text, out hp)) return;

            try
            {
                DateTime bd, kt;
                LayKhoang(ca, out bd, out kt);
                var lop = _lopHocService.GhepLop(hv.MaHV, gs.MaGS, gs.ChuyenMon.Trim(), hp, null, bd, kt, ca);

                MessageBox.Show(
                    $"Đã ghép lớp {lop.MonHoc}.\n\nHọc viên: {hv.HoTen}\nGia sư: {gs.HoTen}\nCa: {ca}\n" +
                    $"Từ {bd:dd/MM/yyyy} đến {kt:dd/MM/yyyy}\nHọc phí: {hp:N0} đ\nTrạng thái: {lop.TrangThai}",
                    Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Constants.Messages.LoiKhiGhepLop}{ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
