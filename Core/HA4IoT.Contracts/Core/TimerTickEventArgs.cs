using System;

namespace HA4IoT.Contracts.Core
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
