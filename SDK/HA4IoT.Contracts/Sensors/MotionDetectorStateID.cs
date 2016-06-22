using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class MotionDetectorStateId
    {
        public static readonly NamedComponentState Idle = new NamedComponentState("Idle");
        public static readonly NamedComponentState MotionDetected = new NamedComponentState("MotionDetected");
    }
}
