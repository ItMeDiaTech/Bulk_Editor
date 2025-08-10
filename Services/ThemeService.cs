using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Bulk_Editor.Configuration;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Service for managing application themes
    /// </summary>
    public class ThemeService
    {
        private readonly UiSettings _settings;

        public ThemeService(UiSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Apply theme to a form and all its controls
        /// </summary>
        public void ApplyTheme(Form form)
        {
            try
            {
                // Suspend layout to prevent flicker and improve performance
                form.SuspendLayout();

                var theme = GetCurrentTheme();
                ApplyThemeToControl(form, theme);

                // Apply theme-appropriate icons
                ApplyThemeIcons(form, theme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to form: {ex.Message}");
            }
            finally
            {
                // Always resume layout
                try
                {
                    form.ResumeLayout(true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error resuming layout: {ex.Message}");
                }
            }
        }

        private void ApplySettingsButtonTheme(Button b, ThemeConfiguration theme)
        {
            // Surface: fully transparent, no OS painting
            b.UseVisualStyleBackColor = false;
            b.BackColor = Color.Transparent;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.Transparent;
            b.FlatAppearance.MouseDownBackColor = Color.Transparent;

            // Icon only, no background image stacking
            b.BackgroundImage = null;
            b.Text = string.Empty;
            b.ImageAlign = ContentAlignment.MiddleCenter;
            b.Padding = Padding.Empty;

            // Pick an icon for the current theme
            bool dark = string.Equals(_settings.Theme, "dark", StringComparison.OrdinalIgnoreCase)
                     || (string.Equals(_settings.Theme, "auto", StringComparison.OrdinalIgnoreCase) && SystemSupportsAutoDarkMode());

            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var iconResource = dark ? "Bulk_Editor.White_Settings_Icon_25x25.png"   // white gear for dark UI
                                        : "Bulk_Editor.Settings_Icon.png";              // dark gear for light UI

                using var stream = assembly.GetManifestResourceStream(iconResource);
                if (stream != null)
                {
                    // Dispose old image if it exists to prevent memory leaks
                    var oldImage = b.Image;
                    b.Image = System.Drawing.Image.FromStream(stream);

                    // Only dispose if it's not the same image reference
                    if (oldImage != null && oldImage != b.Image)
                    {
                        oldImage.Dispose();
                    }
                }
                else
                {
                    // Fallback to emoji if resource not found
                    b.Text = "⚙️";
                    b.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                    b.ForeColor = dark ? System.Drawing.Color.White : System.Drawing.Color.Black;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings icon: {ex.Message}");
                // Fallback to emoji
                b.Text = "⚙️";
                b.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                b.ForeColor = dark ? System.Drawing.Color.White : System.Drawing.Color.Black;
            }

            // Wire the pressed effect once
            if (!Equals(b.Tag, "settings-press-wired"))
            {
                var ring = theme.BorderColor; // use your theme border color

                // Mouse press -> push + ring
                b.MouseDown += (s, e) =>
                {
                    var btn = (Button)s!;
                    btn.Padding = new Padding(1, 2, 0, 0); // nudge down/right
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = ring;
                };

                // Release (mouse/key/leave) -> reset
                void ResetButtonState(object s, EventArgs e)
                {
                    var btn = (Button)s!;
                    btn.Padding = Padding.Empty;
                    btn.FlatAppearance.BorderSize = 0;
                }

                b.MouseUp += (s, e) => ResetButtonState(s, e);
                b.MouseLeave += ResetButtonState;
                b.KeyUp += (s, e) => ResetButtonState(s, e);

                // Keyboard press should also look pressed
                b.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
                    {
                        var btn = (Button)s!;
                        btn.Padding = new Padding(1, 2, 0, 0);
                        btn.FlatAppearance.BorderSize = 1;
                        btn.FlatAppearance.BorderColor = ring;
                    }
                };

                b.Tag = "settings-press-wired";
            }
        }

        /// <summary>
        /// Apply theme-appropriate icons to controls
        /// </summary>
        private void ApplyThemeIcons(Form form, ThemeConfiguration theme)
        {
            try
            {
                // Find settings button and apply appropriate icon
                var settingsButton = FindControlByName(form, "btnSettings") as Button;
                if (settingsButton != null)
                {
                    ApplySettingsButtonIcon(settingsButton, theme);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme icons: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply appropriate settings icon based on theme
        /// </summary>
        private static void ApplySettingsButtonIcon(Button settingsButton, ThemeConfiguration theme)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var iconResource = IsLightTheme(theme) ? "Bulk_Editor.Settings_Icon.png" : "Bulk_Editor.White_Settings_Icon_25x25.png";

                using var stream = assembly.GetManifestResourceStream(iconResource);
                if (stream != null)
                {
                    // Dispose old image if it exists to prevent memory leaks
                    var oldImage = settingsButton.Image;
                    settingsButton.Image = System.Drawing.Image.FromStream(stream);

                    // Only dispose if it's not the same image reference
                    if (oldImage != null && oldImage != settingsButton.Image)
                    {
                        oldImage.Dispose();
                    }
                }
                else
                {
                    // Fallback to emoji if resource not found
                    settingsButton.Text = "⚙️";
                    settingsButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                    settingsButton.ForeColor = IsLightTheme(theme) ? System.Drawing.Color.Black : System.Drawing.Color.White;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings icon: {ex.Message}");
                // Fallback to emoji
                settingsButton.Text = "⚙️";
                settingsButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                settingsButton.ForeColor = IsLightTheme(theme) ? System.Drawing.Color.Black : System.Drawing.Color.White;
            }
        }

        /// <summary>
        /// Recursively find a control by name
        /// </summary>
        private static Control FindControlByName(Control parent, string name)
        {
            if (parent.Name == name)
                return parent;

            foreach (Control child in parent.Controls)
            {
                var found = FindControlByName(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Determine if the current theme is light theme
        /// </summary>
        private static bool IsLightTheme(ThemeConfiguration theme)
        {
            // Check if the form background is closer to white (light theme) or dark
            var formBg = theme.FormBackground;
            var brightness = (formBg.R + formBg.G + formBg.B) / 3.0;
            return brightness > 127; // Threshold for light vs dark
        }

        /// <summary>
        /// Get the current theme configuration
        /// </summary>
        public ThemeConfiguration GetCurrentTheme()
        {
            return _settings.Theme.ToLowerInvariant() switch
            {
                "dark" => CreateDarkTheme(),
                "light" => CreateLightTheme(),
                "auto" => SystemSupportsAutoDarkMode() ? CreateDarkTheme() : CreateLightTheme(),
                _ => CreateLightTheme()
            };
        }

        private void ApplyThemeToControl(Control control, ThemeConfiguration theme)
        {
            try
            {
                // Skip disposed or null controls
                if (control?.IsDisposed != false)
                    return;

                // Apply theme to the control itself
                ApplyControlTheme(control, theme);

                // Use a copy of the controls collection to avoid modification issues
                // Check if control is still valid before accessing its Controls collection
                if (control.IsDisposed)
                    return;

                var childControls = new Control[control.Controls.Count];
                control.Controls.CopyTo(childControls, 0);

                // Recursively apply to child controls
                foreach (Control child in childControls)
                {
                    if (child?.IsDisposed == false)
                    {
                        ApplyThemeToControl(child, theme);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Control was disposed during theming - this is normal, just return
                return;
            }
            catch (Exception ex)
            {
                // Log the error but don't let theming break the application
                System.Diagnostics.Debug.WriteLine($"Error applying theme to control {control?.Name ?? "unknown"}: {ex.Message}");
            }
        }

        private void ApplyControlTheme(Control control, ThemeConfiguration theme)
        {
            ListBox listBoxToEndUpdate = null;
            ComboBox comboBoxToEndUpdate = null;

            try
            {
                // Skip controls that should maintain their custom styling or are disposed
                if (ShouldSkipControl(control))
                    return;

                // Use BeginUpdate/EndUpdate for controls that support it to reduce flicker
                if (control is ListBox listBox && !listBox.IsDisposed)
                {
                    listBox.BeginUpdate();
                    listBoxToEndUpdate = listBox;
                }
                else if (control is ComboBox comboBox && !comboBox.IsDisposed)
                {
                    comboBox.BeginUpdate();
                    comboBoxToEndUpdate = comboBox;
                }

                // Apply colors safely - always set both background and foreground to prevent stacking
                try
                {
                    if (control.IsDisposed)
                        return;

                    var backgroundColor = GetBackgroundColor(control, theme);
                    var foregroundColor = GetForegroundColor(control, theme);

                    // Only set colors if they're different from current to avoid flicker
                    if (!control.IsDisposed && control.BackColor != backgroundColor)
                        control.BackColor = backgroundColor;
                    if (!control.IsDisposed && control.ForeColor != foregroundColor)
                        control.ForeColor = foregroundColor;
                }
                catch (ObjectDisposedException)
                {
                    // Control was disposed - this is normal during form closing
                    return;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting colors for {control?.Name ?? "unknown"}: {ex.Message}");
                }

                // Skip special handling if control is disposed
                if (control.IsDisposed)
                    return;

                // Special handling for specific control types
                switch (control)
                {
                    case Button b when b.Name == "btnSettings":
                        ApplySettingsButtonTheme(b, theme);
                        break;
                    case Button button when !button.IsDisposed:
                        ApplyButtonTheme(button, theme);
                        break;
                    case TextBox textBox when !textBox.IsDisposed:
                        ApplyTextBoxTheme(textBox, theme);
                        break;
                    case ListBox lb when !lb.IsDisposed:
                        ApplyListBoxTheme(lb, theme);
                        break;
                    case ComboBox cb when !cb.IsDisposed:
                        ApplyComboBoxTheme(cb, theme);
                        break;
                    case GroupBox groupBox when !groupBox.IsDisposed:
                        ApplyGroupBoxTheme(groupBox, theme);
                        break;
                    case CheckBox checkBox when !checkBox.IsDisposed:
                        ApplyCheckBoxTheme(checkBox, theme);
                        break;
                    case RadioButton radio when !radio.IsDisposed:
                        ApplyRadioButtonTheme(radio, theme);
                        break;
                    case TabControl tabControl when !tabControl.IsDisposed:
                        ApplyTabControlTheme(tabControl, theme);
                        break;
                    case ProgressBar progressBar when !progressBar.IsDisposed:
                        ApplyProgressBarTheme(progressBar, theme);
                        break;
                }
            }
            catch (ObjectDisposedException)
            {
                // Control was disposed during theming - this is normal
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to control {control?.Name ?? "unknown"}: {ex.Message}");
            }
            finally
            {
                // Always end update for controls that support it, even if an exception occurred
                try
                {
                    listBoxToEndUpdate?.EndUpdate();
                }
                catch (ObjectDisposedException)
                {
                    // Control disposed - ignore
                }

                try
                {
                    comboBoxToEndUpdate?.EndUpdate();
                }
                catch (ObjectDisposedException)
                {
                    // Control disposed - ignore
                }
            }
        }

        private bool ShouldSkipControl(Control control)
        {
            // Skip disposed controls
            if (control.IsDisposed)
                return true;

            // Skip controls that already have custom styling or are colored buttons
            if (control is Button button)
            {
                // Skip buttons with custom colors (non-default) except transparent ones
                return button.BackColor != SystemColors.Control &&
                       button.BackColor != Color.Empty &&
                       button.BackColor != Color.Transparent &&
                       !IsDefaultButtonColor(button.BackColor);
            }

            // Skip certain system controls that shouldn't be themed
            if (control is Form || control is MenuStrip || control is ToolStrip || control is StatusStrip)
            {
                return false; // These can be themed, but handle them carefully
            }

            // Don't skip invisible controls - they may become visible later
            return false;
        }

        private static bool IsDefaultButtonColor(Color color)
        {
            // Check if it's a default system color
            return color == SystemColors.Control ||
                   color == SystemColors.ButtonFace ||
                   color == Color.FromArgb(240, 240, 240); // Default light theme button
        }

        private Color GetBackgroundColor(Control control, ThemeConfiguration theme)
        {
            // Keep surfaces for inputs and containers, but make text-only chrome transparent
            return control switch
            {
                Form => theme.FormBackground,
                Panel panel when panel.Name == "pnlStatus" => theme.StatusBarBackground,
                Panel => theme.PanelBackground,

                // Inputs/surfaces keep their own background
                TextBox => theme.TextBoxBackground,
                ListBox => theme.ListBoxBackground,
                ComboBox => theme.ComboBoxBackground,

                // Containers
                GroupBox => theme.GroupBoxBackground,
                TabControl => theme.TabControlBackground,

                // TEXT-ONLY / CHROME-LIGHT controls -> transparent
                Label => Color.Transparent,
                CheckBox => Color.Transparent,
                RadioButton => Color.Transparent,

                _ => theme.ControlBackground
            };
        }

        private Color GetForegroundColor(Control control, ThemeConfiguration theme)
        {
            return control switch
            {
                Label label when label.Name == "lblTitle" => theme.TitleForeground,
                Label label when label.Name == "lblStatus" => theme.StatusForeground,
                LinkLabel => theme.LabelForeground,
                Label => theme.LabelForeground,
                CheckBox => theme.CheckBoxForeground,
                RadioButton => theme.CheckBoxForeground,
                _ => theme.ControlForeground
            };
        }

        private static void ApplyButtonTheme(Button button, ThemeConfiguration theme)
        {
            // Only apply theme to default-styled buttons
            if (IsDefaultButtonColor(button.BackColor))
            {
                button.BackColor = theme.ButtonBackground;
                button.ForeColor = theme.ButtonForeground;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = theme.BorderColor;
                button.FlatAppearance.BorderSize = 1;
            }
        }

        private void ApplyCheckBoxTheme(CheckBox checkBox, ThemeConfiguration theme)
        {
            // Use OS visual styles and let parent surface show through
            checkBox.UseVisualStyleBackColor = true;
            checkBox.FlatStyle = FlatStyle.Standard; // or FlatStyle.System
            checkBox.BackColor = Color.Transparent;

            // For the main checkbox and its sub-checkboxes, let MainForm.UIHelpers handle the coloring
            // to avoid conflicts between theme service and UI helpers
            if (checkBox.Name == "chkFixSourceHyperlinks" || IsSubCheckBox(checkBox))
            {
                // Don't set colors here - UIHelpers will handle it
                return;
            }

            // For other checkboxes, apply standard theme coloring
            checkBox.ForeColor = theme.CheckBoxForeground;
        }

        private static void UpdatePrimaryCheckBoxColor(CheckBox checkBox, ThemeConfiguration theme)
        {
            // Primary checkbox (Fix Source Hyperlinks) should always remain visible
            // regardless of its checked state
            checkBox.ForeColor = theme.PrimaryCheckBoxForeground;
        }

        private static void UpdateSubCheckBoxColor(CheckBox checkBox, ThemeConfiguration theme)
        {
            var parentCheckBox = GetParentCheckBox(checkBox);
            checkBox.ForeColor = (parentCheckBox != null && !parentCheckBox.Checked)
                ? theme.DisabledCheckBoxForeground
                : theme.SubCheckBoxForeground;
        }

        private static void UpdateSubCheckBoxColors(Form form, ThemeConfiguration theme)
        {
            if (form == null) return;

            var subCheckBoxNames = new[] { "chkAppendContentID", "chkCheckTitleChanges", "chkFixTitles" };
            foreach (var name in subCheckBoxNames)
            {
                var subCheckBox = form.Controls.Find(name, true).FirstOrDefault() as CheckBox;
                if (subCheckBox != null)
                {
                    UpdateSubCheckBoxColor(subCheckBox, theme);
                }
            }
        }

        private static void ApplyRadioButtonTheme(RadioButton radio, ThemeConfiguration theme)
        {
            radio.UseVisualStyleBackColor = true;
            radio.FlatStyle = FlatStyle.Standard;
            radio.BackColor = Color.Transparent;
            radio.ForeColor = theme.CheckBoxForeground;
        }

        private static bool IsSubCheckBox(CheckBox checkBox)
        {
            // Check if this is a sub-checkbox based on naming convention
            return checkBox.Name == "chkAppendContentID" ||
                   checkBox.Name == "chkCheckTitleChanges" ||
                   checkBox.Name == "chkFixTitles";
        }

        private static CheckBox GetParentCheckBox(CheckBox subCheckBox)
        {
            // Find the parent form and locate the Fix Source Hyperlinks checkbox
            var form = subCheckBox.FindForm();
            return form?.Controls.Find("chkFixSourceHyperlinks", true).FirstOrDefault() as CheckBox;
        }

        private static void ApplyTextBoxTheme(TextBox textBox, ThemeConfiguration theme)
        {
            textBox.BackColor = theme.TextBoxBackground;
            textBox.ForeColor = theme.TextBoxForeground;
            textBox.BorderStyle = BorderStyle.FixedSingle;

            // Special handling for changelog text box to match files list font
            if (textBox.Name == "txtChangelog")
            {
                var form = textBox.FindForm();
                var filesListBox = form?.Controls.Find("lstFiles", true).FirstOrDefault() as ListBox;
                if (filesListBox != null)
                {
                    // Dispose the old font if it's not the default font to prevent memory leaks
                    var oldFont = textBox.Font;
                    var newFont = new Font(filesListBox.Font.FontFamily, filesListBox.Font.Size, FontStyle.Regular);

                    textBox.Font = newFont;

                    // Only dispose the old font if it's not the default system font
                    if (oldFont != null && oldFont != Control.DefaultFont && oldFont != SystemFonts.DefaultFont)
                    {
                        oldFont.Dispose();
                    }
                }
            }
        }

        private static void ApplyListBoxTheme(ListBox listBox, ThemeConfiguration theme)
        {
            listBox.BackColor = theme.ListBoxBackground;
            listBox.ForeColor = theme.ListBoxForeground;
            listBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void ApplyComboBoxTheme(ComboBox comboBox, ThemeConfiguration theme)
        {
            comboBox.BackColor = theme.ComboBoxBackground;
            comboBox.ForeColor = theme.ComboBoxForeground;
            comboBox.FlatStyle = FlatStyle.Flat;
        }

        private static void ApplyGroupBoxTheme(GroupBox groupBox, ThemeConfiguration theme)
        {
            groupBox.ForeColor = theme.GroupBoxForeground;
            groupBox.BackColor = theme.GroupBoxBackground; // Transparent in both themes
        }

        private static void ApplyTabControlTheme(TabControl tabControl, ThemeConfiguration theme)
        {
            tabControl.BackColor = theme.TabControlBackground;
            foreach (TabPage tab in tabControl.TabPages)
            {
                tab.BackColor = theme.TabPageBackground;
                tab.ForeColor = theme.TabPageForeground;
            }
        }

        private static void ApplyProgressBarTheme(ProgressBar progressBar, ThemeConfiguration theme)
        {
            progressBar.BackColor = theme.ProgressBarBackground;
        }

        private static bool SystemSupportsAutoDarkMode()
        {
            try
            {
                // Simple check for Windows dark mode
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key?.GetValue("AppsUseLightTheme") is int value)
                {
                    return value == 0; // 0 = dark mode, 1 = light mode
                }
            }
            catch
            {
                // Fall back to light theme if unable to detect
            }
            return false;
        }

        private static ThemeConfiguration CreateLightTheme()
        {
            return new ThemeConfiguration
            {
                FormBackground = Color.White,
                PanelBackground = Color.White,
                ControlBackground = Color.FromArgb(248, 249, 250),
                ControlForeground = Color.FromArgb(33, 37, 41),

                ButtonBackground = Color.FromArgb(248, 249, 250),
                ButtonForeground = Color.FromArgb(33, 37, 41),

                TextBoxBackground = Color.FromArgb(238, 242, 246),
                TextBoxForeground = Color.FromArgb(33, 37, 41),

                ListBoxBackground = Color.FromArgb(238, 242, 246),
                ListBoxForeground = Color.FromArgb(33, 37, 41),

                ComboBoxBackground = Color.FromArgb(238, 242, 246),
                ComboBoxForeground = Color.FromArgb(33, 37, 41),

                LabelForeground = Color.FromArgb(73, 80, 87),
                GroupBoxForeground = Color.FromArgb(73, 80, 87),

                TabControlBackground = Color.White,
                TabPageBackground = Color.White,
                TabPageForeground = Color.FromArgb(33, 37, 41),

                ProgressBarBackground = Color.FromArgb(233, 236, 239),
                BorderColor = Color.FromArgb(206, 212, 218),

                // Enhanced light theme colors
                StatusBarBackground = Color.FromArgb(248, 249, 250),
                StatusForeground = Color.FromArgb(98, 117, 125),
                TitleForeground = Color.FromArgb(33, 37, 41),
                GroupBoxBackground = Color.Transparent,
                CheckBoxForeground = Color.FromArgb(73, 80, 87),
                PrimaryCheckBoxForeground = Color.FromArgb(73, 80, 87),
                SubCheckBoxForeground = Color.FromArgb(73, 80, 87),
                DisabledCheckBoxForeground = Color.FromArgb(173, 181, 189)
            };
        }

        private static ThemeConfiguration CreateDarkTheme()
        {
            return new ThemeConfiguration
            {
                FormBackground = Color.FromArgb(45, 45, 48),
                PanelBackground = Color.FromArgb(45, 45, 48),
                ControlBackground = Color.FromArgb(37, 37, 38),
                ControlForeground = Color.FromArgb(241, 241, 241),

                ButtonBackground = Color.FromArgb(62, 62, 64),
                ButtonForeground = Color.FromArgb(241, 241, 241),

                TextBoxBackground = Color.FromArgb(34, 39, 44),
                TextBoxForeground = Color.FromArgb(241, 241, 241),

                ListBoxBackground = Color.FromArgb(34, 39, 44),
                ListBoxForeground = Color.FromArgb(241, 241, 241),

                ComboBoxBackground = Color.FromArgb(34, 39, 44),
                ComboBoxForeground = Color.FromArgb(241, 241, 241),

                LabelForeground = Color.FromArgb(204, 204, 204),
                GroupBoxForeground = Color.FromArgb(204, 204, 204),

                TabControlBackground = Color.FromArgb(45, 45, 48),
                TabPageBackground = Color.FromArgb(45, 45, 48),
                TabPageForeground = Color.FromArgb(241, 241, 241),

                ProgressBarBackground = Color.FromArgb(28, 28, 30),
                BorderColor = Color.FromArgb(82, 82, 82),

                // Enhanced dark theme colors
                StatusBarBackground = Color.FromArgb(37, 37, 38),
                StatusForeground = Color.FromArgb(204, 204, 204),
                TitleForeground = Color.FromArgb(241, 241, 241),
                GroupBoxBackground = Color.Transparent,
                CheckBoxForeground = Color.FromArgb(241, 241, 241),
                PrimaryCheckBoxForeground = Color.FromArgb(241, 241, 241),
                SubCheckBoxForeground = Color.FromArgb(241, 241, 241),
                DisabledCheckBoxForeground = Color.FromArgb(120, 120, 120)
            };
        }
    }

    /// <summary>
    /// Theme color configuration
    /// </summary>
    public class ThemeConfiguration
    {
        public Color FormBackground { get; set; }
        public Color PanelBackground { get; set; }
        public Color ControlBackground { get; set; }
        public Color ControlForeground { get; set; }

        public Color ButtonBackground { get; set; }
        public Color ButtonForeground { get; set; }

        public Color TextBoxBackground { get; set; }
        public Color TextBoxForeground { get; set; }

        public Color ListBoxBackground { get; set; }
        public Color ListBoxForeground { get; set; }

        public Color ComboBoxBackground { get; set; }
        public Color ComboBoxForeground { get; set; }

        public Color LabelForeground { get; set; }
        public Color GroupBoxForeground { get; set; }

        public Color TabControlBackground { get; set; }
        public Color TabPageBackground { get; set; }
        public Color TabPageForeground { get; set; }

        public Color ProgressBarBackground { get; set; }
        public Color BorderColor { get; set; }

        // Enhanced theme properties
        public Color StatusBarBackground { get; set; }
        public Color StatusForeground { get; set; }
        public Color TitleForeground { get; set; }
        public Color GroupBoxBackground { get; set; }
        public Color CheckBoxForeground { get; set; }
        public Color PrimaryCheckBoxForeground { get; set; }
        public Color SubCheckBoxForeground { get; set; }
        public Color DisabledCheckBoxForeground { get; set; }
    }
}