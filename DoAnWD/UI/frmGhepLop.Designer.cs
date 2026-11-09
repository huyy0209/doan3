namespace DoAnWD
{
    partial class frmGhepLop
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.SplitContainer splitContainer;

        // Left Panel - Gia Sư
        private System.Windows.Forms.Panel panelGiaSu;
        private System.Windows.Forms.DataGridView dgvGiaSu;
        private System.Windows.Forms.Panel panelGiaSuInfo;
        private System.Windows.Forms.Label lblGiaSuTitle;
        private System.Windows.Forms.Panel panelGiaSuDetails;
        private System.Windows.Forms.Label lblMaGS;
        private System.Windows.Forms.TextBox txtMaGS;
        private System.Windows.Forms.Label lblHoTen;
        private System.Windows.Forms.TextBox txtHoTen;
        private System.Windows.Forms.Label lblChuyenMon;
        private System.Windows.Forms.TextBox txtChuyenMon;
        private System.Windows.Forms.Label lblTrangThai;
        private System.Windows.Forms.TextBox txtTrangThai;
        private System.Windows.Forms.Label lblSoLop;
        private System.Windows.Forms.TextBox txtSoLop;

        // Right Panel - Lớp Học
        private System.Windows.Forms.Panel panelLopHoc;
        private System.Windows.Forms.DataGridView dgvLopHoc;
        private System.Windows.Forms.Panel panelLopHocHeader;
        private System.Windows.Forms.Label lblLopHocTitle;
        private System.Windows.Forms.Panel panelButtons;
        private DoAnWD.UI.ThemedButton btnGhepLop;
        private DoAnWD.UI.ThemedButton btnXoaLop;
        private DoAnWD.UI.ThemedButton btnRefresh;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();

            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelMain = new System.Windows.Forms.Panel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panelGiaSu = new System.Windows.Forms.Panel();
            this.dgvGiaSu = new System.Windows.Forms.DataGridView();
            this.panelGiaSuInfo = new System.Windows.Forms.Panel();
            this.lblGiaSuTitle = new System.Windows.Forms.Label();
            this.panelGiaSuDetails = new System.Windows.Forms.Panel();
            this.lblMaGS = new System.Windows.Forms.Label();
            this.txtMaGS = new System.Windows.Forms.TextBox();
            this.lblHoTen = new System.Windows.Forms.Label();
            this.txtHoTen = new System.Windows.Forms.TextBox();
            this.lblChuyenMon = new System.Windows.Forms.Label();
            this.txtChuyenMon = new System.Windows.Forms.TextBox();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.txtTrangThai = new System.Windows.Forms.TextBox();
            this.lblSoLop = new System.Windows.Forms.Label();
            this.txtSoLop = new System.Windows.Forms.TextBox();
            this.panelLopHoc = new System.Windows.Forms.Panel();
            this.dgvLopHoc = new System.Windows.Forms.DataGridView();
            this.panelLopHocHeader = new System.Windows.Forms.Panel();
            this.lblLopHocTitle = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnGhepLop = new DoAnWD.UI.ThemedButton();
            this.btnXoaLop = new DoAnWD.UI.ThemedButton();
            this.btnRefresh = new DoAnWD.UI.ThemedButton();

            this.panelTop.SuspendLayout();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelGiaSu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGiaSu)).BeginInit();
            this.panelGiaSuInfo.SuspendLayout();
            this.panelGiaSuDetails.SuspendLayout();
            this.panelLopHoc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLopHoc)).BeginInit();
            this.panelLopHocHeader.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();

            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1200, 60);
            this.panelTop.TabIndex = 0;

            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(400, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Ghép lớp";

            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelMain.Controls.Add(this.splitContainer);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 60);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(15);
            this.panelMain.Size = new System.Drawing.Size(1200, 690);
            this.panelMain.TabIndex = 1;

            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.Location = new System.Drawing.Point(15, 15);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Panel1MinSize = 400;
            this.splitContainer.Panel2MinSize = 500;
            this.splitContainer.Size = new System.Drawing.Size(1170, 660);
            this.splitContainer.SplitterDistance = 450;
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.TabIndex = 0;

            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.panelGiaSu);

            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.panelLopHoc);

            // 
            // panelGiaSu
            // 
            this.panelGiaSu.BackColor = System.Drawing.Color.White;
            this.panelGiaSu.Controls.Add(this.dgvGiaSu);
            this.panelGiaSu.Controls.Add(this.panelGiaSuInfo);
            this.panelGiaSu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGiaSu.Location = new System.Drawing.Point(0, 0);
            this.panelGiaSu.Name = "panelGiaSu";
            this.panelGiaSu.Padding = new System.Windows.Forms.Padding(15);
            this.panelGiaSu.Size = new System.Drawing.Size(450, 660);
            this.panelGiaSu.TabIndex = 0;

            // 
            // dgvGiaSu
            // 
            this.dgvGiaSu.AllowUserToAddRows = false;
            this.dgvGiaSu.AllowUserToDeleteRows = false;
            this.dgvGiaSu.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvGiaSu.BackgroundColor = System.Drawing.Color.White;
            this.dgvGiaSu.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvGiaSu.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvGiaSu.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvGiaSu.ColumnHeadersHeight = 35;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvGiaSu.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvGiaSu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvGiaSu.EnableHeadersVisualStyles = false;
            this.dgvGiaSu.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.dgvGiaSu.Location = new System.Drawing.Point(15, 60);
            this.dgvGiaSu.Name = "dgvGiaSu";
            this.dgvGiaSu.ReadOnly = true;
            this.dgvGiaSu.RowHeadersVisible = false;
            this.dgvGiaSu.RowTemplate.Height = 30;
            this.dgvGiaSu.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvGiaSu.Size = new System.Drawing.Size(420, 355);
            this.dgvGiaSu.TabIndex = 0;
            this.dgvGiaSu.SelectionChanged += new System.EventHandler(this.dgvGiaSu_SelectionChanged);

            // 
            // panelGiaSuInfo
            // 
            this.panelGiaSuInfo.BackColor = System.Drawing.Color.White;
            this.panelGiaSuInfo.Controls.Add(this.lblGiaSuTitle);
            this.panelGiaSuInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelGiaSuInfo.Location = new System.Drawing.Point(15, 15);
            this.panelGiaSuInfo.Name = "panelGiaSuInfo";
            this.panelGiaSuInfo.Size = new System.Drawing.Size(420, 45);
            this.panelGiaSuInfo.TabIndex = 1;

            // 
            // lblGiaSuTitle
            // 
            this.lblGiaSuTitle.AutoSize = true;
            this.lblGiaSuTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblGiaSuTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            this.lblGiaSuTitle.Location = new System.Drawing.Point(5, 10);
            this.lblGiaSuTitle.Name = "lblGiaSuTitle";
            this.lblGiaSuTitle.Size = new System.Drawing.Size(186, 21);
            this.lblGiaSuTitle.TabIndex = 0;
            this.lblGiaSuTitle.Text = "Danh sách gia sư";

            // 
            // panelGiaSuDetails
            // 
            this.panelGiaSuDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelGiaSuDetails.Controls.Add(this.lblMaGS);
            this.panelGiaSuDetails.Controls.Add(this.txtMaGS);
            this.panelGiaSuDetails.Controls.Add(this.lblHoTen);
            this.panelGiaSuDetails.Controls.Add(this.txtHoTen);
            this.panelGiaSuDetails.Controls.Add(this.lblChuyenMon);
            this.panelGiaSuDetails.Controls.Add(this.txtChuyenMon);
            this.panelGiaSuDetails.Controls.Add(this.lblTrangThai);
            this.panelGiaSuDetails.Controls.Add(this.txtTrangThai);
            this.panelGiaSuDetails.Controls.Add(this.lblSoLop);
            this.panelGiaSuDetails.Controls.Add(this.txtSoLop);
            this.panelGiaSuDetails.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelGiaSuDetails.Location = new System.Drawing.Point(15, 415);
            this.panelGiaSuDetails.Name = "panelGiaSuDetails";
            this.panelGiaSuDetails.Padding = new System.Windows.Forms.Padding(10);
            this.panelGiaSuDetails.Size = new System.Drawing.Size(420, 230);
            this.panelGiaSuDetails.TabIndex = 2;

            // 
            // lblMaGS
            // 
            this.lblMaGS.AutoSize = true;
            this.lblMaGS.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblMaGS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblMaGS.Location = new System.Drawing.Point(10, 15);
            this.lblMaGS.Name = "lblMaGS";
            this.lblMaGS.Size = new System.Drawing.Size(72, 15);
            this.lblMaGS.TabIndex = 0;
            this.lblMaGS.Text = "Mã gia sư:";

            // 
            // txtMaGS
            // 
            this.txtMaGS.BackColor = System.Drawing.Color.White;
            this.txtMaGS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMaGS.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtMaGS.Location = new System.Drawing.Point(115, 12);
            this.txtMaGS.Name = "txtMaGS";
            this.txtMaGS.ReadOnly = true;
            this.txtMaGS.Size = new System.Drawing.Size(80, 23);
            this.txtMaGS.TabIndex = 1;
            this.txtMaGS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // 
            // lblHoTen
            // 
            this.lblHoTen.AutoSize = true;
            this.lblHoTen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblHoTen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblHoTen.Location = new System.Drawing.Point(10, 50);
            this.lblHoTen.Name = "lblHoTen";
            this.lblHoTen.Size = new System.Drawing.Size(54, 15);
            this.lblHoTen.TabIndex = 2;
            this.lblHoTen.Text = "Họ tên:";

            // 
            // txtHoTen
            // 
            this.txtHoTen.BackColor = System.Drawing.Color.White;
            this.txtHoTen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHoTen.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.txtHoTen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.txtHoTen.Location = new System.Drawing.Point(115, 47);
            this.txtHoTen.Name = "txtHoTen";
            this.txtHoTen.ReadOnly = true;
            this.txtHoTen.Size = new System.Drawing.Size(290, 25);
            this.txtHoTen.TabIndex = 3;

            // 
            // lblChuyenMon
            // 
            this.lblChuyenMon.AutoSize = true;
            this.lblChuyenMon.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblChuyenMon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblChuyenMon.Location = new System.Drawing.Point(10, 85);
            this.lblChuyenMon.Name = "lblChuyenMon";
            this.lblChuyenMon.Size = new System.Drawing.Size(84, 15);
            this.lblChuyenMon.TabIndex = 4;
            this.lblChuyenMon.Text = "Chuyên môn:";

            // 
            // txtChuyenMon
            // 
            this.txtChuyenMon.BackColor = System.Drawing.Color.White;
            this.txtChuyenMon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtChuyenMon.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtChuyenMon.Location = new System.Drawing.Point(115, 82);
            this.txtChuyenMon.Multiline = true;
            this.txtChuyenMon.Name = "txtChuyenMon";
            this.txtChuyenMon.ReadOnly = true;
            this.txtChuyenMon.Size = new System.Drawing.Size(290, 40);
            this.txtChuyenMon.TabIndex = 5;

            // 
            // lblTrangThai
            // 
            this.lblTrangThai.AutoSize = true;
            this.lblTrangThai.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTrangThai.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTrangThai.Location = new System.Drawing.Point(10, 135);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Size = new System.Drawing.Size(72, 15);
            this.lblTrangThai.TabIndex = 6;
            this.lblTrangThai.Text = "Trạng thái:";

            // 
            // txtTrangThai
            // 
            this.txtTrangThai.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.txtTrangThai.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTrangThai.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.txtTrangThai.ForeColor = System.Drawing.Color.White;
            this.txtTrangThai.Location = new System.Drawing.Point(115, 132);
            this.txtTrangThai.Name = "txtTrangThai";
            this.txtTrangThai.ReadOnly = true;
            this.txtTrangThai.Size = new System.Drawing.Size(140, 23);
            this.txtTrangThai.TabIndex = 7;
            this.txtTrangThai.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // 
            // lblSoLop
            // 
            this.lblSoLop.AutoSize = true;
            this.lblSoLop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSoLop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblSoLop.Location = new System.Drawing.Point(10, 170);
            this.lblSoLop.Name = "lblSoLop";
            this.lblSoLop.Size = new System.Drawing.Size(101, 15);
            this.lblSoLop.TabIndex = 8;
            this.lblSoLop.Text = "Số lớp đang dạy:";

            // 
            // txtSoLop
            // 
            this.txtSoLop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.txtSoLop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSoLop.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.txtSoLop.ForeColor = System.Drawing.Color.White;
            this.txtSoLop.Location = new System.Drawing.Point(115, 163);
            this.txtSoLop.Name = "txtSoLop";
            this.txtSoLop.ReadOnly = true;
            this.txtSoLop.Size = new System.Drawing.Size(100, 36);
            this.txtSoLop.TabIndex = 9;
            this.txtSoLop.Text = "0";
            this.txtSoLop.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // Add panelGiaSuDetails to panelGiaSu
            this.panelGiaSu.Controls.Add(this.panelGiaSuDetails);

            // 
            // panelLopHoc
            // 
            this.panelLopHoc.BackColor = System.Drawing.Color.White;
            this.panelLopHoc.Controls.Add(this.dgvLopHoc);
            this.panelLopHoc.Controls.Add(this.panelButtons);
            this.panelLopHoc.Controls.Add(this.panelLopHocHeader);
            this.panelLopHoc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLopHoc.Location = new System.Drawing.Point(0, 0);
            this.panelLopHoc.Name = "panelLopHoc";
            this.panelLopHoc.Padding = new System.Windows.Forms.Padding(15);
            this.panelLopHoc.Size = new System.Drawing.Size(715, 660);
            this.panelLopHoc.TabIndex = 0;

            // 
            // dgvLopHoc
            // 
            this.dgvLopHoc.AllowUserToAddRows = false;
            this.dgvLopHoc.AllowUserToDeleteRows = false;
            this.dgvLopHoc.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLopHoc.BackgroundColor = System.Drawing.Color.White;
            this.dgvLopHoc.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvLopHoc.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvLopHoc.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvLopHoc.ColumnHeadersHeight = 35;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvLopHoc.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvLopHoc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLopHoc.EnableHeadersVisualStyles = false;
            this.dgvLopHoc.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.dgvLopHoc.Location = new System.Drawing.Point(15, 60);
            this.dgvLopHoc.Name = "dgvLopHoc";
            this.dgvLopHoc.ReadOnly = true;
            this.dgvLopHoc.RowHeadersVisible = false;
            this.dgvLopHoc.RowTemplate.Height = 30;
            this.dgvLopHoc.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLopHoc.Size = new System.Drawing.Size(685, 505);
            this.dgvLopHoc.TabIndex = 0;

            // 
            // panelLopHocHeader
            // 
            this.panelLopHocHeader.BackColor = System.Drawing.Color.White;
            this.panelLopHocHeader.Controls.Add(this.lblLopHocTitle);
            this.panelLopHocHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLopHocHeader.Location = new System.Drawing.Point(15, 15);
            this.panelLopHocHeader.Name = "panelLopHocHeader";
            this.panelLopHocHeader.Size = new System.Drawing.Size(685, 45);
            this.panelLopHocHeader.TabIndex = 1;

            // 
            // lblLopHocTitle
            // 
            this.lblLopHocTitle.AutoSize = true;
            this.lblLopHocTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblLopHocTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.lblLopHocTitle.Location = new System.Drawing.Point(5, 10);
            this.lblLopHocTitle.Name = "lblLopHocTitle";
            this.lblLopHocTitle.Size = new System.Drawing.Size(192, 21);
            this.lblLopHocTitle.TabIndex = 0;
            this.lblLopHocTitle.Text = "Danh sách lớp học";

            // 
            // panelButtons
            // 
            this.panelButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelButtons.Controls.Add(this.btnGhepLop);
            this.panelButtons.Controls.Add(this.btnXoaLop);
            this.panelButtons.Controls.Add(this.btnRefresh);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(15, 565);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(10);
            this.panelButtons.Size = new System.Drawing.Size(685, 80);
            this.panelButtons.TabIndex = 2;

            // 
            // btnGhepLop
            // 
            this.btnGhepLop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnGhepLop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGhepLop.FlatAppearance.BorderSize = 0;
            this.btnGhepLop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGhepLop.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnGhepLop.ForeColor = System.Drawing.Color.White;
            this.btnGhepLop.Location = new System.Drawing.Point(15, 20);
            this.btnGhepLop.Name = "btnGhepLop";
            this.btnGhepLop.Size = new System.Drawing.Size(170, 45);
            this.btnGhepLop.TabIndex = 0;
            this.btnGhepLop.Text = "Ghép lớp mới";
            this.btnGhepLop.UseVisualStyleBackColor = false;
            this.btnGhepLop.Click += new System.EventHandler(this.btnGhepLop_Click);

            // 
            // btnXoaLop
            // 
            this.btnXoaLop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnXoaLop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnXoaLop.FlatAppearance.BorderSize = 0;
            this.btnXoaLop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnXoaLop.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnXoaLop.ForeColor = System.Drawing.Color.White;
            this.btnXoaLop.Location = new System.Drawing.Point(200, 20);
            this.btnXoaLop.Name = "btnXoaLop";
            this.btnXoaLop.Size = new System.Drawing.Size(160, 45);
            this.btnXoaLop.TabIndex = 1;
            this.btnXoaLop.Text = "Hủy/Xóa lớp";
            this.btnXoaLop.UseVisualStyleBackColor = false;
            this.btnXoaLop.Click += new System.EventHandler(this.btnXoaLop_Click);

            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(375, 20);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(140, 45);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Làm mới";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // 
            // frmGhepLop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(1200, 750);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "frmGhepLop";
            this.Text = "Quản lý Lớp Học theo Gia Sư";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panelGiaSu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvGiaSu)).EndInit();
            this.panelGiaSuInfo.ResumeLayout(false);
            this.panelGiaSuInfo.PerformLayout();
            this.panelGiaSuDetails.ResumeLayout(false);
            this.panelGiaSuDetails.PerformLayout();
            this.panelLopHoc.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLopHoc)).EndInit();
            this.panelLopHocHeader.ResumeLayout(false);
            this.panelLopHocHeader.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}