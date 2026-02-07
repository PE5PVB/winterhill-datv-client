using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace datvreceiver
{
    /// <summary>
    /// Event arguments for WebSocket message received events.
    /// </summary>
    public class WsMessageEventArgs : EventArgs
    {
        public string Data { get; }

        public WsMessageEventArgs(string data)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Event arguments for WebSocket close events.
    /// </summary>
    public class WsCloseEventArgs : EventArgs
    {
        public string Reason { get; }

        public WsCloseEventArgs(string reason)
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// WebSocket client wrapper using System.Net.WebSockets.ClientWebSocket.
    /// Provides a similar API to WebSocketSharp for easy migration.
    /// Supports .NET 8 and handles async operations properly.
    /// </summary>
    public class WebSocketClient : IDisposable
    {
        #region Events

        /// <summary>Raised when connection is established</summary>
        public event EventHandler OnOpen;

        /// <summary>Raised when a message is received</summary>
        public event EventHandler<WsMessageEventArgs> OnMessage;

        /// <summary>Raised when connection is closed</summary>
        public event EventHandler<WsCloseEventArgs> OnClose;

        /// <summary>Raised when an error occurs</summary>
        public event EventHandler<Exception> OnError;

        #endregion

        #region Private Fields

        private ClientWebSocket _webSocket;
        private readonly Uri _uri;
        private readonly string _subProtocol;
        private CancellationTokenSource _cts;
        private readonly object _sendLock = new object();
        private bool _disposed = false;
        private Task _receiveTask;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the WebSocket connection is open.
        /// </summary>
        public bool IsAlive => _webSocket?.State == WebSocketState.Open;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new WebSocket client.
        /// </summary>
        /// <param name="url">WebSocket URL (ws:// or wss://)</param>
        /// <param name="subProtocol">Optional sub-protocol</param>
        public WebSocketClient(string url, string subProtocol = null)
        {
            _uri = new Uri(url.Trim());
            _subProtocol = subProtocol;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connects to the WebSocket server asynchronously.
        /// </summary>
        public void ConnectAsync()
        {
            _ = ConnectInternalAsync();
        }

        /// <summary>
        /// Sends a text message to the server.
        /// </summary>
        public void Send(string message)
        {
            if (_webSocket?.State != WebSocketState.Open)
                return;

            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(bytes);

                lock (_sendLock)
                {
                    // Use synchronous wait for compatibility with existing code
                    _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None)
                        .GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// Closes the WebSocket connection.
        /// </summary>
        public void Close()
        {
            CloseAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Closes the WebSocket connection asynchronously.
        /// </summary>
        public async Task CloseAsync()
        {
            if (_disposed) return;

            try
            {
                _cts?.Cancel();

                if (_webSocket?.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
            }
            catch
            {
                // Ignore close errors
            }
        }

        #endregion

        #region Private Methods

        private async Task ConnectInternalAsync()
        {
            try
            {
                // Dispose previous connection if any
                _cts?.Cancel();
                _webSocket?.Dispose();

                _webSocket = new ClientWebSocket();
                _cts = new CancellationTokenSource();

                // Add sub-protocol if specified
                if (!string.IsNullOrEmpty(_subProtocol))
                {
                    _webSocket.Options.AddSubProtocol(_subProtocol);
                }

                await _webSocket.ConnectAsync(_uri, _cts.Token);

                // Notify connection opened
                OnOpen?.Invoke(this, EventArgs.Empty);

                // Start receiving messages
                _receiveTask = ReceiveLoopAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
                OnClose?.Invoke(this, new WsCloseEventArgs(ex.Message));
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[8192];

            try
            {
                while (_webSocket?.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                {
                    var messageBuilder = new StringBuilder();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            OnClose?.Invoke(this, new WsCloseEventArgs(result.CloseStatusDescription ?? "Connection closed"));
                            return;
                        }

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        }
                    }
                    while (!result.EndOfMessage);

                    if (messageBuilder.Length > 0)
                    {
                        OnMessage?.Invoke(this, new WsMessageEventArgs(messageBuilder.ToString()));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, ignore
            }
            catch (WebSocketException ex)
            {
                OnError?.Invoke(this, ex);
                OnClose?.Invoke(this, new WsCloseEventArgs(ex.Message));
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
                OnClose?.Invoke(this, new WsCloseEventArgs(ex.Message));
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cts?.Cancel();
            _cts?.Dispose();
            _webSocket?.Dispose();
        }

        #endregion
    }
}
