namespace HA4IoT.Contracts.Notifications
{
    public interface INotificationHandler
    {
        void Publish(NotificationType type, string message, params object[] parameters);

        void Info(string message, params object[] parameters);

        void Warning(string message, params object[] parameters);

        void Error(string message, params object[] parameters);

        void Verbose(string message, params object[] parameters);
    }
}
