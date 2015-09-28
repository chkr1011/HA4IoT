using System;

namespace CK.HomeAutomation.TraceViewer
{
    public class ControllerNotificationReceivedEventArguments : EventArgs
    {
        public ControllerNotificationReceivedEventArguments(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            Notification = notification;
        }

        public Notification Notification { get; }
    }
}
