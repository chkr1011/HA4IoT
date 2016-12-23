using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class MotionDetectorStateId
    {
        public static readonly ComponentState Idle = new ComponentState("Idle");
        public static readonly ComponentState MotionDetected = new ComponentState("MotionDetected");
    }
}
