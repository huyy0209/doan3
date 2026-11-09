using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD
{
    /// <summary>
    /// Form quản lý Gia Sư - Refactored 3-Layer
    /// UI chỉ gọi Service, không truy cập DB trực tiếp
    /// </summary>
    public partial class frmGiaSu : Form
    {
        private GiaSuService _service;
        private BindingSource _bs;
        private bool isAdding = false;
        private bool isEditing = false;

        public frmGiaSu()
        {
            InitializeComponent();
            _service = new GiaSuService();
            _bs = new BindingSource();
            Load += FrmGiaSu_Load;
            FormClosed += FrmGiaSu_FormClosed;
            dgv.DataBindingComplete += (s, e) => ToMauTrangThai();
        }

        private void FrmGiaSu_Load(object sender, EventArgs e)
        {
            cboTrangThai.SelectedIndex = 0;
            new ToolTip().SetToolTip(cboTrangThai,
                "Rảnh / Bận / Đang dạy được tính tự động theo lịch dạy.\n" +
                "Chọn \"Tạm nghỉ\" để gia sư ngừng nhận lớp mới.");
            LoadData();
            LockInputs(true);
        }

        private void LoadData()
        {
            try
            {
                var data = _service.GetAll();
                _bs.DataSource = data.ToList();
                dgv.DataSource = _bs;

                ConfigureGridColumns();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách gia sư: {ex.GetBaseException().Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            if (dgv.Columns.Count > 0)
            {
                dgv.Columns["MaGS"].HeaderText = "Mã GS";
                dgv.Columns["HoTen"].HeaderText = "Họ Tên";
                dgv.Columns["SDT"].HeaderText = "Số ĐT";
                dgv.Columns["ChuyenMon"].HeaderText = "Chuyên Môn";
                dgv.Columns["TrangThai"].HeaderText = "Trạng Thái";

                if (dgv.Columns.Contains("LopHocs"))
                    dgv.Columns["LopHocs"].Visible = false;
            }
        }

        /// <summary>
        /// Tô màu dòng theo trạng thái (chạy lại mỗi khi grid bind dữ liệu, kể cả khi tìm kiếm)
        /// </summary>
        private void ToMauTrangThai()
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                var gs = row.DataBoundItem as GiaSu;
                if (gs == null) continue;

                if (gs.TrangThai == Constants.GiaSuStatus.TamNghi)
                    row.DefaultCellStyle.ForeColor = Constants.Colors.Gray;
                else if (gs.TrangThai == Constants.GiaSuStatus.DangDay)
                    row.DefaultCellStyle.BackColor = Constants.Colors.ActiveOrange;
                else if (gs.TrangThai == Constants.GiaSuStatus.Ban)
                    row.DefaultCellStyle.BackColor = Constants.Colors.ActiveBlue;
            }
        }

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (!isAdding && !isEditing && dgv.CurrentRow != null && dgv.CurrentRow.DataBoundItem is GiaSu gs)
            {
                txtMaGS.Text = gs.MaGS.ToString();
                txtHoTen.Text = gs.HoTen;
                txtSDT.Text = gs.SDT;
                txtChuyenMon.Text = gs.ChuyenMon;

                string trangThai = gs.TrangThai ?? Constants.GiaSuStatus.Ranh;
                int index = cboTrangThai.Items.IndexOf(trangThai);
                cboTrangThai.SelectedIndex = index >= 0 ? index : 0;
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (!isAdding)
            {
                StartAdding();
            }
            else
            {
                SaveNew();
            }
        }

        private void StartAdding()
        {
            isAdding = true;
            ClearInputs();
            LockInputs(false);
            btnThem.Text = "💾 Lưu";
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
            btnHuy.Visible = true;
            dgv.Enabled = false;
            txtHoTen.Focus();
        }

        private void SaveNew()
        {
            try
            {
                if (IsAllInputsEmpty())
                {
                    CancelOperation(Constants.Messages.HuyThaoTac);
                    return;
                }

                _service.Add(
                    txtHoTen.Text,
                    txtSDT.Text,
                    txtChuyenMon.Text,
                    cboTrangThai.SelectedItem?.ToString()
                );

                MessageBox.Show(Constants.Messages.ThemThanhCong,
                    Constants.Messages.SuccessTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ResetFormState();
                LoadData();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message,
                    Constants.Messages.WarningTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                FocusFirstError(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Constants.Messages.LoiKhiThem}{ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (!isEditing)
            {
                if (string.IsNullOrWhiteSpace(txtMaGS.Text))
                {
                    MessageBox.Show("Vui lòng chọn gia sư cần sửa.",
                        Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                StartEditing();
            }
            else
            {
                SaveEdit();
            }
        }

        private void StartEditing()
        {
            isEditing = true;
            LockInputs(false);
            txtMaGS.ReadOnly = true;
            btnSua.Text = "💾 Lưu";
            btnThem.Enabled = false;
            btnXoa.Enabled = false;
            btnHuy.Visible = true;
            dgv.Enabled = false;
            txtHoTen.Focus();
        }

        private void SaveEdit()
        {
            try
            {
                if (IsAllInputsEmpty())
                {
                    CancelOperation(Constants.Messages.HuyThaoTac);
                    return;
                }

                _service.Update(
                    int.Parse(txtMaGS.Text),
                    txtHoTen.Text,
                    txtSDT.Text,
                    txtChuyenMon.Text,
                    cboTrangThai.SelectedItem?.ToString()
                );

                MessageBox.Show(Constants.Messages.SuaThanhCong,
                    Constants.Messages.SuccessTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ResetFormState();
                LoadData();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message,
                    Constants.Messages.WarningTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                FocusFirstError(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Constants.Messages.LoiKhiSua}{ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtMaGS.Text))
                {
                    MessageBox.Show("Vui lòng chọn gia sư cần xóa.",
                        Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa gia sư '{txtHoTen.Text}'?",
                    Constants.Messages.ConfirmTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _service.Delete(int.Parse(txtMaGS.Text));

                    MessageBox.Show(Constants.Messages.XoaThanhCong,
                        Constants.Messages.SuccessTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    LoadData();
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message,
                    Constants.Messages.WarningTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Constants.Messages.LoiKhiXoa}{ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            if (isAdding || isEditing)
            {
                CancelOperation(Constants.Messages.HuyThaoTac);
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            if (txtTimKiem.ForeColor == Color.Gray) return;

            try
            {
                var results = _service.Search(txtTimKiem.Text);
                _bs.DataSource = results.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void txtTimKiem_Enter(object sender, EventArgs e)
        {
            if (txtTimKiem.Text == "Nhập tên, số điện thoại hoặc chuyên môn...")
            {
                txtTimKiem.Text = "";
                txtTimKiem.ForeColor = Color.Black;
            }
        }

        private void txtTimKiem_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTimKiem.Text))
            {
                txtTimKiem.Text = "Nhập tên, số điện thoại hoặc chuyên môn...";
                txtTimKiem.ForeColor = Color.Gray;
            }
        }

        private void btn_MouseEnter(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Enabled)
            {
                btn.BackColor = ControlPaint.Light(btn.BackColor, 0.2f);
                btn.Cursor = Cursors.Hand;
            }
        }

        private void btn_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                if (btn == btnThem)
                    btn.BackColor = Constants.Colors.Green;
                else if (btn == btnSua)
                    btn.BackColor = Constants.Colors.Blue;
                else if (btn == btnXoa)
                    btn.BackColor = Constants.Colors.Red;
                else if (btn == btnHuy)
                    btn.BackColor = Color.FromArgb(149, 165, 166);
            }
        }

        private bool IsAllInputsEmpty()
        {
            return string.IsNullOrWhiteSpace(txtHoTen.Text) &&
                   string.IsNullOrWhiteSpace(txtSDT.Text) &&
                   string.IsNullOrWhiteSpace(txtChuyenMon.Text);
        }

        private void ResetFormState()
        {
            LockInputs(true);
            btnThem.Text = "➕ Thêm";
            btnSua.Text = "✏️ Sửa";
            btnThem.Enabled = true;
            btnSua.Enabled = true;
            btnXoa.Enabled = true;
            btnHuy.Visible = false;
            dgv.Enabled = true;
            isAdding = false;
            isEditing = false;
        }

        private void CancelOperation(string message)
        {
            ResetFormState();
            ClearInputs();
            MessageBox.Show(message, Constants.Messages.InfoTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LockInputs(bool lockState)
        {
            txtHoTen.ReadOnly = lockState;
            txtSDT.ReadOnly = lockState;
            txtChuyenMon.ReadOnly = lockState;
            cboTrangThai.Enabled = !lockState;

            Color bgColor = lockState ? Constants.Colors.LightGray : Color.White;
            txtHoTen.BackColor = bgColor;
            txtSDT.BackColor = bgColor;
            txtChuyenMon.BackColor = bgColor;
            cboTrangThai.BackColor = bgColor;
        }

        private void ClearInputs()
        {
            txtMaGS.Clear();
            txtHoTen.Clear();
            txtSDT.Clear();
            txtChuyenMon.Clear();
            cboTrangThai.SelectedIndex = 0;

            if (txtTimKiem.ForeColor != Color.Gray)
            {
                txtTimKiem.Clear();
                txtTimKiem.Text = "Nhập tên, số điện thoại hoặc chuyên môn...";
                txtTimKiem.ForeColor = Color.Gray;
            }
        }

        private void FocusFirstError(string errorMessage)
        {
            if (errorMessage.Contains("họ tên"))
                txtHoTen.Focus();
            else if (errorMessage.Contains("số điện thoại") || errorMessage.Contains("SĐT"))
                txtSDT.Focus();
            else if (errorMessage.Contains("chuyên môn"))
                txtChuyenMon.Focus();
        }

        private void FrmGiaSu_FormClosed(object sender, FormClosedEventArgs e)
        {
            _service?.Dispose();
        }
    }
}