using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Dto;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;
using DoAnWD.UI.Controls;
using DoAnWD.UI.Pages;

namespace DoAnWD.UI.Dialogs
{
    /// <summary>
    /// Ghép lớp mới theo 3 bước:
    ///   1. Tìm và chọn học viên (gõ tên, SĐT, tên phụ huynh, trường... không cần dấu)
    ///   2. Lớp học: môn (danh mục), cấp độ đầu vào (môn năng khiếu), ca + thứ học + khoảng ngày, học phí
    ///   3. Chọn gia sư: mặc định chỉ hiện người dạy môn đã chọn và còn trống ca
    /// Tên lớp tự gợi ý "Môn - Học viên - Ca", sửa được (sửa rồi thì không tự đổi nữa).
    /// Kiểm tra trùng lịch được tính ngay trong bộ nhớ; khi lưu, LopHocService kiểm tra lại lần cuối.
    /// </summary>
    public partial class frmChucNangGhepLop : Form, ITuGiaoDien
    {
        private readonly LopHocService _lopHocService = new LopHocService();

        private List<HocVien> _hocViens = new List<HocVien>();
        private List<GiaSu> _giaSus = new List<GiaSu>();
        private List<LopHoc> _lopHoatDong = new List<LopHoc>();
        private List<MonHocDanhMuc> _mons = new List<MonHocDanhMuc>();
        private HashSet<string> _tenLopDaCo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _tenLopTuGo;      // người dùng đã tự sửa tên lớp => thôi tự gợi ý
        private bool _dangGoiYTen;

        // Bước 1
        private TextBox txtTimHV;
        private ListBox lstHV;
        private Label lblHVInfo;
        // Bước 2
        private ComboBox cboMonLop, cboCapDo;
        private Control khungCapDo;
        private ChonCa chonCa;
        private ChonThu chonThu;
        private DateTimePicker dtpBatDau, dtpKetThuc;
        private TextBox txtHocPhi;
        private Label lblCanhBao;
        // Bước 3
        private TextBox txtTimGS;
        private CheckBox chkCungMon, chkChiTrong;
        private ListBox lstGS;
        private Label lblLichGS, lblHVTrong, lblGSTrong;
        // Chân
        private TextBox txtTenLop;
        private Label lblTomTat;
        private ThemedButton btnGhepLop, btnHuy;

        /// <summary>Mã lớp vừa ghép (để trang gọi chọn luôn lớp đó)</summary>
        public int? MaLopVuaGhep { get; private set; }

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
            lstHV.SelectedIndexChanged += (s, e) => { CapNhatCa(); NapCapDo(); LocGiaSu(); CapNhat(); };
            lblHVInfo = Ui.Text("", true);
            lblHVInfo.MaximumSize = new Size(330, 0);
            var sec1 = Ui.Section("1. Học viên");
            lblHVTrong = NhanTrong();
            sec1.Controls.Add(Ui.Stack(false, txtTimHV, KhungList(lstHV, lblHVTrong), lblHVInfo));
            ((TableLayoutPanel)sec1.Controls[0]).RowStyles[1] = new RowStyle(SizeType.Percent, 100);

            // ---- Bước 2: lớp học ----
            cboMonLop = Ui.Combo(false);
            ((ThemedComboBox)cboMonLop).GoiY = "Chọn môn học...";
            cboMonLop.Format += (s, e) =>
            {
                var m = e.ListItem as MonHocDanhMuc;
                if (m != null) e.Value = m.LaNangKhieu ? $"{m.TenMon} (năng khiếu)" : m.TenMon;
            };
            cboMonLop.FormattingEnabled = true;
            cboMonLop.SelectedIndexChanged += (s, e) => { NapCapDo(); LocGiaSu(); CapNhat(); };
            cboCapDo = Ui.Combo(false);
            cboCapDo.SelectedIndexChanged += (s, e) => CapNhat();
            var oMon = Ui.Stack(false, Ui.FieldLabel("Môn học"), cboMonLop);
            khungCapDo = Ui.Stack(false, Ui.FieldLabel("Cấp độ đầu vào"), cboCapDo);
            var hangMon = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Margin = new Padding(0) };
            hangMon.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            hangMon.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            foreach (var o in new[] { oMon, khungCapDo }) { o.AutoSize = true; o.Dock = DockStyle.Top; }
            oMon.Margin = new Padding(0, 0, 8, 0);
            cboMonLop.Dock = cboCapDo.Dock = DockStyle.Top;
            hangMon.Controls.Add(oMon, 0, 0);
            hangMon.Controls.Add(khungCapDo, 1, 0);

            chonCa = new ChonCa
            {
                // Ca học viên đã có lớp trùng lịch (xét cả thứ học và khoảng ngày đang chọn)
                Ban = ca => HocVienChon != null && LopTrung(l => l.MaHV == HocVienChon.MaHV, ca) != null
            };
            chonCa.ThayDoi += (s, e) => { LocGiaSu(); GoiYTenLop(); CapNhat(); };
            chonThu = new ChonThu();
            chonThu.ThayDoi += (s, e) => { CapNhatCa(); LocGiaSu(); CapNhat(); };

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
                if (SoTien.TryParse(txtHocPhi.Text, out hp))
                    txtHocPhi.Text = hp.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
            };

            lblCanhBao = Ui.Text("", true);
            lblCanhBao.MaximumSize = new Size(300, 0);

            var sec2 = Ui.Section("2. Lớp học");
            sec2.Controls.Add(CuonDoc(Ui.Stack(false,
                hangMon,
                Ui.FieldLabel("Ca học"), chonCa,
                Ui.FieldLabel("Học vào các thứ"), chonThu,
                Ui.FieldLabel("Từ ngày – đến ngày"), Ui.Row(dtpBatDau, dtpKetThuc), nhanh,
                Ui.FieldLabel("Học phí cả khóa (đ)"), txtHocPhi,
                lblCanhBao)));

            // ---- Bước 3: gia sư ----
            txtTimGS = Ui.Input("Tìm tên, SĐT gia sư...");
            txtTimGS.TextChanged += (s, e) => LocGiaSu();
            chkCungMon = new CheckBox
            {
                Text = "Chỉ hiện gia sư dạy môn này", Checked = true, AutoSize = true,
                Font = Theme.Body, ForeColor = Theme.Text, Margin = new Padding(0, 4, 0, 0)
            };
            chkCungMon.CheckedChanged += (s, e) => LocGiaSu();
            chkChiTrong = new CheckBox
            {
                Text = "Chỉ hiện gia sư nhận và còn trống ca này", Checked = true, AutoSize = true,
                Font = Theme.Body, ForeColor = Theme.Text, Margin = new Padding(0, 4, 0, 6)
            };
            chkChiTrong.CheckedChanged += (s, e) => LocGiaSu();
            lstGS = Ui.List(52, VeGiaSu);
            lstGS.SelectedIndexChanged += (s, e) =>
            {
                if (MonChon == null && GiaSuChon != null)
                {
                    var mon = _mons.FirstOrDefault(m => string.Equals(m.TenMon, GiaSuChon.ChuyenMon?.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (mon != null) cboMonLop.SelectedItem = mon;
                }
                HienLichGiaSu(); CapNhat();
            };
            lblLichGS = Ui.Text("", true);
            lblLichGS.MaximumSize = new Size(380, 0);

            var sec3 = Ui.Section("3. Gia sư");
            lblGSTrong = NhanTrong();
            var stack3 = Ui.Stack(false, txtTimGS, chkCungMon, chkChiTrong, KhungList(lstGS, lblGSTrong), lblLichGS);
            stack3.RowStyles[3] = new RowStyle(SizeType.Percent, 100);
            sec3.Controls.Add(stack3);

            // ---- Ba cột ----
            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(0, 12, 0, 12) };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            cot.Controls.Add(sec1, 0, 0);
            cot.Controls.Add(sec2, 1, 0);
            cot.Controls.Add(sec3, 2, 0);
            sec3.Margin = new Padding(0);

            // ---- Tiêu đề + chân ----
            var header = Ui.DauTrang("Ghép lớp mới", "Chọn học viên, rồi môn, ca và thứ học, cuối cùng là gia sư còn trống lịch. Tên lớp tự gợi ý, sửa được.");

            lblTomTat = new Label { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = Theme.BodyStrong, ForeColor = Theme.Text };
            btnHuy = Ui.Button("Hủy", ButtonKind.Secondary, Glyph.Close, 110);
            btnHuy.Click += (s, e) => Close();
            btnGhepLop = Ui.Button("Ghép lớp", ButtonKind.Primary, Glyph.Check, 150);
            btnGhepLop.Margin = new Padding(0, 4, 0, 4);
            btnGhepLop.Click += BtnGhepLop_Click;
            txtTenLop = Ui.Input("Tên lớp, ví dụ: Cờ vua - Bảo Ngọc - Tối");
            txtTenLop.MaxLength = 150;
            txtTenLop.TextChanged += (s, e) => { if (!_dangGoiYTen) _tenLopTuGo = txtTenLop.Text.Trim().Length > 0; CapNhat(); };
            var btnGoiY = Ui.Button("Gợi ý lại", ButtonKind.Ghost, Glyph.Refresh, 110);
            btnGoiY.Click += (s, e) => { _tenLopTuGo = false; GoiYTenLop(); };
            var nhanTen = Ui.FieldLabel("Tên lớp");
            nhanTen.Margin = new Padding(0, 10, 8, 0);
            var hangTen = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true, Margin = new Padding(0, 0, 0, 4) };
            hangTen.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            hangTen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            hangTen.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtTenLop.Dock = DockStyle.Fill;
            txtTenLop.Margin = new Padding(0, 6, 8, 0);
            hangTen.Controls.Add(nhanTen, 0, 0);
            hangTen.Controls.Add(txtTenLop, 1, 0);
            hangTen.Controls.Add(btnGoiY, 2, 0);

            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoSize = true };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.Controls.Add(hangTen, 0, 0);
            footer.SetColumnSpan(hangTen, 3);
            footer.Controls.Add(lblTomTat, 0, 1);
            footer.Controls.Add(btnHuy, 1, 1);
            footer.Controls.Add(btnGhepLop, 2, 1);

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

        /// <summary>Cho cột cuộn dọc khi cửa sổ thấp (laptop 768p) thay vì bị cắt mất ô cuối</summary>
        private static Control CuonDoc(TableLayoutPanel noiDung)
        {
            noiDung.AutoScroll = true;
            return noiDung;
        }

        /// <summary>Bọc ListBox trong khung viền mảnh</summary>
        private static Control KhungList(ListBox l, Label khiTrong = null)
        {
            var p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = Theme.Rule, Margin = new Padding(0, 4, 0, 4), MinimumSize = new Size(0, 160) };
            p.Controls.Add(l);
            if (khiTrong != null) p.Controls.Add(khiTrong);
            return p;
        }

        /// <summary>Dòng chữ hiện thay danh sách khi không có mục nào (nói rõ vì sao và làm gì tiếp)</summary>
        private static Label NhanTrong()
        {
            return new Label
            {
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = Theme.Body, ForeColor = Theme.TextMuted,
                BackColor = Theme.Surface, Padding = new Padding(24, 0, 24, 0), Visible = false, UseMnemonic = false
            };
        }

        private static void HienTrong(ListBox l, Label nhan, string chu)
        {
            bool trong = l.Items.Count == 0;
            nhan.Text = chu;
            nhan.Visible = trong;
            l.Visible = !trong;
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
                _mons = new MonHocService().GetAll(kemNgungDay: false);
                using (var db = new GiaSuContextDB())
                    _tenLopDaCo = new HashSet<string>(db.LopHocs.Where(l => l.TenLop != null).Select(l => l.TenLop).ToList(),
                        StringComparer.OrdinalIgnoreCase);

                cboMonLop.Items.Clear();
                cboMonLop.Items.AddRange(_mons.Cast<object>().ToArray());
                // Chưa chọn môn: hiện mọi gia sư; chọn gia sư trước thì tự lấy môn theo chuyên môn
                cboMonLop.SelectedIndex = -1;
                NapCapDo();

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
        private MonHocDanhMuc MonChon => cboMonLop.SelectedItem as MonHocDanhMuc;

        /// <summary>Môn năng khiếu: hiện ô cấp độ đầu vào (mặc định theo trình độ của học viên nếu khớp)</summary>
        private void NapCapDo()
        {
            var mon = MonChon;
            var cap = mon?.DanhSachCapDo ?? new List<string>();
            khungCapDo.Visible = cap.Count > 0;
            cboCapDo.Items.Clear();
            cboCapDo.Items.AddRange(cap.Cast<object>().ToArray());
            string trinhDo = HocVienChon?.TrinhDo;
            int i = cap.FindIndex(c => string.Equals(c, trinhDo, StringComparison.OrdinalIgnoreCase));
            if (cap.Count > 0) cboCapDo.SelectedIndex = Math.Max(0, i);
            GoiYTenLop();
        }

        /// <summary>Tự điền tên lớp "Môn - Học viên - Ca" (không trùng) nếu người dùng chưa tự sửa</summary>
        private void GoiYTenLop()
        {
            if (_tenLopTuGo || txtTenLop == null) return;
            string goiY = LopHocService.GoiYTenLop(MonChon?.TenMon, HocVienChon?.HoTen, CaChon);
            string ten = goiY;
            for (int i = 2; _tenLopDaCo.Contains(ten) && i < 100; i++) ten = $"{goiY} ({i})";
            _dangGoiYTen = true;
            txtTenLop.Text = ten;
            _dangGoiYTen = false;
        }
        private GiaSu GiaSuChon => lstGS.SelectedItem as GiaSu;
        private string CaChon => chonCa.GiaTri;

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

        /// <summary>Lớp đang hoạt động trùng ca + trùng ngày + có chung thứ học với lựa chọn hiện tại</summary>
        private LopHoc LopTrung(Func<LopHoc, bool> cuaAi, string ca)
        {
            if (ca == null) return null;
            DateTime bd, kt;
            LayKhoang(ca, out bd, out kt);
            var moi = new LopHoc { CaHoc = ca, ThoiGianBatDau = bd, ThoiGianKetThuc = kt, ThuHoc = chonThu.GiaTri };
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
            HienTrong(lstHV, lblHVTrong, _hocViens.Count == 0
                ? "Chưa có học viên nào.\nThêm học viên ở trang Học viên trước."
                : "Không tìm thấy học viên khớp từ khóa.");
            if (lstHV.SelectedIndex < 0) { CapNhatCa(); CapNhat(); }
        }

        private void LocGiaSu()
        {
            var chon = GiaSuChon;
            string tk = txtTimGS.Text;
            string mon = chkCungMon.Checked ? MonChon?.TenMon : null;
            string ca = CaChon;

            var ds = _giaSus
                .Where(g => mon == null || string.Equals(g.ChuyenMon?.Trim(), mon, StringComparison.OrdinalIgnoreCase))
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
            HienTrong(lstGS, lblGSTrong, chkCungMon.Checked || chkChiTrong.Checked
                ? "Không có gia sư nào dạy môn này mà còn trống ca và thứ đã chọn.\n\nBỏ dấu ở hai ô lọc phía trên để xem mọi gia sư, hoặc đổi ca / thứ học."
                : "Không tìm thấy gia sư khớp từ khóa.");
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
                    : "Đang học: " + string.Join("; ", lop.Select(l => $"{l.MonHoc} ({Constants.CaHoc.TenNgan(l.CaHoc)})"));
            }
            chonCa.Invalidate();
        }

        private void HienLichGiaSu()
        {
            var gs = GiaSuChon;
            if (gs == null) { lblLichGS.Text = ""; return; }
            var lop = _lopHoatDong.Where(l => l.MaGS == gs.MaGS)
                .OrderBy(l => Array.IndexOf(Constants.CaHoc.TatCaCaHoc, l.CaHoc)).ToList();
            lblLichGS.Text = lop.Count == 0
                ? $"{gs.HoTen} chưa dạy lớp nào."
                : $"Lịch dạy: " + string.Join("; ", lop.Select(l => $"{Constants.CaHoc.TenNgan(l.CaHoc)} {l.MonHoc} đến {l.ThoiGianKetThuc:dd/MM}"));
        }

        /// <summary>Tóm tắt + cảnh báo + bật/tắt nút Ghép lớp</summary>
        private void CapNhat()
        {
            var hv = HocVienChon;
            var gs = GiaSuChon;
            string ca = CaChon;
            decimal hp;
            bool coHocPhi = SoTien.TryParse(txtHocPhi.Text, out hp);

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
            if (loi == null && !chonThu.HopLe)
                loi = "Chọn ít nhất một thứ học trong tuần.";
            int soBuoi = new LopHoc { ThoiGianBatDau = dtpBatDau.Value.Date, ThoiGianKetThuc = dtpKetThuc.Value.Date, ThuHoc = chonThu.GiaTri }.DemSoBuoi();
            if (loi == null && soBuoi == 0)
                loi = "Khoảng ngày này không có buổi nào rơi vào các thứ đã chọn.";
            if (loi == null && !string.IsNullOrWhiteSpace(txtHocPhi.Text) && !coHocPhi)
                loi = "Học phí phải là số, ví dụ 1.500.000.";
            string tenLop = txtTenLop.Text.Trim();
            if (loi == null && tenLop.Length > 0 && _tenLopDaCo.Contains(tenLop))
                loi = $"Đã có lớp tên \"{tenLop}\". Hãy đặt tên khác.";

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
                lblCanhBao.Text = $"{soBuoi} buổi ({chonThu.MoTa}), {bd:HH:mm}–{kt:HH:mm}.";
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
            if (MonChon == null) thieu.Add("môn học");
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
                string cap = khungCapDo.Visible && cboCapDo.SelectedItem != null ? $" ({cboCapDo.SelectedItem})" : "";
                lblTomTat.Text = $"{hv.HoTen} học {MonChon.TenMon}{cap} với {gs.HoTen}, {chonThu.MoTa} ca {Constants.CaHoc.TenNgan(ca)}, " +
                                 $"{dtpBatDau.Value:dd/MM} – {dtpKetThuc.Value:dd/MM/yyyy}, {hp.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} đ";
                lblTomTat.ForeColor = loi == null ? Theme.Text : Theme.Margin;
            }

            btnGhepLop.Enabled = thieu.Count == 0 && loi == null;
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
            if (hv == null || gs == null || ca == null || MonChon == null || !SoTien.TryParse(txtHocPhi.Text, out hp)) return;

            try
            {
                DateTime bd, kt;
                LayKhoang(ca, out bd, out kt);
                var lop = _lopHocService.GhepLop(new GhepLopYeuCau
                {
                    MaHV = hv.MaHV, MaGS = gs.MaGS, MonHoc = MonChon.TenMon, HocPhi = hp,
                    BatDau = bd, KetThuc = kt, CaHoc = ca, ThuHoc = chonThu.GiaTri,
                    TenLop = txtTenLop.Text, CapDoDauVao = khungCapDo.Visible ? cboCapDo.SelectedItem as string : null
                });
                MaLopVuaGhep = lop.MaLop;

                ThongBao.ThanhCong($"Đã ghép lớp \"{lop.TenLop}\"");

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
