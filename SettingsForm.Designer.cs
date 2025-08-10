namespace Bulk_Editor
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabChangelog;
        private System.Windows.Forms.TabPage tabApplication;
        private System.Windows.Forms.TabPage tabProcessing;
        private System.Windows.Forms.TabPage tabInterface;

        private System.Windows.Forms.GroupBox grpChangelogSettings;
        private System.Windows.Forms.Label lblBaseStoragePath;
        private System.Windows.Forms.TextBox txtBaseStoragePath;
        private System.Windows.Forms.Button btnBrowsePath;
        private System.Windows.Forms.CheckBox chkUseCentralizedStorage;
        private System.Windows.Forms.CheckBox chkOrganizeByDate;
        private System.Windows.Forms.CheckBox chkSeparateIndividualAndCombined;
        private System.Windows.Forms.CheckBox chkCentralizeBackups;
        private System.Windows.Forms.Label lblAutoCleanupDays;
        private System.Windows.Forms.NumericUpDown numAutoCleanupDays;
        private System.Windows.Forms.Label lblAutoCleanupNote;

        private System.Windows.Forms.GroupBox grpApplicationSettings;
        private System.Windows.Forms.CheckBox chkRememberWindowPosition;
        private System.Windows.Forms.CheckBox chkConfirmBeforeProcessing;
        private System.Windows.Forms.CheckBox chkRemoveDocumentFilesOnExit;
        private System.Windows.Forms.Label lblMaxBatchSize;
        private System.Windows.Forms.NumericUpDown numMaxBatchSize;
        private System.Windows.Forms.Label lblRecentFiles;
        private System.Windows.Forms.NumericUpDown numRecentFiles;

        private System.Windows.Forms.GroupBox grpProcessingSettings;
        private System.Windows.Forms.CheckBox chkCreateBackups;
        private System.Windows.Forms.CheckBox chkValidateDocuments;
        private System.Windows.Forms.CheckBox chkSkipCorrupted;
        private System.Windows.Forms.CheckBox chkPreserveAttributes;
        private System.Windows.Forms.Label lblMaxFileSize;
        private System.Windows.Forms.NumericUpDown numMaxFileSize;
        private System.Windows.Forms.Label lblConcurrentFiles;
        private System.Windows.Forms.NumericUpDown numConcurrentFiles;
        private System.Windows.Forms.Label lblProcessingTimeout;
        private System.Windows.Forms.NumericUpDown numProcessingTimeout;

        private System.Windows.Forms.GroupBox grpInterfaceSettings;
        private System.Windows.Forms.CheckBox chkRememberWindowSize;
        private System.Windows.Forms.CheckBox chkShowProgressDetails;
        private System.Windows.Forms.CheckBox chkAutoSelectFirstFile;
        private System.Windows.Forms.CheckBox chkShowToolTips;
        private System.Windows.Forms.CheckBox chkConfirmOnExit;
        private System.Windows.Forms.CheckBox chkShowStatusBar;
        private System.Windows.Forms.Label lblTheme;
        private System.Windows.Forms.ComboBox cmbTheme;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.ComboBox cmbLanguage;


        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnResetToDefaults;
        private System.Windows.Forms.Button btnOpenCurrentFolder;
        private System.Windows.Forms.Button btnImportSettings;
        private System.Windows.Forms.Button btnExportSettings;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDescription;

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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabChangelog = new System.Windows.Forms.TabPage();
            this.grpChangelogSettings = new System.Windows.Forms.GroupBox();
            this.lblBaseStoragePath = new System.Windows.Forms.Label();
            this.txtBaseStoragePath = new System.Windows.Forms.TextBox();
            this.btnBrowsePath = new System.Windows.Forms.Button();
            this.chkUseCentralizedStorage = new System.Windows.Forms.CheckBox();
            this.chkOrganizeByDate = new System.Windows.Forms.CheckBox();
            this.chkSeparateIndividualAndCombined = new System.Windows.Forms.CheckBox();
            this.chkCentralizeBackups = new System.Windows.Forms.CheckBox();
            this.lblAutoCleanupDays = new System.Windows.Forms.Label();
            this.numAutoCleanupDays = new System.Windows.Forms.NumericUpDown();
            this.lblAutoCleanupNote = new System.Windows.Forms.Label();
            this.tabApplication = new System.Windows.Forms.TabPage();
            this.grpApplicationSettings = new System.Windows.Forms.GroupBox();
            this.chkRememberWindowPosition = new System.Windows.Forms.CheckBox();
            this.chkConfirmBeforeProcessing = new System.Windows.Forms.CheckBox();
            this.chkRemoveDocumentFilesOnExit = new System.Windows.Forms.CheckBox();
            this.lblMaxBatchSize = new System.Windows.Forms.Label();
            this.numMaxBatchSize = new System.Windows.Forms.NumericUpDown();
            this.lblRecentFiles = new System.Windows.Forms.Label();
            this.numRecentFiles = new System.Windows.Forms.NumericUpDown();
            this.tabProcessing = new System.Windows.Forms.TabPage();
            this.grpProcessingSettings = new System.Windows.Forms.GroupBox();
            this.chkCreateBackups = new System.Windows.Forms.CheckBox();
            this.chkValidateDocuments = new System.Windows.Forms.CheckBox();
            this.chkSkipCorrupted = new System.Windows.Forms.CheckBox();
            this.chkPreserveAttributes = new System.Windows.Forms.CheckBox();
            this.lblMaxFileSize = new System.Windows.Forms.Label();
            this.numMaxFileSize = new System.Windows.Forms.NumericUpDown();
            this.lblConcurrentFiles = new System.Windows.Forms.Label();
            this.numConcurrentFiles = new System.Windows.Forms.NumericUpDown();
            this.lblProcessingTimeout = new System.Windows.Forms.Label();
            this.numProcessingTimeout = new System.Windows.Forms.NumericUpDown();
            this.tabInterface = new System.Windows.Forms.TabPage();
            this.grpInterfaceSettings = new System.Windows.Forms.GroupBox();
            this.chkRememberWindowSize = new System.Windows.Forms.CheckBox();
            this.chkShowProgressDetails = new System.Windows.Forms.CheckBox();
            this.chkAutoSelectFirstFile = new System.Windows.Forms.CheckBox();
            this.chkShowToolTips = new System.Windows.Forms.CheckBox();
            this.chkConfirmOnExit = new System.Windows.Forms.CheckBox();
            this.chkShowStatusBar = new System.Windows.Forms.CheckBox();
            this.lblTheme = new System.Windows.Forms.Label();
            this.cmbTheme = new System.Windows.Forms.ComboBox();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnResetToDefaults = new System.Windows.Forms.Button();
            this.btnOpenCurrentFolder = new System.Windows.Forms.Button();
            this.btnImportSettings = new System.Windows.Forms.Button();
            this.btnExportSettings = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();

            this.tabControl.SuspendLayout();
            this.tabChangelog.SuspendLayout();
            this.grpChangelogSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoCleanupDays)).BeginInit();
            this.tabApplication.SuspendLayout();
            this.grpApplicationSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxBatchSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRecentFiles)).BeginInit();
            this.tabProcessing.SuspendLayout();
            this.grpProcessingSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxFileSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numConcurrentFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numProcessingTimeout)).BeginInit();
            this.tabInterface.SuspendLayout();
            this.grpInterfaceSettings.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            //
            // tabControl
            //
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabChangelog);
            this.tabControl.Controls.Add(this.tabApplication);
            this.tabControl.Controls.Add(this.tabProcessing);
            this.tabControl.Controls.Add(this.tabInterface);
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tabControl.Location = new System.Drawing.Point(20, 80);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(760, 420);
            this.tabControl.TabIndex = 0;

            //
            // tabChangelog
            //
            this.tabChangelog.Controls.Add(this.grpChangelogSettings);
            this.tabChangelog.Location = new System.Drawing.Point(4, 24);
            this.tabChangelog.Name = "tabChangelog";
            this.tabChangelog.Padding = new System.Windows.Forms.Padding(15);
            this.tabChangelog.Size = new System.Drawing.Size(752, 392);
            this.tabChangelog.TabIndex = 0;
            this.tabChangelog.Text = "📋 Changelog";
            this.tabChangelog.UseVisualStyleBackColor = true;

            //
            // grpChangelogSettings
            //
            this.grpChangelogSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpChangelogSettings.BackColor = System.Drawing.Color.Transparent;
            this.grpChangelogSettings.Controls.Add(this.lblAutoCleanupNote);
            this.grpChangelogSettings.Controls.Add(this.numAutoCleanupDays);
            this.grpChangelogSettings.Controls.Add(this.lblAutoCleanupDays);
            this.grpChangelogSettings.Controls.Add(this.chkCentralizeBackups);
            this.grpChangelogSettings.Controls.Add(this.chkSeparateIndividualAndCombined);
            this.grpChangelogSettings.Controls.Add(this.chkOrganizeByDate);
            this.grpChangelogSettings.Controls.Add(this.chkUseCentralizedStorage);
            this.grpChangelogSettings.Controls.Add(this.btnBrowsePath);
            this.grpChangelogSettings.Controls.Add(this.txtBaseStoragePath);
            this.grpChangelogSettings.Controls.Add(this.lblBaseStoragePath);
            this.grpChangelogSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpChangelogSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(80)))), ((int)(((byte)(87)))));
            this.grpChangelogSettings.Location = new System.Drawing.Point(15, 15);
            this.grpChangelogSettings.Name = "grpChangelogSettings";
            this.grpChangelogSettings.Padding = new System.Windows.Forms.Padding(20);
            this.grpChangelogSettings.Size = new System.Drawing.Size(722, 362);
            this.grpChangelogSettings.TabIndex = 0;
            this.grpChangelogSettings.TabStop = false;
            this.grpChangelogSettings.Text = "Changelog Storage Settings";

            //
            // lblBaseStoragePath
            //
            this.lblBaseStoragePath.AutoSize = true;
            this.lblBaseStoragePath.BackColor = System.Drawing.Color.Transparent;
            this.lblBaseStoragePath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblBaseStoragePath.Location = new System.Drawing.Point(20, 70);
            this.lblBaseStoragePath.Name = "lblBaseStoragePath";
            this.lblBaseStoragePath.Size = new System.Drawing.Size(105, 15);
            this.lblBaseStoragePath.TabIndex = 1;
            this.lblBaseStoragePath.Text = "Base Storage Path:";

            //
            // txtBaseStoragePath
            //
            this.txtBaseStoragePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBaseStoragePath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtBaseStoragePath.Location = new System.Drawing.Point(20, 90);
            this.txtBaseStoragePath.Name = "txtBaseStoragePath";
            this.txtBaseStoragePath.Size = new System.Drawing.Size(420, 23);
            this.txtBaseStoragePath.TabIndex = 2;
            //
            // btnBrowsePath
            //
            this.btnBrowsePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowsePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnBrowsePath.FlatAppearance.BorderSize = 0;
            this.btnBrowsePath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowsePath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnBrowsePath.ForeColor = System.Drawing.Color.White;
            this.btnBrowsePath.Location = new System.Drawing.Point(450, 90);
            this.btnBrowsePath.Name = "btnBrowsePath";
            this.btnBrowsePath.Size = new System.Drawing.Size(80, 23);
            this.btnBrowsePath.TabIndex = 3;
            this.btnBrowsePath.Text = "Browse...";
            this.btnBrowsePath.UseVisualStyleBackColor = true;
            this.btnBrowsePath.Click += new System.EventHandler(this.BtnBrowsePath_Click);

            //
            // chkUseCentralizedStorage
            //
            this.chkUseCentralizedStorage.AutoSize = true;
            this.chkUseCentralizedStorage.BackColor = System.Drawing.Color.White;
            this.chkUseCentralizedStorage.FlatAppearance.BorderSize = 0;
            this.chkUseCentralizedStorage.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkUseCentralizedStorage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkUseCentralizedStorage.Location = new System.Drawing.Point(20, 40);
            this.chkUseCentralizedStorage.Name = "chkUseCentralizedStorage";
            this.chkUseCentralizedStorage.Size = new System.Drawing.Size(273, 19);
            this.chkUseCentralizedStorage.TabIndex = 0;
            this.chkUseCentralizedStorage.Text = "Use centralized storage (instead of document folders)";
            this.chkUseCentralizedStorage.UseVisualStyleBackColor = true;

            //
            // chkOrganizeByDate
            //
            this.chkOrganizeByDate.AutoSize = true;
            this.chkOrganizeByDate.BackColor = System.Drawing.Color.White;
            this.chkOrganizeByDate.FlatAppearance.BorderSize = 0;
            this.chkOrganizeByDate.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkOrganizeByDate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkOrganizeByDate.Location = new System.Drawing.Point(40, 130);
            this.chkOrganizeByDate.Name = "chkOrganizeByDate";
            this.chkOrganizeByDate.Size = new System.Drawing.Size(252, 19);
            this.chkOrganizeByDate.TabIndex = 4;
            this.chkOrganizeByDate.Text = "Organize changelogs by date (MM-dd-yyyy)";
            this.chkOrganizeByDate.UseVisualStyleBackColor = true;

            //
            // chkSeparateIndividualAndCombined
            //
            this.chkSeparateIndividualAndCombined.AutoSize = true;
            this.chkSeparateIndividualAndCombined.BackColor = System.Drawing.Color.White;
            this.chkSeparateIndividualAndCombined.FlatAppearance.BorderSize = 0;
            this.chkSeparateIndividualAndCombined.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkSeparateIndividualAndCombined.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkSeparateIndividualAndCombined.Location = new System.Drawing.Point(40, 155);
            this.chkSeparateIndividualAndCombined.Name = "chkSeparateIndividualAndCombined";
            this.chkSeparateIndividualAndCombined.Size = new System.Drawing.Size(301, 19);
            this.chkSeparateIndividualAndCombined.TabIndex = 5;
            this.chkSeparateIndividualAndCombined.Text = "Separate folders for individual and combined changelogs";
            this.chkSeparateIndividualAndCombined.UseVisualStyleBackColor = true;

            //
            // chkCentralizeBackups
            //
            this.chkCentralizeBackups.AutoSize = true;
            this.chkCentralizeBackups.BackColor = System.Drawing.Color.White;
            this.chkCentralizeBackups.FlatAppearance.BorderSize = 0;
            this.chkCentralizeBackups.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkCentralizeBackups.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkCentralizeBackups.Location = new System.Drawing.Point(40, 180);
            this.chkCentralizeBackups.Name = "chkCentralizeBackups";
            this.chkCentralizeBackups.Size = new System.Drawing.Size(294, 19);
            this.chkCentralizeBackups.TabIndex = 6;
            this.chkCentralizeBackups.Text = "Store backups of files in the centralized location";
            this.chkCentralizeBackups.UseVisualStyleBackColor = true;

            //
            // lblAutoCleanupDays
            //
            this.lblAutoCleanupDays.AutoSize = true;
            this.lblAutoCleanupDays.BackColor = System.Drawing.Color.Transparent;
            this.lblAutoCleanupDays.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblAutoCleanupDays.Location = new System.Drawing.Point(40, 220);
            this.lblAutoCleanupDays.Name = "lblAutoCleanupDays";
            this.lblAutoCleanupDays.Size = new System.Drawing.Size(181, 15);
            this.lblAutoCleanupDays.TabIndex = 7;
            this.lblAutoCleanupDays.Text = "Auto-cleanup changelogs after:";

            //
            // numAutoCleanupDays
            //
            this.numAutoCleanupDays.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.numAutoCleanupDays.Location = new System.Drawing.Point(230, 218);
            this.numAutoCleanupDays.Maximum = new decimal(new int[] { 365, 0, 0, 0 });
            this.numAutoCleanupDays.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.numAutoCleanupDays.Name = "numAutoCleanupDays";
            this.numAutoCleanupDays.Size = new System.Drawing.Size(60, 23);
            this.numAutoCleanupDays.TabIndex = 8;

            //
            // lblAutoCleanupNote
            //
            this.lblAutoCleanupNote.AutoSize = true;
            this.lblAutoCleanupNote.BackColor = System.Drawing.Color.Transparent;
            this.lblAutoCleanupNote.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblAutoCleanupNote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblAutoCleanupNote.Location = new System.Drawing.Point(296, 222);
            this.lblAutoCleanupNote.Name = "lblAutoCleanupNote";
            this.lblAutoCleanupNote.Size = new System.Drawing.Size(91, 13);
            this.lblAutoCleanupNote.TabIndex = 9;
            this.lblAutoCleanupNote.Text = "days (0 = disabled)";

            //
            // tabApplication
            //
            this.tabApplication.Controls.Add(this.grpApplicationSettings);
            this.tabApplication.Location = new System.Drawing.Point(4, 24);
            this.tabApplication.Name = "tabApplication";
            this.tabApplication.Padding = new System.Windows.Forms.Padding(15);
            this.tabApplication.Size = new System.Drawing.Size(752, 392);
            this.tabApplication.TabIndex = 1;
            this.tabApplication.Text = "⚙️ Application";
            this.tabApplication.UseVisualStyleBackColor = true;

            //
            // grpApplicationSettings
            //
            this.grpApplicationSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpApplicationSettings.BackColor = System.Drawing.Color.Transparent;
            this.grpApplicationSettings.Controls.Add(this.numRecentFiles);
            this.grpApplicationSettings.Controls.Add(this.lblRecentFiles);
            this.grpApplicationSettings.Controls.Add(this.numMaxBatchSize);
            this.grpApplicationSettings.Controls.Add(this.lblMaxBatchSize);
            this.grpApplicationSettings.Controls.Add(this.chkRemoveDocumentFilesOnExit);
            this.grpApplicationSettings.Controls.Add(this.chkConfirmBeforeProcessing);
            this.grpApplicationSettings.Controls.Add(this.chkRememberWindowPosition);
            this.grpApplicationSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpApplicationSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(80)))), ((int)(((byte)(87)))));
            this.grpApplicationSettings.Location = new System.Drawing.Point(15, 15);
            this.grpApplicationSettings.Name = "grpApplicationSettings";
            this.grpApplicationSettings.Padding = new System.Windows.Forms.Padding(20);
            this.grpApplicationSettings.Size = new System.Drawing.Size(722, 362);
            this.grpApplicationSettings.TabIndex = 0;
            this.grpApplicationSettings.TabStop = false;
            this.grpApplicationSettings.Text = "Application Behavior";

            //
            // chkRememberWindowPosition
            //
            this.chkRememberWindowPosition.AutoSize = true;
            this.chkRememberWindowPosition.BackColor = System.Drawing.Color.White;
            this.chkRememberWindowPosition.FlatAppearance.BorderSize = 0;
            this.chkRememberWindowPosition.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkRememberWindowPosition.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkRememberWindowPosition.Location = new System.Drawing.Point(20, 40);
            this.chkRememberWindowPosition.Name = "chkRememberWindowPosition";
            this.chkRememberWindowPosition.Size = new System.Drawing.Size(174, 19);
            this.chkRememberWindowPosition.TabIndex = 0;
            this.chkRememberWindowPosition.Text = "Remember window position";
            this.chkRememberWindowPosition.UseVisualStyleBackColor = true;

            //
            // chkConfirmBeforeProcessing
            //
            this.chkConfirmBeforeProcessing.AutoSize = true;
            this.chkConfirmBeforeProcessing.BackColor = System.Drawing.Color.White;
            this.chkConfirmBeforeProcessing.FlatAppearance.BorderSize = 0;
            this.chkConfirmBeforeProcessing.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkConfirmBeforeProcessing.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkConfirmBeforeProcessing.Location = new System.Drawing.Point(20, 65);
            this.chkConfirmBeforeProcessing.Name = "chkConfirmBeforeProcessing";
            this.chkConfirmBeforeProcessing.Size = new System.Drawing.Size(176, 19);
            this.chkConfirmBeforeProcessing.TabIndex = 1;
            this.chkConfirmBeforeProcessing.Text = "Confirm before processing";
            this.chkConfirmBeforeProcessing.UseVisualStyleBackColor = true;

            //
            // chkRemoveDocumentFilesOnExit
            //
            this.chkRemoveDocumentFilesOnExit.AutoSize = true;
            this.chkRemoveDocumentFilesOnExit.BackColor = System.Drawing.Color.White;
            this.chkRemoveDocumentFilesOnExit.FlatAppearance.BorderSize = 0;
            this.chkRemoveDocumentFilesOnExit.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkRemoveDocumentFilesOnExit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkRemoveDocumentFilesOnExit.Location = new System.Drawing.Point(20, 90);
            this.chkRemoveDocumentFilesOnExit.Name = "chkRemoveDocumentFilesOnExit";
            this.chkRemoveDocumentFilesOnExit.Size = new System.Drawing.Size(186, 19);
            this.chkRemoveDocumentFilesOnExit.TabIndex = 2;
            this.chkRemoveDocumentFilesOnExit.Text = "Remove document files on exit";
            this.chkRemoveDocumentFilesOnExit.UseVisualStyleBackColor = true;

            //
            // lblMaxBatchSize
            //
            this.lblMaxBatchSize.AutoSize = true;
            this.lblMaxBatchSize.BackColor = System.Drawing.Color.Transparent;
            this.lblMaxBatchSize.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblMaxBatchSize.Location = new System.Drawing.Point(20, 130);
            this.lblMaxBatchSize.Name = "lblMaxBatchSize";
            this.lblMaxBatchSize.Size = new System.Drawing.Size(132, 15);
            this.lblMaxBatchSize.TabIndex = 5;
            this.lblMaxBatchSize.Text = "Maximum batch size:";

            //
            // numMaxBatchSize
            //
            this.numMaxBatchSize.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.numMaxBatchSize.Location = new System.Drawing.Point(160, 128);
            this.numMaxBatchSize.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numMaxBatchSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMaxBatchSize.Name = "numMaxBatchSize";
            this.numMaxBatchSize.Size = new System.Drawing.Size(80, 23);
            this.numMaxBatchSize.TabIndex = 6;
            this.numMaxBatchSize.Value = new decimal(new int[] { 50, 0, 0, 0 });

            //
            // lblRecentFiles
            //
            this.lblRecentFiles.AutoSize = true;
            this.lblRecentFiles.BackColor = System.Drawing.Color.Transparent;
            this.lblRecentFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblRecentFiles.Location = new System.Drawing.Point(20, 160);
            this.lblRecentFiles.Name = "lblRecentFiles";
            this.lblRecentFiles.Size = new System.Drawing.Size(118, 15);
            this.lblRecentFiles.TabIndex = 7;
            this.lblRecentFiles.Text = "Recent files to keep:";

            //
            // numRecentFiles
            //
            this.numRecentFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.numRecentFiles.Location = new System.Drawing.Point(160, 158);
            this.numRecentFiles.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            this.numRecentFiles.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.numRecentFiles.Name = "numRecentFiles";
            this.numRecentFiles.Size = new System.Drawing.Size(80, 23);
            this.numRecentFiles.TabIndex = 8;
            this.numRecentFiles.Value = new decimal(new int[] { 10, 0, 0, 0 });

            //
            // tabProcessing
            //
            this.tabProcessing.Controls.Add(this.grpProcessingSettings);
            this.tabProcessing.Location = new System.Drawing.Point(4, 24);
            this.tabProcessing.Name = "tabProcessing";
            this.tabProcessing.Padding = new System.Windows.Forms.Padding(15);
            this.tabProcessing.Size = new System.Drawing.Size(752, 392);
            this.tabProcessing.TabIndex = 2;
            this.tabProcessing.Text = "🔧 Processing";
            this.tabProcessing.UseVisualStyleBackColor = true;

            //
            // grpProcessingSettings
            //
            this.grpProcessingSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpProcessingSettings.BackColor = System.Drawing.Color.Transparent;
            this.grpProcessingSettings.Controls.Add(this.numProcessingTimeout);
            this.grpProcessingSettings.Controls.Add(this.lblProcessingTimeout);
            this.grpProcessingSettings.Controls.Add(this.numConcurrentFiles);
            this.grpProcessingSettings.Controls.Add(this.lblConcurrentFiles);
            this.grpProcessingSettings.Controls.Add(this.numMaxFileSize);
            this.grpProcessingSettings.Controls.Add(this.lblMaxFileSize);
            this.grpProcessingSettings.Controls.Add(this.chkPreserveAttributes);
            this.grpProcessingSettings.Controls.Add(this.chkSkipCorrupted);
            this.grpProcessingSettings.Controls.Add(this.chkValidateDocuments);
            this.grpProcessingSettings.Controls.Add(this.chkCreateBackups);
            this.grpProcessingSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpProcessingSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(80)))), ((int)(((byte)(87)))));
            this.grpProcessingSettings.Location = new System.Drawing.Point(15, 15);
            this.grpProcessingSettings.Name = "grpProcessingSettings";
            this.grpProcessingSettings.Padding = new System.Windows.Forms.Padding(20);
            this.grpProcessingSettings.Size = new System.Drawing.Size(722, 362);
            this.grpProcessingSettings.TabIndex = 0;
            this.grpProcessingSettings.TabStop = false;
            this.grpProcessingSettings.Text = "Document Processing";

            //
            // chkCreateBackups
            //
            this.chkCreateBackups.AutoSize = true;
            this.chkCreateBackups.BackColor = System.Drawing.Color.White;
            this.chkCreateBackups.FlatAppearance.BorderSize = 0;
            this.chkCreateBackups.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkCreateBackups.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkCreateBackups.Location = new System.Drawing.Point(20, 40);
            this.chkCreateBackups.Name = "chkCreateBackups";
            this.chkCreateBackups.Size = new System.Drawing.Size(194, 19);
            this.chkCreateBackups.TabIndex = 0;
            this.chkCreateBackups.Text = "Create backups before processing";
            this.chkCreateBackups.UseVisualStyleBackColor = true;

            //
            // chkValidateDocuments
            //
            this.chkValidateDocuments.AutoSize = true;
            this.chkValidateDocuments.BackColor = System.Drawing.Color.White;
            this.chkValidateDocuments.FlatAppearance.BorderSize = 0;
            this.chkValidateDocuments.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkValidateDocuments.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkValidateDocuments.Location = new System.Drawing.Point(20, 65);
            this.chkValidateDocuments.Name = "chkValidateDocuments";
            this.chkValidateDocuments.Size = new System.Drawing.Size(176, 19);
            this.chkValidateDocuments.TabIndex = 1;
            this.chkValidateDocuments.Text = "Validate documents before processing";
            this.chkValidateDocuments.UseVisualStyleBackColor = true;

            //
            // chkSkipCorrupted
            //
            this.chkSkipCorrupted.AutoSize = true;
            this.chkSkipCorrupted.BackColor = System.Drawing.Color.White;
            this.chkSkipCorrupted.FlatAppearance.BorderSize = 0;
            this.chkSkipCorrupted.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkSkipCorrupted.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkSkipCorrupted.Location = new System.Drawing.Point(20, 90);
            this.chkSkipCorrupted.Name = "chkSkipCorrupted";
            this.chkSkipCorrupted.Size = new System.Drawing.Size(189, 19);
            this.chkSkipCorrupted.TabIndex = 2;
            this.chkSkipCorrupted.Text = "Skip corrupted documents";
            this.chkSkipCorrupted.UseVisualStyleBackColor = true;

            //
            // chkPreserveAttributes
            //
            this.chkPreserveAttributes.AutoSize = true;
            this.chkPreserveAttributes.BackColor = System.Drawing.Color.White;
            this.chkPreserveAttributes.FlatAppearance.BorderSize = 0;
            this.chkPreserveAttributes.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkPreserveAttributes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkPreserveAttributes.Location = new System.Drawing.Point(20, 115);
            this.chkPreserveAttributes.Name = "chkPreserveAttributes";
            this.chkPreserveAttributes.Size = new System.Drawing.Size(151, 19);
            this.chkPreserveAttributes.TabIndex = 3;
            this.chkPreserveAttributes.Text = "Preserve file attributes (timestamps, read-only, etc.)";
            this.chkPreserveAttributes.UseVisualStyleBackColor = true;


            //
            // lblMaxFileSize
            //
            this.lblMaxFileSize.AutoSize = true;
            this.lblMaxFileSize.BackColor = System.Drawing.Color.Transparent;
            this.lblMaxFileSize.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblMaxFileSize.Location = new System.Drawing.Point(20, 140);
            this.lblMaxFileSize.Name = "lblMaxFileSize";
            this.lblMaxFileSize.Size = new System.Drawing.Size(120, 15);
            this.lblMaxFileSize.TabIndex = 5;
            this.lblMaxFileSize.Text = "Max file size (MB):";

            //
            // numMaxFileSize
            //
            this.numMaxFileSize.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.numMaxFileSize.Location = new System.Drawing.Point(150, 138);
            this.numMaxFileSize.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numMaxFileSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMaxFileSize.Name = "numMaxFileSize";
            this.numMaxFileSize.Size = new System.Drawing.Size(80, 23);
            this.numMaxFileSize.TabIndex = 6;
            this.numMaxFileSize.Value = new decimal(new int[] { 100, 0, 0, 0 });

            //
            // lblConcurrentFiles
            //
            this.lblConcurrentFiles.AutoSize = true;
            this.lblConcurrentFiles.BackColor = System.Drawing.Color.Transparent;
            this.lblConcurrentFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblConcurrentFiles.Location = new System.Drawing.Point(20, 170);
            this.lblConcurrentFiles.Name = "lblConcurrentFiles";
            this.lblConcurrentFiles.Size = new System.Drawing.Size(107, 15);
            this.lblConcurrentFiles.TabIndex = 7;
            this.lblConcurrentFiles.Text = "Concurrent files:";

            //
            // numConcurrentFiles
            //
            this.numConcurrentFiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.numConcurrentFiles.Location = new System.Drawing.Point(150, 168);
            this.numConcurrentFiles.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numConcurrentFiles.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numConcurrentFiles.Name = "numConcurrentFiles";
            this.numConcurrentFiles.Size = new System.Drawing.Size(80, 23);
            this.numConcurrentFiles.TabIndex = 8;
            this.numConcurrentFiles.Value = new decimal(new int[] { 3, 0, 0, 0 });

            //
            // lblProcessingTimeout
            //
            this.lblProcessingTimeout.AutoSize = true;
            this.lblProcessingTimeout.BackColor = System.Drawing.Color.Transparent;
            this.lblProcessingTimeout.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblProcessingTimeout.Location = new System.Drawing.Point(20, 200);
            this.lblProcessingTimeout.Name = "lblProcessingTimeout";
            this.lblProcessingTimeout.Size = new System.Drawing.Size(140, 15);
            this.lblProcessingTimeout.TabIndex = 9;
            this.lblProcessingTimeout.Text = "Processing timeout (sec):";

            //
            // numProcessingTimeout
            //
            this.numProcessingTimeout.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.numProcessingTimeout.Location = new System.Drawing.Point(170, 198);
            this.numProcessingTimeout.Maximum = new decimal(new int[] { 7200, 0, 0, 0 });
            this.numProcessingTimeout.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numProcessingTimeout.Name = "numProcessingTimeout";
            this.numProcessingTimeout.Size = new System.Drawing.Size(80, 23);
            this.numProcessingTimeout.TabIndex = 10;
            this.numProcessingTimeout.Value = new decimal(new int[] { 60, 0, 0, 0 });

            //
            // tabInterface
            //
            this.tabInterface.Controls.Add(this.grpInterfaceSettings);
            this.tabInterface.Location = new System.Drawing.Point(4, 24);
            this.tabInterface.Name = "tabInterface";
            this.tabInterface.Padding = new System.Windows.Forms.Padding(15);
            this.tabInterface.Size = new System.Drawing.Size(752, 392);
            this.tabInterface.TabIndex = 3;
            this.tabInterface.Text = "🎨 Interface";
            this.tabInterface.UseVisualStyleBackColor = true;

            //
            // grpInterfaceSettings
            //
            this.grpInterfaceSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpInterfaceSettings.BackColor = System.Drawing.Color.Transparent;
            this.grpInterfaceSettings.Controls.Add(this.cmbLanguage);
            this.grpInterfaceSettings.Controls.Add(this.lblLanguage);
            this.grpInterfaceSettings.Controls.Add(this.cmbTheme);
            this.grpInterfaceSettings.Controls.Add(this.lblTheme);
            this.grpInterfaceSettings.Controls.Add(this.chkShowStatusBar);
            this.grpInterfaceSettings.Controls.Add(this.chkConfirmOnExit);
            this.grpInterfaceSettings.Controls.Add(this.chkShowToolTips);
            this.grpInterfaceSettings.Controls.Add(this.chkAutoSelectFirstFile);
            this.grpInterfaceSettings.Controls.Add(this.chkShowProgressDetails);
            this.grpInterfaceSettings.Controls.Add(this.chkRememberWindowSize);
            this.grpInterfaceSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpInterfaceSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(80)))), ((int)(((byte)(87)))));
            this.grpInterfaceSettings.Location = new System.Drawing.Point(15, 15);
            this.grpInterfaceSettings.Name = "grpInterfaceSettings";
            this.grpInterfaceSettings.Padding = new System.Windows.Forms.Padding(20);
            this.grpInterfaceSettings.Size = new System.Drawing.Size(722, 362);
            this.grpInterfaceSettings.TabIndex = 0;
            this.grpInterfaceSettings.TabStop = false;
            this.grpInterfaceSettings.Text = "User Interface";

            //
            // chkRememberWindowSize
            //
            this.chkRememberWindowSize.AutoSize = true;
            this.chkRememberWindowSize.BackColor = System.Drawing.Color.White;
            this.chkRememberWindowSize.FlatAppearance.BorderSize = 0;
            this.chkRememberWindowSize.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkRememberWindowSize.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkRememberWindowSize.Location = new System.Drawing.Point(20, 40);
            this.chkRememberWindowSize.Name = "chkRememberWindowSize";
            this.chkRememberWindowSize.Size = new System.Drawing.Size(154, 19);
            this.chkRememberWindowSize.TabIndex = 0;
            this.chkRememberWindowSize.Text = "Remember window size";
            this.chkRememberWindowSize.UseVisualStyleBackColor = true;

            //
            // chkShowProgressDetails
            //
            this.chkShowProgressDetails.AutoSize = true;
            this.chkShowProgressDetails.BackColor = System.Drawing.Color.White;
            this.chkShowProgressDetails.FlatAppearance.BorderSize = 0;
            this.chkShowProgressDetails.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkShowProgressDetails.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkShowProgressDetails.Location = new System.Drawing.Point(20, 65);
            this.chkShowProgressDetails.Name = "chkShowProgressDetails";
            this.chkShowProgressDetails.Size = new System.Drawing.Size(139, 19);
            this.chkShowProgressDetails.TabIndex = 1;
            this.chkShowProgressDetails.Text = "Show progress details";
            this.chkShowProgressDetails.UseVisualStyleBackColor = true;

            //
            // chkAutoSelectFirstFile
            //
            this.chkAutoSelectFirstFile.AutoSize = true;
            this.chkAutoSelectFirstFile.BackColor = System.Drawing.Color.White;
            this.chkAutoSelectFirstFile.FlatAppearance.BorderSize = 0;
            this.chkAutoSelectFirstFile.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkAutoSelectFirstFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkAutoSelectFirstFile.Location = new System.Drawing.Point(20, 90);
            this.chkAutoSelectFirstFile.Name = "chkAutoSelectFirstFile";
            this.chkAutoSelectFirstFile.Size = new System.Drawing.Size(143, 19);
            this.chkAutoSelectFirstFile.TabIndex = 2;
            this.chkAutoSelectFirstFile.Text = "Auto-select first file";
            this.chkAutoSelectFirstFile.UseVisualStyleBackColor = true;

            //
            // chkShowToolTips
            //
            this.chkShowToolTips.AutoSize = true;
            this.chkShowToolTips.BackColor = System.Drawing.Color.White;
            this.chkShowToolTips.FlatAppearance.BorderSize = 0;
            this.chkShowToolTips.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkShowToolTips.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkShowToolTips.Location = new System.Drawing.Point(20, 115);
            this.chkShowToolTips.Name = "chkShowToolTips";
            this.chkShowToolTips.Size = new System.Drawing.Size(105, 19);
            this.chkShowToolTips.TabIndex = 3;
            this.chkShowToolTips.Text = "Show tool tips";
            this.chkShowToolTips.UseVisualStyleBackColor = true;

            //
            // chkConfirmOnExit
            //
            this.chkConfirmOnExit.AutoSize = true;
            this.chkConfirmOnExit.BackColor = System.Drawing.Color.White;
            this.chkConfirmOnExit.FlatAppearance.BorderSize = 0;
            this.chkConfirmOnExit.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkConfirmOnExit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkConfirmOnExit.Location = new System.Drawing.Point(20, 140);
            this.chkConfirmOnExit.Name = "chkConfirmOnExit";
            this.chkConfirmOnExit.Size = new System.Drawing.Size(111, 19);
            this.chkConfirmOnExit.TabIndex = 4;
            this.chkConfirmOnExit.Text = "Confirm on exit";
            this.chkConfirmOnExit.UseVisualStyleBackColor = true;

            //
            // chkShowStatusBar
            //
            this.chkShowStatusBar.AutoSize = true;
            this.chkShowStatusBar.BackColor = System.Drawing.Color.White;
            this.chkShowStatusBar.FlatAppearance.BorderSize = 0;
            this.chkShowStatusBar.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.chkShowStatusBar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkShowStatusBar.Location = new System.Drawing.Point(20, 165);
            this.chkShowStatusBar.Name = "chkShowStatusBar";
            this.chkShowStatusBar.Size = new System.Drawing.Size(110, 19);
            this.chkShowStatusBar.TabIndex = 5;
            this.chkShowStatusBar.Text = "Show status bar";
            this.chkShowStatusBar.UseVisualStyleBackColor = true;


            //
            // lblTheme
            //
            this.lblTheme.AutoSize = true;
            this.lblTheme.BackColor = System.Drawing.Color.Transparent;
            this.lblTheme.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTheme.Location = new System.Drawing.Point(20, 255);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Size = new System.Drawing.Size(46, 15);
            this.lblTheme.TabIndex = 8;
            this.lblTheme.Text = "Theme:";

            //
            // cmbTheme
            //
            this.cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTheme.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbTheme.FormattingEnabled = true;
            this.cmbTheme.Items.AddRange(new object[] { "Light", "Dark", "Auto" });
            this.cmbTheme.Location = new System.Drawing.Point(80, 252);
            this.cmbTheme.Name = "cmbTheme";
            this.cmbTheme.Size = new System.Drawing.Size(120, 23);
            this.cmbTheme.TabIndex = 9;

            //
            // lblLanguage
            //
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.BackColor = System.Drawing.Color.Transparent;
            this.lblLanguage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblLanguage.Location = new System.Drawing.Point(220, 255);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(62, 15);
            this.lblLanguage.TabIndex = 10;
            this.lblLanguage.Text = "Language:";

            //
            // cmbLanguage
            //
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] { "English", "Spanish", "French", "German" });
            this.cmbLanguage.Location = new System.Drawing.Point(290, 252);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(120, 23);
            this.cmbLanguage.TabIndex = 11;


            //
            // pnlButtons
            //
            this.pnlButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlButtons.Controls.Add(this.btnExportSettings);
            this.pnlButtons.Controls.Add(this.btnImportSettings);
            this.pnlButtons.Controls.Add(this.btnOpenCurrentFolder);
            this.pnlButtons.Controls.Add(this.btnResetToDefaults);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Location = new System.Drawing.Point(20, 510);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(760, 50);
            this.pnlButtons.TabIndex = 2;

            //
            // btnSave
            //
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(560, 10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 35);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "💾 Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);

            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(660, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "❌ Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

            //
            // btnResetToDefaults
            //
            this.btnResetToDefaults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
            this.btnResetToDefaults.FlatAppearance.BorderSize = 0;
            this.btnResetToDefaults.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetToDefaults.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnResetToDefaults.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.btnResetToDefaults.Location = new System.Drawing.Point(140, 10);
            this.btnResetToDefaults.Name = "btnResetToDefaults";
            this.btnResetToDefaults.Size = new System.Drawing.Size(120, 45);
            this.btnResetToDefaults.TabIndex = 1;
            this.btnResetToDefaults.Text = "🔄 Reset to Defaults";
            this.btnResetToDefaults.UseVisualStyleBackColor = true;
            this.btnResetToDefaults.Click += new System.EventHandler(this.BtnResetToDefaults_Click);

            //
            // btnOpenCurrentFolder
            //
            this.btnOpenCurrentFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnOpenCurrentFolder.FlatAppearance.BorderSize = 0;
            this.btnOpenCurrentFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenCurrentFolder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnOpenCurrentFolder.ForeColor = System.Drawing.Color.White;
            this.btnOpenCurrentFolder.Location = new System.Drawing.Point(10, 10);
            this.btnOpenCurrentFolder.Name = "btnOpenCurrentFolder";
            this.btnOpenCurrentFolder.Size = new System.Drawing.Size(120, 35);
            this.btnOpenCurrentFolder.TabIndex = 0;
            this.btnOpenCurrentFolder.Text = "📁 Open Folder";
            this.btnOpenCurrentFolder.UseVisualStyleBackColor = true;
            this.btnOpenCurrentFolder.Click += new System.EventHandler(this.BtnOpenCurrentFolder_Click);

            //
            // btnImportSettings
            //
            this.btnImportSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnImportSettings.FlatAppearance.BorderSize = 0;
            this.btnImportSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportSettings.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnImportSettings.ForeColor = System.Drawing.Color.White;
            this.btnImportSettings.Location = new System.Drawing.Point(270, 10);
            this.btnImportSettings.Name = "btnImportSettings";
            this.btnImportSettings.Size = new System.Drawing.Size(80, 35);
            this.btnImportSettings.TabIndex = 4;
            this.btnImportSettings.Text = "📥 Import";
            this.btnImportSettings.UseVisualStyleBackColor = true;
            this.btnImportSettings.Click += new System.EventHandler(this.BtnImportSettings_Click);

            //
            // btnExportSettings
            //
            this.btnExportSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnExportSettings.FlatAppearance.BorderSize = 0;
            this.btnExportSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportSettings.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnExportSettings.ForeColor = System.Drawing.Color.White;
            this.btnExportSettings.Location = new System.Drawing.Point(360, 10);
            this.btnExportSettings.Name = "btnExportSettings";
            this.btnExportSettings.Size = new System.Drawing.Size(80, 35);
            this.btnExportSettings.TabIndex = 5;
            this.btnExportSettings.Text = "📤 Export";
            this.btnExportSettings.UseVisualStyleBackColor = true;
            this.btnExportSettings.Click += new System.EventHandler(this.BtnExportSettings_Click);

            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(183, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Application Settings";

            //
            // lblDescription
            //
            this.lblDescription.AutoSize = true;
            this.lblDescription.BackColor = System.Drawing.Color.Transparent;
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblDescription.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblDescription.Location = new System.Drawing.Point(20, 50);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(451, 15);
            this.lblDescription.TabIndex = 1;
            this.lblDescription.Text = "Configure how the application stores and organizes changelogs and other settings.";

            //
            // SettingsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 560);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.tabControl);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Bulk Editor - Settings";

            this.tabControl.ResumeLayout(false);
            this.tabChangelog.ResumeLayout(false);
            this.grpChangelogSettings.ResumeLayout(false);
            this.grpChangelogSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoCleanupDays)).EndInit();
            this.tabApplication.ResumeLayout(false);
            this.grpApplicationSettings.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numMaxBatchSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRecentFiles)).EndInit();
            this.tabProcessing.ResumeLayout(false);
            this.grpProcessingSettings.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numMaxFileSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numConcurrentFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numProcessingTimeout)).EndInit();
            this.tabInterface.ResumeLayout(false);
            this.grpInterfaceSettings.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}