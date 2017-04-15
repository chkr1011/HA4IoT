using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Logging;
using HA4IoT.Net.Http;
using HA4IoT.Net.WebSockets;

namespace HA4IoT.Net
{
    public sealed class ClientSession : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly StreamSocket _clientSocket;
        private readonly ILogger _log;

        private HttpClientSession _httpClientSession;
        private WebSocketClientSession _webSocketClientSession;

        public ClientSession(StreamSocket clientSocket, ILogger log)
        {
            _clientSocket = clientSocket ?? throw new ArgumentNullException(nameof(clientSocket));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _httpClientSession = new HttpClientSession(clientSocket, _cancellationTokenSource, HandleHttpRequest, UpgradeToWebSocketSession, _log);
        }

        public event EventHandler<HttpRequestReceivedEventArgs> HttpRequestReceived;
        public event EventHandler<WebSocketConnectedEventArgs> WebSocketConnected;

        public async Task RunAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_httpClientSession != null && _webSocketClientSession != null)
                {
                    throw new InvalidOperationException();
                }

                if (_httpClientSession != null)
                {
                    await _httpClientSession.WaitForRequestAsync();
                }

                if (_webSocketClientSession != null)
                {
                    await _webSocketClientSession.WaitForFrameAsync();
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _clientSocket?.Dispose();
        }

        private void UpgradeToWebSocketSession(UpgradedToWebSocketSessionEventArgs eventArgs)
        {
            _httpClientSession = null;

            _webSocketClientSession = new WebSocketClientSession(_clientSocket, _log);
            _webSocketClientSession.Closed += OnWebSocketClientSessionClosed;

            var webSocketConnectedEventArgs = new WebSocketConnectedEventArgs(eventArgs.HttpRequest, _webSocketClientSession);

            try
            {
                WebSocketConnected?.Invoke(this, webSocketConnectedEventArgs);
            }
            finally
            {
                if (!webSocketConnectedEventArgs.IsHandled)
                {
                    _cancellationTokenSource.Cancel();
                }
            }
        }

        private void OnWebSocketClientSessionClosed(object sender, EventArgs eventArgs)
        {
            _cancellationTokenSource.Cancel();
            _webSocketClientSession.Closed -= OnWebSocketClientSessionClosed;
        }

        private void HandleHttpRequest(HttpRequestReceivedEventArgs e)
        {
            try
            {
                HttpRequestReceived?.Invoke(this, e);
            }
            finally
            {
                if (_webSocketClientSession == null)
                {
                    if (!e.IsHandled || e.Context.Request.Headers.ConnectionMustBeClosed())
                    {
                        _cancellationTokenSource.Cancel();
                    }
                }                
            }
        }
    }
}
