using System;

namespace HA4IoT.Contracts.Triggers
{
    public class TriggeredEventArgs : EventArgs
    {
        public static readonly TriggeredEventArgs Empty = new TriggeredEventArgs();
    }
}
