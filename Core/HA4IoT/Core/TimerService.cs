using System;
using System.Diagnostics;
using System.Threading;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Core
{
    public class TimerService : ServiceBase, ITimerService
    {
        private readonly TimerTickEventArgs _timerTickEventArgs = new TimerTickEventArgs();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly ILogger _log;
        // ReSharper disable once NotAccessedField.Local
        private readonly Timer _timer;
        
        private int _runningThreads;

        public TimerService(ILogService logService)
        {
            _log = logService?.CreatePublisher(nameof(TimerService)) ?? throw new ArgumentNullException(nameof(logService));

            _timer = new Timer(TickInternal, null, 0, 50);
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
                _timerTickEventArgs.ElapsedTime = _stopwatch.Elapsed;
                _stopwatch.Restart();

                if (_timerTickEventArgs.ElapsedTime.TotalMilliseconds > 1000)
                {
                    _log.Warning($"Tick took {_timerTickEventArgs.ElapsedTime.TotalMilliseconds}ms.");
                }

                Tick?.Invoke(this, _timerTickEventArgs);
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Tick has catched an unhandled exception.");
            }
            finally
            {
                Interlocked.Decrement(ref _runningThreads);
            }
        }
    }
}