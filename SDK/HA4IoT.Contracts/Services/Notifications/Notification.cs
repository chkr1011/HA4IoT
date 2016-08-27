using System;

namespace HA4IoT.Contracts.Services.Notifications
{
    public class Notification
    {
        public Notification()
        {
        }

        public Notification(Guid uid, NotificationType type, DateTime timestamp, string message, TimeSpan timeToLive)
        {
            Uid = uid;
            Type = type;
            Timestamp = timestamp;
            Message = message;
            TimeToLive = timeToLive;
        }

        public Guid Uid { get; }

        public NotificationType Type { get; }

        public DateTime Timestamp { get; }

        public string Message { get; }

        public TimeSpan TimeToLive { get; }
    }
}
