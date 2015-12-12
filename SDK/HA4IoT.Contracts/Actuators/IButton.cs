using System;

namespace HA4IoT.Contracts.Actuators
{
    public interface IButton : IActuator
    {
        event EventHandler PressedShort;
        event EventHandler PressedLong;

        event EventHandler<ButtonStateChangedEventArgs> StateChanged;

        ButtonState GetState();
    }
}
