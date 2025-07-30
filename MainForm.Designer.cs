namespace Bulk_Editor
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.txtFolderPath = new System.Windows.Forms.TextBox();
            this.lstFiles = new System.Windows.Forms.ListBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grpTools = new System.Windows.Forms.GroupBox();
            this.chkFixSourceHyperlinks = new System.Windows.Forms.CheckBox();
            this.chkAppendContentID = new System.Windows.Forms.CheckBox();
            this.chkFixInternalHyperlink = new System.Windows.Forms.CheckBox();
            this.chkFixTitles = new System.Windows.Forms.CheckBox();
            this.btnRunTools = new System.Windows.Forms.Button();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.grpTools.SuspendLayout();
            this.SuspendLayout();
            //
            // btnSelectFolder
            //
            this.btnSelectFolder.Location = new System.Drawing.Point(12, 12);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(120, 30);
            this.btnSelectFolder.TabIndex = 0;
            this.btnSelectFolder.Text = "Select Folder";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.BtnSelectFolder_Click);
            //
            // txtFolderPath
            //
            this.txtFolderPath.Location = new System.Drawing.Point(138, 15);
            this.txtFolderPath.Name = "txtFolderPath";
            this.txtFolderPath.ReadOnly = true;
            this.txtFolderPath.Size = new System.Drawing.Size(540, 22);
            this.txtFolderPath.TabIndex = 1;
            //
            // lstFiles
            //
            this.lstFiles.FormattingEnabled = true;
            this.lstFiles.Location = new System.Drawing.Point(12, 50);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(666, 251);
            this.lstFiles.TabIndex = 2;
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 410);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 15);
            this.lblStatus.TabIndex = 3;
            //
            // grpTools
            //
            this.grpTools.Controls.Add(this.chkFixTitles);
            this.grpTools.Controls.Add(this.chkFixInternalHyperlink);
            this.grpTools.Controls.Add(this.chkAppendContentID);
            this.grpTools.Controls.Add(this.chkFixSourceHyperlinks);
            this.grpTools.Location = new System.Drawing.Point(12, 307);
            this.grpTools.Name = "grpTools";
            this.grpTools.Size = new System.Drawing.Size(540, 100);
            this.grpTools.TabIndex = 4;
            this.grpTools.TabStop = false;
            this.grpTools.Text = "Tools";
            //
            // chkFixSourceHyperlinks
            //
            this.chkFixSourceHyperlinks.AutoSize = true;
            this.chkFixSourceHyperlinks.Location = new System.Drawing.Point(10, 20);
            this.chkFixSourceHyperlinks.Name = "chkFixSourceHyperlinks";
            this.chkFixSourceHyperlinks.Size = new System.Drawing.Size(150, 19);
            this.chkFixSourceHyperlinks.TabIndex = 0;
            this.chkFixSourceHyperlinks.Text = "Fix Source Hyperlinks";
            this.chkFixSourceHyperlinks.UseVisualStyleBackColor = true;
            //
            // chkAppendContentID
            //
            this.chkAppendContentID.AutoSize = true;
            this.chkAppendContentID.Location = new System.Drawing.Point(10, 45);
            this.chkAppendContentID.Name = "chkAppendContentID";
            this.chkAppendContentID.Size = new System.Drawing.Size(130, 19);
            this.chkAppendContentID.TabIndex = 1;
            this.chkAppendContentID.Text = "Append Content ID";
            this.chkAppendContentID.UseVisualStyleBackColor = true;
            //
            // chkFixInternalHyperlink
            //
            this.chkFixInternalHyperlink.AutoSize = true;
            this.chkFixInternalHyperlink.Location = new System.Drawing.Point(270, 20);
            this.chkFixInternalHyperlink.Name = "chkFixInternalHyperlink";
            this.chkFixInternalHyperlink.Size = new System.Drawing.Size(150, 19);
            this.chkFixInternalHyperlink.TabIndex = 2;
            this.chkFixInternalHyperlink.Text = "Fix Internal Hyperlink";
            this.chkFixInternalHyperlink.UseVisualStyleBackColor = true;
            //
            // chkFixTitles
            //
            this.chkFixTitles.AutoSize = true;
            this.chkFixTitles.Location = new System.Drawing.Point(270, 45);
            this.chkFixTitles.Name = "chkFixTitles";
            this.chkFixTitles.Size = new System.Drawing.Size(90, 19);
            this.chkFixTitles.TabIndex = 3;
            this.chkFixTitles.Text = "Fix Titles";
            this.chkFixTitles.UseVisualStyleBackColor = true;
            //
            // btnRunTools
            //
            this.btnRunTools.Location = new System.Drawing.Point(558, 337);
            this.btnRunTools.Name = "btnRunTools";
            this.btnRunTools.Size = new System.Drawing.Size(120, 30);
            this.btnRunTools.TabIndex = 5;
            this.btnRunTools.Text = "Run Tools";
            this.btnRunTools.UseVisualStyleBackColor = true;
            this.btnRunTools.Click += new System.EventHandler(this.BtnRunTools_Click);
            //
            // btnSelectFile
            //
            this.btnSelectFile.Location = new System.Drawing.Point(12, 48);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(120, 30);
            this.btnSelectFile.TabIndex = 6;
            this.btnSelectFile.Text = "Select File";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.BtnSelectFile_Click);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 435);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.btnRunTools);
            this.Controls.Add(this.grpTools);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lstFiles);
            this.Controls.Add(this.txtFolderPath);
            this.Controls.Add(this.btnSelectFolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bulk Editor";
            this.grpTools.ResumeLayout(false);
            this.grpTools.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.TextBox txtFolderPath;
        private System.Windows.Forms.ListBox lstFiles;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox grpTools;
        private System.Windows.Forms.CheckBox chkFixSourceHyperlinks;
        private System.Windows.Forms.CheckBox chkAppendContentID;
        private System.Windows.Forms.CheckBox chkFixInternalHyperlink;
        private System.Windows.Forms.CheckBox chkFixTitles;
        private System.Windows.Forms.Button btnRunTools;
        private System.Windows.Forms.Button btnSelectFile;
    }
}
