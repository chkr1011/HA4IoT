using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Services.System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Scheduling
{
    [ApiServiceClass(typeof(ISchedulerService))]
    public class SchedulerService : ServiceBase, ISchedulerService
    {
        private readonly ITimerService _timerService;
        private readonly object _syncRoot = new object();
        private readonly List<Schedule> _schedules = new List<Schedule>();
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger _log;

        public SchedulerService(IDateTimeService dateTimeService, ITimerService timerService, ILogService logService)
        {
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));

            _log = logService?.CreatePublisher(nameof(SchedulerService)) ?? throw new ArgumentNullException(nameof(logService));

            Task.Factory.StartNew(ExecuteScheduleds, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public IDelayedAction In(TimeSpan delay, Action action)
        {
            return new DelayedAction(delay, action, _timerService);
        }

        [ApiMethod]
        public void GetSchedules(IApiCall apiCall)
        {
            lock (_syncRoot)
            {
                apiCall.Result = JObject.FromObject(_schedules);
            }
        }

        public void Register(string name, TimeSpan interval, Action action)
        {
            Register(name, interval, () =>
            {
                action();
                return Task.FromResult(0);
            });
        }

        public void Register(string name, TimeSpan interval, Func<Task> action)
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

                _log.Info($"Registerd schedule '{name}' with interval of {interval}.");
            }
        }

        public void Remove(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            lock (_syncRoot)
            {
                _schedules.RemoveAll(s => s.Name.Equals(name));

                _log.Info($"Removed schedule '{name}'.");
            }
        }

        private void ExecuteScheduleds()
        {
            while (true)
            {
                Task.Delay(100).Wait();
                var now = _dateTimeService.Now;

                lock (_syncRoot)
                {
                    for (var i = _schedules.Count - 1; i >= 0; i--)
                    {
                        var schedule = _schedules[i];

                        if (schedule.Status == ScheduleStatus.Running || now < schedule.NextExecution)
                        {
                            continue;
                        }

                        schedule.Status = ScheduleStatus.Running;
                        Task.Run(() => TryExecuteSchedule(schedule));

                        if (schedule.IsOneTimeSchedule)
                        {
                            _schedules.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private async Task TryExecuteSchedule(Schedule schedule)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _log.Verbose($"Executing schedule '{schedule.Name}'.");

                await schedule.Action();

                schedule.LastErrorMessage = null;
                schedule.Status = ScheduleStatus.Idle;
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error while executing schedule '{schedule.Name}'.");

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
