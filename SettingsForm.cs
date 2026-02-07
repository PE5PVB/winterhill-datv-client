using System;
using System.Windows.Forms;

namespace datvreceiver
{
    /// <summary>
    /// Settings form for configuring Winterhill connection, offsets, and display options.
    /// Also displays debug log messages from the application.
    /// </summary>
    public partial class SettingsForm : Form
    {
        #region Events

        /// <summary>Raised when auto-zoom checkbox changes</summary>
        public event Action<bool> OnAutoZoomChange;

        /// <summary>Raised when user saves settings, requiring reconnection</summary>
        public event Action OnSettingsChanged;

        #endregion

        #region Private Fields

        private IniSettings iniSettings;
        private bool allowClose = false;

        #endregion

        #region Constructor and Lifecycle

        public SettingsForm()
        {
            InitializeComponent();
            this.FormClosing += SettingsForm_FormClosing;
            ThemeHelper.ApplyTheme(this);
        }

        /// <summary>
        /// Intercepts close to hide instead, keeping form alive for reuse.
        /// </summary>
        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allowClose)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        /// <summary>
        /// Allows form to actually close (called on application exit).
        /// </summary>
        public void ForceClose()
        {
            allowClose = true;
            this.Close();
        }

        /// <summary>
        /// Sets the IniSettings instance for saving configuration.
        /// </summary>
        public void SetIniSettings(IniSettings settings)
        {
            iniSettings = settings;
        }

        #endregion

        #region Display Setting Handlers

        /// <summary>
        /// Handles auto-zoom checkbox change. Takes effect immediately.
        /// </summary>
        private void checkAutoZoom_CheckedChanged(object sender, EventArgs e)
        {
            bool autoZoom = checkAutoZoom.Checked;

            if (iniSettings != null)
            {
                iniSettings.AutoZoom = autoZoom;
                iniSettings.Save();
            }

            OnAutoZoomChange?.Invoke(autoZoom);
        }

        /// <summary>
        /// Handles hardware decoding checkbox change. Requires restart.
        /// </summary>
        private void checkHardwareDecoding_CheckedChanged(object sender, EventArgs e)
        {
            bool hwDecoding = checkHardwareDecoding.Checked;

            if (iniSettings != null)
            {
                iniSettings.HardwareDecoding = hwDecoding;
                iniSettings.Save();
            }
            // Note: Requires restart to take effect (VLC is already initialized)
        }

        /// <summary>
        /// Handles compatibility mode checkbox change. Requires restart.
        /// </summary>
        private void checkCompatibilityMode_CheckedChanged(object sender, EventArgs e)
        {
            bool compatMode = checkCompatibilityMode.Checked;

            if (iniSettings != null)
            {
                iniSettings.CompatibilityMode = compatMode;
                iniSettings.Save();
            }
            // Note: Requires restart to take effect (VLC is already initialized)
        }

        /// <summary>
        /// Handles video caching slider change. Requires restart.
        /// </summary>
        private void trackBarVideoCaching_Scroll(object sender, EventArgs e)
        {
            int caching = trackBarVideoCaching.Value;
            labelVideoCachingValue.Text = caching.ToString() + " ms";

            if (iniSettings != null)
            {
                iniSettings.VideoCaching = caching;
                iniSettings.Save();
            }
            // Note: Requires restart to take effect (VLC is already initialized)
        }

        #endregion

        #region Debug Logging

        /// <summary>
        /// Adds a message to the debug log listbox. Thread-safe.
        /// </summary>
        public void AddDebugMessage(string msg)
        {
            if (dbgListBox == null || dbgListBox.IsDisposed || this.IsDisposed)
                return;

            try
            {
                if (dbgListBox.InvokeRequired)
                {
                    dbgListBox.BeginInvoke(new Action(() => AddDebugMessageInternal(msg)));
                }
                else
                {
                    AddDebugMessageInternal(msg);
                }
            }
            catch (ObjectDisposedException)
            {
                // Form or control was disposed, ignore
            }
        }

        /// <summary>
        /// Internal method to add debug message (must be called on UI thread).
        /// Limits log to 1000 entries.
        /// </summary>
        private void AddDebugMessageInternal(string msg)
        {
            if (dbgListBox == null || dbgListBox.IsDisposed)
                return;

            try
            {
                dbgListBox.BeginUpdate();

                // Keep only last 1000 messages
                while (dbgListBox.Items.Count > 1000)
                {
                    dbgListBox.Items.RemoveAt(0);
                }

                // Add timestamped message and scroll to bottom
                int i = dbgListBox.Items.Add(DateTime.Now.ToShortTimeString() + " : " + msg);
                dbgListBox.TopIndex = i;
                dbgListBox.EndUpdate();
            }
            catch (Exception)
            {
                // Ignore errors during debug message display
            }
        }

        #endregion

        #region Save Button Handler

        /// <summary>
        /// Saves all settings to INI file and notifies main form.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (iniSettings != null)
            {
                // Save connection settings
                iniSettings.WinterhillHost = txtWinterhillHost.Text;
                iniSettings.WinterhillBasePort = Int32.Parse(txtWinterhillBasePort.Text);
                iniSettings.ForceLocalIP = checkForceLocalIP.Checked;
                iniSettings.ForceLocalIPAddress = txtLocalIP.Text;

                // Save IF offsets
                iniSettings.Rx1Offset = int.Parse(txtRX1Offset.Text);
                iniSettings.Rx2Offset = int.Parse(txtRX2Offset.Text);
                iniSettings.Rx3Offset = int.Parse(txtRX3Offset.Text);
                iniSettings.Rx4Offset = int.Parse(txtRX4Offset.Text);

                iniSettings.Save();
            }

            // Notify main form to apply settings and reconnect if needed
            OnSettingsChanged?.Invoke();

            MessageBox.Show("Settings saved and applied.");
        }

        #endregion

        #region About Button Handler

        /// <summary>
        /// Shows application about dialog with version and credits.
        /// </summary>
        private void btnAbout_Click(object sender, EventArgs e)
        {
            // Get build date from assembly file timestamp
            var buildDate = System.IO.File.GetLastWriteTime(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            string version = buildDate.ToString("yyyy-MM-dd");

            string aboutText = "Winterhill Client for Repeaters\n\n" +
                "Version: " + version + "\n\n" +
                "You use this software at your own risk.\n\n" +
                "Original software design by Tom van den Bon - ZR6TG\n" +
                "Modified for repeater use by Sjef Verhoeven - PE5PVB";

            MessageBox.Show(aboutText, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads settings into form controls for display.
        /// </summary>
        public void LoadSettings(string winterhillHost, int winterhillBaseport, bool forceLocalIP,
            string localIP, int rx1Offset, int rx2Offset, int rx3Offset, int rx4Offset,
            bool autoZoom, bool hardwareDecoding, bool compatibilityMode, int videoCaching)
        {
            // Connection settings
            txtWinterhillHost.Text = winterhillHost;
            txtWinterhillBasePort.Text = winterhillBaseport.ToString();
            checkForceLocalIP.Checked = forceLocalIP;
            txtLocalIP.Text = localIP;
            labLocalIP.Text = localIP;

            // IF offsets
            txtRX1Offset.Text = rx1Offset.ToString();
            txtRX2Offset.Text = rx2Offset.ToString();
            txtRX3Offset.Text = rx3Offset.ToString();
            txtRX4Offset.Text = rx4Offset.ToString();

            // Video settings
            trackBarVideoCaching.Value = Math.Min(500, Math.Max(50, videoCaching));
            labelVideoCachingValue.Text = videoCaching.ToString() + " ms";
            checkAutoZoom.Checked = autoZoom;
            checkHardwareDecoding.Checked = hardwareDecoding;
            checkCompatibilityMode.Checked = compatibilityMode;
        }

        #endregion
    }
}
