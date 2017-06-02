using System;

namespace HA4IoT.Contracts.Core
{
    public sealed class TimerTickEventArgs : EventArgs
    {
        public TimeSpan ElapsedTime { get; set; }
    }
}
