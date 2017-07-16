using System;

namespace HA4IoT.Contracts.Core
{
    public class StartupCompletedEventArgs : EventArgs
    {
        public StartupCompletedEventArgs(TimeSpan duration)
        {
            Duration = duration;
        }

        public TimeSpan Duration { get; }
    }
}
