using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Controller
{
    public sealed class DiscoveryClient : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private UdpClient _listenerSocket;
        private UdpClient _broadcastSocket;
        
        public void Start()
        {
            if (_listenerSocket != null)
            {
                throw new InvalidOperationException("The discovery client is already started.");
            }

            _broadcastSocket = new UdpClient();
            _broadcastSocket.EnableBroadcast = true;
            _broadcastSocket.Connect(IPAddress.Broadcast, 19228);

            _listenerSocket = new UdpClient(19228);
            _listenerSocket.EnableBroadcast = true;

            Task.Factory.StartNew(WaitForResponse, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Task.Factory.StartNew(SendRequests, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public event EventHandler<DiscoveryResponseReceivedEventArgs> ResponseReceived;

        private void WaitForResponse()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                IPEndPoint endPoint = null;
                byte[] buffer = _listenerSocket.Receive(ref endPoint);
                if (buffer.Length == 0)
                {
                    continue;
                }

                JObject response;
                if (TryParseResponse(buffer, out response))
                {
                    ResponseReceived?.Invoke(this, new DiscoveryResponseReceivedEventArgs(endPoint, response));
                }
            }
        }

        private bool TryParseResponse(byte[] buffer, out JObject response)
        {
            try
            {
                string json = Encoding.UTF8.GetString(buffer);
                response =JObject.Parse(json);

                return true;
            }
            catch (Exception)
            {
                response = null;
                return false;
            }
        }

        private void SendRequests()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                _broadcastSocket.Send(new byte[0], 0);
                Thread.Sleep(2500);
            }
        }
        
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();

            _broadcastSocket?.Dispose();
            _listenerSocket?.Dispose();
        }
    }
}
