using System;

namespace HA4IoT.Core.Timer
{
    public class Timeout
    {
        private TimeSpan _timeout;

        public bool IsRunning => _timeout > TimeSpan.Zero;

        public bool IsElapsed => _timeout == TimeSpan.Zero;

        public void Start(TimeSpan duration)
        {
            _timeout = duration;
        }

        public void Tick(TimeSpan duration)
        {
            _timeout -= duration;
            if (_timeout < TimeSpan.Zero)
            {
                _timeout = TimeSpan.Zero;
            }
        }
    }
}
