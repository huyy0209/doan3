using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Validators;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD.UI
{
    /// <summary>
    /// Khung nhập hồ sơ dùng chung: vừa nằm trong trang (sửa người đang chọn),
    /// vừa nằm trong hộp thoại "Thêm ..." (nhập người mới). Chỉ lo nhập liệu, không gọi DB.
    /// </summary>
    public abstract class HoSoEditor : UserControl
    {
        protected HoSoEditor()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Dock = DockStyle.Top;
            BackColor = Theme.Surface;
            Font = Theme.Body;
            Margin = new Padding(0);
        }

        /// <summary>Ô nhập đầu tiên (để đặt con trỏ khi mở)</summary>
        public abstract Control OBatDau { get; }

        /// <summary>Đưa con trỏ tới ô gây lỗi dựa vào nội dung thông báo của validator</summary>
        public abstract void FocusTheoLoi(string thongBao);

        public abstract void XoaTrang();

        /// <summary>Hai ô đặt cạnh nhau trên cùng một hàng</summary>
        protected static TableLayoutPanel HaiCot(params Tuple<string, Control>[] o)
        {
            var hai = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true, Margin = new Padding(0) };
            hai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            hai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < o.Length; i++)
            {
                int col = i % 2;
                o[i].Item2.Dock = DockStyle.Top;
                var st = Ui.Stack(false, Ui.FieldLabel(o[i].Item1), o[i].Item2);
                st.AutoSize = true;
                st.Dock = DockStyle.Top;
                st.Margin = new Padding(0, 0, col == 0 ? 10 : 0, 0);
                hai.Controls.Add(st, col, i / 2);
            }
            return hai;
        }

        protected static Tuple<string, Control> O(string nhan, Control c) => Tuple.Create(nhan, c);

        protected static TextBox GhiChu(int cao)
        {
            var t = Ui.Input();
            t.Multiline = true;
            t.AcceptsReturn = true;     // Enter xuống dòng, không bấm nhầm nút Lưu
            t.Height = cao;
            t.ScrollBars = ScrollBars.Vertical;
            return t;
        }

        protected void Dung(params Control[] cs)
        {
            var st = Ui.Stack(false, cs);
            st.AutoSize = true;
            st.Dock = DockStyle.Top;
            Controls.Add(st);
        }
    }

    // =======================================================================
    public class HoSoHocVienEditor : HoSoEditor
    {
        private readonly TextBox txtHoTen, txtSDT, txtDiaChi, txtPhuHuynh, txtSDTPH, txtTruongLop, txtGhiChu;
        private readonly ComboBox cboTrinhDo;

        public HoSoHocVienEditor()
        {
            txtHoTen = Ui.Input("Bắt buộc");
            txtSDT = Ui.Input("Không bắt buộc nếu có SĐT phụ huynh");
            txtPhuHuynh = Ui.Input("Ví dụ: Chị Mai (mẹ)");
            txtSDTPH = Ui.Input("Ví dụ: 0909123456");
            txtTruongLop = Ui.Input("Ví dụ: TH Lê Văn Tám, lớp 4");
            txtDiaChi = Ui.Input();
            cboTrinhDo = Ui.Combo(true);
            cboTrinhDo.Items.AddRange(Constants.GoiY.TrinhDo);
            txtGhiChu = GhiChu(64);

            Dung(Ui.FieldLabel("Họ tên *"), txtHoTen,
                 HaiCot(O("SĐT học viên", txtSDT), O("Trình độ", cboTrinhDo),
                        O("Phụ huynh", txtPhuHuynh), O("SĐT phụ huynh", txtSDTPH)),
                 Ui.FieldLabel("Trường / lớp"), txtTruongLop,
                 Ui.FieldLabel("Địa chỉ"), txtDiaChi,
                 Ui.FieldLabel("Ghi chú"), txtGhiChu);
        }

        public override Control OBatDau => txtHoTen;

        public void Nap(HocVien hv)
        {
            txtHoTen.Text = hv.HoTen;
            txtSDT.Text = hv.SDT;
            txtPhuHuynh.Text = hv.TenPhuHuynh;
            txtSDTPH.Text = hv.SDTPhuHuynh;
            txtTruongLop.Text = hv.TruongLop;
            txtDiaChi.Text = hv.DiaChi;
            cboTrinhDo.Text = hv.TrinhDo ?? "";
            txtGhiChu.Text = hv.GhiChu;
        }

        public override void XoaTrang()
        {
            foreach (var t in new[] { txtHoTen, txtSDT, txtPhuHuynh, txtSDTPH, txtTruongLop, txtDiaChi, txtGhiChu }) t.Clear();
            cboTrinhDo.Text = "";
        }

        public HoSoHocVien Lay() => new HoSoHocVien
        {
            HoTen = txtHoTen.Text, SDT = txtSDT.Text, DiaChi = txtDiaChi.Text,
            TenPhuHuynh = txtPhuHuynh.Text, SDTPhuHuynh = txtSDTPH.Text,
            TruongLop = txtTruongLop.Text, TrinhDo = cboTrinhDo.Text, GhiChu = txtGhiChu.Text
        };

        public override void FocusTheoLoi(string thongBao)
        {
            string m = (thongBao ?? "").ToLower();
            if (m.Contains("họ tên")) txtHoTen.Focus();
            else if (m.Contains("phụ huynh")) txtSDTPH.Focus();
            else if (m.Contains("điện thoại")) txtSDT.Focus();
        }
    }

    // =======================================================================
    public class HoSoGiaSuEditor : HoSoEditor
    {
        private readonly TextBox txtHoTen, txtSDT, txtEmail, txtHocVan, txtGhiChu;
        private readonly ComboBox cboChuyenMon;
        private readonly NumericUpDown nudKinhNghiem;
        private readonly CheckBox[] chkCa;
        private readonly CheckBox chkTamNghi;

        public HoSoGiaSuEditor()
        {
            txtHoTen = Ui.Input("Bắt buộc");
            txtSDT = Ui.Input("Bắt buộc, ví dụ 0909123456");
            cboChuyenMon = Ui.Combo(true);
            txtEmail = Ui.Input("ten@gmail.com");
            txtHocVan = Ui.Input("Ví dụ: SV năm 4 HUTECH, Kiện tướng quốc gia");
            nudKinhNghiem = new NumericUpDown { Minimum = 0, Maximum = 60, Width = 90, Font = Theme.Body, Margin = new Padding(0, 0, 0, 4) };
            txtGhiChu = GhiChu(56);

            // Ca nhận dạy: bỏ chọn hết = nhận mọi ca
            var cas = Constants.CaHoc.TatCaCaHoc;
            chkCa = new CheckBox[cas.Length];
            var luoiCa = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            for (int i = 0; i < 3; i++) luoiCa.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            for (int i = 0; i < cas.Length; i++)
            {
                chkCa[i] = new CheckBox
                {
                    Text = Constants.CaHoc.TenNgan(cas[i]), Tag = cas[i], AutoSize = true, Font = Theme.Body,
                    ForeColor = Theme.Blend(Theme.ShiftColor(cas[i]), Theme.Text, 0.35f), Margin = new Padding(0, 2, 6, 2)
                };
                luoiCa.Controls.Add(chkCa[i], i % 3, i / 3);
            }
            chkTamNghi = new CheckBox
            {
                Text = "Tạm nghỉ (không nhận lớp mới)", AutoSize = true, Font = Theme.Body, ForeColor = Theme.Text,
                Margin = new Padding(0, 8, 0, 2)
            };

            Dung(Ui.FieldLabel("Họ tên *"), txtHoTen,
                 HaiCot(O("Số điện thoại *", txtSDT), O("Chuyên môn *", cboChuyenMon),
                        O("Email", txtEmail), O("Kinh nghiệm (năm)", nudKinhNghiem)),
                 Ui.FieldLabel("Học vấn / bằng cấp"), txtHocVan,
                 Ui.FieldLabel("Ca nhận dạy (bỏ trống = nhận mọi ca)"), luoiCa,
                 chkTamNghi,
                 Ui.FieldLabel("Ghi chú"), txtGhiChu);
            nudKinhNghiem.Dock = DockStyle.None;   // ô số ngắn, không kéo dài
        }

        public override Control OBatDau => txtHoTen;

        /// <summary>Gợi ý các môn đã có để nhập nhanh, giữ nguyên chữ đang gõ</summary>
        public void DatGoiYMon(string[] mon)
        {
            string dangGo = cboChuyenMon.Text;
            cboChuyenMon.Items.Clear();
            cboChuyenMon.Items.AddRange(mon);
            cboChuyenMon.Text = dangGo;
        }

        public void Nap(GiaSu gs)
        {
            txtHoTen.Text = gs.HoTen;
            txtSDT.Text = gs.SDT;
            cboChuyenMon.Text = gs.ChuyenMon ?? "";
            txtEmail.Text = gs.Email;
            txtHocVan.Text = gs.HocVan;
            nudKinhNghiem.Value = Math.Max(0, Math.Min(60, gs.KinhNghiem ?? 0));
            txtGhiChu.Text = gs.GhiChu;
            foreach (var c in chkCa) c.Checked = !string.IsNullOrWhiteSpace(gs.CaNhanDay) && gs.NhanCa((string)c.Tag);
            chkTamNghi.Checked = gs.TrangThai == Constants.GiaSuStatus.TamNghi;
        }

        public override void XoaTrang()
        {
            foreach (var t in new[] { txtHoTen, txtSDT, txtEmail, txtHocVan, txtGhiChu }) t.Clear();
            cboChuyenMon.Text = "";
            nudKinhNghiem.Value = 0;
            foreach (var c in chkCa) c.Checked = false;
            chkTamNghi.Checked = false;
        }

        public HoSoGiaSu Lay() => new HoSoGiaSu
        {
            HoTen = txtHoTen.Text, SDT = txtSDT.Text, ChuyenMon = cboChuyenMon.Text,
            Email = txtEmail.Text, HocVan = txtHocVan.Text,
            KinhNghiem = nudKinhNghiem.Value > 0 ? (int?)nudKinhNghiem.Value : null,
            CaNhanDay = chkCa.Where(c => c.Checked).Select(c => c.Text).ToList(),
            GhiChu = txtGhiChu.Text, TamNghi = chkTamNghi.Checked
        };

        public override void FocusTheoLoi(string thongBao)
        {
            string m = (thongBao ?? "").ToLower();
            if (m.Contains("họ tên")) txtHoTen.Focus();
            else if (m.Contains("điện thoại")) txtSDT.Focus();
            else if (m.Contains("chuyên môn")) cboChuyenMon.Focus();
            else if (m.Contains("email")) txtEmail.Focus();
        }
    }

    // =======================================================================
    /// <summary>
    /// Cửa sổ nhỏ bật ra khi bấm "Thêm học viên" / "Thêm gia sư".
    /// luu() trả về true khi lưu thành công thì cửa sổ đóng; lỗi nhập liệu thì giữ cửa sổ để sửa.
    /// </summary>
    public class dlgHoSo : Form, ITuGiaoDien
    {
        private readonly HoSoEditor _editor;
        private readonly Func<bool> _luu;

        public dlgHoSo(string tieuDe, string moTa, HoSoEditor editor, Func<bool> luu, string chuNutLuu = "Lưu")
        {
            _editor = editor;
            _luu = luu;

            Text = tieuDe;
            Font = Theme.Body;
            BackColor = Theme.Surface;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Dpi;
            Padding = new Padding(22, 16, 22, 16);

            var lblTieuDe = new Label { Text = tieuDe, Font = Theme.Section, ForeColor = Theme.Ink, AutoSize = true, Margin = new Padding(0, 0, 0, 2) };
            var lblMoTa = Ui.Text(moTa, true);
            lblMoTa.Margin = new Padding(0, 0, 0, 10);

            var btnLuu = Ui.Button(chuNutLuu, ButtonKind.Primary, Glyph.Check, 120);
            var btnHuy = Ui.Button("Hủy", ButtonKind.Ghost, Glyph.None, 90);
            btnLuu.Click += (s, e) => { if (_luu()) { DialogResult = DialogResult.OK; Close(); } };
            btnHuy.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            AcceptButton = btnLuu;
            CancelButton = btnHuy;

            var nut = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Top,
                Margin = new Padding(0, 14, 0, 0), WrapContents = false
            };
            nut.Controls.Add(btnLuu);
            nut.Controls.Add(btnHuy);

            const int rong = 520;
            var khung = new TableLayoutPanel
            {
                ColumnCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Location = new Point(Padding.Left, Padding.Top), BackColor = Theme.Surface
            };
            khung.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, rong));
            khung.Controls.Add(lblTieuDe, 0, 0);
            khung.Controls.Add(lblMoTa, 0, 1);
            khung.Controls.Add(editor, 0, 2);
            khung.Controls.Add(nut, 0, 3);
            Controls.Add(khung);

            // Tự tính kích thước cửa sổ theo nội dung (không dùng Form.AutoSize vì dễ co sai)
            ClientSize = new Size(rong + Padding.Horizontal, 400);
            Load += (s, e) =>
            {
                khung.PerformLayout();
                var co = khung.GetPreferredSize(new Size(rong, 0));
                ClientSize = new Size(rong + Padding.Horizontal, co.Height + Padding.Vertical);
            };
            Shown += (s, e) => _editor.OBatDau.Focus();
        }
    }
}
