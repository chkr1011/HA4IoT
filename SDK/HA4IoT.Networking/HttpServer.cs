using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace HA4IoT.Networking
{
    public class HttpServer
    {
        private readonly StreamSocketListener _serverSocket = new StreamSocketListener();

        public async Task StartAsync(int port)
        {
            _serverSocket.Control.KeepAlive = true;
            _serverSocket.ConnectionReceived += HandleConnection;

            await _serverSocket.BindServiceNameAsync(port.ToString());
        }

        public event EventHandler<RequestReceivedEventArgs> RequestReceived;

        private void HandleConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Task.Factory.StartNew(() => HandleRequests(args.Socket).Wait(), TaskCreationOptions.LongRunning);
        }

        private async Task HandleRequests(StreamSocket client)
        {
            using (var clientHandler = new HttpClientHandler(client))
            {
                try
                {
                    clientHandler.RequestReceived += ForwardRequestReceivedEvents;
                    await clientHandler.HandleRequests();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("Error while handling HTTP client requests. " + exception.Message);
                }
                finally
                {
                    clientHandler.RequestReceived -= ForwardRequestReceivedEvents;
                    Debug.WriteLine("Completed handling HTTP client requests.");
                }
            }
        }

        private void ForwardRequestReceivedEvents(object sender, RequestReceivedEventArgs e)
        {
            RequestReceived?.Invoke(sender, e);
        }
    }
}