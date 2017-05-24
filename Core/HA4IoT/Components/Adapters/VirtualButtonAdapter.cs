using System;
using HA4IoT.Contracts.Components.Adapters;

namespace HA4IoT.Components.Adapters
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
