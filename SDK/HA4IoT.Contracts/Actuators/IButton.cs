using System;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Actuators
{
    public interface IButton : IActuator
    {
        event EventHandler<ButtonStateChangedEventArgs> StateChanged;

        ButtonState GetState();

        ITrigger GetPressedShortlyTrigger();
        ITrigger GetPressedLongTrigger();
    }
}
