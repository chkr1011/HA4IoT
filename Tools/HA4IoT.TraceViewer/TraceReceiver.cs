using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HA4IoT.TraceViewer
{
    public class TraceReceiver
    {
        private readonly UdpClient _udpClient;
        private CancellationTokenSource _cancellationTokenSource;

        public TraceReceiver()
        {
            _udpClient = new UdpClient(19227);
            _udpClient.EnableBroadcast = true;
        }

        public event EventHandler<TraceItemReceivedEventArgs> TraceItemReceived;

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(
                () =>
                {
                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        IPEndPoint remoteEndPoint = null;
                        byte[] buffer = _udpClient.Receive(ref remoteEndPoint);

                        string data = Encoding.UTF8.GetString(buffer);
                        OnDataReceived(remoteEndPoint.Address, data);
                    }
                },
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
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
