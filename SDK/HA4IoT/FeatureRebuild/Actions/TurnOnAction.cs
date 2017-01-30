using System;
using HA4IoT.Contracts.Actions;
using HA4IoT.FeatureRebuild.Commands;
using HA4IoT.FeatureRebuild.Features;

namespace HA4IoT.FeatureRebuild.Actions
{
    public class TurnOnAction : IAction
    {
        private readonly Component _component;

        public TurnOnAction(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            component.ThrowIfFeatureNotSupported<TurnOnFeature>();
            _component = component;
        }

        public void Execute()
        {
            _component.InvokeCommand(new TurnOnCommand());
        }
    }
}
