using System;

namespace HA4IoT.Contracts.Core
{
    public class TimerTickEventArgs : EventArgs
    {
        public TimeSpan ElapsedTime { get; set; }
    }
}
