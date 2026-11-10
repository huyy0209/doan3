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
using DoAnWD.UI.Controls;
using DoAnWD.UI.Dialogs;

namespace DoAnWD.UI.Pages
{
    /// <summary>
    /// Học viên (gộp cả điểm danh + điểm):
    ///  - Tab "Theo học viên": danh sách bên trái; bên phải là buổi học & điểm của học viên đang chọn
    ///    (sửa điểm danh / điểm / nhận xét, xếp học bù) và hồ sơ.
    ///  - Tab "Điểm danh theo ca": điểm danh cả ca trong một ngày, gồm cả học viên đến học bù.
    /// Gia sư đăng nhập chỉ thấy học viên của mình và không sửa hồ sơ.
    /// File này: khung trang, danh sách, hồ sơ. Xem thêm frmHocVien.BuoiHoc.cs, frmHocVien.DiemDanh.cs.
    /// </summary>
    public partial class frmHocVien : Form, ITuGiaoDien, ILamMoi
    {
        public void LamMoi()
        {
            NapDuLieu(_maDangSua);
            // Danh sách điểm danh chỉ nạp khi đang mở tab đó (chuyển tab sẽ tự nạp)
            if (tabsTrang.SelectedIndex == 1) NapDiemDanh();
        }

        /// <summary>Gia sư đăng nhập: chỉ học viên / lớp của mình; quản lý = null</summary>
        private readonly int? _gioiHanGS = PhienDangNhap.GioiHanGiaSu;
        private bool LaQuanLy => !_gioiHanGS.HasValue;

        private TabControl tabsTrang, tabsChiTiet;
        private Label lblLienHe;

        private const string TatCa = "Tất cả";
        private const string DangHoc = "Đang học";
        private const string ChuaCoLop = "Chưa có lớp";
        private const string DaNghi = "Đã nghỉ";

        private readonly HocTapService _hocTap = new HocTapService();
        private SapXepLoc _sx;
        private List<HocVien> _hocViens = new List<HocVien>();
        private List<LopHoc> _lops = new List<LopHoc>();
        private int? _maDangSua;   // null = đang thêm mới

        private TextBox txtTim;
        private ComboBox cboTinhTrang, cboTrinhDoLoc;
        private DataGridView dgv;
        private Label lblDem;
        private Pager pager;

        private Label lblTieuDeForm, lblLop;
        private HoSoHocVienEditor editor;
        private ThemedButton btnThemMoi, btnLuu, btnXoa, btnPhieuBao, btnXuat;

        public frmHocVien()
        {
            InitializeComponent();
            BuildUi();
            Load += (s, e) =>
            {
                NapDuLieu();
                if (!_maDangSua.HasValue) ChuaChon();
            };
        }

        /// <summary>Mở thẳng tab điểm danh theo ca (màn hình mặc định của gia sư)</summary>
        public void MoTabDiemDanh()
        {
            if (tabsTrang != null) tabsTrang.SelectedIndex = 1;
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
            cboTinhTrang = Ui.Combo(false, 130);
            cboTinhTrang.Items.AddRange(new object[] { TatCa, DangHoc, ChuaCoLop, DaNghi });
            cboTinhTrang.SelectedIndex = 0;
            cboTinhTrang.SelectedIndexChanged += (s, e) => ApDungLoc();
            cboTrinhDoLoc = Ui.Combo(false, 130);
            cboTrinhDoLoc.SelectedIndexChanged += (s, e) => ApDungLoc();
            Ui.DanhDauBoLoc(cboTinhTrang, cboTrinhDoLoc);

            var loc = Ui.HangLoc(txtTim, cboTinhTrang, cboTrinhDoLoc);

            dgv = Ui.Grid();
            dgv.SelectionChanged += (s, e) => HienHoSoDangChon();
            // Trường / lớp, trình độ đã hiện ở phần đầu bên phải => ẩn cho bảng gọn (vẫn tìm / lọc được)
            ThemeManager.TieuDeCot(dgv, "MaHV", null, "TruongLop", null, "TrinhDo", null, "HoTen", "Họ tên",
                "SDT", "SĐT liên hệ", "PhuHuynh", "Phụ huynh", "DangHocMon", "Đang học");
            // Mặc định A-Z theo tên gọi; bấm tiêu đề cột để đổi cách sắp xếp
            _sx = new SapXepLoc(dgv, () => ApDungLoc(), "HoTen");
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
            lblTieuDeForm = new Label { Font = Theme.Section, ForeColor = Theme.Ink, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            lblLienHe = Ui.Text("", true);
            lblLienHe.Margin = new Padding(0, 0, 0, 4);
            editor = new HoSoHocVienEditor();
            lblLop = Ui.Text("", true);
            lblLop.MaximumSize = new Size(560, 0);

            btnThemMoi = Ui.Button("Thêm học viên", ButtonKind.Secondary, Glyph.Plus);
            btnThemMoi.Click += (s, e) => MoCuaSoThem();
            btnThemMoi.Visible = LaQuanLy;
            btnLuu = Ui.Button("Lưu hồ sơ", ButtonKind.Primary, Glyph.Check, 130);
            btnLuu.Click += (s, e) => Luu();
            btnXoa = Ui.Button("Xóa học viên", ButtonKind.Danger, Glyph.Trash, 130);
            btnXoa.Click += (s, e) => Xoa();
            btnPhieuBao = Ui.Button("Phiếu báo kết quả", ButtonKind.Ghost, Glyph.Chart);
            btnPhieuBao.Click += (s, e) => TaoPhieuBao();

            var hoSo = Ui.Stack(false, editor, lblLop, Ui.Row(btnLuu, btnXoa));
            hoSo.Dock = DockStyle.Fill;
            hoSo.AutoScroll = true;
            hoSo.Padding = new Padding(0, 8, 8, 0);
            if (!LaQuanLy)
            {
                // Gia sư xem hồ sơ (liên hệ phụ huynh) nhưng không sửa / xóa
                btnLuu.Visible = btnXoa.Visible = false;
            }

            // ---- Bên phải: tên học viên + 2 tab (Buổi học & điểm, Hồ sơ) ----
            tabsChiTiet = new TabControl { Dock = DockStyle.Fill };
            var tBuoi = new TabPage("Buổi học & điểm") { BackColor = Theme.Surface, Padding = new Padding(0, 8, 0, 0) };
            var tHoSo = new TabPage("Hồ sơ") { BackColor = Theme.Surface, Padding = new Padding(0, 8, 0, 0) };
            tBuoi.Controls.Add(BuildBuoiHoc());
            tHoSo.Controls.Add(hoSo);
            tabsChiTiet.TabPages.Add(tBuoi);
            tabsChiTiet.TabPages.Add(tHoSo);
            var hostCT = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = Theme.Surface };
            hostCT.Controls.Add(tabsChiTiet);

            var dauCT = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Margin = new Padding(0) };
            dauCT.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            dauCT.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dauCT.Controls.Add(Ui.Stack(false, lblTieuDeForm, lblLienHe), 0, 0);
            dauCT.Controls.Add(btnPhieuBao, 1, 0);

            var secCT = Ui.Section("Học viên đang chọn");
            secCT.Margin = new Padding(0);
            var stCT = Ui.Stack(false, dauCT, hostCT);
            stCT.RowStyles[1] = new RowStyle(SizeType.Percent, 100);
            hostCT.Dock = DockStyle.Fill;
            secCT.Controls.Add(stCT);

            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = new Padding(0) };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63));
            cot.Controls.Add(secDs, 0, 0);
            cot.Controls.Add(secCT, 1, 0);

            // ---- Trang: 2 tab ----
            tabsTrang = new TabControl { Dock = DockStyle.Fill };
            var tab1 = new TabPage("Theo học viên") { BackColor = Theme.Paper, Padding = new Padding(0, 10, 0, 0) };
            var tab2 = new TabPage("Điểm danh theo ca") { BackColor = Theme.Paper, Padding = new Padding(0, 10, 0, 0) };
            tab1.Controls.Add(cot);
            tab2.Controls.Add(BuildDiemDanh());
            tabsTrang.TabPages.Add(tab1);
            tabsTrang.TabPages.Add(tab2);
            // Vừa điểm danh theo ca xong, quay lại tab học viên thì thấy ngay dữ liệu mới
            tabsTrang.SelectedIndexChanged += (s, e) =>
            {
                if (tabsTrang.SelectedIndex == 0) HienBuoiHoc(); else NapDiemDanh();
            };
            var hostTrang = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 0) };
            hostTrang.Controls.Add(tabsTrang);

            var tieuDe = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tieuDe.Controls.Add(Ui.Stack(false, Ui.Title("Học viên"),
                Ui.Text("Chọn học viên để sửa điểm danh, điểm test, xếp học bù. Điểm danh cả ca ở tab \"Điểm danh theo ca\".", true)), 0, 0);
            tieuDe.Controls.Add(btnThemMoi, 1, 0);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(tieuDe, 0, 0);
            root.Controls.Add(hostTrang, 0, 1);
            Controls.Add(root);
            TabStrip.Attach(tabsTrang);
            var stripCT = TabStrip.Attach(tabsChiTiet);
            if (stripCT != null) { stripCT.BackColor = Theme.Surface; stripCT.Padding = new Padding(0); }
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
                // GetAll của lớp đã kèm gia sư + học viên trong một truy vấn
                using (var ls = new LopHocService()) _lops = ls.GetAll().ToList();
                if (LaQuanLy)
                {
                    using (var hvs = new HocVienService()) _hocViens = hvs.GetAll().ToList();
                }
                else
                {
                    // Gia sư: học viên của mình (kể cả học viên mình dạy bù), lớp của mình
                    _hocViens = _hocTap.GetHocVienCoLop(_gioiHanGS);
                    _lops = _lops.Where(l => l.MaGS == _gioiHanGS).ToList();
                }

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
                ;
            var dsSap = (_sx != null ? _sx.Ap(ds) : ds).ToList();

            int? giu = chonMaHV ?? _maDangSua;
            int tong = dsSap.Count;
            dgv.DataSource = pager.Cat(dsSap);   // chỉ đưa 1 trang vào lưới
            lblDem.Text = $"{tong} / {_hocViens.Count} học viên";

            if (giu.HasValue)
            {
                foreach (DataGridViewRow r in dgv.Rows)
                    if ((int)r.Cells["MaHV"].Value == giu.Value) { dgv.CurrentCell = r.Cells["HoTen"]; break; }
            }
        }

        private void HienHoSoDangChon()
        {
            // Không có dòng được chọn (vừa bấm "Thêm học viên") => giữ form trống
            if (dgv.CurrentRow == null || dgv.SelectedRows.Count == 0) return;
            int ma = (int)dgv.CurrentRow.Cells["MaHV"].Value;
            var hv = _hocViens.FirstOrDefault(h => h.MaHV == ma);
            if (hv == null) return;

            bool doiNguoi = _maDangSua != hv.MaHV;
            if (doiNguoi && !HoiLuuBuoiHoc()) return;
            _maDangSua = hv.MaHV;
            lblTieuDeForm.Text = hv.HoTen;
            lblLienHe.Text = string.Join(" · ", new[]
            {
                string.IsNullOrEmpty(hv.TenPhuHuynh) && string.IsNullOrEmpty(hv.SDTPhuHuynh) ? null : $"PH {hv.TenPhuHuynh} {hv.SDTPhuHuynh}".Trim(),
                hv.SDT, hv.TrinhDo, hv.TruongLop
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
            editor.Enabled = LaQuanLy;
            editor.Nap(hv);

            var lop = _lops.Where(l => l.MaHV == hv.MaHV).OrderByDescending(l => l.ThoiGianBatDau).ToList();
            lblLop.Text = lop.Count == 0
                ? "Chưa học lớp nào."
                : "Các lớp: " + string.Join("; ", lop.Take(4).Select(l => $"{l.MonHoc} với {l.GiaSu?.HoTen} ({l.TrangThaiTheoThoiGian.ToLower()})"))
                  + (lop.Count > 4 ? $"; và {lop.Count - 4} lớp khác" : "");

            btnLuu.Enabled = btnXoa.Enabled = true;
            btnPhieuBao.Enabled = lop.Count > 0;
            if (doiNguoi) NapBuoiHoc(); else HienBuoiHoc();
        }

        /// <summary>Chưa chọn ai: khung hồ sơ để trống và khóa lại</summary>
        private void ChuaChon()
        {
            _maDangSua = null;
            dgv.ClearSelection();
            lblTieuDeForm.Text = "Chưa chọn học viên";
            lblLienHe.Text = "";
            editor.XoaTrang();
            editor.Enabled = false;
            lblLop.Text = "Chọn một dòng ở danh sách để xem và sửa hồ sơ.\nBấm \"Thêm học viên\" để nhập người mới.";
            btnLuu.Enabled = btnXoa.Enabled = btnPhieuBao.Enabled = false;
            NapBuoiHoc();
        }

        /// <summary>Bật cửa sổ nhập học viên mới; lưu xong thì chọn luôn người vừa thêm</summary>
        private void MoCuaSoThem()
        {
            var ed = new HoSoHocVienEditor();
            HocVien moi = null;
            using (var dlg = new dlgHoSo("Thêm học viên", "Chỉ cần họ tên và một số điện thoại liên hệ (của bé hoặc phụ huynh).",
                       ed, () => LuuHoSo(null, ed, out moi), "Thêm"))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK || moi == null) return;
            }
            txtTim.Clear();
            cboTinhTrang.SelectedIndex = 0;
            NapDuLieu(moi.MaHV);
            HienHoSoDangChon();
        }

        /// <summary>Lưu qua service; lỗi nhập liệu thì báo và đưa con trỏ về đúng ô</summary>
        private bool LuuHoSo(int? ma, HoSoHocVienEditor ed, out HocVien kq)
        {
            kq = null;
            try
            {
                using (var service = new HocVienService()) kq = service.LuuHoSo(ma, ed.Lay());
                return true;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ed.FocusTheoLoi(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không lưu được: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void Luu()
        {
            if (!_maDangSua.HasValue) return;
            HocVien hv;
            if (!LuuHoSo(_maDangSua, editor, out hv)) return;
            NapDuLieu(hv.MaHV);
            HienHoSoDangChon();
            MessageBox.Show($"Đã lưu hồ sơ {hv.HoTen}.", Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                ChuaChon();
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
                var kq = _hocTap.GetKetQuaHocVien(hv.MaHV, _gioiHanGS);
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
