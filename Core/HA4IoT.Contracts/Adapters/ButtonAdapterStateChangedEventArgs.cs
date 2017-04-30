using System;

namespace HA4IoT.Contracts.Adapters
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
