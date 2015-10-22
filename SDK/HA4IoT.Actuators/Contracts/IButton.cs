using System;

namespace HA4IoT.Actuators.Contracts
{
    public interface IButton : IActuatorBase
    {
        event EventHandler PressedShort;

        event EventHandler PressedLong;
    }
}
