using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class CasementStateId
    {
        public static readonly ComponentState Closed = new ComponentState("Closed");
        public static readonly ComponentState Open = new ComponentState("Open");
        public static readonly ComponentState Tilt = new ComponentState("Tilt");
    }
}
