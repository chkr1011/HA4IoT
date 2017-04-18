using System;
using System.Diagnostics;
using System.Threading;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.System
{
    public class TimerService : ServiceBase, ITimerService
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly ILogger _log;
        // ReSharper disable once NotAccessedField.Local
        private readonly Timer _timer;

        private int _runningThreads;

        public TimerService(ILogService logService)
        {
            _log = logService?.CreatePublisher(nameof(TimerService)) ?? throw new ArgumentNullException(nameof(logService));

            _timer = new Timer(TickInternal, null, 0, 10);
        }

        public event EventHandler<TimerTickEventArgs> Tick;

        private void TickInternal(object state)
        {
            try
            {
                if (Interlocked.Increment(ref _runningThreads) > 1)
                {
                    return;
                }
                
                _stopwatch.Stop();
                var elapsedTime = _stopwatch.Elapsed;
                _stopwatch.Restart();

                Tick?.Invoke(this, new TimerTickEventArgs(elapsedTime));
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Timer tick has catched an unhandled exception.");
            }
            finally
            {
                Interlocked.Decrement(ref _runningThreads);
            }
        }
    }
}