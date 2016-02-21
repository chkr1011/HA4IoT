using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HA4IoT.TraceReceiver
{
    public class TraceReceiverClient
    {
        private readonly UdpClient _udpClient;
        private CancellationTokenSource _cancellationTokenSource;

        public TraceReceiverClient()
        {
            _udpClient = new UdpClient(19227);
            _udpClient.Client.ReceiveBufferSize = 64*1024;
            _udpClient.EnableBroadcast = true;
        }

        public event EventHandler<TraceItemReceivedEventArgs> TraceItemReceived;

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(
                Receive,
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async Task Receive()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync();

                string data = Encoding.UTF8.GetString(result.Buffer);
                OnDataReceived(result.RemoteEndPoint.Address, data);
            }
        }

        public void Stop()
        {
            if (_cancellationTokenSource == null)
            {
                throw new InvalidOperationException("The Controller Notification Receiver is not running.");
            }

            _cancellationTokenSource.Cancel(false);
        }

        private void OnDataReceived(IPAddress senderAddress, string data)
        {
            IList<TraceItem> traceItems;
            if (!TryParseNotifications(data, out traceItems))
            {
                return;
            }

            foreach (var notification in traceItems)
            {
                TraceItemReceived?.Invoke(this, new TraceItemReceivedEventArgs(senderAddress, notification));
            }
        }

        private bool TryParseNotifications(string data, out IList<TraceItem> traceItems)
        {
            try
            {
                var parser = new TraceItemsParser();
                traceItems = parser.Parse(data);

                return true;
            }
            catch
            {
                traceItems = null;
                return false;
            }
        }
    }
}
