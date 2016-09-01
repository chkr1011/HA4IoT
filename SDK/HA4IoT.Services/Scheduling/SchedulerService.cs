using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Scheduling
{
    public class SchedulerService : ServiceBase, ISchedulerService
    {
        private readonly object _syncRoot = new object();
        private readonly List<Schedule> _schedules = new List<Schedule>();
        private readonly Timer _timer;
        private readonly ITimerService _timerService;
        private readonly IDateTimeService _dateTimeService;

        public SchedulerService(ITimerService timerService, IDateTimeService dateTimeService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _timerService = timerService;
            _dateTimeService = dateTimeService;

            _timer = new Timer(e => ExecuteSchedules(), null, -1, Timeout.Infinite);
        }

        public override void Startup()
        {
            _timer.Change(100, 0);
        }

        public TimedAction In(TimeSpan dueTime)
        {
            return new TimedAction(dueTime, TimeSpan.Zero, _timerService);
        }

        public void In(TimeSpan dueTime, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            ThreadPoolTimer.CreateTimer(p => action(), dueTime);
        }

        public void HandleApiCall(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                apiContext.Response = JObject.FromObject(_schedules);
            }
        }

        public void RegisterSchedule(string name, TimeSpan interval, Action action)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (action == null) throw new ArgumentNullException(nameof(action));

            lock (_syncRoot)
            {
                if (_schedules.Any(s => s.Name.Equals(name)))
                {
                    throw new InvalidOperationException($"Schedule with name '{name}' is already registered.");
                }

                var schedule = new Schedule(name, interval, action) { NextExecution = _dateTimeService.Now };
                _schedules.Add(schedule);

                Log.Info($"Registerd schedule '{name}' with interval of {interval}.");
            }
        }

        private void ExecuteSchedules()
        {
            lock (_syncRoot)
            {
                var deletedSchedules = new List<Schedule>();

                var now = _dateTimeService.Now;
                foreach (var schedule in _schedules)
                {
                    if (schedule.Status == ScheduleStatus.Running || now < schedule.NextExecution)
                    {
                        continue;
                    }

                    if (schedule.IsOneTimeSchedule)
                    {
                        deletedSchedules.Add(schedule);
                    }

                    schedule.Status = ScheduleStatus.Running;
                    Task.Run(() => ExecuteSchedule(schedule));
                }

                foreach (var deletedSchedule in deletedSchedules)
                {
                    _schedules.Remove(deletedSchedule);
                }

                _timer.Change(100, 0);
            }
        }

        private void ExecuteSchedule(Schedule schedule)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                Log.Verbose($"Executing schedule '{schedule.Name}'.");

                schedule.Action();
                schedule.LastErrorMessage = null;
                schedule.Status = ScheduleStatus.Idle;
            }
            catch (Exception exception)
            {
                Log.Error(exception, $"Error while executing schedule '{schedule.Name}'.");

                schedule.Status = ScheduleStatus.Faulted;
                schedule.LastErrorMessage = exception.Message;
            }
            finally
            {
                schedule.LastExecutionDuration = stopwatch.Elapsed;
                schedule.LastExecution = _dateTimeService.Now;
                schedule.NextExecution = _dateTimeService.Now + schedule.Interval;
            }
        }
    }
}
