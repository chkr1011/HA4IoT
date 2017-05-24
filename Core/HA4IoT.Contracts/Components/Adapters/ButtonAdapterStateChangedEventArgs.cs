using System;

namespace HA4IoT.Contracts.Components.Adapters
{
    public class ButtonAdapterStateChangedEventArgs : EventArgs
    {
        public ButtonAdapterStateChangedEventArgs(AdapterButtonState state)
        {
            State = state;
        }

        public AdapterButtonState State { get; }
    }
}
