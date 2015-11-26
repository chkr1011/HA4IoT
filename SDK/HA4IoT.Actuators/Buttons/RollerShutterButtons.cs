using System;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Core.Timer;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class RollerShutterButtons : ActuatorBase
    {
        public RollerShutterButtons(ActuatorId id, IBinaryInput upInput, IBinaryInput downInput,
            IHttpRequestController request, INotificationHandler log, IHomeAutomationTimer timer) : base(id, request, log)
        {
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            Up = new Button(new ActuatorId(id + "-up"), upInput, request, log, timer);
            Down = new Button(new ActuatorId(id + "-down"), downInput, request, log, timer);
        }

        public IButton Up { get; }
        public IButton Down { get; }
    }
}