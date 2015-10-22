using System;

namespace HA4IoT.Actuators.Contracts
{
    public interface IActuatorBase
    {
        event EventHandler<ActuatorIsEnabledChangedEventArgs>  IsEnabledChanged;

        string Id { get; }

        bool IsEnabled { get; }
    }
}
