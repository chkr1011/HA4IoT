using System;

namespace HA4IoT.Contracts.Adapters
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
