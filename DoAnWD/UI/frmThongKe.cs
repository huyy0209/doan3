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
            tabControl.SizeChanged += (s, ev) => SapXepBoCuc();
            SapXepBoCuc();
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
        /// Co giãn các khối theo bề ngang cửa sổ (Designer đặt kích thước cố định 1400px,
        /// màn hình 1366px sẽ bị cắt mất bên phải)
        /// </summary>
        private void SapXepBoCuc()
        {
            const int le = 20, khe = 16;
            int w = Math.Max(900, tabControl.ClientSize.Width - 2 * le - 8);

            // Tab Tổng quan: 3 ô số liệu + 2 khối trạng thái
            panelCards.Width = w;
            int cardW = (w - 2 * khe) / 3;
            cardTongGS.SetBounds(0, 0, cardW, cardTongGS.Height);
            cardTongHV.SetBounds(cardW + khe, 0, cardW, cardTongHV.Height);
            cardTongLop.SetBounds(2 * (cardW + khe), 0, w - 2 * (cardW + khe), cardTongLop.Height);

            int half = (w - khe) / 2;
            gbTrangThaiGS.SetBounds(le, gbTrangThaiGS.Top, half, gbTrangThaiGS.Height);
            gbTrangThaiLop.SetBounds(le + half + khe, gbTrangThaiLop.Top, w - half - khe, gbTrangThaiLop.Height);
            ChiaCot(gbTrangThaiGS, new[] { lblCountGSRanh, lblCountGSDangDay }, new[] { lblLabelGSRanh, lblLabelGSBan });
            // Số theo màu trạng thái để nhìn là phân biệt được
            lblCountGSRanh.ForeColor = DoAnWD.UI.Theme.Success;
            lblCountGSDangDay.ForeColor = DoAnWD.UI.Theme.Ink;
            lblCountChuaBatDau.ForeColor = DoAnWD.UI.Theme.Warning;
            lblCountDangDienRa.ForeColor = DoAnWD.UI.Theme.Ink;
            lblCountDaKetThuc.ForeColor = DoAnWD.UI.Theme.Neutral;
            lblCountLopDangHoc.ForeColor = DoAnWD.UI.Theme.Success;
            ChiaCot(gbTrangThaiLop,
                new[] { lblCountChuaBatDau, lblCountDangDienRa, lblCountDaKetThuc, lblCountLopDangHoc },
                new[] { lblLabelChuaBatDau, lblLabelDangDienRa, lblLabelDaKetThuc, lblLabelLopDangHoc });

            // Tab Gia sư: danh sách + chi tiết bên phải
            int chiTietW = 420;
            gbDanhSachGS.SetBounds(le, gbDanhSachGS.Top, w - chiTietW - khe, gbDanhSachGS.Height);
            gbChiTietGS.SetBounds(le + w - chiTietW, gbChiTietGS.Top, chiTietW, gbChiTietGS.Height);
            gbLopHocGS.SetBounds(le, gbLopHocGS.Top, w, gbLopHocGS.Height);

            // Tab Lớp học
            int sapW = 440;
            gbThongKeCa.SetBounds(le, gbThongKeCa.Top, w - sapW - khe, gbThongKeCa.Height);
            gbSapBatDau.SetBounds(le + w - sapW, gbSapBatDau.Top, sapW, gbSapBatDau.Height);
            gbSapKetThuc.SetBounds(le, gbSapKetThuc.Top, w, gbSapKetThuc.Height);
        }

        private static decimal ConPhaiThu()
        {
            try { return new BLL.Services.HocPhiService().TongConNo(); }
            catch { return 0; } // CSDL chưa nâng cấp v2
        }

        /// <summary>
        /// Chia đều các cặp (số, nhãn) thành cột trong một khối: số ở trên, nhãn ở dưới
        /// </summary>
        private static void ChiaCot(Control khoi, Label[] so, Label[] nhan)
        {
            int n = so.Length;
            int colW = (khoi.Width - 32) / n;
            for (int i = 0; i < n; i++)
            {
                int x = 16 + i * colW;
                so[i].AutoSize = false;
                so[i].TextAlign = ContentAlignment.MiddleLeft;
                so[i].SetBounds(x, 38, colW - 8, 52);
                nhan[i].AutoSize = false;
                nhan[i].TextAlign = ContentAlignment.TopLeft;
                nhan[i].Text = nhan[i].Text.TrimEnd(':');
                nhan[i].ForeColor = DoAnWD.UI.Theme.TextMuted;
                nhan[i].SetBounds(x + 2, 92, colW - 8, 24);
            }
        }

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
                Font = DoAnWD.UI.Theme.Small,
                ForeColor = DoAnWD.UI.Theme.TextMuted,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
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
                        $"Học phí đang thu: {stats.DoanhThuDangHoatDong.ToString("N0", vi)} đ\n" +
                        $"Tháng này: {stats.DoanhThuThangNay.ToString("N0", vi)} đ\n" +
                        $"Còn phải thu: {ConPhaiThu().ToString("N0", vi)} đ";
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
                col.HeaderText = "Họ tên";
            });

            SetColumnProperty(dgvGiaSu, "ChuyenMon", col =>
            {
                col.HeaderText = "Chuyên môn";
            });

            SetColumnProperty(dgvGiaSu, "TrangThai", col =>
            {
                col.HeaderText = "Trạng thái";
                col.Width = 100;
            });

            SetColumnProperty(dgvGiaSu, "SoLop", col =>
            {
                col.HeaderText = "Số lớp";
                col.Width = 70;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Font = DoAnWD.UI.Theme.BodyStrong;
            });
        }

        private void ApplyGiaSuRowColors()
        {
            // Trạng thái hiển thị bằng nhãn màu trong cột "Trạng thái" (ThemeManager)
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

                    {
                        txtTrangThai.BackColor = DoAnWD.UI.Theme.StatusColor(txtTrangThai.Text);
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
                col.HeaderText = "Mã lớp";
                col.Width = 60;
            });

            SetColumnProperty(dgvLopHoc, "HocVien", col =>
            {
                col.HeaderText = "Học viên";
            });

            SetColumnProperty(dgvLopHoc, "MonHoc", col =>
            {
                col.HeaderText = "Môn học";
            });

            SetColumnProperty(dgvLopHoc, "HocPhi", col =>
            {
                col.HeaderText = "Học phí";
                col.DefaultCellStyle.Format = "N0";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            });

            SetColumnProperty(dgvLopHoc, "TrangThai", col =>
            {
                col.HeaderText = "Trạng thái";
                col.Width = 120;
            });

            SetColumnProperty(dgvLopHoc, "CaHoc", col =>
            {
                col.HeaderText = "Ca học";
                col.Width = 120;
            });
        }

        private void ApplyLopHocRowColors()
        {
            // Trạng thái hiển thị bằng nhãn màu trong cột "Trạng thái" (ThemeManager)
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
                    col.HeaderText = "Ca học";
                    col.Width = 150;
                });

                SetColumnProperty(dgvThongKeCa, "SoLop", col =>
                {
                    col.HeaderText = "Số lớp";
                    col.Width = 70;
                });

                SetColumnProperty(dgvThongKeCa, "SoGiaSu", col =>
                {
                    col.HeaderText = "Số GS";
                    col.Width = 70;
                });

                SetColumnProperty(dgvThongKeCa, "TongHocPhi", col =>
                {
                    col.HeaderText = "Tổng học phí";
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
                    col.HeaderText = "Học phí";
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
                            row.DefaultCellStyle.BackColor = DoAnWD.UI.Theme.WarningSoft;
                            row.DefaultCellStyle.Font = DoAnWD.UI.Theme.BodyStrong;
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
                    col.HeaderText = "Gia sư";
                });

                SetColumnProperty(dgvSapKetThuc, "HocVien", col =>
                {
                    col.HeaderText = "Học viên";
                });

                SetColumnProperty(dgvSapKetThuc, "MonHoc", col =>
                {
                    col.HeaderText = "Môn học";
                });

                SetColumnProperty(dgvSapKetThuc, "CaHoc", col =>
                {
                    col.HeaderText = "Ca học";
                });

                SetColumnProperty(dgvSapKetThuc, "KetThuc", col =>
                {
                    col.HeaderText = "Kết thúc";
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
                            row.DefaultCellStyle.BackColor = DoAnWD.UI.Theme.WarningSoft;
                            row.DefaultCellStyle.Font = DoAnWD.UI.Theme.BodyStrong;
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