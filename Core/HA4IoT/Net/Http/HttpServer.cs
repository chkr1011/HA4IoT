using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Logging;
using HA4IoT.Net.WebSockets;

namespace HA4IoT.Net.Http
{
    public sealed class HttpServer : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly StreamSocketListener _serverSocket = new StreamSocketListener();
        private readonly ILogger _log;

        public HttpServer(ILogService logService)
        {
            _log = logService?.CreatePublisher(nameof(HttpServer)) ?? throw new ArgumentNullException(nameof(logService));

            _serverSocket.Control.KeepAlive = true;
            _serverSocket.Control.NoDelay = true;

            _serverSocket.ConnectionReceived += HandleConnection;
        }

        public void Bind(int port)
        {
            _serverSocket.BindServiceNameAsync(port.ToString()).GetAwaiter().GetResult();

            _log.Info($"Binded HTTP server to port {port}");
        }

        public event EventHandler<HttpRequestReceivedEventArgs> HttpRequestReceived;
        public event EventHandler<WebSocketConnectedEventArgs> WebSocketConnected;

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _serverSocket.Dispose();
        }

        private void HandleConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Task.Run(() => HandleConnectionAsync(args.Socket), _cancellationTokenSource.Token);
        }

        private async Task HandleConnectionAsync(StreamSocket clientSocket)
        {
            using (var clientSession = new ClientSession(clientSocket, _log))
            {
                clientSession.HttpRequestReceived += HandleHttpRequest;
                clientSession.WebSocketConnected += HandleWebSocketConnected;

                try
                {
                    await clientSession.RunAsync();
                }
                catch (Exception exception)
                {
                    var comException = exception as COMException;
                    if (comException?.HResult == -2147014843)
                    {
                        return;
                    }

                    _log.Verbose("Error while handling HTTP client requests. " + exception);
                }
                finally
                {
                    clientSession.HttpRequestReceived -= HandleHttpRequest;
                    clientSession.WebSocketConnected -= WebSocketConnected;
                }
            }
        }

        private void HandleWebSocketConnected(object sender, WebSocketConnectedEventArgs eventArgs)
        {
            WebSocketConnected?.Invoke(this, eventArgs);
        }

        private void HandleHttpRequest(object sender, HttpRequestReceivedEventArgs eventArgs)
        {
            var handlerCollection = HttpRequestReceived;
            if (handlerCollection == null)
            {
                return;
            }

            foreach (var handler in handlerCollection.GetInvocationList())
            {
                handler.DynamicInvoke(this, eventArgs);
                if (eventArgs.IsHandled)
                {
                    return;
                }
            }
        }
    }
}