using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.System
{
    public class DelayedAction : IDelayedAction
    {
        private readonly ITimerService _timerService;
        private readonly Action _action;
        private readonly Timeout _timeout;
        
        public DelayedAction(TimeSpan delay, Action action, ITimerService timerService)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));

            _timerService = timerService;
            _timerService.Tick += CheckForTimeout;

            _timeout = new Timeout(timerService, delay);
            _action = action;
        }

        private void CheckForTimeout(object sender, TimerTickEventArgs e)
        {
            if (!_timeout.IsElapsed)
            {
                return;
            }

            try
            {
                _action?.Invoke();
            }
            finally
            {
                Cancel();
            }
        }

        public void Cancel()
        {
            _timerService.Tick -= CheckForTimeout;
            _timeout.Stop();
        }
    }
}
