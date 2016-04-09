using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Sensors
{
    public static class MotionDetectorStateId
    {
        public static readonly StatefulComponentState Idle = new StatefulComponentState("Idle");
        public static readonly StatefulComponentState MotionDetected = new StatefulComponentState("MotionDetected");
    }
}
