using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HA4IoT.Scheduling
{
    public class Schedule
    {
        public Schedule(string name, TimeSpan interval, Func<Task> action)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Action = action ?? throw new ArgumentNullException(nameof(action));

            Interval = interval;
        }

        public string Name { get; }
        public TimeSpan Interval { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ScheduleStatus Status { get; set; }
        
        public TimeSpan? LastExecutionDuration { get; set; }
        public DateTime? LastExecution { get; set; }
        public DateTime NextExecution { get; set; }
        public string LastErrorMessage { get; set; }
        public bool IsOneTimeSchedule { get; set; }

        [JsonIgnore]
        public Func<Task> Action { get; }
    }
}
