using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class SwitchStateId
    {
        public static readonly StatefulComponentState Off = new StatefulComponentState("Off");
        public static readonly StatefulComponentState On = new StatefulComponentState("On");
    }
}
