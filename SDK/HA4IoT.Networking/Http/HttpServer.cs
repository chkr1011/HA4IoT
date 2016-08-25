using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking.WebSockets;

namespace HA4IoT.Networking.Http
{
    public sealed class HttpServer : IDisposable
    {
        private readonly StreamSocketListener _serverSocket = new StreamSocketListener();

        public HttpServer()
        {
            _serverSocket.Control.KeepAlive = true;
            _serverSocket.Control.NoDelay = true;
            _serverSocket.Control.QualityOfService = SocketQualityOfService.LowLatency;

            _serverSocket.ConnectionReceived += HandleConnection;
        }

        public void Bind(int port)
        {
            _serverSocket.BindServiceNameAsync(port.ToString()).AsTask().Wait();

            Log.Info($"Binded HTTP server to port {port}");
        }

        public event EventHandler<HttpRequestReceivedEventArgs> RequestReceived;
        public event EventHandler<WebSocketConnectedEventArgs> WebSocketConnected;

        public void Dispose()
        {
            _serverSocket.Dispose();
        }

        private void HandleConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Task.Factory.StartNew(
                () => HandleConnection(args.Socket),
                CancellationToken.None,
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Default).ConfigureAwait(false);
        }

        private void HandleConnection(StreamSocket clientSocket)
        {
            using (var clientSession = new ClientSession(clientSocket))
            {
                try
                {
                    clientSession.HttpRequestReceived += HandleHttpRequest;
                    clientSession.WebSocketConnected += HandleWebSocketConnected;

                    clientSession.Run();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("ERROR: Error while handling HTTP client requests. " + exception);
                }
            }
        }

        private void HandleWebSocketConnected(object sender, WebSocketConnectedEventArgs eventArgs)
        {
            WebSocketConnected?.Invoke(this, eventArgs);
        }

        private void HandleHttpRequest(object sender, HttpRequestReceivedEventArgs eventArgs)
        {
            var handlerCollection = RequestReceived;
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