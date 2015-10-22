using System;

namespace CK.HomeAutomation.Actuators.Contracts
{
    public interface IButton : IActuatorBase
    {
        event EventHandler PressedShort;

        event EventHandler PressedLong;
    }
}
