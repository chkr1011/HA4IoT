using System;

namespace HA4IoT.Contracts.Services.Notifications
{
    public class Notification
    {
        public Notification(Guid id, NotificationType type, DateTime timestamp, string message, TimeSpan timeToLive)
        {
            Id = id;
            Type = type;
            Timestamp = timestamp;
            Message = message;
            TimeToLive = timeToLive;
        }

        public Guid Id { get; }

        public NotificationType Type { get; }

        public DateTime Timestamp { get; }

        public string Message { get; }

        public TimeSpan TimeToLive { get; }
    }
}
