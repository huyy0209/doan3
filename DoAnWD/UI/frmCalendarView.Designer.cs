namespace DoAnWD
{
    partial class frmCalendarView
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelNav;
        private DoAnWD.UI.ThemedButton btnPrevMonth;
        private DoAnWD.UI.ThemedButton btnToday;
        private DoAnWD.UI.ThemedButton btnNextMonth;
        private System.Windows.Forms.Label lblMonth;
        private System.Windows.Forms.ComboBox cboGiaSu;
        private DoAnWD.UI.MonthGrid monthGrid;

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
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelNav = new System.Windows.Forms.Panel();
            this.btnPrevMonth = new DoAnWD.UI.ThemedButton();
            this.btnToday = new DoAnWD.UI.ThemedButton();
            this.btnNextMonth = new DoAnWD.UI.ThemedButton();
            this.lblMonth = new System.Windows.Forms.Label();
            this.cboGiaSu = new System.Windows.Forms.ComboBox();
            this.monthGrid = new DoAnWD.UI.MonthGrid();
            this.panelTop.SuspendLayout();
            this.panelNav.SuspendLayout();
            this.SuspendLayout();

            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1000, 60);
            this.panelTop.TabIndex = 0;

            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(250, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Lịch học";

            // 
            // panelNav
            // 
            this.panelNav.BackColor = System.Drawing.Color.White;
            this.panelNav.Controls.Add(this.btnPrevMonth);
            this.panelNav.Controls.Add(this.btnToday);
            this.panelNav.Controls.Add(this.btnNextMonth);
            this.panelNav.Controls.Add(this.lblMonth);
            this.panelNav.Controls.Add(this.cboGiaSu);
            this.panelNav.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelNav.Location = new System.Drawing.Point(0, 60);
            this.panelNav.Name = "panelNav";
            this.panelNav.Size = new System.Drawing.Size(1000, 60);
            this.panelNav.TabIndex = 1;

            // 
            // btnPrevMonth
            // 
            this.btnPrevMonth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnPrevMonth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevMonth.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPrevMonth.ForeColor = System.Drawing.Color.White;
            this.btnPrevMonth.Location = new System.Drawing.Point(244, 14);
            this.btnPrevMonth.Name = "btnPrevMonth";
            this.btnPrevMonth.Size = new System.Drawing.Size(84, 32);
            this.btnPrevMonth.TabIndex = 0;
            this.btnPrevMonth.Text = "Trước";
            this.btnPrevMonth.UseVisualStyleBackColor = false;
            this.btnPrevMonth.Click += new System.EventHandler(this.btnPrevMonth_Click);

            // 
            // btnToday
            // 
            this.btnToday.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnToday.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToday.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnToday.ForeColor = System.Drawing.Color.White;
            this.btnToday.Location = new System.Drawing.Point(332, 14);
            this.btnToday.Name = "btnToday";
            this.btnToday.Size = new System.Drawing.Size(96, 32);
            this.btnToday.TabIndex = 1;
            this.btnToday.Text = "Hôm nay";
            this.btnToday.UseVisualStyleBackColor = false;
            this.btnToday.Click += new System.EventHandler(this.btnToday_Click);

            // 
            // btnNextMonth
            // 
            this.btnNextMonth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnNextMonth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextMonth.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNextMonth.ForeColor = System.Drawing.Color.White;
            this.btnNextMonth.Location = new System.Drawing.Point(432, 14);
            this.btnNextMonth.Name = "btnNextMonth";
            this.btnNextMonth.Size = new System.Drawing.Size(84, 32);
            this.btnNextMonth.TabIndex = 2;
            this.btnNextMonth.Text = "Sau";
            this.btnNextMonth.UseVisualStyleBackColor = false;
            this.btnNextMonth.Click += new System.EventHandler(this.btnNextMonth_Click);

            // 
            // lblMonth
            // 
            this.lblMonth.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblMonth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblMonth.Location = new System.Drawing.Point(16, 12);
            this.lblMonth.Name = "lblMonth";
            this.lblMonth.Size = new System.Drawing.Size(220, 36);
            this.lblMonth.TabIndex = 3;
            this.lblMonth.Text = "January 2025";
            this.lblMonth.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // cboGiaSu
            // 
            this.cboGiaSu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGiaSu.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboGiaSu.FormattingEnabled = true;
            this.cboGiaSu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboGiaSu.Location = new System.Drawing.Point(720, 17);
            this.cboGiaSu.Name = "cboGiaSu";
            this.cboGiaSu.Size = new System.Drawing.Size(260, 25);
            this.cboGiaSu.TabIndex = 4;
            this.cboGiaSu.SelectedIndexChanged += new System.EventHandler(this.cboGiaSu_SelectedIndexChanged);

            // 
            // monthGrid - lịch tháng tự vẽ, co giãn theo cửa sổ
            // 
            this.monthGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.monthGrid.Location = new System.Drawing.Point(20, 128);
            this.monthGrid.Name = "monthGrid";
            this.monthGrid.Size = new System.Drawing.Size(960, 612);
            this.monthGrid.TabIndex = 2;

            // 
            // frmCalendarView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(1000, 760);
            this.Controls.Add(this.monthGrid);
            this.Controls.Add(this.panelNav);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmCalendarView";
            this.Text = "Calendar View";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelNav.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}