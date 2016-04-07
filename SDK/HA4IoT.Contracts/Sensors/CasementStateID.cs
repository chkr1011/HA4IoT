using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.Sensors
{
    public static class CasementStateId
    {
        public static readonly StateId Closed = new StateId("Closed");
        public static readonly StateId Open = new StateId("Open");
        public static readonly StateId Tilt = new StateId("Tilt");
    }
}
