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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.txtFolderPath = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnSettings = new System.Windows.Forms.Button();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.tlpContent = new System.Windows.Forms.TableLayoutPanel();
            this.pnlFiles = new System.Windows.Forms.Panel();
            this.btnFixHighlightedDocument = new System.Windows.Forms.Button();
            this.tlpFileButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnAddFile = new System.Windows.Forms.Button();
            this.btnRemoveFile = new System.Windows.Forms.Button();
            this.btnOpenFileLocation = new System.Windows.Forms.Button();
            this.btnClearFileList = new System.Windows.Forms.Button();
            this.lstFiles = new System.Windows.Forms.ListBox();
            this.lblFilesTitle = new System.Windows.Forms.Label();
            this.pnlChangelog = new System.Windows.Forms.Panel();
            this.txtChangelog = new System.Windows.Forms.TextBox();
            this.btnExportSingleChangelog = new System.Windows.Forms.Button();
            this.lblChangelogTitle = new System.Windows.Forms.Label();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.tlpActions = new System.Windows.Forms.TableLayoutPanel();
            this.grpTools = new System.Windows.Forms.GroupBox();
            this.tlpTools = new System.Windows.Forms.TableLayoutPanel();
            this.chkFixSourceHyperlinks = new System.Windows.Forms.CheckBox();
            this.chkAppendContentID = new System.Windows.Forms.CheckBox();
            this.chkFixTitles = new System.Windows.Forms.CheckBox();
            this.chkFixInternalHyperlink = new System.Windows.Forms.CheckBox();
            this.chkFixDoubleSpaces = new System.Windows.Forms.CheckBox();
            this.chkReplaceHyperlink = new System.Windows.Forms.CheckBox();
            this.chkOpenChangelogAfterUpdates = new System.Windows.Forms.CheckBox();
            this.btnConfigureReplaceHyperlink = new System.Windows.Forms.Button();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnOpenChangelogFolder = new System.Windows.Forms.Button();
            this.btnExportChangelog = new System.Windows.Forms.Button();
            this.btnRunTools = new System.Windows.Forms.Button();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();

            this.tlpMain.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.tlpContent.SuspendLayout();
            this.pnlFiles.SuspendLayout();
            this.tlpFileButtons.SuspendLayout();
            this.pnlChangelog.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.tlpActions.SuspendLayout();
            this.grpTools.SuspendLayout();
            this.tlpTools.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.SuspendLayout();

            //
            // tlpMain
            //
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpMain.Controls.Add(this.pnlContent, 0, 1);
            this.tlpMain.Controls.Add(this.pnlActions, 0, 2);
            this.tlpMain.Controls.Add(this.pnlStatus, 0, 3);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(16);
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 210F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tlpMain.Size = new System.Drawing.Size(1280, 760);
            this.tlpMain.TabIndex = 0;

            //
            // pnlHeader
            //
            this.pnlHeader.Controls.Add(this.btnSettings);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.txtFolderPath);
            this.pnlHeader.Controls.Add(this.btnSelectFolder);
            this.pnlHeader.Controls.Add(this.btnSelectFile);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(16, 16);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(20);
            this.pnlHeader.Size = new System.Drawing.Size(1248, 120);
            this.pnlHeader.TabIndex = 0;

            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(20, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(133, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Bulk Editor";

            //
            // btnSelectFile
            //
            this.btnSelectFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnSelectFile.FlatAppearance.BorderSize = 0;
            this.btnSelectFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectFile.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSelectFile.ForeColor = System.Drawing.Color.White;
            this.btnSelectFile.Location = new System.Drawing.Point(20, 55);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(140, 35);
            this.btnSelectFile.TabIndex = 1;
            this.btnSelectFile.Text = "📄 Select File";
            this.btnSelectFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.BtnSelectFile_Click);

            //
            // btnSelectFolder
            //
            this.btnSelectFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnSelectFolder.FlatAppearance.BorderSize = 0;
            this.btnSelectFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectFolder.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSelectFolder.ForeColor = System.Drawing.Color.White;
            this.btnSelectFolder.Location = new System.Drawing.Point(170, 55);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(140, 35);
            this.btnSelectFolder.TabIndex = 2;
            this.btnSelectFolder.Text = "📁 Select Folder";
            this.btnSelectFolder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.BtnSelectFolder_Click);

            //
            // txtFolderPath
            //
            this.txtFolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolderPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFolderPath.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtFolderPath.Location = new System.Drawing.Point(320, 62);
            this.txtFolderPath.Name = "txtFolderPath";
            this.txtFolderPath.ReadOnly = true;
            this.txtFolderPath.Size = new System.Drawing.Size(858, 25);
            this.txtFolderPath.TabIndex = 3;

            //
            // btnSettings
            //
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSettings.BackColor = System.Drawing.Color.Transparent;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent; // <- was 240,240,240
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Location = new System.Drawing.Point(1188, 10);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(25, 25);
            this.btnSettings.TabIndex = 4;
            this.btnSettings.UseVisualStyleBackColor = false; // <- make the button honor our transparent BackColor
            this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);

            //
            // pnlContent
            //
            this.pnlContent.Controls.Add(this.tlpContent);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(16, 144);
            this.pnlContent.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(20);
            this.pnlContent.Size = new System.Drawing.Size(1248, 376);
            this.pnlContent.TabIndex = 1;

            //
            // tlpContent
            //
            this.tlpContent.ColumnCount = 2;
            this.tlpContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpContent.Controls.Add(this.pnlFiles, 0, 0);
            this.tlpContent.Controls.Add(this.pnlChangelog, 1, 0);
            this.tlpContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpContent.Location = new System.Drawing.Point(20, 20);
            this.tlpContent.Name = "tlpContent";
            this.tlpContent.RowCount = 1;
            this.tlpContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpContent.Size = new System.Drawing.Size(1208, 336);
            this.tlpContent.TabIndex = 0;

            //
            // pnlFiles
            //
            this.pnlFiles.Controls.Add(this.btnFixHighlightedDocument);
            this.pnlFiles.Controls.Add(this.tlpFileButtons);
            this.pnlFiles.Controls.Add(this.lstFiles);
            this.pnlFiles.Controls.Add(this.lblFilesTitle);
            this.pnlFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFiles.Location = new System.Drawing.Point(0, 0);
            this.pnlFiles.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.pnlFiles.Name = "pnlFiles";
            this.pnlFiles.Size = new System.Drawing.Size(475, 336);
            this.pnlFiles.TabIndex = 0;

            //
            // lblFilesTitle
            //
            this.lblFilesTitle.AutoSize = true;
            this.lblFilesTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblFilesTitle.Location = new System.Drawing.Point(0, 0);
            this.lblFilesTitle.Name = "lblFilesTitle";
            this.lblFilesTitle.Size = new System.Drawing.Size(46, 21);
            this.lblFilesTitle.TabIndex = 0;
            this.lblFilesTitle.Text = "Files";

            //
            // lstFiles
            //
            this.lstFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lstFiles.FormattingEnabled = true;
            this.lstFiles.ItemHeight = 15;
            this.lstFiles.Location = new System.Drawing.Point(0, 30);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(475, 246);
            this.lstFiles.TabIndex = 1;

            //
            // tlpFileButtons
            //
            this.tlpFileButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpFileButtons.ColumnCount = 2;
            this.tlpFileButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFileButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFileButtons.Controls.Add(this.btnAddFile, 0, 0);
            this.tlpFileButtons.Controls.Add(this.btnRemoveFile, 1, 0);
            this.tlpFileButtons.Controls.Add(this.btnOpenFileLocation, 0, 1);
            this.tlpFileButtons.Controls.Add(this.btnClearFileList, 1, 1);
            this.tlpFileButtons.Location = new System.Drawing.Point(0, 286);
            this.tlpFileButtons.Name = "tlpFileButtons";
            this.tlpFileButtons.RowCount = 2;
            this.tlpFileButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFileButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFileButtons.Size = new System.Drawing.Size(475, 50);
            this.tlpFileButtons.TabIndex = 2;

            //
            // btnAddFile
            //
            this.btnAddFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnAddFile.FlatAppearance.BorderSize = 0;
            this.btnAddFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddFile.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnAddFile.ForeColor = System.Drawing.Color.White;
            this.btnAddFile.Location = new System.Drawing.Point(2, 2);
            this.btnAddFile.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddFile.Name = "btnAddFile";
            this.btnAddFile.Size = new System.Drawing.Size(233, 21);
            this.btnAddFile.TabIndex = 0;
            this.btnAddFile.Text = "➕ Add File to Selection";
            this.btnAddFile.UseVisualStyleBackColor = true;
            this.btnAddFile.Click += new System.EventHandler(this.BtnAddFile_Click);

            //
            // btnRemoveFile
            //
            this.btnRemoveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnRemoveFile.FlatAppearance.BorderSize = 0;
            this.btnRemoveFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveFile.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnRemoveFile.ForeColor = System.Drawing.Color.White;
            this.btnRemoveFile.Location = new System.Drawing.Point(239, 2);
            this.btnRemoveFile.Margin = new System.Windows.Forms.Padding(2);
            this.btnRemoveFile.Name = "btnRemoveFile";
            this.btnRemoveFile.Size = new System.Drawing.Size(234, 21);
            this.btnRemoveFile.TabIndex = 1;
            this.btnRemoveFile.Text = "🗑️ Remove Highlighted Document";
            this.btnRemoveFile.UseVisualStyleBackColor = true;
            this.btnRemoveFile.Click += new System.EventHandler(this.BtnRemoveFile_Click);

            //
            // btnOpenFileLocation
            //
            this.btnOpenFileLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFileLocation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnOpenFileLocation.FlatAppearance.BorderSize = 0;
            this.btnOpenFileLocation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenFileLocation.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnOpenFileLocation.ForeColor = System.Drawing.Color.White;
            this.btnOpenFileLocation.Location = new System.Drawing.Point(2, 27);
            this.btnOpenFileLocation.Margin = new System.Windows.Forms.Padding(2);
            this.btnOpenFileLocation.Name = "btnOpenFileLocation";
            this.btnOpenFileLocation.Size = new System.Drawing.Size(233, 21);
            this.btnOpenFileLocation.TabIndex = 2;
            this.btnOpenFileLocation.Text = "📂 Open Highlighted File Location";
            this.btnOpenFileLocation.UseVisualStyleBackColor = true;
            this.btnOpenFileLocation.Click += new System.EventHandler(this.BtnOpenFileLocation_Click);

            //
            // btnClearFileList
            //
            this.btnClearFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearFileList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnClearFileList.FlatAppearance.BorderSize = 0;
            this.btnClearFileList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearFileList.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnClearFileList.ForeColor = System.Drawing.Color.White;
            this.btnClearFileList.Location = new System.Drawing.Point(239, 27);
            this.btnClearFileList.Margin = new System.Windows.Forms.Padding(2);
            this.btnClearFileList.Name = "btnClearFileList";
            this.btnClearFileList.Size = new System.Drawing.Size(234, 21);
            this.btnClearFileList.TabIndex = 3;
            this.btnClearFileList.Text = "🗂️ Clear Document List";
            this.btnClearFileList.UseVisualStyleBackColor = true;
            this.btnClearFileList.Click += new System.EventHandler(this.BtnClearFileList_Click);

            //
            // btnFixHighlightedDocument
            //
            this.btnFixHighlightedDocument.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFixHighlightedDocument.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
            this.btnFixHighlightedDocument.FlatAppearance.BorderSize = 0;
            this.btnFixHighlightedDocument.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFixHighlightedDocument.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnFixHighlightedDocument.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.btnFixHighlightedDocument.Location = new System.Drawing.Point(295, 0);
            this.btnFixHighlightedDocument.Name = "btnFixHighlightedDocument";
            this.btnFixHighlightedDocument.Size = new System.Drawing.Size(180, 25);
            this.btnFixHighlightedDocument.TabIndex = 3;
            this.btnFixHighlightedDocument.Text = "🔧 Fix Highlighted Document";
            this.btnFixHighlightedDocument.UseVisualStyleBackColor = true;
            this.btnFixHighlightedDocument.Click += new System.EventHandler(this.BtnFixHighlightedDocument_Click);

            //
            // pnlChangelog
            //
            this.pnlChangelog.Controls.Add(this.txtChangelog);
            this.pnlChangelog.Controls.Add(this.btnExportSingleChangelog);
            this.pnlChangelog.Controls.Add(this.lblChangelogTitle);
            this.pnlChangelog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlChangelog.Location = new System.Drawing.Point(491, 0);
            this.pnlChangelog.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.pnlChangelog.Name = "pnlChangelog";
            this.pnlChangelog.Size = new System.Drawing.Size(717, 336);
            this.pnlChangelog.TabIndex = 1;

            //
            // lblChangelogTitle
            //
            this.lblChangelogTitle.AutoSize = true;
            this.lblChangelogTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblChangelogTitle.Location = new System.Drawing.Point(0, 0);
            this.lblChangelogTitle.Name = "lblChangelogTitle";
            this.lblChangelogTitle.Size = new System.Drawing.Size(85, 21);
            this.lblChangelogTitle.TabIndex = 0;
            this.lblChangelogTitle.Text = "Changelog";

            //
            // btnExportSingleChangelog
            //
            this.btnExportSingleChangelog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportSingleChangelog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnExportSingleChangelog.FlatAppearance.BorderSize = 0;
            this.btnExportSingleChangelog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportSingleChangelog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnExportSingleChangelog.ForeColor = System.Drawing.Color.White;
            this.btnExportSingleChangelog.Location = new System.Drawing.Point(500, 0);
            this.btnExportSingleChangelog.Name = "btnExportSingleChangelog";
            this.btnExportSingleChangelog.Size = new System.Drawing.Size(180, 25);
            this.btnExportSingleChangelog.TabIndex = 2;
            this.btnExportSingleChangelog.Text = "📄 Export This Changelog";
            this.btnExportSingleChangelog.UseVisualStyleBackColor = true;
            this.btnExportSingleChangelog.Click += new System.EventHandler(this.BtnExportSingleChangelog_Click);

            //
            // txtChangelog
            //
            this.txtChangelog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChangelog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtChangelog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtChangelog.Location = new System.Drawing.Point(0, 30);
            this.txtChangelog.Multiline = true;
            this.txtChangelog.Name = "txtChangelog";
            this.txtChangelog.ReadOnly = true;
            this.txtChangelog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtChangelog.Size = new System.Drawing.Size(717, 306);
            this.txtChangelog.TabIndex = 1;

            //
            // pnlActions
            //
            this.pnlActions.Controls.Add(this.tlpActions);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActions.Location = new System.Drawing.Point(16, 528);
            this.pnlActions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(20);
            this.pnlActions.Size = new System.Drawing.Size(1248, 210);
            this.pnlActions.TabIndex = 2;

            //
            // tlpActions
            //
            this.tlpActions.ColumnCount = 2;
            this.tlpActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tlpActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpActions.Controls.Add(this.grpTools, 0, 0);
            this.tlpActions.Controls.Add(this.pnlButtons, 1, 0);
            this.tlpActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpActions.Location = new System.Drawing.Point(20, 20);
            this.tlpActions.Name = "tlpActions";
            this.tlpActions.RowCount = 1;
            this.tlpActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpActions.Size = new System.Drawing.Size(1208, 170);
            this.tlpActions.TabIndex = 0;

            //
            // grpTools
            //
            this.grpTools.Controls.Add(this.tlpTools);
            this.grpTools.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpTools.AutoSize = true;
            this.grpTools.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpTools.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpTools.Location = new System.Drawing.Point(0, 0);
            this.grpTools.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.grpTools.Name = "grpTools";
            this.grpTools.Padding = new System.Windows.Forms.Padding(16);
            this.grpTools.Size = new System.Drawing.Size(890, 210);
            this.grpTools.TabIndex = 0;
            this.grpTools.TabStop = false;
            this.grpTools.Text = "Processing Tools";

            //
            // tlpTools
            //
            this.tlpTools.AutoSize = true;
            this.tlpTools.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTools.ColumnCount = 3;
            this.tlpTools.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tlpTools.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tlpTools.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.tlpTools.Controls.Add(this.chkFixSourceHyperlinks, 0, 0);
            this.tlpTools.Controls.Add(this.chkAppendContentID, 0, 1);
            this.tlpTools.Controls.Add(this.chkCheckTitleChanges, 0, 2);
            this.tlpTools.Controls.Add(this.chkFixTitles, 0, 3);
            this.tlpTools.Controls.Add(this.chkFixInternalHyperlink, 1, 0);
            this.tlpTools.Controls.Add(this.chkFixDoubleSpaces, 1, 1);
            this.tlpTools.Controls.Add(this.chkReplaceHyperlink, 2, 0);
            this.tlpTools.Controls.Add(this.btnConfigureReplaceHyperlink, 2, 3);
            this.tlpTools.Controls.Add(this.chkOpenChangelogAfterUpdates, 1, 3);
            this.tlpTools.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpTools.Location = new System.Drawing.Point(16, 38);
            this.tlpTools.Name = "tlpTools";
            this.tlpTools.RowCount = 4;
            this.tlpTools.RowStyles.Clear();
            this.tlpTools.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpTools.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpTools.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpTools.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpTools.Size = new System.Drawing.Size(858, 114);
            this.tlpTools.TabIndex = 0;

            //
            // chkFixSourceHyperlinks
            //
            this.chkFixSourceHyperlinks.AutoSize = true;
            this.chkFixSourceHyperlinks.BackColor = System.Drawing.Color.Transparent;
            this.chkFixSourceHyperlinks.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkFixSourceHyperlinks.Location = new System.Drawing.Point(3, 3);
            this.chkFixSourceHyperlinks.Name = "chkFixSourceHyperlinks";
            this.chkFixSourceHyperlinks.Size = new System.Drawing.Size(138, 19);
            this.chkFixSourceHyperlinks.TabIndex = 0;
            this.chkFixSourceHyperlinks.Text = "Fix Source Hyperlinks";
            this.chkFixSourceHyperlinks.UseVisualStyleBackColor = false;

            //
            // chkAppendContentID
            //
            this.chkAppendContentID.AutoSize = true;
            this.chkAppendContentID.BackColor = System.Drawing.Color.Transparent;
            this.chkAppendContentID.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkAppendContentID.Location = new System.Drawing.Point(23, 31);
            this.chkAppendContentID.Margin = new System.Windows.Forms.Padding(23, 3, 3, 3);
            this.chkAppendContentID.Name = "chkAppendContentID";
            this.chkAppendContentID.Size = new System.Drawing.Size(128, 19);
            this.chkAppendContentID.TabIndex = 1;
            this.chkAppendContentID.Text = "Append Content ID";
            this.chkAppendContentID.UseVisualStyleBackColor = false;

            //
            // chkCheckTitleChanges
            //
            this.chkCheckTitleChanges = new System.Windows.Forms.CheckBox();
            this.chkCheckTitleChanges.AutoSize = true;
            this.chkCheckTitleChanges.BackColor = System.Drawing.Color.Transparent;
            this.chkCheckTitleChanges.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkCheckTitleChanges.Location = new System.Drawing.Point(23, 59);
            this.chkCheckTitleChanges.Margin = new System.Windows.Forms.Padding(23, 3, 3, 3);
            this.chkCheckTitleChanges.Name = "chkCheckTitleChanges";
            this.chkCheckTitleChanges.Size = new System.Drawing.Size(171, 19);
            this.chkCheckTitleChanges.TabIndex = 2;
            this.chkCheckTitleChanges.Text = "Check Possible Title Changes";
            this.chkCheckTitleChanges.UseVisualStyleBackColor = false;

            //
            // chkFixTitles
            //
            this.chkFixTitles = new System.Windows.Forms.CheckBox();
            this.chkFixTitles.AutoSize = true;
            this.chkFixTitles.BackColor = System.Drawing.Color.Transparent;
            this.chkFixTitles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkFixTitles.Location = new System.Drawing.Point(23, 87);
            this.chkFixTitles.Margin = new System.Windows.Forms.Padding(23, 3, 3, 3);
            this.chkFixTitles.Name = "chkFixTitles";
            this.chkFixTitles.Size = new System.Drawing.Size(140, 19);
            this.chkFixTitles.TabIndex = 3;
            this.chkFixTitles.Text = "Update Incorrect Titles";
            this.chkFixTitles.UseVisualStyleBackColor = false;

            //
            // chkFixInternalHyperlink
            //
            this.chkFixInternalHyperlink.AutoSize = true;
            this.chkFixInternalHyperlink.BackColor = System.Drawing.Color.Transparent;
            this.chkFixInternalHyperlink.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkFixInternalHyperlink.Location = new System.Drawing.Point(289, 3);
            this.chkFixInternalHyperlink.Name = "chkFixInternalHyperlink";
            this.chkFixInternalHyperlink.Size = new System.Drawing.Size(138, 19);
            this.chkFixInternalHyperlink.TabIndex = 4;
            this.chkFixInternalHyperlink.Text = "Fix Internal Hyperlink";
            this.chkFixInternalHyperlink.UseVisualStyleBackColor = false;

            //
            // chkFixDoubleSpaces
            //
            this.chkFixDoubleSpaces.AutoSize = true;
            this.chkFixDoubleSpaces.BackColor = System.Drawing.Color.Transparent;
            this.chkFixDoubleSpaces.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkFixDoubleSpaces.Location = new System.Drawing.Point(289, 31);
            this.chkFixDoubleSpaces.Name = "chkFixDoubleSpaces";
            this.chkFixDoubleSpaces.Size = new System.Drawing.Size(122, 19);
            this.chkFixDoubleSpaces.TabIndex = 5;
            this.chkFixDoubleSpaces.Text = "Fix Double Spaces";
            this.chkFixDoubleSpaces.UseVisualStyleBackColor = false;

            //
            // chkReplaceHyperlink
            //
            this.chkReplaceHyperlink.AutoSize = true;
            this.chkReplaceHyperlink.BackColor = System.Drawing.Color.Transparent;
            this.chkReplaceHyperlink.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkReplaceHyperlink.Location = new System.Drawing.Point(575, 3);
            this.chkReplaceHyperlink.Name = "chkReplaceHyperlink";
            this.chkReplaceHyperlink.Size = new System.Drawing.Size(126, 19);
            this.chkReplaceHyperlink.TabIndex = 6;
            this.chkReplaceHyperlink.Text = "Replace Hyperlinks";
            this.chkReplaceHyperlink.UseVisualStyleBackColor = false;

            //
            // chkOpenChangelogAfterUpdates
            //
            this.chkOpenChangelogAfterUpdates.AutoSize = true;
            this.chkOpenChangelogAfterUpdates.BackColor = System.Drawing.Color.Transparent;
            this.chkOpenChangelogAfterUpdates.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkOpenChangelogAfterUpdates.Location = new System.Drawing.Point(289, 87);
            this.chkOpenChangelogAfterUpdates.Name = "chkOpenChangelogAfterUpdates";
            this.chkOpenChangelogAfterUpdates.Size = new System.Drawing.Size(181, 19);
            this.chkOpenChangelogAfterUpdates.TabIndex = 8;
            this.chkOpenChangelogAfterUpdates.Text = "Open Changelog After Updates";
            this.chkOpenChangelogAfterUpdates.UseVisualStyleBackColor = false;

            //
            // btnConfigureReplaceHyperlink
            //
            this.btnConfigureReplaceHyperlink.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
            this.btnConfigureReplaceHyperlink.FlatAppearance.BorderSize = 0;
            this.btnConfigureReplaceHyperlink.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfigureReplaceHyperlink.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnConfigureReplaceHyperlink.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.btnConfigureReplaceHyperlink.Location = new System.Drawing.Point(575, 87);
            this.btnConfigureReplaceHyperlink.Name = "btnConfigureReplaceHyperlink";
            this.btnConfigureReplaceHyperlink.Size = new System.Drawing.Size(195, 30);
            this.btnConfigureReplaceHyperlink.TabIndex = 7;
            this.btnConfigureReplaceHyperlink.Text = "⚙️ Edit Hyperlink Replacements";
            this.btnConfigureReplaceHyperlink.UseVisualStyleBackColor = true;
            this.btnConfigureReplaceHyperlink.Click += new System.EventHandler(this.BtnConfigureReplaceHyperlink_Click);

            //
            // pnlButtons
            //
            this.pnlButtons.Controls.Add(this.btnOpenChangelogFolder);
            this.pnlButtons.Controls.Add(this.btnExportChangelog);
            this.pnlButtons.Controls.Add(this.btnRunTools);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButtons.Location = new System.Drawing.Point(916, 10);
            this.pnlButtons.Margin = new System.Windows.Forms.Padding(10);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(282, 150);
            this.pnlButtons.TabIndex = 1;

            //
            // btnRunTools
            //
            this.btnRunTools.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunTools.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnRunTools.FlatAppearance.BorderSize = 0;
            this.btnRunTools.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunTools.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnRunTools.ForeColor = System.Drawing.Color.White;
            this.btnRunTools.Location = new System.Drawing.Point(0, 0);
            this.btnRunTools.Name = "btnRunTools";
            this.btnRunTools.Size = new System.Drawing.Size(282, 50);
            this.btnRunTools.TabIndex = 0;
            this.btnRunTools.Text = "🚀 Fix All Documents";
            this.btnRunTools.UseVisualStyleBackColor = true;
            this.btnRunTools.Click += new System.EventHandler(this.BtnRunTools_Click);

            //
            // btnExportChangelog
            //
            this.btnExportChangelog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportChangelog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnExportChangelog.FlatAppearance.BorderSize = 0;
            this.btnExportChangelog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportChangelog.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnExportChangelog.ForeColor = System.Drawing.Color.White;
            this.btnExportChangelog.Location = new System.Drawing.Point(0, 55);
            this.btnExportChangelog.Name = "btnExportChangelog";
            this.btnExportChangelog.Size = new System.Drawing.Size(282, 40);
            this.btnExportChangelog.TabIndex = 1;
            this.btnExportChangelog.Text = "📤 Export Batch Changelog";
            this.btnExportChangelog.UseVisualStyleBackColor = true;
            this.btnExportChangelog.Click += new System.EventHandler(this.BtnExportChangelog_Click);

            //
            // btnOpenChangelogFolder
            //
            this.btnOpenChangelogFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenChangelogFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnOpenChangelogFolder.FlatAppearance.BorderSize = 0;
            this.btnOpenChangelogFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenChangelogFolder.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnOpenChangelogFolder.ForeColor = System.Drawing.Color.White;
            this.btnOpenChangelogFolder.Location = new System.Drawing.Point(0, 100);
            this.btnOpenChangelogFolder.Name = "btnOpenChangelogFolder";
            this.btnOpenChangelogFolder.Size = new System.Drawing.Size(282, 40);
            this.btnOpenChangelogFolder.TabIndex = 2;
            this.btnOpenChangelogFolder.Text = "📁 Open Changelog Folder";
            this.btnOpenChangelogFolder.UseVisualStyleBackColor = true;
            this.btnOpenChangelogFolder.Click += new System.EventHandler(this.BtnOpenChangelogFolder_Click);

            //
            // pnlStatus
            //
            this.pnlStatus.Controls.Add(this.lblStatus);
            this.pnlStatus.Controls.Add(this.progressBar);
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStatus.Location = new System.Drawing.Point(16, 698);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.pnlStatus.Size = new System.Drawing.Size(1248, 58);
            this.pnlStatus.TabIndex = 3;

            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblStatus.Location = new System.Drawing.Point(0, 8);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(127, 15);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Ready to process files...";

            //
            // progressBar
            //
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(0, 28);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1248, 6);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 1;
            this.progressBar.Visible = false;

            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1280, 760);
            this.Controls.Add(this.tlpMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bulk Editor - Document Hyperlink Processor";

            this.tlpMain.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlContent.ResumeLayout(false);
            this.tlpContent.ResumeLayout(false);
            this.pnlFiles.ResumeLayout(false);
            this.pnlFiles.PerformLayout();
            this.tlpFileButtons.ResumeLayout(false);
            this.pnlChangelog.ResumeLayout(false);
            this.pnlChangelog.PerformLayout();
            this.pnlActions.ResumeLayout(false);
            this.tlpActions.ResumeLayout(false);
            this.grpTools.ResumeLayout(false);
            this.tlpTools.ResumeLayout(false);
            this.tlpTools.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlStatus.ResumeLayout(false);
            this.pnlStatus.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.TextBox txtFolderPath;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.TableLayoutPanel tlpContent;
        private System.Windows.Forms.Panel pnlFiles;
        private System.Windows.Forms.Label lblFilesTitle;
        private System.Windows.Forms.ListBox lstFiles;
        private System.Windows.Forms.Panel pnlChangelog;
        private System.Windows.Forms.Label lblChangelogTitle;
        private System.Windows.Forms.TextBox txtChangelog;
        private System.Windows.Forms.Button btnExportSingleChangelog;
        private System.Windows.Forms.Panel pnlActions;
        private System.Windows.Forms.TableLayoutPanel tlpActions;
        private System.Windows.Forms.GroupBox grpTools;
        private System.Windows.Forms.TableLayoutPanel tlpTools;
        private System.Windows.Forms.CheckBox chkFixSourceHyperlinks;
        private System.Windows.Forms.CheckBox chkAppendContentID;
        private System.Windows.Forms.CheckBox chkCheckTitleChanges;
        private System.Windows.Forms.CheckBox chkFixTitles;
        private System.Windows.Forms.CheckBox chkFixInternalHyperlink;
        private System.Windows.Forms.CheckBox chkFixDoubleSpaces;
        private System.Windows.Forms.CheckBox chkReplaceHyperlink;
        private System.Windows.Forms.Button btnConfigureReplaceHyperlink;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnRunTools;
        private System.Windows.Forms.Button btnExportChangelog;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.CheckBox chkOpenChangelogAfterUpdates;
        private System.Windows.Forms.TableLayoutPanel tlpFileButtons;
        private System.Windows.Forms.Button btnAddFile;
        private System.Windows.Forms.Button btnRemoveFile;
        private System.Windows.Forms.Button btnOpenFileLocation;
        private System.Windows.Forms.Button btnClearFileList;
        private System.Windows.Forms.Button btnOpenChangelogFolder;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnFixHighlightedDocument;
    }
}
