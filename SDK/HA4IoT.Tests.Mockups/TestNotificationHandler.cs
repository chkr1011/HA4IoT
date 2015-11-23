using HA4IoT.Contracts;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Notifications;

namespace HA4IoT.Tests.Mockups
{
    public class TestNotificationHandler : INotificationHandler
    {
        public void Publish(NotificationType type, string message, params object[] parameters)
        {   
        }

        public void Publish<TSender>(TSender sender, NotificationType type, string message, params object[] parameters) where TSender : class
        {
        }

        public void Info(string message, params object[] parameters)
        {
        }

        public void Info(object sender, string message, params object[] parameters)
        {
        }

        public void Warning(string message, params object[] parameters)
        {
        }

        public void Warning(object sender, string message, params object[] parameters)
        {
        }

        public void Error(string message, params object[] parameters)
        {
        }

        public void Error(object sender, string message, params object[] parameters)
        {
        }

        public void Verbose(string message, params object[] parameters)
        {
        }

        public void Verbose(object sender, string message, params object[] parameters)
        {
        }
    }
}
