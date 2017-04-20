using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.System
{
    public sealed class DelayedAction : IDelayedAction, IDisposable
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly ITimerService _timerService;
        private readonly Action _action;
        private readonly TimeSpan _delay;
        
        public DelayedAction(TimeSpan delay, Action action, ITimerService timerService)
        {
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _delay = delay;
            _timerService.Tick += CheckForTimeout;
        }

        private void CheckForTimeout(object sender, TimerTickEventArgs e)
        {
            if (_stopwatch.Elapsed < _delay)
            {
                return;
            }
            
            try
            {
                Task.Run(() => _action?.Invoke());
            }
            finally
            {
                Cancel();
            }
        }

        public void Cancel()
        {
            _timerService.Tick -= CheckForTimeout;
            _stopwatch.Stop();
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}
