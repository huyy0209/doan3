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
    /// ✨ Form Thống Kê - CẬP NHẬT CHO OPTION 2
    /// Bổ sung: Thống kê ca học, lớp sắp bắt đầu/kết thúc, auto-refresh
    /// </summary>
    public partial class frmThongKe : Form
    {
        private ThongKeService _service;
        private LopHocService _lopHocService;
        private Timer _refreshTimer;
        private Label lblDoanhThu;

        public frmThongKe()
        {
            InitializeComponent();
            _service = new ThongKeService();
            _lopHocService = new LopHocService();

            Load += FrmThongKe_Load;
            FormClosed += FrmThongKe_FormClosed;
        }

        private void FrmThongKe_Load(object sender, EventArgs e)
        {
            InitializeDoanhThuLabel();
            RefreshAll();
            InitializeAutoRefresh();
        }

        // ===================================================================
        // ✨ HELPER METHOD - AN TOÀN KHI SET COLUMN PROPERTIES
        // ===================================================================

        /// <summary>
        /// Helper method để set column property an toàn
        /// </summary>
        private void SetColumnProperty(DataGridView dgv, string columnName, Action<DataGridViewColumn> action)
        {
            if (dgv?.Columns == null) return;

            try
            {
                var column = dgv.Columns[columnName];
                if (column != null && column.DataGridView != null)
                {
                    action(column);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi set column '{columnName}': {ex.Message}");
            }
        }

        // ===================================================================
        // ✨ AUTO-REFRESH TIMER
        // ===================================================================

        private void InitializeAutoRefresh()
        {
            _refreshTimer = new Timer();
            _refreshTimer.Interval = 30000; // 30 giây
            _refreshTimer.Tick += (s, ev) => RefreshAll();
            _refreshTimer.Start();
        }

        // ===================================================================
        // ✨ LOAD TẤT CẢ DỮ LIỆU
        // ===================================================================

        /// <summary>
        /// Thêm dòng doanh thu vào thẻ "Tổng lớp học" (bên phải con số)
        /// </summary>
        private void InitializeDoanhThuLabel()
        {
            var card = lblCountLop.Parent;
            lblCountLop.Width = 150;

            lblDoanhThu = new Label
            {
                AutoSize = false,
                Location = new Point(170, 48),
                Size = new Size(card.Width - 185, 62),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight
            };
            card.Controls.Add(lblDoanhThu);
            lblDoanhThu.BringToFront();
        }

        private void RefreshAll()
        {
            try
            {
                // DbContext mới mỗi lần làm mới => số liệu luôn khớp CSDL
                _service?.Dispose();
                _lopHocService?.Dispose();
                _service = new ThongKeService();
                _lopHocService = new LopHocService();

                LoadStatistics();
                LoadGiaSuList();

                // ✨ CÁC THỐNG KÊ MỚI CHO OPTION 2
                LoadThongKeCaHoc();
                LoadLopSapBatDau();
                LoadLopSapKetThuc();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi RefreshAll: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===================================================================
        // THỐNG KÊ TỔNG QUAN
        // ===================================================================

        private void LoadStatistics()
        {
            try
            {
                var stats = _service.GetStatistics();

                lblCountGS.Text = stats.TongGiaSu.ToString();
                lblCountHV.Text = stats.TongHocVien.ToString();
                lblCountLop.Text = stats.TongLopHoc.ToString();
                lblCountGSRanh.Text = stats.GiaSuRanh.ToString();
                lblCountGSDangDay.Text = stats.GiaSuDangDay.ToString();
                lblCountLopDangHoc.Text = stats.LopDangHoc.ToString();

                if (lblDoanhThu != null)
                {
                    var vi = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
                    lblDoanhThu.Text =
                        $"💰 Học phí lớp đang hoạt động: {stats.DoanhThuDangHoatDong.ToString("N0", vi)} đ\n" +
                        $"📅 Khai giảng tháng {DateTime.Now:MM/yyyy}: {stats.DoanhThuThangNay.ToString("N0", vi)} đ\n" +
                        $"🚫 Lớp đã hủy: {stats.LopDaHuy}";
                }

                // ✨ THÊM: Thống kê theo trạng thái mới
                int chuaBatDau = _lopHocService.CountByStatus(Constants.LopHocStatus.ChuaBatDau);
                int dangDienRa = _lopHocService.CountByStatus(Constants.LopHocStatus.DangDienRa);
                int daKetThuc = _lopHocService.CountByStatus(Constants.LopHocStatus.DaKetThuc);

                lblCountChuaBatDau.Text = chuaBatDau.ToString();
                lblCountDangDienRa.Text = dangDienRa.ToString();
                lblCountDaKetThuc.Text = daKetThuc.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadStatistics: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải thống kê: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===================================================================
        // DANH SÁCH GIA SƯ
        // ===================================================================

        private void LoadGiaSuList()
        {
            try
            {
                var giaSuData = _service.GetGiaSuWithLopCount();
                dgvGiaSu.DataSource = giaSuData;

                ConfigureGiaSuColumns();
                ApplyGiaSuRowColors();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadGiaSuList: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải danh sách gia sư: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigureGiaSuColumns()
        {
            if (dgvGiaSu?.Columns == null || dgvGiaSu.Columns.Count == 0)
                return;

            SetColumnProperty(dgvGiaSu, "MaGS", col =>
            {
                col.HeaderText = "Mã";
                col.Width = 50;
            });

            SetColumnProperty(dgvGiaSu, "HoTen", col =>
            {
                col.HeaderText = "Họ Tên";
            });

            SetColumnProperty(dgvGiaSu, "ChuyenMon", col =>
            {
                col.HeaderText = "Chuyên Môn";
            });

            SetColumnProperty(dgvGiaSu, "TrangThai", col =>
            {
                col.HeaderText = "Trạng Thái";
                col.Width = 100;
            });

            SetColumnProperty(dgvGiaSu, "SoLop", col =>
            {
                col.HeaderText = "Số Lớp";
                col.Width = 70;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            });
        }

        private void ApplyGiaSuRowColors()
        {
            foreach (DataGridViewRow row in dgvGiaSu.Rows)
            {
                if (row.Cells["TrangThai"] != null && row.Cells["TrangThai"].Value != null)
                {
                    var trangThai = row.Cells["TrangThai"].Value.ToString();
                    if (trangThai == Constants.GiaSuStatus.Ban)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 224);
                    else if (trangThai == Constants.GiaSuStatus.Ranh)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(212, 239, 223);
                }
            }
        }

        // ===================================================================
        // CHI TIẾT GIA SƯ
        // ===================================================================

        private void dgvGiaSu_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvGiaSu.CurrentRow == null) return;

            try
            {
                if (dgvGiaSu.CurrentRow.Cells["MaGS"] == null ||
                    dgvGiaSu.CurrentRow.Cells["MaGS"].Value == null)
                    return;

                int maGS = Convert.ToInt32(dgvGiaSu.CurrentRow.Cells["MaGS"].Value);
                LoadGiaSuDetails(maGS);
                LoadLopHocByGiaSu(maGS);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi dgvGiaSu_SelectionChanged: {ex.Message}");
            }
        }

        private void LoadGiaSuDetails(int maGS)
        {
            try
            {
                var giaSuService = new GiaSuService();
                var giaSu = giaSuService.GetById(maGS);

                if (giaSu != null)
                {
                    txtMaGS.Text = giaSu.MaGS.ToString();
                    txtHoTen.Text = giaSu.HoTen ?? "";
                    txtChuyenMon.Text = giaSu.ChuyenMon ?? "";
                    txtTrangThai.Text = giaSu.TrangThai ?? Constants.GiaSuStatus.Ranh;

                    if (giaSu.TrangThai == Constants.GiaSuStatus.Ban)
                    {
                        txtTrangThai.BackColor = Constants.Colors.Red;
                        txtTrangThai.ForeColor = Color.White;
                    }
                    else
                    {
                        txtTrangThai.BackColor = Constants.Colors.Green;
                        txtTrangThai.ForeColor = Color.White;
                    }
                }

                giaSuService.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadGiaSuDetails: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải chi tiết: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===================================================================
        // LỚP HỌC CỦA GIA SƯ
        // ===================================================================

        private void LoadLopHocByGiaSu(int maGS)
        {
            try
            {
                var lopHocData = _service.GetLopHocByGiaSu(maGS);
                dgvLopHoc.DataSource = lopHocData;

                var activeLopCount = _service.CountActiveLopHocByGiaSu(maGS);
                txtSoLop.Text = activeLopCount.ToString();

                ConfigureLopHocColumns();
                ApplyLopHocRowColors();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadLopHocByGiaSu: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải danh sách lớp học: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConfigureLopHocColumns()
        {
            if (dgvLopHoc?.Columns == null || dgvLopHoc.Columns.Count == 0)
                return;

            SetColumnProperty(dgvLopHoc, "MaLop", col =>
            {
                col.HeaderText = "Mã Lớp";
                col.Width = 60;
            });

            SetColumnProperty(dgvLopHoc, "HocVien", col =>
            {
                col.HeaderText = "Học Viên";
            });

            SetColumnProperty(dgvLopHoc, "MonHoc", col =>
            {
                col.HeaderText = "Môn Học";
            });

            SetColumnProperty(dgvLopHoc, "HocPhi", col =>
            {
                col.HeaderText = "Học Phí";
                col.DefaultCellStyle.Format = "N0";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            });

            SetColumnProperty(dgvLopHoc, "TrangThai", col =>
            {
                col.HeaderText = "Trạng Thái";
                col.Width = 120;
            });

            SetColumnProperty(dgvLopHoc, "CaHoc", col =>
            {
                col.HeaderText = "Ca Học";
                col.Width = 120;
            });
        }

        private void ApplyLopHocRowColors()
        {
            foreach (DataGridViewRow row in dgvLopHoc.Rows)
            {
                if (row.Cells["TrangThai"] != null && row.Cells["TrangThai"].Value != null)
                {
                    var trangThai = row.Cells["TrangThai"].Value.ToString();
                    if (trangThai == Constants.LopHocStatus.DangDienRa)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(212, 239, 223);
                    else if (trangThai == Constants.LopHocStatus.DaKetThuc)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241);
                    else if (trangThai == Constants.LopHocStatus.ChuaBatDau)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 224);
                }
            }
        }

        // ===================================================================
        // ✨ THỐNG KÊ THEO CA HỌC - TÍNH NĂNG MỚI
        // ===================================================================

        private void LoadThongKeCaHoc()
        {
            try
            {
                var thongKeCa = _lopHocService.GetAll()
                    .Where(l => l.DangHoatDong && !string.IsNullOrEmpty(l.CaHoc))
                    .GroupBy(l => l.CaHoc)
                    .Select(g => new
                    {
                        CaHoc = g.Key,
                        SoLop = g.Count(),
                        SoGiaSu = g.Select(l => l.MaGS).Distinct().Count(),
                        TongHocPhi = g.Sum(l => l.HocPhi)
                    })
                    .OrderByDescending(x => x.SoLop)
                    .ToList();

                dgvThongKeCa.DataSource = thongKeCa;

                if (dgvThongKeCa?.Columns == null || dgvThongKeCa.Columns.Count == 0)
                    return;

                SetColumnProperty(dgvThongKeCa, "CaHoc", col =>
                {
                    col.HeaderText = "Ca Học";
                    col.Width = 150;
                });

                SetColumnProperty(dgvThongKeCa, "SoLop", col =>
                {
                    col.HeaderText = "Số Lớp";
                    col.Width = 70;
                });

                SetColumnProperty(dgvThongKeCa, "SoGiaSu", col =>
                {
                    col.HeaderText = "Số GS";
                    col.Width = 70;
                });

                SetColumnProperty(dgvThongKeCa, "TongHocPhi", col =>
                {
                    col.HeaderText = "Tổng Học Phí";
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadThongKeCaHoc: {ex.Message}");
            }
        }

        // ===================================================================
        // ✨ LỚP SẮP BẮT ĐẦU (7 NGÀY TỚI)
        // ===================================================================

        private void LoadLopSapBatDau()
        {
            try
            {
                var lopSapBatDau = _lopHocService.GetUpcomingLopHoc(7)
                    .Select(l => new
                    {
                        l.MaLop,
                        GiaSu = l.GiaSu?.HoTen ?? "N/A",
                        HocVien = l.HocVien?.HoTen ?? "N/A",
                        l.MonHoc,
                        l.CaHoc,
                        BatDau = l.ThoiGianBatDau.HasValue ? l.ThoiGianBatDau.Value.ToString("dd/MM HH:mm") : "",
                        SoNgayConLai = l.ThoiGianBatDau.HasValue
                            ? (int)(l.ThoiGianBatDau.Value - DateTime.Now).TotalDays
                            : 0,
                        l.HocPhi
                    })
                    .OrderBy(l => l.SoNgayConLai)
                    .ToList();

                dgvSapBatDau.DataSource = lopSapBatDau;

                if (dgvSapBatDau?.Columns == null || dgvSapBatDau.Columns.Count == 0)
                    return;

                SetColumnProperty(dgvSapBatDau, "MaLop", col =>
                {
                    col.HeaderText = "Mã";
                    col.Width = 50;
                });

                SetColumnProperty(dgvSapBatDau, "GiaSu", col =>
                {
                    col.Width = 100;
                });

                SetColumnProperty(dgvSapBatDau, "HocVien", col =>
                {
                    col.Width = 100;
                });

                SetColumnProperty(dgvSapBatDau, "MonHoc", col =>
                {
                    col.Width = 70;
                });

                SetColumnProperty(dgvSapBatDau, "CaHoc", col =>
                {
                    col.Width = 120;
                });

                SetColumnProperty(dgvSapBatDau, "BatDau", col =>
                {
                    col.Width = 90;
                });

                SetColumnProperty(dgvSapBatDau, "SoNgayConLai", col =>
                {
                    col.HeaderText = "Còn (ngày)";
                    col.Width = 80;
                });

                SetColumnProperty(dgvSapBatDau, "HocPhi", col =>
                {
                    col.HeaderText = "Học Phí";
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                });

                // Highlight lớp sắp bắt đầu trong 24h
                foreach (DataGridViewRow row in dgvSapBatDau.Rows)
                {
                    if (row.Cells["SoNgayConLai"]?.Value != null)
                    {
                        int soNgay = Convert.ToInt32(row.Cells["SoNgayConLai"].Value);
                        if (soNgay <= 1)
                        {
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 224);
                            row.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadLopSapBatDau: {ex.Message}");
            }
        }

        // ===================================================================
        // ✨ LỚP SẮP KẾT THÚC (7 NGÀY TỚI)
        // ===================================================================

        private void LoadLopSapKetThuc()
        {
            try
            {
                var lopSapKetThuc = _lopHocService.GetAll()
                    .Where(l => l.TrangThai == Constants.LopHocStatus.DangDienRa &&
                                l.ThoiGianKetThuc.HasValue &&
                                l.ThoiGianKetThuc.Value >= DateTime.Now &&
                                l.ThoiGianKetThuc.Value <= DateTime.Now.AddDays(7))
                    .Select(l => new
                    {
                        l.MaLop,
                        GiaSu = l.GiaSu?.HoTen ?? "N/A",
                        HocVien = l.HocVien?.HoTen ?? "N/A",
                        l.MonHoc,
                        l.CaHoc,
                        KetThuc = l.ThoiGianKetThuc.HasValue ? l.ThoiGianKetThuc.Value.ToString("dd/MM HH:mm") : "",
                        SoNgayConLai = (int)(l.ThoiGianKetThuc.Value - DateTime.Now).TotalDays
                    })
                    .OrderBy(l => l.SoNgayConLai)
                    .ToList();

                dgvSapKetThuc.DataSource = lopSapKetThuc;

                if (dgvSapKetThuc?.Columns == null || dgvSapKetThuc.Columns.Count == 0)
                    return;

                SetColumnProperty(dgvSapKetThuc, "MaLop", col =>
                {
                    col.HeaderText = "Mã";
                    col.Width = 50;
                });

                SetColumnProperty(dgvSapKetThuc, "GiaSu", col =>
                {
                    col.HeaderText = "Gia Sư";
                });

                SetColumnProperty(dgvSapKetThuc, "HocVien", col =>
                {
                    col.HeaderText = "Học Viên";
                });

                SetColumnProperty(dgvSapKetThuc, "MonHoc", col =>
                {
                    col.HeaderText = "Môn Học";
                });

                SetColumnProperty(dgvSapKetThuc, "CaHoc", col =>
                {
                    col.HeaderText = "Ca Học";
                });

                SetColumnProperty(dgvSapKetThuc, "KetThuc", col =>
                {
                    col.HeaderText = "Kết Thúc";
                });

                SetColumnProperty(dgvSapKetThuc, "SoNgayConLai", col =>
                {
                    col.HeaderText = "Còn (ngày)";
                    col.Width = 80;
                });

                // Highlight lớp sắp kết thúc trong 24h
                foreach (DataGridViewRow row in dgvSapKetThuc.Rows)
                {
                    if (row.Cells["SoNgayConLai"]?.Value != null)
                    {
                        int soNgay = Convert.ToInt32(row.Cells["SoNgayConLai"].Value);
                        if (soNgay <= 1)
                        {
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 224);
                            row.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadLopSapKetThuc: {ex.Message}");
            }
        }

        // ===================================================================
        // EVENTS
        // ===================================================================

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                _service?.Dispose();
                _lopHocService?.Dispose();

                _service = new ThongKeService();
                _lopHocService = new LopHocService();

                RefreshAll();

                MessageBox.Show(Constants.Messages.LamMoiThanhCong,
                    Constants.Messages.InfoTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi btnRefresh_Click: {ex.Message}");
                MessageBox.Show($"Lỗi khi làm mới: {ex.Message}",
                    Constants.Messages.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void FrmThongKe_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
                _service?.Dispose();
                _lopHocService?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi FrmThongKe_FormClosed: {ex.Message}");
            }
        }

    }
}