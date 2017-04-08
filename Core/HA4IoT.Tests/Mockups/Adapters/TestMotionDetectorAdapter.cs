using System;
using HA4IoT.Contracts.Adapters;

namespace HA4IoT.Tests.Mockups.Adapters
{
    public class TestMotionDetectorAdapter : IMotionDetectorAdapter
    {
        public event EventHandler MotionDetectionBegin;
        public event EventHandler MotionDetectionEnd;

        public void Begin()
        {
            MotionDetectionBegin?.Invoke(this, EventArgs.Empty);
        }

        public void End()
        {
            MotionDetectionEnd?.Invoke(this, EventArgs.Empty);
        }

        public void Invoke()
        {
            try
            {
                Begin();
            }
            finally
            {
                End();
            }           
        }
    }
}
