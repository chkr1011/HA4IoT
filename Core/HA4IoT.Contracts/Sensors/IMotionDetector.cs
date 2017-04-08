using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IMotionDetector : IComponent
    {
        ITrigger MotionDetectedTrigger { get; }
        ITrigger MotionDetectionCompletedTrigger { get; }
    }
}
