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

        public event EventHandler<TimerTickEventArgs> Tick;
        
        public void Run()
        {
            var threadId = global::System.Environment.CurrentManagedThreadId;
            Log.Verbose($"Timer is running on thread {threadId}");

            while (true)
            {
                SpinWait.SpinUntil(() => _stopwatch.ElapsedMilliseconds >= 50);
                
                TimeSpan elapsedTime = _stopwatch.Elapsed;
                _stopwatch.Restart();

                InvokeTickEvent(elapsedTime);
            }
        }

        private void InvokeTickEvent(TimeSpan elapsedTime)
        {
            try
            {
                Tick?.Invoke(this, new TimerTickEventArgs(elapsedTime));
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Timer tick has catched an unhandled exception");
            }
        }
    }
}
