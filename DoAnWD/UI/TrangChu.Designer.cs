namespace DoAnWD
{
    partial class TrangChu
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelSidebar;
        private System.Windows.Forms.Panel panelLogo;
        private System.Windows.Forms.Label lblAppName;
        private System.Windows.Forms.Label lblAppSub;
        private System.Windows.Forms.Panel panelNav;
        private System.Windows.Forms.Panel panelNavFooter;
        private System.Windows.Forms.Panel pnlIndicator;
        private DoAnWD.UI.NavButton btnThongKe;
        private DoAnWD.UI.NavButton btnGhepLop;
        private DoAnWD.UI.NavButton btnCalendar;
        private DoAnWD.UI.NavButton btnGiaSu;
        private DoAnWD.UI.NavButton btnHocVien;
        private DoAnWD.UI.NavButton btnRefresh;
        private DoAnWD.UI.NavButton btnExit;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private DoAnWD.UI.ShiftRibbon shiftRibbon;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblUpdated;
        private System.Windows.Forms.Panel panelMarginLine;
        private System.Windows.Forms.Panel panelHost;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.panelLogo = new System.Windows.Forms.Panel();
            this.lblAppName = new System.Windows.Forms.Label();
            this.lblAppSub = new System.Windows.Forms.Label();
            this.panelNav = new System.Windows.Forms.Panel();
            this.panelNavFooter = new System.Windows.Forms.Panel();
            this.pnlIndicator = new System.Windows.Forms.Panel();
            this.btnThongKe = new DoAnWD.UI.NavButton();
            this.btnGhepLop = new DoAnWD.UI.NavButton();
            this.btnCalendar = new DoAnWD.UI.NavButton();
            this.btnGiaSu = new DoAnWD.UI.NavButton();
            this.btnHocVien = new DoAnWD.UI.NavButton();
            this.btnRefresh = new DoAnWD.UI.NavButton();
            this.btnExit = new DoAnWD.UI.NavButton();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.shiftRibbon = new DoAnWD.UI.ShiftRibbon();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblUpdated = new System.Windows.Forms.Label();
            this.panelMarginLine = new System.Windows.Forms.Panel();
            this.panelHost = new System.Windows.Forms.Panel();
            this.panelSidebar.SuspendLayout();
            this.panelLogo.SuspendLayout();
            this.panelNav.SuspendLayout();
            this.panelNavFooter.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            //
            // panelSidebar
            //
            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(42, 35, 96);
            this.panelSidebar.Controls.Add(this.panelNav);
            this.panelSidebar.Controls.Add(this.panelNavFooter);
            this.panelSidebar.Controls.Add(this.panelLogo);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Location = new System.Drawing.Point(0, 0);
            this.panelSidebar.Name = "panelSidebar";
            this.panelSidebar.Size = new System.Drawing.Size(232, 800);
            this.panelSidebar.TabIndex = 0;
            //
            // panelLogo
            //
            this.panelLogo.BackColor = System.Drawing.Color.FromArgb(42, 35, 96);
            this.panelLogo.Controls.Add(this.lblAppSub);
            this.panelLogo.Controls.Add(this.lblAppName);
            this.panelLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLogo.Location = new System.Drawing.Point(0, 0);
            this.panelLogo.Name = "panelLogo";
            this.panelLogo.Size = new System.Drawing.Size(232, 104);
            this.panelLogo.TabIndex = 0;
            //
            // lblAppName
            //
            this.lblAppName.AutoSize = true;
            this.lblAppName.BackColor = System.Drawing.Color.Transparent;
            this.lblAppName.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
            this.lblAppName.ForeColor = System.Drawing.Color.White;
            this.lblAppName.Location = new System.Drawing.Point(62, 30);
            this.lblAppName.Name = "lblAppName";
            this.lblAppName.Text = "Quản lý gia sư";
            //
            // lblAppSub
            //
            this.lblAppSub.AutoSize = true;
            this.lblAppSub.BackColor = System.Drawing.Color.Transparent;
            this.lblAppSub.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblAppSub.ForeColor = System.Drawing.Color.FromArgb(185, 180, 224);
            this.lblAppSub.Location = new System.Drawing.Point(63, 55);
            this.lblAppSub.Name = "lblAppSub";
            this.lblAppSub.Text = "Sổ lớp của trung tâm";
            //
            // panelNav
            //
            this.panelNav.BackColor = System.Drawing.Color.FromArgb(42, 35, 96);
            this.panelNav.Controls.Add(this.pnlIndicator);
            this.panelNav.Controls.Add(this.btnHocVien);
            this.panelNav.Controls.Add(this.btnGiaSu);
            this.panelNav.Controls.Add(this.btnCalendar);
            this.panelNav.Controls.Add(this.btnGhepLop);
            this.panelNav.Controls.Add(this.btnThongKe);
            this.panelNav.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelNav.Name = "panelNav";
            this.panelNav.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.panelNav.TabIndex = 1;
            //
            // pnlIndicator - vạch lề đỏ chỉ mục đang chọn
            //
            this.pnlIndicator.BackColor = System.Drawing.Color.FromArgb(200, 65, 75);
            this.pnlIndicator.Location = new System.Drawing.Point(0, 8);
            this.pnlIndicator.Name = "pnlIndicator";
            this.pnlIndicator.Size = new System.Drawing.Size(4, 48);
            this.pnlIndicator.Visible = false;
            //
            // panelNavFooter
            //
            this.panelNavFooter.BackColor = System.Drawing.Color.FromArgb(42, 35, 96);
            this.panelNavFooter.Controls.Add(this.btnExit);
            this.panelNavFooter.Controls.Add(this.btnRefresh);
            this.panelNavFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelNavFooter.Name = "panelNavFooter";
            this.panelNavFooter.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.panelNavFooter.Size = new System.Drawing.Size(232, 112);
            this.panelNavFooter.TabIndex = 2;
            //
            // btnThongKe
            //
            this.btnThongKe.Name = "btnThongKe";
            this.btnThongKe.Text = "Tổng quan";
            this.btnThongKe.TabIndex = 0;
            this.btnThongKe.Click += new System.EventHandler(this.btnThongKe_Click);
            //
            // btnGhepLop
            //
            this.btnGhepLop.Name = "btnGhepLop";
            this.btnGhepLop.Text = "Ghép lớp";
            this.btnGhepLop.TabIndex = 1;
            this.btnGhepLop.Click += new System.EventHandler(this.btnGhepLop_Click);
            //
            // btnCalendar
            //
            this.btnCalendar.Name = "btnCalendar";
            this.btnCalendar.Text = "Lịch học";
            this.btnCalendar.TabIndex = 2;
            this.btnCalendar.Click += new System.EventHandler(this.btnCalendar_Click);
            //
            // btnGiaSu
            //
            this.btnGiaSu.Name = "btnGiaSu";
            this.btnGiaSu.Text = "Gia sư";
            this.btnGiaSu.TabIndex = 3;
            this.btnGiaSu.Click += new System.EventHandler(this.btnGiaSu_Click);
            //
            // btnHocVien
            //
            this.btnHocVien.Name = "btnHocVien";
            this.btnHocVien.Text = "Học viên";
            this.btnHocVien.TabIndex = 4;
            this.btnHocVien.Click += new System.EventHandler(this.btnHocVien_Click);
            //
            // btnRefresh
            //
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Text = "Làm mới dữ liệu";
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            //
            // btnExit
            //
            this.btnExit.Name = "btnExit";
            this.btnExit.Text = "Thoát";
            this.btnExit.TabIndex = 6;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            //
            // panelTop - thanh tiêu đề: ngày, dải ca học, trạng thái
            //
            this.panelTop.BackColor = System.Drawing.Color.White;
            this.panelTop.Controls.Add(this.lblUpdated);
            this.panelTop.Controls.Add(this.lblStatus);
            this.panelTop.Controls.Add(this.shiftRibbon);
            this.panelTop.Controls.Add(this.lblSubtitle);
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(232, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1168, 76);
            this.panelTop.TabIndex = 1;
            //
            // lblTitle - ngày hôm nay
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 13F);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(35, 35, 58);
            this.lblTitle.Location = new System.Drawing.Point(26, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "Hôm nay";
            //
            // lblSubtitle - ca hiện tại
            //
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(107, 111, 134);
            this.lblSubtitle.Location = new System.Drawing.Point(27, 42);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = "Ngoài giờ học";
            //
            // shiftRibbon
            //
            this.shiftRibbon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.shiftRibbon.Location = new System.Drawing.Point(250, 16);
            this.shiftRibbon.Name = "shiftRibbon";
            this.shiftRibbon.Size = new System.Drawing.Size(620, 44);
            this.shiftRibbon.TabStop = false;
            //
            // lblStatus - số lớp sắp bắt đầu
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(107, 111, 134);
            this.lblStatus.Location = new System.Drawing.Point(890, 18);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(256, 40);
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblUpdated - giờ cập nhật gần nhất
            //
            this.lblUpdated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUpdated.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblUpdated.ForeColor = System.Drawing.Color.FromArgb(154, 157, 178);
            this.lblUpdated.Location = new System.Drawing.Point(890, 40);
            this.lblUpdated.Visible = false;
            this.lblUpdated.Name = "lblUpdated";
            this.lblUpdated.Size = new System.Drawing.Size(256, 20);
            this.lblUpdated.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // panelMarginLine - đường lề đỏ của trang vở
            //
            this.panelMarginLine.BackColor = System.Drawing.Color.FromArgb(246, 247, 251);
            this.panelMarginLine.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelMarginLine.Name = "panelMarginLine";
            this.panelMarginLine.Size = new System.Drawing.Size(14, 724);
            this.panelMarginLine.TabIndex = 3;
            //
            // panelHost
            //
            this.panelHost.BackColor = System.Drawing.Color.FromArgb(246, 247, 251);
            this.panelHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHost.Name = "panelHost";
            this.panelHost.TabIndex = 2;
            //
            // TrangChu
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(246, 247, 251);
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.panelHost);
            this.Controls.Add(this.panelMarginLine);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelSidebar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1180, 720);
            this.Name = "TrangChu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quản lý gia sư";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panelSidebar.ResumeLayout(false);
            this.panelLogo.ResumeLayout(false);
            this.panelLogo.PerformLayout();
            this.panelNav.ResumeLayout(false);
            this.panelNavFooter.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
