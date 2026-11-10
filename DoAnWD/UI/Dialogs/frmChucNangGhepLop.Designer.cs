namespace DoAnWD.UI.Dialogs
{
    partial class frmChucNangGhepLop
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Giao diện của hộp thoại được dựng trong code (BuildUi) bằng TableLayoutPanel
        /// để co giãn theo kích thước cửa sổ.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1240, 780);
            this.MinimumSize = new System.Drawing.Size(1000, 680);
            this.Name = "frmChucNangGhepLop";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ghép lớp mới";
            this.ResumeLayout(false);
        }
    }
}
