using System;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Adapters
{
    public class PortBasedMotionDetectorAdapter : IMotionDetectorAdapter
    {
        public PortBasedMotionDetectorAdapter(IBinaryInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            input.StateChanged += DispatchEvents;
        }

        public event EventHandler MotionDetectionBegin;

        public event EventHandler MotionDetectionEnd;

        private void DispatchEvents(object sender, BinaryStateChangedEventArgs eventArgs)
        {
            // The relay at the motion detector is awlays held to high.
            // The signal is set to false if motion is detected.
            if (eventArgs.NewState == BinaryState.Low)
            {
                MotionDetectionBegin?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MotionDetectionEnd?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
