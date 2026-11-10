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
using DoAnWD.UI.Controls;

namespace DoAnWD.UI.Dialogs
{
    /// <summary>
    /// Sửa một lớp đã ghép: tên lớp, học phí, ngày kết thúc (gia hạn / rút ngắn), thứ học, đổi gia sư.
    /// Ca học và ngày bắt đầu không đổi ở đây (đã gắn với điểm danh); muốn đổi ca thì hủy lớp rồi ghép lại.
    /// Luật kiểm tra nằm ở LopHocService.SuaLop; lỗi hiện ngay trong hộp thoại.
    /// </summary>
    public class dlgSuaLop : Form, ITuGiaoDien
    {
        private readonly LopHocDong _lop;
        private readonly TextBox txtTen, txtHocPhi;
        private readonly DateTimePicker dtpKetThuc;
        private readonly ChonThu chonThu;
        private readonly ComboBox cboGiaSu;
        private readonly Label lblSoBuoi, lblLoi;

        private class MucGiaSu
        {
            public int MaGS;
            public string Ten;
            public override string ToString() => Ten;
        }

        public dlgSuaLop(LopHocDong lop)
        {
            _lop = lop;
            Text = "Sửa lớp";
            Font = Theme.Body;
            BackColor = Theme.Surface;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(26, 20, 26, 20);
            const int rong = 480;

            txtTen = Ui.Input("Tên lớp");
            txtTen.MaxLength = 150;
            txtTen.Text = lop.TenLop;

            txtHocPhi = Ui.Input("Ví dụ: 1500000", 200);
            txtHocPhi.Text = lop.HocPhi.ToString("0");
            txtHocPhi.TextAlign = HorizontalAlignment.Right;

            dtpKetThuc = Ui.Date(200);
            if (lop.BatDau.HasValue) dtpKetThuc.MinDate = lop.BatDau.Value.Date;
            dtpKetThuc.Value = (lop.KetThuc ?? DateTime.Today).Date < dtpKetThuc.MinDate ? dtpKetThuc.MinDate : (lop.KetThuc ?? DateTime.Today).Date;
            dtpKetThuc.ValueChanged += (s, e) => CapNhatSoBuoi();

            chonThu = new ChonThu { GiaTri = lop.ThuHocGoc };
            chonThu.ThayDoi += (s, e) => CapNhatSoBuoi();
            lblSoBuoi = Ui.Text("", true);
            lblSoBuoi.Margin = new Padding(0, 2, 0, 0);

            cboGiaSu = Ui.Combo();
            using (var service = new GiaSuService())
            {
                var ds = service.GetForGhepLop().OrderBy(g => g.HoTen)
                    .Select(g => new MucGiaSu { MaGS = g.MaGS, Ten = string.IsNullOrWhiteSpace(g.ChuyenMon) ? g.HoTen : $"{g.HoTen} ({g.ChuyenMon})" }).ToList();
                // Gia sư hiện tại đang tạm nghỉ vẫn phải hiện để giữ nguyên được
                if (lop.MaGS.HasValue && ds.All(g => g.MaGS != lop.MaGS.Value))
                    ds.Insert(0, new MucGiaSu { MaGS = lop.MaGS.Value, Ten = lop.GiaSu + " (đang tạm nghỉ)" });
                cboGiaSu.Items.AddRange(ds.Cast<object>().ToArray());
                cboGiaSu.SelectedItem = ds.FirstOrDefault(g => g.MaGS == lop.MaGS);
            }

            lblLoi = new Label { ForeColor = Theme.Margin, Font = Theme.Body, AutoSize = true, MaximumSize = new Size(rong, 0), Margin = new Padding(0, 10, 0, 0), UseMnemonic = false };

            var btnLuu = Ui.Button("Lưu thay đổi", ButtonKind.Primary, Glyph.Check);
            var btnHuy = Ui.Button("Hủy", ButtonKind.Ghost, Glyph.None, 90);
            btnLuu.Click += (s, e) => Luu();
            btnHuy.Click += (s, e) => Close();
            AcceptButton = btnLuu;
            CancelButton = btnHuy;
            var nut = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Top, WrapContents = false, Margin = new Padding(0, 12, 0, 0) };
            nut.Controls.Add(btnLuu);
            nut.Controls.Add(btnHuy);

            var coDinh = Ui.Text($"Học viên {lop.HocVien}, môn {lop.MonHoc}, ca {Constants.CaHoc.TenNgan(lop.CaHoc)}, bắt đầu {lop.BatDau:dd/MM/yyyy}. " +
                                 "Muốn đổi ca hoặc ngày bắt đầu: hủy lớp rồi ghép lớp mới.", true);
            coDinh.MaximumSize = new Size(rong, 0);

            // Học phí và ngày kết thúc nằm cùng một hàng
            var hai = new TableLayoutPanel { ColumnCount = 2, RowCount = 2, AutoSize = true, Dock = DockStyle.Top, Margin = Padding.Empty };
            hai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            hai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            hai.Controls.Add(Ui.FieldLabel("Học phí cả khóa (đ)"), 0, 0);
            hai.Controls.Add(Ui.FieldLabel("Ngày kết thúc"), 1, 0);
            hai.Controls.Add(txtHocPhi, 0, 1);
            hai.Controls.Add(dtpKetThuc, 1, 1);

            var khung = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Location = new Point(Padding.Left, Padding.Top), BackColor = Theme.Surface };
            khung.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, rong));
            var tieuDe = new Label { Text = "Sửa lớp", Font = Theme.Chu(14F, 2), ForeColor = Theme.Text, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            foreach (var c in new Control[] { tieuDe, coDinh, Ui.FieldLabel("Tên lớp"), txtTen, hai,
                         Ui.FieldLabel("Học vào các thứ"), chonThu, lblSoBuoi, Ui.FieldLabel("Gia sư"), cboGiaSu, lblLoi, nut })
                khung.Controls.Add(c);
            Controls.Add(khung);

            ClientSize = new Size(rong + Padding.Horizontal, 520);
            Action coLai = () =>
            {
                khung.PerformLayout();
                ClientSize = new Size(rong + Padding.Horizontal, khung.GetPreferredSize(new Size(rong, 0)).Height + Padding.Vertical);
            };
            Load += (s, e) => { CapNhatSoBuoi(); coLai(); };
            lblLoi.TextChanged += (s, e) => coLai();
        }

        private void CapNhatSoBuoi()
        {
            if (!chonThu.HopLe) { lblSoBuoi.Text = "Chọn ít nhất một thứ."; lblSoBuoi.ForeColor = Theme.Margin; return; }
            int soBuoi = new LopHoc { ThoiGianBatDau = _lop.BatDau, ThoiGianKetThuc = dtpKetThuc.Value.Date, ThuHoc = chonThu.GiaTri }.DemSoBuoi();
            lblSoBuoi.ForeColor = soBuoi == 0 ? Theme.Margin : Theme.TextMuted;
            lblSoBuoi.Text = soBuoi == 0 ? "Khoảng ngày này không có buổi nào rơi vào các thứ đã chọn."
                : $"{chonThu.MoTa}: {soBuoi} buổi từ {_lop.BatDau:dd/MM/yyyy} đến {dtpKetThuc.Value:dd/MM/yyyy}.";
        }

        private void Luu()
        {
            lblLoi.Text = "";
            decimal hocPhi;
            string so = txtHocPhi.Text.Replace(".", "").Replace(",", "").Replace(" ", "");
            if (!decimal.TryParse(so, NumberStyles.Number, CultureInfo.InvariantCulture, out hocPhi)) { BaoLoi("Học phí phải là số, ví dụ 1500000.", txtHocPhi); return; }
            if (!chonThu.HopLe) { BaoLoi("Chọn ít nhất một thứ học trong tuần.", chonThu); return; }
            var gs = cboGiaSu.SelectedItem as MucGiaSu;
            if (gs == null) { BaoLoi("Chọn gia sư.", cboGiaSu); return; }

            try
            {
                Cursor = Cursors.WaitCursor;
                using (var service = new LopHocService())
                    service.SuaLop(_lop.MaLop, new SuaLopYeuCau
                    {
                        TenLop = txtTen.Text, HocPhi = hocPhi, KetThuc = dtpKetThuc.Value.Date, ThuHoc = chonThu.GiaTri, MaGS = gs.MaGS
                    });
                ThongBao.ThanhCong("Đã lưu thay đổi của lớp");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is UnauthorizedAccessException)
            {
                BaoLoi(ex.Message, null);
            }
            finally { Cursor = Cursors.Default; }
        }

        private void BaoLoi(string loi, Control o)
        {
            lblLoi.Text = loi;
            o?.Focus();
        }
    }
}
