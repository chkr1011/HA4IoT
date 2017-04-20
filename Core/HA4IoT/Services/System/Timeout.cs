using System;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.System
{
    public class Timeout
    {
        private TimeSpan _timeLeft;
        
        public Timeout(ITimerService timerService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));

            timerService.Tick += (s, e) =>
            {
                Tick(e.ElapsedTime);
            };
        }

        public event EventHandler Elapsed; 

        public TimeSpan Duration { get; private set; }

        public bool IsEnabled { get; private set; }

        public bool IsElapsed => _timeLeft == TimeSpan.Zero;

        public void Start(TimeSpan duration)
        {
            Duration = duration;
            _timeLeft = duration;

            IsEnabled = true;
        }

        public void Stop()
        {
            IsEnabled = false;
            _timeLeft = TimeSpan.Zero;
        }

        public void Restart()
        {
            Start(Duration);
        }

        private void Tick(TimeSpan elapsedTime)
        {
            if (!IsEnabled)
            {
                return;
            }

            _timeLeft -= elapsedTime;
            if (_timeLeft <= TimeSpan.Zero)
            {
                Stop();
                Elapsed?.Invoke(this, EventArgs.Empty);
            }      
        }
    }
}
