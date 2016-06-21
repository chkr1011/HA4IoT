using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class CasementStateId
    {
        public static readonly NamedComponentState Closed = new NamedComponentState("Closed");
        public static readonly NamedComponentState Open = new NamedComponentState("Open");
        public static readonly NamedComponentState Tilt = new NamedComponentState("Tilt");
    }
}
