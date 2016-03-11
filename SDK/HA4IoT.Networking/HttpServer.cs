using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Networking
{
    public sealed class HttpServer : IDisposable
    {
        private readonly StreamSocketListener _serverSocket = new StreamSocketListener();

        public void Start(int port)
        {
            _serverSocket.Control.KeepAlive = true;
            _serverSocket.ConnectionReceived += HandleConnection;

            _serverSocket.BindServiceNameAsync(port.ToString()).AsTask().Wait();
        }

        public event EventHandler<HttpRequestReceivedEventArgs> RequestReceived;

        public void Dispose()
        {
            _serverSocket.Dispose();
        }

        private void HandleConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Task.Factory.StartNew(() => HandleRequests(args.Socket), CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void HandleRequests(StreamSocket client)
        {
            using (var clientHandler = new HttpClientHandler(client, HandleClientRequest))
            {
                try
                {
                    clientHandler.HandleRequests();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("ERROR: Error while handling HTTP client requests. " + exception);
                }
            }
        }

        private bool HandleClientRequest(HttpClientHandler clientHandler, HttpContext httpContext)
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