using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Actuators
{
    public interface IActuator : IComponent
    {
        event EventHandler<StateChangedEventArgs> ActiveStateChanged;

        StateId GetActiveState();
    }
}
