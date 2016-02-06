using System;
using System.Diagnostics;
using HA4IoT.Contracts.Notifications;

namespace HA4IoT.Tests.Mockups
{
    public class TestNotificationHandler : INotificationHandler
    {
        public void Publish(NotificationType type, string message, params object[] parameters)
        {
            Debug.WriteLine(type + ": " + string.Format(message, parameters));
        }

        public void Info(string message, params object[] parameters)
        {
            Publish(NotificationType.Info, message, parameters);
        }

        public void Warning(string message, params object[] parameters)
        {
            Publish(NotificationType.Warning, message, parameters);
        }

        public void Warning(Exception exception, string message, params object[] parameters)
        {
            Publish(NotificationType.Warning, message + "\r\n" + exception.Message, parameters);
        }

        public void Error(string message, params object[] parameters)
        {
            Publish(NotificationType.Error, message, parameters);
        }

        public void Error(Exception exception, string message, params object[] parameters)
        {
            Publish(NotificationType.Error, message + "\r\n" + exception.Message, parameters);
        }

        public void Verbose(string message, params object[] parameters)
        {
            Publish(NotificationType.Verbose, message, parameters);
        }
    }
}
