using System;

namespace CK.HomeAutomation.Actuators.Contracts
{
    public interface IActuatorBase
    {
        event EventHandler<ActuatorIsEnabledChangedEventArgs>  IsEnabledChanged;

        string Id { get; }

        bool IsEnabled { get; }
    }
}
