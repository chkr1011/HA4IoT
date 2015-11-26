using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IActuator
    {
        event EventHandler<ActuatorIsEnabledChangedEventArgs>  IsEnabledChanged;

        ActuatorId Id { get; }

        bool IsEnabled { get; }
    }
}
