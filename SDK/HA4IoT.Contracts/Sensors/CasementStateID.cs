using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class CasementStateId
    {
        public static readonly StatefulComponentState Closed = new StatefulComponentState("Closed");
        public static readonly StatefulComponentState Open = new StatefulComponentState("Open");
        public static readonly StatefulComponentState Tilt = new StatefulComponentState("Tilt");
    }
}
