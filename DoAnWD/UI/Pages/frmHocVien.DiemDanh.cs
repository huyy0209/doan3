using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Tab "Điểm danh theo ca": chọn ngày + ca => mọi học viên học ca đó (lớp thường + học viên đến học bù),
    /// đánh dấu có mặt / vắng, gõ điểm test (bỏ trống nếu không kiểm tra), nhận xét => Lưu.
    /// Bấm đúp tên học viên để mở trang của học viên đó.
    /// </summary>
    public partial class frmHocVien
    {
        private DateTimePicker dtpNgay;
        private ComboBox cboCa;
        private DataGridView dgvDiemDanh;
        private Label lblDemDiemDanh;
        private BindingList<DongNhap> _dongs = new BindingList<DongNhap>();

        private class DongNhap
        {
            public int MaLop { get; set; }
            [Browsable(false)] public int? MaBuoi { get; set; }
            [Browsable(false)] public int MaHV { get; set; }
            [Browsable(false)] public bool LaHocBu { get; set; }
            [Browsable(false)] public bool LaNangKhieu { get; set; }
            [Browsable(false)] public string CapDoThi { get; set; }
            public string TenLop { get; set; }
            public string HocVien { get; set; }
            public string MonHoc { get; set; }
            public string GiaSu { get; set; }
            public string CaHoc { get; set; }
            public string DiemDanh { get; set; }
            public string DiemText { get; set; }
            public string NhanXet { get; set; }
            public bool ThiLenCap { get; set; }
            public string GhiChu { get; set; }
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
            cboCa.SelectedItem = ShiftRibbon.CaHienTai(DateTime.Now);
            if (cboCa.SelectedIndex < 0) cboCa.SelectedIndex = 0;
            cboCa.SelectedIndexChanged += (s, e) => NapDiemDanh();

            var btnHomNay = Ui.Button("Hôm nay", ButtonKind.Ghost, Glyph.Calendar);
            btnHomNay.Click += (s, e) => dtpNgay.Value = DateTime.Now.Date;
            var btnCoMat = Ui.Button("Tất cả có mặt", ButtonKind.Secondary, Glyph.Check);
            btnCoMat.Click += (s, e) =>
            {
                // Bỏ qua buổi đã xếp bù (vắng có lịch bù thì giữ nguyên)
                foreach (var d in _dongs.Where(x => !(x.GhiChu ?? "").StartsWith("Đã xếp bù"))) d.DiemDanh = Constants.DiemDanh.CoMat;
                dgvDiemDanh.Refresh();
            };
            var btnLuu = Ui.Button("Lưu điểm danh", ButtonKind.Primary, Glyph.Check);
            btnLuu.Click += (s, e) => LuuDiemDanh();

            var nhanNgay = Ui.FieldLabel("Ngày");
            var nhanCa = Ui.FieldLabel("Ca");
            nhanNgay.Margin = nhanCa.Margin = new System.Windows.Forms.Padding(0, 10, 6, 0);
            dtpNgay.Margin = new System.Windows.Forms.Padding(0, 6, 16, 0);
            cboCa.Margin = new System.Windows.Forms.Padding(0, 6, 8, 0);
            var thanh = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = false, Margin = new System.Windows.Forms.Padding(0) };
            thanh.Controls.AddRange(new Control[] { nhanNgay, dtpNgay, nhanCa, cboCa, btnHomNay });

            dgvDiemDanh = Ui.Grid();
            dgvDiemDanh.ReadOnly = false;
            dgvDiemDanh.AutoGenerateColumns = false;
            dgvDiemDanh.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvDiemDanh.SelectionMode = DataGridViewSelectionMode.CellSelect;
            Func<string, string, int, bool, DataGridViewTextBoxColumn> cot = (prop, header, w, ro) =>
                new DataGridViewTextBoxColumn { DataPropertyName = prop, Name = prop, HeaderText = header, ReadOnly = ro, FillWeight = w };
            dgvDiemDanh.Columns.Add(cot("HocVien", "Học viên", 150, true));
            dgvDiemDanh.Columns.Add(cot("TenLop", "Lớp", 170, true));
            var cGS = cot("GiaSu", "Gia sư", 130, true);
            cGS.Visible = LaQuanLy;
            dgvDiemDanh.Columns.Add(cGS);
            dgvDiemDanh.Columns.Add(cot("CaHoc", "Ca", 130, true));
            var cDD = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "DiemDanh", Name = "DiemDanhChon", HeaderText = "Điểm danh", FillWeight = 125,
                FlatStyle = FlatStyle.Flat, DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
            cDD.Items.AddRange(Constants.DiemDanh.TatCa);
            cDD.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            dgvDiemDanh.Columns.Add(cDD);
            var cDiem = cot("DiemText", "Điểm test (0–10)", 85, false);
            cDiem.DefaultCellStyle.Font = Theme.BodyStrong;
            cDiem.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(11, 0, 11, 0);
            dgvDiemDanh.Columns.Add(cDiem);
            var cNX = cot("NhanXet", "Nhận xét", 190, false);
            cNX.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(11, 0, 11, 0);
            dgvDiemDanh.Columns.Add(cNX);
            // Bài thi lên cấp: chỉ bật với lớp năng khiếu
            dgvDiemDanh.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "ThiLenCap", Name = "ThiLenCap", HeaderText = "Thi lên cấp", FillWeight = 70
            });
            dgvDiemDanh.Columns.Add(cot("GhiChu", "Ghi chú", 120, true));
            dgvDiemDanh.DataBindingComplete += (s, e) =>
            {
                for (int i = 0; i < dgvDiemDanh.Rows.Count && i < _dongs.Count; i++)
                    dgvDiemDanh.Rows[i].Cells["ThiLenCap"].ReadOnly = !_dongs[i].LaNangKhieu;
            };
            dgvDiemDanh.DataSource = _dongs;
            dgvDiemDanh.CellValidating += (s, e) => KiemTraOdiem(dgvDiemDanh, e);
            dgvDiemDanh.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _dongs.Count) return;
                var d = _dongs[e.RowIndex];
                var col = dgvDiemDanh.Columns[e.ColumnIndex].Name;
                if (d.LaHocBu) e.CellStyle.BackColor = Theme.InkSoft;   // học viên đến học bù: nền tím nhạt
                if (col == "GhiChu")
                {
                    e.CellStyle.Font = Theme.SmallStrong;
                    e.CellStyle.ForeColor = d.LaHocBu || (d.GhiChu ?? "").StartsWith("Đã xếp bù") ? Theme.Ink : Theme.Success;
                }
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
                if (dgvDiemDanh.IsCurrentCellDirty && (dgvDiemDanh.CurrentCell is DataGridViewComboBoxCell || dgvDiemDanh.CurrentCell is DataGridViewCheckBoxCell))
                    dgvDiemDanh.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvDiemDanh.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _dongs.Count && dgvDiemDanh.Columns[e.ColumnIndex].Name == "HocVien")
                    MoHocVien(_dongs[e.RowIndex].MaHV);
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

            var sec = Ui.Section("Học viên trong ca (gồm cả học viên đến học bù)");
            sec.Margin = new System.Windows.Forms.Padding(0);
            var st = Ui.Stack(false, thanh, dgvDiemDanh, chan);
            st.RowStyles[1] = new RowStyle(SizeType.Percent, 100);
            dgvDiemDanh.Dock = DockStyle.Fill;
            sec.Controls.Add(st);
            return sec;
        }

        private void NapDiemDanh()
        {
            if (dtpNgay == null) return;
            try
            {
                string ca = cboCa.SelectedIndex > 0 ? cboCa.SelectedItem as string : null;
                var rows = _hocTap.GetLopTrongNgay(dtpNgay.Value, ca, _gioiHanGS);
                _dongs = new BindingList<DongNhap>(rows.Select(r => new DongNhap
                {
                    MaLop = r.MaLop, MaBuoi = r.MaBuoi, MaHV = r.MaHV, LaHocBu = r.LaHocBu,
                    TenLop = r.TenLop, LaNangKhieu = r.LaNangKhieu, ThiLenCap = r.ThiLenCap, CapDoThi = r.CapDoThi,
                    HocVien = r.HocVien, MonHoc = r.MonHoc, GiaSu = r.GiaSu, CaHoc = r.CaHoc,
                    DiemDanh = r.DiemDanh, DiemText = r.Diem?.ToString("0.##"), NhanXet = r.NhanXet,
                    GhiChu = r.GhiChu ?? (r.ThiLenCap ? $"Thi lên {r.CapDoThi}" : r.DaLuu ? "Đã lưu" : "")
                }).ToList());
                dgvDiemDanh.DataSource = _dongs;
                int daLuu = rows.Count(r => r.DaLuu && !r.LaHocBu);
                int bu = rows.Count(r => r.LaHocBu);
                lblDemDiemDanh.Text = rows.Count == 0
                    ? "Không có học viên nào học vào ngày / ca này."
                    : $"{rows.Count - bu} lớp, đã lưu {daLuu}" + (bu > 0 ? $"; {bu} học viên học bù" : "") +
                      ". Bỏ trống ô điểm nếu không kiểm tra. Bấm đúp tên để mở trang học viên.";
            }
            catch (Exception ex)
            {
                lblDemDiemDanh.Text = "Không tải được: " + ex.GetBaseException().Message;
            }
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
                rows.Add(new DiemDanhRow
                {
                    MaLop = d.MaLop, MaBuoi = d.MaBuoi, DiemDanh = d.DiemDanh, Diem = diem, NhanXet = d.NhanXet,
                    ThiLenCap = d.LaNangKhieu && d.ThiLenCap, CapDoThi = d.CapDoThi
                });
            }

            try
            {
                var loi = _hocTap.LuuNgay(dtpNgay.Value, rows, _gioiHanGS);
                NapDiemDanh();
                if (loi.Count == 0)
                    MessageBox.Show($"Đã lưu điểm danh ngày {dtpNgay.Value:dd/MM/yyyy}.", Constants.Messages.SuccessTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Một số dòng chưa lưu:\n\n" + string.Join("\n", loi), Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không lưu được: {ex.GetBaseException().Message}", Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Mở tab học viên, chọn học viên (và lớp nếu có) - dùng khi trang khác chuyển tới</summary>
        public void ChonHocVien(int maHV, int? maLop = null)
        {
            if (!_hocViens.Any(h => h.MaHV == maHV)) NapDuLieu();
            MoHocVien(maHV);
            if (maLop.HasValue)
            {
                var k = _ketQua.FirstOrDefault(x => x.MaLop == maLop.Value);
                if (k != null) cboLopHV.SelectedItem = k;
            }
        }

        /// <summary>Lọc danh sách theo tình trạng ("Đang học", "Chưa có lớp", "Đã nghỉ")</summary>
        public void LocTinhTrang(string tinhTrang)
        {
            tabsTrang.SelectedIndex = 0;
            txtTim.Clear();
            cboTrinhDoLoc.SelectedIndex = 0;
            cboTinhTrang.SelectedItem = tinhTrang;
            if (cboTinhTrang.SelectedIndex < 0) cboTinhTrang.SelectedIndex = 0;
        }

        /// <summary>Chuyển sang tab học viên và chọn học viên đó (tìm theo tên để chắc chắn thấy trong danh sách)</summary>
        private void MoHocVien(int maHV)
        {
            var hv = _hocViens.FirstOrDefault(h => h.MaHV == maHV);
            if (hv == null) return;
            tabsTrang.SelectedIndex = 0;
            tabsChiTiet.SelectedIndex = 0;
            cboTinhTrang.SelectedIndex = 0;
            cboTrinhDoLoc.SelectedIndex = 0;
            txtTim.Text = hv.HoTen;   // => ApDungLoc
            ApDungLoc(maHV);
            HienHoSoDangChon();
        }
    }
}
