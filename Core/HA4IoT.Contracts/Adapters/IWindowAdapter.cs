using System;

namespace HA4IoT.Contracts.Adapters
{
    public interface IWindowAdapter
    {
        event EventHandler<WindowStateChangedEventArgs> StateChanged;

        void Refresh();
    }
}
