using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CK.HomeAutomation.TraceViewer
{
    public class ControllerNotificationReceiver
    {
        private readonly Regex _parser = new Regex(@"^CK.HA/1.0 (?<type>[0-9]*) (?<message>[\w|\W]*)$", RegexOptions.Compiled);
        private readonly UdpClient _udpClient;
        private CancellationTokenSource _cancellationTokenSource;

        public ControllerNotificationReceiver()
        {
            _udpClient = new UdpClient(19227);
            _udpClient.EnableBroadcast = true;
        }

        public event EventHandler<ControllerNotificationReceivedEventArguments> NotificationReceived;

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

                        string package = Encoding.ASCII.GetString(buffer);
                        OnNotificationReceived(remoteEndPoint, package);
                    }
                },
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void Stop()
        {
            if (_cancellationTokenSource == null)
            {
                throw new InvalidOperationException("The Controller Notification Receiver is not running.");
            }

            _cancellationTokenSource.Cancel(false);
        }

        private void OnNotificationReceived(IPEndPoint remotEndPoint, string package)
        {
            Notification notification;
            if (TryParseNotification(remotEndPoint, package, out notification))
            {
                NotificationReceived?.Invoke(this, new ControllerNotificationReceivedEventArguments(notification));
            }
        }

        private bool TryParseNotification(IPEndPoint remotEndPoint, string package, out Notification notification)
        {
            if (string.IsNullOrEmpty(package) || !package.StartsWith("CK.HA/1.0"))
            {
                notification = null;
                return false;
            }

            Match match = _parser.Match(package);
            string type = match.Groups["type"].Value;
            string message = match.Groups["message"].Value;

            int typeID = int.Parse(type);
            NotificationType typeValue = (NotificationType)typeID;

            notification = new Notification(DateTime.Now, remotEndPoint.Address, typeValue, message);
            return true;
        }
    }
}
