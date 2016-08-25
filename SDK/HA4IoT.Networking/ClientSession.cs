using System;
using System.Threading;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Networking.WebSockets;
using HA4IoT.Networking.Http;
using HA4IoT.Networking.WebSockets;

namespace HA4IoT.Networking
{
    public sealed class ClientSession : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly StreamSocket _clientSocket;

        private HttpClientSession _httpClientSession;
        private WebSocketClientSession _webSocketClientSession;

        public ClientSession(StreamSocket clientSocket)
        {
            if (clientSocket == null) throw new ArgumentNullException(nameof(clientSocket));

            _clientSocket = clientSocket;

            _httpClientSession = new HttpClientSession(clientSocket);
            _httpClientSession.HttpRequestReceived += HandleHttpRequest;
            _httpClientSession.UpgradedToWebSocketSession += UpgradeToWebSocketSession;
        }

        public event EventHandler<HttpRequestReceivedEventArgs> HttpRequestReceived;
        public event EventHandler<WebSocketConnectedEventArgs> WebSocketConnected;

        public void Run()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_httpClientSession != null && _webSocketClientSession != null)
                {
                    throw new InvalidOperationException();
                }

                _httpClientSession?.WaitForRequest();
                _webSocketClientSession?.WaitForFrameAsync().Wait();
            }
        }

        public void Dispose()
        {
            _clientSocket.Dispose();
        }

        private void UpgradeToWebSocketSession(object sender, UpgradedToWebSocketSessionEventArgs eventArgs)
        {
            _httpClientSession.HttpRequestReceived -= HandleHttpRequest;
            _httpClientSession.UpgradedToWebSocketSession -= UpgradeToWebSocketSession;
            _httpClientSession = null;

            _webSocketClientSession = new WebSocketClientSession(_clientSocket);
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
        }

        private void HandleHttpRequest(object sender, HttpRequestReceivedEventArgs e)
        {
            try
            {
                HttpRequestReceived?.Invoke(this, e);
            }
            finally
            {
                if (_webSocketClientSession == null)
                {
                    if (!e.IsHandled || e.Context.Request.Headers.GetConnectionMustBeClosed())
                    {
                        _cancellationTokenSource.Cancel();
                    }
                }                
            }
        }
    }
}
