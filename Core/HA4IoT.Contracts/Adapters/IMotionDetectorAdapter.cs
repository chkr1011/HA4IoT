using System;

namespace HA4IoT.Contracts.Adapters
{
    public interface IMotionDetectorAdapter
    {
        event EventHandler<MotionDetectorAdapterStateChangedEventArgs> StateChanged;

        void Refresh();
    }
}
