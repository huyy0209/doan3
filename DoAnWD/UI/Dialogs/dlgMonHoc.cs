using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;
using DoAnWD.UI;
using DoAnWD.UI.Controls;
using DoAnWD.UI.Pages;

namespace DoAnWD.UI.Dialogs
{
    /// <summary>
    /// Danh mục môn học (quản lý): thêm, sửa, xóa môn; chọn loại môn
    ///  - Môn chính: đánh giá bằng xếp loại Yếu / Trung bình / Khá / Giỏi theo điểm TB.
    ///  - Môn năng khiếu: đánh giá theo cấp độ; nhập các cấp từ thấp đến cao, mỗi dòng một cấp.
    /// Môn đang có lớp thì không xóa được, chọn "Ngừng dạy" để ẩn khỏi ghép lớp mới.
    /// </summary>
    public class dlgMonHoc : Form, ITuGiaoDien
    {
        private readonly MonHocService _service = new MonHocService();
        private List<MonHocDanhMuc> _mons = new List<MonHocDanhMuc>();
        private Dictionary<string, int> _soLop = new Dictionary<string, int>();
        private int? _maDangSua;

        private readonly ListBox lst;
        private readonly TextBox txtTen, txtCapDo;
        private readonly RadioButton radChinh, radNangKhieu;
        private readonly CheckBox chkNgung;
        private readonly Label lblTieuDeSua, lblGoiY, lblCapDo;
        private readonly ThemedButton btnLuu, btnXoa;

        public dlgMonHoc()
        {
            Text = "Môn học";
            Font = Theme.Body;
            BackColor = Theme.Surface;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;
            Padding = new Padding(20, 16, 20, 16);
            ClientSize = new Size(820, 560);

            // ---- Danh sách môn ----
            lst = Ui.List(48, VeMon);
            lst.SelectedIndexChanged += (s, e) => HienMon(lst.SelectedItem as MonHocDanhMuc);
            var khung = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = Theme.Rule, Margin = new Padding(0, 4, 0, 4) };
            khung.Controls.Add(lst);
            var btnThem = Ui.Button("Thêm môn", ButtonKind.Secondary, Glyph.Plus);
            btnThem.Click += (s, e) => { lst.ClearSelected(); HienMon(null); txtTen.Focus(); };
            var trai = Ui.Stack(false, Ui.FieldLabel("Các môn (môn chính trước, năng khiếu sau)"), khung, btnThem);
            trai.RowStyles[1] = new RowStyle(SizeType.Percent, 100);
            trai.Dock = DockStyle.Fill;

            // ---- Sửa môn ----
            lblTieuDeSua = new Label { Font = Theme.Section, ForeColor = Theme.Ink, AutoSize = true, Margin = new Padding(0, 0, 0, 6) };
            txtTen = Ui.Input("Ví dụ: Cờ vua");
            txtTen.MaxLength = 50;
            radChinh = new RadioButton { Text = "Môn chính (xếp loại Yếu / Trung bình / Khá / Giỏi)", AutoSize = true, Font = Theme.Body, Checked = true, Margin = new Padding(0, 2, 0, 2) };
            radNangKhieu = new RadioButton { Text = "Năng khiếu (đánh giá theo cấp độ, thi lên cấp)", AutoSize = true, Font = Theme.Body, Margin = new Padding(0, 2, 0, 6) };
            radChinh.CheckedChanged += (s, e) => CapNhatLoai();
            lblCapDo = Ui.FieldLabel("Cấp độ, từ thấp đến cao (mỗi dòng một cấp)");
            txtCapDo = Ui.Input();
            txtCapDo.Multiline = true;
            txtCapDo.AcceptsReturn = true;
            txtCapDo.ScrollBars = ScrollBars.Vertical;
            txtCapDo.Height = 150;
            chkNgung = new CheckBox { Text = "Ngừng dạy (ẩn khỏi ghép lớp mới, lớp cũ giữ nguyên)", AutoSize = true, Font = Theme.Body, Margin = new Padding(0, 8, 0, 4) };
            lblGoiY = Ui.Text("", true);
            lblGoiY.MaximumSize = new Size(440, 0);

            btnLuu = Ui.Button("Lưu", ButtonKind.Primary, Glyph.Check, 110);
            btnLuu.Click += (s, e) => Luu();
            btnXoa = Ui.Button("Xóa môn", ButtonKind.Danger, Glyph.Trash, 120);
            btnXoa.Click += (s, e) => Xoa();
            var btnDong = Ui.Button("Đóng", ButtonKind.Ghost, Glyph.None, 90);
            btnDong.Click += (s, e) => Close();
            CancelButton = btnDong;

            var phai = Ui.Stack(false, lblTieuDeSua, Ui.FieldLabel("Tên môn"), txtTen, Ui.FieldLabel("Loại môn"),
                radChinh, radNangKhieu, lblCapDo, txtCapDo, chkNgung, lblGoiY, Ui.Row(btnLuu, btnXoa, btnDong));
            phai.Dock = DockStyle.Fill;
            phai.Padding = new Padding(16, 0, 0, 0);

            var cot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            cot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            cot.Controls.Add(trai, 0, 0);
            cot.Controls.Add(phai, 1, 0);
            Controls.Add(cot);

            Load += (s, e) => { Nap(); HienMon(null); };
        }

        private void Nap(int? chonMa = null)
        {
            try
            {
                _mons = _service.GetAll();
                _soLop = _service.SoLopTheoMon();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được danh mục môn: " + ex.GetBaseException().Message, Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            lst.BeginUpdate();
            lst.Items.Clear();
            lst.Items.AddRange(_mons.Cast<object>().ToArray());
            var chon = _mons.FirstOrDefault(m => m.MaMon == chonMa);
            if (chon != null) lst.SelectedItem = chon;
            lst.EndUpdate();
        }

        private int SoLop(MonHocDanhMuc m)
        {
            int n;
            return _soLop.TryGetValue(m.TenMon.Trim(), out n) ? n : 0;
        }

        private void VeMon(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var m = (MonHocDanhMuc)lst.Items[e.Index];
            var r = Ui.DrawListItemBackground(e);
            var g = e.Graphics;
            int tagW = Ui.DrawTag(g, m.LaNangKhieu ? "Năng khiếu" : "Môn chính", r,
                m.LaNangKhieu ? Theme.Warning : Theme.Ink, m.LaNangKhieu ? Theme.WarningSoft : Theme.InkSoft);
            TextRenderer.DrawText(g, m.TenMon + (m.NgungDay ? " (ngừng dạy)" : ""), Theme.BodyStrong,
                new Rectangle(r.X, r.Y, r.Width - tagW, r.Height / 2), m.NgungDay ? Theme.TextFaint : Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            string phu = $"{SoLop(m)} lớp" + (m.LaNangKhieu ? " · " + string.Join(" → ", m.DanhSachCapDo) : "");
            TextRenderer.DrawText(g, phu, Theme.Small, new Rectangle(r.X, r.Y + r.Height / 2, r.Width - tagW, r.Height / 2), Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void HienMon(MonHocDanhMuc m)
        {
            _maDangSua = m?.MaMon;
            lblTieuDeSua.Text = m == null ? "Thêm môn mới" : "Sửa môn " + m.TenMon;
            txtTen.Text = m?.TenMon ?? "";
            radNangKhieu.Checked = m != null && m.LaNangKhieu;
            radChinh.Checked = !radNangKhieu.Checked;
            txtCapDo.Text = m == null ? "" : string.Join(Environment.NewLine, m.DanhSachCapDo);
            chkNgung.Checked = m?.NgungDay ?? false;
            btnXoa.Enabled = m != null;
            btnLuu.Text = m == null ? "Thêm" : "Lưu";
            int soLop = m == null ? 0 : SoLop(m);
            lblGoiY.Text = m == null
                ? "Đổi tên môn sau này sẽ cập nhật luôn các lớp và chuyên môn gia sư đang dùng tên đó."
                : soLop > 0 ? $"Môn đang có {soLop} lớp: đổi tên sẽ cập nhật theo; không xóa được (chọn Ngừng dạy)."
                            : "Môn chưa có lớp nào, xóa được.";
            CapNhatLoai();
        }

        private void CapNhatLoai()
        {
            bool nk = radNangKhieu.Checked;
            lblCapDo.Visible = txtCapDo.Visible = nk;
            if (nk && txtCapDo.Text.Trim().Length == 0)
                txtCapDo.Text = string.Join(Environment.NewLine, new[] { "Cấp 1", "Cấp 2", "Cấp 3", "Cấp 4", "Cấp 5" });
        }

        private void Luu()
        {
            string loai = radNangKhieu.Checked ? Constants.LoaiMon.NangKhieu : Constants.LoaiMon.Chinh;
            var capDo = txtCapDo.Lines.Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
            try
            {
                var mon = _service.Luu(_maDangSua, txtTen.Text, loai, capDo, chkNgung.Checked);
                Nap(mon.MaMon);
                HienMon(mon);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không lưu được: " + ex.GetBaseException().Message, Constants.Messages.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Xoa()
        {
            if (!_maDangSua.HasValue) return;
            if (MessageBox.Show($"Xóa môn {txtTen.Text}?", Constants.Messages.ConfirmTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                _service.Xoa(_maDangSua.Value);
                Nap();
                HienMon(null);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
