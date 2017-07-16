using System;

namespace HA4IoT.Contracts.Components.Adapters
{
    public class MotionDetectorAdapterStateChangedEventArgs : EventArgs
    {
        public MotionDetectorAdapterStateChangedEventArgs(AdapterMotionDetectionState state)
        {
            State = state;
        }

        public AdapterMotionDetectionState State { get; }
    }
}
