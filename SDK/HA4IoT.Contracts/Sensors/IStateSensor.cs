using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.Sensors
{
    public interface IStateValueSensor : ISensor
    {
        event EventHandler<StateChangedEventArgs> ActiveStateChanged;

        StateId GetActiveState();
    }
}
