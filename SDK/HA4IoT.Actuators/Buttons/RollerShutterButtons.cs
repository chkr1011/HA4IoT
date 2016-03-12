using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public class RollerShutterButtons : ActuatorBase<ActuatorSettings>, IRollerShutterButtons
    {
        public RollerShutterButtons(ActuatorId id, IBinaryInput upInput, IBinaryInput downInput,
            IApiController apiController, ILogger logger, IHomeAutomationTimer timer) : base(id, apiController, logger)
        {
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            Up = new Button(new ActuatorId(id + "-up"), new PortBasedButtonEndpoint(upInput), apiController, logger, timer);
            Down = new Button(new ActuatorId(id + "-down"), new PortBasedButtonEndpoint(downInput), apiController, logger, timer);

            Settings = new ActuatorSettings(id, logger);
        }

        public IButton Up { get; }
        public IButton Down { get; }
    }
}