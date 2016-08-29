using System;

namespace HA4IoT.Contracts.Services.Notifications
{
    public class Notification
    {
        public Guid Uid { get; set; }

        public NotificationType Type { get; set; }

        public DateTime Timestamp { get; set; }

        public string Message { get; set; }

        public TimeSpan TimeToLive { get; set; }
    }
}
