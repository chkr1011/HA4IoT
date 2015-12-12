using System;
using System.Net;

namespace HA4IoT.TraceViewer
{
    public class Notification
    {
        public Notification(
            DateTime timestamp,
            IPAddress remoteAddress, 
            NotificationType type,
            string message)
        {
            Timestamp = timestamp;
            RemoteAddress = remoteAddress;
            Type = type;
            Message = message;
        }

        public DateTime Timestamp { get; }

        public IPAddress RemoteAddress { get; }

        public NotificationType Type { get; }

        public string Message { get; }
    }
}
