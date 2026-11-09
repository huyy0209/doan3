namespace DoAnWD
{
    partial class frmThongKe
    {
        private System.ComponentModel.IContainer components = null;

        // Top Panel
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
        private DoAnWD.UI.ThemedButton btnRefresh;

        // TabControl for organizing sections
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabTongQuan;
        private System.Windows.Forms.TabPage tabGiaSu;
        private System.Windows.Forms.TabPage tabLopHoc;

        // ===== TAB 1: TỔNG QUAN =====
        private System.Windows.Forms.Panel panelCards;
        private System.Windows.Forms.Panel cardTongGS;
        private System.Windows.Forms.Label lblCardTitleGS;
        private System.Windows.Forms.Label lblCountGS;
        private System.Windows.Forms.Panel cardTongHV;
        private System.Windows.Forms.Label lblCardTitleHV;
        private System.Windows.Forms.Label lblCountHV;
        private System.Windows.Forms.Panel cardTongLop;
        private System.Windows.Forms.Label lblCardTitleLop;
        private System.Windows.Forms.Label lblCountLop;

        private DoAnWD.UI.SectionBox gbTrangThaiGS;
        private System.Windows.Forms.Label lblLabelGSRanh;
        private System.Windows.Forms.Label lblCountGSRanh;
        private System.Windows.Forms.Label lblLabelGSBan;
        private System.Windows.Forms.Label lblCountGSDangDay;

        private DoAnWD.UI.SectionBox gbTrangThaiLop;
        private System.Windows.Forms.Label lblLabelChuaBatDau;
        private System.Windows.Forms.Label lblCountChuaBatDau;
        private System.Windows.Forms.Label lblLabelDangDienRa;
        private System.Windows.Forms.Label lblCountDangDienRa;
        private System.Windows.Forms.Label lblLabelDaKetThuc;
        private System.Windows.Forms.Label lblCountDaKetThuc;
        private System.Windows.Forms.Label lblLabelLopDangHoc;
        private System.Windows.Forms.Label lblCountLopDangHoc;

        // ===== TAB 2: GIA SƯ =====
        private DoAnWD.UI.SectionBox gbDanhSachGS;
        private System.Windows.Forms.DataGridView dgvGiaSu;
        private DoAnWD.UI.SectionBox gbChiTietGS;
        private System.Windows.Forms.Label lblLabelMaGS;
        private System.Windows.Forms.TextBox txtMaGS;
        private System.Windows.Forms.Label lblLabelHoTen;
        private System.Windows.Forms.TextBox txtHoTen;
        private System.Windows.Forms.Label lblLabelChuyenMon;
        private System.Windows.Forms.TextBox txtChuyenMon;
        private System.Windows.Forms.Label lblLabelTrangThai;
        private System.Windows.Forms.TextBox txtTrangThai;
        private System.Windows.Forms.Label lblLabelSoLop;
        private System.Windows.Forms.TextBox txtSoLop;
        private DoAnWD.UI.SectionBox gbLopHocGS;
        private System.Windows.Forms.DataGridView dgvLopHoc;

        // ===== TAB 3: LỚP HỌC =====
        private DoAnWD.UI.SectionBox gbThongKeCa;
        private System.Windows.Forms.DataGridView dgvThongKeCa;
        private DoAnWD.UI.SectionBox gbSapBatDau;
        private System.Windows.Forms.DataGridView dgvSapBatDau;
        private DoAnWD.UI.SectionBox gbSapKetThuc;
        private System.Windows.Forms.DataGridView dgvSapKetThuc;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();

            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnRefresh = new DoAnWD.UI.ThemedButton();

            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabTongQuan = new System.Windows.Forms.TabPage();
            this.tabGiaSu = new System.Windows.Forms.TabPage();
            this.tabLopHoc = new System.Windows.Forms.TabPage();

            this.panelCards = new System.Windows.Forms.Panel();
            this.cardTongGS = new System.Windows.Forms.Panel();
            this.lblCardTitleGS = new System.Windows.Forms.Label();
            this.lblCountGS = new System.Windows.Forms.Label();
            this.cardTongHV = new System.Windows.Forms.Panel();
            this.lblCardTitleHV = new System.Windows.Forms.Label();
            this.lblCountHV = new System.Windows.Forms.Label();
            this.cardTongLop = new System.Windows.Forms.Panel();
            this.lblCardTitleLop = new System.Windows.Forms.Label();
            this.lblCountLop = new System.Windows.Forms.Label();

            this.gbTrangThaiGS = new DoAnWD.UI.SectionBox();
            this.lblLabelGSRanh = new System.Windows.Forms.Label();
            this.lblCountGSRanh = new System.Windows.Forms.Label();
            this.lblLabelGSBan = new System.Windows.Forms.Label();
            this.lblCountGSDangDay = new System.Windows.Forms.Label();

            this.gbTrangThaiLop = new DoAnWD.UI.SectionBox();
            this.lblLabelChuaBatDau = new System.Windows.Forms.Label();
            this.lblCountChuaBatDau = new System.Windows.Forms.Label();
            this.lblLabelDangDienRa = new System.Windows.Forms.Label();
            this.lblCountDangDienRa = new System.Windows.Forms.Label();
            this.lblLabelDaKetThuc = new System.Windows.Forms.Label();
            this.lblCountDaKetThuc = new System.Windows.Forms.Label();
            this.lblLabelLopDangHoc = new System.Windows.Forms.Label();
            this.lblCountLopDangHoc = new System.Windows.Forms.Label();

            this.gbDanhSachGS = new DoAnWD.UI.SectionBox();
            this.dgvGiaSu = new System.Windows.Forms.DataGridView();
            this.gbChiTietGS = new DoAnWD.UI.SectionBox();
            this.lblLabelMaGS = new System.Windows.Forms.Label();
            this.txtMaGS = new System.Windows.Forms.TextBox();
            this.lblLabelHoTen = new System.Windows.Forms.Label();
            this.txtHoTen = new System.Windows.Forms.TextBox();
            this.lblLabelChuyenMon = new System.Windows.Forms.Label();
            this.txtChuyenMon = new System.Windows.Forms.TextBox();
            this.lblLabelTrangThai = new System.Windows.Forms.Label();
            this.txtTrangThai = new System.Windows.Forms.TextBox();
            this.lblLabelSoLop = new System.Windows.Forms.Label();
            this.txtSoLop = new System.Windows.Forms.TextBox();
            this.gbLopHocGS = new DoAnWD.UI.SectionBox();
            this.dgvLopHoc = new System.Windows.Forms.DataGridView();

            this.gbThongKeCa = new DoAnWD.UI.SectionBox();
            this.dgvThongKeCa = new System.Windows.Forms.DataGridView();
            this.gbSapBatDau = new DoAnWD.UI.SectionBox();
            this.dgvSapBatDau = new System.Windows.Forms.DataGridView();
            this.gbSapKetThuc = new DoAnWD.UI.SectionBox();
            this.dgvSapKetThuc = new System.Windows.Forms.DataGridView();

            this.panelTop.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabTongQuan.SuspendLayout();
            this.tabGiaSu.SuspendLayout();
            this.tabLopHoc.SuspendLayout();
            this.panelCards.SuspendLayout();
            this.cardTongGS.SuspendLayout();
            this.cardTongHV.SuspendLayout();
            this.cardTongLop.SuspendLayout();
            this.gbTrangThaiGS.SuspendLayout();
            this.gbTrangThaiLop.SuspendLayout();
            this.gbDanhSachGS.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGiaSu)).BeginInit();
            this.gbChiTietGS.SuspendLayout();
            this.gbLopHocGS.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLopHoc)).BeginInit();
            this.gbThongKeCa.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvThongKeCa)).BeginInit();
            this.gbSapBatDau.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSapBatDau)).BeginInit();
            this.gbSapKetThuc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSapKetThuc)).BeginInit();
            this.SuspendLayout();

            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Controls.Add(this.btnRefresh);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1400, 70);
            this.panelTop.TabIndex = 0;

            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 17);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(350, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Tổng quan";

            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.Color.White;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(1240, 15);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(140, 40);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Làm mới";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabTongQuan);
            this.tabControl.Controls.Add(this.tabGiaSu);
            this.tabControl.Controls.Add(this.tabLopHoc);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.tabControl.ItemSize = new System.Drawing.Size(200, 35);
            this.tabControl.Location = new System.Drawing.Point(0, 70);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1400, 880);
            this.tabControl.TabIndex = 1;

            // 
            // tabTongQuan
            // 
            this.tabTongQuan.AutoScroll = true;
            this.tabTongQuan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.tabTongQuan.Controls.Add(this.panelCards);
            this.tabTongQuan.Controls.Add(this.gbTrangThaiGS);
            this.tabTongQuan.Controls.Add(this.gbTrangThaiLop);
            this.tabTongQuan.Location = new System.Drawing.Point(4, 39);
            this.tabTongQuan.Name = "tabTongQuan";
            this.tabTongQuan.Padding = new System.Windows.Forms.Padding(20);
            this.tabTongQuan.Size = new System.Drawing.Size(1392, 837);
            this.tabTongQuan.TabIndex = 0;
            this.tabTongQuan.Text = "Tổng quan";

            // 
            // tabGiaSu
            // 
            this.tabGiaSu.AutoScroll = true;
            this.tabGiaSu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.tabGiaSu.Controls.Add(this.gbDanhSachGS);
            this.tabGiaSu.Controls.Add(this.gbChiTietGS);
            this.tabGiaSu.Controls.Add(this.gbLopHocGS);
            this.tabGiaSu.Location = new System.Drawing.Point(4, 39);
            this.tabGiaSu.Name = "tabGiaSu";
            this.tabGiaSu.Padding = new System.Windows.Forms.Padding(20);
            this.tabGiaSu.Size = new System.Drawing.Size(1392, 837);
            this.tabGiaSu.TabIndex = 1;
            this.tabGiaSu.Text = "Gia sư";

            // 
            // tabLopHoc
            // 
            this.tabLopHoc.AutoScroll = true;
            this.tabLopHoc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.tabLopHoc.Controls.Add(this.gbThongKeCa);
            this.tabLopHoc.Controls.Add(this.gbSapBatDau);
            this.tabLopHoc.Controls.Add(this.gbSapKetThuc);
            this.tabLopHoc.Location = new System.Drawing.Point(4, 39);
            this.tabLopHoc.Name = "tabLopHoc";
            this.tabLopHoc.Padding = new System.Windows.Forms.Padding(20);
            this.tabLopHoc.Size = new System.Drawing.Size(1392, 837);
            this.tabLopHoc.TabIndex = 2;
            this.tabLopHoc.Text = "Lớp học";

            // 
            // panelCards
            // 
            this.panelCards.BackColor = System.Drawing.Color.Transparent;
            this.panelCards.Controls.Add(this.cardTongGS);
            this.panelCards.Controls.Add(this.cardTongHV);
            this.panelCards.Controls.Add(this.cardTongLop);
            this.panelCards.Location = new System.Drawing.Point(20, 20);
            this.panelCards.Name = "panelCards";
            this.panelCards.Size = new System.Drawing.Size(1340, 120);
            this.panelCards.TabIndex = 0;

            // 
            // cardTongGS
            // 
            this.cardTongGS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.cardTongGS.Controls.Add(this.lblCardTitleGS);
            this.cardTongGS.Controls.Add(this.lblCountGS);
            this.cardTongGS.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cardTongGS.Location = new System.Drawing.Point(0, 0);
            this.cardTongGS.Name = "cardTongGS";
            this.cardTongGS.Size = new System.Drawing.Size(430, 120);
            this.cardTongGS.TabIndex = 0;

            // 
            // lblCardTitleGS
            // 
            this.lblCardTitleGS.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardTitleGS.ForeColor = System.Drawing.Color.White;
            this.lblCardTitleGS.Location = new System.Drawing.Point(15, 15);
            this.lblCardTitleGS.Name = "lblCardTitleGS";
            this.lblCardTitleGS.Size = new System.Drawing.Size(400, 30);
            this.lblCardTitleGS.TabIndex = 0;
            this.lblCardTitleGS.Text = "Gia sư";

            // 
            // lblCountGS
            // 
            this.lblCountGS.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold);
            this.lblCountGS.ForeColor = System.Drawing.Color.White;
            this.lblCountGS.Location = new System.Drawing.Point(15, 45);
            this.lblCountGS.Name = "lblCountGS";
            this.lblCountGS.Size = new System.Drawing.Size(400, 65);
            this.lblCountGS.TabIndex = 1;
            this.lblCountGS.Text = "0";

            // 
            // cardTongHV
            // 
            this.cardTongHV.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.cardTongHV.Controls.Add(this.lblCardTitleHV);
            this.cardTongHV.Controls.Add(this.lblCountHV);
            this.cardTongHV.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cardTongHV.Location = new System.Drawing.Point(455, 0);
            this.cardTongHV.Name = "cardTongHV";
            this.cardTongHV.Size = new System.Drawing.Size(430, 120);
            this.cardTongHV.TabIndex = 1;

            // 
            // lblCardTitleHV
            // 
            this.lblCardTitleHV.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardTitleHV.ForeColor = System.Drawing.Color.White;
            this.lblCardTitleHV.Location = new System.Drawing.Point(15, 15);
            this.lblCardTitleHV.Name = "lblCardTitleHV";
            this.lblCardTitleHV.Size = new System.Drawing.Size(400, 30);
            this.lblCardTitleHV.TabIndex = 0;
            this.lblCardTitleHV.Text = "Học viên";

            // 
            // lblCountHV
            // 
            this.lblCountHV.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold);
            this.lblCountHV.ForeColor = System.Drawing.Color.White;
            this.lblCountHV.Location = new System.Drawing.Point(15, 45);
            this.lblCountHV.Name = "lblCountHV";
            this.lblCountHV.Size = new System.Drawing.Size(400, 65);
            this.lblCountHV.TabIndex = 1;
            this.lblCountHV.Text = "0";

            // 
            // cardTongLop
            // 
            this.cardTongLop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.cardTongLop.Controls.Add(this.lblCardTitleLop);
            this.cardTongLop.Controls.Add(this.lblCountLop);
            this.cardTongLop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cardTongLop.Location = new System.Drawing.Point(910, 0);
            this.cardTongLop.Name = "cardTongLop";
            this.cardTongLop.Size = new System.Drawing.Size(430, 120);
            this.cardTongLop.TabIndex = 2;

            // 
            // lblCardTitleLop
            // 
            this.lblCardTitleLop.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCardTitleLop.ForeColor = System.Drawing.Color.White;
            this.lblCardTitleLop.Location = new System.Drawing.Point(15, 15);
            this.lblCardTitleLop.Name = "lblCardTitleLop";
            this.lblCardTitleLop.Size = new System.Drawing.Size(400, 30);
            this.lblCardTitleLop.TabIndex = 0;
            this.lblCardTitleLop.Text = "Lớp học";

            // 
            // lblCountLop
            // 
            this.lblCountLop.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold);
            this.lblCountLop.ForeColor = System.Drawing.Color.White;
            this.lblCountLop.Location = new System.Drawing.Point(15, 45);
            this.lblCountLop.Name = "lblCountLop";
            this.lblCountLop.Size = new System.Drawing.Size(400, 65);
            this.lblCountLop.TabIndex = 1;
            this.lblCountLop.Text = "0";

            // 
            // gbTrangThaiGS
            // 
            this.gbTrangThaiGS.BackColor = System.Drawing.Color.White;
            this.gbTrangThaiGS.Controls.Add(this.lblLabelGSRanh);
            this.gbTrangThaiGS.Controls.Add(this.lblCountGSRanh);
            this.gbTrangThaiGS.Controls.Add(this.lblLabelGSBan);
            this.gbTrangThaiGS.Controls.Add(this.lblCountGSDangDay);
            this.gbTrangThaiGS.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbTrangThaiGS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbTrangThaiGS.Location = new System.Drawing.Point(20, 160);
            this.gbTrangThaiGS.Name = "gbTrangThaiGS";
            this.gbTrangThaiGS.Padding = new System.Windows.Forms.Padding(15);
            this.gbTrangThaiGS.Size = new System.Drawing.Size(650, 140);
            this.gbTrangThaiGS.TabIndex = 1;
            this.gbTrangThaiGS.TabStop = false;
            this.gbTrangThaiGS.Text = "Trạng thái gia sư";

            // 
            // lblLabelGSRanh
            // 
            this.lblLabelGSRanh.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblLabelGSRanh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.lblLabelGSRanh.Location = new System.Drawing.Point(30, 50);
            this.lblLabelGSRanh.Name = "lblLabelGSRanh";
            this.lblLabelGSRanh.Size = new System.Drawing.Size(120, 30);
            this.lblLabelGSRanh.TabIndex = 0;
            this.lblLabelGSRanh.Text = "Rảnh:";
            this.lblLabelGSRanh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // lblCountGSRanh
            // 
            this.lblCountGSRanh.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblCountGSRanh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.lblCountGSRanh.Location = new System.Drawing.Point(150, 35);
            this.lblCountGSRanh.Name = "lblCountGSRanh";
            this.lblCountGSRanh.Size = new System.Drawing.Size(150, 60);
            this.lblCountGSRanh.TabIndex = 1;
            this.lblCountGSRanh.Text = "0";
            this.lblCountGSRanh.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblLabelGSBan
            // 
            this.lblLabelGSBan.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblLabelGSBan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.lblLabelGSBan.Location = new System.Drawing.Point(350, 50);
            this.lblLabelGSBan.Name = "lblLabelGSBan";
            this.lblLabelGSBan.Size = new System.Drawing.Size(120, 30);
            this.lblLabelGSBan.TabIndex = 2;
            this.lblLabelGSBan.Text = "Đang có lớp:";
            this.lblLabelGSBan.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // lblCountGSDangDay
            // 
            this.lblCountGSDangDay.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblCountGSDangDay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.lblCountGSDangDay.Location = new System.Drawing.Point(470, 35);
            this.lblCountGSDangDay.Name = "lblCountGSDangDay";
            this.lblCountGSDangDay.Size = new System.Drawing.Size(150, 60);
            this.lblCountGSDangDay.TabIndex = 3;
            this.lblCountGSDangDay.Text = "0";
            this.lblCountGSDangDay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // gbTrangThaiLop
            // 
            this.gbTrangThaiLop.BackColor = System.Drawing.Color.White;
            this.gbTrangThaiLop.Controls.Add(this.lblLabelChuaBatDau);
            this.gbTrangThaiLop.Controls.Add(this.lblCountChuaBatDau);
            this.gbTrangThaiLop.Controls.Add(this.lblLabelDangDienRa);
            this.gbTrangThaiLop.Controls.Add(this.lblCountDangDienRa);
            this.gbTrangThaiLop.Controls.Add(this.lblLabelDaKetThuc);
            this.gbTrangThaiLop.Controls.Add(this.lblCountDaKetThuc);
            this.gbTrangThaiLop.Controls.Add(this.lblLabelLopDangHoc);
            this.gbTrangThaiLop.Controls.Add(this.lblCountLopDangHoc);
            this.gbTrangThaiLop.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbTrangThaiLop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbTrangThaiLop.Location = new System.Drawing.Point(690, 160);
            this.gbTrangThaiLop.Name = "gbTrangThaiLop";
            this.gbTrangThaiLop.Padding = new System.Windows.Forms.Padding(15);
            this.gbTrangThaiLop.Size = new System.Drawing.Size(670, 140);
            this.gbTrangThaiLop.TabIndex = 2;
            this.gbTrangThaiLop.TabStop = false;
            this.gbTrangThaiLop.Text = "Trạng thái lớp học";

            // 
            // lblLabelChuaBatDau
            // 
            this.lblLabelChuaBatDau.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLabelChuaBatDau.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.lblLabelChuaBatDau.Location = new System.Drawing.Point(15, 95);
            this.lblLabelChuaBatDau.Name = "lblLabelChuaBatDau";
            this.lblLabelChuaBatDau.Size = new System.Drawing.Size(130, 25);
            this.lblLabelChuaBatDau.TabIndex = 0;
            this.lblLabelChuaBatDau.Text = "Chưa bắt đầu";
            this.lblLabelChuaBatDau.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblCountChuaBatDau
            // 
            this.lblCountChuaBatDau.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblCountChuaBatDau.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.lblCountChuaBatDau.Location = new System.Drawing.Point(15, 40);
            this.lblCountChuaBatDau.Name = "lblCountChuaBatDau";
            this.lblCountChuaBatDau.Size = new System.Drawing.Size(130, 50);
            this.lblCountChuaBatDau.TabIndex = 1;
            this.lblCountChuaBatDau.Text = "0";
            this.lblCountChuaBatDau.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblLabelDangDienRa
            // 
            this.lblLabelDangDienRa.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLabelDangDienRa.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.lblLabelDangDienRa.Location = new System.Drawing.Point(180, 95);
            this.lblLabelDangDienRa.Name = "lblLabelDangDienRa";
            this.lblLabelDangDienRa.Size = new System.Drawing.Size(130, 25);
            this.lblLabelDangDienRa.TabIndex = 2;
            this.lblLabelDangDienRa.Text = "Đang diễn ra";
            this.lblLabelDangDienRa.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblCountDangDienRa
            // 
            this.lblCountDangDienRa.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblCountDangDienRa.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.lblCountDangDienRa.Location = new System.Drawing.Point(180, 40);
            this.lblCountDangDienRa.Name = "lblCountDangDienRa";
            this.lblCountDangDienRa.Size = new System.Drawing.Size(130, 50);
            this.lblCountDangDienRa.TabIndex = 3;
            this.lblCountDangDienRa.Text = "0";
            this.lblCountDangDienRa.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblLabelDaKetThuc
            // 
            this.lblLabelDaKetThuc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLabelDaKetThuc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.lblLabelDaKetThuc.Location = new System.Drawing.Point(345, 95);
            this.lblLabelDaKetThuc.Name = "lblLabelDaKetThuc";
            this.lblLabelDaKetThuc.Size = new System.Drawing.Size(130, 25);
            this.lblLabelDaKetThuc.TabIndex = 4;
            this.lblLabelDaKetThuc.Text = "Đã kết thúc";
            this.lblLabelDaKetThuc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblCountDaKetThuc
            // 
            this.lblCountDaKetThuc.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblCountDaKetThuc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.lblCountDaKetThuc.Location = new System.Drawing.Point(345, 40);
            this.lblCountDaKetThuc.Name = "lblCountDaKetThuc";
            this.lblCountDaKetThuc.Size = new System.Drawing.Size(130, 50);
            this.lblCountDaKetThuc.TabIndex = 5;
            this.lblCountDaKetThuc.Text = "0";
            this.lblCountDaKetThuc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblLabelLopDangHoc
            // 
            this.lblLabelLopDangHoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLabelLopDangHoc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.lblLabelLopDangHoc.Location = new System.Drawing.Point(510, 95);
            this.lblLabelLopDangHoc.Name = "lblLabelLopDangHoc";
            this.lblLabelLopDangHoc.Size = new System.Drawing.Size(130, 25);
            this.lblLabelLopDangHoc.TabIndex = 6;
            this.lblLabelLopDangHoc.Text = "Đang học";
            this.lblLabelLopDangHoc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblCountLopDangHoc
            // 
            this.lblCountLopDangHoc.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblCountLopDangHoc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.lblCountLopDangHoc.Location = new System.Drawing.Point(510, 40);
            this.lblCountLopDangHoc.Name = "lblCountLopDangHoc";
            this.lblCountLopDangHoc.Size = new System.Drawing.Size(130, 50);
            this.lblCountLopDangHoc.TabIndex = 7;
            this.lblCountLopDangHoc.Text = "0";
            this.lblCountLopDangHoc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // gbDanhSachGS
            // 
            this.gbDanhSachGS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDanhSachGS.BackColor = System.Drawing.Color.White;
            this.gbDanhSachGS.Controls.Add(this.dgvGiaSu);
            this.gbDanhSachGS.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbDanhSachGS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbDanhSachGS.Location = new System.Drawing.Point(20, 20);
            this.gbDanhSachGS.Name = "gbDanhSachGS";
            this.gbDanhSachGS.Padding = new System.Windows.Forms.Padding(12);
            this.gbDanhSachGS.Size = new System.Drawing.Size(900, 380);
            this.gbDanhSachGS.TabIndex = 0;
            this.gbDanhSachGS.TabStop = false;
            this.gbDanhSachGS.Text = "Danh sách gia sư";

            // 
            // dgvGiaSu
            // 
            this.dgvGiaSu.AllowUserToAddRows = false;
            this.dgvGiaSu.AllowUserToDeleteRows = false;
            this.dgvGiaSu.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvGiaSu.BackgroundColor = System.Drawing.Color.White;
            this.dgvGiaSu.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvGiaSu.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvGiaSu.ColumnHeadersHeight = 35;
            this.dgvGiaSu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvGiaSu.EnableHeadersVisualStyles = false;
            this.dgvGiaSu.Location = new System.Drawing.Point(12, 28);
            this.dgvGiaSu.Name = "dgvGiaSu";
            this.dgvGiaSu.ReadOnly = true;
            this.dgvGiaSu.RowHeadersVisible = false;
            this.dgvGiaSu.RowTemplate.Height = 30;
            this.dgvGiaSu.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvGiaSu.Size = new System.Drawing.Size(876, 340);
            this.dgvGiaSu.TabIndex = 0;
            this.dgvGiaSu.SelectionChanged += new System.EventHandler(this.dgvGiaSu_SelectionChanged);

            // 
            // gbChiTietGS
            // 
            this.gbChiTietGS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gbChiTietGS.BackColor = System.Drawing.Color.White;
            this.gbChiTietGS.Controls.Add(this.lblLabelMaGS);
            this.gbChiTietGS.Controls.Add(this.txtMaGS);
            this.gbChiTietGS.Controls.Add(this.lblLabelHoTen);
            this.gbChiTietGS.Controls.Add(this.txtHoTen);
            this.gbChiTietGS.Controls.Add(this.lblLabelChuyenMon);
            this.gbChiTietGS.Controls.Add(this.txtChuyenMon);
            this.gbChiTietGS.Controls.Add(this.lblLabelTrangThai);
            this.gbChiTietGS.Controls.Add(this.txtTrangThai);
            this.gbChiTietGS.Controls.Add(this.lblLabelSoLop);
            this.gbChiTietGS.Controls.Add(this.txtSoLop);
            this.gbChiTietGS.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbChiTietGS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbChiTietGS.Location = new System.Drawing.Point(940, 20);
            this.gbChiTietGS.Name = "gbChiTietGS";
            this.gbChiTietGS.Padding = new System.Windows.Forms.Padding(15);
            this.gbChiTietGS.Size = new System.Drawing.Size(420, 380);
            this.gbChiTietGS.TabIndex = 1;
            this.gbChiTietGS.TabStop = false;
            this.gbChiTietGS.Text = "Chi tiết gia sư";

            // 
            // lblLabelMaGS
            // 
            this.lblLabelMaGS.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblLabelMaGS.Location = new System.Drawing.Point(20, 45);
            this.lblLabelMaGS.Name = "lblLabelMaGS";
            this.lblLabelMaGS.Size = new System.Drawing.Size(120, 30);
            this.lblLabelMaGS.TabIndex = 0;
            this.lblLabelMaGS.Text = "Mã GS:";
            this.lblLabelMaGS.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // txtMaGS
            // 
            this.txtMaGS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.txtMaGS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMaGS.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMaGS.Location = new System.Drawing.Point(150, 45);
            this.txtMaGS.Name = "txtMaGS";
            this.txtMaGS.ReadOnly = true;
            this.txtMaGS.Size = new System.Drawing.Size(250, 25);
            this.txtMaGS.TabIndex = 1;

            // 
            // lblLabelHoTen
            // 
            this.lblLabelHoTen.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblLabelHoTen.Location = new System.Drawing.Point(20, 105);
            this.lblLabelHoTen.Name = "lblLabelHoTen";
            this.lblLabelHoTen.Size = new System.Drawing.Size(120, 30);
            this.lblLabelHoTen.TabIndex = 2;
            this.lblLabelHoTen.Text = "Họ tên:";
            this.lblLabelHoTen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // txtHoTen
            // 
            this.txtHoTen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.txtHoTen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHoTen.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtHoTen.Location = new System.Drawing.Point(150, 105);
            this.txtHoTen.Name = "txtHoTen";
            this.txtHoTen.ReadOnly = true;
            this.txtHoTen.Size = new System.Drawing.Size(250, 25);
            this.txtHoTen.TabIndex = 3;

            // 
            // lblLabelChuyenMon
            // 
            this.lblLabelChuyenMon.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblLabelChuyenMon.Location = new System.Drawing.Point(20, 165);
            this.lblLabelChuyenMon.Name = "lblLabelChuyenMon";
            this.lblLabelChuyenMon.Size = new System.Drawing.Size(120, 30);
            this.lblLabelChuyenMon.TabIndex = 4;
            this.lblLabelChuyenMon.Text = "Chuyên môn:";
            this.lblLabelChuyenMon.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // txtChuyenMon
            // 
            this.txtChuyenMon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.txtChuyenMon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtChuyenMon.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtChuyenMon.Location = new System.Drawing.Point(150, 165);
            this.txtChuyenMon.Name = "txtChuyenMon";
            this.txtChuyenMon.ReadOnly = true;
            this.txtChuyenMon.Size = new System.Drawing.Size(250, 25);
            this.txtChuyenMon.TabIndex = 5;

            // 
            // lblLabelTrangThai
            // 
            this.lblLabelTrangThai.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblLabelTrangThai.Location = new System.Drawing.Point(20, 225);
            this.lblLabelTrangThai.Name = "lblLabelTrangThai";
            this.lblLabelTrangThai.Size = new System.Drawing.Size(120, 30);
            this.lblLabelTrangThai.TabIndex = 6;
            this.lblLabelTrangThai.Text = "Trạng thái:";
            this.lblLabelTrangThai.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // txtTrangThai
            // 
            this.txtTrangThai.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.txtTrangThai.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTrangThai.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.txtTrangThai.ForeColor = System.Drawing.Color.White;
            this.txtTrangThai.Location = new System.Drawing.Point(150, 225);
            this.txtTrangThai.Name = "txtTrangThai";
            this.txtTrangThai.ReadOnly = true;
            this.txtTrangThai.Size = new System.Drawing.Size(250, 27);
            this.txtTrangThai.TabIndex = 7;
            this.txtTrangThai.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // 
            // lblLabelSoLop
            // 
            this.lblLabelSoLop.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblLabelSoLop.Location = new System.Drawing.Point(20, 285);
            this.lblLabelSoLop.Name = "lblLabelSoLop";
            this.lblLabelSoLop.Size = new System.Drawing.Size(120, 30);
            this.lblLabelSoLop.TabIndex = 8;
            this.lblLabelSoLop.Text = "Số lớp đang dạy:";
            this.lblLabelSoLop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // txtSoLop
            // 
            this.txtSoLop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.txtSoLop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSoLop.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.txtSoLop.Location = new System.Drawing.Point(150, 285);
            this.txtSoLop.Name = "txtSoLop";
            this.txtSoLop.ReadOnly = true;
            this.txtSoLop.Size = new System.Drawing.Size(250, 32);
            this.txtSoLop.TabIndex = 9;
            this.txtSoLop.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // 
            // gbLopHocGS
            // 
            this.gbLopHocGS.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbLopHocGS.BackColor = System.Drawing.Color.White;
            this.gbLopHocGS.Controls.Add(this.dgvLopHoc);
            this.gbLopHocGS.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbLopHocGS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbLopHocGS.Location = new System.Drawing.Point(20, 420);
            this.gbLopHocGS.Name = "gbLopHocGS";
            this.gbLopHocGS.Padding = new System.Windows.Forms.Padding(12);
            this.gbLopHocGS.Size = new System.Drawing.Size(1340, 380);
            this.gbLopHocGS.TabIndex = 2;
            this.gbLopHocGS.TabStop = false;
            this.gbLopHocGS.Text = "Lớp học của gia sư";

            // 
            // dgvLopHoc
            // 
            this.dgvLopHoc.AllowUserToAddRows = false;
            this.dgvLopHoc.AllowUserToDeleteRows = false;
            this.dgvLopHoc.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLopHoc.BackgroundColor = System.Drawing.Color.White;
            this.dgvLopHoc.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvLopHoc.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvLopHoc.ColumnHeadersHeight = 35;
            this.dgvLopHoc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLopHoc.EnableHeadersVisualStyles = false;
            this.dgvLopHoc.Location = new System.Drawing.Point(12, 28);
            this.dgvLopHoc.Name = "dgvLopHoc";
            this.dgvLopHoc.ReadOnly = true;
            this.dgvLopHoc.RowHeadersVisible = false;
            this.dgvLopHoc.RowTemplate.Height = 30;
            this.dgvLopHoc.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLopHoc.Size = new System.Drawing.Size(1316, 340);
            this.dgvLopHoc.TabIndex = 0;

            // 
            // gbThongKeCa
            // 
            this.gbThongKeCa.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbThongKeCa.BackColor = System.Drawing.Color.White;
            this.gbThongKeCa.Controls.Add(this.dgvThongKeCa);
            this.gbThongKeCa.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbThongKeCa.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbThongKeCa.Location = new System.Drawing.Point(20, 20);
            this.gbThongKeCa.Name = "gbThongKeCa";
            this.gbThongKeCa.Padding = new System.Windows.Forms.Padding(12);
            this.gbThongKeCa.Size = new System.Drawing.Size(880, 380);
            this.gbThongKeCa.TabIndex = 0;
            this.gbThongKeCa.TabStop = false;
            this.gbThongKeCa.Text = "Thống kê theo ca học";

            // 
            // dgvThongKeCa
            // 
            this.dgvThongKeCa.AllowUserToAddRows = false;
            this.dgvThongKeCa.AllowUserToDeleteRows = false;
            this.dgvThongKeCa.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvThongKeCa.BackgroundColor = System.Drawing.Color.White;
            this.dgvThongKeCa.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvThongKeCa.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvThongKeCa.ColumnHeadersHeight = 35;
            this.dgvThongKeCa.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvThongKeCa.EnableHeadersVisualStyles = false;
            this.dgvThongKeCa.Location = new System.Drawing.Point(12, 28);
            this.dgvThongKeCa.Name = "dgvThongKeCa";
            this.dgvThongKeCa.ReadOnly = true;
            this.dgvThongKeCa.RowHeadersVisible = false;
            this.dgvThongKeCa.RowTemplate.Height = 30;
            this.dgvThongKeCa.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvThongKeCa.Size = new System.Drawing.Size(856, 340);
            this.dgvThongKeCa.TabIndex = 0;

            // 
            // gbSapBatDau
            // 
            this.gbSapBatDau.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSapBatDau.BackColor = System.Drawing.Color.White;
            this.gbSapBatDau.Controls.Add(this.dgvSapBatDau);
            this.gbSapBatDau.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbSapBatDau.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbSapBatDau.Location = new System.Drawing.Point(920, 20);
            this.gbSapBatDau.Name = "gbSapBatDau";
            this.gbSapBatDau.Padding = new System.Windows.Forms.Padding(12);
            this.gbSapBatDau.Size = new System.Drawing.Size(440, 380);
            this.gbSapBatDau.TabIndex = 1;
            this.gbSapBatDau.TabStop = false;
            this.gbSapBatDau.Text = "Sắp bắt đầu (7 ngày)";

            // 
            // dgvSapBatDau
            // 
            this.dgvSapBatDau.AllowUserToAddRows = false;
            this.dgvSapBatDau.AllowUserToDeleteRows = false;
            this.dgvSapBatDau.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSapBatDau.BackgroundColor = System.Drawing.Color.White;
            this.dgvSapBatDau.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvSapBatDau.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvSapBatDau.ColumnHeadersHeight = 32;
            this.dgvSapBatDau.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSapBatDau.EnableHeadersVisualStyles = false;
            this.dgvSapBatDau.Location = new System.Drawing.Point(12, 28);
            this.dgvSapBatDau.Name = "dgvSapBatDau";
            this.dgvSapBatDau.ReadOnly = true;
            this.dgvSapBatDau.RowHeadersVisible = false;
            this.dgvSapBatDau.RowTemplate.Height = 28;
            this.dgvSapBatDau.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSapBatDau.Size = new System.Drawing.Size(416, 340);
            this.dgvSapBatDau.TabIndex = 0;

            // 
            // gbSapKetThuc
            // 
            this.gbSapKetThuc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSapKetThuc.BackColor = System.Drawing.Color.White;
            this.gbSapKetThuc.Controls.Add(this.dgvSapKetThuc);
            this.gbSapKetThuc.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.gbSapKetThuc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbSapKetThuc.Location = new System.Drawing.Point(20, 420);
            this.gbSapKetThuc.Name = "gbSapKetThuc";
            this.gbSapKetThuc.Padding = new System.Windows.Forms.Padding(12);
            this.gbSapKetThuc.Size = new System.Drawing.Size(1340, 380);
            this.gbSapKetThuc.TabIndex = 2;
            this.gbSapKetThuc.TabStop = false;
            this.gbSapKetThuc.Text = "Sắp kết thúc (7 ngày)";

            // 
            // dgvSapKetThuc
            // 
            this.dgvSapKetThuc.AllowUserToAddRows = false;
            this.dgvSapKetThuc.AllowUserToDeleteRows = false;
            this.dgvSapKetThuc.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSapKetThuc.BackgroundColor = System.Drawing.Color.White;
            this.dgvSapKetThuc.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvSapKetThuc.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvSapKetThuc.ColumnHeadersHeight = 35;
            this.dgvSapKetThuc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSapKetThuc.EnableHeadersVisualStyles = false;
            this.dgvSapKetThuc.Location = new System.Drawing.Point(12, 28);
            this.dgvSapKetThuc.Name = "dgvSapKetThuc";
            this.dgvSapKetThuc.ReadOnly = true;
            this.dgvSapKetThuc.RowHeadersVisible = false;
            this.dgvSapKetThuc.RowTemplate.Height = 30;
            this.dgvSapKetThuc.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSapKetThuc.Size = new System.Drawing.Size(1316, 340);
            this.dgvSapKetThuc.TabIndex = 0;

            // 
            // frmThongKe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(1400, 950);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(1200, 700);
            this.Name = "frmThongKe";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thống Kê & Dashboard";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabTongQuan.ResumeLayout(false);
            this.tabGiaSu.ResumeLayout(false);
            this.tabLopHoc.ResumeLayout(false);
            this.panelCards.ResumeLayout(false);
            this.cardTongGS.ResumeLayout(false);
            this.cardTongHV.ResumeLayout(false);
            this.cardTongLop.ResumeLayout(false);
            this.gbTrangThaiGS.ResumeLayout(false);
            this.gbTrangThaiLop.ResumeLayout(false);
            this.gbDanhSachGS.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvGiaSu)).EndInit();
            this.gbChiTietGS.ResumeLayout(false);
            this.gbChiTietGS.PerformLayout();
            this.gbLopHocGS.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLopHoc)).EndInit();
            this.gbThongKeCa.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvThongKeCa)).EndInit();
            this.gbSapBatDau.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSapBatDau)).EndInit();
            this.gbSapKetThuc.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSapKetThuc)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
