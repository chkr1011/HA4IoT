using System;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Actuators
{
    public interface ISwitch : IActuator
    {
        event EventHandler<ButtonStateChangedEventArgs> StateChanged;

        SwitchState GetState();

        ITrigger GetTurnedOnTrigger();
        ITrigger GetTurnedOffTrigger();
    }
}
