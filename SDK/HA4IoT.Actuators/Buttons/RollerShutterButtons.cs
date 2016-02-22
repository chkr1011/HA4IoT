using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Actuators
{
    public class RollerShutterButtons : ActuatorBase<ActuatorSettings>
    {
        public RollerShutterButtons(ActuatorId id, IBinaryInput upInput, IBinaryInput downInput,
            IHttpRequestController httpApiController, ILogger logger, IHomeAutomationTimer timer) : base(id, httpApiController, logger)
        {
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            Up = new Button(new ActuatorId(id + "-up"), upInput, httpApiController, logger, timer);
            Down = new Button(new ActuatorId(id + "-down"), downInput, httpApiController, logger, timer);

            Settings = new ActuatorSettings(id, logger);
        }

        public IButton Up { get; }
        public IButton Down { get; }
    }
}