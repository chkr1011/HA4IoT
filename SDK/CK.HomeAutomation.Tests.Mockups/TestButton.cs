using System;
using CK.HomeAutomation.Actuators.Contracts;

namespace CK.HomeAutomation.Tests.Mockups
{
    public class TestButton : IButton
    {
        public string Id { get; }
        public event EventHandler PressedShort;
        public event EventHandler PressedLong;

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
