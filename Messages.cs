using System;
using System.Collections.Generic;
using System.Globalization;

namespace datvreceiver
{
    /// <summary>
    /// Represents status data for a single Winterhill receiver channel.
    /// Populated from UDP status messages or JSON messages.
    /// </summary>
    [Serializable]
    public class ReceiverMessage
    {
        /// <summary>Receiver number (1-4)</summary>
        public int rx;

        /// <summary>
        /// Current scan state:
        /// 0 = No Signal, 1 = Init, 2 = Lock DVB-S2, 3 = Lock DVB-S,
        /// 0x80 = Lost, 0x81 = Timeout, 0x82 = Idle
        /// </summary>
        public int scanstate;

        /// <summary>Service name from transport stream (e.g., callsign)</summary>
        public string service_name;

        /// <summary>Service provider name from transport stream</summary>
        public string service_provider_name;

        /// <summary>Modulation Error Ratio in dB</summary>
        public string mer;

        /// <summary>dB margin above threshold</summary>
        public string dbmargin;

        /// <summary>Receive frequency in MHz</summary>
        public string frequency;

        /// <summary>Symbol rate in kS/s</summary>
        public string symbol_rate;

        /// <summary>Modulation and coding scheme (e.g., QPSK 1/2)</summary>
        public string modcod;

        /// <summary>Null packet percentage in transport stream</summary>
        public string null_percentage;

        /// <summary>Video codec type (e.g., H.264, H.265)</summary>
        public string video_type;

        /// <summary>Audio codec type (e.g., AAC, MP2)</summary>
        public string audio_type;

        /// <summary>IP address receiving the transport stream</summary>
        public string ts_addr;

        /// <summary>UDP port for transport stream</summary>
        public int ts_port;
    }

    /// <summary>
    /// Container for Winterhill monitor messages.
    /// Contains status data for all receiver channels.
    /// </summary>
    [Serializable]
    public class monitorMessage
    {
        /// <summary>Message type identifier</summary>
        public string type;

        /// <summary>Unix timestamp of the message</summary>
        public double timestamp;

        /// <summary>Array of receiver status messages</summary>
        public ReceiverMessage[] rx;
    }

    /// <summary>
    /// Parser for Winterhill UDP status messages.
    /// Converts text-based status lines to ReceiverMessage objects.
    /// </summary>
    /// <remarks>
    /// UDP status message format (one line per receiver):
    /// RX STATUS CALLSIGN MER D FREQUENCY SR MODULATION FEC CONST VIDEO-AUDIO ANT PACKETS %NUL NIMTYPE TS_DESTINATION
    ///
    /// Example:
    /// 1 DVB-S2 A71A 12.0 7.3 10491.500 1500 QPSK 4/5 LN20 H264-MPA TOP 441572 3.3 FTS4334L 192.168.20.109
    /// 2 lost "M0PIT" 10495.259 250 QPSK 3/4 LN35 -AAC BOT 52826 10.8 FTS4334L 192.168.20.109
    /// </remarks>
    public static class UdpMessageParser
    {
        /// <summary>
        /// Maps status strings to scan state codes.
        /// </summary>
        private static readonly Dictionary<string, int> StatusToScanState = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "DVB-S2", 2 },      // Lock DVB-S2
            { "DVB-S", 3 },       // Lock DVB-S
            { "lost", 0x80 },     // Lost
            { "timeout", 0x81 }, // Timeout
            { "idle", 0x82 },    // Idle
            { "hunting", 0 },    // No Signal / Hunting
            { "header", 1 },     // Found header / Init
        };

        /// <summary>
        /// Parses a multi-line UDP status message into a monitorMessage object.
        /// </summary>
        /// <param name="udpData">Raw UDP data containing 1-4 status lines</param>
        /// <returns>Parsed monitorMessage with receiver status data</returns>
        public static monitorMessage ParseStatusMessage(string udpData)
        {
            var result = new monitorMessage
            {
                type = "status",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                rx = new ReceiverMessage[0]
            };

            if (string.IsNullOrWhiteSpace(udpData))
                return result;

            var lines = udpData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var receivers = new List<ReceiverMessage>();

            foreach (var line in lines)
            {
                var rxMsg = ParseStatusLine(line);
                if (rxMsg != null)
                {
                    receivers.Add(rxMsg);
                }
            }

            result.rx = receivers.ToArray();
            return result;
        }

        /// <summary>
        /// Parses a single status line into a ReceiverMessage object.
        /// </summary>
        /// <param name="line">Single status line</param>
        /// <returns>Parsed ReceiverMessage or null if parsing fails</returns>
        public static ReceiverMessage ParseStatusLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            // Split line by whitespace, handling quoted strings
            var parts = SplitStatusLine(line);
            if (parts.Count < 5)
                return null;

            var rxMsg = new ReceiverMessage();

            try
            {
                int idx = 0;

                // Field 1: Receiver number (1-4)
                if (int.TryParse(parts[idx++], out int rxNum))
                    rxMsg.rx = rxNum;
                else
                    return null;

                // Field 2: Status (DVB-S2, DVB-S, lost, idle, hunting, etc.)
                string status = parts[idx++];
                rxMsg.scanstate = ParseScanState(status);

                // Field 3: Callsign/Service name (may be quoted)
                rxMsg.service_name = parts[idx++].Trim('"');

                // Field 4: MER (dB)
                rxMsg.mer = parts[idx++];

                // Field 5: D margin (dB)
                rxMsg.dbmargin = parts[idx++];

                // Field 6: Frequency (MHz)
                if (idx < parts.Count)
                    rxMsg.frequency = parts[idx++];

                // Field 7: Symbol Rate (kS/s)
                if (idx < parts.Count)
                    rxMsg.symbol_rate = parts[idx++];

                // Field 8: Modulation (QPSK, 8PSK, etc.)
                string modulation = "";
                if (idx < parts.Count)
                    modulation = parts[idx++];

                // Field 9: FEC (1/2, 2/3, 3/4, etc.)
                string fec = "";
                if (idx < parts.Count)
                    fec = parts[idx++];

                // Combine modulation and FEC
                rxMsg.modcod = $"{modulation} {fec}".Trim();

                // Field 10: Constellation/LNA gain
                if (idx < parts.Count)
                    idx++; // Skip for now

                // Field 11: Video-Audio type (H264-MPA, H265-AAC, etc.)
                if (idx < parts.Count)
                {
                    string videoAudio = parts[idx++];
                    ParseVideoAudio(videoAudio, rxMsg);
                }

                // Field 12: Antenna position (TOP, BOT)
                if (idx < parts.Count)
                    idx++; // Skip for now

                // Field 13: Packet count
                if (idx < parts.Count)
                    idx++; // Skip for now

                // Field 14: Null percentage
                if (idx < parts.Count)
                    rxMsg.null_percentage = parts[idx++];

                // Field 15: NIM type
                if (idx < parts.Count)
                    idx++; // Skip for now

                // Field 16: TS destination IP
                if (idx < parts.Count)
                {
                    rxMsg.ts_addr = parts[idx++];
                }
            }
            catch
            {
                // Parsing error, return partial result
            }

            return rxMsg;
        }

        /// <summary>
        /// Splits a status line by whitespace, preserving quoted strings.
        /// </summary>
        private static List<string> SplitStatusLine(string line)
        {
            var parts = new List<string>();
            bool inQuotes = false;
            var currentPart = new System.Text.StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    currentPart.Append(c);
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (currentPart.Length > 0)
                    {
                        parts.Add(currentPart.ToString());
                        currentPart.Clear();
                    }
                }
                else
                {
                    currentPart.Append(c);
                }
            }

            if (currentPart.Length > 0)
                parts.Add(currentPart.ToString());

            return parts;
        }

        /// <summary>
        /// Parses status string to scan state code.
        /// </summary>
        private static int ParseScanState(string status)
        {
            if (StatusToScanState.TryGetValue(status, out int state))
                return state;

            // Check for partial matches
            foreach (var kvp in StatusToScanState)
            {
                if (status.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    return kvp.Value;
            }

            return 0; // Default to No Signal
        }

        /// <summary>
        /// Parses video-audio type string (e.g., "H264-MPA", "H265-AAC").
        /// </summary>
        private static void ParseVideoAudio(string videoAudio, ReceiverMessage rxMsg)
        {
            if (string.IsNullOrEmpty(videoAudio) || videoAudio == "-")
            {
                rxMsg.video_type = "";
                rxMsg.audio_type = "";
                return;
            }

            var parts = videoAudio.Split('-');
            if (parts.Length >= 1)
            {
                rxMsg.video_type = parts[0];
            }
            if (parts.Length >= 2)
            {
                rxMsg.audio_type = parts[1];
            }
        }
    }
}
