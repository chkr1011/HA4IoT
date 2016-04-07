using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.Sensors
{
    public interface ICasement
    {
        string Id { get; }

        event EventHandler<StateChangedEventArgs> StateChanged;

        StateId GetState();
    }
}