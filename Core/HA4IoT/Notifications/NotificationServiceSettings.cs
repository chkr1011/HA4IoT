using System;

namespace HA4IoT.Notifications
{
    public class NotificationServiceSettings
    {
        public TimeSpan InformationTimeToLive { get; set; } = TimeSpan.FromHours(8);

        public TimeSpan WarningTimeToLive { get; set; } = TimeSpan.FromHours(24);

        public TimeSpan ErrorTimeToLive { get; set; } = TimeSpan.FromHours(96);
    }
}
