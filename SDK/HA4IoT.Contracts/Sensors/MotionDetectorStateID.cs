using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.Sensors
{
    public static class MotionDetectorStateId
    {
        public static readonly StateId Idle = new StateId("Idle");
        public static readonly StateId MotionDetected = new StateId("MotionDetected");
    }
}
