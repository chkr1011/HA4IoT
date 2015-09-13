using System;
using CK.HomeAutomation.Core.Timer;
using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class RollerShutterButtons : BaseActuator
    {
        public RollerShutterButtons(string id, IBinaryInput upInput, IBinaryInput downInput,
            IHttpRequestController httpRequestController, INotificationHandler notificationHandler, IHomeAutomationTimer timer) : base(id, httpRequestController, notificationHandler)
        {
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            Up = new Button(id + "-up", upInput, httpRequestController, notificationHandler, timer);
            Down = new Button(id + "-down", downInput, httpRequestController, notificationHandler, timer);
        }

        public IButton Up { get; }
        public IButton Down { get; }
    }
}