using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Dto;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.UI;
using DoAnWD.UI.Controls;
using DoAnWD.UI.Dialogs;

namespace DoAnWD.UI.Pages
{
    /// <summary>
    /// Tab "Buổi học & điểm" của học viên đang chọn:
    /// chọn lớp => tổng kết + biểu đồ điểm + danh sách buổi (sửa trực tiếp điểm danh, điểm, nhận xét),
    /// thêm buổi còn thiếu, xếp / hủy học bù cho buổi vắng.
    /// </summary>
    public partial class frmHocVien
    {
        private ComboBox cboLopHV;
        private Label lblTongKet, lblGoiYBuoi;
        private ScoreChart chart;
        private DataGridView dgvBuoi;
        private DateTimePicker dtpThemBuoi;
        private ThemedButton btnThemBuoi, btnXepBu, btnHuyBu, btnLuuBuoi;

        private List<KetQuaLop> _ketQua = new List<KetQuaLop>();
        private BindingList<DongBuoi> _buoi = new BindingList<DongBuoi>();
        private CuonTaiDan<DongBuoi> _cuonBuoi;   // lớp học lâu có nhiều buổi: cuộn tới đâu hiện tới đó

        /// <summary>Một dòng trên lưới buổi học (điểm nhập dạng chữ để gõ "8,5" hay "8.5" đều được)</summary>
        private class DongBuoi
        {
            public int MaBuoi { get; set; }
            public int MaLop { get; set; }
            public DateTime NgayHoc { get; set; }
            public string Ca { get; set; }
            public string GiaSu { get; set; }
            public string DiemDanh { get; set; }
            public string DiemText { get; set; }
            public string NhanXet { get; set; }
            public bool ThiLenCap { get; set; }
            public string GhiChu { get; set; }

            [Browsable(false)] public BuoiHocRow Goc { get; set; }
            /// <summary>Đã sửa trên lưới, chưa lưu</summary>
            [Browsable(false)] public bool Sua { get; set; }
            /// <summary>Không sửa được: buổi bù chưa tới ngày, hoặc không phải lớp / buổi của gia sư đang đăng nhập</summary>
            [Browsable(false)] public bool KhoaSua { get; set; }
        }

        // ===================================================================
        // GIAO DIỆN
        // ===================================================================

        private Control BuildBuoiHoc()
        {
            cboLopHV = Ui.Combo(false, 330);
            cboLopHV.FormattingEnabled = true;   // để sự kiện Format đổi tên hiển thị của lớp
            cboLopHV.SelectedIndexChanged += (s, e) => { if (HoiLuuBuoiHoc()) HienBuoiHoc(); };
            cboLopHV.Format += (s, e) =>
            {
                var k = e.ListItem as KetQuaLop;
                if (k != null) e.Value = $"{k.TenLop} ({k.TrangThai?.ToLower()})";
            };

            dtpThemBuoi = Ui.Date(130);
            dtpThemBuoi.MaxDate = DateTime.Today;
            dtpThemBuoi.Value = DateTime.Today;
            btnThemBuoi = Ui.Button("Thêm buổi", ButtonKind.Ghost, Glyph.Plus);
            btnThemBuoi.Click += (s, e) => ThemBuoi();

            var dau = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true, Margin = new Padding(0) };
            dau.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dau.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            dau.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            dau.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            cboLopHV.Margin = new Padding(0, 4, 8, 0);
            dtpThemBuoi.Margin = new Padding(0, 6, 6, 0);
            dau.Controls.Add(cboLopHV, 0, 0);
            dau.Controls.Add(new Label { AutoSize = true }, 1, 0);
            dau.Controls.Add(dtpThemBuoi, 2, 0);
            dau.Controls.Add(btnThemBuoi, 3, 0);

            lblTongKet = new Label { AutoSize = true, Font = Theme.BodyStrong, ForeColor = Theme.Ink, Margin = new Padding(0, 6, 0, 2) };
            chart = new ScoreChart { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 6) };

            dgvBuoi = Ui.Grid();
            dgvBuoi.ReadOnly = false;
            dgvBuoi.AutoGenerateColumns = false;
            dgvBuoi.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvBuoi.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvBuoi.MultiSelect = false;
            dgvBuoi.ScrollBars = ScrollBars.Vertical;
            dgvBuoi.Tag = ThemeManager.TuChiaCot;   // cột chia theo tỉ lệ bên dưới, luôn vừa khung
            Func<string, string, int, bool, DataGridViewTextBoxColumn> cot = (prop, header, w, ro) =>
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = prop, Name = prop, HeaderText = header, ReadOnly = ro, FillWeight = w,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = Math.Min(w, 64)
                };
            var cNgay = cot("NgayHoc", "Ngày", 64, true);
            cNgay.DefaultCellStyle.Format = "dd/MM";
            dgvBuoi.Columns.Add(cNgay);
            dgvBuoi.Columns.Add(cot("Ca", "Ca", 84, true));
            dgvBuoi.Columns.Add(cot("GiaSu", "Gia sư", 90, true));
            var cDD = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "DiemDanh", Name = "DiemDanhChon", HeaderText = "Điểm danh", FillWeight = 124,
                FlatStyle = FlatStyle.Flat, DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 118
            };
            cDD.Items.AddRange(Constants.DiemDanh.TatCa);
            cDD.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);
            dgvBuoi.Columns.Add(cDD);
            var cDiem = cot("DiemText", "Điểm test", 62, false);
            cDiem.DefaultCellStyle.Font = Theme.BodyStrong;
            cDiem.DefaultCellStyle.Padding = new Padding(11, 0, 11, 0);
            dgvBuoi.Columns.Add(cDiem);
            var cNX = cot("NhanXet", "Nhận xét", 150, false);
            cNX.DefaultCellStyle.Padding = new Padding(11, 0, 11, 0);
            dgvBuoi.Columns.Add(cNX);
            dgvBuoi.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "ThiLenCap", Name = "ThiLenCap", HeaderText = "Thi lên cấp", FillWeight = 64,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 64
            });
            dgvBuoi.Columns.Add(cot("GhiChu", "Ghi chú", 112, true));
            dgvBuoi.DataSource = _buoi;

            dgvBuoi.CellValidating += (s, e) => KiemTraOdiem(dgvBuoi, e);
            dgvBuoi.CellBeginEdit += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _buoi.Count && _buoi[e.RowIndex].KhoaSua) e.Cancel = true;
            };
            dgvBuoi.CellValueChanged += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _buoi.Count) return;
                var d = _buoi[e.RowIndex];
                d.Sua = true;
                // Chọn vắng thì xóa điểm của dòng đó
                if (dgvBuoi.Columns[e.ColumnIndex].Name == "DiemDanhChon" &&
                    !Constants.DiemDanh.CoHoc(d.DiemDanh) && !string.IsNullOrEmpty(d.DiemText))
                {
                    d.DiemText = "";
                    dgvBuoi.InvalidateRow(e.RowIndex);
                }
                CapNhatNutBuoi();
            };
            dgvBuoi.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvBuoi.IsCurrentCellDirty && (dgvBuoi.CurrentCell is DataGridViewComboBoxCell || dgvBuoi.CurrentCell is DataGridViewCheckBoxCell))
                    dgvBuoi.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvBuoi.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _buoi.Count) return;
                var d = _buoi[e.RowIndex];
                string col = dgvBuoi.Columns[e.ColumnIndex].Name;
                if (d.Goc != null && d.Goc.LaHocBu) e.CellStyle.BackColor = Theme.InkSoft;     // dòng học bù: nền tím nhạt
                if (col == "DiemDanhChon" && !Constants.DiemDanh.CoHoc(d.DiemDanh) && d.Goc?.MaBuoiBu == null)
                    e.CellStyle.ForeColor = Theme.Margin;                                       // vắng chưa bù: chữ đỏ
                if (col == "GhiChu")
                {
                    e.CellStyle.Font = Theme.SmallStrong;
                    e.CellStyle.ForeColor = d.Goc != null && d.Goc.ThiLenCap
                        ? (d.Goc.KetQuaThi.EndsWith("đạt") && !d.Goc.KetQuaThi.EndsWith("chưa đạt") ? Theme.Success : Theme.Warning)
                        : Theme.Ink;
                }
                // Buổi bù chưa tới ngày: chữ mờ (điểm danh khi tới ngày học)
                if (d.Goc != null && d.Goc.LaHocBu && d.Goc.ChuaToiNgay && col != "GhiChu") e.CellStyle.ForeColor = Theme.TextFaint;
            };
            // Buổi khóa (bù chưa tới ngày / không phải của mình) hiện như chữ thường, không có khung nhập
            dgvBuoi.DataBindingComplete += (s, e) => KhoaDong(0, dgvBuoi.Rows.Count);
            dgvBuoi.RowsAdded += (s, e) => KhoaDong(e.RowIndex, e.RowCount);
            _cuonBuoi = new CuonTaiDan<DongBuoi>(dgvBuoi, 30);
            dgvBuoi.CurrentCellChanged += (s, e) => CapNhatNutBuoi();
            dgvBuoi.DataError += (s, e) => { e.ThrowException = false; };

            lblGoiYBuoi = Ui.Text("", true);
            btnXepBu = Ui.Button("Xếp học bù", ButtonKind.Secondary, Glyph.Calendar);
            btnXepBu.Click += (s, e) => XepHocBu();
            btnHuyBu = Ui.Button("Hủy học bù", ButtonKind.Ghost, Glyph.Trash);
            btnHuyBu.Click += (s, e) => HuyHocBu();
            btnLuuBuoi = Ui.Button("Lưu thay đổi", ButtonKind.Primary, Glyph.Check);
            btnLuuBuoi.Click += (s, e) => LuuBuoiHoc();

            var chan = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true, Margin = new Padding(0, 4, 0, 0) };
            chan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            chan.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            chan.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            chan.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            chan.Controls.Add(lblGoiYBuoi, 0, 0);
            chan.Controls.Add(btnHuyBu, 1, 0);
            chan.Controls.Add(btnXepBu, 2, 0);
            chan.Controls.Add(btnLuuBuoi, 3, 0);

            var st = Ui.Stack(false, dau, lblTongKet, chart, dgvBuoi, chan);
            st.Dock = DockStyle.Fill;
            st.RowStyles[2] = new RowStyle(SizeType.Absolute, 84);
            st.RowStyles[3] = new RowStyle(SizeType.Percent, 100);
            dgvBuoi.Dock = DockStyle.Fill;
            return st;
        }

        private void KhoaDong(int tu, int soDong)
        {
            for (int i = tu; i < tu + soDong && i < dgvBuoi.Rows.Count && i < _buoi.Count; i++)
                if (_buoi[i].KhoaSua)
                    foreach (var c in new[] { "DiemDanhChon", "DiemText", "NhanXet", "ThiLenCap" })
                        dgvBuoi.Rows[i].Cells[c].ReadOnly = true;
        }

        /// <summary>Ô điểm phải là số 0-10 (bỏ trống = buổi đó không kiểm tra)</summary>
        private static void KiemTraOdiem(DataGridView dgv, DataGridViewCellValidatingEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].Name != "DiemText") return;
            decimal? d;
            var row = dgv.Rows[e.RowIndex];
            if (!HocTapService.TryParseDiem(e.FormattedValue?.ToString(), out d) || (d.HasValue && (d < 0 || d > 10)))
            {
                row.ErrorText = "Điểm phải là số từ 0 đến 10, ví dụ 8,5";
                e.Cancel = true;
            }
            else row.ErrorText = "";
        }

        // ===================================================================
        // DỮ LIỆU
        // ===================================================================

        /// <summary>Nạp các lớp + buổi học của học viên đang chọn</summary>
        private void NapBuoiHoc()
        {
            int? lopDangChon = (cboLopHV.SelectedItem as KetQuaLop)?.MaLop;
            try
            {
                _ketQua = _maDangSua.HasValue ? _hocTap.GetKetQuaHocVien(_maDangSua.Value, _gioiHanGS) : new List<KetQuaLop>();
            }
            catch (Exception ex)
            {
                _ketQua = new List<KetQuaLop>();
                lblTongKet.Text = "Không tải được buổi học: " + ex.GetBaseException().Message;
            }

            cboLopHV.BeginUpdate();
            cboLopHV.Items.Clear();
            cboLopHV.Items.AddRange(_ketQua.Cast<object>().ToArray());
            cboLopHV.EndUpdate();
            // Ưu tiên giữ lớp đang xem, rồi lớp đang học, rồi lớp mới nhất
            var chon = _ketQua.FirstOrDefault(k => k.MaLop == lopDangChon)
                       ?? _ketQua.FirstOrDefault(k => k.TrangThai == Constants.LopHocStatus.DangDienRa)
                       ?? _ketQua.FirstOrDefault();
            _vuaNap = true;
            if (chon != null && cboLopHV.SelectedItem != chon) cboLopHV.SelectedItem = chon;   // => HienBuoiHoc
            else HienBuoiHoc();
        }

        private bool _buoiSua;   // đang hỏi lưu (tránh hỏi lồng nhau)
        private bool _vuaNap;    // dữ liệu lớp vừa đọc từ CSDL, HienBuoiHoc khỏi đọc lại

        private KetQuaLop LopDangXem => cboLopHV.SelectedItem as KetQuaLop;

        /// <summary>Gia sư chỉ sửa buổi của lớp mình, hoặc buổi bù mình dạy</summary>
        private bool DuocSua(KetQuaLop k, BuoiHocRow b) =>
            LaQuanLy || k.MaGS == _gioiHanGS || (b != null && b.LaHocBu && b.MaGSDay == _gioiHanGS);

        private void HienBuoiHoc()
        {
            var k = LopDangXem;
            if (k == null)
            {
                _vuaNap = false;
                _cuonBuoi.Nap(new DongBuoi[0]);
                _buoi = _cuonBuoi.HienThi;
                chart.SetData(new KeyValuePair<DateTime, decimal>[0]);
                lblTongKet.Text = !_maDangSua.HasValue ? "" : "Học viên chưa có lớp nào.";
                lblGoiYBuoi.Text = !_maDangSua.HasValue ? "Chọn học viên ở danh sách bên trái." : "";
                cboLopHV.Enabled = btnThemBuoi.Enabled = dtpThemBuoi.Enabled = false;
                CapNhatNutBuoi();
                return;
            }

            // Lấy số liệu mới nhất của lớp (sau khi lưu / xếp bù ở nơi khác); vừa nạp xong thì khỏi đọc lại
            if (_maDangSua.HasValue && !_vuaNap)
            {
                try
                {
                    var moi = _hocTap.GetKetQuaHocVien(_maDangSua.Value, _gioiHanGS).FirstOrDefault(x => x.MaLop == k.MaLop);
                    if (moi != null)
                    {
                        int i = _ketQua.FindIndex(x => x.MaLop == k.MaLop);
                        if (i >= 0) _ketQua[i] = moi;
                        k = moi;
                    }
                }
                catch { /* giữ số liệu cũ */ }
            }

            _vuaNap = false;
            cboLopHV.Enabled = true;
            string danhGia = k.TienTrinh != null
                ? $"  ·  cấp {k.TienTrinh.CapHienTai}" + (k.TienTrinh.CapKeTiep != null ? $" (tiếp theo: {k.TienTrinh.CapKeTiep})" : " (cao nhất)")
                : (k.DiemTrungBinh.HasValue ? $"  ·  điểm TB {k.DiemTrungBinh:0.##} ({k.XepLoai})" : "");
            lblTongKet.Text = $"Chuyên cần {k.SoBuoiCoMat}/{k.SoBuoi} buổi" +
                              (k.SoBuoiHocBu > 0 ? $" (có {k.SoBuoiHocBu} buổi học bù)" : "") +
                              (k.SoBuoiVang > 0 ? $"  ·  vắng {k.SoBuoiVang}" : "") +
                              $"  ·  {k.SoBaiTest} bài test" + danhGia;
            // Cột thi lên cấp chỉ có ở môn năng khiếu
            dgvBuoi.Columns["ThiLenCap"].Visible = k.LaNangKhieu;
            chart.SetData(k.Buoi.Where(b => b.Diem.HasValue)
                .Select(b => new KeyValuePair<DateTime, decimal>(b.NgayHoc, b.Diem.Value)));

            _cuonBuoi.Nap(k.Buoi.Select(b => new DongBuoi
            {
                MaBuoi = b.MaBuoi,
                MaLop = b.MaLop,
                NgayHoc = b.NgayHoc,
                Ca = Constants.CaHoc.TenNgan(b.CaHoc),
                GiaSu = b.GiaSu,
                DiemDanh = b.DiemDanh,
                DiemText = b.Diem?.ToString("0.##"),
                NhanXet = b.NhanXet,
                ThiLenCap = b.ThiLenCap,
                GhiChu = GhiChuBu(b),
                Goc = b,
                KhoaSua = (b.LaHocBu && b.ChuaToiNgay) || !DuocSua(k, b)
            }));
            _buoi = _cuonBuoi.HienThi;

            bool sua = DuocSua(k, null);
            btnThemBuoi.Enabled = dtpThemBuoi.Enabled = sua;
            if (k.BatDau.HasValue) dtpThemBuoi.MinDate = new DateTime(Math.Min(k.BatDau.Value.Date.Ticks, DateTime.Today.Ticks));
            lblGoiYBuoi.Text = _buoi.Count == 0
                ? "Chưa có buổi nào được điểm danh. Thêm buổi hoặc điểm danh ở tab \"Điểm danh theo ca\"."
                : "Ô điểm trống = buổi đó không kiểm tra. Chọn buổi vắng để xếp học bù.";
            CapNhatNutBuoi();
        }

        private static string GhiChuBu(BuoiHocRow b)
        {
            if (b.ThiLenCap) return b.KetQuaThi;
            if (b.LaHocBu)
                return b.ChuaToiNgay
                    ? $"Lịch bù {b.NgayVangGoc:dd/MM} (chưa học)"
                    : "Bù cho " + (b.NgayVangGoc.HasValue ? $"buổi {b.NgayVangGoc:dd/MM}" : "buổi vắng");
            if (b.MaBuoiBu.HasValue)
                return $"Bù {b.NgayBu:dd/MM}, {Constants.CaHoc.TenNgan(b.CaBu)}";
            if (b.Vang) return "Chưa bù";
            return "";
        }

        private DongBuoi DongDangChon =>
            dgvBuoi.CurrentCell != null && dgvBuoi.CurrentCell.RowIndex >= 0 && dgvBuoi.CurrentCell.RowIndex < _buoi.Count
                ? _buoi[dgvBuoi.CurrentCell.RowIndex] : null;

        private void CapNhatNutBuoi()
        {
            var k = LopDangXem;
            var d = DongDangChon;
            bool coSua = _buoi.Any(x => x.Sua);
            btnLuuBuoi.Enabled = coSua;
            // Xếp bù: buổi vắng đã lưu, chưa có lịch bù, không sửa dở; gia sư chỉ xếp cho lớp mình
            btnXepBu.Enabled = k != null && d != null && d.Goc != null && !d.Sua && d.Goc.CoTheXepBu &&
                               (LaQuanLy || k.MaGS == _gioiHanGS);
            btnHuyBu.Enabled = k != null && d != null && d.Goc != null && !d.Sua &&
                               (d.Goc.LaHocBu || d.Goc.MaBuoiBu.HasValue) && DuocSua(k, d.Goc);
        }

        /// <summary>Thêm một buổi thường (ngày đang chọn) vào lưới; bấm Lưu mới ghi vào CSDL</summary>
        private void ThemBuoi()
        {
            var k = LopDangXem;
            if (k == null) return;
            var ngay = dtpThemBuoi.Value.Date;
            if (k.BatDau.HasValue && ngay < k.BatDau.Value.Date || k.KetThuc.HasValue && ngay > k.KetThuc.Value.Date)
            {
                MessageBox.Show($"Lớp học từ {k.BatDau:dd/MM/yyyy} đến {k.KetThuc:dd/MM/yyyy}.", Constants.Messages.WarningTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            while (_cuonBuoi.ConLai > 0) _cuonBuoi.TaiThem();   // cần đủ danh sách để kiểm tra trùng ngày + chèn đúng chỗ
            var daCo = _buoi.FirstOrDefault(x => x.Goc != null && !x.Goc.LaHocBu && x.NgayHoc.Date == ngay || x.Goc == null && x.NgayHoc.Date == ngay);
            if (daCo != null)
            {
                dgvBuoi.CurrentCell = dgvBuoi.Rows[_buoi.IndexOf(daCo)].Cells["DiemDanhChon"];
                return;
            }

            var moi = new DongBuoi
            {
                MaLop = k.MaLop, NgayHoc = ngay, Ca = Constants.CaHoc.TenNgan(k.CaHoc), GiaSu = k.GiaSu,
                DiemDanh = Constants.DiemDanh.CoMat, GhiChu = "Buổi mới", Sua = true
            };
            // Chèn đúng vị trí theo ngày (mới nhất trước)
            int viTri = 0;
            while (viTri < _buoi.Count && _buoi[viTri].NgayHoc.Date > ngay) viTri++;
            _buoi.Insert(viTri, moi);
            dgvBuoi.CurrentCell = dgvBuoi.Rows[viTri].Cells["DiemDanhChon"];
            CapNhatNutBuoi();
        }

        private bool LuuBuoiHoc(bool baoThanhCong = true)
        {
            dgvBuoi.EndEdit();
            var k = LopDangXem;
            var dong = _buoi.Where(x => x.Sua).ToList();
            if (k == null || dong.Count == 0) return true;

            var rows = new List<BuoiHocRow>();
            foreach (var d in dong)
            {
                decimal? diem;
                if (!HocTapService.TryParseDiem(d.DiemText, out diem))
                {
                    MessageBox.Show($"Điểm ngày {d.NgayHoc:dd/MM} không hợp lệ.", Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                rows.Add(new BuoiHocRow
                {
                    MaBuoi = d.MaBuoi, MaLop = d.MaLop, NgayHoc = d.NgayHoc, DiemDanh = d.DiemDanh, Diem = diem, NhanXet = d.NhanXet,
                    ThiLenCap = k.LaNangKhieu && d.ThiLenCap, CapDoThi = d.Goc?.CapDoThi
                });
            }

            try
            {
                var loi = _hocTap.LuuCacBuoi(rows, _gioiHanGS);
                HienBuoiHoc();
                if (loi.Count > 0)
                {
                    MessageBox.Show("Một số buổi chưa lưu:\n\n" + string.Join("\n", loi), Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (baoThanhCong)
                    ThongBao.ThanhCong($"Đã lưu {rows.Count} buổi.");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không lưu được: {ex.GetBaseException().Message}", Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>Còn buổi sửa dở chưa lưu => hỏi lưu trước khi đổi học viên / lớp. Luôn cho đi tiếp.</summary>
        private bool HoiLuuBuoiHoc()
        {
            if (_buoiSua || _buoi == null || !_buoi.Any(x => x.Sua)) return true;
            _buoiSua = true;
            try
            {
                if (MessageBox.Show("Có buổi học đã sửa nhưng chưa lưu. Lưu ngay?", Constants.Messages.ConfirmTitle,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    LuuBuoiHoc(false);
                foreach (var d in _buoi) d.Sua = false;
            }
            finally { _buoiSua = false; }
            return true;
        }

        // ===================================================================
        // HỌC BÙ
        // ===================================================================

        private void XepHocBu()
        {
            var k = LopDangXem;
            var d = DongDangChon;
            if (k == null || d?.Goc == null || !d.Goc.CoTheXepBu) return;

            using (var dlg = new dlgHocBu(_hocTap, k, d.Goc, _gioiHanGS))
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            }
            HienBuoiHoc();
        }

        private void HuyHocBu()
        {
            var d = DongDangChon;
            if (d?.Goc == null) return;
            int maBu = d.Goc.LaHocBu ? d.Goc.MaBuoi : d.Goc.MaBuoiBu ?? 0;
            if (maBu == 0) return;
            DateTime ngayBu = d.Goc.LaHocBu ? d.Goc.NgayHoc : d.Goc.NgayBu ?? d.NgayHoc;
            if (MessageBox.Show($"Hủy buổi học bù ngày {ngayBu:dd/MM/yyyy}? Điểm danh và điểm của buổi bù sẽ bị xóa, buổi vắng giữ nguyên.",
                    Constants.Messages.ConfirmTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            try
            {
                _hocTap.HuyHocBu(maBu, _gioiHanGS);
                HienBuoiHoc();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
