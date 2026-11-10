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
using DoAnWD.UI.Dialogs;

namespace DoAnWD.UI.Pages
{
    /// <summary>
    /// ✨ Form Thống Kê - CẬP NHẬT CHO OPTION 2
    /// Bổ sung: Thống kê ca học, lớp sắp bắt đầu/kết thúc, auto-refresh
    /// </summary>
    public partial class frmThongKe : Form, ILamMoi
    {
        public void LamMoi() { RefreshAll(); }

        private ThongKeService _service;
        private LopHocService _lopHocService;
        private Timer _refreshTimer;
        private Label lblDoanhThu;
        private List<LopHoc> _lopHoatDong = new List<LopHoc>();

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
            // Tiêu đề cột khai báo một lần; tự áp lại mỗi khi bảng nạp dữ liệu (kể cả ở tab chưa mở)
            ThemeManager.TieuDeCot(dgvGiaSu, "MaGS", "Mã", "HoTen", "Họ tên", "ChuyenMon", "Chuyên môn", "TrangThai", "Trạng thái", "SoLop", "Số lớp");
            ThemeManager.TieuDeCot(dgvLopHoc, "MaLop", null, "TenLop", "Tên lớp", "HocVien", "Học viên", "MonHoc", "Môn học", "CaHoc", "Ca học",
                "BatDau", "Bắt đầu", "KetThuc", "Kết thúc", "HocPhi", "Học phí", "TrangThai", "Trạng thái");
            ThemeManager.TieuDeCot(dgvThongKeCa, "CaHoc", "Ca học", "SoLop", "Số lớp", "SoGiaSu", "Số gia sư", "TongHocPhi", "Tổng học phí");
            // Hai bảng "sắp ..." bỏ cột mã lớp để đủ chỗ cho tên lớp và số ngày còn lại
            ThemeManager.TieuDeCot(dgvSapBatDau, "MaLop", null, "Gap", null, "TenLop", "Lớp", "GiaSu", "Gia sư", "CaHoc", "Ca",
                "Ngay", "Bắt đầu", "SoNgayConLai", "Còn", "HocPhi", "Học phí");
            ThemeManager.TieuDeCot(dgvSapKetThuc, "MaLop", null, "Gap", null, "TenLop", "Lớp", "GiaSu", "Gia sư", "CaHoc", "Ca",
                "Ngay", "Kết thúc", "SoNgayConLai", "Còn", "HocPhi", "Học phí");
            dgvSapBatDau.DataBindingComplete += (s, ev) => ToDongGap(dgvSapBatDau);
            dgvSapKetThuc.DataBindingComplete += (s, ev) => ToDongGap(dgvSapKetThuc);
            // Bấm tiêu đề cột ở mọi bảng để sắp xếp (tên người xếp theo tên gọi)
            foreach (var g in new[] { dgvGiaSu, dgvLopHoc, dgvThongKeCa, dgvSapBatDau, dgvSapKetThuc }) new SapXepLoc(g);

            InitializeDoanhThuLabel();
            lblLabelLopDangHoc.Text = "HV đang học";
            GanBamChuyenTrang();
            tabControl.SizeChanged += (s, ev) => SapXepBoCuc();
            SapXepBoCuc();
            RefreshAll();
            InitializeAutoRefresh();
        }

        // ===================================================================
        // BẤM Ô SỐ LIỆU => CHUYỂN TỚI TRANG LIÊN QUAN (đã lọc sẵn)
        // ===================================================================

        private void GanBamChuyenTrang()
        {
            Gan(cardTongGS, "Mở trang Gia sư", () => DieuHuong.Mo<frmGiaSu>()?.LocTrangThai(null));
            Gan(cardTongHV, "Mở trang Học viên", () => DieuHuong.Mo<frmHocVien>()?.LocTinhTrang("Tất cả"));
            Gan(cardTongLop, "Mở trang Lớp học", () => DieuHuong.Mo<frmLopHoc>()?.LocTrangThai(frmLopHoc.ConHoatDong));
            if (lblDoanhThu != null)
                Gan(lblDoanhThu, "Mở trang Học phí", () => DieuHuong.Mo<frmHocPhi>());

            Gan(new Control[] { lblCountGSRanh, lblLabelGSRanh }, "Gia sư đang rảnh",
                () => DieuHuong.Mo<frmGiaSu>()?.LocTrangThai(Constants.GiaSuStatus.Ranh));
            Gan(new Control[] { lblCountGSDangDay, lblLabelGSBan }, "Gia sư đang có lớp",
                () => DieuHuong.Mo<frmGiaSu>()?.LocTrangThai(frmGiaSu.DangCoLop));
            Gan(new Control[] { lblCountChuaBatDau, lblLabelChuaBatDau }, "Lớp chưa bắt đầu",
                () => DieuHuong.Mo<frmLopHoc>()?.LocTrangThai(Constants.LopHocStatus.ChuaBatDau));
            Gan(new Control[] { lblCountDangDienRa, lblLabelDangDienRa }, "Lớp đang diễn ra",
                () => DieuHuong.Mo<frmLopHoc>()?.LocTrangThai(Constants.LopHocStatus.DangDienRa));
            Gan(new Control[] { lblCountDaKetThuc, lblLabelDaKetThuc }, "Lớp đã kết thúc",
                () => DieuHuong.Mo<frmLopHoc>()?.LocTrangThai(Constants.LopHocStatus.DaKetThuc));
            Gan(new Control[] { lblCountLopDangHoc, lblLabelLopDangHoc }, "Học viên đang học",
                () => DieuHuong.Mo<frmHocVien>()?.LocTinhTrang("Đang học"));
        }

        private readonly ToolTip _goiY = new ToolTip();

        /// <summary>Cả khối (và mọi chữ bên trong) bấm được, con trỏ hình bàn tay, có chú thích</summary>
        private void Gan(Control khoi, string chuThich, Action viec) => Gan(new[] { khoi }, chuThich, viec);

        private void Gan(Control[] cacKhoi, string chuThich, Action viec)
        {
            foreach (var k in cacKhoi)
            {
                foreach (var c in new[] { k }.Concat(k.Controls.Cast<Control>()))
                {
                    if (c == lblDoanhThu && k != lblDoanhThu) continue;   // ô học phí có đích riêng
                    c.Cursor = Cursors.Hand;
                    c.Click += (s, e) => viec();
                    _goiY.SetToolTip(c, chuThich + " (bấm để xem)");
                }
            }
        }

        // ===================================================================
        // ✨ AUTO-REFRESH TIMER
        // ===================================================================

        private void InitializeAutoRefresh()
        {
            _refreshTimer = new Timer();
            _refreshTimer.Interval = 120000; // 2 phút: số liệu tổng quan không cần cập nhật dày hơn
            _refreshTimer.Tick += (s, ev) => { if (Visible) RefreshAll(); };
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
            // Trừ sẵn bề rộng thanh cuộn dọc của tab để khối bên phải không bị cắt mất cạnh
            int w = Math.Max(900, tabLopHoc.ClientSize.Width - 2 * le - SystemInformation.VerticalScrollBarWidth);

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
            // Thống kê theo ca chỉ 4 cột => hẹp; lớp sắp bắt đầu nhiều cột => rộng hơn
            int caW = Math.Max(440, (int)(w * 0.36));
            gbThongKeCa.SetBounds(le, gbThongKeCa.Top, caW, gbThongKeCa.Height);
            gbSapBatDau.SetBounds(le + caW + khe, gbSapBatDau.Top, w - caW - khe, gbSapBatDau.Height);
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

                // Lớp còn hoạt động đọc MỘT lần, dùng chung cho 3 bảng của tab Lớp học
                _lopHoatDong = _lopHocService.GetLopConHoatDong();
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
                lblCountLopDangHoc.Text = stats.HocVienDangHoc.ToString();

                if (lblDoanhThu != null)
                {
                    var vi = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
                    lblDoanhThu.Text =
                        $"Học phí đang thu: {stats.DoanhThuDangHoatDong.ToString("N0", vi)} đ\n" +
                        $"Tháng này: {stats.DoanhThuThangNay.ToString("N0", vi)} đ\n" +
                        $"Còn phải thu: {ConPhaiThu().ToString("N0", vi)} đ";
                }

                lblCountChuaBatDau.Text = stats.LopChuaBatDau.ToString();
                lblCountDangDienRa.Text = stats.LopDangDienRa.ToString();
                lblCountDaKetThuc.Text = stats.LopDaKetThuc.ToString();
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

                txtSoLop.Text = _service.CountActiveLopHocByGiaSu(maGS).ToString();
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

        // ===================================================================
        // ✨ THỐNG KÊ THEO CA HỌC - TÍNH NĂNG MỚI
        // ===================================================================

        private void LoadThongKeCaHoc()
        {
            try
            {
                // Đủ 6 ca theo thứ tự trong ngày (ca chưa có lớp hiện số 0 để thấy ngay ca còn trống)
                var lop = _lopHoatDong.Where(l => !string.IsNullOrEmpty(l.CaHoc)).ToList();
                var thongKeCa = Constants.CaHoc.TatCaCaHoc
                    .Select(ca =>
                    {
                        var g = lop.Where(l => l.CaHoc == ca).ToList();
                        return new
                        {
                            CaHoc = ca,
                            SoLop = g.Count,
                            SoGiaSu = g.Select(l => l.MaGS).Distinct().Count(),
                            TongHocPhi = g.Sum(l => l.HocPhi ?? 0)
                        };
                    })
                    .ToList();

                dgvThongKeCa.DataSource = thongKeCa;
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
            var now = DateTime.Now;
            NapLopSap(dgvSapBatDau, _lopHoatDong.Where(l => l.ThoiGianBatDau >= now && l.ThoiGianBatDau <= now.AddDays(7)),
                l => l.ThoiGianBatDau);
        }

        private void LoadLopSapKetThuc()
        {
            var now = DateTime.Now;
            NapLopSap(dgvSapKetThuc, _lopHoatDong.Where(l => l.TrangThai == Constants.LopHocStatus.DangDienRa &&
                                                         l.ThoiGianKetThuc >= now && l.ThoiGianKetThuc <= now.AddDays(7)),
                l => l.ThoiGianKetThuc);
        }

        /// <summary>Bảng lớp sắp bắt đầu / sắp kết thúc: cột Ngay = mốc (bắt đầu hoặc kết thúc), Gap = trong 24 giờ tới</summary>
        private static void NapLopSap(DataGridView dgv, IEnumerable<LopHoc> lops, Func<LopHoc, DateTime?> layMoc)
        {
            dgv.DataSource = lops.Where(l => layMoc(l).HasValue)
                .OrderBy(l => layMoc(l))
                .Select(l =>
                {
                    var moc = layMoc(l).Value;
                    return new
                    {
                        l.MaLop,
                        TenLop = l.TenHienThi,
                        GiaSu = l.GiaSu?.HoTen ?? "",
                        CaHoc = Constants.CaHoc.TenNgan(l.CaHoc),
                        Ngay = moc.ToString("dd/MM"),
                        SoNgayConLai = ConBaoLau(moc),
                        l.HocPhi,
                        Gap = (moc - DateTime.Now).TotalHours <= 24
                    };
                })
                .ToList();
        }

        /// <summary>"Hôm nay", "Ngày mai", "3 ngày"... dễ đọc hơn số ngày bị làm tròn xuống 0</summary>
        private static string ConBaoLau(DateTime moc)
        {
            int ngay = (moc.Date - DateTime.Today).Days;
            if (ngay <= 0) return "Hôm nay";
            if (ngay == 1) return "Ngày mai";
            return ngay + " ngày";
        }

        /// <summary>Tô nền vàng nhạt cho lớp diễn ra trong vòng 24 giờ (theo cột Gap, đúng cả khi đã sắp xếp lại)</summary>
        private static void ToDongGap(DataGridView dgv)
        {
            if (!dgv.Columns.Contains("Gap")) return;
            foreach (DataGridViewRow r in dgv.Rows)
                if (r.Cells["Gap"].Value is bool && (bool)r.Cells["Gap"].Value)
                {
                    r.DefaultCellStyle.BackColor = DoAnWD.UI.Theme.WarningSoft;
                    r.DefaultCellStyle.Font = DoAnWD.UI.Theme.BodyStrong;
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