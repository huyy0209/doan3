using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.BLL.Validators;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;

namespace DoAnWD
{
    /// <summary>
    /// Học viên: danh sách có tìm kiếm + lọc bên trái, hồ sơ chi tiết bên phải.
    /// Chọn một dòng để xem/sửa; "Thêm học viên" để nhập người mới.
    /// </summary>
    public partial class frmHocVien : Form, ITuGiaoDien
    {
        private const string TatCa = "Tất cả";
        private const string DangHoc = "Đang học";
        private const string ChuaCoLop = "Chưa có lớp";
        private const string DaNghi = "Đã nghỉ";

        private List<HocVien> _hocViens = new List<HocVien>();
        private List<LopHoc> _lops = new List<LopHoc>();
        private int? _maDangSua;   // null = đang thêm mới

        private TextBox txtTim;
        private ComboBox cboTinhTrang, cboTrinhDoLoc;
        private DataGridView dgv;
        private Label lblDem;
        private Pager pager;

        private Label lblTieuDeForm, lblLop;
        private TextBox txtHoTen, txtSDT, txtDiaChi, txtPhuHuynh, txtSDTPH, txtTruongLop, txtGhiChu;
        private ComboBox cboTrinhDo;
        private ThemedButton btnThemMoi, btnLuu, btnXoa, btnPhieuBao, btnXuat;

        public frmHocVien()
        {
            InitializeComponent();
            BuildUi();
            Load += (s, e) => { NapDuLieu(); BatDauThemMoi(); };
        }

        // ===================================================================
        // GIAO DIỆN
        // ===================================================================

        private void BuildUi()
        {
            BackColor = Theme.Paper;
            Font = Theme.Body;
            Padding = new Padding(20, 14, 20, 16);

            // ---- Danh sách ----
            txtTim = Ui.Input("Tìm tên, SĐT, phụ huynh, trường... (không cần dấu)");
            txtTim.TextChanged += (s, e) => ApDungLoc();
            cboTinhTrang = Ui.Combo(false, 150);
            cboTinhTrang.Items.AddRange(new object[] { TatCa, DangHoc, ChuaCoLop, DaNghi });
            cboTinhTrang.SelectedIndex = 0;
            cboTinhTrang.SelectedIndexChanged += (s, e) => ApDungLoc();
            cboTrinhDoLoc = Ui.Combo(false, 150);
            cboTrinhDoLoc.SelectedIndexChanged += (s, e) => ApDungLoc();

            var loc = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true, Margin = new Padding(0) };
            loc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            loc.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            loc.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtTim.Margin = new Padding(0, 2, 10, 4);
            cboTinhTrang.Margin = new Padding(0, 2, 10, 4);
            cboTrinhDoLoc.Margin = new Padding(0, 2, 0, 4);
            loc.Controls.Add(txtTim, 0, 0);
            loc.Controls.Add(cboTinhTrang, 1, 0);
            loc.Controls.Add(cboTrinhDoLoc, 2, 0);

            dgv = Ui.Grid();
            dgv.SelectionChanged += (s, e) => HienHoSoDangChon();
            lblDem = Ui.Text("", true);
            btnXuat = Ui.Button("Xuất CSV", ButtonKind.Ghost, Glyph.None, 110);
            btnXuat.Click += (s, e) => XuatCsv();

            pager = new Pager { PageSize = 50 };
            pager.PageChanged += (s, e) => ApDungLoc(null, false);
            var chanDs = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true };
            chanDs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            chanDs.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            chanDs.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            chanDs.Controls.Add(lblDem, 0, 0);
            chanDs.Controls.Add(pager, 1, 0);
            chanDs.Controls.Add(btnXuat, 2, 0);

            var secDs = Ui.Section("Danh sách học viên");
            var stackDs = Ui.Stack(false, loc, dgv, chanDs);
            stackDs.RowStyles[1] = new RowStyle(SizeType.Percent, 100);
            dgv.Dock = DockStyle.Fill;
            secDs.Controls.Add(stackDs);

            // ---- Hồ sơ ----
            lblTieuDeForm = new Label { Font = Theme.Section, ForeColor = Theme.Ink, AutoSize = true, Margin = new Padding(0, 0, 0, 4) };
            txtHoTen = Ui.Input("Bắt buộc");
            txtSDT = Ui.Input("Không bắt buộc nếu có SĐT phụ huynh");
            txtPhuHuynh = Ui.Input("Ví dụ: Chị Mai (mẹ)");
            txtSDTPH = Ui.Input("Ví dụ: 0909123456");
            txtTruongLop = Ui.Input("Ví dụ: TH Lê Văn Tám, lớp 4");
            txtDiaChi = Ui.Input();
            cboTrinhDo = Ui.Combo(true);
            cboTrinhDo.Items.AddRange(Constants.GoiY.TrinhDo);
            txtGhiChu = Ui.Input();
            txtGhiChu.Multiline = true;
            txtGhiChu.Height = 64;
            txtGhiChu.ScrollBars = ScrollBars.Vertical;
            lblLop = Ui.Text("", true);
            lblLop.MaximumSize = new Size(420, 0);

            btnThemMoi = Ui.Button("Thêm học viên", ButtonKind.Secondary, Glyph.Plus);
            btnThemMoi.Click += (s, e) => BatDauThemMoi();
            btnLuu = Ui.Button("Lưu", ButtonKind.Primary, Glyph.Check, 110);
            btnLuu.Click += (s, e) => Luu();
            btnXoa = Ui.Button("Xóa", ButtonKind.Danger, Glyph.Trash, 96);
            btnXoa.Click += (s, e) => Xoa();
            btnPhieuBao = Ui.Button("Phiếu báo kết quả", ButtonKind.Ghost, Glyph.Chart);
            btnPhieuBao.Click += (s, e) => TaoPhieuBao();

            var hai = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Margin = new Padding(0) };
            hai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            hai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            Action<int, int, string, Control> o = (col, row, nhan, ctl) =>
            {
                var st = Ui.Stack(false, Ui.FieldLabel(nhan), ctl);
                st.AutoSize = true;
                st.Dock = DockStyle.Top;
                st.Margin = new Padding(0, 0, col == 0 ? 10 : 0, 0);
                hai.Controls.Add(st, col, row);
            };
            o(0, 0, "SĐT học viên", txtSDT);
            o(1, 0, "Trình độ", cboTrinhDo);
            o(0, 1, "Phụ huynh", txtPhuHuynh);
            o(1, 1, "SĐT phụ huynh", txtSDTPH);

            var secHs = Ui.Section("Hồ sơ");
            secHs.Margin = new Padding(0);
            secHs.Controls.Add(Ui.Stack(false,
                lblTieuDeForm,
                Ui.FieldLabel("Họ tên *"), txtHoTen,
                hai,
                Ui.FieldLabel("Trường / lớp"), txtTruongLop,
                Ui.FieldLabel("Địa chỉ"), txtDiaChi,
                Ui.FieldLabel("Ghi chú"), txtGhiChu,
                lblLop,
                Ui.Row(btnLuu, btnXoa, btnPhieuBao)));

            // ---- Trang ----
            var tieuDe = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tieuDe.Controls.Add(Ui.Title("Học viên"), 0, 0);
            tieuDe.Controls.Add(btnThemMoi, 1, 0);

            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = new Padding(0, 10, 0, 0) };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            cot.Controls.Add(secDs, 0, 0);
            cot.Controls.Add(secHs, 1, 0);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(tieuDe, 0, 0);
            root.Controls.Add(cot, 0, 1);
            Controls.Add(root);
        }

        // ===================================================================
        // DỮ LIỆU
        // ===================================================================

        private class Dong
        {
            public int MaHV { get; set; }
            public string HoTen { get; set; }
            public string SDT { get; set; }
            public string PhuHuynh { get; set; }
            public string TruongLop { get; set; }
            public string TrinhDo { get; set; }
            public string DangHocMon { get; set; }
        }

        private void NapDuLieu(int? chonMaHV = null)
        {
            try
            {
                using (var hvs = new HocVienService()) _hocViens = hvs.GetAll().ToList();
                // GetAll đã kèm gia sư + học viên trong một truy vấn
                using (var ls = new LopHocService()) _lops = ls.GetAll().ToList();

                string tdChon = cboTrinhDoLoc.SelectedIndex > 0 ? cboTrinhDoLoc.SelectedItem as string : null;
                cboTrinhDoLoc.Items.Clear();
                cboTrinhDoLoc.Items.Add("Mọi trình độ");
                cboTrinhDoLoc.Items.AddRange(_hocViens.Select(h => h.TrinhDo).Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct().OrderBy(t => t).ToArray());
                cboTrinhDoLoc.SelectedItem = tdChon;
                if (cboTrinhDoLoc.SelectedIndex < 0) cboTrinhDoLoc.SelectedIndex = 0;

                ApDungLoc(chonMaHV);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tải được danh sách học viên: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private IEnumerable<LopHoc> LopDangHoc(int maHV) => _lops.Where(l => l.MaHV == maHV && l.DangHoatDong);

        private void ApDungLoc(int? chonMaHV = null, bool veTrangDau = true)
        {
            if (cboTrinhDoLoc.Items.Count == 0) return;
            if (veTrangDau) pager.Reset();
            string tt = cboTinhTrang.SelectedItem as string;
            string td = cboTrinhDoLoc.SelectedIndex > 0 ? cboTrinhDoLoc.SelectedItem as string : null;

            var ds = _hocViens
                .Where(h => TimKiem.Khop(txtTim.Text, h.HoTen, h.SDT, h.TenPhuHuynh, h.SDTPhuHuynh, h.TruongLop, h.DiaChi, h.TrinhDo))
                .Where(h => td == null || h.TrinhDo == td)
                .Where(h =>
                {
                    bool dangHoc = LopDangHoc(h.MaHV).Any();
                    bool coLichSu = _lops.Any(l => l.MaHV == h.MaHV);
                    switch (tt)
                    {
                        case DangHoc: return dangHoc;
                        case ChuaCoLop: return !coLichSu;
                        case DaNghi: return coLichSu && !dangHoc;
                        default: return true;
                    }
                })
                .OrderBy(h => h.HoTen)
                .Select(h => new Dong
                {
                    MaHV = h.MaHV,
                    HoTen = h.HoTen,
                    SDT = h.SDTLienHe,
                    PhuHuynh = h.TenPhuHuynh,
                    TruongLop = h.TruongLop,
                    TrinhDo = h.TrinhDo,
                    DangHocMon = string.Join(", ", LopDangHoc(h.MaHV).Select(l => l.MonHoc).Distinct())
                })
                .ToList();

            int? giu = chonMaHV ?? _maDangSua;
            int tong = ds.Count;
            dgv.DataSource = pager.Cat(ds);   // chỉ đưa 1 trang vào lưới
            DatCot();
            lblDem.Text = $"{tong} / {_hocViens.Count} học viên";

            if (giu.HasValue)
            {
                foreach (DataGridViewRow r in dgv.Rows)
                    if ((int)r.Cells["MaHV"].Value == giu.Value) { dgv.CurrentCell = r.Cells["HoTen"]; break; }
            }
        }

        private void DatCot()
        {
            if (dgv.Columns.Count == 0) return;
            dgv.Columns["MaHV"].Visible = false;
            dgv.Columns["HoTen"].HeaderText = "Họ tên";
            dgv.Columns["SDT"].HeaderText = "SĐT liên hệ";
            dgv.Columns["PhuHuynh"].HeaderText = "Phụ huynh";
            dgv.Columns["TruongLop"].HeaderText = "Trường / lớp";
            dgv.Columns["TrinhDo"].HeaderText = "Trình độ";
            dgv.Columns["DangHocMon"].HeaderText = "Đang học";
        }

        private void HienHoSoDangChon()
        {
            // Không có dòng được chọn (vừa bấm "Thêm học viên") => giữ form trống
            if (dgv.CurrentRow == null || dgv.SelectedRows.Count == 0) return;
            int ma = (int)dgv.CurrentRow.Cells["MaHV"].Value;
            var hv = _hocViens.FirstOrDefault(h => h.MaHV == ma);
            if (hv == null) return;

            _maDangSua = hv.MaHV;
            lblTieuDeForm.Text = hv.HoTen;
            txtHoTen.Text = hv.HoTen;
            txtSDT.Text = hv.SDT;
            txtPhuHuynh.Text = hv.TenPhuHuynh;
            txtSDTPH.Text = hv.SDTPhuHuynh;
            txtTruongLop.Text = hv.TruongLop;
            txtDiaChi.Text = hv.DiaChi;
            cboTrinhDo.Text = hv.TrinhDo ?? "";
            txtGhiChu.Text = hv.GhiChu;

            var lop = _lops.Where(l => l.MaHV == hv.MaHV).OrderByDescending(l => l.ThoiGianBatDau).ToList();
            lblLop.Text = lop.Count == 0
                ? "Chưa học lớp nào."
                : "Các lớp: " + string.Join("; ", lop.Take(4).Select(l => $"{l.MonHoc} với {l.GiaSu?.HoTen} ({l.TrangThaiTheoThoiGian.ToLower()})"))
                  + (lop.Count > 4 ? $"; và {lop.Count - 4} lớp khác" : "");

            btnXoa.Enabled = true;
            btnPhieuBao.Enabled = lop.Count > 0;
            btnLuu.Text = "Lưu thay đổi";
            btnLuu.Width = 150;
        }

        private void BatDauThemMoi()
        {
            _maDangSua = null;
            dgv.ClearSelection();
            lblTieuDeForm.Text = "Học viên mới";
            foreach (var t in new[] { txtHoTen, txtSDT, txtPhuHuynh, txtSDTPH, txtTruongLop, txtDiaChi, txtGhiChu }) t.Clear();
            cboTrinhDo.Text = "";
            lblLop.Text = "";
            btnXoa.Enabled = false;
            btnPhieuBao.Enabled = false;
            btnLuu.Text = "Thêm";
            btnLuu.Width = 110;
            txtHoTen.Focus();
        }

        private void Luu()
        {
            var hs = new HoSoHocVien
            {
                HoTen = txtHoTen.Text, SDT = txtSDT.Text, DiaChi = txtDiaChi.Text,
                TenPhuHuynh = txtPhuHuynh.Text, SDTPhuHuynh = txtSDTPH.Text,
                TruongLop = txtTruongLop.Text, TrinhDo = cboTrinhDo.Text, GhiChu = txtGhiChu.Text
            };
            try
            {
                HocVien hv;
                bool moi = !_maDangSua.HasValue;
                using (var service = new HocVienService()) hv = service.LuuHoSo(_maDangSua, hs);
                NapDuLieu(hv.MaHV);
                _maDangSua = hv.MaHV;
                HienHoSoDangChon();
                MessageBox.Show(moi ? $"Đã thêm học viên {hv.HoTen}." : $"Đã lưu hồ sơ {hv.HoTen}.",
                    Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (ex.Message.Contains("Họ tên") || ex.Message.Contains("họ tên")) txtHoTen.Focus();
                else if (ex.Message.Contains("phụ huynh")) txtSDTPH.Focus();
                else if (ex.Message.Contains("điện thoại")) txtSDT.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không lưu được: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Xoa()
        {
            if (!_maDangSua.HasValue) return;
            var hv = _hocViens.FirstOrDefault(h => h.MaHV == _maDangSua.Value);
            if (MessageBox.Show($"Xóa học viên {hv?.HoTen}? Thao tác này không hoàn tác được.",
                    Constants.Messages.ConfirmTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            try
            {
                using (var service = new HocVienService()) service.Delete(_maDangSua.Value);
                _maDangSua = null;
                NapDuLieu();
                BatDauThemMoi();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Constants.Messages.LoiKhiXoa}{ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaoPhieuBao()
        {
            if (!_maDangSua.HasValue) return;
            var hv = _hocViens.First(h => h.MaHV == _maDangSua.Value);
            try
            {
                var kq = new HocTapService().GetKetQuaHocVien(hv.MaHV);
                string path = new BaoCaoService().TaoPhieuBao(hv, kq);
                Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tạo được phiếu báo: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void XuatCsv()
        {
            using (var dlg = new SaveFileDialog { Filter = "CSV (Excel)|*.csv", FileName = $"HocVien_{DateTime.Now:yyyyMMdd}.csv" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    new BaoCaoService().XuatCsv(dgv, dlg.FileName);
                    MessageBox.Show("Đã xuất danh sách.", Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không xuất được: {ex.Message}", Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
