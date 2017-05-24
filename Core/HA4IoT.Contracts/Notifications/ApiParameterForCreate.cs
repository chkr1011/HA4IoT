using System;

namespace HA4IoT.Contracts.Notifications
{
    public class ApiParameterForCreate
    {
        public NotificationType Type { get; set; } = NotificationType.Information;
        public string Text { get; set; }
        public TimeSpan TimeToLive { get; set; } = TimeSpan.FromMinutes(1);
    }
}
