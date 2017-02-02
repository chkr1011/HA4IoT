using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class MotionDetectorStateId
    {
        public static readonly GenericComponentState Idle = new GenericComponentState("Idle");
        public static readonly GenericComponentState MotionDetected = new GenericComponentState("MotionDetected");
    }
}
