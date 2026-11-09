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
    /// Form quản lý Học Viên - KHÔNG CÓ EMAIL
    /// </summary>
    public partial class frmHocVien : Form
    {
        private HocVienService _service;
        private BindingSource _bs;
        private bool isAdding = false;
        private bool isEditing = false;

        public frmHocVien()
        {
            InitializeComponent();
            _service = new HocVienService();
            _bs = new BindingSource();
            Load += FrmHocVien_Load;
            FormClosed += FrmHocVien_FormClosed;
        }

        private void FrmHocVien_Load(object sender, EventArgs e)
        {
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
                MessageBox.Show($"{Constants.Messages.LoiKhiThem}{ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            if (dgv.Columns.Count > 0)
            {
                dgv.Columns["MaHV"].HeaderText = "Mã HV";
                dgv.Columns["HoTen"].HeaderText = "Họ tên";
                dgv.Columns["SDT"].HeaderText = "Số ĐT";
                dgv.Columns["DiaChi"].HeaderText = "Địa chỉ";

                if (dgv.Columns.Contains("LopHocs"))
                    dgv.Columns["LopHocs"].Visible = false;
            }
        }

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (!isAdding && !isEditing && dgv.CurrentRow != null && dgv.CurrentRow.DataBoundItem is HocVien hv)
            {
                txtMaHV.Text = hv.MaHV.ToString();
                txtHoTen.Text = hv.HoTen;
                txtSDT.Text = hv.SDT;
                txtDiaChi.Text = hv.DiaChi;
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
            btnThem.Text = "Lưu";
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
                    txtDiaChi.Text
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
                if (string.IsNullOrWhiteSpace(txtMaHV.Text))
                {
                    MessageBox.Show("Vui lòng chọn học viên cần sửa.",
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
            txtMaHV.ReadOnly = true;
            btnSua.Text = "Lưu";
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
                    int.Parse(txtMaHV.Text),
                    txtHoTen.Text,
                    txtSDT.Text,
                    txtDiaChi.Text
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
                if (string.IsNullOrWhiteSpace(txtMaHV.Text))
                {
                    MessageBox.Show("Vui lòng chọn học viên cần xóa.",
                        Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa học viên '{txtHoTen.Text}'?",
                    Constants.Messages.ConfirmTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _service.Delete(int.Parse(txtMaHV.Text));

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
            if (txtTimKiem.Text == "Nhập tên, số điện thoại hoặc địa chỉ...")
            {
                txtTimKiem.Text = "";
                txtTimKiem.ForeColor = DoAnWD.UI.Theme.Text;
            }
        }

        private void txtTimKiem_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTimKiem.Text))
            {
                txtTimKiem.Text = "Nhập tên, số điện thoại hoặc địa chỉ...";
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
                   string.IsNullOrWhiteSpace(txtDiaChi.Text);
        }

        private void ResetFormState()
        {
            LockInputs(true);
            btnThem.Text = "Thêm";
            btnSua.Text = "Sửa";
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
            txtDiaChi.ReadOnly = lockState;

            Color bgColor = lockState ? Constants.Colors.LightGray : Color.White;
            txtHoTen.BackColor = bgColor;
            txtSDT.BackColor = bgColor;
            txtDiaChi.BackColor = bgColor;
        }

        private void ClearInputs()
        {
            txtMaHV.Clear();
            txtHoTen.Clear();
            txtSDT.Clear();
            txtDiaChi.Clear();

            if (txtTimKiem.ForeColor != Color.Gray)
            {
                txtTimKiem.Clear();
                txtTimKiem.Text = "Nhập tên, số điện thoại hoặc địa chỉ...";
                txtTimKiem.ForeColor = Color.Gray;
            }
        }

        private void FocusFirstError(string errorMessage)
        {
            if (errorMessage.Contains("họ tên"))
                txtHoTen.Focus();
            else if (errorMessage.Contains("số điện thoại") || errorMessage.Contains("SĐT"))
                txtSDT.Focus();
            else if (errorMessage.Contains("địa chỉ"))
                txtDiaChi.Focus();
        }

        private void FrmHocVien_FormClosed(object sender, FormClosedEventArgs e)
        {
            _service?.Dispose();
        }
    }
}