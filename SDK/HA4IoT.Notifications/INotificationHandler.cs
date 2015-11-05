namespace HA4IoT.Notifications
{
    public interface INotificationHandler
    {
        void Publish(NotificationType type, string message, params object[] parameters);

        void PublishFrom<TSender>(TSender sender, NotificationType type, string message, params object[] parameters) where TSender : class;
    }
}
