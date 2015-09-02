using System;

namespace CK.HomeAutomation.Actuators
{
    public interface IButton
    {
        string Id { get; }

        event EventHandler PressedShort;

        event EventHandler PressedLong;
    }
}
