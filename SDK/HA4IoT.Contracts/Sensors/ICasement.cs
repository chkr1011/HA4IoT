using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface ICasement
    {
        string Id { get; }

        event EventHandler<ComponentFeatureStateChangedEventArgs> StateChanged;

        GenericComponentState GetState();

        ITrigger GetOpenedTrigger();

        ITrigger GetClosedTrigger();
    }
}