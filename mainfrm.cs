using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebSocketSharp;
using LibVLCSharp.Shared;
using System.Net;
using System.Net.Sockets;

namespace datvreceiver
{
    public partial class mainForm : Form
    {

        LibVLC libVLC1 = new LibVLC("--aout=directsound");
        LibVLC libVLC2 = new LibVLC("--aout=directsound");
        LibVLC libVLC3 = new LibVLC("--aout=directsound");
        LibVLC libVLC4 = new LibVLC("--aout=directsound");

        Media rx1Media; // 9941
        Media rx2Media;
        Media rx3Media;
        Media rx4Media;

        int rx1PrevState = -1;
        int rx2PrevState = -1;
        int rx3PrevState = -1;
        int rx4PrevState = -1;

        string winterhill_host = "";
        int winterhill_baseport = 0;
        bool forceLocalIP = false;
        string localip;
        string snapShotPath = "";

        int rx1Offset = 0;
        int rx2Offset = 0;
        int rx3Offset = 0;
        int rx4Offset = 0;

        bool enableSpectrum = false;
        bool enableChat = false;
        bool minPropertiesAtStartup = false;

        private Object _volumelock = new Object();
        int rx1Volume = 50;
        int rx2Volume = 50;
        int rx3Volume = 50;
        int rx4Volume = 50;

        private wbchat chatForm;


        // constants
        Dictionary<int, string> scanstate_lookup = new Dictionary<int, string>()
        {
            { 0 , "Hunting" },
            { 1 , "Header" },
            { 2 , "Lock DVB-S2" },
            { 3 , "Lock DVB-S" },
            { 0x80 , "Lost" },
            { 0x81 , "Timeout" },
            { 0x82 , "Idle" },
        };


        // variables
        private WebSocket monitorWS;        
        private WebSocket controlWS;

        private delegate void UpdateVisibilityDelegate(PictureBox pic,bool visible);
        private delegate void UpdateLabelVisibilityDelegate(Label lbl, bool visible);
        private delegate void UpdateLBDelegate(ListBox LB, Object obj);
        private delegate void UpdateLabelDelegate(Label lb, string text);
        private delegate void UpdateLBColorDelegate(Label LB, Color col);

        private delegate int GetValueDelegate(TrackBar track);

        SpectrumForm specForm;

        public void setFreq(int rx, int freq, int sr)
        {
            debug("Set Freq: " + (rx + 1).ToString() + "," + freq + " - " + sr);

            int ifOffset = 0;

            switch(rx)
            {
                case 0: ifOffset = rx1Offset; break;
                case 1: ifOffset = rx2Offset; break;
                case 2: ifOffset = rx3Offset; break;
                case 3: ifOffset = rx4Offset; break;
            }

            controlWS.Send("F" + (rx + 1).ToString() + "," + freq + "," + sr.ToString() + "," + ifOffset);

        }

        public static int GetTrackValue(TrackBar track)
        {
            if (track.InvokeRequired)
            {
                GetValueDelegate ulb = new GetValueDelegate(GetTrackValue);
                return (int)track.Invoke(ulb, new object[] { track });
            }
            else
            {
                return track.Value;
            }
        }


        public static void UpdateVisiblity(PictureBox pic, bool visibility)
        {
            if (pic.InvokeRequired)
            {
                UpdateVisibilityDelegate ulb = new UpdateVisibilityDelegate(UpdateVisiblity);
                pic.Invoke(ulb, new object[] { pic, visibility });
            }
            else
            {
                pic.Visible = visibility;
            }
        }

        public static void UpdateLabelVisiblity(Label lbl, bool visibility)
        {
            if (lbl.InvokeRequired)
            {
                UpdateLabelVisibilityDelegate ulb = new UpdateLabelVisibilityDelegate(UpdateLabelVisiblity);
                lbl.Invoke(ulb, new object[] { lbl, visibility });
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
                UpdateLBColorDelegate ulb = new UpdateLBColorDelegate(UpdateLabelColor);
                lb.Invoke(ulb, new object[] { lb, col });
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
                UpdateLabelDelegate ulb = new UpdateLabelDelegate(UpdateLabel);
                lb.Invoke(ulb, new object[] { lb, text });
            }
            else
            {
                lb.Text = text;
            }
        }

        private void loadSettings()
        {
            Properties.Settings.Default.Reload();

            winterhill_host = Properties.Settings.Default.winterhill_host;
            winterhill_baseport = Properties.Settings.Default.winterhill_baseport;
            forceLocalIP = Properties.Settings.Default.force_local_ip;
            localip = Properties.Settings.Default.force_local_ip_ip;
            snapShotPath = Properties.Settings.Default.snapshot_path;

            rx1Offset = Properties.Settings.Default.rx1_offset;
            rx2Offset = Properties.Settings.Default.rx2_offset;
            rx3Offset = Properties.Settings.Default.rx3_offset;
            rx4Offset = Properties.Settings.Default.rx4_offset;

            enableSpectrum = Properties.Settings.Default.enable_qo100_spectrum;
            enableChat = Properties.Settings.Default.enable_qo100_chat;
            minPropertiesAtStartup = Properties.Settings.Default.minimize_properties;
            txtWinterhillHost.Text = winterhill_host;
            txtWinterhillBasePort.Text = winterhill_baseport.ToString();
            checkForceLocalIP.Checked = forceLocalIP;
            txtSnapshotPath.Text = snapShotPath;
            txtLocalIP.Text = localip;
            txtRX1Offset.Text = rx1Offset.ToString();
            txtRX2Offset.Text = rx2Offset.ToString();
            txtRX3Offset.Text = rx3Offset.ToString();
            txtRX4Offset.Text = rx4Offset.ToString();
            checkEnableSpectrum.Checked = enableSpectrum;
            checkEnableChat.Checked = enableChat;
            checkMinProperties.Checked = minPropertiesAtStartup;




        }

        private Media configureMedia(int rx)
        {
            Media media;

            int port = winterhill_baseport + 40 + rx;

            debug("udp://@:" + port.ToString());
            switch (rx)
            {
                case 1:  media = new Media(libVLC1, "udp://@:" + port.ToString(), FromType.FromLocation); break;
                case 2:  media = new Media(libVLC2, "udp://@:" + port.ToString(), FromType.FromLocation); break;
                case 3:  media = new Media(libVLC3, "udp://@:" + port.ToString(), FromType.FromLocation); break;
                case 4:  media = new Media(libVLC4, "udp://@:" + port.ToString(), FromType.FromLocation); break;
                default: media = new Media(libVLC1, "udp://@:" + port.ToString(), FromType.FromLocation); break;
            }

            MediaConfiguration mediaConfig = new MediaConfiguration();
            mediaConfig.EnableHardwareDecoding = false;
            media.AddOption(mediaConfig);

            return media;
        }

        private void configureVideo()
        {
            rx1Media = configureMedia(1);

            videoRx1.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC1);
            videoRx1.MediaPlayer.Stopped += rx1_MediaPlayer_Stopped;
            videoRx1.MediaPlayer.Playing += rx1_MediaPlayer_Playing;
            videoRx1.MediaPlayer.TimeChanged += rx1_MediaPlayer_TimeChanged;
            videoRx1.MediaPlayer.EnableMouseInput = false;
            videoRx1.MediaPlayer.EnableKeyInput = false;

            rx2Media = configureMedia(2);

            videoRx2.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC2);
            videoRx2.MediaPlayer.Stopped += rx2_MediaPlayer_Stopped;
            videoRx2.MediaPlayer.Playing += rx2_MediaPlayer_Playing;
            videoRx2.MediaPlayer.TimeChanged += rx2_MediaPlayer_TimeChanged;

            videoRx2.MediaPlayer.EnableMouseInput = false;
            videoRx2.MediaPlayer.EnableKeyInput = false;

            rx3Media = configureMedia(3);

            videoRx3.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC3);
            videoRx3.MediaPlayer.Stopped += rx3_MediaPlayer_Stopped;
            videoRx3.MediaPlayer.Playing += rx3_MediaPlayer_Playing;
            videoRx3.MediaPlayer.TimeChanged += rx3_MediaPlayer_TimeChanged;
            videoRx3.MediaPlayer.EnableMouseInput = false;
            videoRx3.MediaPlayer.EnableKeyInput = false;

            rx4Media = configureMedia(4);

            videoRx4.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVLC4);
            videoRx4.MediaPlayer.Stopped += rx4_MediaPlayer_Stopped;
            videoRx4.MediaPlayer.Playing += rx4_MediaPlayer_Playing;
            videoRx4.MediaPlayer.TimeChanged += rx4_MediaPlayer_TimeChanged;
            videoRx4.MediaPlayer.EnableMouseInput = false;
            videoRx4.MediaPlayer.EnableKeyInput = false;

        }

        private void rx4_MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            lock (_volumelock)
            {
                videoRx4.MediaPlayer.Volume = rx4Volume;
            }
        }

        private void rx3_MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            lock (_volumelock)
            {
                videoRx3.MediaPlayer.Volume = rx3Volume;
            }
        }

        private void rx2_MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            lock (_volumelock)
            {
                videoRx2.MediaPlayer.Volume = rx2Volume;
            }
        }

        private void rx1_MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            lock (_volumelock)
            {
                videoRx1.MediaPlayer.Volume = rx1Volume;
            }
        }

        private void rx1_MediaPlayer_Playing(object sender, EventArgs e)
        {
            debug("rx1 playing");
        }

        private void rx1_MediaPlayer_Stopped(object sender, EventArgs e)
        {
            debug("rx1 stopped");
        }

        private void rx2_MediaPlayer_Playing(object sender, EventArgs e)
        {
            debug("rx2 playing");
        }

        private void rx2_MediaPlayer_Stopped(object sender, EventArgs e)
        {
            debug("rx2 stopped");
        }

        private void rx3_MediaPlayer_Playing(object sender, EventArgs e)
        {
            debug("rx3 playing");
        }

        private void rx3_MediaPlayer_Stopped(object sender, EventArgs e)
        {
            debug("rx3 stopped");
        }

        private void rx4_MediaPlayer_Playing(object sender, EventArgs e)
        {
            debug("rx4 playing");
        }

        private void rx4_MediaPlayer_Stopped(object sender, EventArgs e)
        {
            debug("rx4 stopped");
        }


        public static void UpdateLB(ListBox LB, Object obj)
        {
            if (LB.InvokeRequired)
            {
                UpdateLBDelegate ulb = new UpdateLBDelegate(UpdateLB);
                LB.Invoke(ulb, new object[] { LB, obj });
            }
            else
            {
                if (LB.Items.Count > 1000)
                {
                    LB.Items.Remove(0);
                }

                int i = LB.Items.Add(DateTime.Now.ToShortTimeString() + " : " + obj);
                LB.TopIndex = i;
            }

        }
        private void debug(string msg)
        {
            UpdateLB(dbgListBox, msg);
        }

        private void connectWebSockets()
        {

            string host = winterhill_host;

            if (host.Length == 0)
            {
                debug("Error: No Winterhill Host Specified");
                return;
            }

            int port = 8080;

            string url = "ws://" + host + ":" + port.ToString() + "/ ";

            monitorWS = new WebSocket(url, "monitor");
            monitorWS.OnOpen += MonitorWS_OnOpen;
            monitorWS.OnMessage += MonitorWS_OnMessage;
            monitorWS.OnClose += MonitorWS_OnClose;
            monitorWS.ConnectAsync();

            controlWS = new WebSocket(url, "control");
            controlWS.OnClose += ControlWS_OnClose;
            controlWS.OnMessage += ControlWS_OnMessage;
            controlWS.OnOpen += ControlWS_OnOpen;
            controlWS.ConnectAsync();
        }

        private void ControlWS_OnOpen(object sender, EventArgs e)
        {
            debug("Control WS Open");
        }

        private void ControlWS_OnMessage(object sender, MessageEventArgs e)
        {
        }

        private void ControlWS_OnClose(object sender, CloseEventArgs e)
        {
            debug("Control WS Closed");
            controlWS.Connect();
        }

        private void MonitorWS_OnClose(object sender, CloseEventArgs e)
        {
            debug("Monitor WS Closed");
            monitorWS.Connect();
        }

        private void MonitorWS_OnMessage(object sender, MessageEventArgs e)
        {
            monitorMessage mm = JsonConvert.DeserializeObject<monitorMessage>(e.Data);
            updateInfo(mm);
        }

        private void TakeSnapshot(int rx)
        {
            // get path
            string path = snapShotPath;

            string filename = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".png";

            switch(rx)
            {
                case 1: if (lab_rx1_service.Text.Length > 0) { filename = lab_rx1_service.Text.ToString() + "_" + filename; } break;
                case 2: if (lab_rx2_service.Text.Length > 0) { filename = lab_rx2_service.Text.ToString() + "_" + filename; } break;
                case 3: if (lab_rx3_service.Text.Length > 0) { filename = lab_rx3_service.Text.ToString() + "_" + filename; } break;
                case 4: if (lab_rx4_service.Text.Length > 0) { filename = lab_rx4_service.Text.ToString() + "_" + filename; } break;
            }

            // remove any possible spaces
            filename = filename.Replace(" ", "");

            debug("snapshot: " + rx.ToString() + " - " + filename);

            switch (rx)
            {
                case 1: videoRx1.MediaPlayer.TakeSnapshot(0, path + "\\" + filename, 0, 0); break;
                case 2: videoRx2.MediaPlayer.TakeSnapshot(0, path + "\\" + filename, 0, 0); break;
                case 3: videoRx3.MediaPlayer.TakeSnapshot(0, path + "\\" + filename, 0, 0); break;
                case 4: videoRx4.MediaPlayer.TakeSnapshot(0, path + "\\" + filename, 0, 0); break;
            }
        }

        private void updateInfo(monitorMessage mm)
        {
            string statusMsg = "";

            for (int x = 1; x < mm.rx.Length; x++)
            {
                if (mm.rx[x].rx == 1 )
                {
                    int newState = mm.rx[x].scanstate;

                    if ( newState != rx1PrevState )
                    {
                        if (newState == 2 || newState == 3)
                        {
                            videoRx1.MediaPlayer.Play(rx1Media);
                            UpdateLabelColor(lab_rx1_scanstate, Color.Green);
                            UpdateVisiblity(nolock_rx1, false);
                            UpdateLabelVisiblity(lab_rx1_nothing, false);

                        }
                        else
                        {
                            videoRx1.MediaPlayer.Stop();
                            UpdateLabelColor(lab_rx1_scanstate, Color.Red);
                            UpdateVisiblity(nolock_rx1, true);
                            UpdateLabelVisiblity(lab_rx1_nothing, true);
                        }

                        rx1PrevState = newState;
                    }

                    UpdateLabel(lab_rx1_scanstate, scanstate_lookup[mm.rx[x].scanstate].ToString());
                    UpdateLabel(lab_rx1_service, mm.rx[x].service_name.ToString());
                    UpdateLabel(lab_rx1_provider, mm.rx[x].service_provider_name.ToString());
                    UpdateLabel(lab_rx1_mer, mm.rx[x].mer.ToString());
                    UpdateLabel(lab_rx1_dbmargin, mm.rx[x].dbmargin.ToString());
                    UpdateLabel(lab_rx1_frequency, mm.rx[x].frequency.ToString());
                    UpdateLabel(lab_rx1_sr, mm.rx[x].symbol_rate.ToString());
                    UpdateLabel(lab_rx1_nothing, mm.rx[x].frequency.ToString() + " MHz - " + mm.rx[x].symbol_rate.ToString() + " Ks");
                    UpdateLabel(lab_rx1_mode, mm.rx[x].modcod.ToString());
                    UpdateLabel(lab_rx1_null, mm.rx[x].null_percentage.ToString());
                    UpdateLabel(lab_rx1_mediatypes, mm.rx[x].video_type.ToString() + "-" + mm.rx[x].audio_type.ToString());

                    UpdateLabel(lab_rx1_tsaddr, mm.rx[x].ts_addr.ToString());

                    if ( mm.rx[x].ts_addr.ToString() != localip )
                    {
                        UpdateLabelColor(lab_rx1_tsaddr, Color.Red);
                    }
                    else
                    {
                        UpdateLabelColor(lab_rx1_tsaddr, Color.Green);
                    }

                    statusMsg += "RX1: " + scanstate_lookup[mm.rx[x].scanstate].ToString();

                    if (mm.rx[x].scanstate == 2 || mm.rx[x].scanstate == 3)
                    {
                        statusMsg += " - " + mm.rx[x].service_name.ToString() + " " + mm.rx[x].mer.ToString() + " dB Mer," + "D" + mm.rx[x].dbmargin.ToString();
                    }

                }
                if (mm.rx[x].rx == 2)
                {

                    int newState = mm.rx[x].scanstate;

                    if (newState != rx2PrevState)
                    {
                        if (newState == 2 || newState == 3)
                        {
                            videoRx2.MediaPlayer.Play(rx2Media);
                            UpdateLabelColor(lab_rx2_scanstate, Color.Green);
                            UpdateVisiblity(nolock_rx2, false);
                            UpdateLabelVisiblity(lab_rx2_nothing, false);

                        }
                        else
                        {
                            videoRx2.MediaPlayer.Stop();
                            UpdateLabelColor(lab_rx2_scanstate, Color.Red);
                            UpdateVisiblity(nolock_rx2, true);
                            UpdateLabelVisiblity(lab_rx2_nothing, true);
                        }

                        rx2PrevState = newState;
                    }

                    UpdateLabel(lab_rx2_scanstate, scanstate_lookup[mm.rx[x].scanstate].ToString());
                    UpdateLabel(lab_rx2_service, mm.rx[x].service_name.ToString());
                    UpdateLabel(lab_rx2_provider, mm.rx[x].service_provider_name.ToString());
                    UpdateLabel(lab_rx2_mer, mm.rx[x].mer.ToString());
                    UpdateLabel(lab_rx2_dbmargin, mm.rx[x].dbmargin.ToString());
                    UpdateLabel(lab_rx2_frequency, mm.rx[x].frequency.ToString());
                    UpdateLabel(lab_rx2_sr, mm.rx[x].symbol_rate.ToString());
                    UpdateLabel(lab_rx2_nothing, mm.rx[x].frequency.ToString() + " MHz - " + mm.rx[x].symbol_rate.ToString() + " Ks");
                    UpdateLabel(lab_rx2_mode, mm.rx[x].modcod.ToString());
                    UpdateLabel(lab_rx2_null, mm.rx[x].null_percentage.ToString());
                    UpdateLabel(lab_rx2_mediatypes, mm.rx[x].video_type.ToString() + "-" + mm.rx[x].audio_type.ToString());
                    UpdateLabel(lab_rx2_tsaddr, mm.rx[x].ts_addr.ToString());

                    if (mm.rx[x].ts_addr.ToString() != localip)
                    {
                        UpdateLabelColor(lab_rx2_tsaddr, Color.Red);
                    }
                    else
                    {
                        UpdateLabelColor(lab_rx2_tsaddr, Color.Green);
                    }

                    statusMsg += " - RX2: " + scanstate_lookup[mm.rx[x].scanstate].ToString();

                    if (mm.rx[x].scanstate == 2 || mm.rx[x].scanstate == 3)
                    {
                        statusMsg += " - " + mm.rx[x].service_name.ToString() + " " + mm.rx[x].mer.ToString() + " dB Mer," + "D" + mm.rx[x].dbmargin.ToString();
                    }

                }

                if (mm.rx[x].rx == 3)
                {
                    int newState = mm.rx[x].scanstate;

                    if (newState != rx3PrevState)
                    {
                        if (newState == 2 || newState == 3)
                        {
                            videoRx3.MediaPlayer.Play(rx3Media);
                            UpdateLabelColor(lab_rx3_scanstate, Color.Green);
                            UpdateVisiblity(nolock_rx3, false);
                            UpdateLabelVisiblity(lab_rx3_nothing, false);

                        }
                        else
                        {
                            videoRx3.MediaPlayer.Stop();
                            UpdateLabelColor(lab_rx3_scanstate, Color.Red);
                            UpdateVisiblity(nolock_rx3, true);
                            UpdateLabelVisiblity(lab_rx3_nothing, true);

                        }

                        rx3PrevState = newState;
                    }

                    UpdateLabel(lab_rx3_scanstate, scanstate_lookup[mm.rx[x].scanstate].ToString());
                    UpdateLabel(lab_rx3_service, mm.rx[x].service_name.ToString());
                    UpdateLabel(lab_rx3_provider, mm.rx[x].service_provider_name.ToString());
                    UpdateLabel(lab_rx3_mer, mm.rx[x].mer.ToString());
                    UpdateLabel(lab_rx3_dbmargin, mm.rx[x].dbmargin.ToString());
                    UpdateLabel(lab_rx3_frequency, mm.rx[x].frequency.ToString());
                    UpdateLabel(lab_rx3_sr, mm.rx[x].symbol_rate.ToString());
                    UpdateLabel(lab_rx3_nothing, mm.rx[x].frequency.ToString() + " MHz - " + mm.rx[x].symbol_rate.ToString() + " Ks");
                    UpdateLabel(lab_rx3_mode, mm.rx[x].modcod.ToString());
                    UpdateLabel(lab_rx3_null, mm.rx[x].null_percentage.ToString());
                    UpdateLabel(lab_rx3_mediatypes, mm.rx[x].video_type.ToString() + "-" + mm.rx[x].audio_type.ToString());
                    UpdateLabel(lab_rx3_tsaddr, mm.rx[x].ts_addr.ToString());

                    if (mm.rx[x].ts_addr.ToString() != localip)
                    {
                        UpdateLabelColor(lab_rx3_tsaddr, Color.Red);
                    }
                    else
                    {
                        UpdateLabelColor(lab_rx3_tsaddr, Color.Green);
                    }

                    statusMsg += " - RX3: " + scanstate_lookup[mm.rx[x].scanstate].ToString();

                    if (mm.rx[x].scanstate == 2 || mm.rx[x].scanstate == 3)
                    {
                        statusMsg += " - " + mm.rx[x].service_name.ToString() + " " + mm.rx[x].mer.ToString() + " dB Mer," + "D" + mm.rx[x].dbmargin.ToString();
                    }

                }
                if (mm.rx[x].rx == 4)
                {
                    int newState = mm.rx[x].scanstate;

                    if (newState != rx4PrevState)
                    {
                        if (newState == 2 || newState == 3)
                        {
                            videoRx4.MediaPlayer.Play(rx4Media);
                            UpdateLabelColor(lab_rx4_scanstate, Color.Green);
                            UpdateVisiblity(nolock_rx4, false);
                            UpdateLabelVisiblity(lab_rx4_nothing, false);

                        }
                        else
                        {
                            videoRx4.MediaPlayer.Stop();
                            UpdateLabelColor(lab_rx4_scanstate, Color.Red);
                            UpdateVisiblity(nolock_rx4, true);
                            UpdateLabelVisiblity(lab_rx4_nothing, true);
                        }

                        rx4PrevState = newState;
                    }

                    UpdateLabel(lab_rx4_scanstate, scanstate_lookup[mm.rx[x].scanstate].ToString());
                    UpdateLabel(lab_rx4_service, mm.rx[x].service_name.ToString());
                    UpdateLabel(lab_rx4_provider, mm.rx[x].service_provider_name.ToString());
                    UpdateLabel(lab_rx4_mer, mm.rx[x].mer.ToString());
                    UpdateLabel(lab_rx4_dbmargin, mm.rx[x].dbmargin.ToString());
                    UpdateLabel(lab_rx4_frequency, mm.rx[x].frequency.ToString());
                    UpdateLabel(lab_rx4_sr, mm.rx[x].symbol_rate.ToString());
                    UpdateLabel(lab_rx4_nothing, mm.rx[x].frequency.ToString() + " MHz - " + mm.rx[x].symbol_rate.ToString() + " Ks");
                    UpdateLabel(lab_rx4_mode, mm.rx[x].modcod.ToString());
                    UpdateLabel(lab_rx4_null, mm.rx[x].null_percentage.ToString());
                    UpdateLabel(lab_rx4_mediatypes, mm.rx[x].video_type.ToString() + "-" + mm.rx[x].audio_type.ToString());
                    UpdateLabel(lab_rx4_tsaddr, mm.rx[x].ts_addr.ToString());

                    if (mm.rx[x].ts_addr.ToString() != localip)
                    {
                        UpdateLabelColor(lab_rx4_tsaddr, Color.Red);
                    }
                    else
                    {
                        UpdateLabelColor(lab_rx4_tsaddr, Color.Green);
                    }

                    statusMsg += " - RX4: " + scanstate_lookup[mm.rx[x].scanstate].ToString();
                    if (mm.rx[x].scanstate == 2 || mm.rx[x].scanstate == 3)
                    {
                        statusMsg += " - " + mm.rx[x].service_name.ToString() + " " + mm.rx[x].mer.ToString() + " dB Mer," + "D" + mm.rx[x].dbmargin.ToString();
                    }


                }
            }

            lblStatus.Text = statusMsg;
        }

        private void MonitorWS_OnOpen(object sender, EventArgs e)
        {
            debug("Monitor WS Open");
        }

        public mainForm()
        {
            InitializeComponent();
        }

        private void determineIP()
        {
            // get ip addresses
            debug("Get Local IP: ");

            int ipCount = 0;

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    debug(ip.ToString());
                    localip = ip.ToString();
                    ipCount += 1;
                }
            }

            if (ipCount == 0)
            {
                debug("Warning: No Local IP Address Detected");
            }
            else
            {
                if (ipCount > 1)
                {
                    debug("Warning: Multiple IP Addresses Detected for this pc");
                }
            }

        }


        private void mainForm_Load(object sender, EventArgs e)
        {

            loadSettings();

            if (forceLocalIP == false)
            {
                determineIP();
            }

            labLocalIP.Text = localip;

            connectWebSockets();
            configureVideo();

            changeVolume(1);
            changeVolume(2);
            changeVolume(3);
            changeVolume(4);

            if (enableSpectrum)
            {
                specForm = new SpectrumForm();
                specForm.setFreq += setFreq;
                butShowSpectrum.Visible = true;
            }

            // load chat window
            if (enableChat)
            {
                chatForm = new wbchat();
                btn_qo100chat.Visible = true;
            }

            if(minPropertiesAtStartup)
            {
                MainSplitter.Panel2Collapsed = true;
                MainSplitter.Panel2.Hide();
            }

        }

        private void updateTS(int rx)
        {
            controlWS.Send("U" + rx.ToString() + "," + localip);

            switch(rx)
            {
                case 1: rx1PrevState= -1; break;
                case 2: rx2PrevState = -1; break;
                case 3: rx3PrevState = -1; break;
                case 4: rx4PrevState = -1; break;
            }
        }

        private void resetTS(int rx)
        {
            controlWS.Send("U" + rx.ToString() + ",127.0.0.1");

            switch (rx)
            {
                case 1: rx1PrevState = -1; break;
                case 2: rx2PrevState = -1; break;
                case 3: rx3PrevState = -1; break;
                case 4: rx4PrevState = -1; break;
            }
        }


        private void butSnapshotRx1_Click(object sender, EventArgs e)
        {
            TakeSnapshot(1);
        }

        private void butSnapshotRx2_Click(object sender, EventArgs e)
        {
            TakeSnapshot(2);
        }

        private void butSnapshotRx3_Click(object sender, EventArgs e)
        {
            TakeSnapshot(3);
        }

        private void butSnapshotRx4_Click(object sender, EventArgs e)
        {
            TakeSnapshot(4);
        }

        void changeVolume(int rx)
        {
            lock (_volumelock)
            {
                switch (rx)
                {
                    case 1:
                        rx1Volume = trackBar1.Value;
                        videoRx1.MediaPlayer.Volume = rx1Volume;
                        UpdateLabel(lab_rx1_vol, rx1Volume.ToString() + " %");
                        break;
                    case 2:
                        rx2Volume = trackBar2.Value;
                        videoRx2.MediaPlayer.Volume = rx2Volume;
                        UpdateLabel(lab_rx2_vol, rx2Volume.ToString() + " %");
                        break;
                    case 3:
                        rx3Volume = trackBar3.Value;
                        videoRx3.MediaPlayer.Volume = rx3Volume;
                        UpdateLabel(lab_rx3_vol, rx3Volume.ToString() + " %");
                        break;
                    case 4:
                        rx4Volume = trackBar4.Value;
                        videoRx4.MediaPlayer.Volume = rx4Volume;
                        UpdateLabel(lab_rx4_vol, rx4Volume.ToString() + " %");
                        break;
                }
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            changeVolume(1);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            changeVolume(2);
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            changeVolume(3);
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            changeVolume(4);
        }

        private void butForceTSRx1_Click(object sender, EventArgs e)
        {
            updateTS(1);
        }

        private void butForceTSRx2_Click(object sender, EventArgs e)
        {
            updateTS(2);
        }

        private void butForceTSRx3_Click(object sender, EventArgs e)
        {
            updateTS(3);
        }

        private void butForceTSRx4_Click(object sender, EventArgs e)
        {
            updateTS(4);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // save settings
            Properties.Settings.Default.winterhill_host = txtWinterhillHost.Text;
            Properties.Settings.Default.winterhill_baseport = Int32.Parse(txtWinterhillBasePort.Text);
            Properties.Settings.Default.force_local_ip = checkForceLocalIP.Checked;
            Properties.Settings.Default.force_local_ip_ip = txtLocalIP.Text;
            Properties.Settings.Default.snapshot_path = txtSnapshotPath.Text;
            Properties.Settings.Default.rx1_offset = int.Parse(txtRX1Offset.Text);
            Properties.Settings.Default.rx2_offset = int.Parse(txtRX2Offset.Text);
            Properties.Settings.Default.rx3_offset = int.Parse(txtRX3Offset.Text);
            Properties.Settings.Default.rx4_offset = int.Parse(txtRX4Offset.Text);
            Properties.Settings.Default.enable_qo100_spectrum = checkEnableSpectrum.Checked;
            Properties.Settings.Default.enable_qo100_chat = checkEnableChat.Checked;
            Properties.Settings.Default.minimize_properties = checkMinProperties.Checked;
            Properties.Settings.Default.Save();

            MessageBox.Show("You need to restart for these settings to come into effect");
        }

        private void lab_rx1_tsaddr_Click(object sender, EventArgs e)
        {
            resetTS(1);
        }

        private void lab_rx2_tsaddr_Click(object sender, EventArgs e)
        {
            resetTS(2);
        }

        private void lab_rx3_tsaddr_Click(object sender, EventArgs e)
        {
            resetTS(3);
        }

        private void lab_rx4_tsaddr_Click(object sender, EventArgs e)
        {
            resetTS(4);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.buymeacoffee.com/zr6tg/");
        }

        private void butChooseSnapshotPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK )
            {
                txtSnapshotPath.Text = fbd.SelectedPath;
            }
        }

        private void butShowSpectrum_Click(object sender, EventArgs e)
        {
            if (specForm.Visible == false)
            {
                specForm.Show();
            }
            else
            {
                specForm.Hide();
            }
        }

        bool fullScreen = false;

        private void resetFullView()
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

            MainSplitter.Panel2Collapsed = false;
            MainSplitter.Panel2.Show();
        }


        private void changeView(int rx)
        {
            if (fullScreen == true)
            {
                resetFullView();
                fullScreen = false;
            }
            else
            {
                fullScreen = true;
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

                MainSplitter.Panel2Collapsed = true;
                MainSplitter.Panel2.Hide();
            }

        }

        private void nolock_rx1_DoubleClick(object sender, EventArgs e)
        {
            changeView(1);
        }

        private void videoRx1_DoubleClick(object sender, EventArgs e)
        {
            changeView(1);
        }

        private void nolock_rx2_DoubleClick(object sender, EventArgs e)
        {
            changeView(2);
        }

        private void videoRx2_DoubleClick(object sender, EventArgs e)
        {
            changeView(2);
        }

        private void nolock_rx3_DoubleClick(object sender, EventArgs e)
        {
            changeView(3);
        }

        private void videoRx3_DoubleClick(object sender, EventArgs e)
        {
            changeView(3);
        }

        private void nolock_rx4_DoubleClick(object sender, EventArgs e)
        {
            changeView(4);
        }

        private void videoRx4_DoubleClick(object sender, EventArgs e)
        {
            changeView(4);
        }

        private void btn_qo100chat_Click(object sender, EventArgs e)
        {
            chatForm.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MainSplitter.Panel2Collapsed = true;
            MainSplitter.Panel2.Hide();
        }

        private void statusStrip1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            MainSplitter.Panel2Collapsed = false;
            MainSplitter.Panel2.Show();
        }

        private void newFrequencySelect(int rx)
        {
            freq_select freqSet = new freq_select();
            freqSet.lblCurRx.Text = rx.ToString();

            switch (rx)
            {
                case 1: 
                    freqSet.lblCurOffset.Text = Properties.Settings.Default.rx1_offset.ToString();
                    freqSet.txtNewFrequency.Text = lab_rx1_frequency.Text.ToString();
                    break;
                case 2:
                    freqSet.lblCurOffset.Text = Properties.Settings.Default.rx1_offset.ToString();
                    freqSet.txtNewFrequency.Text = lab_rx2_frequency.Text.ToString();
                    break;
                case 3:
                    freqSet.lblCurOffset.Text = Properties.Settings.Default.rx1_offset.ToString();
                    freqSet.txtNewFrequency.Text = lab_rx3_frequency.Text.ToString();
                    break;
                case 4:
                    freqSet.lblCurOffset.Text = Properties.Settings.Default.rx1_offset.ToString();
                    freqSet.txtNewFrequency.Text = lab_rx4_frequency.Text.ToString();
                    break;
            }

            double freq = 0;
            double.TryParse(freqSet.txtNewFrequency.Text, out freq);
            freq = freq * 1000;

            freqSet.txtNewFrequency.Text = Math.Round(freq).ToString();

            if ( freqSet.ShowDialog() != DialogResult.Cancel )
            {
                try
                {
                    int newFreq = Int32.Parse(freqSet.txtNewFrequency.Text);
                    int newSR = Int16.Parse(freqSet.comboNewSR.SelectedItem.ToString());

                    setFreq(rx-1, newFreq, newSR);
                }
                catch( Exception Ex )
                {
                    debug("Something went wrong:\n" + Ex.Message);
                }
                // public void setFreq(int rx, int freq, int sr)
            }
        }
        private void lab_rx1_frequency_Click(object sender, EventArgs e)
        {
            newFrequencySelect(1);
        }

        private void lab_rx2_frequency_Click(object sender, EventArgs e)
        {
            newFrequencySelect(2);
        }

        private void lab_rx3_frequency_Click(object sender, EventArgs e)
        {
            newFrequencySelect(3);
        }

        private void lab_rx4_frequency_Click(object sender, EventArgs e)
        {
            newFrequencySelect(4);
        }
    }


    [Serializable]
    public class ReceiverMessage
    {
        public int rx;
        public int scanstate;
        public string service_name;
        public string service_provider_name;
        public string mer;
        public string dbmargin;
        public string frequency;
        public string symbol_rate;
        public string modcod;
        public string null_percentage;
        public string video_type;
        public string audio_type;
        public string ts_addr;
        public string ts_port;
    }


    [Serializable]
    public class monitorMessage
    {
        public string type;
        public double timestamp;
        public ReceiverMessage[] rx;
    }

}
