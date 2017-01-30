using System;
using HA4IoT.Contracts.Actions;
using HA4IoT.FeatureRebuild.Commands;
using HA4IoT.FeatureRebuild.Features;

namespace HA4IoT.FeatureRebuild.Actions
{
    public class TurnOffAction : IAction
    {
        private readonly Component _component;

        public TurnOffAction(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            component.ThrowIfFeatureNotSupported<TurnOffFeature>();
            _component = component;
        }

        public void Execute()
        {
            _component.InvokeCommand(new TurnOffCommand());
        }
    }
}
