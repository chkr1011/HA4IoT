using System;

namespace HA4IoT.Contracts.Components.Adapters
{
    public interface IButtonAdapter
    {
        event EventHandler<ButtonAdapterStateChangedEventArgs> StateChanged;
    }
}
