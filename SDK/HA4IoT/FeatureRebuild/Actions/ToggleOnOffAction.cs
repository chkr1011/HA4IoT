using System;
using HA4IoT.Contracts.Actions;
using HA4IoT.FeatureRebuild.Commands;
using HA4IoT.FeatureRebuild.Features;
using HA4IoT.FeatureRebuild.Status;

namespace HA4IoT.FeatureRebuild.Actions
{
    public class ToggleOnOffAction : IAction
    {
        private readonly Component _component;

        public ToggleOnOffAction(Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            component.ThrowIfFeatureNotSupported<TurnOnFeature>();
            component.ThrowIfFeatureNotSupported<TurnOffFeature>();

            _component = component;
        }

        public void Execute()
        {
            var isTurnedOn = _component.GetStatus<TurnOnStatus>().IsTurnedOn;
            if (isTurnedOn)
            {
                _component.InvokeCommand(new TurnOffCommand());
            }
            else
            {
                _component.InvokeCommand(new TurnOnCommand());
            }
        }
    }
}