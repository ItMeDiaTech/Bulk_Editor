namespace Bulk_Editor
{
    partial class SettingsForm
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
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnResetToDefaults = new System.Windows.Forms.Button();
            this.btnOpenCurrentFolder = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();

            this.grpChangelogSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoCleanupDays)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            //
            // grpChangelogSettings
            //
            this.grpChangelogSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.grpChangelogSettings.Location = new System.Drawing.Point(20, 80);
            this.grpChangelogSettings.Name = "grpChangelogSettings";
            this.grpChangelogSettings.Padding = new System.Windows.Forms.Padding(20);
            this.grpChangelogSettings.Size = new System.Drawing.Size(560, 300);
            this.grpChangelogSettings.TabIndex = 0;
            this.grpChangelogSettings.TabStop = false;
            this.grpChangelogSettings.Text = "Changelog Storage Settings";

            //
            // lblBaseStoragePath
            //
            this.lblBaseStoragePath.AutoSize = true;
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
            this.btnBrowsePath.UseVisualStyleBackColor = false;
            this.btnBrowsePath.Click += new System.EventHandler(this.BtnBrowsePath_Click);

            //
            // chkUseCentralizedStorage
            //
            this.chkUseCentralizedStorage.AutoSize = true;
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
            this.chkCentralizeBackups.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkCentralizeBackups.Location = new System.Drawing.Point(40, 180);
            this.chkCentralizeBackups.Name = "chkCentralizeBackups";
            this.chkCentralizeBackups.Size = new System.Drawing.Size(294, 19);
            this.chkCentralizeBackups.TabIndex = 6;
            this.chkCentralizeBackups.Text = "Store document backups in centralized location too";
            this.chkCentralizeBackups.UseVisualStyleBackColor = true;

            //
            // lblAutoCleanupDays
            //
            this.lblAutoCleanupDays.AutoSize = true;
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
            this.lblAutoCleanupNote.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblAutoCleanupNote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblAutoCleanupNote.Location = new System.Drawing.Point(296, 222);
            this.lblAutoCleanupNote.Name = "lblAutoCleanupNote";
            this.lblAutoCleanupNote.Size = new System.Drawing.Size(91, 13);
            this.lblAutoCleanupNote.TabIndex = 9;
            this.lblAutoCleanupNote.Text = "days (0 = disabled)";

            //
            // pnlButtons
            //
            this.pnlButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlButtons.Controls.Add(this.btnOpenCurrentFolder);
            this.pnlButtons.Controls.Add(this.btnResetToDefaults);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Location = new System.Drawing.Point(20, 390);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(560, 50);
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
            this.btnSave.Location = new System.Drawing.Point(360, 10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 35);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "💾 Save";
            this.btnSave.UseVisualStyleBackColor = false;
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
            this.btnCancel.Location = new System.Drawing.Point(460, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "❌ Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
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
            this.btnResetToDefaults.Size = new System.Drawing.Size(120, 35);
            this.btnResetToDefaults.TabIndex = 1;
            this.btnResetToDefaults.Text = "🔄 Reset to Defaults";
            this.btnResetToDefaults.UseVisualStyleBackColor = false;
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
            this.btnOpenCurrentFolder.UseVisualStyleBackColor = false;
            this.btnOpenCurrentFolder.Click += new System.EventHandler(this.BtnOpenCurrentFolder_Click);

            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
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
            this.ClientSize = new System.Drawing.Size(600, 460);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.grpChangelogSettings);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Bulk Editor - Settings";

            this.grpChangelogSettings.ResumeLayout(false);
            this.grpChangelogSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoCleanupDays)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

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
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnResetToDefaults;
        private System.Windows.Forms.Button btnOpenCurrentFolder;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDescription;
    }
}