using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Dto;
using DoAnWD.BLL.Services;
using DoAnWD.Common;

namespace DoAnWD.UI
{
    /// <summary>
    /// Xếp lịch học bù cho một buổi vắng: chọn ngày, ca, gia sư dạy bù.
    /// Danh sách gia sư cập nhật theo ngày + ca, người đang dạy ca đó có ghi "bận".
    /// Học viên trùng ca => không cho xếp; gia sư bận => hỏi có cho học chung không.
    /// </summary>
    public class dlgHocBu : Form, ITuGiaoDien
    {
        private readonly HocTapService _service;
        private readonly KetQuaLop _lop;
        private readonly BuoiHocRow _buoiVang;
        private readonly int? _gioiHanGS;

        private readonly DateTimePicker dtpNgay;
        private readonly ComboBox cboCa, cboGS;
        private readonly Label lblBan;

        public dlgHocBu(HocTapService service, KetQuaLop lop, BuoiHocRow buoiVang, int? gioiHanGS)
        {
            _service = service;
            _lop = lop;
            _buoiVang = buoiVang;
            _gioiHanGS = gioiHanGS;

            Text = "Xếp học bù";
            Font = Theme.Body;
            BackColor = Theme.Surface;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;
            Padding = new Padding(22, 16, 22, 16);

            var lblTieuDe = new Label { Text = "Xếp học bù", Font = Theme.Section, ForeColor = Theme.Ink, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            var lblMoTa = Ui.Text($"Bù cho buổi vắng {buoiVang.NgayHoc:dd/MM/yyyy} ({buoiVang.DiemDanh.ToLower()}), " +
                                  $"lớp {lop.MonHoc} {Constants.CaHoc.TenNgan(lop.CaHoc)} với {lop.GiaSu}.", true);
            lblMoTa.MaximumSize = new Size(470, 0);
            lblMoTa.Margin = new Padding(0, 0, 0, 10);

            dtpNgay = Ui.Date(160);
            dtpNgay.MaxDate = DateTime.Today.AddDays(60);
            dtpNgay.Value = DateTime.Today;

            cboCa = Ui.Combo(false, 230);
            cboCa.Items.AddRange(Constants.CaHoc.TatCaCaHoc);
            // Mặc định: ca khác ca của lớp (cùng ca thì học viên đang học lớp mình rồi)
            cboCa.SelectedItem = Constants.CaHoc.TatCaCaHoc.FirstOrDefault(c => c != lop.CaHoc);

            cboGS = Ui.Combo(false, 470);
            cboGS.DropDownWidth = 560;
            lblBan = Ui.Text("", true);
            lblBan.MaximumSize = new Size(470, 0);

            dtpNgay.ValueChanged += (s, e) => NapGiaSu();
            cboCa.SelectedIndexChanged += (s, e) => NapGiaSu();
            cboGS.SelectedIndexChanged += (s, e) => HienBan();

            var hang = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Margin = new Padding(0) };
            hang.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            hang.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            var cNgay = Ui.Stack(false, Ui.FieldLabel("Ngày học bù"), dtpNgay);
            var cCa = Ui.Stack(false, Ui.FieldLabel("Ca"), cboCa);
            cNgay.AutoSize = cCa.AutoSize = true;
            cNgay.Margin = new Padding(0, 0, 16, 0);
            hang.Controls.Add(cNgay, 0, 0);
            hang.Controls.Add(cCa, 1, 0);

            var btnLuu = Ui.Button("Xếp lịch", ButtonKind.Primary, Glyph.Check, 120);
            var btnHuy = Ui.Button("Hủy", ButtonKind.Ghost, Glyph.None, 90);
            btnLuu.Click += (s, e) => Luu();
            btnHuy.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            AcceptButton = btnLuu;
            CancelButton = btnHuy;
            var nut = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 14, 0, 0), WrapContents = false };
            nut.Controls.Add(btnLuu);
            nut.Controls.Add(btnHuy);

            const int rong = 480;
            var khung = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Location = new Point(Padding.Left, Padding.Top) };
            khung.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, rong));
            int r = 0;
            foreach (var c in new Control[] { lblTieuDe, lblMoTa, hang, Ui.FieldLabel("Gia sư dạy bù"), cboGS, lblBan, nut })
                khung.Controls.Add(c, 0, r++);
            Controls.Add(khung);

            ClientSize = new Size(rong + Padding.Horizontal, 380);
            Load += (s, e) =>
            {
                NapGiaSu();
                khung.PerformLayout();
                var co = khung.GetPreferredSize(new Size(rong, 0));
                ClientSize = new Size(rong + Padding.Horizontal, co.Height + Padding.Vertical);
            };
        }

        /// <summary>Danh sách gia sư theo ngày + ca đang chọn; giữ người đang chọn, mặc định gia sư của lớp</summary>
        private void NapGiaSu()
        {
            int? dangChon = (cboGS.SelectedItem as GiaSuHocBu)?.MaGS ?? _lop.MaGS;
            string ca = cboCa.SelectedItem as string;
            try
            {
                var ds = _service.GetGiaSuChoHocBu(dtpNgay.Value, ca)
                    // Rảnh trước, cùng môn trước
                    .OrderBy(g => g.Ban != null)
                    .ThenBy(g => !string.Equals(g.ChuyenMon, _lop.MonHoc, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(g => g.HoTen)
                    .ToList();
                cboGS.BeginUpdate();
                cboGS.Items.Clear();
                cboGS.Items.AddRange(ds.Cast<object>().ToArray());
                cboGS.SelectedItem = ds.FirstOrDefault(g => g.MaGS == dangChon) ?? ds.FirstOrDefault();
                cboGS.EndUpdate();
                HienBan();
            }
            catch (Exception ex)
            {
                lblBan.Text = "Không tải được danh sách gia sư: " + ex.GetBaseException().Message;
            }
        }

        private void HienBan()
        {
            var g = cboGS.SelectedItem as GiaSuHocBu;
            if (g == null) { lblBan.Text = ""; return; }
            lblBan.ForeColor = g.Ban == null ? Theme.Success : Theme.Warning;
            lblBan.Text = g.Ban == null
                ? "Gia sư rảnh ở ca này."
                : $"Gia sư đang có {g.Ban} ở ca này. Vẫn xếp được nếu cho học chung.";
        }

        private void Luu()
        {
            var g = cboGS.SelectedItem as GiaSuHocBu;
            string ca = cboCa.SelectedItem as string;
            if (g == null || ca == null)
            {
                MessageBox.Show("Chọn ca và gia sư dạy bù.", Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var kq = _service.XepHocBu(_buoiVang.MaBuoi, dtpNgay.Value, ca, g.MaGS, false, _gioiHanGS);
                if (kq.CanhBao != null)
                {
                    if (MessageBox.Show(kq.CanhBao, Constants.Messages.ConfirmTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    kq = _service.XepHocBu(_buoiVang.MaBuoi, dtpNgay.Value, ca, g.MaGS, true, _gioiHanGS);
                }
                if (!kq.ThanhCong)
                {
                    MessageBox.Show(kq.Loi ?? "Không xếp được lịch học bù.", Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không lưu được: {ex.GetBaseException().Message}", Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
