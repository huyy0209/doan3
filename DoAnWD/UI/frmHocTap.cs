using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Dto;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;

namespace DoAnWD
{
    /// <summary>
    /// Điểm danh & điểm test 15 phút.
    ///  - Tab "Điểm danh theo ngày": chọn ngày + ca => danh sách lớp hôm đó, chọn có mặt/vắng,
    ///    gõ điểm (bỏ trống nếu hôm đó không kiểm tra), nhận xét => Lưu.
    ///  - Tab "Kết quả học viên": chọn học viên => điểm TB, xếp loại, chuyên cần từng lớp,
    ///    biểu đồ điểm, lịch sử buổi học, xuất phiếu báo cho phụ huynh.
    /// Gia sư đăng nhập chỉ thấy lớp của mình.
    /// </summary>
    public class frmHocTap : Form, ITuGiaoDien, ILamMoi
    {
        public void LamMoi() { NapDiemDanh(); NapHocVien(); }

        private readonly HocTapService _service = new HocTapService();
        private readonly int? _gioiHanGS = PhienDangNhap.GioiHanGiaSu;

        // Tab 1
        private DateTimePicker dtpNgay;
        private ComboBox cboCa;
        private DataGridView dgvDiemDanh;
        private Label lblDemDiemDanh;
        private BindingList<DongNhap> _dongs = new BindingList<DongNhap>();

        // Tab 2
        private TextBox txtTimHV;
        private ListBox lstHV;
        private Label lblTenHV, lblHoSoHV;
        private DataGridView dgvLop, dgvBuoi;
        private ScoreChart chart;
        private List<HocVien> _hocViens = new List<HocVien>();
        private List<KetQuaLop> _ketQua = new List<KetQuaLop>();

        public TabControl Tabs { get; private set; }

        /// <summary>Dòng nhập liệu trên lưới điểm danh (điểm nhập dạng chữ để gõ "8,5" hay "8.5" đều được)</summary>
        private class DongNhap
        {
            public int MaLop { get; set; }
            public string HocVien { get; set; }
            public string MonHoc { get; set; }
            public string GiaSu { get; set; }
            public string CaHoc { get; set; }
            public string DiemDanh { get; set; }
            public string DiemText { get; set; }
            public string NhanXet { get; set; }
            public string DaLuu { get; set; }
        }

        public frmHocTap()
        {
            BackColor = Theme.Paper;
            Font = Theme.Body;
            FormBorderStyle = FormBorderStyle.None;
            Padding = new Padding(20, 14, 20, 16);
            Text = "Điểm danh & điểm";
            BuildUi();
            Load += (s, e) => { NapDiemDanh(); NapHocVien(); };
        }

        // ===================================================================
        // GIAO DIỆN
        // ===================================================================

        private void BuildUi()
        {
            Tabs = new TabControl { Dock = DockStyle.Fill };
            var tab1 = new TabPage("Điểm danh theo ngày") { BackColor = Theme.Paper, Padding = new Padding(0, 12, 0, 0) };
            var tab2 = new TabPage("Kết quả học viên") { BackColor = Theme.Paper, Padding = new Padding(0, 12, 0, 0) };
            Tabs.TabPages.Add(tab1);
            Tabs.TabPages.Add(tab2);
            tab1.Controls.Add(BuildDiemDanh());
            tab2.Controls.Add(BuildKetQua());

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(Ui.Stack(false, Ui.Title("Điểm danh & điểm"),
                Ui.Text("Cuối mỗi buổi: đánh dấu có mặt / vắng, nhập điểm bài test 15 phút nếu hôm đó có kiểm tra.", true)), 0, 0);
            var host = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 0) };
            host.Controls.Add(Tabs);
            root.Controls.Add(host, 0, 1);
            Controls.Add(root);
            TabStrip.Attach(Tabs);
        }

        private Control BuildDiemDanh()
        {
            dtpNgay = Ui.Date(140);
            dtpNgay.MaxDate = DateTime.Now.Date;
            dtpNgay.Value = DateTime.Now.Date;
            dtpNgay.ValueChanged += (s, e) => NapDiemDanh();

            cboCa = Ui.Combo(false, 210);
            cboCa.Items.Add("Tất cả ca");
            cboCa.Items.AddRange(Constants.CaHoc.TatCaCaHoc);
            string caNay = ShiftRibbon.CaHienTai(DateTime.Now);
            cboCa.SelectedItem = caNay;
            if (cboCa.SelectedIndex < 0) cboCa.SelectedIndex = 0;
            cboCa.SelectedIndexChanged += (s, e) => NapDiemDanh();

            var btnHomNay = Ui.Button("Hôm nay", ButtonKind.Ghost, Glyph.Calendar);
            btnHomNay.Click += (s, e) => dtpNgay.Value = DateTime.Now.Date;
            var btnCoMat = Ui.Button("Tất cả có mặt", ButtonKind.Secondary, Glyph.Check);
            btnCoMat.Click += (s, e) => { foreach (var d in _dongs) d.DiemDanh = Constants.DiemDanh.CoMat; dgvDiemDanh.Refresh(); };
            var btnLuu = Ui.Button("Lưu điểm danh", ButtonKind.Primary, Glyph.Check);
            btnLuu.Click += (s, e) => LuuDiemDanh();

            var nhanNgay = Ui.FieldLabel("Ngày");
            var nhanCa = Ui.FieldLabel("Ca");
            nhanNgay.Margin = nhanCa.Margin = new Padding(0, 10, 6, 0);
            dtpNgay.Margin = new Padding(0, 6, 16, 0);
            cboCa.Margin = new Padding(0, 6, 8, 0);
            var thanh = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            thanh.Controls.AddRange(new Control[] { nhanNgay, dtpNgay, nhanCa, cboCa, btnHomNay });

            dgvDiemDanh = Ui.Grid();
            dgvDiemDanh.ReadOnly = false;
            dgvDiemDanh.AutoGenerateColumns = false;
            dgvDiemDanh.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvDiemDanh.SelectionMode = DataGridViewSelectionMode.CellSelect;
            Func<string, string, int, bool, DataGridViewTextBoxColumn> cot = (prop, header, w, ro) =>
                new DataGridViewTextBoxColumn { DataPropertyName = prop, Name = prop, HeaderText = header, ReadOnly = ro, FillWeight = w };
            dgvDiemDanh.Columns.Add(cot("HocVien", "Học viên", 170, true));
            dgvDiemDanh.Columns.Add(cot("MonHoc", "Môn", 90, true));
            var cGS = cot("GiaSu", "Gia sư", 140, true);
            cGS.Visible = !_gioiHanGS.HasValue;
            dgvDiemDanh.Columns.Add(cGS);
            dgvDiemDanh.Columns.Add(cot("CaHoc", "Ca", 140, true));
            var cDD = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "DiemDanh", Name = "DiemDanhChon", HeaderText = "Điểm danh", FillWeight = 130,
                FlatStyle = FlatStyle.Flat, DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
            cDD.Items.AddRange(Constants.DiemDanh.TatCa);
            dgvDiemDanh.Columns.Add(cDD);
            var cDiem = cot("DiemText", "Điểm test (0–10)", 90, false);
            cDiem.DefaultCellStyle.BackColor = Theme.InkFocus;
            cDiem.DefaultCellStyle.Font = Theme.BodyStrong;
            dgvDiemDanh.Columns.Add(cDiem);
            var cNX = cot("NhanXet", "Nhận xét", 200, false);
            cNX.DefaultCellStyle.BackColor = Theme.InkFocus;
            dgvDiemDanh.Columns.Add(cNX);
            dgvDiemDanh.Columns.Add(cot("DaLuu", "", 70, true));
            dgvDiemDanh.DataSource = _dongs;
            dgvDiemDanh.CellValidating += DgvDiemDanh_CellValidating;
            dgvDiemDanh.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var col = dgvDiemDanh.Columns[e.ColumnIndex].Name;
                if (col == "DaLuu") { e.CellStyle.ForeColor = Theme.Success; e.CellStyle.Font = Theme.SmallStrong; }
                if (col == "DiemDanhChon")
                {
                    string v = e.Value as string;
                    if (v != null && !Constants.DiemDanh.CoHoc(v)) e.CellStyle.ForeColor = Theme.Margin;
                }
            };
            dgvDiemDanh.CellValueChanged += (s, e) =>
            {
                // Chọn vắng thì xóa điểm của dòng đó
                if (e.RowIndex >= 0 && dgvDiemDanh.Columns[e.ColumnIndex].Name == "DiemDanhChon")
                {
                    var d = _dongs[e.RowIndex];
                    if (!Constants.DiemDanh.CoHoc(d.DiemDanh) && !string.IsNullOrEmpty(d.DiemText))
                    {
                        d.DiemText = "";
                        dgvDiemDanh.InvalidateRow(e.RowIndex);
                    }
                }
            };
            dgvDiemDanh.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvDiemDanh.IsCurrentCellDirty && dgvDiemDanh.CurrentCell is DataGridViewComboBoxCell)
                    dgvDiemDanh.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvDiemDanh.DataError += (s, e) => { e.ThrowException = false; };

            lblDemDiemDanh = Ui.Text("", true);
            var chan = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true };
            chan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            chan.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            chan.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            chan.Controls.Add(lblDemDiemDanh, 0, 0);
            chan.Controls.Add(btnCoMat, 1, 0);
            chan.Controls.Add(btnLuu, 2, 0);

            var sec = Ui.Section("Danh sách lớp trong ngày");
            sec.Margin = new Padding(0);
            var st = Ui.Stack(false, thanh, dgvDiemDanh, chan);
            st.RowStyles[1] = new RowStyle(SizeType.Percent, 100);
            dgvDiemDanh.Dock = DockStyle.Fill;
            sec.Controls.Add(st);
            return sec;
        }

        private Control BuildKetQua()
        {
            txtTimHV = Ui.Input("Tìm học viên, phụ huynh...");
            txtTimHV.TextChanged += (s, e) => LocHocVien();
            lstHV = Ui.List(48, VeHocVien);
            lstHV.SelectedIndexChanged += (s, e) => HienKetQua();
            var khung = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = Theme.Rule, Margin = new Padding(0, 4, 0, 0) };
            khung.Controls.Add(lstHV);
            var secHV = Ui.Section("Học viên");
            var st1 = Ui.Stack(false, txtTimHV, khung);
            st1.RowStyles[1] = new RowStyle(SizeType.Percent, 100);
            khung.Dock = DockStyle.Fill;
            secHV.Controls.Add(st1);

            lblTenHV = new Label { Font = Theme.Section, ForeColor = Theme.Ink, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            lblHoSoHV = Ui.Text("", true);
            var btnPhieu = Ui.Button("Phiếu báo kết quả", ButtonKind.Primary, Glyph.Clipboard);
            btnPhieu.Click += (s, e) => TaoPhieuBao();
            var dau = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
            dau.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            dau.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dau.Controls.Add(Ui.Stack(false, lblTenHV, lblHoSoHV), 0, 0);
            dau.Controls.Add(btnPhieu, 1, 0);

            dgvLop = Ui.Grid();
            dgvLop.Height = 150;
            dgvLop.Dock = DockStyle.Top;
            dgvLop.SelectionChanged += (s, e) => HienBuoi();
            // Cấu hình cột sau khi bind (lưới ở tab ẩn chỉ tạo cột khi hiện ra)
            dgvLop.DataBindingComplete += (s, e) =>
            {
                if (dgvLop.Columns.Count == 0) return;
                dgvLop.Columns["MaLop"].Visible = false;
                dgvLop.Columns["MonHoc"].HeaderText = "Môn";
                dgvLop.Columns["GiaSu"].HeaderText = "Gia sư";
                dgvLop.Columns["CaHoc"].HeaderText = "Ca học";
                dgvLop.Columns["ChuyenCan"].HeaderText = "Chuyên cần";
                dgvLop.Columns["SoBaiTest"].HeaderText = "Số bài test";
                dgvLop.Columns["DiemTB"].HeaderText = "Điểm TB";
                dgvLop.Columns["DiemTB"].DefaultCellStyle.Font = Theme.BodyStrong;
                dgvLop.Columns["XepLoai"].HeaderText = "Xếp loại";
                dgvLop.Columns["GiaSu"].Visible = !_gioiHanGS.HasValue;
            };

            chart = new ScoreChart { Dock = DockStyle.Top, Height = 150, Margin = new Padding(0, 8, 0, 8) };

            dgvBuoi = Ui.Grid();
            dgvBuoi.DataBindingComplete += (s, e) =>
            {
                if (dgvBuoi.Columns.Count == 0) return;
                dgvBuoi.Columns["NgayHoc"].HeaderText = "Ngày";
                dgvBuoi.Columns["NgayHoc"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvBuoi.Columns["DiemDanh"].HeaderText = "Điểm danh";
                dgvBuoi.Columns["Diem"].HeaderText = "Điểm test";
                dgvBuoi.Columns["Diem"].DefaultCellStyle.Font = Theme.BodyStrong;
                dgvBuoi.Columns["NhanXet"].HeaderText = "Nhận xét";
            };

            var secKQ = Ui.Section("Kết quả học tập");
            secKQ.Margin = new Padding(0);
            var st2 = Ui.Stack(false, dau, Ui.FieldLabel("Các lớp"), dgvLop,
                Ui.FieldLabel("Điểm test theo ngày (vạch xanh: 8 = Giỏi, vạch cam: 5 = Trung bình)"), chart,
                Ui.FieldLabel("Các buổi học của lớp đang chọn"), dgvBuoi);
            dgvLop.Dock = DockStyle.Fill;
            chart.Dock = DockStyle.Fill;
            st2.RowStyles[2] = new RowStyle(SizeType.Absolute, 124);
            st2.RowStyles[4] = new RowStyle(SizeType.Absolute, 130);
            st2.RowStyles[6] = new RowStyle(SizeType.Percent, 100);
            dgvBuoi.Dock = DockStyle.Fill;
            secKQ.Controls.Add(st2);

            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            cot.Controls.Add(secHV, 0, 0);
            cot.Controls.Add(secKQ, 1, 0);
            return cot;
        }

        // ===================================================================
        // ĐIỂM DANH THEO NGÀY
        // ===================================================================

        private void NapDiemDanh()
        {
            try
            {
                string ca = cboCa.SelectedIndex > 0 ? cboCa.SelectedItem as string : null;
                var rows = _service.GetLopTrongNgay(dtpNgay.Value, ca, _gioiHanGS);
                _dongs = new BindingList<DongNhap>(rows.Select(r => new DongNhap
                {
                    MaLop = r.MaLop, HocVien = r.HocVien, MonHoc = r.MonHoc, GiaSu = r.GiaSu, CaHoc = r.CaHoc,
                    DiemDanh = r.DiemDanh, DiemText = r.Diem?.ToString("0.##"), NhanXet = r.NhanXet,
                    DaLuu = r.DaLuu ? "Đã lưu" : ""
                }).ToList());
                dgvDiemDanh.DataSource = _dongs;
                int daLuu = rows.Count(r => r.DaLuu);
                lblDemDiemDanh.Text = rows.Count == 0
                    ? "Không có lớp nào học vào ngày / ca này."
                    : $"{rows.Count} lớp, đã lưu {daLuu}. Bỏ trống ô điểm nếu hôm nay không kiểm tra.";
            }
            catch (Exception ex)
            {
                lblDemDiemDanh.Text = "Không tải được: " + ex.GetBaseException().Message;
            }
        }

        private void DgvDiemDanh_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (dgvDiemDanh.Columns[e.ColumnIndex].Name != "DiemText") return;
            decimal? d;
            string text = e.FormattedValue?.ToString();
            var row = dgvDiemDanh.Rows[e.RowIndex];
            if (!HocTapService.TryParseDiem(text, out d) || (d.HasValue && (d < 0 || d > 10)))
            {
                row.ErrorText = "Điểm phải là số từ 0 đến 10, ví dụ 8,5";
                e.Cancel = true;
            }
            else row.ErrorText = "";
        }

        private void LuuDiemDanh()
        {
            dgvDiemDanh.EndEdit();
            if (_dongs.Count == 0) return;

            var rows = new List<DiemDanhRow>();
            foreach (var d in _dongs)
            {
                decimal? diem;
                if (!HocTapService.TryParseDiem(d.DiemText, out diem))
                {
                    MessageBox.Show($"Điểm của {d.HocVien} không hợp lệ.", Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                rows.Add(new DiemDanhRow { MaLop = d.MaLop, DiemDanh = d.DiemDanh, Diem = diem, NhanXet = d.NhanXet });
            }

            try
            {
                var loi = _service.LuuNgay(dtpNgay.Value, rows, _gioiHanGS);
                NapDiemDanh();
                if (loi.Count == 0)
                    MessageBox.Show($"Đã lưu điểm danh ngày {dtpNgay.Value:dd/MM/yyyy}.", Constants.Messages.SuccessTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Một số dòng chưa lưu:\n\n" + string.Join("\n", loi), Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NapHocVien();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không lưu được: {ex.GetBaseException().Message}", Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================================================================
        // KẾT QUẢ HỌC VIÊN
        // ===================================================================

        private void NapHocVien()
        {
            try
            {
                int? chon = (lstHV.SelectedItem as HocVien)?.MaHV;
                _hocViens = _service.GetHocVienCoLop(_gioiHanGS);
                LocHocVien(chon);
            }
            catch (Exception ex)
            {
                lblHoSoHV.Text = "Không tải được học viên: " + ex.GetBaseException().Message;
            }
        }

        private void LocHocVien(int? chon = null)
        {
            chon = chon ?? (lstHV.SelectedItem as HocVien)?.MaHV;
            var ds = _hocViens.Where(h => TimKiem.Khop(txtTimHV.Text, h.HoTen, h.TenPhuHuynh, h.SDTLienHe, h.TruongLop)).ToList();
            lstHV.BeginUpdate();
            lstHV.Items.Clear();
            lstHV.Items.AddRange(ds.Cast<object>().ToArray());
            var giu = ds.FirstOrDefault(h => h.MaHV == chon);
            if (giu != null) lstHV.SelectedItem = giu;
            else if (ds.Count > 0) lstHV.SelectedIndex = 0;
            lstHV.EndUpdate();
            if (ds.Count == 0) HienKetQua();
        }

        private void VeHocVien(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var hv = (HocVien)lstHV.Items[e.Index];
            var r = Ui.DrawListItemBackground(e);
            TextRenderer.DrawText(e.Graphics, hv.HoTen, Theme.BodyStrong, new Rectangle(r.X, r.Y, r.Width, r.Height / 2),
                Theme.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(e.Graphics, string.Join(", ", new[] { hv.TrinhDo, hv.TruongLop }.Where(x => !string.IsNullOrEmpty(x))),
                Theme.Small, new Rectangle(r.X, r.Y + r.Height / 2, r.Width, r.Height / 2), Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void HienKetQua()
        {
            var hv = lstHV.SelectedItem as HocVien;
            if (hv == null)
            {
                lblTenHV.Text = "Chưa chọn học viên";
                lblHoSoHV.Text = _hocViens.Count == 0 ? "Chưa có học viên nào có lớp." : "";
                dgvLop.DataSource = null; dgvBuoi.DataSource = null; chart.SetData(new KeyValuePair<DateTime, decimal>[0]);
                return;
            }

            lblTenHV.Text = hv.HoTen;
            lblHoSoHV.Text = string.Join(", ", new[]
            {
                hv.TrinhDo, hv.TruongLop,
                string.IsNullOrEmpty(hv.TenPhuHuynh) ? null : $"PH {hv.TenPhuHuynh} {hv.SDTPhuHuynh}"
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

            _ketQua = _service.GetKetQuaHocVien(hv.MaHV, _gioiHanGS);
            dgvLop.DataSource = _ketQua.Select(k => new
            {
                k.MaLop, k.MonHoc, k.GiaSu, k.CaHoc, ChuyenCan = k.ChuyenCan,
                SoBaiTest = k.SoBaiTest, DiemTB = k.DiemTrungBinh?.ToString("0.##") ?? "", k.XepLoai
            }).ToList();
            HienBuoi();
        }

        private void HienBuoi()
        {
            KetQuaLop kq = null;
            if (dgvLop.CurrentRow != null && dgvLop.Columns.Contains("MaLop"))
            {
                int ma = (int)dgvLop.CurrentRow.Cells["MaLop"].Value;
                kq = _ketQua.FirstOrDefault(k => k.MaLop == ma);
            }
            kq = kq ?? _ketQua.FirstOrDefault();
            if (kq == null) { dgvBuoi.DataSource = null; chart.SetData(new KeyValuePair<DateTime, decimal>[0]); return; }

            chart.SetData(kq.Buoi.Where(b => b.Diem.HasValue).Select(b => new KeyValuePair<DateTime, decimal>(b.NgayHoc, b.Diem.Value)));
            // Chỉ hiện 30 buổi gần nhất cho gọn (phiếu báo vẫn có đủ)
            dgvBuoi.DataSource = kq.Buoi.Take(30).Select(b => new
            {
                NgayHoc = b.NgayHoc, DiemDanh = b.DiemDanh, Diem = b.Diem?.ToString("0.##") ?? "", b.NhanXet
            }).ToList();
        }

        private void TaoPhieuBao()
        {
            var hv = lstHV.SelectedItem as HocVien;
            if (hv == null) return;
            try
            {
                string path = new BaoCaoService().TaoPhieuBao(hv, _service.GetKetQuaHocVien(hv.MaHV, _gioiHanGS));
                Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không tạo được phiếu báo: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
