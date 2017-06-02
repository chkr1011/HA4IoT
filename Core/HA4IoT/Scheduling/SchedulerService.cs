using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.Threading;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scheduling;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using HA4IoT.Core;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Scheduling
{
    [ApiServiceClass(typeof(ISchedulerService))]
    public class SchedulerService : ServiceBase, ISchedulerService
    {
        private readonly List<Schedule> _schedules = new List<Schedule>();
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger _log;

        public SchedulerService(IDateTimeService dateTimeService, ILogService logService, IScriptingService scriptingService)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));

            _log = logService?.CreatePublisher(nameof(SchedulerService)) ?? throw new ArgumentNullException(nameof(logService));
            scriptingService.RegisterScriptProxy(s => new SchedulerScriptProxy(this, s));

            ThreadPoolTimer.CreatePeriodicTimer(ExecuteSchedules, TimeSpan.FromMilliseconds(250));
        }

        [ApiMethod]
        public void GetSchedules(IApiCall apiCall)
        {
            lock (_schedules)
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

            lock (_schedules)
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

            lock (_schedules)
            {
                _schedules.RemoveAll(s => s.Name.Equals(name));

                _log.Info($"Removed schedule '{name}'.");
            }
        }

        private void ExecuteSchedules(ThreadPoolTimer timer)
        {
            var now = _dateTimeService.Now;

            lock (_schedules)
            {
                for (var i = _schedules.Count - 1; i >= 0; i--)
                {
                    var schedule = _schedules[i];

                    if (schedule.Status == ScheduleStatus.Running || now < schedule.NextExecution)
                    {
                        continue;
                    }

                    Task.Run(() => TryExecuteScheduleAsync(schedule));

                    if (schedule.IsOneTimeSchedule)
                    {
                        _schedules.RemoveAt(i);
                    }
                }
            }
        }

        private async Task TryExecuteScheduleAsync(Schedule schedule)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _log.Verbose($"Executing schedule '{schedule.Name}'.");
                schedule.Status = ScheduleStatus.Running;

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
                schedule.NextExecution = schedule.LastExecution.Value + schedule.Interval;
            }
        }
    }
}
