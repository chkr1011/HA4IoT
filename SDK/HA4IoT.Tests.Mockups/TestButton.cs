using System;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Contracts;

namespace CK.HomeAutomation.Tests.Mockups
{
    public class TestButton : IButton
    {
        public event EventHandler<ActuatorIsEnabledChangedEventArgs> IsEnabledChanged;
        public event EventHandler PressedShort;
        public event EventHandler PressedLong;
        public string Id { get; }
        public bool IsEnabled { get; }

        public void PressShort()
        {
            PressedShort?.Invoke(this, EventArgs.Empty);
        }

        public void PressLong()
        {
            PressedLong?.Invoke(this, EventArgs.Empty);
        }
    }
}
