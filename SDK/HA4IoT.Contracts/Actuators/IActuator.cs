using System;
using Windows.Data.Json;

namespace HA4IoT.Contracts.Actuators
{
    public interface IActuator
    {
        event EventHandler<ActuatorIsEnabledChangedEventArgs>  IsEnabledChanged;

        ActuatorId Id { get; }

        bool IsEnabled { get; }

        JsonObject GetConfigurationForApi();

        JsonObject GetStatusForApi();
    }
}
