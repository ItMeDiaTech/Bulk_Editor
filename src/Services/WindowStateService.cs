#nullable enable
using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Bulk_Editor.Configuration;

using Bulk_Editor.Services.Abstractions;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Service for managing window state (position, size)
    /// </summary>
    public class WindowStateService : IWindowStateService
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly UiSettings _uiSettings;
        private readonly string _stateFilePath;

        public WindowStateService(ApplicationSettings applicationSettings, UiSettings uiSettings)
        {
            _applicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
            _uiSettings = uiSettings ?? throw new ArgumentNullException(nameof(uiSettings));
            _stateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window_state.json");
        }

        /// <summary>
        /// Save the current window state
        /// </summary>
        public void SaveWindowState(Form form)
        {
            if (form == null) return;

            try
            {
                var state = new WindowState
                {
                    Location = form.Location,
                    Size = form.Size,
                    FormState = form.WindowState,
                    LastSaved = DateTime.Now
                };

                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_stateFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving window state: {ex.Message}");
            }
        }

        /// <summary>
        /// Restore window state from saved settings
        /// </summary>
        public void RestoreWindowState(Form form)
        {
            if (form == null) return;

            try
            {
                // Check if settings allow window state restoration
                if (!_applicationSettings.RememberWindowPosition && !_uiSettings.RememberWindowSize)
                    return;

                if (!File.Exists(_stateFilePath))
                    return;

                var json = File.ReadAllText(_stateFilePath);
                var state = JsonSerializer.Deserialize<WindowState>(json);

                if (state == null) return;

                // Validate that the saved position is still valid (monitor might have been disconnected)
                if (_applicationSettings.RememberWindowPosition && IsLocationValid(state.Location))
                {
                    form.StartPosition = FormStartPosition.Manual;
                    form.Location = state.Location;
                }

                // Restore window size if enabled
                if (_uiSettings.RememberWindowSize && IsSizeValid(state.Size, form))
                {
                    form.Size = state.Size;
                }

                // Restore window state (normal, maximized, minimized)
                if (state.FormState == FormWindowState.Maximized)
                {
                    form.WindowState = FormWindowState.Maximized;
                }
                else if (state.FormState == FormWindowState.Normal)
                {
                    form.WindowState = FormWindowState.Normal;
                }
                // Don't restore minimized state as it's confusing for users
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring window state: {ex.Message}");
                // Fall back to default positioning
                form.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        /// <summary>
        /// Check if the saved location is still valid (within screen bounds)
        /// </summary>
        private static bool IsLocationValid(Point location)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Contains(location))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the saved size is valid
        /// </summary>
        private static bool IsSizeValid(Size size, Form form)
        {
            // Check against form's minimum and maximum size constraints
            if (size.Width < form.MinimumSize.Width || size.Height < form.MinimumSize.Height)
                return false;

            if (form.MaximumSize != Size.Empty)
            {
                if (size.Width > form.MaximumSize.Width || size.Height > form.MaximumSize.Height)
                    return false;
            }

            // Check against reasonable limits (not smaller than 300x200, not larger than screen)
            if (size.Width < 300 || size.Height < 200)
                return false;

            var primaryScreen = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
            if (size.Width > primaryScreen.Width || size.Height > primaryScreen.Height)
                return false;

            return true;
        }

        /// <summary>
        /// Setup form event handlers for automatic state saving
        /// </summary>
        public void SetupAutoSave(Form form)
        {
            if (form == null) return;

            // Save state when form is moved or resized
            form.LocationChanged += (s, e) =>
            {
                if (_applicationSettings.RememberWindowPosition && form.WindowState == FormWindowState.Normal)
                {
                    SaveWindowState(form);
                }
            };

            form.SizeChanged += (s, e) =>
            {
                if (_uiSettings.RememberWindowSize && form.WindowState == FormWindowState.Normal)
                {
                    SaveWindowState(form);
                }
            };

            // Save state when form is closing
            form.FormClosing += (s, e) =>
            {
                if (_applicationSettings.RememberWindowPosition || _uiSettings.RememberWindowSize)
                {
                    SaveWindowState(form);
                }
            };
        }

        /// <summary>
        /// Clear saved window state
        /// </summary>
        public void ClearSavedState()
        {
            try
            {
                if (File.Exists(_stateFilePath))
                {
                    File.Delete(_stateFilePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing window state: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Window state data structure
    /// </summary>
    public class WindowState
    {
        public Point Location { get; set; }
        public Size Size { get; set; }
        public FormWindowState FormState { get; set; }
        public DateTime LastSaved { get; set; }
    }
}