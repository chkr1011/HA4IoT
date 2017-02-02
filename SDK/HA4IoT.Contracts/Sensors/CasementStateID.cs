using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class CasementStateId
    {
        public static readonly GenericComponentState Closed = new GenericComponentState("Closed");
        public static readonly GenericComponentState Open = new GenericComponentState("Open");
        public static readonly GenericComponentState Tilt = new GenericComponentState("Tilt");
    }
}
