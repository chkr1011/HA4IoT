using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IMotionDetector : ISensor
    {
        IMotionDetectorSettings Settings { get; }
        ITrigger GetMotionDetectedTrigger();
        ITrigger GetDetectionCompletedTrigger();
    }
}
