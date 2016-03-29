using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Contracts.Sensors
{
    public interface IButton : IActuator
    {
        event EventHandler<ButtonStateChangedEventArgs> StateChanged;
        
        ButtonState GetState();

        ITrigger GetPressedShortlyTrigger();
        ITrigger GetPressedLongTrigger();
    }
}
