using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Tests.Mockups
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
