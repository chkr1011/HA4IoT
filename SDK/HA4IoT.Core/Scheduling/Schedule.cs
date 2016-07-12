using System;

namespace HA4IoT.Core.Scheduling
{
    public class Schedule
    {
        public Schedule(string name, TimeSpan interval, Action action)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (action == null) throw new ArgumentNullException(nameof(action));

            Name = name;
            Interval = interval;
            Action = action;
        }

        public string Name { get; }
        public TimeSpan Interval { get; }
        public Action Action { get; }
        public DateTime? LastExecutionTimestamp { get; set; }
        public TimeSpan? LastExecutionDuration { get; set; }
        public string LastErrorMessage { get; set; }
    }
}
