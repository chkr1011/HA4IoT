using System;

namespace CK.HomeAutomation.Actuators.Contracts
{
    public interface IButton
    {
        string Id { get; }

        event EventHandler PressedShort;

        event EventHandler PressedLong;
    }
}
