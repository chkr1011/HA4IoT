using System;

namespace HA4IoT.Contracts.Components.Adapters
{
    public interface IMotionDetectorAdapter
    {
        event EventHandler<MotionDetectorAdapterStateChangedEventArgs> StateChanged;

        void Refresh();
    }
}
