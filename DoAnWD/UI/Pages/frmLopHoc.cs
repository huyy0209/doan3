using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Dto;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.UI.Controls;
using DoAnWD.UI.Dialogs;
using DoAnWD.UI;

namespace DoAnWD.UI.Pages
{
    /// <summary>
    /// Trang Lớp học (thay trang "Ghép lớp" cũ):
    ///  - Danh sách mọi lớp: tìm theo tên lớp / học viên / gia sư / môn, lọc trạng thái, ca, môn;
    ///    cuộn tới đâu tải tới đó.
    ///  - Chi tiết lớp đang chọn: đổi tên lớp, đánh giá (xếp loại hoặc cấp độ), hủy / xóa lớp,
    ///    mở trang học viên của lớp.
    ///  - "Ghép lớp mới" và "Môn học" (danh mục môn, chỉ quản lý).
    /// Dữ liệu đọc một lần mỗi lần nạp (3 truy vấn), lọc / tìm kiếm làm trong bộ nhớ.
    /// </summary>
    public class frmLopHoc : Form, ITuGiaoDien, ILamMoi
    {
        public const string ConHoatDong = "Còn hoạt động";
        private const string TatCa = "Tất cả";

        private List<LopHocDong> _tatCa = new List<LopHocDong>();
        private int? _maDangChon;

        private TextBox txtTim;
        private ComboBox cboTrangThai, cboCa, cboMon;
        private DataGridView dgv;
        private CuonTaiDan<LopHocDong> _cuon;
        private Label lblDem;

        private Label lblTenHienTai, lblThongTin, lblDanhGia;
        private TextBox txtTenLop;
        private ThemedButton btnDoiTen, btnMoHocVien, btnHuy, btnXoa;

        public frmLopHoc()
        {
            BackColor = Theme.Paper;
            Font = Theme.Body;
            FormBorderStyle = FormBorderStyle.None;
            Padding = new Padding(20, 14, 20, 16);
            Text = "Lớp học";
            BuildUi();
            Load += (s, e) => NapDuLieu();
        }

        public void LamMoi() => NapDuLieu();

        /// <summary>Lọc theo trạng thái (dùng khi bấm ô ở trang Tổng quan)</summary>
        public void LocTrangThai(string trangThai)
        {
            txtTim.Clear();
            cboCa.SelectedIndex = cboMon.SelectedIndex = 0;
            cboTrangThai.SelectedItem = trangThai;
            if (cboTrangThai.SelectedIndex < 0) cboTrangThai.SelectedIndex = 0;
            ApDungLoc();
        }

        // ===================================================================
        // GIAO DIỆN
        // ===================================================================

        private void BuildUi()
        {
            // ---- Tiêu đề + nút ----
            var btnMon = Ui.Button("Môn học", ButtonKind.Ghost, Glyph.Clipboard);
            btnMon.Click += (s, e) => MoDanhMucMon();
            btnMon.Visible = !PhienDangNhap.DaDangNhap || PhienDangNhap.LaQuanLy;
            var btnGhep = Ui.Button("Ghép lớp mới", ButtonKind.Primary, Glyph.Plus, 170);
            btnGhep.Click += (s, e) => GhepLopMoi();

            var tieuDe = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true };
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tieuDe.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tieuDe.Controls.Add(Ui.Stack(false, Ui.Title("Lớp học"),
                Ui.Text("Tìm lớp theo tên, học viên, gia sư hoặc môn. Chọn một lớp để xem đánh giá, đổi tên, hủy lớp.", true)), 0, 0);
            tieuDe.Controls.Add(btnMon, 1, 0);
            tieuDe.Controls.Add(btnGhep, 2, 0);

            // ---- Bộ lọc ----
            txtTim = Ui.Input("Tìm tên lớp, học viên, gia sư, môn... (không cần dấu)");
            txtTim.TextChanged += (s, e) => ApDungLoc();
            cboTrangThai = Ui.Combo(false, 150);
            cboTrangThai.Items.AddRange(new object[]
            {
                ConHoatDong, TatCa, Constants.LopHocStatus.ChuaBatDau, Constants.LopHocStatus.DangDienRa,
                Constants.LopHocStatus.DaKetThuc, Constants.LopHocStatus.DaHuy
            });
            cboTrangThai.SelectedIndex = 0;
            cboCa = Ui.Combo(false, 130);
            cboCa.Items.Add("Mọi ca");
            cboCa.Items.AddRange(Constants.CaHoc.TatCaCaHoc.Select(c => (object)Constants.CaHoc.TenNgan(c)).ToArray());
            cboCa.SelectedIndex = 0;
            cboMon = Ui.Combo(false, 140);
            cboMon.Items.Add("Mọi môn");
            cboMon.SelectedIndex = 0;
            foreach (var c in new[] { cboTrangThai, cboCa, cboMon }) c.SelectedIndexChanged += (s, e) => ApDungLoc();
            Ui.DanhDauBoLoc(cboTrangThai, cboCa, cboMon);

            var loc = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true, Margin = new Padding(0) };
            loc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 3; i++) loc.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtTim.Margin = cboTrangThai.Margin = cboCa.Margin = new Padding(0, 2, 8, 4);
            cboMon.Margin = new Padding(0, 2, 0, 4);
            loc.Controls.Add(txtTim, 0, 0);
            loc.Controls.Add(cboTrangThai, 1, 0);
            loc.Controls.Add(cboCa, 2, 0);
            loc.Controls.Add(cboMon, 3, 0);

            // ---- Danh sách ----
            dgv = Ui.Grid();
            dgv.SelectionChanged += (s, e) => HienChiTiet();
            ThemeManager.TieuDeCot(dgv, "MaLop", null, "MaHV", null, "MaGS", null, "LoaiMon", null, "HocPhi", null,
                "DiemTB", null, "TienTrinh", null, "MonHoc", null,
                // Tên lớp đã có môn, học viên, ca => ẩn 3 cột đó cho bảng thoáng (vẫn tìm / lọc được)
                "HocVien", null, "CaHoc", null,
                "TenLop", "Tên lớp", "GiaSu", "Gia sư",
                "BatDau", "Bắt đầu", "KetThuc", "Kết thúc", "TrangThai", "Trạng thái", "DanhGia", "Đánh giá");
            dgv.CellFormatting += (s, e) =>
            {
                string col = dgv.Columns[e.ColumnIndex].DataPropertyName;
                if ((col == "BatDau" || col == "KetThuc") && e.Value is DateTime) { e.Value = ((DateTime)e.Value).ToString("dd/MM/yy"); e.FormattingApplied = true; }
                if (col == "CaHoc" && e.Value is string) { e.Value = Constants.CaHoc.TenNgan((string)e.Value); e.FormattingApplied = true; }
                if (col == "DanhGia") e.CellStyle.Font = Theme.BodyStrong;
            };
            _cuon = new CuonTaiDan<LopHocDong>(dgv, 40);
            _cuon.DaTaiThem += (s, e) => CapNhatDem();
            lblDem = Ui.Text("", true);

            var secDs = Ui.Section("Danh sách lớp");
            var stDs = Ui.Stack(false, loc, dgv, lblDem);
            stDs.RowStyles[1] = new RowStyle(SizeType.Percent, 100);
            dgv.Dock = DockStyle.Fill;
            secDs.Controls.Add(stDs);

            // ---- Chi tiết ----
            lblTenHienTai = new Label { Font = Theme.Section, ForeColor = Theme.Ink, AutoSize = true, MaximumSize = new Size(440, 0), Margin = new Padding(0, 0, 0, 6) };
            txtTenLop = Ui.Input("Tên lớp");
            txtTenLop.MaxLength = 150;
            btnDoiTen = Ui.Button("Đổi tên", ButtonKind.Secondary, Glyph.Edit, 110);
            btnDoiTen.Click += (s, e) => DoiTen();
            var hangTen = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Margin = new Padding(0) };
            hangTen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            hangTen.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtTenLop.Dock = DockStyle.Fill;
            txtTenLop.Margin = new Padding(0, 4, 8, 0);
            hangTen.Controls.Add(txtTenLop, 0, 0);
            hangTen.Controls.Add(btnDoiTen, 1, 0);

            lblThongTin = new Label { AutoSize = true, Font = Theme.Body, ForeColor = Theme.Text, MaximumSize = new Size(440, 0), Margin = new Padding(0, 10, 0, 6) };
            lblDanhGia = new Label { AutoSize = true, Font = Theme.Body, ForeColor = Theme.Ink, MaximumSize = new Size(440, 0), Margin = new Padding(0, 6, 0, 10) };

            btnMoHocVien = Ui.Button("Mở trang học viên", ButtonKind.Ghost, Glyph.Users);
            btnMoHocVien.Click += (s, e) => MoHocVien();
            btnHuy = Ui.Button("Hủy lớp", ButtonKind.Secondary, Glyph.Close, 110);
            btnHuy.Click += (s, e) => HuyLop();
            btnXoa = Ui.Button("Xóa", ButtonKind.Danger, Glyph.Trash, 90);
            btnXoa.Click += (s, e) => XoaLop();

            var secCT = Ui.Section("Lớp đang chọn");
            secCT.Margin = new Padding(0);
            var stCT = Ui.Stack(false, lblTenHienTai, Ui.FieldLabel("Tên lớp"), hangTen, lblThongTin,
                Ui.FieldLabel("Đánh giá"), lblDanhGia, Ui.Row(btnMoHocVien), Ui.Row(btnHuy, btnXoa));
            stCT.AutoScroll = true;
            secCT.Controls.Add(stCT);

            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = new Padding(0, 10, 0, 0) };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            cot.Controls.Add(secDs, 0, 0);
            cot.Controls.Add(secCT, 1, 0);

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

        private void NapDuLieu(int? chonMaLop = null)
        {
            try
            {
                using (var service = new LopHocService()) _tatCa = service.GetDanhSachLop();

                string monChon = cboMon.SelectedIndex > 0 ? cboMon.SelectedItem as string : null;
                cboMon.BeginUpdate();
                cboMon.Items.Clear();
                cboMon.Items.Add("Mọi môn");
                cboMon.Items.AddRange(_tatCa.Select(l => l.MonHoc).Where(m => !string.IsNullOrWhiteSpace(m))
                    .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m => m).Cast<object>().ToArray());
                cboMon.SelectedItem = monChon;
                if (cboMon.SelectedIndex < 0) cboMon.SelectedIndex = 0;
                cboMon.EndUpdate();

                ApDungLoc(chonMaLop);
            }
            catch (Exception ex)
            {
                lblDem.Text = "Không tải được danh sách lớp: " + ex.GetBaseException().Message;
            }
        }

        private void ApDungLoc(int? chonMaLop = null)
        {
            string tt = cboTrangThai.SelectedItem as string;
            int caIdx = cboCa.SelectedIndex - 1;
            string ca = caIdx >= 0 ? Constants.CaHoc.TatCaCaHoc[caIdx] : null;
            string mon = cboMon.SelectedIndex > 0 ? cboMon.SelectedItem as string : null;

            var ds = _tatCa
                .Where(l => tt == TatCa || (tt == ConHoatDong ? Constants.LopHocStatus.LaHoatDong(l.TrangThai) : l.TrangThai == tt))
                .Where(l => ca == null || l.CaHoc == ca)
                .Where(l => mon == null || string.Equals(l.MonHoc, mon, StringComparison.OrdinalIgnoreCase))
                .Where(l => TimKiem.Khop(txtTim.Text, l.TenLop, l.HocVien, l.GiaSu, l.MonHoc))
                .ToList();

            int? giu = chonMaLop ?? _maDangChon;
            _cuon.Nap(ds);
            if (giu.HasValue)
            {
                int i = _cuon.TaiDen(l => l.MaLop == giu.Value);
                if (i >= 0 && i < dgv.Rows.Count && dgv.Columns.Contains("TenLop"))
                    dgv.CurrentCell = dgv.Rows[i].Cells["TenLop"];
            }
            if (ds.Count == 0) HienChiTiet();
        }

        private void CapNhatDem()
        {
            lblDem.Text = _cuon.TongSo == 0
                ? "Không có lớp nào khớp bộ lọc."
                : $"{_cuon.TongSo} lớp" + (_cuon.ConLai > 0 ? $", đang hiện {_cuon.HienThi.Count} (cuộn xuống để xem thêm)" : "");
        }

        private LopHocDong LopChon =>
            dgv.CurrentRow != null && dgv.CurrentRow.Index < _cuon.HienThi.Count ? _cuon.HienThi[dgv.CurrentRow.Index] : null;

        private void HienChiTiet()
        {
            var l = dgv.SelectedRows.Count > 0 ? LopChon : null;
            bool co = l != null;
            txtTenLop.Enabled = btnDoiTen.Enabled = btnMoHocVien.Enabled = btnXoa.Enabled = co;
            btnHuy.Enabled = co && Constants.LopHocStatus.LaHoatDong(l.TrangThai);
            if (!co)
            {
                _maDangChon = null;
                lblTenHienTai.Text = "Chưa chọn lớp";
                txtTenLop.Clear();
                lblThongTin.Text = "Chọn một lớp trong danh sách.";
                lblDanhGia.Text = "";
                return;
            }

            _maDangChon = l.MaLop;
            lblTenHienTai.Text = l.TenLop;
            txtTenLop.Text = l.TenLop;
            lblThongTin.Text =
                $"Môn: {l.MonHoc} ({l.LoaiMon?.ToLower()})\n" +
                $"Học viên: {l.HocVien}\nGia sư: {l.GiaSu}\n" +
                $"Ca: {l.CaHoc}\nThời gian: {l.BatDau:dd/MM/yyyy} – {l.KetThuc:dd/MM/yyyy}\n" +
                $"Học phí: {l.HocPhi:N0} đ\nTrạng thái: {l.TrangThai}";

            if (l.TienTrinh != null)
            {
                var t = l.TienTrinh;
                lblDanhGia.Text = $"Cấp hiện tại: {t.CapHienTai}" +
                                  (t.CapDauVao != null && t.CapDauVao != t.CapHienTai ? $" (vào lớp: {t.CapDauVao})" : "") +
                                  $"\nCấp kế tiếp: {t.CapKeTiep ?? "đã ở cấp cao nhất"}" +
                                  (t.BaiThi.Count == 0 ? "\nChưa có bài thi lên cấp." : "\n" + string.Join("\n", t.BaiThi.Select(b => b.ToString())));
            }
            else
                lblDanhGia.Text = l.DiemTB.HasValue ? $"Điểm TB {l.DiemTB:0.##} · {l.DanhGia}" : "Chưa có bài test.";
        }

        // ===================================================================
        // THAO TÁC
        // ===================================================================

        private void GhepLopMoi()
        {
            using (var dlg = new frmChucNangGhepLop())
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                cboTrangThai.SelectedItem = ConHoatDong;
                txtTim.Clear();
                NapDuLieu(dlg.MaLopVuaGhep);
            }
        }

        private void MoDanhMucMon()
        {
            using (var dlg = new dlgMonHoc()) dlg.ShowDialog(FindForm());
            NapDuLieu();   // đổi tên môn / loại môn ảnh hưởng tên môn và đánh giá
        }

        private void DoiTen()
        {
            var l = LopChon;
            if (l == null) return;
            try
            {
                using (var service = new LopHocService()) service.DoiTenLop(l.MaLop, txtTenLop.Text);
                NapDuLieu(l.MaLop);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MoHocVien()
        {
            var l = LopChon;
            if (l?.MaHV == null) return;
            DieuHuong.Mo<frmHocVien>()?.ChonHocVien(l.MaHV.Value, l.MaLop);
        }

        private void HuyLop()
        {
            var l = LopChon;
            if (l == null) return;
            if (MessageBox.Show($"Hủy lớp \"{l.TenLop}\"?\n\nLớp được giữ lại trong lịch sử, lịch của gia sư và học viên được giải phóng.",
                    Constants.Messages.ConfirmTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            ThucHien(s => s.HuyLop(l.MaLop), "Đã hủy lớp.", l.MaLop);
        }

        private void XoaLop()
        {
            var l = LopChon;
            if (l == null) return;
            if (MessageBox.Show($"Xóa hẳn lớp \"{l.TenLop}\"? Thao tác này không hoàn tác được.\n\n" +
                                "Lớp đã có điểm danh hoặc học phí thì không xóa được, hãy dùng Hủy lớp.",
                    Constants.Messages.ConfirmTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            _maDangChon = null;
            ThucHien(s => s.Delete(l.MaLop), Constants.Messages.XoaThanhCong, null);
        }

        private void ThucHien(Action<LopHocService> viec, string thongBao, int? chonLai)
        {
            try
            {
                using (var service = new LopHocService()) viec(service);
                NapDuLieu(chonLai);
                MessageBox.Show(thongBao, Constants.Messages.SuccessTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().Message, Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
