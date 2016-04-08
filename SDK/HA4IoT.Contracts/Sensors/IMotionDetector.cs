using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IMotionDetector : ISensor
    {
        ITrigger GetMotionDetectedTrigger();
        ITrigger GetDetectionCompletedTrigger();
    }
}
