using System;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD
{
    /// <summary>
    /// ✨ Form quản lý Lớp Học - CẬP NHẬT CHO OPTION 2
    /// Master-Detail: Gia Sư → Lớp Học
    /// </summary>
    public partial class frmGhepLop : Form
    {
        private GiaSuService _giaSuService;
        private LopHocService _lopHocService;

        public frmGhepLop()
        {
            InitializeComponent();
            _giaSuService = new GiaSuService();
            _lopHocService = new LopHocService();
            Load += FrmGhepLop_Load;
            FormClosed += FrmGhepLop_FormClosed;
        }

        private void FrmGhepLop_Load(object sender, EventArgs e)
        {
            LoadGiaSuList();
        }

        /// <summary>
        /// Tạo lại service (DbContext mới) để không đọc dữ liệu cũ đã cache
        /// sau khi form khác (dialog ghép lớp, timer) thay đổi CSDL.
        /// </summary>
        private void TaoLaiServices()
        {
            _giaSuService?.Dispose();
            _lopHocService?.Dispose();
            _giaSuService = new GiaSuService();
            _lopHocService = new LopHocService();
        }

        private void LoadGiaSuList()
        {
            try
            {
                int? maGSDangChon = null;
                if (dgvGiaSu.CurrentRow != null && dgvGiaSu.Columns.Contains("MaGS"))
                    maGSDangChon = Convert.ToInt32(dgvGiaSu.CurrentRow.Cells["MaGS"].Value);

                TaoLaiServices();
                var giaSuData = _giaSuService.GetWithLopCount();
                dgvGiaSu.DataSource = giaSuData;

                ConfigureGiaSuColumns();
                ApplyRowColors();

                // Giữ lại gia sư đang chọn sau khi tải lại
                if (maGSDangChon.HasValue)
                {
                    foreach (DataGridViewRow row in dgvGiaSu.Rows)
                    {
                        if (Convert.ToInt32(row.Cells["MaGS"].Value) == maGSDangChon.Value)
                        {
                            dgvGiaSu.CurrentCell = row.Cells["HoTen"];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách gia sư: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigureGiaSuColumns()
        {
            if (dgvGiaSu.Columns.Count > 0)
            {
                dgvGiaSu.Columns["MaGS"].HeaderText = "Mã";
                dgvGiaSu.Columns["MaGS"].Width = 50;
                dgvGiaSu.Columns["HoTen"].HeaderText = "Họ Tên";
                dgvGiaSu.Columns["ChuyenMon"].HeaderText = "Chuyên Môn";
                dgvGiaSu.Columns["TrangThai"].HeaderText = "Trạng Thái";
                dgvGiaSu.Columns["TrangThai"].Width = 100;
                dgvGiaSu.Columns["SoLop"].HeaderText = "Số Lớp";
                dgvGiaSu.Columns["SoLop"].Width = 70;
                dgvGiaSu.Columns["SoLop"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvGiaSu.Columns["SoLop"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                dgvGiaSu.Columns["SoLop"].DefaultCellStyle.ForeColor = Constants.Colors.Orange;
            }
        }

        private void ApplyRowColors()
        {
            foreach (DataGridViewRow row in dgvGiaSu.Rows)
            {
                var trangThai = row.Cells["TrangThai"].Value?.ToString();

                // ✅ SỬA: Hỗ trợ cả trạng thái mới và cũ
                if (trangThai == Constants.GiaSuStatus.DangDay ||
                    trangThai == Constants.GiaSuStatus.Ban)
                {
                    row.DefaultCellStyle.BackColor = Constants.Colors.ActiveOrange;
                }
                else if (trangThai == Constants.GiaSuStatus.Ranh ||
                         trangThai == Constants.GiaSuStatus.SanSang)
                {
                    row.DefaultCellStyle.BackColor = Constants.Colors.ActiveGreen;
                }
                else if (trangThai == Constants.GiaSuStatus.TamNghi)
                {
                    row.DefaultCellStyle.BackColor = Constants.Colors.InactiveGray;
                    row.DefaultCellStyle.ForeColor = Constants.Colors.Gray;
                }
            }
        }

        private void dgvGiaSu_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvGiaSu.CurrentRow == null) return;

            try
            {
                int maGS = Convert.ToInt32(dgvGiaSu.CurrentRow.Cells["MaGS"].Value);
                LoadGiaSuDetails(maGS);
                LoadLopHocByGiaSu(maGS);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadGiaSuDetails(int maGS)
        {
            try
            {
                var giaSu = _giaSuService.GetById(maGS);
                if (giaSu != null)
                {
                    txtMaGS.Text = giaSu.MaGS.ToString();
                    txtHoTen.Text = giaSu.HoTen ?? "";
                    txtChuyenMon.Text = giaSu.ChuyenMon ?? "";
                    txtTrangThai.Text = giaSu.TrangThai ?? Constants.GiaSuStatus.Ranh;

                    // ✅ SỬA: Color code hỗ trợ cả trạng thái mới
                    if (giaSu.TrangThai == Constants.GiaSuStatus.DangDay)
                        txtTrangThai.BackColor = Constants.Colors.Red;
                    else if (giaSu.TrangThai == Constants.GiaSuStatus.Ban)
                        txtTrangThai.BackColor = Constants.Colors.Orange;
                    else if (giaSu.TrangThai == Constants.GiaSuStatus.TamNghi)
                        txtTrangThai.BackColor = Constants.Colors.Gray;
                    else
                        txtTrangThai.BackColor = Constants.Colors.Green;
                    txtTrangThai.ForeColor = Color.White;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải chi tiết gia sư: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadLopHocByGiaSu(int maGS)
        {
            try
            {
                var lopHocData = _lopHocService.GetByGiaSu(maGS);
                dgvLopHoc.DataSource = lopHocData;

                ConfigureLopHocColumns();
                ApplyLopHocRowColors();

                txtSoLop.Text = _giaSuService.CountActiveLopHoc(maGS).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách lớp học: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigureLopHocColumns()
        {
            if (dgvLopHoc.Columns.Count > 0)
            {
                dgvLopHoc.Columns["MaLop"].HeaderText = "Mã Lớp";
                dgvLopHoc.Columns["MaLop"].Width = 70;
                dgvLopHoc.Columns["HocVien"].HeaderText = "Học Viên";
                dgvLopHoc.Columns["MonHoc"].HeaderText = "Môn Học";
                dgvLopHoc.Columns["BatDau"].HeaderText = "Bắt Đầu";
                dgvLopHoc.Columns["BatDau"].DefaultCellStyle.Format = Constants.Format.DateFormat;
                dgvLopHoc.Columns["BatDau"].Width = 90;
                dgvLopHoc.Columns["KetThuc"].HeaderText = "Kết Thúc";
                dgvLopHoc.Columns["KetThuc"].DefaultCellStyle.Format = Constants.Format.DateFormat;
                dgvLopHoc.Columns["KetThuc"].Width = 90;
                dgvLopHoc.Columns["HocPhi"].HeaderText = "Học Phí";
                dgvLopHoc.Columns["HocPhi"].DefaultCellStyle.Format = "N0";
                dgvLopHoc.Columns["HocPhi"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvLopHoc.Columns["TrangThai"].HeaderText = "Trạng Thái";
                dgvLopHoc.Columns["TrangThai"].Width = 110;

                // ✅ THÊM: Hiển thị Ca học nếu có
                if (dgvLopHoc.Columns.Contains("CaHoc"))
                {
                    dgvLopHoc.Columns["CaHoc"].HeaderText = "Ca Học";
                    dgvLopHoc.Columns["CaHoc"].Width = 120;
                }
            }
        }

        private void ApplyLopHocRowColors()
        {
            foreach (DataGridViewRow row in dgvLopHoc.Rows)
            {
                var trangThai = row.Cells["TrangThai"].Value?.ToString();

                // ✅ SỬA: Hỗ trợ cả trạng thái mới
                if (trangThai == Constants.LopHocStatus.DangHoc ||
                    trangThai == Constants.LopHocStatus.DangDienRa)
                {
                    row.DefaultCellStyle.BackColor = Constants.Colors.ActiveGreen;
                }
                else if (trangThai == Constants.LopHocStatus.ChuaBatDau)
                {
                    row.DefaultCellStyle.BackColor = Constants.Colors.ActiveOrange;
                }
                else if (trangThai == Constants.LopHocStatus.HoanThanh ||
                         trangThai == Constants.LopHocStatus.DaKetThuc)
                {
                    row.DefaultCellStyle.BackColor = Constants.Colors.LightGray;
                }
                else if (trangThai == Constants.LopHocStatus.DaHuy)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);
                    row.DefaultCellStyle.ForeColor = Constants.Colors.Red;
                    row.DefaultCellStyle.Font = new Font(dgvLopHoc.Font, FontStyle.Strikeout);
                }
            }
        }

        private void btnGhepLop_Click(object sender, EventArgs e)
        {
            using (var dialog = new frmChucNangGhepLop())
            {
                // Dialog đã báo thành công chi tiết - chỉ cần tải lại danh sách
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    LoadGiaSuList();
            }
        }

        private void btnXoaLop_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvLopHoc.CurrentRow == null)
                {
                    MessageBox.Show("Vui lòng chọn lớp học cần xóa.",
                        Constants.Messages.WarningTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                int maLop = Convert.ToInt32(dgvLopHoc.CurrentRow.Cells["MaLop"].Value);
                string hocVien = dgvLopHoc.CurrentRow.Cells["HocVien"].Value?.ToString() ?? "";
                string monHoc = dgvLopHoc.CurrentRow.Cells["MonHoc"].Value?.ToString() ?? "";
                string trangThai = dgvLopHoc.CurrentRow.Cells["TrangThai"].Value?.ToString() ?? "";

                bool coTheHuy = Constants.LopHocStatus.LaHoatDong(trangThai);
                DialogResult result;

                if (coTheHuy)
                {
                    // Lớp còn hoạt động: ưu tiên HỦY (giữ lịch sử) thay vì xóa hẳn
                    result = MessageBox.Show(
                        $"Lớp {maLop} - {monHoc} (học viên: {hocVien})\n\n" +
                        "• Yes: HỦY lớp (giữ lại lịch sử, giải phóng lịch gia sư/học viên)\n" +
                        "• No: XÓA HẲN lớp khỏi hệ thống\n" +
                        "• Cancel: không làm gì",
                        Constants.Messages.ConfirmTitle,
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                }
                else
                {
                    result = MessageBox.Show(
                        $"Xóa hẳn lớp {maLop} - {monHoc} (học viên: {hocVien})?",
                        Constants.Messages.ConfirmTitle,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes ? DialogResult.No : DialogResult.Cancel;
                }

                if (result == DialogResult.Cancel)
                    return;

                if (result == DialogResult.Yes)
                {
                    _lopHocService.HuyLop(maLop);
                    MessageBox.Show("Đã hủy lớp.", Constants.Messages.SuccessTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _lopHocService.Delete(maLop);
                    MessageBox.Show(Constants.Messages.XoaThanhCong, Constants.Messages.SuccessTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadGiaSuList();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, Constants.Messages.WarningTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Constants.Messages.LoiKhiXoa}{ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadGiaSuList();
        }

        private void FrmGhepLop_FormClosed(object sender, FormClosedEventArgs e)
        {
            _giaSuService?.Dispose();
            _lopHocService?.Dispose();
        }
    }
}