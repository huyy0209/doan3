using DoAnWD.BLL.Services;
using DoAnWD.Common;
using DoAnWD.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DoAnWD.UI.Controls;
using DoAnWD.UI.Dialogs;
using DoAnWD.UI.Pages;

namespace DoAnWD.UI
{
    /// <summary>
    /// Khung chính của ứng dụng: menu trái màu xanh bảng (tên trung tâm, các trang, người đang đăng nhập),
    /// thanh trên (ngày + dải 6 ca học + lớp sắp bắt đầu), vùng nội dung.
    /// Tự cập nhật trạng thái lớp mỗi phút và báo lớp sắp học mỗi 5 phút.
    /// </summary>
    public partial class TrangChu : Form
    {
        private Form activeChild;
        private NavButton activeMenuButton;
        private NavButton btnHocPhi, btnTaiKhoan, btnCaiDat;
        private Panel pnlNguoiDung;
        private bool _hoverNguoiDung;

        private Timer autoUpdateTimer, notificationTimer;
        private DateTime lastUpdateTime = DateTime.Now;
        private readonly NotificationService _notificationService = new NotificationService();

        private static readonly string[] TenThu =
            { "Chủ nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };

        private static readonly Font FontTenTrungTam = Theme.Chu(10.75F, 2);
        private static readonly Font FontNgay = Theme.Chu(13F, 2);

        public TrangChu()
        {
            InitializeComponent();
            DungKhung();
            TaoMenu();
            KhoiDongHenGio();

            Load += TrangChu_Load;
            FormClosing += TrangChu_FormClosing;
        }

        // ===================================================================
        // KHUNG
        // ===================================================================

        /// <summary>Màu, chữ và các phần tự vẽ của khung (Designer chỉ giữ bố cục)</summary>
        private void DungKhung()
        {
            BackColor = panelHost.BackColor = Theme.Paper;
            panelSidebar.BackColor = panelLogo.BackColor = panelNav.BackColor = panelNavFooter.BackColor = Theme.Board;
            panelTop.BackColor = Theme.Surface;
            Font = Theme.Body;
            CapNhatTenTrungTam();

            // Mục menu tự vẽ dấu đang chọn; trang tự có lề => bỏ vạch trượt và đường lề cũ
            pnlIndicator.Visible = false;
            panelMarginLine.Visible = false;
            lblAppName.Visible = lblAppSub.Visible = false;
            panelSidebar.Width = 252;
            panelLogo.Height = 92;
            panelLogo.Paint += PanelLogo_Paint;
            panelNav.Padding = new Padding(0, 4, 0, 0);

            lblTitle.Font = FontNgay;
            lblTitle.ForeColor = Theme.Text;
            lblSubtitle.Font = Theme.Small;
            lblSubtitle.ForeColor = Theme.TextMuted;
            lblStatus.Font = Theme.Chu(9.75F, 1);
            // Dải ca chiếm phần giữa: sau dòng ngày, trước dòng trạng thái
            EventHandler xepThanhTren = (s, e) =>
            {
                lblStatus.SetBounds(panelTop.Width - 22 - 236, 18, 236, 40);
                shiftRibbon.SetBounds(250, 15, Math.Max(300, panelTop.Width - 250 - 22 - 236 - 16), 46);
            };
            lblStatus.Anchor = shiftRibbon.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            panelTop.Resize += xepThanhTren;
            xepThanhTren(null, EventArgs.Empty);
            panelTop.Paint += (s, e) =>
            {
                using (var p = new Pen(Theme.Rule))
                    e.Graphics.DrawLine(p, 0, panelTop.Height - 1, panelTop.Width, panelTop.Height - 1);
            };

            ThemeManager.EnableDoubleBuffer(panelHost);
            ThemeManager.EnableDoubleBuffer(panelSidebar);
            ThemeManager.EnableDoubleBuffer(panelLogo);
        }

        private void CapNhatTenTrungTam()
        {
            Text = CaiDatService.TrungTam.TenHienThi + " - Quản lý gia sư";
            panelLogo.Invalidate();
        }

        /// <summary>Dấu hiệu: tấm bảng xanh viền phấn vàng, bên cạnh là tên trung tâm</summary>
        private void PanelLogo_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var o = new RectangleF(20, 24, 40, 40);
            using (var path = Theme.RoundedRect(o, 11))
            using (var b = new SolidBrush(Theme.Chalk))
                g.FillPath(b, path);
            Icons.Draw(g, Glyph.Book, new RectangleF(o.X + 9, o.Y + 9, 22, 22), Theme.InkDarker, 1.9f);

            // Tên trung tâm: vừa một dòng thì kèm dòng "Quản lý gia sư", dài thì xuống hai dòng
            string tenTT = CaiDatService.TrungTam.TenHienThi;
            int rong = panelLogo.Width - 72 - 14;
            bool motDong = TextRenderer.MeasureText(g, tenTT, FontTenTrungTam, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width <= rong;
            if (motDong)
            {
                TextRenderer.DrawText(g, tenTT, FontTenTrungTam, new Rectangle(72, 22, rong, 24), Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, "Quản lý gia sư", Theme.Small, new Rectangle(72, 46, rong, 20), Theme.InkMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
            else
                TextRenderer.DrawText(g, tenTT, FontTenTrungTam, new Rectangle(72, 20, rong, 48), Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            using (var p = new Pen(Theme.BoardLine))
                g.DrawLine(p, 20, panelLogo.Height - 1, panelLogo.Width - 20, panelLogo.Height - 1);
        }

        // ===================================================================
        // MENU
        // ===================================================================

        /// <summary>
        /// Các mục menu và phân quyền:
        ///  - Quản lý: mọi mục + Cài đặt
        ///  - Gia sư: Học viên (điểm danh, điểm, học bù) và Lịch học của mình
        /// </summary>
        private void TaoMenu()
        {
            btnThongKe.Glyph = Glyph.Home;
            btnGhepLop.Glyph = Glyph.Book;
            btnHocVien.Glyph = Glyph.Users;
            btnCalendar.Glyph = Glyph.Calendar;
            btnGiaSu.Glyph = Glyph.User;
            btnRefresh.Glyph = Glyph.Refresh;
            btnGhepLop.Text = "Lớp học";
            btnHocVien.Text = "Học viên & điểm";   // gộp điểm danh + điểm + học bù

            btnHocPhi = new NavButton { Text = "Học phí", Glyph = Glyph.Money, Name = "btnHocPhi" };
            btnTaiKhoan = new NavButton { Text = "Tài khoản", Glyph = Glyph.Key, Name = "btnTaiKhoan" };
            btnHocPhi.Click += (s, e) => { ChonMenu(btnHocPhi); MoTrang<frmHocPhi>(); };
            btnTaiKhoan.Click += (s, e) => { ChonMenu(btnTaiKhoan); MoTrang<frmTaiKhoan>(); };

            // Thứ tự từ trên xuống (Dock Top: thêm sau nằm trên => thêm ngược)
            var thuTu = new[] { btnThongKe, btnGhepLop, btnHocVien, btnCalendar, btnHocPhi, btnGiaSu, btnTaiKhoan };
            panelNav.SuspendLayout();
            foreach (var b in thuTu) panelNav.Controls.Remove(b);
            for (int i = thuTu.Length - 1; i >= 0; i--) panelNav.Controls.Add(thuTu[i]);
            panelNav.ResumeLayout(true);

            _menuCuaTrang = new Dictionary<Type, NavButton>
            {
                { typeof(frmTongQuan), btnThongKe }, { typeof(frmLopHoc), btnGhepLop }, { typeof(frmHocVien), btnHocVien },
                { typeof(frmCalendarView), btnCalendar }, { typeof(frmHocPhi), btnHocPhi }, { typeof(frmGiaSu), btnGiaSu },
                { typeof(frmTaiKhoan), btnTaiKhoan }
            };
            DieuHuong.MoTrang = MoTrangTuNoiKhac;

            bool quanLy = !PhienDangNhap.DaDangNhap || PhienDangNhap.LaQuanLy;
            if (!quanLy)
                foreach (var b in new[] { btnThongKe, btnGhepLop, btnHocPhi, btnGiaSu, btnTaiKhoan })
                    b.Visible = false;

            // Chân menu: Cài đặt (quản lý), Làm mới, rồi tới người đang đăng nhập
            btnCaiDat = new NavButton { Text = "Cài đặt trung tâm", Glyph = Glyph.Settings, Name = "btnCaiDat", Visible = quanLy };
            btnCaiDat.Click += (s, e) =>
            {
                using (var f = new dlgCaiDat())
                    if (f.ShowDialog(this) == DialogResult.OK) CapNhatTenTrungTam();
            };
            btnExit.Visible = false;   // Đăng xuất nằm trong ô người dùng
            panelNavFooter.Padding = Padding.Empty;

            pnlNguoiDung = new Panel { Dock = DockStyle.Bottom, Height = 66, BackColor = Theme.InkDarker, Cursor = Cursors.Hand };
            ThemeManager.EnableDoubleBuffer(pnlNguoiDung);
            pnlNguoiDung.Paint += PnlNguoiDung_Paint;
            pnlNguoiDung.MouseEnter += (s, e) => { _hoverNguoiDung = true; pnlNguoiDung.Invalidate(); };
            pnlNguoiDung.MouseLeave += (s, e) => { _hoverNguoiDung = false; pnlNguoiDung.Invalidate(); };
            pnlNguoiDung.Click += (s, e) => MoMenuNguoiDung();
            new ToolTip().SetToolTip(pnlNguoiDung, "Đổi mật khẩu, đăng xuất");

            panelNavFooter.Controls.Add(btnCaiDat);
            panelNavFooter.Controls.Add(pnlNguoiDung);
            // Dock: control đứng cuối danh sách được xếp trước => ô người dùng sát đáy, các nút xếp phía trên
            panelNavFooter.Controls.SetChildIndex(pnlNguoiDung, panelNavFooter.Controls.Count - 1);
            btnRefresh.Dock = btnCaiDat.Dock = DockStyle.Bottom;
            panelNavFooter.Controls.SetChildIndex(btnRefresh, 0);
            panelNavFooter.Controls.SetChildIndex(btnCaiDat, 1);
            panelNavFooter.Height = pnlNguoiDung.Height + (quanLy ? 2 : 1) * btnRefresh.Height + 8;
        }

        private void PnlNguoiDung_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (_hoverNguoiDung) g.Clear(Theme.Blend(Theme.InkDarker, Color.White, 0.06f));

            bool quanLy = !PhienDangNhap.DaDangNhap || PhienDangNhap.LaQuanLy;
            string ten = quanLy ? (PhienDangNhap.TenDangNhap ?? "admin") : (PhienDangNhap.TenGiaSu ?? PhienDangNhap.TenDangNhap);
            string chuDau = ten.Trim().Length > 0 ? ten.Trim().Substring(ten.Trim().LastIndexOf(' ') + 1, 1).ToUpper() : "?";

            var avatar = new RectangleF(18, 14, 38, 38);
            using (var b = new SolidBrush(Theme.Chalk)) g.FillEllipse(b, avatar);
            TextRenderer.DrawText(g, chuDau, Theme.Section, Rectangle.Round(avatar), Theme.InkDarker,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            int rong = pnlNguoiDung.Width - 70 - 34;
            TextRenderer.DrawText(g, ten, Theme.BodyStrong, new Rectangle(66, 13, rong, 22), Color.White,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, quanLy ? "Quản lý" : "Gia sư", Theme.Small, new Rectangle(66, 33, rong, 20), Theme.InkMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            Icons.Draw(g, Glyph.ChevronDown, new RectangleF(pnlNguoiDung.Width - 34, 26, 14, 14), Theme.InkMuted);
        }

        private void MoMenuNguoiDung()
        {
            var menu = new ContextMenuStrip { Font = Theme.Body, ShowImageMargin = false };
            menu.Items.Add("Đổi mật khẩu", null, (s, e) =>
            {
                using (var f = new frmDoiMatKhau(PhienDangNhap.MaTK, PhienDangNhap.TenDangNhap)) f.ShowDialog(this);
            });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Đăng xuất", null, btnExit_Click);
            foreach (ToolStripItem i in menu.Items) i.Padding = new Padding(8, 5, 8, 5);
            menu.Closed += (s, e) => BeginInvoke((MethodInvoker)menu.Dispose);
            menu.Show(pnlNguoiDung, new Point(14, -menu.GetPreferredSize(Size.Empty).Height - 4));
        }

        private void ChonMenu(NavButton btn)
        {
            if (activeMenuButton != null) activeMenuButton.Active = false;
            activeMenuButton = btn;
            btn.Active = true;
        }

        // ===================================================================
        // THANH TRÊN + TỰ CẬP NHẬT
        // ===================================================================

        private void KhoiDongHenGio()
        {
            autoUpdateTimer = new Timer { Interval = Constants.Timer.AutoUpdateInterval };   // 60 giây
            autoUpdateTimer.Tick += (s, e) =>
            {
                try
                {
                    int soLopDoi = ChayCapNhatTrangThai();
                    lastUpdateTime = DateTime.Now;
                    CapNhatThanhTrangThai();
                    if (soLopDoi > 0) NapLaiTrangChiXem();
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[TIMER] " + ex.Message); }
            };
            autoUpdateTimer.Start();

            notificationTimer = new Timer { Interval = 300000 };   // 5 phút
            notificationTimer.Tick += (s, e) => { BaoLopSapHoc(); CapNhatThanhTrangThai(); };
            notificationTimer.Start();

            // Lần báo đầu tiên: 10 giây sau khi mở
            var lanDau = new Timer { Interval = 10000 };
            lanDau.Tick += (s, e) => { lanDau.Stop(); lanDau.Dispose(); BaoLopSapHoc(); };
            lanDau.Start();
        }

        /// <summary>
        /// Cập nhật trạng thái lớp/gia sư bằng DbContext MỚI mỗi lần,
        /// tránh ghi đè dữ liệu cũ (ví dụ lớp vừa bị hủy ở form khác).
        /// </summary>
        private static int ChayCapNhatTrangThai()
        {
            using (var service = new LopHocService())
                return service.AutoUpdateStatus();
        }

        /// <summary>Thanh trên: ngày, số lớp theo ca hôm nay, lớp sắp bắt đầu</summary>
        private void CapNhatThanhTrangThai()
        {
            var now = DateTime.Now;
            lblTitle.Text = $"{TenThu[(int)now.DayOfWeek]}, {now:dd/MM/yyyy}";
            try
            {
                using (var service = new LopHocService())
                    shiftRibbon.SetSoLop(service.GetSoLopHomNayTheoCa(PhienDangNhap.GioiHanGiaSu));

                int sapBatDau = _notificationService.GetUpcomingClassCount(24);
                lblStatus.Text = sapBatDau > 0 ? $"{sapBatDau} lớp bắt đầu trong 24 giờ tới" : "Không có lớp sắp bắt đầu";
                lblStatus.ForeColor = sapBatDau > 0 ? Theme.Warning : Theme.TextMuted;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không đọc được dữ liệu";
                lblStatus.ForeColor = Theme.Margin;
                System.Diagnostics.Debug.WriteLine("[HEADER] " + ex.Message);
            }
            lblSubtitle.Text = $"Cập nhật lúc {lastUpdateTime:HH:mm}";
        }

        /// <summary>
        /// Tự cập nhật chỉ nạp lại các trang chỉ-xem (Tổng quan, Lớp học, Lịch học),
        /// không đụng trang đang nhập liệu để khỏi mất chữ người dùng đang gõ.
        /// </summary>
        private void NapLaiTrangChiXem()
        {
            if (activeChild is frmTongQuan || activeChild is frmLopHoc || activeChild is frmCalendarView)
                (activeChild as ILamMoi)?.LamMoi();
        }

        private void BaoLopSapHoc()
        {
            try
            {
                var ds = _notificationService.GetUpcomingClassesForNotification(24);
                if (ds.Count == 0) return;
                _notificationService.PlayNotificationSound();

                int cho = 0;
                foreach (var lop in ds)
                {
                    var l = lop;
                    var t = new Timer { Interval = Math.Max(1, cho) };
                    t.Tick += (s, e) =>
                    {
                        t.Stop(); t.Dispose();
                        try { new frmNotificationToast(l).Show(); }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[TOAST] " + ex.Message); }
                    };
                    t.Start();
                    cho += 2000;   // mỗi thông báo cách nhau 2 giây
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[NOTIFICATION] " + ex.Message); }
        }

        private void TrangChu_Load(object sender, EventArgs e)
        {
            // Đồng bộ trạng thái ngay khi mở app (không đợi 60 giây)
            try { ChayCapNhatTrangThai(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[APP] " + ex.Message); }
            lastUpdateTime = DateTime.Now;
            CapNhatThanhTrangThai();

            // Màn hình đầu: quản lý xem Tổng quan, gia sư vào thẳng điểm danh theo ca
            if (!PhienDangNhap.DaDangNhap || PhienDangNhap.LaQuanLy)
                btnThongKe_Click(btnThongKe, EventArgs.Empty);
            else
            {
                btnHocVien.PerformClick();
                (activeChild as frmHocVien)?.MoTabDiemDanh();
            }
        }

        private void TrangChu_FormClosing(object sender, FormClosingEventArgs e)
        {
            autoUpdateTimer?.Stop();
            autoUpdateTimer?.Dispose();
            notificationTimer?.Stop();
            notificationTimer?.Dispose();
            _notificationService.Dispose();
        }

        // ===================================================================
        // ĐIỀU HƯỚNG GIỮA CÁC TRANG
        // ===================================================================

        // Mỗi màn hình chỉ dựng giao diện MỘT lần rồi giữ lại; lần sau mở chỉ nạp lại dữ liệu
        // => chuyển trang tức thì, không khựng.
        private readonly Dictionary<Type, Form> _trang = new Dictionary<Type, Form>();

        /// <summary>Mục menu ứng với từng trang (để DieuHuong mở trang thì menu cũng sáng theo)</summary>
        private Dictionary<Type, NavButton> _menuCuaTrang;

        private void MoTrang<T>() where T : Form, new() => MoTrang(typeof(T));

        /// <summary>Mở trang + đánh dấu menu; null nếu menu trang đó đang ẩn (không có quyền)</summary>
        private Form MoTrangTuNoiKhac(Type kieu)
        {
            NavButton nut;
            if (_menuCuaTrang != null && _menuCuaTrang.TryGetValue(kieu, out nut))
            {
                if (!nut.Visible) return null;
                ChonMenu(nut);
            }
            return MoTrang(kieu);
        }

        private Form MoTrang(Type kieu)
        {
            Form frm;
            bool moi = !_trang.TryGetValue(kieu, out frm) || frm.IsDisposed;
            if (!moi && frm == activeChild) return frm;

            panelHost.SuspendLayout();
            try
            {
                if (moi)
                {
                    frm = (Form)Activator.CreateInstance(kieu);
                    frm.TopLevel = false;
                    frm.FormBorderStyle = FormBorderStyle.None;
                    frm.Dock = DockStyle.Fill;
                    ThemeManager.Apply(frm);   // màn hình dựng bằng Designer: áp giao diện trước khi hiện
                    panelHost.Controls.Add(frm);
                    _trang[kieu] = frm;
                }

                frm.Show();
                frm.BringToFront();
                if (activeChild != null && activeChild != frm) activeChild.Hide();
                activeChild = frm;
            }
            finally
            {
                panelHost.ResumeLayout(true);
            }

            // Trang đã có sẵn: hiện ngay, nạp dữ liệu mới ngay sau đó
            if (!moi && frm is ILamMoi)
                BeginInvoke((MethodInvoker)(() => ((ILamMoi)frm).LamMoi()));
            return frm;
        }

        private void btnGiaSu_Click(object sender, EventArgs e) { ChonMenu(btnGiaSu); MoTrang<frmGiaSu>(); }
        private void btnHocVien_Click(object sender, EventArgs e) { ChonMenu(btnHocVien); MoTrang<frmHocVien>(); }
        private void btnGhepLop_Click(object sender, EventArgs e) { ChonMenu(btnGhepLop); MoTrang<frmLopHoc>(); }
        private void btnThongKe_Click(object sender, EventArgs e) { ChonMenu(btnThongKe); MoTrang<frmTongQuan>(); }
        private void btnCalendar_Click(object sender, EventArgs e) { ChonMenu(btnCalendar); MoTrang<frmCalendarView>(); }

        /// <summary>Bấm "Làm mới": cập nhật trạng thái lớp rồi nạp lại trang đang mở</summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (activeChild == null) return;
            try
            {
                ChayCapNhatTrangThai();
                lastUpdateTime = DateTime.Now;
                CapNhatThanhTrangThai();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[REFRESH] " + ex.Message); }
            (activeChild as ILamMoi)?.LamMoi();
        }

        /// <summary>Đăng xuất: đóng cửa sổ chính và mở lại màn hình đăng nhập</summary>
        public bool DangXuat { get; private set; }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Đăng xuất khỏi tài khoản này?", Constants.Messages.ConfirmTitle,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            DangXuat = true;
            Close();
        }
    }
}
