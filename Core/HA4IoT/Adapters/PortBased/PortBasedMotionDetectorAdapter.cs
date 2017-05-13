using System;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Adapters.PortBased
{
    public class PortBasedMotionDetectorAdapter : IMotionDetectorAdapter
    {
        public PortBasedMotionDetectorAdapter(IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            input.StateChanged += ForwardState;
        }

        public event EventHandler<MotionDetectorAdapterStateChangedEventArgs> StateChanged;
        
        public void Refresh()
        {
        }

        private void ForwardState(object sender, BinaryStateChangedEventArgs eventArgs)
        {
            // The relay at the motion detector is awlays held to high.
            // The signal is set to false if motion is detected.
            if (eventArgs.NewState == BinaryState.Low)
            {
                StateChanged?.Invoke(this, new MotionDetectorAdapterStateChangedEventArgs(AdapterMotionDetectionState.MotionDetected));
            }
            else
            {
                StateChanged?.Invoke(this, new MotionDetectorAdapterStateChangedEventArgs(AdapterMotionDetectionState.Idle));
            }
        }
    }
}
