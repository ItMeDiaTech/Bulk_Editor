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
            this.chkFixDoubleSpaces = new System.Windows.Forms.CheckBox();
            this.btnRunTools = new System.Windows.Forms.Button();
            this.btnExportChangelog = new System.Windows.Forms.Button();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.pnlChangelog = new System.Windows.Forms.Panel();
            this.lblChangelogTitle = new System.Windows.Forms.Label();
            this.txtChangelog = new System.Windows.Forms.TextBox();
            this.grpTools.SuspendLayout();
            this.pnlChangelog.SuspendLayout();
            this.SuspendLayout();
            //
            // btnSelectFolder
            //
            this.btnSelectFolder.BackColor = System.Drawing.SystemColors.Control;
            this.btnSelectFolder.FlatAppearance.BorderSize = 0;
            this.btnSelectFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectFolder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectFolder.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnSelectFolder.Location = new System.Drawing.Point(12, 12);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(120, 35);
            this.btnSelectFolder.TabIndex = 0;
            this.btnSelectFolder.Text = "Select Folder";
            this.btnSelectFolder.UseVisualStyleBackColor = false;
            this.btnSelectFolder.Click += new System.EventHandler(this.BtnSelectFolder_Click);
            //
            // txtFolderPath
            //
            this.txtFolderPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFolderPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFolderPath.Location = new System.Drawing.Point(138, 15);
            this.txtFolderPath.Name = "txtFolderPath";
            this.txtFolderPath.ReadOnly = true;
            this.txtFolderPath.Size = new System.Drawing.Size(540, 27);
            this.txtFolderPath.TabIndex = 1;
            //
            // lstFiles
            //
            this.lstFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstFiles.FormattingEnabled = true;
            this.lstFiles.ItemHeight = 16;
            this.lstFiles.Location = new System.Drawing.Point(12, 105);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(450, 280);
            this.lstFiles.TabIndex = 2;
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblStatus.Location = new System.Drawing.Point(12, 410);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 3;
            //
            // grpTools
            //
            this.grpTools.Controls.Add(this.chkFixDoubleSpaces);
            this.grpTools.Controls.Add(this.chkFixTitles);
            this.grpTools.Controls.Add(this.chkFixInternalHyperlink);
            this.grpTools.Controls.Add(this.chkAppendContentID);
            this.grpTools.Controls.Add(this.chkFixSourceHyperlinks);
            this.grpTools.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpTools.Location = new System.Drawing.Point(12, 391);
            this.grpTools.Name = "grpTools";
            this.grpTools.Size = new System.Drawing.Size(1030, 145);
            this.grpTools.TabIndex = 4;
            this.grpTools.TabStop = false;
            this.grpTools.Text = "Tools";
            //
            // chkFixSourceHyperlinks
            //
            this.chkFixSourceHyperlinks.AutoSize = true;
            this.chkFixSourceHyperlinks.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkFixSourceHyperlinks.Location = new System.Drawing.Point(10, 25);
            this.chkFixSourceHyperlinks.Name = "chkFixSourceHyperlinks";
            this.chkFixSourceHyperlinks.Size = new System.Drawing.Size(150, 19);
            this.chkFixSourceHyperlinks.TabIndex = 0;
            this.chkFixSourceHyperlinks.Text = "Fix Source Hyperlinks";
            this.chkFixSourceHyperlinks.UseVisualStyleBackColor = true;
            //
            // chkAppendContentID
            //
            this.chkAppendContentID.AutoSize = true;
            this.chkAppendContentID.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkAppendContentID.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.chkAppendContentID.Location = new System.Drawing.Point(30, 55);
            this.chkAppendContentID.Name = "chkAppendContentID";
            this.chkAppendContentID.Size = new System.Drawing.Size(130, 19);
            this.chkAppendContentID.TabIndex = 1;
            this.chkAppendContentID.Text = "  Append Content ID";
            this.chkAppendContentID.UseVisualStyleBackColor = true;
            //
            // chkFixInternalHyperlink
            //
            this.chkFixInternalHyperlink.AutoSize = true;
            this.chkFixInternalHyperlink.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkFixInternalHyperlink.Location = new System.Drawing.Point(270, 25);
            this.chkFixInternalHyperlink.Name = "chkFixInternalHyperlink";
            this.chkFixInternalHyperlink.Size = new System.Drawing.Size(150, 19);
            this.chkFixInternalHyperlink.TabIndex = 2;
            this.chkFixInternalHyperlink.Text = "Fix Internal Hyperlink";
            this.chkFixInternalHyperlink.UseVisualStyleBackColor = true;
            //
            // chkFixTitles
            //
            this.chkFixTitles.AutoSize = true;
            this.chkFixTitles.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkFixTitles.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.chkFixTitles.Location = new System.Drawing.Point(30, 80);
            this.chkFixTitles.Name = "chkFixTitles";
            this.chkFixTitles.Size = new System.Drawing.Size(90, 19);
            this.chkFixTitles.TabIndex = 3;
            this.chkFixTitles.Text = "  Fix Titles";
            this.chkFixTitles.UseVisualStyleBackColor = true;
            //
            // chkFixDoubleSpaces
            //
            this.chkFixDoubleSpaces.AutoSize = true;
            this.chkFixDoubleSpaces.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkFixDoubleSpaces.Location = new System.Drawing.Point(270, 55);
            this.chkFixDoubleSpaces.Name = "chkFixDoubleSpaces";
            this.chkFixDoubleSpaces.Size = new System.Drawing.Size(150, 19);
            this.chkFixDoubleSpaces.TabIndex = 4;
            this.chkFixDoubleSpaces.Text = "Fix Double Spaces";
            this.chkFixDoubleSpaces.UseVisualStyleBackColor = true;
            //
            // btnRunTools
            //
            this.btnRunTools.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnRunTools.FlatAppearance.BorderSize = 0;
            this.btnRunTools.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunTools.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRunTools.ForeColor = System.Drawing.Color.White;
            this.btnRunTools.Location = new System.Drawing.Point(922, 421);
            this.btnRunTools.Name = "btnRunTools";
            this.btnRunTools.Size = new System.Drawing.Size(120, 35);
            this.btnRunTools.TabIndex = 5;
            this.btnRunTools.Text = "Run Tools";
            this.btnRunTools.UseVisualStyleBackColor = false;
            this.btnRunTools.Click += new System.EventHandler(this.BtnRunTools_Click);
            //
            // btnExportChangelog
            //
            this.btnExportChangelog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnExportChangelog.FlatAppearance.BorderSize = 0;
            this.btnExportChangelog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportChangelog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportChangelog.ForeColor = System.Drawing.Color.White;
            this.btnExportChangelog.Location = new System.Drawing.Point(922, 462);
            this.btnExportChangelog.Name = "btnExportChangelog";
            this.btnExportChangelog.Size = new System.Drawing.Size(120, 35);
            this.btnExportChangelog.TabIndex = 8;
            this.btnExportChangelog.Text = "Export";
            this.btnExportChangelog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnExportChangelog.UseVisualStyleBackColor = false;
            this.btnExportChangelog.Click += new System.EventHandler(this.BtnExportChangelog_Click);
            //
            // btnSelectFile
            //
            this.btnSelectFile.BackColor = System.Drawing.SystemColors.Control;
            this.btnSelectFile.FlatAppearance.BorderSize = 0;
            this.btnSelectFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectFile.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnSelectFile.Location = new System.Drawing.Point(12, 48);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(120, 35);
            this.btnSelectFile.TabIndex = 6;
            this.btnSelectFile.Text = "Select File";
            this.btnSelectFile.UseVisualStyleBackColor = false;
            this.btnSelectFile.Click += new System.EventHandler(this.BtnSelectFile_Click);
            //
            // pnlChangelog
            //
            this.pnlChangelog.Controls.Add(this.txtChangelog);
            this.pnlChangelog.Controls.Add(this.lblChangelogTitle);
            this.pnlChangelog.Location = new System.Drawing.Point(468, 105);
            this.pnlChangelog.Name = "pnlChangelog";
            this.pnlChangelog.Size = new System.Drawing.Size(574, 280);
            this.pnlChangelog.TabIndex = 7;
            this.pnlChangelog.Visible = true;
            //
            // lblChangelogTitle
            //
            this.lblChangelogTitle.AutoSize = true;
            this.lblChangelogTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChangelogTitle.Location = new System.Drawing.Point(10, 10);
            this.lblChangelogTitle.Name = "lblChangelogTitle";
            this.lblChangelogTitle.Size = new System.Drawing.Size(150, 19);
            this.lblChangelogTitle.TabIndex = 0;
            this.lblChangelogTitle.Text = "Changelog";
            //
            // txtChangelog
            //
            this.txtChangelog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtChangelog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtChangelog.Location = new System.Drawing.Point(10, 35);
            this.txtChangelog.Multiline = true;
            this.txtChangelog.Name = "txtChangelog";
            this.txtChangelog.ReadOnly = true;
            this.txtChangelog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtChangelog.Size = new System.Drawing.Size(554, 235);
            this.txtChangelog.TabIndex = 1;
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1060, 540);
            this.Controls.Add(this.pnlChangelog);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.btnExportChangelog);
            this.Controls.Add(this.btnRunTools);
            this.Controls.Add(this.grpTools);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lstFiles);
            this.Controls.Add(this.txtFolderPath);
            this.Controls.Add(this.btnSelectFolder);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bulk Editor";
            this.grpTools.ResumeLayout(false);
            this.grpTools.PerformLayout();
            this.pnlChangelog.ResumeLayout(false);
            this.pnlChangelog.PerformLayout();
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
        private System.Windows.Forms.Button btnExportChangelog;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Panel pnlChangelog;
        private System.Windows.Forms.Label lblChangelogTitle;
        private System.Windows.Forms.TextBox txtChangelog;
        private System.Windows.Forms.CheckBox chkFixDoubleSpaces;
    }
}
