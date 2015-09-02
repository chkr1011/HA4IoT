using System;

namespace CK.HomeAutomation.Core
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
