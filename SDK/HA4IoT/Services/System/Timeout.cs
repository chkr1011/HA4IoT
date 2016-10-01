using System;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Services.System
{
    public class Timeout
    {
        private TimeSpan _duration;
        private TimeSpan _timeLeft;

        public Timeout()
        {
        }

        public Timeout(TimeSpan defaultDuration)
        {
            _duration = defaultDuration;
        }

        public TimeSpan Duration => _duration;

        public bool IsRunning => _timeLeft > TimeSpan.Zero;

        public bool IsElapsed => _timeLeft == TimeSpan.Zero;

        public void Start(TimeSpan duration)
        {
            _duration = duration;
            _timeLeft = duration;
        }

        public void Restart()
        {
            _timeLeft = _duration;
        }

        public void Tick(TimerTickEventArgs timerTickEventArgs)
        {
            if (timerTickEventArgs == null) throw new ArgumentNullException(nameof(timerTickEventArgs));

            Tick(timerTickEventArgs.ElapsedTime);
        }

        public void Tick(TimeSpan elapsedTime)
        {
            _timeLeft -= elapsedTime;
            if (_timeLeft < TimeSpan.Zero)
            {
                _timeLeft = TimeSpan.Zero;
            }
        }
    }
}
