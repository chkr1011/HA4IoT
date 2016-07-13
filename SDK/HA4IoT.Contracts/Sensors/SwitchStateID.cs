using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class SwitchStateId
    {
        public static readonly NamedComponentState Off = new NamedComponentState("Off");
        public static readonly NamedComponentState On = new NamedComponentState("On");
    }
}
