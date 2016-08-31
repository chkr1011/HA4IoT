using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
            Task.Run(() => HandleConnection(args.Socket));
        }

        private void HandleConnection(StreamSocket clientSocket)
        {
            using (var clientSession = new ClientSession(clientSocket))
            {
                clientSession.HttpRequestReceived += HandleHttpRequest;
                clientSession.WebSocketConnected += HandleWebSocketConnected;

                try
                {
                    clientSession.Run();
                }
                catch (Exception exception)
                {
                    var comException = exception as COMException;
                    if (comException?.HResult == -2147014843)
                    {
                        return;
                    }

                    Debug.WriteLine("ERROR: Error while handling HTTP client requests. " + exception);
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