using System;

namespace HA4IoT.Services.Scheduling
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
        public ScheduleStatus Status { get; set; }
        public Action Action { get; }
        public TimeSpan? LastExecutionDuration { get; set; }
        public DateTime? LastExecution { get; set; }
        public DateTime NextExecution { get; set; }
        public string LastErrorMessage { get; set; }
        public bool IsOneTimeSchedule { get; set; }
    }
}
