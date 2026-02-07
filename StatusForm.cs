using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace datvreceiver
{
    /// <summary>
    /// Status form displaying detailed information for all 4 receivers.
    /// Shows signal status, service info, frequency, symbol rate, MER, and volume controls.
    /// Supports symbol rate hopping configuration and force TS assignment.
    /// </summary>
    public partial class StatusForm : Form
    {
        #region Events

        /// <summary>Raised when user requests force TS assignment. Positive = assign, negative = reset.</summary>
        public event Action<int> OnForceTS;

        /// <summary>Raised when user changes volume slider. Parameters: rx number, volume 0-100.</summary>
        public event Action<int, int> OnVolumeChange;

        /// <summary>Raised when user changes symbol rate checkboxes. Parameters: rx number, enabled rates array.</summary>
        public event Action<int, int[]> OnSymbolRatesChange;

        /// <summary>Raised when user clicks a frequency label to change it.</summary>
        public event Action<int> OnFrequencyClick;

        /// <summary>Raised when user clicks the settings button.</summary>
        public event Action OnOpenSettings;

        #endregion

        #region Private Fields - Lookup Tables

        private Dictionary<int, string> scanstateLookup;

        #endregion

        #region Private Fields - Control Arrays

        // Arrays to hold controls for each receiver (indexed 0-3)
        private Label[] scanstateLabels;
        private Label[] serviceLabels;
        private Label[] providerLabels;
        private Label[] merLabels;
        private Label[] dbmarginLabels;
        private Label[] frequencyLabels;
        private Label[] srLabels;
        private Label[] modeLabels;
        private Label[] nullLabels;
        private Label[] mediatypesLabels;
        private Label[] tsaddrLabels;
        private Label[] nothingLabels;
        private Label[] volLabels;
        private TrackBar[] volumeTrackbars;
        private CheckBox[][] srCheckboxes;  // [rx][sr_index]

        #endregion

        #region Private Fields - State Tracking

        /// <summary>Previous scan states for detecting state changes (indexed 0-3)</summary>
        private int[] prevStates = new int[] { -1, -1, -1, -1 };

        /// <summary>Frozen frequency values during SR hopping</summary>
        private string[] frozenFrequency = new string[4];

        /// <summary>Frozen symbol rate values during SR hopping</summary>
        private string[] frozenSymbolRate = new string[4];

        /// <summary>Whether display is frozen during SR hopping</summary>
        private bool[] displayFrozen = new bool[4];

        /// <summary>Last known good frequency values (used when current value is empty)</summary>
        private string[] lastGoodFrequency = new string[] { "", "", "", "" };

        /// <summary>Last known good symbol rate values (used when current value is empty)</summary>
        private string[] lastGoodSymbolRate = new string[] { "", "", "", "" };

        /// <summary>Allow form to actually close (vs. minimize)</summary>
        private bool allowClose = false;

        #endregion

        #region Constructor and Initialization

        public StatusForm()
        {
            InitializeComponent();
            InitializeControlArrays();
            this.FormClosing += StatusForm_FormClosing;
            ThemeHelper.ApplyTheme(this);
            ApplyCustomStyling();
        }

        /// <summary>
        /// Applies additional custom styling beyond the base theme.
        /// </summary>
        private void ApplyCustomStyling()
        {
            // Style header labels with blue accent
            foreach (Control c in this.Controls)
            {
                if (c is Label lbl && lbl.Font.Bold)
                {
                    lbl.ForeColor = ThemeHelper.AccentBlue;
                }
            }

            // Style clickable frequency labels as links
            foreach (var lbl in frequencyLabels)
            {
                lbl.ForeColor = ThemeHelper.AccentBlue;
            }

            // Style clickable TS address labels as links
            foreach (var lbl in tsaddrLabels)
            {
                lbl.ForeColor = ThemeHelper.AccentBlue;
            }
        }

        /// <summary>
        /// Intercepts close to minimize instead, keeping the form alive.
        /// </summary>
        private void StatusForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allowClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
        }

        /// <summary>
        /// Allows the form to actually close (called on application exit).
        /// </summary>
        public void ForceClose()
        {
            allowClose = true;
            this.Close();
        }

        /// <summary>
        /// Shows and brings the status form to front.
        /// </summary>
        public void ShowStatusForm()
        {
            this.WindowState = FormWindowState.Normal;
            this.Show();
            this.BringToFront();
        }

        /// <summary>
        /// Initializes control arrays for easy indexed access to receiver-specific controls.
        /// </summary>
        private void InitializeControlArrays()
        {
            scanstateLabels = new Label[] { lab_rx1_scanstate, lab_rx2_scanstate, lab_rx3_scanstate, lab_rx4_scanstate };
            serviceLabels = new Label[] { lab_rx1_service, lab_rx2_service, lab_rx3_service, lab_rx4_service };
            providerLabels = new Label[] { lab_rx1_provider, lab_rx2_provider, lab_rx3_provider, lab_rx4_provider };
            merLabels = new Label[] { lab_rx1_mer, lab_rx2_mer, lab_rx3_mer, lab_rx4_mer };
            dbmarginLabels = new Label[] { lab_rx1_dbmargin, lab_rx2_dbmargin, lab_rx3_dbmargin, lab_rx4_dbmargin };
            frequencyLabels = new Label[] { lab_rx1_frequency, lab_rx2_frequency, lab_rx3_frequency, lab_rx4_frequency };
            srLabels = new Label[] { lab_rx1_sr, lab_rx2_sr, lab_rx3_sr, lab_rx4_sr };
            modeLabels = new Label[] { lab_rx1_mode, lab_rx2_mode, lab_rx3_mode, lab_rx4_mode };
            nullLabels = new Label[] { lab_rx1_null, lab_rx2_null, lab_rx3_null, lab_rx4_null };
            mediatypesLabels = new Label[] { lab_rx1_mediatypes, lab_rx2_mediatypes, lab_rx3_mediatypes, lab_rx4_mediatypes };
            tsaddrLabels = new Label[] { lab_rx1_tsaddr, lab_rx2_tsaddr, lab_rx3_tsaddr, lab_rx4_tsaddr };
            nothingLabels = new Label[] { lab_rx1_nothing, lab_rx2_nothing, lab_rx3_nothing, lab_rx4_nothing };
            volLabels = new Label[] { lab_rx1_vol, lab_rx2_vol, lab_rx3_vol, lab_rx4_vol };
            volumeTrackbars = new TrackBar[] { trackBar1, trackBar2, trackBar3, trackBar4 };

            // Symbol rate checkboxes: [rx][sr_index] where sr_index is 0=66, 1=125, 2=250, 3=333, 4=500, 5=1000, 6=2000
            srCheckboxes = new CheckBox[][] {
                new CheckBox[] { chkRx1SR66, chkRx1SR125, chkRx1SR250, chkRx1SR333, chkRx1SR500, chkRx1SR1000, chkRx1SR2000 },
                new CheckBox[] { chkRx2SR66, chkRx2SR125, chkRx2SR250, chkRx2SR333, chkRx2SR500, chkRx2SR1000, chkRx2SR2000 },
                new CheckBox[] { chkRx3SR66, chkRx3SR125, chkRx3SR250, chkRx3SR333, chkRx3SR500, chkRx3SR1000, chkRx3SR2000 },
                new CheckBox[] { chkRx4SR66, chkRx4SR125, chkRx4SR250, chkRx4SR333, chkRx4SR500, chkRx4SR1000, chkRx4SR2000 }
            };
        }

        #endregion

        #region Public Configuration Methods

        /// <summary>
        /// Sets the scan state lookup table for displaying human-readable state names.
        /// </summary>
        public void SetScanStateLookup(Dictionary<int, string> lookup)
        {
            scanstateLookup = lookup;
        }

        /// <summary>
        /// Initializes volume sliders with saved values.
        /// </summary>
        public void SetInitialVolumes(int vol1, int vol2, int vol3, int vol4)
        {
            trackBar1.Value = vol1;
            trackBar2.Value = vol2;
            trackBar3.Value = vol3;
            trackBar4.Value = vol4;
            lab_rx1_vol.Text = vol1.ToString() + " %";
            lab_rx2_vol.Text = vol2.ToString() + " %";
            lab_rx3_vol.Text = vol3.ToString() + " %";
            lab_rx4_vol.Text = vol4.ToString() + " %";
        }

        #endregion

        #region Status Update Methods

        /// <summary>
        /// Updates display for a single receiver with latest status from Winterhill.
        /// Handles state changes, color updates, and display freezing during SR hopping.
        /// </summary>
        /// <param name="rx">Receiver number (1-4)</param>
        /// <param name="msg">Receiver status message from Winterhill</param>
        /// <param name="localip">Local IP address to compare with TS destination</param>
        public void UpdateReceiverStatus(int rx, ReceiverMessage msg, string localip)
        {
            if (rx < 1 || rx > 4) return;
            int idx = rx - 1;

            int newState = msg.scanstate;
            bool stateChanged = newState != prevStates[idx];

            if (stateChanged)
            {
                if (newState == 2 || newState == 3)
                {
                    UpdateLabelColor(scanstateLabels[idx], ThemeHelper.AccentGreen);
                    UpdateLabelVisibility(nothingLabels[idx], false);
                }
                else
                {
                    UpdateLabelColor(scanstateLabels[idx], ThemeHelper.AccentRed);
                    UpdateLabelVisibility(nothingLabels[idx], true);
                }
                prevStates[idx] = newState;
            }

            string scanstateText = scanstateLookup != null && scanstateLookup.ContainsKey(msg.scanstate)
                ? scanstateLookup[msg.scanstate]
                : msg.scanstate.ToString();

            UpdateLabel(scanstateLabels[idx], scanstateText);

            // Only show service info when locked (scanstate 2 or 3)
            bool isLocked = newState == 2 || newState == 3;
            UpdateLabel(serviceLabels[idx], isLocked ? (msg.service_name ?? "") : "");
            UpdateLabel(providerLabels[idx], isLocked ? (msg.service_provider_name ?? "") : "");
            UpdateLabel(merLabels[idx], isLocked ? (msg.mer ?? "") : "");
            UpdateLabel(dbmarginLabels[idx], isLocked ? (msg.dbmargin ?? "") : "");

            // Get current values from message
            string currentFreq = msg.frequency ?? "";
            string currentSR = msg.symbol_rate ?? "";

            // Update last known good values if we have valid data
            if (!string.IsNullOrEmpty(currentFreq) && currentFreq != "0")
                lastGoodFrequency[idx] = currentFreq;
            if (!string.IsNullOrEmpty(currentSR) && currentSR != "0")
                lastGoodSymbolRate[idx] = currentSR;

            // Determine what to display:
            // - If frozen, use frozen values
            // - If current is empty/zero, use last known good
            // - Otherwise use current
            string displayFreq;
            string displaySR;

            if (displayFrozen[idx])
            {
                displayFreq = frozenFrequency[idx];
                displaySR = frozenSymbolRate[idx];
            }
            else
            {
                displayFreq = (!string.IsNullOrEmpty(currentFreq) && currentFreq != "0") ? currentFreq : lastGoodFrequency[idx];
                displaySR = (!string.IsNullOrEmpty(currentSR) && currentSR != "0") ? currentSR : lastGoodSymbolRate[idx];
            }

            UpdateLabel(frequencyLabels[idx], displayFreq);
            UpdateLabel(srLabels[idx], displaySR);

            // Only show frequency/SR info if we have valid values
            if (!string.IsNullOrEmpty(displayFreq) && !string.IsNullOrEmpty(displaySR))
            {
                UpdateLabel(nothingLabels[idx], displayFreq + " MHz - " + displaySR + " Ks");
            }
            else if (!string.IsNullOrEmpty(displayFreq))
            {
                UpdateLabel(nothingLabels[idx], displayFreq + " MHz");
            }
            else
            {
                UpdateLabel(nothingLabels[idx], "No signal");
            }

            UpdateLabel(modeLabels[idx], isLocked ? (msg.modcod ?? "") : "");
            UpdateLabel(nullLabels[idx], isLocked ? (msg.null_percentage ?? "") : "");
            UpdateLabel(mediatypesLabels[idx], isLocked ? ((msg.video_type ?? "") + "-" + (msg.audio_type ?? "")) : "");
            UpdateLabel(tsaddrLabels[idx], msg.ts_addr ?? "");

            if (msg.ts_addr != localip)
            {
                UpdateLabelColor(tsaddrLabels[idx], ThemeHelper.AccentRed);
            }
            else
            {
                UpdateLabelColor(tsaddrLabels[idx], ThemeHelper.AccentGreen);
            }
        }

        /// <summary>
        /// Resets previous state for a receiver to force UI update on next message.
        /// </summary>
        public void ResetPrevState(int rx)
        {
            if (rx >= 1 && rx <= 4)
            {
                prevStates[rx - 1] = -1;
            }
        }

        /// <summary>
        /// Checks if a receiver is currently locked (DVB-S or DVB-S2).
        /// </summary>
        public bool IsLocked(int rx)
        {
            if (rx < 1 || rx > 4) return false;
            int state = prevStates[rx - 1];
            return state == 2 || state == 3;
        }

        /// <summary>
        /// Updates status bar text at bottom of form.
        /// </summary>
        public void UpdateStatusText(string text)
        {
            if (lblStatus.Owner != null && lblStatus.Owner.InvokeRequired)
            {
                lblStatus.Owner.Invoke(new Action(() => lblStatus.Text = text));
            }
            else
            {
                lblStatus.Text = text;
            }
        }

        #endregion

        #region Thread-Safe UI Helpers

        /// <summary>
        /// Thread-safe label text update.
        /// </summary>
        private void UpdateLabel(Label lb, string text)
        {
            if (lb.InvokeRequired)
            {
                lb.Invoke(new Action(() => lb.Text = text));
            }
            else
            {
                lb.Text = text;
            }
        }

        /// <summary>
        /// Thread-safe label color update.
        /// </summary>
        private void UpdateLabelColor(Label lb, Color col)
        {
            if (lb.InvokeRequired)
            {
                lb.Invoke(new Action(() => lb.ForeColor = col));
            }
            else
            {
                lb.ForeColor = col;
            }
        }

        /// <summary>
        /// Thread-safe label visibility update.
        /// </summary>
        private void UpdateLabelVisibility(Label lb, bool visible)
        {
            if (lb.InvokeRequired)
            {
                lb.Invoke(new Action(() => lb.Visible = visible));
            }
            else
            {
                lb.Visible = visible;
            }
        }

        #endregion

        #region Volume Control Event Handlers

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            int vol = trackBar1.Value;
            lab_rx1_vol.Text = vol.ToString() + " %";
            OnVolumeChange?.Invoke(1, vol);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            int vol = trackBar2.Value;
            lab_rx2_vol.Text = vol.ToString() + " %";
            OnVolumeChange?.Invoke(2, vol);
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            int vol = trackBar3.Value;
            lab_rx3_vol.Text = vol.ToString() + " %";
            OnVolumeChange?.Invoke(3, vol);
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            int vol = trackBar4.Value;
            lab_rx4_vol.Text = vol.ToString() + " %";
            OnVolumeChange?.Invoke(4, vol);
        }

        #endregion

        #region Force TS Button Handlers

        private void butForceTSRx1_Click(object sender, EventArgs e) => OnForceTS?.Invoke(1);
        private void butForceTSRx2_Click(object sender, EventArgs e) => OnForceTS?.Invoke(2);
        private void butForceTSRx3_Click(object sender, EventArgs e) => OnForceTS?.Invoke(3);
        private void butForceTSRx4_Click(object sender, EventArgs e) => OnForceTS?.Invoke(4);

        // Click on TS address label to reset (send negative rx number)
        private void lab_rx1_tsaddr_Click(object sender, EventArgs e) => OnForceTS?.Invoke(-1);
        private void lab_rx2_tsaddr_Click(object sender, EventArgs e) => OnForceTS?.Invoke(-2);
        private void lab_rx3_tsaddr_Click(object sender, EventArgs e) => OnForceTS?.Invoke(-3);
        private void lab_rx4_tsaddr_Click(object sender, EventArgs e) => OnForceTS?.Invoke(-4);

        #endregion

        #region Frequency Click Handlers

        private void lab_rx1_frequency_Click(object sender, EventArgs e) => OnFrequencyClick?.Invoke(1);
        private void lab_rx2_frequency_Click(object sender, EventArgs e) => OnFrequencyClick?.Invoke(2);
        private void lab_rx3_frequency_Click(object sender, EventArgs e) => OnFrequencyClick?.Invoke(3);
        private void lab_rx4_frequency_Click(object sender, EventArgs e) => OnFrequencyClick?.Invoke(4);

        #endregion

        #region Settings Button Handler

        private void btnSettings_Click(object sender, EventArgs e)
        {
            OnOpenSettings?.Invoke();
        }

        #endregion

        #region Public Getters

        /// <summary>
        /// Gets the service name for a receiver.
        /// </summary>
        public string GetServiceName(int rx)
        {
            if (rx < 1 || rx > 4) return "";
            return serviceLabels[rx - 1].Text;
        }

        /// <summary>
        /// Gets the current frequency for a receiver.
        /// </summary>
        public string GetFrequency(int rx)
        {
            if (rx < 1 || rx > 4) return "";
            return frequencyLabels[rx - 1].Text;
        }

        /// <summary>
        /// Gets the current symbol rate for a receiver.
        /// </summary>
        public string GetSymbolRate(int rx)
        {
            if (rx < 1 || rx > 4) return "";
            return srLabels[rx - 1].Text;
        }

        #endregion

        #region Symbol Rate Hopping

        /// <summary>
        /// Handles symbol rate checkbox changes, notifying main form.
        /// </summary>
        private void SRCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk == null || chk.Tag == null) return;

            string[] parts = chk.Tag.ToString().Split(',');
            if (parts.Length != 2) return;

            int rx = int.Parse(parts[0]);
            int[] enabledRates = GetEnabledSymbolRates(rx);

            // Notify about change
            OnSymbolRatesChange?.Invoke(rx, enabledRates);
        }

        /// <summary>
        /// Gets array of enabled symbol rates for a receiver.
        /// </summary>
        public int[] GetEnabledSymbolRates(int rx)
        {
            if (rx < 1 || rx > 4) return new int[0];

            var rates = new List<int>();
            int[] srValues = { 66, 125, 250, 333, 500, 1000, 2000 };

            for (int i = 0; i < srCheckboxes[rx - 1].Length; i++)
            {
                if (srCheckboxes[rx - 1][i].Checked)
                    rates.Add(srValues[i]);
            }

            return rates.ToArray();
        }

        /// <summary>
        /// Loads symbol rate checkbox states from comma-separated string.
        /// Temporarily unhooks event handlers to prevent cascade.
        /// </summary>
        public void LoadSymbolRateCheckboxes(int rx, string symbolRatesStr)
        {
            if (rx < 1 || rx > 4) return;

            int[] srValues = { 66, 125, 250, 333, 500, 1000, 2000 };

            // Parse the comma-separated string into a set
            var enabledRates = new HashSet<int>();
            if (!string.IsNullOrEmpty(symbolRatesStr))
            {
                foreach (var rate in symbolRatesStr.Split(','))
                {
                    if (int.TryParse(rate.Trim(), out int r))
                        enabledRates.Add(r);
                }
            }

            // Set checkbox states without triggering events
            for (int i = 0; i < srCheckboxes[rx - 1].Length; i++)
            {
                srCheckboxes[rx - 1][i].CheckedChanged -= SRCheckbox_CheckedChanged;
                srCheckboxes[rx - 1][i].Checked = enabledRates.Contains(srValues[i]);
                srCheckboxes[rx - 1][i].CheckedChanged += SRCheckbox_CheckedChanged;
            }
        }

        /// <summary>
        /// Gets enabled symbol rates as comma-separated string for saving.
        /// </summary>
        public string GetSymbolRatesString(int rx)
        {
            if (rx < 1 || rx > 4) return "";

            var rates = new List<string>();
            int[] srValues = { 66, 125, 250, 333, 500, 1000, 2000 };

            for (int i = 0; i < srCheckboxes[rx - 1].Length; i++)
            {
                if (srCheckboxes[rx - 1][i].Checked)
                    rates.Add(srValues[i].ToString());
            }

            return string.Join(",", rates);
        }

        #endregion

        #region Display Freeze (for SR Hopping)

        /// <summary>
        /// Freezes display for a receiver during symbol rate hopping.
        /// Uses last known good values to prevent flickering.
        /// </summary>
        public void FreezeDisplay(int rx)
        {
            if (rx < 1 || rx > 4) return;
            int idx = rx - 1;
            frozenFrequency[idx] = lastGoodFrequency[idx];
            frozenSymbolRate[idx] = lastGoodSymbolRate[idx];
            displayFrozen[idx] = true;
        }

        /// <summary>
        /// Unfreezes display for a receiver after lock is achieved.
        /// </summary>
        public void UnfreezeDisplay(int rx)
        {
            if (rx < 1 || rx > 4) return;
            displayFrozen[rx - 1] = false;
        }

        #endregion
    }
}
