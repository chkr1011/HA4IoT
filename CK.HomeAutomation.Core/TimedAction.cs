using System;

namespace CK.HomeAutomation.Core
{
    public class TimedAction
    {
        private readonly TimeSpan _interval;
        private readonly HomeAutomationTimer _timer;
        private TimeSpan _timeout;
        private Action _action;
        
        public TimedAction(TimeSpan dueTime, TimeSpan interval, HomeAutomationTimer timer)
        {
            _timeout = dueTime;
            _interval = interval;

            _timer = timer;
            _timer.Tick += CheckForTimeout;
        }

        public TimedAction Do(Action action)
        {
            _action = action;
            return this;
        }

        private void CheckForTimeout(object sender, TimerTickEventArgs e)
        {
            bool isRunning = _timeout > TimeSpan.Zero;
            if (!isRunning)
            {
                return;
            }

            _timeout -= e.ElapsedTime;

            bool isElapsed = _timeout <= TimeSpan.Zero;
            if (!isElapsed)
            {
                return;
            }

            _action?.Invoke();

            if (_interval == TimeSpan.Zero)
            {
                Cancel();
            }
            else
            {
                _timeout = _interval;
            }
        }

        public void Cancel()
        {
            _timeout = TimeSpan.Zero;
            _action = null;

            _timer.Tick -= CheckForTimeout;
        }
    }
}
