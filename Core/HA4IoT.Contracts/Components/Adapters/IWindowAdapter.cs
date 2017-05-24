using System;

namespace HA4IoT.Contracts.Components.Adapters
{
    public interface IWindowAdapter
    {
        event EventHandler<WindowStateChangedEventArgs> StateChanged;

        void Refresh();
    }
}
