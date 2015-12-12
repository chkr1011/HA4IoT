using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HA4IoT.TraceViewer
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

                        string data = Encoding.ASCII.GetString(buffer);
                        OnDataReceived(remoteEndPoint, data);
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

        private void OnDataReceived(IPEndPoint remotEndPoint, string data)
        {
            List<Notification> notifications;
            if (!TryParseNotifications(remotEndPoint, data, out notifications))
            {
                return;
            }

            foreach (var notification in notifications)
            {
                NotificationReceived?.Invoke(this, new ControllerNotificationReceivedEventArguments(notification));
            }
        }

        private bool TryParseNotifications(IPEndPoint remotEndPoint, string data, out List<Notification> notifications)
        {
            try
            {
                JObject package = JObject.Parse(data);
                string type = package.Property("type").Value.ToString();
                int version = (int)package.Property("version").Value;

                notifications = new List<Notification>();
                foreach (var notification in package.Property("notifications").Value)
                {
                    var item = notification.ToObject<JObject>();

                    string typeText = item.Property("type").Value.ToString();
                    string message = item.Property("message").Value.ToString();

                    var typeValue = (NotificationType)Enum.Parse(typeof (NotificationType), typeText);                   
                    notifications.Add(new Notification(DateTime.Now, remotEndPoint.Address, typeValue, message));
                }

                return true;
            }
            catch
            {
                if (string.IsNullOrEmpty(data) || !data.StartsWith("CK.HA/1.0"))
                {
                    notifications = null;
                    return false;
                }

                Match match = _parser.Match(data);
                string type = match.Groups["type"].Value;
                string message = match.Groups["message"].Value;

                int typeID = int.Parse(type);
                NotificationType typeValue = (NotificationType)typeID;

                notifications = new List<Notification>();
                notifications.Add(new Notification(DateTime.Now, remotEndPoint.Address, typeValue, message));
                return true;
            }
        }
    }
}
