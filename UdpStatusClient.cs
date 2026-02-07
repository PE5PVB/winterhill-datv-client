using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace datvreceiver
{
    /// <summary>
    /// Event arguments for UDP status message received events.
    /// </summary>
    public class UdpStatusEventArgs : EventArgs
    {
        public string Data { get; }
        public string SourceIP { get; }
        public int SourcePort { get; }

        public UdpStatusEventArgs(string data, string sourceIP = "", int sourcePort = 0)
        {
            Data = data;
            SourceIP = sourceIP;
            SourcePort = sourcePort;
        }
    }

    /// <summary>
    /// UDP client for receiving status messages from Winterhill receiver.
    /// Supports both unicast and multicast (230.0.0.230) reception.
    /// </summary>
    public class UdpStatusClient : IDisposable
    {
        #region Events

        /// <summary>Raised when a status message is received</summary>
        public event EventHandler<UdpStatusEventArgs> OnStatusReceived;

        /// <summary>Raised when an error occurs</summary>
        public event EventHandler<Exception> OnError;

        /// <summary>Raised when client starts listening</summary>
        public event EventHandler OnConnected;

        /// <summary>Raised when client stops listening</summary>
        public event EventHandler OnDisconnected;

        #endregion

        #region Private Fields

        private UdpClient _udpClient;
        private readonly string _host;
        private readonly int _basePort;
        private int _listenPort;
        private CancellationTokenSource _cts;
        private Task _receiveTask;
        private bool _disposed = false;
        private bool _isListening = false;
        private bool _useMulticast = false;
        private IPAddress _multicastAddress;

        // Winterhill multicast address
        private static readonly IPAddress DefaultMulticastAddress = IPAddress.Parse("230.0.0.230");

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the UDP client is actively listening.
        /// </summary>
        public bool IsListening => _isListening;

        /// <summary>
        /// The port currently being listened on.
        /// </summary>
        public int ListenPort => _listenPort;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new UDP status client.
        /// </summary>
        /// <param name="host">Winterhill host address (or multicast address like 230.0.0.230)</param>
        /// <param name="basePort">Winterhill base port</param>
        public UdpStatusClient(string host, int basePort)
        {
            _host = host;
            _basePort = basePort;

            // Check if host is a multicast address
            if (IPAddress.TryParse(host, out IPAddress hostIp))
            {
                // Check if it's a multicast address (224.0.0.0 - 239.255.255.255)
                byte[] bytes = hostIp.GetAddressBytes();
                if (bytes[0] >= 224 && bytes[0] <= 239)
                {
                    _useMulticast = true;
                    _multicastAddress = hostIp;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts listening for UDP status messages.
        /// Listens on BasePort+2 (port 9902 for status messages).
        /// </summary>
        public void StartListening()
        {
            if (_isListening) return;

            // Primary port is BasePort+2 (9902) for "4 line receiver summary"
            _listenPort = _basePort + 2;

            try
            {
                _cts = new CancellationTokenSource();

                // Create UDP client - bind to any address on the status port
                _udpClient = new UdpClient();
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.ExclusiveAddressUse = false;
                _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, _listenPort));

                // Join multicast group if configured
                if (_useMulticast && _multicastAddress != null)
                {
                    try
                    {
                        _udpClient.JoinMulticastGroup(_multicastAddress);
                    }
                    catch
                    {
                        // Multicast join failed, continue anyway
                    }
                }

                // Also try joining default Winterhill multicast group (230.0.0.230)
                try
                {
                    _udpClient.JoinMulticastGroup(DefaultMulticastAddress);
                }
                catch
                {
                    // Multicast not available, continue with unicast
                }

                _isListening = true;
                OnConnected?.Invoke(this, EventArgs.Empty);
                _receiveTask = ReceiveLoopAsync();
            }
            catch (SocketException ex)
            {
                OnError?.Invoke(this, new Exception($"Could not bind to UDP port {_listenPort}: {ex.Message}"));
                _udpClient?.Dispose();
                _udpClient = null;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
                _udpClient?.Dispose();
                _udpClient = null;
            }
        }

        /// <summary>
        /// Stops listening for UDP status messages.
        /// </summary>
        public void StopListening()
        {
            if (!_isListening) return;

            try
            {
                _cts?.Cancel();

                // Leave multicast groups
                if (_udpClient != null)
                {
                    try
                    {
                        if (_useMulticast && _multicastAddress != null)
                            _udpClient.DropMulticastGroup(_multicastAddress);
                        _udpClient.DropMulticastGroup(DefaultMulticastAddress);
                    }
                    catch { }
                }

                _udpClient?.Close();
                _isListening = false;
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        #endregion

        #region Private Methods

        private async Task ReceiveLoopAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested && _isListening)
                {
                    try
                    {
                        var result = await _udpClient.ReceiveAsync();
                        string message = Encoding.UTF8.GetString(result.Buffer);

                        if (!string.IsNullOrEmpty(message))
                        {
                            string sourceIP = result.RemoteEndPoint.Address.ToString();
                            int sourcePort = result.RemoteEndPoint.Port;
                            OnStatusReceived?.Invoke(this, new UdpStatusEventArgs(message, sourceIP, sourcePort));
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // Socket was closed, exit gracefully
                        break;
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
                    {
                        // Socket was closed, exit gracefully
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                if (_isListening)
                {
                    OnError?.Invoke(this, ex);
                }
            }
            finally
            {
                _isListening = false;
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            StopListening();
            _cts?.Dispose();
            _udpClient?.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// UDP client for sending control commands to Winterhill receiver.
    /// Sends commands to the command port (BasePort + 20) for each receiver.
    /// </summary>
    public class UdpControlClient : IDisposable
    {
        #region Events

        /// <summary>Raised when an error occurs</summary>
        public event EventHandler<Exception> OnError;

        #endregion

        #region Private Fields

        private UdpClient _udpClient;
        private readonly string _host;
        private readonly int _basePort;
        private bool _disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new UDP control client.
        /// </summary>
        /// <param name="host">Winterhill host address</param>
        /// <param name="basePort">Winterhill base port</param>
        public UdpControlClient(string host, int basePort)
        {
            _host = host;
            _basePort = basePort;
            _udpClient = new UdpClient();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends a frequency/symbol rate command to a specific receiver.
        /// </summary>
        /// <param name="rx">Receiver number (1-4)</param>
        /// <param name="freqKHz">Frequency in kHz</param>
        /// <param name="symbolRate">Symbol rate in kS/s</param>
        /// <param name="offset">LNB IF offset in kHz</param>
        /// <param name="antenna">Antenna selection (A or B)</param>
        public void SendFrequencyCommand(int rx, int freqKHz, int symbolRate, int offset, string antenna = "A")
        {
            if (rx < 1 || rx > 4) return;

            try
            {
                // Build command string: [TO@WH]RCV=n,FREQ=xxxxx,OFFSET=xxxxx,SRATE=xxxx,FPLUG=A
                string command = $"[TO@WH]RCV={rx},FREQ={freqKHz},OFFSET={offset},SRATE={symbolRate},FPLUG={antenna}";

                byte[] data = Encoding.UTF8.GetBytes(command);
                int port = _basePort + 20 + rx; // Command port for each receiver

                _udpClient.Send(data, data.Length, _host, port);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// Sends a TS destination command to redirect transport stream.
        /// </summary>
        /// <param name="rx">Receiver number (1-4)</param>
        /// <param name="ipAddress">Target IP address for TS</param>
        public void SendTsDestinationCommand(int rx, string ipAddress)
        {
            if (rx < 1 || rx > 4) return;

            try
            {
                // Send command to set TS destination
                string command = $"[TO@WH]RCV={rx},TSTGT={ipAddress}";

                byte[] data = Encoding.UTF8.GetBytes(command);
                int port = _basePort + 20 + rx;

                _udpClient.Send(data, data.Length, _host, port);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// Sends a raw command string to all receivers (global command port).
        /// </summary>
        /// <param name="command">Raw command string</param>
        public void SendGlobalCommand(string command)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(command);
                int port = _basePort + 20; // Global command port

                _udpClient.Send(data, data.Length, _host, port);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// Sends a heartbeat/registration command to let Winterhill know our IP address.
        /// In anyhub mode, Winterhill only sends status to clients that have sent a command.
        /// </summary>
        /// <param name="localIP">Local IP address to register for receiving status</param>
        public void SendHeartbeat(string localIP)
        {
            try
            {
                // Send a status request to register with Winterhill
                // Format: [GLOBALMSG] or just a simple ping to let it know our IP
                string command = $"[GLOBALMSG]STATUSREQ";
                byte[] data = Encoding.UTF8.GetBytes(command);
                int port = _basePort + 20;

                _udpClient.Send(data, data.Length, _host, port);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// Sends TS destination commands for all 4 receivers to redirect streams to the local IP.
        /// This also registers the client with the Winterhill for status updates in anyhub mode.
        /// </summary>
        /// <param name="localIP">Local IP address to receive TS and status</param>
        public void RegisterClient(string localIP)
        {
            try
            {
                // Send TS destination commands for all receivers to register our IP
                for (int rx = 1; rx <= 4; rx++)
                {
                    string command = $"[TO@WH]RCV={rx},TSTGT={localIP}";
                    byte[] data = Encoding.UTF8.GetBytes(command);
                    int port = _basePort + 20 + rx;

                    _udpClient.Send(data, data.Length, _host, port);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _udpClient?.Close();
            _udpClient?.Dispose();
        }

        #endregion
    }
}
