using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking.Json;

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
        
        public void Dispose()
        {
            _serverSocket.Dispose();
        }

        private void HandleConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Task.Factory.StartNew(
                () => HandleRequests(args.Socket),
                CancellationToken.None,
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Default).ConfigureAwait(false);
        }

        private void HandleRequests(StreamSocket clientSocket)
        {
            using (var clientSession = new HttpClientSession(clientSocket, HandleHttpRequest, HandleWebSocketConnected))
            {
                try
                {
                    clientSession.WaitForData();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("ERROR: Error while handling HTTP client requests. " + exception);
                }
            }
        }

        private void HandleWebSocketConnected(WebSocketContext webSocketContext)
        {

            webSocketContext.SendFrame(new JsonObject().WithString("Hello12121212121212121212121212121212121212121212121212121AAAAAAAAAAAAAAA", "World56565656565656565656565656565656565656565656565BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBCCCCCCCCCC"));
        }

        private bool HandleHttpRequest(HttpContext httpContext)
        {
            var handlerCollection = RequestReceived;
            if (handlerCollection == null)
            {
                return false;
            }

            var eventArgs = new HttpRequestReceivedEventArgs(httpContext);
            foreach (var handler in handlerCollection.GetInvocationList())
            {
                handler.DynamicInvoke(this, eventArgs);
                if (eventArgs.IsHandled)
                {
                    return true;
                }
            }

            return false;
        }
    }
}