using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IMotionDetector : IActuator
    {
        event EventHandler<MotionDetectorStateChangedEventArgs> StateChanged;
        
        MotionDetectorState GetState();

        ITrigger GetMotionDetectedTrigger();
        ITrigger GetDetectionCompletedTrigger();
    }
}
