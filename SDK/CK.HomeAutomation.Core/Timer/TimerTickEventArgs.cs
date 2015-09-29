using System;

namespace CK.HomeAutomation.Core.Timer
{
    public class TimerTickEventArgs : EventArgs
    {
        public TimerTickEventArgs(TimeSpan elapsedTime)
        {
            ElapsedTime = elapsedTime;
        }

        public TimeSpan ElapsedTime { get; }
    }
}
