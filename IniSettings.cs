using System;
using System.IO;
using System.Collections.Generic;

namespace datvreceiver
{
    /// <summary>
    /// Handles loading and saving of application settings to an INI file.
    /// Settings are stored in %LocalAppData%\datvreceiver\datvreceiver.ini
    /// Automatically migrates settings from old location (application directory) if needed.
    /// </summary>
    public class IniSettings
    {
        #region Private Fields

        private string filePath;
        private Dictionary<string, Dictionary<string, string>> sections;

        #endregion

        #region Winterhill Connection Settings

        /// <summary>Hostname or IP address of the Winterhill receiver</summary>
        public string WinterhillHost { get; set; } = "";

        /// <summary>Base port for Winterhill communication (default 9900)</summary>
        public int WinterhillBasePort { get; set; } = 9900;

        #endregion

        #region Network Settings

        /// <summary>Override automatic local IP detection</summary>
        public bool ForceLocalIP { get; set; } = false;

        /// <summary>Manually specified local IP address when ForceLocalIP is true</summary>
        public string ForceLocalIPAddress { get; set; } = "";

        #endregion

        #region Audio Settings

        /// <summary>Volume level for receiver 1 (0-100)</summary>
        public int Rx1Volume { get; set; } = 100;

        /// <summary>Volume level for receiver 2 (0-100)</summary>
        public int Rx2Volume { get; set; } = 100;

        /// <summary>Volume level for receiver 3 (0-100)</summary>
        public int Rx3Volume { get; set; } = 100;

        /// <summary>Volume level for receiver 4 (0-100)</summary>
        public int Rx4Volume { get; set; } = 100;

        #endregion

        #region Video Settings

        /// <summary>Enable GPU hardware decoding (requires restart)</summary>
        public bool HardwareDecoding { get; set; } = false;

        /// <summary>Use conservative VLC settings for older hardware (requires restart)</summary>
        public bool CompatibilityMode { get; set; } = false;

        /// <summary>VLC network caching in milliseconds (50-500, requires restart)</summary>
        public int VideoCaching { get; set; } = 100;

        /// <summary>Automatically zoom to single receiver when only one is locked</summary>
        public bool AutoZoom { get; set; } = false;

        #endregion

        #region IF Offset Settings

        /// <summary>LNB IF offset for receiver 1 in kHz</summary>
        public int Rx1Offset { get; set; } = 0;

        /// <summary>LNB IF offset for receiver 2 in kHz</summary>
        public int Rx2Offset { get; set; } = 0;

        /// <summary>LNB IF offset for receiver 3 in kHz</summary>
        public int Rx3Offset { get; set; } = 0;

        /// <summary>LNB IF offset for receiver 4 in kHz</summary>
        public int Rx4Offset { get; set; } = 0;

        #endregion

        #region Symbol Rate Hopping Settings

        /// <summary>Comma-separated list of enabled symbol rates for RX1 (e.g., "333,500,1000")</summary>
        public string Rx1SymbolRates { get; set; } = "";

        /// <summary>Comma-separated list of enabled symbol rates for RX2</summary>
        public string Rx2SymbolRates { get; set; } = "";

        /// <summary>Comma-separated list of enabled symbol rates for RX3</summary>
        public string Rx3SymbolRates { get; set; } = "";

        /// <summary>Comma-separated list of enabled symbol rates for RX4</summary>
        public string Rx4SymbolRates { get; set; } = "";

        #endregion

        #region Frequency Settings

        /// <summary>Last used frequency for RX1 in kHz</summary>
        public int Rx1Frequency { get; set; } = 0;

        /// <summary>Last used frequency for RX2 in kHz</summary>
        public int Rx2Frequency { get; set; } = 0;

        /// <summary>Last used frequency for RX3 in kHz</summary>
        public int Rx3Frequency { get; set; } = 0;

        /// <summary>Last used frequency for RX4 in kHz</summary>
        public int Rx4Frequency { get; set; } = 0;

        #endregion

        #region Window State

        /// <summary>Window width in pixels (0 = use default)</summary>
        public int WindowWidth { get; set; } = 0;

        /// <summary>Window height in pixels (0 = use default)</summary>
        public int WindowHeight { get; set; } = 0;

        /// <summary>Window X position (-1 = center on screen)</summary>
        public int WindowX { get; set; } = -1;

        /// <summary>Window Y position (-1 = center on screen)</summary>
        public int WindowY { get; set; } = -1;

        /// <summary>Whether window was maximized on last exit</summary>
        public bool WindowMaximized { get; set; } = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes settings with default file path in LocalAppData.
        /// Automatically migrates settings from old location if present.
        /// </summary>
        public IniSettings()
        {
            // Use LocalAppData for user settings (roaming-safe)
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "datvreceiver");

            // Create directory if it doesn't exist
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            filePath = Path.Combine(appDataPath, "datvreceiver.ini");

            // Migrate from old location (application directory) if needed
            string oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datvreceiver.ini");
            if (!File.Exists(filePath) && File.Exists(oldPath))
            {
                try
                {
                    File.Copy(oldPath, filePath);
                }
                catch { }
            }

            sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes settings with a custom file path.
        /// </summary>
        /// <param name="path">Full path to the INI file</param>
        public IniSettings(string path) : this()
        {
            filePath = path;
        }

        #endregion

        #region Load/Save Methods

        /// <summary>
        /// Loads settings from the INI file into properties.
        /// Creates a default file if none exists.
        /// </summary>
        public void Load()
        {
            sections.Clear();

            if (!File.Exists(filePath))
            {
                // Create default file
                Save();
                return;
            }

            try
            {
                string currentSection = "";
                foreach (string line in File.ReadAllLines(filePath))
                {
                    string trimmed = line.Trim();

                    // Skip empty lines and comments
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";") || trimmed.StartsWith("#"))
                        continue;

                    // Section header
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        currentSection = trimmed.Substring(1, trimmed.Length - 2);
                        if (!sections.ContainsKey(currentSection))
                            sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        continue;
                    }

                    // Key=Value
                    int equalsPos = trimmed.IndexOf('=');
                    if (equalsPos > 0)
                    {
                        string key = trimmed.Substring(0, equalsPos).Trim();
                        string value = trimmed.Substring(equalsPos + 1).Trim();

                        if (!sections.ContainsKey(currentSection))
                            sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        sections[currentSection][key] = value;
                    }
                }

                // Map to properties
                WinterhillHost = GetString("Winterhill", "Host", "");
                WinterhillBasePort = GetInt("Winterhill", "BasePort", 9900);
                ForceLocalIP = GetBool("Network", "ForceLocalIP", false);
                ForceLocalIPAddress = GetString("Network", "LocalIPAddress", "");
                Rx1Volume = GetInt("Audio", "Rx1Volume", 100);
                Rx2Volume = GetInt("Audio", "Rx2Volume", 100);
                Rx3Volume = GetInt("Audio", "Rx3Volume", 100);
                Rx4Volume = GetInt("Audio", "Rx4Volume", 100);
                AutoZoom = GetBool("Display", "AutoZoom", false);
                HardwareDecoding = GetBool("Display", "HardwareDecoding", false);
                CompatibilityMode = GetBool("Display", "CompatibilityMode", false);
                VideoCaching = GetInt("Display", "VideoCaching", 100);
                Rx1Offset = GetInt("Offsets", "Rx1Offset", 9749982);
                Rx2Offset = GetInt("Offsets", "Rx2Offset", 9749982);
                Rx3Offset = GetInt("Offsets", "Rx3Offset", 9749982);
                Rx4Offset = GetInt("Offsets", "Rx4Offset", 9749982);

                Rx1SymbolRates = GetString("SymbolRates", "Rx1SymbolRates", "");
                Rx2SymbolRates = GetString("SymbolRates", "Rx2SymbolRates", "");
                Rx3SymbolRates = GetString("SymbolRates", "Rx3SymbolRates", "");
                Rx4SymbolRates = GetString("SymbolRates", "Rx4SymbolRates", "");

                Rx1Frequency = GetInt("Frequencies", "Rx1Frequency", 0);
                Rx2Frequency = GetInt("Frequencies", "Rx2Frequency", 0);
                Rx3Frequency = GetInt("Frequencies", "Rx3Frequency", 0);
                Rx4Frequency = GetInt("Frequencies", "Rx4Frequency", 0);

                WindowWidth = GetInt("Window", "Width", 0);
                WindowHeight = GetInt("Window", "Height", 0);
                WindowX = GetInt("Window", "X", -1);
                WindowY = GetInt("Window", "Y", -1);
                WindowMaximized = GetBool("Window", "Maximized", false);
            }
            catch (Exception)
            {
                // If loading fails, use defaults
            }
        }

        /// <summary>
        /// Saves current property values to the INI file.
        /// Overwrites existing file with formatted sections and comments.
        /// </summary>
        public void Save()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Write header with timestamp
                    writer.WriteLine("; Winterhill DATV Receiver Settings");
                    writer.WriteLine("; Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    writer.WriteLine();

                    writer.WriteLine("[Winterhill]");
                    writer.WriteLine("Host=" + WinterhillHost);
                    writer.WriteLine("BasePort=" + WinterhillBasePort);
                    writer.WriteLine();

                    writer.WriteLine("[Network]");
                    writer.WriteLine("ForceLocalIP=" + (ForceLocalIP ? "true" : "false"));
                    writer.WriteLine("LocalIPAddress=" + ForceLocalIPAddress);
                    writer.WriteLine();

                    writer.WriteLine("[Offsets]");
                    writer.WriteLine("Rx1Offset=" + Rx1Offset);
                    writer.WriteLine("Rx2Offset=" + Rx2Offset);
                    writer.WriteLine("Rx3Offset=" + Rx3Offset);
                    writer.WriteLine("Rx4Offset=" + Rx4Offset);
                    writer.WriteLine();

                    writer.WriteLine("[Audio]");
                    writer.WriteLine("Rx1Volume=" + Rx1Volume);
                    writer.WriteLine("Rx2Volume=" + Rx2Volume);
                    writer.WriteLine("Rx3Volume=" + Rx3Volume);
                    writer.WriteLine("Rx4Volume=" + Rx4Volume);
                    writer.WriteLine();

                    writer.WriteLine("[Display]");
                    writer.WriteLine("AutoZoom=" + (AutoZoom ? "true" : "false"));
                    writer.WriteLine("HardwareDecoding=" + (HardwareDecoding ? "true" : "false"));
                    writer.WriteLine("CompatibilityMode=" + (CompatibilityMode ? "true" : "false"));
                    writer.WriteLine("VideoCaching=" + VideoCaching);
                    writer.WriteLine();

                    writer.WriteLine("[SymbolRates]");
                    writer.WriteLine("Rx1SymbolRates=" + Rx1SymbolRates);
                    writer.WriteLine("Rx2SymbolRates=" + Rx2SymbolRates);
                    writer.WriteLine("Rx3SymbolRates=" + Rx3SymbolRates);
                    writer.WriteLine("Rx4SymbolRates=" + Rx4SymbolRates);
                    writer.WriteLine();

                    writer.WriteLine("[Frequencies]");
                    writer.WriteLine("Rx1Frequency=" + Rx1Frequency);
                    writer.WriteLine("Rx2Frequency=" + Rx2Frequency);
                    writer.WriteLine("Rx3Frequency=" + Rx3Frequency);
                    writer.WriteLine("Rx4Frequency=" + Rx4Frequency);
                    writer.WriteLine();

                    writer.WriteLine("[Window]");
                    writer.WriteLine("Width=" + WindowWidth);
                    writer.WriteLine("Height=" + WindowHeight);
                    writer.WriteLine("X=" + WindowX);
                    writer.WriteLine("Y=" + WindowY);
                    writer.WriteLine("Maximized=" + (WindowMaximized ? "true" : "false"));
                }
            }
            catch (Exception)
            {
                // Ignore save errors (e.g., file locked, no write permission)
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets a string value from parsed INI sections.
        /// </summary>
        private string GetString(string section, string key, string defaultValue)
        {
            if (sections.ContainsKey(section) && sections[section].ContainsKey(key))
                return sections[section][key];
            return defaultValue;
        }

        /// <summary>
        /// Gets an integer value from parsed INI sections.
        /// </summary>
        private int GetInt(string section, string key, int defaultValue)
        {
            string value = GetString(section, key, null);
            if (value != null && int.TryParse(value, out int result))
                return result;
            return defaultValue;
        }

        /// <summary>
        /// Gets a boolean value from parsed INI sections.
        /// Supports true/false, yes/no, 1/0.
        /// </summary>
        private bool GetBool(string section, string key, bool defaultValue)
        {
            string value = GetString(section, key, null);
            if (value != null)
            {
                value = value.ToLower();
                if (value == "true" || value == "1" || value == "yes")
                    return true;
                if (value == "false" || value == "0" || value == "no")
                    return false;
            }
            return defaultValue;
        }

        #endregion
    }
}
