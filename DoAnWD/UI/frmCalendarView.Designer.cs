namespace DoAnWD
{
    partial class frmCalendarView
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelNav;
        private System.Windows.Forms.Button btnPrevMonth;
        private System.Windows.Forms.Button btnToday;
        private System.Windows.Forms.Button btnNextMonth;
        private System.Windows.Forms.Label lblMonth;
        private System.Windows.Forms.ComboBox cboGiaSu;
        private System.Windows.Forms.Panel panelCalendar;

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
            this.btnPrevMonth = new System.Windows.Forms.Button();
            this.btnToday = new System.Windows.Forms.Button();
            this.btnNextMonth = new System.Windows.Forms.Button();
            this.lblMonth = new System.Windows.Forms.Label();
            this.cboGiaSu = new System.Windows.Forms.ComboBox();
            this.panelCalendar = new System.Windows.Forms.Panel();
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
            this.lblTitle.Text = "📅 LỊCH HỌC THÁNG";

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
            this.btnPrevMonth.Location = new System.Drawing.Point(20, 15);
            this.btnPrevMonth.Name = "btnPrevMonth";
            this.btnPrevMonth.Size = new System.Drawing.Size(100, 30);
            this.btnPrevMonth.TabIndex = 0;
            this.btnPrevMonth.Text = "◀ Trước";
            this.btnPrevMonth.UseVisualStyleBackColor = false;
            this.btnPrevMonth.Click += new System.EventHandler(this.btnPrevMonth_Click);

            // 
            // btnToday
            // 
            this.btnToday.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnToday.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToday.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnToday.ForeColor = System.Drawing.Color.White;
            this.btnToday.Location = new System.Drawing.Point(130, 15);
            this.btnToday.Name = "btnToday";
            this.btnToday.Size = new System.Drawing.Size(100, 30);
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
            this.btnNextMonth.Location = new System.Drawing.Point(240, 15);
            this.btnNextMonth.Name = "btnNextMonth";
            this.btnNextMonth.Size = new System.Drawing.Size(100, 30);
            this.btnNextMonth.TabIndex = 2;
            this.btnNextMonth.Text = "Sau ▶";
            this.btnNextMonth.UseVisualStyleBackColor = false;
            this.btnNextMonth.Click += new System.EventHandler(this.btnNextMonth_Click);

            // 
            // lblMonth
            // 
            this.lblMonth.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblMonth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblMonth.Location = new System.Drawing.Point(360, 10);
            this.lblMonth.Name = "lblMonth";
            this.lblMonth.Size = new System.Drawing.Size(250, 40);
            this.lblMonth.TabIndex = 3;
            this.lblMonth.Text = "January 2025";
            this.lblMonth.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // cboGiaSu
            // 
            this.cboGiaSu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGiaSu.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboGiaSu.FormattingEnabled = true;
            this.cboGiaSu.Location = new System.Drawing.Point(720, 17);
            this.cboGiaSu.Name = "cboGiaSu";
            this.cboGiaSu.Size = new System.Drawing.Size(260, 25);
            this.cboGiaSu.TabIndex = 4;
            this.cboGiaSu.SelectedIndexChanged += new System.EventHandler(this.cboGiaSu_SelectedIndexChanged);

            // 
            // panelCalendar
            // 
            this.panelCalendar.AutoScroll = true;
            this.panelCalendar.BackColor = System.Drawing.Color.White;
            this.panelCalendar.Location = new System.Drawing.Point(20, 140);
            this.panelCalendar.Name = "panelCalendar";
            this.panelCalendar.Size = new System.Drawing.Size(980, 600);
            this.panelCalendar.TabIndex = 2;

            // 
            // frmCalendarView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(1000, 760);
            this.Controls.Add(this.panelCalendar);
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