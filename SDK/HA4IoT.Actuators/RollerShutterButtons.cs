using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core.Timer;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public class RollerShutterButtons : ActuatorBase
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