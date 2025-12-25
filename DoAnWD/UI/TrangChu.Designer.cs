namespace DoAnWD
{
    partial class TrangChu
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelSidebar;
        private System.Windows.Forms.Panel panelLogo;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Button btnGiaSu;
        private System.Windows.Forms.Button btnHocVien;
        private System.Windows.Forms.Button btnGhepLop;
        private System.Windows.Forms.Button btnThongKe;
        private System.Windows.Forms.Button btnCalendar;  // ✅ NÚT MỚI
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
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
            this.lblLogo = new System.Windows.Forms.Label();
            this.btnGiaSu = new System.Windows.Forms.Button();
            this.btnHocVien = new System.Windows.Forms.Button();
            this.btnGhepLop = new System.Windows.Forms.Button();
            this.btnThongKe = new System.Windows.Forms.Button();
            this.btnCalendar = new System.Windows.Forms.Button();  // ✅ KHAI BÁO
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelHost = new System.Windows.Forms.Panel();
            this.panelSidebar.SuspendLayout();
            this.panelLogo.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();

            // 
            // panelSidebar
            // 
            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.panelSidebar.Controls.Add(this.btnExit);
            this.panelSidebar.Controls.Add(this.btnRefresh);
            this.panelSidebar.Controls.Add(this.btnCalendar);  // ✅ THÊM VÀO SIDEBAR
            this.panelSidebar.Controls.Add(this.btnThongKe);
            this.panelSidebar.Controls.Add(this.btnGhepLop);
            this.panelSidebar.Controls.Add(this.btnHocVien);
            this.panelSidebar.Controls.Add(this.btnGiaSu);
            this.panelSidebar.Controls.Add(this.panelLogo);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Location = new System.Drawing.Point(0, 0);
            this.panelSidebar.Name = "panelSidebar";
            this.panelSidebar.Size = new System.Drawing.Size(250, 800);
            this.panelSidebar.TabIndex = 0;

            // 
            // panelLogo
            // 
            this.panelLogo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.panelLogo.Controls.Add(this.lblLogo);
            this.panelLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLogo.Location = new System.Drawing.Point(0, 0);
            this.panelLogo.Name = "panelLogo";
            this.panelLogo.Size = new System.Drawing.Size(250, 100);
            this.panelLogo.TabIndex = 0;

            // 
            // lblLogo
            // 
            this.lblLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblLogo.ForeColor = System.Drawing.Color.White;
            this.lblLogo.Location = new System.Drawing.Point(0, 0);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(250, 100);
            this.lblLogo.TabIndex = 0;
            this.lblLogo.Text = "📚 QUẢN LÝ\r\nGIA SƯ";
            this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // btnGiaSu
            // 
            this.btnGiaSu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGiaSu.FlatAppearance.BorderSize = 0;
            this.btnGiaSu.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnGiaSu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGiaSu.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnGiaSu.ForeColor = System.Drawing.Color.White;
            this.btnGiaSu.Image = null;
            this.btnGiaSu.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGiaSu.Location = new System.Drawing.Point(0, 120);
            this.btnGiaSu.Name = "btnGiaSu";
            this.btnGiaSu.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnGiaSu.Size = new System.Drawing.Size(250, 60);
            this.btnGiaSu.TabIndex = 1;
            this.btnGiaSu.Text = "👨‍🏫 Gia Sư";
            this.btnGiaSu.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGiaSu.UseVisualStyleBackColor = true;
            this.btnGiaSu.Click += new System.EventHandler(this.btnGiaSu_Click);

            // 
            // btnHocVien
            // 
            this.btnHocVien.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHocVien.FlatAppearance.BorderSize = 0;
            this.btnHocVien.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnHocVien.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHocVien.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnHocVien.ForeColor = System.Drawing.Color.White;
            this.btnHocVien.Location = new System.Drawing.Point(0, 180);
            this.btnHocVien.Name = "btnHocVien";
            this.btnHocVien.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnHocVien.Size = new System.Drawing.Size(250, 60);
            this.btnHocVien.TabIndex = 2;
            this.btnHocVien.Text = "👨‍🎓 Học Viên";
            this.btnHocVien.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHocVien.UseVisualStyleBackColor = true;
            this.btnHocVien.Click += new System.EventHandler(this.btnHocVien_Click);

            // 
            // btnGhepLop
            // 
            this.btnGhepLop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGhepLop.FlatAppearance.BorderSize = 0;
            this.btnGhepLop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnGhepLop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGhepLop.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnGhepLop.ForeColor = System.Drawing.Color.White;
            this.btnGhepLop.Location = new System.Drawing.Point(0, 240);
            this.btnGhepLop.Name = "btnGhepLop";
            this.btnGhepLop.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnGhepLop.Size = new System.Drawing.Size(250, 60);
            this.btnGhepLop.TabIndex = 3;
            this.btnGhepLop.Text = "📝 Ghép Lớp";
            this.btnGhepLop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGhepLop.UseVisualStyleBackColor = true;
            this.btnGhepLop.Click += new System.EventHandler(this.btnGhepLop_Click);

            // 
            // btnThongKe
            // 
            this.btnThongKe.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnThongKe.FlatAppearance.BorderSize = 0;
            this.btnThongKe.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnThongKe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnThongKe.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnThongKe.ForeColor = System.Drawing.Color.White;
            this.btnThongKe.Location = new System.Drawing.Point(0, 300);
            this.btnThongKe.Name = "btnThongKe";
            this.btnThongKe.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnThongKe.Size = new System.Drawing.Size(250, 60);
            this.btnThongKe.TabIndex = 4;
            this.btnThongKe.Text = "📊 Thống Kê";
            this.btnThongKe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnThongKe.UseVisualStyleBackColor = true;
            this.btnThongKe.Click += new System.EventHandler(this.btnThongKe_Click);

            // 
            // btnCalendar  ✅ NÚT MỚI
            // 
            this.btnCalendar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCalendar.FlatAppearance.BorderSize = 0;
            this.btnCalendar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnCalendar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCalendar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnCalendar.ForeColor = System.Drawing.Color.White;
            this.btnCalendar.Location = new System.Drawing.Point(0, 360);
            this.btnCalendar.Name = "btnCalendar";
            this.btnCalendar.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnCalendar.Size = new System.Drawing.Size(250, 60);
            this.btnCalendar.TabIndex = 5;
            this.btnCalendar.Text = "📅 Lịch Học";
            this.btnCalendar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCalendar.UseVisualStyleBackColor = true;
            this.btnCalendar.Click += new System.EventHandler(this.btnCalendar_Click);

            // 
            // btnRefresh
            // 
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(0, 680);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnRefresh.Size = new System.Drawing.Size(250, 50);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "🔄 Làm mới";
            this.btnRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // 
            // btnExit
            // 
            this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Location = new System.Drawing.Point(0, 730);
            this.btnExit.Name = "btnExit";
            this.btnExit.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnExit.Size = new System.Drawing.Size(250, 50);
            this.btnExit.TabIndex = 7;
            this.btnExit.Text = "❌ Thoát";
            this.btnExit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);

            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.White;
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(250, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1150, 80);
            this.panelTop.TabIndex = 1;

            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTitle.Location = new System.Drawing.Point(30, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(500, 41);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "HỆ THỐNG QUẢN LÝ GIA SƯ";

            // 
            // panelHost
            // 
            this.panelHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHost.Location = new System.Drawing.Point(250, 80);
            this.panelHost.Name = "panelHost";
            this.panelHost.Size = new System.Drawing.Size(1150, 720);
            this.panelHost.TabIndex = 2;

            // 
            // TrangChu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.panelHost);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelSidebar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "TrangChu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quản Lý Gia Sư";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panelSidebar.ResumeLayout(false);
            this.panelLogo.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}