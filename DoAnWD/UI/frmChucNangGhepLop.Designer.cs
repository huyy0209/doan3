namespace DoAnWD
{
    partial class frmChucNangGhepLop
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox gbHocVien;
        private System.Windows.Forms.GroupBox gbGiaSu;
        private System.Windows.Forms.GroupBox gbThongTinLop;
        private System.Windows.Forms.ComboBox cboHocVien;
        private System.Windows.Forms.ComboBox cboGiaSu;
        private System.Windows.Forms.TextBox txtHocPhi;
        private System.Windows.Forms.ComboBox cboTrangThai;
        private System.Windows.Forms.Label lblHocVien;
        private System.Windows.Forms.Label lblGiaSu;
        private System.Windows.Forms.Label lblHocPhi;
        private System.Windows.Forms.Label lblTrangThai;
        private System.Windows.Forms.Button btnGhepLop;
        private System.Windows.Forms.Button btnHuy;
        private System.Windows.Forms.Label lblSDT_HV;
        private System.Windows.Forms.Label lblDiaChi_HV;
        private System.Windows.Forms.Label lblSDT_GS;
        private System.Windows.Forms.Label lblChuyenMon_GS;
        private System.Windows.Forms.TextBox txtSDT_HV;
        private System.Windows.Forms.TextBox txtDiaChi_HV;
        private System.Windows.Forms.TextBox txtSDT_GS;
        private System.Windows.Forms.TextBox txtChuyenMon_GS;
        private System.Windows.Forms.Label lblThoiGianBatDau;
        private System.Windows.Forms.Label lblThoiGianKetThuc;
        private System.Windows.Forms.Label lblCaHoc;
        private System.Windows.Forms.DateTimePicker dtpBatDau;
        private System.Windows.Forms.DateTimePicker dtpKetThuc;
        private System.Windows.Forms.ComboBox cboCaHoc;

        // ✨ CÁC CONTROLS MỚI CHO LỊCH GIA SƯ
        private System.Windows.Forms.GroupBox gbLichGiaSu;
        private System.Windows.Forms.ListBox lstLichGiaSu;
        private System.Windows.Forms.Label lblTitleLichGS;
        private System.Windows.Forms.Label lblWarningCaHoc;

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
            this.panelContent = new System.Windows.Forms.Panel();
            this.gbHocVien = new System.Windows.Forms.GroupBox();
            this.lblHocVien = new System.Windows.Forms.Label();
            this.cboHocVien = new System.Windows.Forms.ComboBox();
            this.lblSDT_HV = new System.Windows.Forms.Label();
            this.txtSDT_HV = new System.Windows.Forms.TextBox();
            this.lblDiaChi_HV = new System.Windows.Forms.Label();
            this.txtDiaChi_HV = new System.Windows.Forms.TextBox();
            this.gbGiaSu = new System.Windows.Forms.GroupBox();
            this.lblGiaSu = new System.Windows.Forms.Label();
            this.cboGiaSu = new System.Windows.Forms.ComboBox();
            this.lblSDT_GS = new System.Windows.Forms.Label();
            this.txtSDT_GS = new System.Windows.Forms.TextBox();
            this.lblChuyenMon_GS = new System.Windows.Forms.Label();
            this.txtChuyenMon_GS = new System.Windows.Forms.TextBox();
            this.gbThongTinLop = new System.Windows.Forms.GroupBox();
            this.lblHocPhi = new System.Windows.Forms.Label();
            this.txtHocPhi = new System.Windows.Forms.TextBox();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.cboTrangThai = new System.Windows.Forms.ComboBox();
            this.lblCaHoc = new System.Windows.Forms.Label();
            this.cboCaHoc = new System.Windows.Forms.ComboBox();
            this.lblThoiGianBatDau = new System.Windows.Forms.Label();
            this.dtpBatDau = new System.Windows.Forms.DateTimePicker();
            this.lblThoiGianKetThuc = new System.Windows.Forms.Label();
            this.dtpKetThuc = new System.Windows.Forms.DateTimePicker();
            this.lblWarningCaHoc = new System.Windows.Forms.Label();
            this.gbLichGiaSu = new System.Windows.Forms.GroupBox();
            this.lblTitleLichGS = new System.Windows.Forms.Label();
            this.lstLichGiaSu = new System.Windows.Forms.ListBox();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnGhepLop = new System.Windows.Forms.Button();
            this.btnHuy = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.panelContent.SuspendLayout();
            this.gbHocVien.SuspendLayout();
            this.gbGiaSu.SuspendLayout();
            this.gbThongTinLop.SuspendLayout();
            this.gbLichGiaSu.SuspendLayout();
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
            this.panelTop.Size = new System.Drawing.Size(800, 60);
            this.panelTop.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(300, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "➕ GHÉP LỚP MỚI (Option 2)";
            // 
            // panelContent
            // 
            this.panelContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.panelContent.Controls.Add(this.gbHocVien);
            this.panelContent.Controls.Add(this.gbGiaSu);
            this.panelContent.Controls.Add(this.gbThongTinLop);
            this.panelContent.Controls.Add(this.gbLichGiaSu);
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(0, 60);
            this.panelContent.Name = "panelContent";
            this.panelContent.Padding = new System.Windows.Forms.Padding(15);
            this.panelContent.Size = new System.Drawing.Size(800, 570);
            this.panelContent.TabIndex = 1;
            // 
            // gbHocVien
            // 
            this.gbHocVien.BackColor = System.Drawing.Color.White;
            this.gbHocVien.Controls.Add(this.lblHocVien);
            this.gbHocVien.Controls.Add(this.cboHocVien);
            this.gbHocVien.Controls.Add(this.lblSDT_HV);
            this.gbHocVien.Controls.Add(this.txtSDT_HV);
            this.gbHocVien.Controls.Add(this.lblDiaChi_HV);
            this.gbHocVien.Controls.Add(this.txtDiaChi_HV);
            this.gbHocVien.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.gbHocVien.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbHocVien.Location = new System.Drawing.Point(20, 20);
            this.gbHocVien.Name = "gbHocVien";
            this.gbHocVien.Padding = new System.Windows.Forms.Padding(15);
            this.gbHocVien.Size = new System.Drawing.Size(360, 180);
            this.gbHocVien.TabIndex = 0;
            this.gbHocVien.TabStop = false;
            this.gbHocVien.Text = "👨‍🎓 Chọn Học Viên";
            // 
            // lblHocVien
            // 
            this.lblHocVien.AutoSize = true;
            this.lblHocVien.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblHocVien.Location = new System.Drawing.Point(15, 30);
            this.lblHocVien.Name = "lblHocVien";
            this.lblHocVien.Size = new System.Drawing.Size(63, 17);
            this.lblHocVien.TabIndex = 0;
            this.lblHocVien.Text = "Học Viên:";
            // 
            // cboHocVien
            // 
            this.cboHocVien.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboHocVien.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboHocVien.FormattingEnabled = true;
            this.cboHocVien.Location = new System.Drawing.Point(120, 27);
            this.cboHocVien.Name = "cboHocVien";
            this.cboHocVien.Size = new System.Drawing.Size(220, 25);
            this.cboHocVien.TabIndex = 1;
            this.cboHocVien.SelectedIndexChanged += new System.EventHandler(this.cboHocVien_SelectedIndexChanged);
            // 
            // lblSDT_HV
            // 
            this.lblSDT_HV.AutoSize = true;
            this.lblSDT_HV.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSDT_HV.Location = new System.Drawing.Point(15, 70);
            this.lblSDT_HV.Name = "lblSDT_HV";
            this.lblSDT_HV.Size = new System.Drawing.Size(92, 17);
            this.lblSDT_HV.TabIndex = 2;
            this.lblSDT_HV.Text = "Số Điện Thoại:";
            // 
            // txtSDT_HV
            // 
            this.txtSDT_HV.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.txtSDT_HV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSDT_HV.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSDT_HV.Location = new System.Drawing.Point(120, 67);
            this.txtSDT_HV.Name = "txtSDT_HV";
            this.txtSDT_HV.ReadOnly = true;
            this.txtSDT_HV.Size = new System.Drawing.Size(220, 25);
            this.txtSDT_HV.TabIndex = 2;
            // 
            // lblDiaChi_HV
            // 
            this.lblDiaChi_HV.AutoSize = true;
            this.lblDiaChi_HV.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblDiaChi_HV.Location = new System.Drawing.Point(15, 110);
            this.lblDiaChi_HV.Name = "lblDiaChi_HV";
            this.lblDiaChi_HV.Size = new System.Drawing.Size(52, 17);
            this.lblDiaChi_HV.TabIndex = 3;
            this.lblDiaChi_HV.Text = "Địa Chỉ:";
            // 
            // txtDiaChi_HV
            // 
            this.txtDiaChi_HV.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.txtDiaChi_HV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDiaChi_HV.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDiaChi_HV.Location = new System.Drawing.Point(120, 107);
            this.txtDiaChi_HV.Multiline = true;
            this.txtDiaChi_HV.Name = "txtDiaChi_HV";
            this.txtDiaChi_HV.ReadOnly = true;
            this.txtDiaChi_HV.Size = new System.Drawing.Size(220, 50);
            this.txtDiaChi_HV.TabIndex = 3;
            // 
            // gbGiaSu
            // 
            this.gbGiaSu.BackColor = System.Drawing.Color.White;
            this.gbGiaSu.Controls.Add(this.lblGiaSu);
            this.gbGiaSu.Controls.Add(this.cboGiaSu);
            this.gbGiaSu.Controls.Add(this.lblSDT_GS);
            this.gbGiaSu.Controls.Add(this.txtSDT_GS);
            this.gbGiaSu.Controls.Add(this.lblChuyenMon_GS);
            this.gbGiaSu.Controls.Add(this.txtChuyenMon_GS);
            this.gbGiaSu.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.gbGiaSu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbGiaSu.Location = new System.Drawing.Point(400, 20);
            this.gbGiaSu.Name = "gbGiaSu";
            this.gbGiaSu.Padding = new System.Windows.Forms.Padding(15);
            this.gbGiaSu.Size = new System.Drawing.Size(380, 180);
            this.gbGiaSu.TabIndex = 1;
            this.gbGiaSu.TabStop = false;
            this.gbGiaSu.Text = "👨‍🏫 Chọn Gia Sư";
            // 
            // lblGiaSu
            // 
            this.lblGiaSu.AutoSize = true;
            this.lblGiaSu.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblGiaSu.Location = new System.Drawing.Point(15, 30);
            this.lblGiaSu.Name = "lblGiaSu";
            this.lblGiaSu.Size = new System.Drawing.Size(49, 17);
            this.lblGiaSu.TabIndex = 0;
            this.lblGiaSu.Text = "Gia Sư:";
            // 
            // cboGiaSu
            // 
            this.cboGiaSu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGiaSu.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboGiaSu.FormattingEnabled = true;
            this.cboGiaSu.Location = new System.Drawing.Point(130, 27);
            this.cboGiaSu.Name = "cboGiaSu";
            this.cboGiaSu.Size = new System.Drawing.Size(230, 25);
            this.cboGiaSu.TabIndex = 1;
            this.cboGiaSu.SelectedIndexChanged += new System.EventHandler(this.cboGiaSu_SelectedIndexChanged);
            // 
            // lblSDT_GS
            // 
            this.lblSDT_GS.AutoSize = true;
            this.lblSDT_GS.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSDT_GS.Location = new System.Drawing.Point(15, 70);
            this.lblSDT_GS.Name = "lblSDT_GS";
            this.lblSDT_GS.Size = new System.Drawing.Size(92, 17);
            this.lblSDT_GS.TabIndex = 2;
            this.lblSDT_GS.Text = "Số Điện Thoại:";
            // 
            // txtSDT_GS
            // 
            this.txtSDT_GS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.txtSDT_GS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSDT_GS.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSDT_GS.Location = new System.Drawing.Point(130, 67);
            this.txtSDT_GS.Name = "txtSDT_GS";
            this.txtSDT_GS.ReadOnly = true;
            this.txtSDT_GS.Size = new System.Drawing.Size(230, 25);
            this.txtSDT_GS.TabIndex = 2;
            // 
            // lblChuyenMon_GS
            // 
            this.lblChuyenMon_GS.AutoSize = true;
            this.lblChuyenMon_GS.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblChuyenMon_GS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.lblChuyenMon_GS.Location = new System.Drawing.Point(15, 110);
            this.lblChuyenMon_GS.Name = "lblChuyenMon_GS";
            this.lblChuyenMon_GS.Size = new System.Drawing.Size(113, 17);
            this.lblChuyenMon_GS.TabIndex = 3;
            this.lblChuyenMon_GS.Text = "📚 Chuyên Môn:";
            // 
            // txtChuyenMon_GS
            // 
            this.txtChuyenMon_GS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(243)))), ((int)(((byte)(224)))));
            this.txtChuyenMon_GS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtChuyenMon_GS.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.txtChuyenMon_GS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.txtChuyenMon_GS.Location = new System.Drawing.Point(130, 107);
            this.txtChuyenMon_GS.Name = "txtChuyenMon_GS";
            this.txtChuyenMon_GS.ReadOnly = true;
            this.txtChuyenMon_GS.Size = new System.Drawing.Size(230, 25);
            this.txtChuyenMon_GS.TabIndex = 3;
            this.txtChuyenMon_GS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // gbThongTinLop
            // 
            this.gbThongTinLop.BackColor = System.Drawing.Color.White;
            this.gbThongTinLop.Controls.Add(this.lblHocPhi);
            this.gbThongTinLop.Controls.Add(this.txtHocPhi);
            this.gbThongTinLop.Controls.Add(this.lblTrangThai);
            this.gbThongTinLop.Controls.Add(this.cboTrangThai);
            this.gbThongTinLop.Controls.Add(this.lblCaHoc);
            this.gbThongTinLop.Controls.Add(this.cboCaHoc);
            this.gbThongTinLop.Controls.Add(this.lblWarningCaHoc);
            this.gbThongTinLop.Controls.Add(this.lblThoiGianBatDau);
            this.gbThongTinLop.Controls.Add(this.dtpBatDau);
            this.gbThongTinLop.Controls.Add(this.lblThoiGianKetThuc);
            this.gbThongTinLop.Controls.Add(this.dtpKetThuc);
            this.gbThongTinLop.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.gbThongTinLop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.gbThongTinLop.Location = new System.Drawing.Point(20, 220);
            this.gbThongTinLop.Name = "gbThongTinLop";
            this.gbThongTinLop.Padding = new System.Windows.Forms.Padding(15);
            this.gbThongTinLop.Size = new System.Drawing.Size(760, 200);
            this.gbThongTinLop.TabIndex = 2;
            this.gbThongTinLop.TabStop = false;
            this.gbThongTinLop.Text = "💰 Thông Tin Lớp Học & Ca Học";
            // 
            // lblHocPhi
            // 
            this.lblHocPhi.AutoSize = true;
            this.lblHocPhi.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblHocPhi.Location = new System.Drawing.Point(25, 45);
            this.lblHocPhi.Name = "lblHocPhi";
            this.lblHocPhi.Size = new System.Drawing.Size(94, 17);
            this.lblHocPhi.TabIndex = 0;
            this.lblHocPhi.Text = "Học Phí (VNĐ):";
            // 
            // txtHocPhi
            // 
            this.txtHocPhi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHocPhi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtHocPhi.Location = new System.Drawing.Point(150, 42);
            this.txtHocPhi.Name = "txtHocPhi";
            this.txtHocPhi.Size = new System.Drawing.Size(200, 25);
            this.txtHocPhi.TabIndex = 1;
            // 
            // lblTrangThai
            // 
            this.lblTrangThai.AutoSize = true;
            this.lblTrangThai.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTrangThai.Location = new System.Drawing.Point(400, 45);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Size = new System.Drawing.Size(72, 17);
            this.lblTrangThai.TabIndex = 2;
            this.lblTrangThai.Text = "Trạng Thái:";
            // 
            // cboTrangThai
            // 
            this.cboTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrangThai.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboTrangThai.FormattingEnabled = true;
            this.cboTrangThai.Location = new System.Drawing.Point(500, 42);
            this.cboTrangThai.Name = "cboTrangThai";
            this.cboTrangThai.Size = new System.Drawing.Size(200, 25);
            this.cboTrangThai.TabIndex = 3;
            // 
            // lblCaHoc
            // 
            this.lblCaHoc.AutoSize = true;
            this.lblCaHoc.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblCaHoc.Location = new System.Drawing.Point(25, 85);
            this.lblCaHoc.Name = "lblCaHoc";
            this.lblCaHoc.Size = new System.Drawing.Size(75, 17);
            this.lblCaHoc.TabIndex = 4;
            this.lblCaHoc.Text = "⏰ Ca Học:";
            // 
            // cboCaHoc
            // 
            this.cboCaHoc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCaHoc.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboCaHoc.FormattingEnabled = true;
            this.cboCaHoc.Location = new System.Drawing.Point(150, 82);
            this.cboCaHoc.Name = "cboCaHoc";
            this.cboCaHoc.Size = new System.Drawing.Size(200, 25);
            this.cboCaHoc.TabIndex = 5;
            // 
            // lblWarningCaHoc
            // 
            this.lblWarningCaHoc.AutoSize = true;
            this.lblWarningCaHoc.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWarningCaHoc.Location = new System.Drawing.Point(360, 85);
            this.lblWarningCaHoc.Name = "lblWarningCaHoc";
            this.lblWarningCaHoc.Size = new System.Drawing.Size(0, 15);
            this.lblWarningCaHoc.TabIndex = 6;
            this.lblWarningCaHoc.Visible = false;
            // 
            // lblThoiGianBatDau
            // 
            this.lblThoiGianBatDau.AutoSize = true;
            this.lblThoiGianBatDau.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblThoiGianBatDau.Location = new System.Drawing.Point(25, 135);
            this.lblThoiGianBatDau.Name = "lblThoiGianBatDau";
            this.lblThoiGianBatDau.Size = new System.Drawing.Size(78, 17);
            this.lblThoiGianBatDau.TabIndex = 7;
            this.lblThoiGianBatDau.Text = "📅 Bắt Đầu:";
            // 
            // dtpBatDau
            // 
            this.dtpBatDau.CustomFormat = "dd/MM/yyyy HH:mm";
            this.dtpBatDau.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpBatDau.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpBatDau.Location = new System.Drawing.Point(150, 132);
            this.dtpBatDau.Name = "dtpBatDau";
            this.dtpBatDau.Size = new System.Drawing.Size(200, 25);
            this.dtpBatDau.TabIndex = 8;
            // 
            // lblThoiGianKetThuc
            // 
            this.lblThoiGianKetThuc.AutoSize = true;
            this.lblThoiGianKetThuc.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblThoiGianKetThuc.Location = new System.Drawing.Point(400, 135);
            this.lblThoiGianKetThuc.Name = "lblThoiGianKetThuc";
            this.lblThoiGianKetThuc.Size = new System.Drawing.Size(83, 17);
            this.lblThoiGianKetThuc.TabIndex = 9;
            this.lblThoiGianKetThuc.Text = "🏁 Kết Thúc:";
            // 
            // dtpKetThuc
            // 
            this.dtpKetThuc.CustomFormat = "dd/MM/yyyy HH:mm";
            this.dtpKetThuc.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpKetThuc.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpKetThuc.Location = new System.Drawing.Point(500, 132);
            this.dtpKetThuc.Name = "dtpKetThuc";
            this.dtpKetThuc.Size = new System.Drawing.Size(200, 25);
            this.dtpKetThuc.TabIndex = 10;
            // 
            // gbLichGiaSu
            // 
            this.gbLichGiaSu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(243)))), ((int)(((byte)(224)))));
            this.gbLichGiaSu.Controls.Add(this.lblTitleLichGS);
            this.gbLichGiaSu.Controls.Add(this.lstLichGiaSu);
            this.gbLichGiaSu.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.gbLichGiaSu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.gbLichGiaSu.Location = new System.Drawing.Point(20, 430);
            this.gbLichGiaSu.Name = "gbLichGiaSu";
            this.gbLichGiaSu.Size = new System.Drawing.Size(760, 120);
            this.gbLichGiaSu.TabIndex = 3;
            this.gbLichGiaSu.TabStop = false;
            this.gbLichGiaSu.Text = "📅 Lịch Dạy Của Gia Sư";
            // 
            // lblTitleLichGS
            // 
            this.lblTitleLichGS.AutoSize = true;
            this.lblTitleLichGS.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblTitleLichGS.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lblTitleLichGS.Location = new System.Drawing.Point(15, 22);
            this.lblTitleLichGS.Name = "lblTitleLichGS";
            this.lblTitleLichGS.Size = new System.Drawing.Size(250, 15);
            this.lblTitleLichGS.TabIndex = 0;
            this.lblTitleLichGS.Text = "Chọn gia sư để xem lịch dạy hiện tại...";
            // 
            // lstLichGiaSu
            // 
            this.lstLichGiaSu.BackColor = System.Drawing.Color.White;
            this.lstLichGiaSu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstLichGiaSu.Font = new System.Drawing.Font("Consolas", 9F);
            this.lstLichGiaSu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            this.lstLichGiaSu.FormattingEnabled = true;
            this.lstLichGiaSu.ItemHeight = 14;
            this.lstLichGiaSu.Location = new System.Drawing.Point(15, 42);
            this.lstLichGiaSu.Name = "lstLichGiaSu";
            this.lstLichGiaSu.Size = new System.Drawing.Size(730, 58);
            this.lstLichGiaSu.TabIndex = 1;
            // 
            // panelButtons
            // 
            this.panelButtons.BackColor = System.Drawing.Color.White;
            this.panelButtons.Controls.Add(this.btnGhepLop);
            this.panelButtons.Controls.Add(this.btnHuy);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 630);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(20);
            this.panelButtons.Size = new System.Drawing.Size(800, 80);
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
            this.btnGhepLop.Location = new System.Drawing.Point(250, 15);
            this.btnGhepLop.Name = "btnGhepLop";
            this.btnGhepLop.Size = new System.Drawing.Size(150, 50);
            this.btnGhepLop.TabIndex = 0;
            this.btnGhepLop.Text = "✅ Ghép Lớp";
            this.btnGhepLop.UseVisualStyleBackColor = false;
            this.btnGhepLop.Click += new System.EventHandler(this.btnGhepLop_Click);
            // 
            // btnHuy
            // 
            this.btnHuy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
            this.btnHuy.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHuy.FlatAppearance.BorderSize = 0;
            this.btnHuy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHuy.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnHuy.ForeColor = System.Drawing.Color.White;
            this.btnHuy.Location = new System.Drawing.Point(420, 15);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(150, 50);
            this.btnHuy.TabIndex = 1;
            this.btnHuy.Text = "❌ Hủy";
            this.btnHuy.UseVisualStyleBackColor = false;
            this.btnHuy.Click += new System.EventHandler(this.btnHuy_Click);
            // 
            // frmChucNangGhepLop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 710);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmChucNangGhepLop";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ghép Lớp - Quản lý Ca học";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelContent.ResumeLayout(false);
            this.gbHocVien.ResumeLayout(false);
            this.gbHocVien.PerformLayout();
            this.gbGiaSu.ResumeLayout(false);
            this.gbGiaSu.PerformLayout();
            this.gbThongTinLop.ResumeLayout(false);
            this.gbThongTinLop.PerformLayout();
            this.gbLichGiaSu.ResumeLayout(false);
            this.gbLichGiaSu.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}