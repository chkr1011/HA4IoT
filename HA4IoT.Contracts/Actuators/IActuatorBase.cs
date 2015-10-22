using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IActuatorBase
    {
        event EventHandler<ActuatorIsEnabledChangedEventArgs>  IsEnabledChanged;

        string Id { get; }

        bool IsEnabled { get; }
    }
}
