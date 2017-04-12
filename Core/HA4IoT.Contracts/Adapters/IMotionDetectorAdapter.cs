using System;

namespace HA4IoT.Contracts.Adapters
{
    public interface IMotionDetectorAdapter
    {
        event EventHandler MotionDetectionBegin;

        event EventHandler MotionDetectionEnd;
        
        void Refresh();
    }
}
