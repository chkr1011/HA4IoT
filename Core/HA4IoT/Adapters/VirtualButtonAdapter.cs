using System;
using HA4IoT.Contracts.Adapters;

namespace HA4IoT.Adapters
{
    public class VirtualButtonAdapter : IButtonAdapter
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
    }
}
