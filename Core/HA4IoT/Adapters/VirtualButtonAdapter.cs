using System;
using HA4IoT.Contracts.Adapters;

namespace HA4IoT.Adapters
{
    public class VirtualButtonAdapter : IButtonAdapter
    {
        public event EventHandler<ButtonAdapterStateChangedEventArgs> StateChanged;

        public void Press()
        {
            StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Pressed));
        }

        public void Release()
        {
            StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Released));
        }
    }
}
