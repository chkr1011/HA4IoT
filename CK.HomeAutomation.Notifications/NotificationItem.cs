namespace CK.HomeAutomation.Notifications
{
    internal sealed class NotificationItem
    {
        public NotificationItem(NotificationType type, string message)
        {
            Type = type;
            Message = message;
        }

        public NotificationType Type { get; private set; }

        public string Message { get; private set; }
    }
}
