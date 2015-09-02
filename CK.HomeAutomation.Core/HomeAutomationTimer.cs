using System;
using System.Diagnostics;
using System.Threading;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Core
{
    public class HomeAutomationTimer
    {
        private readonly INotificationHandler _notificationHandler;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly int _minIntervalDuration = 50;

        public event EventHandler<TimerTickEventArgs> Tick;

        public HomeAutomationTimer(INotificationHandler notificationHandler)
        {
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            _notificationHandler = notificationHandler;
        }

        public TimedAction In(TimeSpan dueTime)
        {
            return new TimedAction(dueTime, TimeSpan.Zero, this);
        }

        public TimedAction Every(TimeSpan interval)
        {
            return new TimedAction(interval, interval, this);
        }

        public void Run()
        {
            while (true)
            {
                int elapsedMilliseconds = (int)_stopwatch.ElapsedMilliseconds;
                if (elapsedMilliseconds < _minIntervalDuration)
                {
                    SpinWait.SpinUntil(() => _stopwatch.ElapsedMilliseconds >= 50);
                    continue;
                }

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
                _notificationHandler.Publish(NotificationType.Error, "Timer: Tick has catched an unhandled exception. {0}", exception.Message);
            }
        }
    }
}
