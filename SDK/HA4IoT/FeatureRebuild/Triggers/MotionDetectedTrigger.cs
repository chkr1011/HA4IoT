using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.FeatureRebuild.Features;
using HA4IoT.FeatureRebuild.Status;

namespace HA4IoT.FeatureRebuild.Triggers
{
    public class MotionDetectedTrigger : TriggerBase
    {
        public MotionDetectedTrigger(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            component.ThrowIfFeatureNotSupported<DetectMotionFeature>();

            component.StatusChanged += (sender, args) =>
            {
                if (args.GetNewStatus<DetectMotionStatus>().IsMotionDetected)
                {
                    Execute();
                }
            };
        }
    }
}
