using System;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.Buttons
{
    public class EmptyButtonEndpoint : IButtonAdapter
    {
        public event EventHandler Pressed;
        public event EventHandler Released;

        public void Press()
        {
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        public void Release()
        {
            Released?.Invoke(this, EventArgs.Empty);
        }

        public void PressAndRelease()
        {
            Press();
            Release();
        }
    }
}
