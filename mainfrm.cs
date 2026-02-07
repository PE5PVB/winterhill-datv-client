using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text.Json;
using LibVLCSharp.Shared;

namespace datvreceiver
{
    /// <summary>
    /// Main form for the Winterhill DATV Receiver client.
    /// Displays 4 video streams from a Winterhill receiver in a 2x2 grid layout.
    /// </summary>
    public partial class mainForm : Form
    {
        #region Constants

        // Winterhill communication
        private const int WINTERHILL_WEBSOCKET_PORT = 8080;
        private const int RECONNECT_DELAY_MS = 5000;
        private const int COMMAND_DELAY_MS = 300;

        // Symbol rate hopping
        private const int SR_HOP_INTERVAL_MS = 3000;

        // Auto zoom timing
        private const int AUTO_ZOOM_DELAY_MS = 2000;

        // Title bar behavior
        private const int TITLE_BAR_SHOW_ZONE = 50;
        private const int TITLE_BAR_HIDE_DELAY = 1500;

        // Window sizing
        private const int RESIZE_BORDER = 8;
        private const double ASPECT_RATIO = 16.0 / 9.0;

        // Windows messages for borderless window resizing
        private const int WM_SIZING = 0x214;
        private const int WM_NCHITTEST = 0x84;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int WMSZ_LEFT = 1;
        private const int WMSZ_RIGHT = 2;
        private const int WMSZ_TOP = 3;
        private const int WMSZ_TOPLEFT = 4;
        private const int WMSZ_TOPRIGHT = 5;
        private const int WMSZ_BOTTOM = 6;
        private const int WMSZ_BOTTOMLEFT = 7;
        private const int WMSZ_BOTTOMRIGHT = 8;

        // Hit test values for borderless window resizing
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int HTCAPTION = 2;

        /// <summary>
        /// Lookup table for Winterhill scan states.
        /// </summary>
        private readonly Dictionary<int, string> scanStateLookup = new Dictionary<int, string>()
        {
            { 0, "No Signal" },
            { 1, "Init" },
            { 2, "Lock DVB-S2" },
            { 3, "Lock DVB-S" },
            { 0x80, "Lost" },
            { 0x81, "Timeout" },
            { 0x82, "Idle" },
        };

        #endregion

        #region Private Fields - Child Forms

        private StatusForm statusForm;
        private SettingsForm settingsForm;
        private IniSettings iniSettings;

        #endregion

        #region Private Fields - Winterhill Connection

        private string winterhillHost = "";
        private int winterhillBasePort = 0;
        private bool forceLocalIP = false;
        private string localIP;

        private WebSocketClient monitorWS;
        private WebSocketClient controlWS;
        private System.Threading.Timer monitorReconnectTimer;
        private System.Threading.Timer controlReconnectTimer;

        // Command queue to prevent rapid-fire commands causing MHz/kS/s mix-ups
        private readonly object commandQueueLock = new object();
        private DateTime lastCommandTime = DateTime.MinValue;

        #endregion

        #region Private Fields - Receiver Settings

        private int rx1Offset = 0;
        private int rx2Offset = 0;
        private int rx3Offset = 0;
        private int rx4Offset = 0;

        private readonly object volumeLock = new object();
        private int rx1Volume = 100;
        private int rx2Volume = 100;
        private int rx3Volume = 100;
        private int rx4Volume = 100;

        #endregion

        #region Private Fields - VLC Video Players

        private LibVLC libVLC1;
        private LibVLC libVLC2;
        private LibVLC libVLC3;
        private LibVLC libVLC4;

        private Media rx1Media;
        private Media rx2Media;
        private Media rx3Media;
        private Media rx4Media;

        private bool hardwareDecodingEnabled = false;
        private bool compatibilityModeEnabled = false;
        private int videoCaching = 100;

        // Track which receivers have active video output
        private bool[] videoOutputReady = new bool[4];

        // Track previous scan states to detect changes
        private int[] prevStates = new int[] { -1, -1, -1, -1 };

        #endregion

        #region Private Fields - OSD (On-Screen Display)

        // OSD overlays for no-signal screens (frequency/SR at top, status at bottom)
        private PictureBox osdPicTop1, osdPicBottom1;
        private PictureBox osdPicTop2, osdPicBottom2;
        private PictureBox osdPicTop3, osdPicBottom3;
        private PictureBox osdPicTop4, osdPicBottom4;

        // OSD overlays for video streams (shown when video is playing)
        private PictureBox videoOsd1, videoOsd2, videoOsd3, videoOsd4;

        // Current OSD text for each receiver
        private string[] currentVideoOsdText = new string[4] { "", "", "", "" };

        // Last known good OSD values (used during symbol rate hopping)
        private string[] lastGoodOsdFreq = new string[] { "", "", "", "" };
        private string[] lastGoodOsdSr = new string[] { "", "", "", "" };

        #endregion

        #region Private Fields - Symbol Rate Hopping

        private System.Windows.Forms.Timer srHopTimer;
        private int[][] rxSymbolRates = new int[4][];
        private int[] rxCurrentSrIndex = new int[4];
        private bool[] rxInitialSrSet = new bool[4];

        #endregion

        #region Private Fields - Auto Zoom

        private bool autoZoomEnabled = false;
        private System.Windows.Forms.Timer autoZoomTimer;
        private int autoZoomTargetRx = 0;
        private int autoZoomPendingRx = 0;
        private DateTime autoZoomPendingTime;
        private bool isFullScreen = false;

        #endregion

        #region Private Fields - Title Bar and Window Management

        private System.Windows.Forms.Timer titleBarTimer;
        private bool titleBarVisible = false;
        private DateTime titleBarLastActivity = DateTime.MinValue;
        private bool isDragging = false;
        private Point dragStartPoint;

        #endregion

        #region Structs for Windows API

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        #region Delegates for Thread-Safe UI Updates

        private delegate void UpdateVisibilityDelegate(PictureBox pic, bool visible);
        private delegate void UpdateLabelVisibilityDelegate(Label lbl, bool visible);
        private delegate void UpdateLabelDelegate(Label lb, string text);
        private delegate void UpdateLabelColorDelegate(Label lb, Color col);

        #endregion

        #region Constructor and Initialization

        public mainForm()
        {
            InitializeComponent();

            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            this.FormClosing += MainForm_FormClosing;

            // Apply dark theme
            this.BackColor = ThemeHelper.BackgroundDark;
            MiddleVideoSplitter.BackColor = ThemeHelper.BackgroundDark;
            LeftVideoSplitter.BackColor = ThemeHelper.BackgroundDark;
            RightVideoSplitter.BackColor = ThemeHelper.BackgroundDark;

            // Setup title bar auto-hide timer
            titleBarTimer = new System.Windows.Forms.Timer();
            titleBarTimer.Interval = 100;
            titleBarTimer.Tick += TitleBarTimer_Tick;
            titleBarTimer.Start();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
            InitializeLibVLC();
            RestoreWindowState();

            // Always start with title bar hidden
            titleBarVisible = false;
            titleBarPanel.Visible = false;

            // Determine local IP address if not forced
            if (!forceLocalIP)
            {
                DetermineLocalIP();
            }

            InitializeSettingsForm();
            InitializeTimers();
            InitializeStatusForm();

            ConnectWebSockets();
            ConfigureVideoPlayers();
            CreateOsdOverlays();

            // Remove old labels (replaced by OSD overlays)
            lab_rx1_nothing.Parent?.Controls.Remove(lab_rx1_nothing);
            lab_rx2_nothing.Parent?.Controls.Remove(lab_rx2_nothing);
            lab_rx3_nothing.Parent?.Controls.Remove(lab_rx3_nothing);
            lab_rx4_nothing.Parent?.Controls.Remove(lab_rx4_nothing);
        }

        private void RestoreWindowState()
        {
            if (iniSettings.WindowWidth > 0 && iniSettings.WindowHeight > 0)
            {
                this.Width = iniSettings.WindowWidth;
                this.Height = iniSettings.WindowHeight;
            }

            if (iniSettings.WindowX >= 0 && iniSettings.WindowY >= 0)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Left = iniSettings.WindowX;
                this.Top = iniSettings.WindowY;
            }

            if (iniSettings.WindowMaximized)
            {
                this.MaximizedBounds = Screen.FromControl(this).WorkingArea;
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void InitializeSettingsForm()
        {
            settingsForm = new SettingsForm();
            settingsForm.SetIniSettings(iniSettings);
            settingsForm.LoadSettings(
                winterhillHost, winterhillBasePort, forceLocalIP, localIP,
                rx1Offset, rx2Offset, rx3Offset, rx4Offset,
                autoZoomEnabled, hardwareDecodingEnabled, compatibilityModeEnabled, videoCaching);
            settingsForm.OnAutoZoomChange += HandleAutoZoomChange;
            settingsForm.OnSettingsChanged += HandleSettingsChanged;
        }

        private void InitializeTimers()
        {
            // Auto zoom timer - monitors receiver lock states
            autoZoomTimer = new System.Windows.Forms.Timer();
            autoZoomTimer.Interval = 100;
            autoZoomTimer.Tick += AutoZoomTimer_Tick;
            autoZoomTimer.Start();

            // Symbol rate hopping timer
            srHopTimer = new System.Windows.Forms.Timer();
            srHopTimer.Interval = SR_HOP_INTERVAL_MS;
            srHopTimer.Tick += SrHopTimer_Tick;
            srHopTimer.Start();
        }

        private void InitializeStatusForm()
        {
            statusForm = new StatusForm();
            statusForm.SetScanStateLookup(scanStateLookup);
            statusForm.SetInitialVolumes(rx1Volume, rx2Volume, rx3Volume, rx4Volume);
            statusForm.OnForceTS += HandleForceTS;
            statusForm.OnVolumeChange += HandleVolumeChange;
            statusForm.OnFrequencyClick += ShowFrequencySelectDialog;
            statusForm.OnOpenSettings += ShowSettingsForm;
            statusForm.OnSymbolRatesChange += HandleSymbolRatesChange;

            // Load symbol rate checkboxes from settings
            statusForm.LoadSymbolRateCheckboxes(1, iniSettings.Rx1SymbolRates);
            statusForm.LoadSymbolRateCheckboxes(2, iniSettings.Rx2SymbolRates);
            statusForm.LoadSymbolRateCheckboxes(3, iniSettings.Rx3SymbolRates);
            statusForm.LoadSymbolRateCheckboxes(4, iniSettings.Rx4SymbolRates);

            statusForm.Show();
        }

        private void CreateOsdOverlays()
        {
            // Create OSD overlays for no-signal screens
            osdPicTop1 = CreateOsdPictureBox(nolock_rx1);
            osdPicBottom1 = CreateOsdPictureBox(nolock_rx1);
            osdPicTop2 = CreateOsdPictureBox(nolock_rx2);
            osdPicBottom2 = CreateOsdPictureBox(nolock_rx2);
            osdPicTop3 = CreateOsdPictureBox(nolock_rx3);
            osdPicBottom3 = CreateOsdPictureBox(nolock_rx3);
            osdPicTop4 = CreateOsdPictureBox(nolock_rx4);
            osdPicBottom4 = CreateOsdPictureBox(nolock_rx4);

            // Create OSD overlays for video streams
            videoOsd1 = CreateVideoOsdOverlay(videoRx1);
            videoOsd2 = CreateVideoOsdOverlay(videoRx2);
            videoOsd3 = CreateVideoOsdOverlay(videoRx3);
            videoOsd4 = CreateVideoOsdOverlay(videoRx4);
        }

        #endregion

        #region Settings Management

        private void LoadSettings()
        {
            iniSettings = new IniSettings();
            iniSettings.Load();

            winterhillHost = iniSettings.WinterhillHost;
            winterhillBasePort = iniSettings.WinterhillBasePort;
            forceLocalIP = iniSettings.ForceLocalIP;
            localIP = iniSettings.ForceLocalIPAddress;

            rx1Offset = iniSettings.Rx1Offset;
            rx2Offset = iniSettings.Rx2Offset;
            rx3Offset = iniSettings.Rx3Offset;
            rx4Offset = iniSettings.Rx4Offset;

            rx1Volume = iniSettings.Rx1Volume;
            rx2Volume = iniSettings.Rx2Volume;
            rx3Volume = iniSettings.Rx3Volume;
            rx4Volume = iniSettings.Rx4Volume;

            autoZoomEnabled = iniSettings.AutoZoom;
            hardwareDecodingEnabled = iniSettings.HardwareDecoding;
            compatibilityModeEnabled = iniSettings.CompatibilityMode;
            videoCaching = iniSettings.VideoCaching;

            // Load symbol rate hopping settings
            rxSymbolRates[0] = ParseSymbolRates(iniSettings.Rx1SymbolRates);
            rxSymbolRates[1] = ParseSymbolRates(iniSettings.Rx2SymbolRates);
            rxSymbolRates[2] = ParseSymbolRates(iniSettings.Rx3SymbolRates);
            rxSymbolRates[3] = ParseSymbolRates(iniSettings.Rx4SymbolRates);
        }

        private int[] ParseSymbolRates(string symbolRatesStr)
        {
            if (string.IsNullOrEmpty(symbolRatesStr))
                return new int[0];

            var rates = new List<int>();
            foreach (var rate in symbolRatesStr.Split(','))
            {
                if (int.TryParse(rate.Trim(), out int r))
                    rates.Add(r);
            }
            return rates.ToArray();
        }

        private void HandleSettingsChanged()
        {
            iniSettings.Load();

            // Check if Winterhill connection settings changed
            bool reconnectNeeded = (winterhillHost != iniSettings.WinterhillHost ||
                                    winterhillBasePort != iniSettings.WinterhillBasePort);

            // Update local variables
            winterhillHost = iniSettings.WinterhillHost;
            winterhillBasePort = iniSettings.WinterhillBasePort;
            forceLocalIP = iniSettings.ForceLocalIP;
            localIP = iniSettings.ForceLocalIPAddress;

            rx1Offset = iniSettings.Rx1Offset;
            rx2Offset = iniSettings.Rx2Offset;
            rx3Offset = iniSettings.Rx3Offset;
            rx4Offset = iniSettings.Rx4Offset;

            // Reload symbol rate hopping settings
            rxSymbolRates[0] = ParseSymbolRates(iniSettings.Rx1SymbolRates);
            rxSymbolRates[1] = ParseSymbolRates(iniSettings.Rx2SymbolRates);
            rxSymbolRates[2] = ParseSymbolRates(iniSettings.Rx3SymbolRates);
            rxSymbolRates[3] = ParseSymbolRates(iniSettings.Rx4SymbolRates);

            // Reconnect WebSockets if needed
            if (reconnectNeeded)
            {
                Debug("Settings changed - reconnecting to Winterhill...");

                try
                {
                    monitorReconnectTimer?.Dispose();
                    controlReconnectTimer?.Dispose();
                    monitorWS?.Close();
                    controlWS?.Close();
                }
                catch { }

                ConnectWebSockets();
            }

            // Redetermine local IP if not forced
            if (!forceLocalIP)
            {
                DetermineLocalIP();
            }

            Debug("Settings applied successfully");
        }

        private void DetermineLocalIP()
        {
            Debug("Get Local IP: ");

            int ipCount = 0;
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Debug(ip.ToString());
                    localIP = ip.ToString();
                    ipCount++;
                }
            }

            if (ipCount == 0)
            {
                Debug("Warning: No Local IP Address Detected");
            }
            else if (ipCount > 1)
            {
                Debug("Warning: Multiple IP Addresses Detected for this pc");
            }
        }

        #endregion

        #region VLC Configuration

        /// <summary>
        /// Builds VLC options based on hardware decoding and compatibility mode settings.
        /// </summary>
        private string[] GetVlcOptions()
        {
            var options = new List<string>
            {
                "--aout=directsound"
            };

            if (compatibilityModeEnabled)
            {
                // Compatibility mode: more conservative settings for older hardware
                options.Add("--network-caching=300");
                options.Add("--live-caching=300");
                options.Add("--udp-caching=300");
                options.Add("--file-caching=100");
                options.Add("--disc-caching=100");
                options.Add("--vout=direct3d9");
            }
            else
            {
                // Normal mode: low latency settings
                options.Add("--network-caching=100");
                options.Add("--live-caching=100");
                options.Add("--udp-caching=100");
                options.Add("--file-caching=0");
                options.Add("--disc-caching=0");
                options.Add("--sout-mux-caching=0");
                options.Add("--clock-jitter=0");
                options.Add("--clock-synchro=0");

                // Only drop frames when NOT using hardware decoding
                if (!hardwareDecodingEnabled)
                {
                    options.Add("--drop-late-frames");
                    options.Add("--skip-frames");
                    options.Add("--avcodec-fast");
                    options.Add("--avcodec-skiploopfilter=4");
                }

                options.Add("--vout=direct3d11");
            }

            return options.ToArray();
        }

        private void InitializeLibVLC()
        {
            var options = GetVlcOptions();
            libVLC1 = new LibVLC(options);
            libVLC2 = new LibVLC(options);
            libVLC3 = new LibVLC(options);
            libVLC4 = new LibVLC(options);

            // Log VLC decoder selection to debug
            libVLC1.Log += (s, e) => VlcLogFilter(1, e);
            libVLC2.Log += (s, e) => VlcLogFilter(2, e);
            libVLC3.Log += (s, e) => VlcLogFilter(3, e);
            libVLC4.Log += (s, e) => VlcLogFilter(4, e);
        }

        /// <summary>
        /// Filters VLC log messages and shows decoder/codec related entries in debug.
        /// </summary>
        private void VlcLogFilter(int rx, LogEventArgs e)
        {
            string msg = e.FormattedLog;
            if (msg == null) return;

            // Filter for decoder, codec, and hardware acceleration messages
            if (msg.Contains("decoder") || msg.Contains("codec") ||
                msg.Contains("d3d11va") || msg.Contains("dxva") ||
                msg.Contains("qsv") || msg.Contains("nvdec") ||
                msg.Contains("vaapi") || msg.Contains("hw accel") ||
                msg.Contains("hardware") || msg.Contains("using"))
            {
                Debug($"VLC RX{rx}: {msg.Trim()}");
            }
        }

        /// <summary>
        /// Configures media stream for a specific receiver.
        /// </summary>
        private Media ConfigureMedia(int rx)
        {
            int port = winterhillBasePort + 40 + rx;
            string streamUrl = $"udp://@:{port}";
            Debug($"RX{rx}: {streamUrl}");

            Media media;
            switch (rx)
            {
                case 1: media = new Media(libVLC1, streamUrl, FromType.FromLocation); break;
                case 2: media = new Media(libVLC2, streamUrl, FromType.FromLocation); break;
                case 3: media = new Media(libVLC3, streamUrl, FromType.FromLocation); break;
                case 4: media = new Media(libVLC4, streamUrl, FromType.FromLocation); break;
                default: media = new Media(libVLC1, streamUrl, FromType.FromLocation); break;
            }

            MediaConfiguration mediaConfig = new MediaConfiguration();
            mediaConfig.EnableHardwareDecoding = hardwareDecodingEnabled;
            media.AddOption(mediaConfig);

            // Low-latency caching options for live DATV streams
            media.AddOption(":network-caching=" + videoCaching);
            media.AddOption(":live-caching=" + videoCaching);
            media.AddOption(":udp-caching=" + videoCaching);
            media.AddOption(":clock-jitter=0");
            media.AddOption(":clock-synchro=0");
            media.AddOption(":no-audio-time-stretch");
            media.AddOption(":udp-timeout=1000");

            return media;
        }

        private void ConfigureVideoPlayers()
        {
            // Configure RX1
            rx1Media = ConfigureMedia(1);
            videoRx1.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC1);
            videoRx1.MediaPlayer.Stopped += Rx1_MediaPlayer_Stopped;
            videoRx1.MediaPlayer.Playing += Rx1_MediaPlayer_Playing;
            videoRx1.MediaPlayer.TimeChanged += Rx1_MediaPlayer_TimeChanged;
            videoRx1.MediaPlayer.Vout += Rx1_MediaPlayer_Vout;
            videoRx1.MediaPlayer.EnableMouseInput = false;
            videoRx1.MediaPlayer.EnableKeyInput = false;

            // Configure RX2
            rx2Media = ConfigureMedia(2);
            videoRx2.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC2);
            videoRx2.MediaPlayer.Stopped += Rx2_MediaPlayer_Stopped;
            videoRx2.MediaPlayer.Playing += Rx2_MediaPlayer_Playing;
            videoRx2.MediaPlayer.TimeChanged += Rx2_MediaPlayer_TimeChanged;
            videoRx2.MediaPlayer.Vout += Rx2_MediaPlayer_Vout;
            videoRx2.MediaPlayer.EnableMouseInput = false;
            videoRx2.MediaPlayer.EnableKeyInput = false;

            // Configure RX3
            rx3Media = ConfigureMedia(3);
            videoRx3.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC3);
            videoRx3.MediaPlayer.Stopped += Rx3_MediaPlayer_Stopped;
            videoRx3.MediaPlayer.Playing += Rx3_MediaPlayer_Playing;
            videoRx3.MediaPlayer.TimeChanged += Rx3_MediaPlayer_TimeChanged;
            videoRx3.MediaPlayer.Vout += Rx3_MediaPlayer_Vout;
            videoRx3.MediaPlayer.EnableMouseInput = false;
            videoRx3.MediaPlayer.EnableKeyInput = false;

            // Configure RX4
            rx4Media = ConfigureMedia(4);
            videoRx4.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC4);
            videoRx4.MediaPlayer.Stopped += Rx4_MediaPlayer_Stopped;
            videoRx4.MediaPlayer.Playing += Rx4_MediaPlayer_Playing;
            videoRx4.MediaPlayer.TimeChanged += Rx4_MediaPlayer_TimeChanged;
            videoRx4.MediaPlayer.Vout += Rx4_MediaPlayer_Vout;
            videoRx4.MediaPlayer.EnableMouseInput = false;
            videoRx4.MediaPlayer.EnableKeyInput = false;
        }

        #endregion

        #region Video Player Events

        // TimeChanged events - used to maintain volume levels
        private void Rx1_MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            lock (volumeLock) { videoRx1.MediaPlayer.Volume = rx1Volume; }
        }

        private void Rx2_MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            lock (volumeLock) { videoRx2.MediaPlayer.Volume = rx2Volume; }
        }

        private void Rx3_MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            lock (volumeLock) { videoRx3.MediaPlayer.Volume = rx3Volume; }
        }

        private void Rx4_MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            lock (volumeLock) { videoRx4.MediaPlayer.Volume = rx4Volume; }
        }

        // Vout events - fire when video output becomes available
        private void Rx1_MediaPlayer_Vout(object sender, MediaPlayerVoutEventArgs e)
        {
            Debug("rx1 vout: " + e.Count);
            if (e.Count > 0 && !videoOutputReady[0])
            {
                videoOutputReady[0] = true;
                SetNoSignalVisible(nolock_rx1, false);
            }
        }

        private void Rx2_MediaPlayer_Vout(object sender, MediaPlayerVoutEventArgs e)
        {
            Debug("rx2 vout: " + e.Count);
            if (e.Count > 0 && !videoOutputReady[1])
            {
                videoOutputReady[1] = true;
                SetNoSignalVisible(nolock_rx2, false);
            }
        }

        private void Rx3_MediaPlayer_Vout(object sender, MediaPlayerVoutEventArgs e)
        {
            Debug("rx3 vout: " + e.Count);
            if (e.Count > 0 && !videoOutputReady[2])
            {
                videoOutputReady[2] = true;
                SetNoSignalVisible(nolock_rx3, false);
            }
        }

        private void Rx4_MediaPlayer_Vout(object sender, MediaPlayerVoutEventArgs e)
        {
            Debug("rx4 vout: " + e.Count);
            if (e.Count > 0 && !videoOutputReady[3])
            {
                videoOutputReady[3] = true;
                SetNoSignalVisible(nolock_rx4, false);
            }
        }

        // Playing events
        private void Rx1_MediaPlayer_Playing(object sender, EventArgs e) { Debug("rx1 playing"); }
        private void Rx2_MediaPlayer_Playing(object sender, EventArgs e) { Debug("rx2 playing"); }
        private void Rx3_MediaPlayer_Playing(object sender, EventArgs e) { Debug("rx3 playing"); }
        private void Rx4_MediaPlayer_Playing(object sender, EventArgs e) { Debug("rx4 playing"); }

        // Stopped events
        private void Rx1_MediaPlayer_Stopped(object sender, EventArgs e)
        {
            Debug("rx1 stopped");
            videoOutputReady[0] = false;
            SetNoSignalVisible(nolock_rx1, true);
        }

        private void Rx2_MediaPlayer_Stopped(object sender, EventArgs e)
        {
            Debug("rx2 stopped");
            videoOutputReady[1] = false;
            SetNoSignalVisible(nolock_rx2, true);
        }

        private void Rx3_MediaPlayer_Stopped(object sender, EventArgs e)
        {
            Debug("rx3 stopped");
            videoOutputReady[2] = false;
            SetNoSignalVisible(nolock_rx3, true);
        }

        private void Rx4_MediaPlayer_Stopped(object sender, EventArgs e)
        {
            Debug("rx4 stopped");
            videoOutputReady[3] = false;
            SetNoSignalVisible(nolock_rx4, true);
        }

        #endregion

        #region WebSocket Communication

        private void ConnectWebSockets()
        {
            if (string.IsNullOrEmpty(winterhillHost))
            {
                Debug("Error: No Winterhill Host Specified");
                return;
            }

            string url = $"ws://{winterhillHost}:{WINTERHILL_WEBSOCKET_PORT}/";

            // Monitor WebSocket - receives telemetry and status
            monitorWS = new WebSocketClient(url, "monitor");
            monitorWS.OnOpen += MonitorWS_OnOpen;
            monitorWS.OnMessage += MonitorWS_OnMessage;
            monitorWS.OnClose += MonitorWS_OnClose;
            monitorWS.ConnectAsync();

            // Control WebSocket - sends frequency/SR commands
            controlWS = new WebSocketClient(url, "control");
            controlWS.OnOpen += ControlWS_OnOpen;
            controlWS.OnMessage += ControlWS_OnMessage;
            controlWS.OnClose += ControlWS_OnClose;
            controlWS.ConnectAsync();
        }

        private void MonitorWS_OnOpen(object sender, EventArgs e)
        {
            Debug("Monitor WS Open");
        }

        private void MonitorWS_OnMessage(object sender, WsMessageEventArgs e)
        {
            monitorMessage mm = JsonSerializer.Deserialize<monitorMessage>(e.Data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true });
            ProcessReceiverStatus(mm);
        }

        private void MonitorWS_OnClose(object sender, WsCloseEventArgs e)
        {
            Debug("Monitor WS Closed - reconnecting in 5 seconds...");
            monitorReconnectTimer?.Dispose();
            monitorReconnectTimer = new System.Threading.Timer(
                _ => ReconnectMonitorWS(),
                null,
                RECONNECT_DELAY_MS,
                Timeout.Infinite
            );
        }

        private void ReconnectMonitorWS()
        {
            try
            {
                if (monitorWS != null && !monitorWS.IsAlive)
                {
                    Debug("Attempting Monitor WS reconnect...");
                    monitorWS.ConnectAsync();
                }
            }
            catch (Exception ex)
            {
                Debug("Monitor WS reconnect failed: " + ex.Message);
            }
        }

        private void ControlWS_OnOpen(object sender, EventArgs e)
        {
            Debug("Control WS Open");
        }

        private void ControlWS_OnMessage(object sender, WsMessageEventArgs e)
        {
            // Control responses not currently used
        }

        private void ControlWS_OnClose(object sender, WsCloseEventArgs e)
        {
            Debug("Control WS Closed - reconnecting in 5 seconds...");
            controlReconnectTimer?.Dispose();
            controlReconnectTimer = new System.Threading.Timer(
                _ => ReconnectControlWS(),
                null,
                RECONNECT_DELAY_MS,
                Timeout.Infinite
            );
        }

        private void ReconnectControlWS()
        {
            try
            {
                if (controlWS != null && !controlWS.IsAlive)
                {
                    Debug("Attempting Control WS reconnect...");
                    controlWS.ConnectAsync();
                }
            }
            catch (Exception ex)
            {
                Debug("Control WS reconnect failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Sends a frequency/symbol rate command to the Winterhill receiver.
        /// </summary>
        public void SetFrequency(int rx, int freqKHz, int symbolRate)
        {
            QueueFrequencyCommand(rx, freqKHz, symbolRate);
        }

        private async void QueueFrequencyCommand(int rx, int freqKHz, int symbolRate)
        {
            int delayNeeded = 0;

            lock (commandQueueLock)
            {
                var now = DateTime.Now;
                var elapsed = (now - lastCommandTime).TotalMilliseconds;

                if (elapsed < COMMAND_DELAY_MS)
                {
                    delayNeeded = COMMAND_DELAY_MS - (int)elapsed;
                }

                lastCommandTime = now.AddMilliseconds(delayNeeded);
            }

            if (delayNeeded > 0)
            {
                await System.Threading.Tasks.Task.Delay(delayNeeded);
            }

            Debug($"Set Freq: {rx + 1},{freqKHz} - {symbolRate}");

            int ifOffset = 0;
            switch (rx)
            {
                case 0: ifOffset = rx1Offset; break;
                case 1: ifOffset = rx2Offset; break;
                case 2: ifOffset = rx3Offset; break;
                case 3: ifOffset = rx4Offset; break;
            }

            controlWS.Send($"F{rx + 1},{freqKHz},{symbolRate},{ifOffset}");

            lock (commandQueueLock)
            {
                lastCommandTime = DateTime.Now;
            }
        }

        #endregion

        #region Receiver Status Processing

        /// <summary>
        /// Processes incoming receiver status messages from Winterhill.
        /// </summary>
        private void ProcessReceiverStatus(monitorMessage mm)
        {
            string statusMsg = "";

            for (int x = 0; x < mm.rx.Length; x++)
            {
                int rx = mm.rx[x].rx;
                if (rx < 1 || rx > 4) continue;

                int idx = rx - 1;
                int newState = mm.rx[x].scanstate;
                bool stateChanged = newState != prevStates[idx];

                // Update StatusForm
                statusForm.UpdateReceiverStatus(rx, mm.rx[x], localIP);

                // Initialize symbol rate from Winterhill if not configured
                InitializeSymbolRateFromWinterhill(rx, idx, mm.rx[x]);

                // Initialize or validate frequency
                ProcessFrequencyValidation(rx, idx, mm.rx[x]);

                // Update OSD displays
                UpdateOsdDisplays(rx, idx, newState, mm.rx[x]);

                // Handle video play/stop based on state changes
                if (stateChanged)
                {
                    HandleReceiverStateChange(rx, idx, newState);
                    prevStates[idx] = newState;
                }

                // Build status bar message
                statusMsg += (rx > 1 ? " - " : "") + "RX" + rx + ": " + scanStateLookup[newState];
                if (newState == 2 || newState == 3)
                {
                    statusMsg += " - " + mm.rx[x].service_name + " " + mm.rx[x].mer + " dB Mer,D" + mm.rx[x].dbmargin;
                }
            }

            statusForm.UpdateStatusText(statusMsg);
        }

        private void InitializeSymbolRateFromWinterhill(int rx, int idx, ReceiverMessage rxMsg)
        {
            if (!rxInitialSrSet[idx] && (rxSymbolRates[idx] == null || rxSymbolRates[idx].Length == 0))
            {
                string srStr = rxMsg.symbol_rate;
                if (!string.IsNullOrEmpty(srStr) && int.TryParse(srStr, out int winterhillSr))
                {
                    statusForm.LoadSymbolRateCheckboxes(rx, winterhillSr.ToString());
                    rxSymbolRates[idx] = new int[] { winterhillSr };

                    switch (rx)
                    {
                        case 1: iniSettings.Rx1SymbolRates = winterhillSr.ToString(); break;
                        case 2: iniSettings.Rx2SymbolRates = winterhillSr.ToString(); break;
                        case 3: iniSettings.Rx3SymbolRates = winterhillSr.ToString(); break;
                        case 4: iniSettings.Rx4SymbolRates = winterhillSr.ToString(); break;
                    }
                    iniSettings.Save();
                    Debug($"RX{rx} initial SR from Winterhill: {winterhillSr} kS/s");
                }
                rxInitialSrSet[idx] = true;
            }
        }

        private void ProcessFrequencyValidation(int rx, int idx, ReceiverMessage rxMsg)
        {
            // Get configured frequency from settings
            int savedFreq = 0;
            switch (rx)
            {
                case 1: savedFreq = iniSettings.Rx1Frequency; break;
                case 2: savedFreq = iniSettings.Rx2Frequency; break;
                case 3: savedFreq = iniSettings.Rx3Frequency; break;
                case 4: savedFreq = iniSettings.Rx4Frequency; break;
            }

            // Get current Winterhill frequency
            string currentFreqStr = rxMsg.frequency;
            int currentFreqKHz = 0;
            if (!string.IsNullOrEmpty(currentFreqStr) &&
                double.TryParse(currentFreqStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double currentFreqMHz))
            {
                currentFreqKHz = (int)(currentFreqMHz * 1000);
            }

            if (savedFreq == 0)
            {
                // Initialize frequency from Winterhill
                if (currentFreqKHz > 0)
                {
                    switch (rx)
                    {
                        case 1: iniSettings.Rx1Frequency = currentFreqKHz; break;
                        case 2: iniSettings.Rx2Frequency = currentFreqKHz; break;
                        case 3: iniSettings.Rx3Frequency = currentFreqKHz; break;
                        case 4: iniSettings.Rx4Frequency = currentFreqKHz; break;
                    }
                    iniSettings.Save();
                    Debug($"RX{rx} initial frequency from Winterhill: {currentFreqKHz} kHz");
                }
            }
            else if (currentFreqKHz > 0)
            {
                // Check if deviation from configured frequency is > 250 kHz
                int deviation = Math.Abs(currentFreqKHz - savedFreq);
                if (deviation > 250)
                {
                    Debug($"RX{rx} frequency deviation {deviation} kHz > 250 kHz, resetting to {savedFreq} kHz");

                    int currentSr = 333;
                    string srStr = rxMsg.symbol_rate;
                    if (!string.IsNullOrEmpty(srStr) && int.TryParse(srStr, out int parsedSr))
                        currentSr = parsedSr;

                    SetFrequency(rx - 1, savedFreq, currentSr);
                }
            }
        }

        private void UpdateOsdDisplays(int rx, int idx, int newState, ReceiverMessage rxMsg)
        {
            // Get current values for OSD
            string osdCurrentFreq = rxMsg.frequency ?? "";
            string osdCurrentSr = rxMsg.symbol_rate ?? "";

            // Format frequency
            string osdFreq = "";
            if (!string.IsNullOrEmpty(osdCurrentFreq) && osdCurrentFreq != "0")
            {
                if (double.TryParse(osdCurrentFreq, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double freqVal))
                    osdFreq = freqVal.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
                else
                    osdFreq = osdCurrentFreq;
                lastGoodOsdFreq[idx] = osdFreq;
            }
            else
            {
                osdFreq = lastGoodOsdFreq[idx];
            }

            // Symbol rate
            string osdSr = "";
            if (!string.IsNullOrEmpty(osdCurrentSr) && osdCurrentSr != "0")
            {
                osdSr = osdCurrentSr;
                lastGoodOsdSr[idx] = osdSr;
            }
            else
            {
                osdSr = lastGoodOsdSr[idx];
            }

            // Build text strings
            string freqText = !string.IsNullOrEmpty(osdFreq) ? osdFreq + " MHz" : "";
            string srText = !string.IsNullOrEmpty(osdSr) ? osdSr + " kS/s" : "";
            string statusText = scanStateLookup.ContainsKey(newState) ? scanStateLookup[newState] : "";

            // Build marquee text for video overlay
            string marqueeText = BuildMarqueeText(freqText, srText, statusText, rxMsg);

            bool isCurrentlyLocked = newState == 2 || newState == 3;

            // Build text for no-signal display
            string freqSrForNoLock = "";
            if (!string.IsNullOrEmpty(freqText) && !string.IsNullOrEmpty(srText))
                freqSrForNoLock = freqText + " - " + srText;
            else if (!string.IsNullOrEmpty(freqText))
                freqSrForNoLock = freqText;
            else if (!string.IsNullOrEmpty(srText))
                freqSrForNoLock = srText;

            // Update OSD based on receiver
            switch (rx)
            {
                case 1:
                    if (isCurrentlyLocked && videoOutputReady[0])
                        UpdateVideoOsd(videoOsd1, marqueeText, videoRx1.Parent.Width, 0);
                    else
                        ClearVideoOsd(videoOsd1, 0);
                    if (!videoOutputReady[0])
                        SafeUpdateNoLockOsd(osdPicTop1, osdPicBottom1, nolock_rx1, freqSrForNoLock, statusText);
                    break;
                case 2:
                    if (isCurrentlyLocked && videoOutputReady[1])
                        UpdateVideoOsd(videoOsd2, marqueeText, videoRx2.Parent.Width, 1);
                    else
                        ClearVideoOsd(videoOsd2, 1);
                    if (!videoOutputReady[1])
                        SafeUpdateNoLockOsd(osdPicTop2, osdPicBottom2, nolock_rx2, freqSrForNoLock, statusText);
                    break;
                case 3:
                    if (isCurrentlyLocked && videoOutputReady[2])
                        UpdateVideoOsd(videoOsd3, marqueeText, videoRx3.Parent.Width, 2);
                    else
                        ClearVideoOsd(videoOsd3, 2);
                    if (!videoOutputReady[2])
                        SafeUpdateNoLockOsd(osdPicTop3, osdPicBottom3, nolock_rx3, freqSrForNoLock, statusText);
                    break;
                case 4:
                    if (isCurrentlyLocked && videoOutputReady[3])
                        UpdateVideoOsd(videoOsd4, marqueeText, videoRx4.Parent.Width, 3);
                    else
                        ClearVideoOsd(videoOsd4, 3);
                    if (!videoOutputReady[3])
                        SafeUpdateNoLockOsd(osdPicTop4, osdPicBottom4, nolock_rx4, freqSrForNoLock, statusText);
                    break;
            }
        }

        private string BuildMarqueeText(string freqText, string srText, string statusText, ReceiverMessage rxMsg)
        {
            string marqueeText = "";

            if (!string.IsNullOrEmpty(freqText) && !string.IsNullOrEmpty(srText))
                marqueeText = freqText + " - " + srText;
            else if (!string.IsNullOrEmpty(freqText))
                marqueeText = freqText;
            else if (!string.IsNullOrEmpty(srText))
                marqueeText = srText;

            if (!string.IsNullOrEmpty(statusText))
                marqueeText += (marqueeText.Length > 0 ? " - " : "") + statusText;

            string callsign = rxMsg.service_name ?? "";
            string mer = rxMsg.mer ?? "";
            string dbmargin = rxMsg.dbmargin ?? "";

            if (!string.IsNullOrEmpty(callsign))
                marqueeText += " - " + callsign;
            if (!string.IsNullOrEmpty(mer))
                marqueeText += " - " + mer + " dB";
            if (!string.IsNullOrEmpty(dbmargin))
                marqueeText += " - D" + dbmargin;

            return marqueeText;
        }

        private void HandleReceiverStateChange(int rx, int idx, int newState)
        {
            bool isLocked = newState == 2 || newState == 3;

            switch (rx)
            {
                case 1:
                    if (isLocked)
                    {
                        rx1Media?.Dispose();
                        rx1Media = ConfigureMedia(1);
                        videoRx1.MediaPlayer.Play(rx1Media);
                        UpdateLabelVisibility(lab_rx1_nothing, false);
                    }
                    else
                    {
                        videoRx1.MediaPlayer.Stop();
                        ClearVideoOsd(videoOsd1, 0);
                        videoOutputReady[0] = false;
                        UpdateVisibility(nolock_rx1, true);
                        UpdateLabelVisibility(lab_rx1_nothing, true);
                    }
                    break;
                case 2:
                    if (isLocked)
                    {
                        rx2Media?.Dispose();
                        rx2Media = ConfigureMedia(2);
                        videoRx2.MediaPlayer.Play(rx2Media);
                        UpdateLabelVisibility(lab_rx2_nothing, false);
                    }
                    else
                    {
                        videoRx2.MediaPlayer.Stop();
                        ClearVideoOsd(videoOsd2, 1);
                        videoOutputReady[1] = false;
                        UpdateVisibility(nolock_rx2, true);
                        UpdateLabelVisibility(lab_rx2_nothing, true);
                    }
                    break;
                case 3:
                    if (isLocked)
                    {
                        rx3Media?.Dispose();
                        rx3Media = ConfigureMedia(3);
                        videoRx3.MediaPlayer.Play(rx3Media);
                        UpdateLabelVisibility(lab_rx3_nothing, false);
                    }
                    else
                    {
                        videoRx3.MediaPlayer.Stop();
                        ClearVideoOsd(videoOsd3, 2);
                        videoOutputReady[2] = false;
                        UpdateVisibility(nolock_rx3, true);
                        UpdateLabelVisibility(lab_rx3_nothing, true);
                    }
                    break;
                case 4:
                    if (isLocked)
                    {
                        rx4Media?.Dispose();
                        rx4Media = ConfigureMedia(4);
                        videoRx4.MediaPlayer.Play(rx4Media);
                        UpdateLabelVisibility(lab_rx4_nothing, false);
                    }
                    else
                    {
                        videoRx4.MediaPlayer.Stop();
                        ClearVideoOsd(videoOsd4, 3);
                        videoOutputReady[3] = false;
                        UpdateVisibility(nolock_rx4, true);
                        UpdateLabelVisibility(lab_rx4_nothing, true);
                    }
                    break;
            }
        }

        #endregion

        #region OSD Rendering

        private PictureBox CreateOsdPictureBox(PictureBox parent)
        {
            PictureBox osdPic = new PictureBox();
            osdPic.Parent = parent;
            osdPic.BackColor = Color.Transparent;
            osdPic.SizeMode = PictureBoxSizeMode.AutoSize;
            osdPic.Location = new Point(0, 0);
            osdPic.BringToFront();
            return osdPic;
        }

        private PictureBox CreateVideoOsdOverlay(Control videoView)
        {
            PictureBox osdPic = new PictureBox();
            osdPic.Parent = videoView.Parent;
            osdPic.BackColor = Color.Transparent;
            osdPic.SizeMode = PictureBoxSizeMode.AutoSize;
            osdPic.Location = new Point(0, 0);
            osdPic.Visible = false;
            osdPic.BringToFront();
            return osdPic;
        }

        private void UpdateVideoOsd(PictureBox osdPic, string text, int parentWidth, int rxIndex)
        {
            if (osdPic == null) return;

            if (rxIndex >= 0 && rxIndex < 4)
                currentVideoOsdText[rxIndex] = text;

            if (osdPic.InvokeRequired)
            {
                osdPic.Invoke(new Action(() => UpdateVideoOsdInternal(osdPic, text, parentWidth)));
            }
            else
            {
                UpdateVideoOsdInternal(osdPic, text, parentWidth);
            }
        }

        private void UpdateVideoOsdInternal(PictureBox osdPic, string text, int parentWidth)
        {
            if (string.IsNullOrEmpty(text))
            {
                osdPic.Visible = false;
                osdPic.Image?.Dispose();
                osdPic.Image = null;
                return;
            }

            Bitmap osdImage = CreateVideoOsdImage(text, parentWidth);
            osdPic.Image?.Dispose();
            osdPic.Image = osdImage;
            osdPic.Location = new Point((parentWidth - osdImage.Width) / 2, 5);
            osdPic.Visible = true;
            osdPic.BringToFront();
        }

        private Bitmap CreateVideoOsdImage(string text, int maxWidth)
        {
            int padding = 12;
            int availableWidth = maxWidth - padding * 2 - 20;

            float fontSize = 48f;
            float minFontSize = 12f;
            SizeF textSize;

            // Find appropriate font size
            using (Bitmap measureBmp = new Bitmap(1, 1))
            using (Graphics measureG = Graphics.FromImage(measureBmp))
            {
                while (fontSize > minFontSize)
                {
                    using (Font testFont = new Font("Arial", fontSize, FontStyle.Bold))
                    {
                        textSize = measureG.MeasureString(text, testFont);
                        if (textSize.Width <= availableWidth)
                            break;
                    }
                    fontSize -= 2f;
                }
            }

            using (Font font = new Font("Arial", fontSize, FontStyle.Bold))
            {
                using (Bitmap measureBmp = new Bitmap(1, 1))
                using (Graphics measureG = Graphics.FromImage(measureBmp))
                {
                    textSize = measureG.MeasureString(text, font);
                }

                int width = (int)textSize.Width + padding * 2 + 8;
                int height = (int)textSize.Height + padding * 2;
                float outlineWidth = Math.Max(2f, fontSize / 12f);

                Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    // Black background
                    using (SolidBrush bgBrush = new SolidBrush(Color.Black))
                    {
                        g.FillRectangle(bgBrush, 0, 0, width, height);
                    }

                    // White text with black outline
                    using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        path.AddString(text, font.FontFamily, (int)font.Style,
                            g.DpiY * font.Size / 72f, new PointF(padding, padding), StringFormat.GenericDefault);

                        using (Pen outlinePen = new Pen(Color.Black, outlineWidth))
                        {
                            outlinePen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                            g.DrawPath(outlinePen, path);
                        }

                        using (SolidBrush textBrush = new SolidBrush(Color.White))
                        {
                            g.FillPath(textBrush, path);
                        }
                    }
                }
                return bmp;
            }
        }

        private void ClearVideoOsd(PictureBox osdPic, int rxIndex)
        {
            if (osdPic == null) return;

            if (rxIndex >= 0 && rxIndex < 4)
                currentVideoOsdText[rxIndex] = "";

            if (osdPic.InvokeRequired)
            {
                osdPic.Invoke(new Action(() =>
                {
                    osdPic.Visible = false;
                    osdPic.Image?.Dispose();
                    osdPic.Image = null;
                }));
            }
            else
            {
                osdPic.Visible = false;
                osdPic.Image?.Dispose();
                osdPic.Image = null;
            }
        }

        private void SafeUpdateNoLockOsd(PictureBox osdPicTop, PictureBox osdPicBottom, PictureBox parent, string freqSrText, string statusText)
        {
            if (parent == null) return;

            if (parent.InvokeRequired)
            {
                parent.Invoke(new Action(() => UpdateNoLockOsd(osdPicTop, osdPicBottom, parent, freqSrText, statusText)));
            }
            else
            {
                UpdateNoLockOsd(osdPicTop, osdPicBottom, parent, freqSrText, statusText);
            }
        }

        private Bitmap CreateOsdTextImage(string text, Font font)
        {
            if (string.IsNullOrEmpty(text)) return null;

            SizeF textSize;
            using (Bitmap measureBmp = new Bitmap(1, 1))
            using (Graphics measureG = Graphics.FromImage(measureBmp))
            {
                textSize = measureG.MeasureString(text, font);
            }

            int padding = 10;
            int width = (int)textSize.Width + padding * 2 + 8;
            int height = (int)textSize.Height + padding * 2 + 8;

            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.Clear(Color.Transparent);

                float emSize = g.DpiY * font.Size / 72f;
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddString(text, font.FontFamily, (int)font.Style, emSize,
                        new PointF(padding, padding), System.Drawing.StringFormat.GenericDefault);

                    using (Pen outlinePen = new Pen(Color.Black, 4f))
                    {
                        outlinePen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                        g.DrawPath(outlinePen, path);
                    }
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    {
                        g.FillPath(textBrush, path);
                    }
                }
            }
            return bmp;
        }

        private void UpdateNoLockOsd(PictureBox osdPicTop, PictureBox osdPicBottom, PictureBox parent, string freqSrText, string statusText)
        {
            try
            {
                using (Font font = new Font("Arial", 36F, FontStyle.Bold))
                {
                    int imageHeight = parent.Image?.Height ?? 0;
                    int centerY = parent.Height / 2;
                    int imageTop = centerY - imageHeight / 2;
                    int imageBottom = centerY + imageHeight / 2;
                    int lineMargin = 40;

                    // Update top text (frequency + symbol rate)
                    if (osdPicTop != null)
                    {
                        Bitmap topBmp = CreateOsdTextImage(freqSrText, font);
                        if (topBmp != null)
                        {
                            if (osdPicTop.Image != null)
                            {
                                var oldImage = osdPicTop.Image;
                                osdPicTop.Image = null;
                                oldImage.Dispose();
                            }

                            int x = (parent.Width - topBmp.Width) / 2;
                            int y = imageTop - lineMargin - topBmp.Height;
                            if (y < 5) y = 5;
                            osdPicTop.Location = new Point(x, y);
                            osdPicTop.Image = topBmp;
                        }
                    }

                    // Update bottom text (status)
                    if (osdPicBottom != null)
                    {
                        Bitmap bottomBmp = CreateOsdTextImage(statusText, font);
                        if (bottomBmp != null)
                        {
                            if (osdPicBottom.Image != null)
                            {
                                var oldImage = osdPicBottom.Image;
                                osdPicBottom.Image = null;
                                oldImage.Dispose();
                            }

                            int x = (parent.Width - bottomBmp.Width) / 2;
                            int y = imageBottom + lineMargin;
                            if (y + bottomBmp.Height > parent.Height - 5)
                                y = parent.Height - bottomBmp.Height - 5;
                            osdPicBottom.Location = new Point(x, y);
                            osdPicBottom.Image = bottomBmp;
                        }
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Thread-Safe UI Updates

        public static void UpdateVisibility(PictureBox pic, bool visibility)
        {
            if (pic.InvokeRequired)
            {
                UpdateVisibilityDelegate del = new UpdateVisibilityDelegate(UpdateVisibility);
                pic.Invoke(del, new object[] { pic, visibility });
            }
            else
            {
                pic.Visible = visibility;
                if (visibility) pic.BringToFront();
            }
        }

        public static void UpdateLabelVisibility(Label lbl, bool visibility)
        {
            if (lbl.InvokeRequired)
            {
                UpdateLabelVisibilityDelegate del = new UpdateLabelVisibilityDelegate(UpdateLabelVisibility);
                lbl.Invoke(del, new object[] { lbl, visibility });
            }
            else
            {
                lbl.Visible = visibility;
            }
        }

        public static void UpdateLabelColor(Label lb, Color col)
        {
            if (lb.InvokeRequired)
            {
                UpdateLabelColorDelegate del = new UpdateLabelColorDelegate(UpdateLabelColor);
                lb.Invoke(del, new object[] { lb, col });
            }
            else
            {
                lb.ForeColor = col;
            }
        }

        public static void UpdateLabel(Label lb, string text)
        {
            if (lb.InvokeRequired)
            {
                UpdateLabelDelegate del = new UpdateLabelDelegate(UpdateLabel);
                lb.Invoke(del, new object[] { lb, text });
            }
            else
            {
                lb.Text = text;
            }
        }

        private void SetNoSignalVisible(PictureBox noSignal, bool visible)
        {
            if (noSignal.InvokeRequired)
            {
                noSignal.Invoke(new Action(() =>
                {
                    noSignal.Visible = visible;
                    if (visible) noSignal.BringToFront();
                }));
            }
            else
            {
                noSignal.Visible = visible;
                if (visible) noSignal.BringToFront();
            }
        }

        private void Debug(string msg)
        {
            settingsForm?.AddDebugMessage(msg);
        }

        #endregion

        #region Symbol Rate Hopping

        private void SrHopTimer_Tick(object sender, EventArgs e)
        {
            for (int rx = 1; rx <= 4; rx++)
            {
                int idx = rx - 1;
                int[] rates = rxSymbolRates[idx];

                // Skip if less than 2 symbol rates enabled
                if (rates == null || rates.Length < 2)
                {
                    statusForm?.UnfreezeDisplay(rx);
                    continue;
                }

                int state = prevStates[idx];

                // Don't hop when lock is imminent or achieved
                bool lockImminent = (state == 1 || state == 2 || state == 3);
                if (lockImminent)
                {
                    statusForm?.UnfreezeDisplay(rx);
                    continue;
                }

                string freqStr = statusForm?.GetFrequency(rx);
                if (string.IsNullOrEmpty(freqStr))
                    continue;

                // Freeze display before hopping
                statusForm?.FreezeDisplay(rx);

                // Hop to next symbol rate
                rxCurrentSrIndex[idx] = (rxCurrentSrIndex[idx] + 1) % rates.Length;
                int newSr = rates[rxCurrentSrIndex[idx]];

                if (double.TryParse(freqStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double freqMHz))
                {
                    int freqKHz = (int)(freqMHz * 1000);
                    Debug($"SR Hop RX{rx}: {newSr} kS/s");
                    SetFrequency(rx - 1, freqKHz, newSr);
                }
            }
        }

        private void HandleSymbolRatesChange(int rx, int[] rates)
        {
            if (rx < 1 || rx > 4) return;

            int idx = rx - 1;
            rxSymbolRates[idx] = rates;
            rxCurrentSrIndex[idx] = 0;

            // Save to settings
            string ratesStr = string.Join(",", rates);
            switch (rx)
            {
                case 1: iniSettings.Rx1SymbolRates = ratesStr; break;
                case 2: iniSettings.Rx2SymbolRates = ratesStr; break;
                case 3: iniSettings.Rx3SymbolRates = ratesStr; break;
                case 4: iniSettings.Rx4SymbolRates = ratesStr; break;
            }
            iniSettings.Save();

            // If exactly 1 SR selected, send it immediately
            if (rates.Length == 1)
            {
                string freqStr = statusForm?.GetFrequency(rx);
                if (!string.IsNullOrEmpty(freqStr))
                {
                    if (double.TryParse(freqStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double freqMHz))
                    {
                        int freqKHz = (int)(freqMHz * 1000);
                        Debug($"Single SR selected RX{rx}: {rates[0]} kS/s");
                        SetFrequency(rx - 1, freqKHz, rates[0]);
                    }
                }

                statusForm?.UnfreezeDisplay(rx);
            }
        }

        #endregion

        #region Auto Zoom

        private void AutoZoomTimer_Tick(object sender, EventArgs e)
        {
            if (!autoZoomEnabled) return;

            // Count locked receivers
            int lockedCount = 0;
            int lockedRx = 0;
            for (int i = 0; i < 4; i++)
            {
                if (prevStates[i] == 2 || prevStates[i] == 3)
                {
                    lockedCount++;
                    lockedRx = i + 1;
                }
            }

            // Zoom to single receiver, or show 4-way if 0 or 2+ locked
            int targetRx = (lockedCount == 1) ? lockedRx : 0;

            if (targetRx != autoZoomTargetRx)
            {
                if (targetRx != autoZoomPendingRx)
                {
                    autoZoomPendingRx = targetRx;
                    autoZoomPendingTime = DateTime.Now;
                }
                else
                {
                    if ((DateTime.Now - autoZoomPendingTime).TotalMilliseconds >= AUTO_ZOOM_DELAY_MS)
                    {
                        ApplyAutoZoom(targetRx);
                        autoZoomTargetRx = targetRx;
                    }
                }
            }
            else
            {
                autoZoomPendingRx = targetRx;
            }
        }

        private void ApplyAutoZoom(int rx)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ApplyAutoZoom(rx)));
                return;
            }

            if (rx == 0)
            {
                if (isFullScreen)
                {
                    ResetToQuadView();
                    isFullScreen = false;
                }
            }
            else
            {
                if (!isFullScreen)
                {
                    SwitchToSingleView(rx);
                }
                else
                {
                    ResetToQuadView();
                    isFullScreen = false;
                    SwitchToSingleView(rx);
                }
            }
        }

        private void HandleAutoZoomChange(bool enabled)
        {
            autoZoomEnabled = enabled;
            iniSettings.AutoZoom = enabled;
            iniSettings.Save();

            if (!enabled)
            {
                autoZoomPendingRx = 0;
                if (autoZoomTargetRx != 0)
                {
                    autoZoomTargetRx = 0;
                    if (isFullScreen)
                    {
                        ResetToQuadView();
                        isFullScreen = false;
                    }
                }
            }
        }

        #endregion

        #region View Switching

        private void ResetToQuadView()
        {
            MiddleVideoSplitter.Panel1Collapsed = false;
            MiddleVideoSplitter.Panel2Collapsed = false;
            LeftVideoSplitter.Panel1Collapsed = false;
            LeftVideoSplitter.Panel2Collapsed = false;
            RightVideoSplitter.Panel1Collapsed = false;
            RightVideoSplitter.Panel2Collapsed = false;

            MiddleVideoSplitter.Panel1.Show();
            MiddleVideoSplitter.Panel2.Show();
            LeftVideoSplitter.Panel1.Show();
            LeftVideoSplitter.Panel2.Show();
            RightVideoSplitter.Panel1.Show();
            RightVideoSplitter.Panel2.Show();
        }

        private void SwitchToSingleView(int rx)
        {
            if (isFullScreen)
            {
                ResetToQuadView();
                isFullScreen = false;
            }
            else
            {
                isFullScreen = true;
                switch (rx)
                {
                    case 1:
                        MiddleVideoSplitter.Panel2Collapsed = true;
                        MiddleVideoSplitter.Panel2.Hide();
                        LeftVideoSplitter.Panel2Collapsed = true;
                        LeftVideoSplitter.Panel2.Hide();
                        break;
                    case 2:
                        MiddleVideoSplitter.Panel1Collapsed = true;
                        MiddleVideoSplitter.Panel1.Hide();
                        RightVideoSplitter.Panel2Collapsed = true;
                        RightVideoSplitter.Panel2.Hide();
                        break;
                    case 3:
                        MiddleVideoSplitter.Panel2Collapsed = true;
                        MiddleVideoSplitter.Panel2.Hide();
                        LeftVideoSplitter.Panel1Collapsed = true;
                        LeftVideoSplitter.Panel1.Hide();
                        break;
                    case 4:
                        MiddleVideoSplitter.Panel1Collapsed = true;
                        MiddleVideoSplitter.Panel1.Hide();
                        RightVideoSplitter.Panel1Collapsed = true;
                        RightVideoSplitter.Panel1.Hide();
                        break;
                }
            }
        }

        // Double-click handlers for switching views
        private void nolock_rx1_DoubleClick(object sender, EventArgs e) { SwitchToSingleView(1); }
        private void videoRx1_DoubleClick(object sender, EventArgs e) { SwitchToSingleView(1); }
        private void nolock_rx2_DoubleClick(object sender, EventArgs e) { SwitchToSingleView(2); }
        private void videoRx2_DoubleClick(object sender, EventArgs e) { SwitchToSingleView(2); }
        private void nolock_rx3_DoubleClick(object sender, EventArgs e) { SwitchToSingleView(3); }
        private void videoRx3_DoubleClick(object sender, EventArgs e) { SwitchToSingleView(3); }
        private void nolock_rx4_DoubleClick(object sender, EventArgs e) { SwitchToSingleView(4); }
        private void videoRx4_DoubleClick(object sender, EventArgs e) { SwitchToSingleView(4); }

        #endregion

        #region Event Handlers

        private void HandleForceTS(int rx)
        {
            if (rx < 0)
            {
                // Negative rx means reset TS
                int absRx = Math.Abs(rx);
                controlWS.Send($"U{absRx},127.0.0.1");
                if (absRx >= 1 && absRx <= 4)
                {
                    prevStates[absRx - 1] = -1;
                    statusForm.ResetPrevState(absRx);
                }
            }
            else
            {
                controlWS.Send($"U{rx},{localIP}");
                if (rx >= 1 && rx <= 4)
                {
                    prevStates[rx - 1] = -1;
                    statusForm.ResetPrevState(rx);
                }
            }
        }

        private void HandleVolumeChange(int rx, int volume)
        {
            lock (volumeLock)
            {
                switch (rx)
                {
                    case 1:
                        rx1Volume = volume;
                        iniSettings.Rx1Volume = volume;
                        if (videoRx1.MediaPlayer != null)
                            videoRx1.MediaPlayer.Volume = rx1Volume;
                        break;
                    case 2:
                        rx2Volume = volume;
                        iniSettings.Rx2Volume = volume;
                        if (videoRx2.MediaPlayer != null)
                            videoRx2.MediaPlayer.Volume = rx2Volume;
                        break;
                    case 3:
                        rx3Volume = volume;
                        iniSettings.Rx3Volume = volume;
                        if (videoRx3.MediaPlayer != null)
                            videoRx3.MediaPlayer.Volume = rx3Volume;
                        break;
                    case 4:
                        rx4Volume = volume;
                        iniSettings.Rx4Volume = volume;
                        if (videoRx4.MediaPlayer != null)
                            videoRx4.MediaPlayer.Volume = rx4Volume;
                        break;
                }
                iniSettings.Save();
            }
        }

        private void ShowSettingsForm()
        {
            settingsForm.Show();
            settingsForm.BringToFront();
        }

        private void ShowFrequencySelectDialog(int rx)
        {
            freq_select freqSet = new freq_select();
            freqSet.lblCurRx.Text = rx.ToString();

            switch (rx)
            {
                case 1: freqSet.lblCurOffset.Text = rx1Offset.ToString(); break;
                case 2: freqSet.lblCurOffset.Text = rx2Offset.ToString(); break;
                case 3: freqSet.lblCurOffset.Text = rx3Offset.ToString(); break;
                case 4: freqSet.lblCurOffset.Text = rx4Offset.ToString(); break;
            }

            string freqText = statusForm.GetFrequency(rx);
            if (double.TryParse(freqText, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double freq))
            {
                freqSet.txtNewFrequency.Text = Math.Round(freq * 1000).ToString();
            }

            if (freqSet.ShowDialog() != DialogResult.Cancel)
            {
                try
                {
                    int newFreq = int.Parse(freqSet.txtNewFrequency.Text);
                    int newSR = int.Parse(freqSet.comboNewSR.SelectedItem.ToString());
                    SetFrequency(rx - 1, newFreq, newSR);
                }
                catch (Exception ex)
                {
                    Debug("Something went wrong:\n" + ex.Message);
                }
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                statusForm?.ShowStatusForm();
                e.Handled = true;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save window state
            iniSettings.WindowMaximized = (this.WindowState == FormWindowState.Maximized);

            if (this.WindowState == FormWindowState.Normal)
            {
                iniSettings.WindowWidth = this.Width;
                iniSettings.WindowHeight = this.Height;
                iniSettings.WindowX = this.Left;
                iniSettings.WindowY = this.Top;
            }
            else if (this.WindowState == FormWindowState.Maximized)
            {
                iniSettings.WindowWidth = this.RestoreBounds.Width;
                iniSettings.WindowHeight = this.RestoreBounds.Height;
                iniSettings.WindowX = this.RestoreBounds.X;
                iniSettings.WindowY = this.RestoreBounds.Y;
            }
            iniSettings.Save();

            statusForm?.ForceClose();
            settingsForm?.ForceClose();
        }

        #endregion

        #region Title Bar Management

        private void TitleBarTimer_Tick(object sender, EventArgs e)
        {
            if (!this.IsHandleCreated || !this.Visible)
                return;

            Point screenCursor = Cursor.Position;
            Rectangle formBounds = this.Bounds;

            bool nearTop = screenCursor.X >= formBounds.Left &&
                           screenCursor.X <= formBounds.Right &&
                           screenCursor.Y >= formBounds.Top &&
                           screenCursor.Y <= formBounds.Top + TITLE_BAR_SHOW_ZONE;

            bool overTitleBar = titleBarVisible &&
                                screenCursor.X >= formBounds.Left &&
                                screenCursor.X <= formBounds.Right &&
                                screenCursor.Y >= formBounds.Top &&
                                screenCursor.Y <= formBounds.Top + titleBarPanel.Height;

            if (nearTop || overTitleBar)
            {
                if (!titleBarVisible)
                {
                    titleBarVisible = true;
                    titleBarPanel.Visible = true;
                    titleBarPanel.BringToFront();
                }
                titleBarLastActivity = DateTime.Now;
            }
            else if (titleBarVisible)
            {
                if ((DateTime.Now - titleBarLastActivity).TotalMilliseconds > TITLE_BAR_HIDE_DELAY)
                {
                    titleBarVisible = false;
                    titleBarPanel.Visible = false;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                titleBarVisible = false;
                titleBarPanel.Visible = false;
            }
            else
            {
                this.MaximizedBounds = Screen.FromControl(this).WorkingArea;
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnClose_MouseEnter(object sender, EventArgs e)
        {
            btnClose.BackColor = Color.FromArgb(232, 17, 35);
        }

        private void btnClose_MouseLeave(object sender, EventArgs e)
        {
            btnClose.BackColor = Color.Transparent;
        }

        private void titleBarPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
            }
        }

        private void titleBarPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    double ratio = (double)e.X / titleBarPanel.Width;
                    this.WindowState = FormWindowState.Normal;
                    dragStartPoint = new Point((int)(this.Width * ratio), dragStartPoint.Y);
                    titleBarVisible = false;
                    titleBarPanel.Visible = false;
                    return;
                }

                Point newLocation = this.Location;
                newLocation.X += e.X - dragStartPoint.X;
                newLocation.Y += e.Y - dragStartPoint.Y;
                this.Location = newLocation;
            }
        }

        private void titleBarPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void titleBarPanel_DoubleClick(object sender, EventArgs e)
        {
            btnMaximize_Click(sender, e);
        }

        #endregion

        #region Window Resizing (Borderless)

        protected override void WndProc(ref Message m)
        {
            // Handle borderless window resizing
            if (m.Msg == WM_NCHITTEST && this.WindowState == FormWindowState.Normal)
            {
                Point cursor = this.PointToClient(Cursor.Position);

                // Check corners first
                if (cursor.X <= RESIZE_BORDER && cursor.Y <= RESIZE_BORDER)
                { m.Result = (IntPtr)HTTOPLEFT; return; }
                if (cursor.X >= this.Width - RESIZE_BORDER && cursor.Y <= RESIZE_BORDER)
                { m.Result = (IntPtr)HTTOPRIGHT; return; }
                if (cursor.X <= RESIZE_BORDER && cursor.Y >= this.Height - RESIZE_BORDER)
                { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                if (cursor.X >= this.Width - RESIZE_BORDER && cursor.Y >= this.Height - RESIZE_BORDER)
                { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }

                // Then check edges
                if (cursor.X <= RESIZE_BORDER)
                { m.Result = (IntPtr)HTLEFT; return; }
                if (cursor.X >= this.Width - RESIZE_BORDER)
                { m.Result = (IntPtr)HTRIGHT; return; }
                if (cursor.Y <= RESIZE_BORDER)
                { m.Result = (IntPtr)HTTOP; return; }
                if (cursor.Y >= this.Height - RESIZE_BORDER)
                { m.Result = (IntPtr)HTBOTTOM; return; }
            }

            // Maintain 16:9 aspect ratio while resizing
            if (m.Msg == WM_SIZING)
            {
                RECT rect = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));

                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                int clientWidth = width;
                int clientHeight = height;

                switch ((int)m.WParam)
                {
                    case WMSZ_LEFT:
                    case WMSZ_RIGHT:
                        clientHeight = (int)(clientWidth / ASPECT_RATIO);
                        rect.Bottom = rect.Top + clientHeight;
                        break;

                    case WMSZ_TOP:
                    case WMSZ_BOTTOM:
                        clientWidth = (int)(clientHeight * ASPECT_RATIO);
                        rect.Right = rect.Left + clientWidth;
                        break;

                    case WMSZ_TOPLEFT:
                    case WMSZ_TOPRIGHT:
                        clientHeight = (int)(clientWidth / ASPECT_RATIO);
                        rect.Top = rect.Bottom - clientHeight;
                        break;

                    case WMSZ_BOTTOMLEFT:
                    case WMSZ_BOTTOMRIGHT:
                        clientHeight = (int)(clientWidth / ASPECT_RATIO);
                        rect.Bottom = rect.Top + clientHeight;
                        break;
                }

                Marshal.StructureToPtr(rect, m.LParam, true);
            }

            base.WndProc(ref m);
        }

        #endregion
    }
}
