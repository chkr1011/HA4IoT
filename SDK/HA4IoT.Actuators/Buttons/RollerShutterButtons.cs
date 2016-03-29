using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Actuators
{
    public class RollerShutterButtons : ActuatorBase, IRollerShutterButtons
    {
        public RollerShutterButtons(ActuatorId id, IBinaryInput upInput, IBinaryInput downInput, IHomeAutomationTimer timer) 
            : base(id)
        {
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            Up = new Button(new ActuatorId(id + "-up"), new PortBasedButtonEndpoint(upInput), timer);
            Down = new Button(new ActuatorId(id + "-down"), new PortBasedButtonEndpoint(downInput), timer);
        }

        public IButton Up { get; }
        public IButton Down { get; }
    }
}