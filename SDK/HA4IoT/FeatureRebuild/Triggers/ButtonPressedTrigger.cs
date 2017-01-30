using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.FeatureRebuild.Features;
using HA4IoT.FeatureRebuild.Status;

namespace HA4IoT.FeatureRebuild.Triggers
{
    public class ButtonPressedTrigger : TriggerBase
    {
        public ButtonPressedTrigger(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            component.ThrowIfFeatureNotSupported<PressButtonFeature>();

            component.StatusChanged += (sender, args) =>
            {
                if (args.GetNewStatus<PressButtonStatus>().IsPressed)
                {
                    Execute();
                }
            };
        }
    }
}
