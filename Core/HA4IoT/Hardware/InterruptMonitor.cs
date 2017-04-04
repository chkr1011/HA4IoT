using System;
using System.Threading;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware
{
    public class InterruptMonitor
    {
        private readonly IBinaryInput _pin;
        private readonly ILogger _log;
        private readonly Timer _timer;

        public InterruptMonitor(IBinaryInput pin, ILogService logService)
        {
            if (pin == null) throw new ArgumentNullException(nameof(pin));
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            _pin = pin;
            _log = logService.CreatePublisher(nameof(InterruptMonitor));

            // The server-based Timer is designed for use with worker threads in a multithreaded environment.
            // Server timers can move among threads to handle the raised Elapsed event, resulting in more accuracy
            // than Windows timers in raising the event on time.
            _timer = new Timer(Poll, null, Timeout.Infinite, Timeout.Infinite);
        }

        public event EventHandler InterruptDetected;

        public void Start()
        {
            _timer.Change(0, Timeout.Infinite);
        }

        private void Poll(object state)
        {
            try
            {
                if (_pin.Read() == BinaryState.Low)
                {
                    InterruptDetected?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error while polling interrupt pin '" + _pin + "'");

                // Ensure that a persistent error will not flood the trace.
                _timer.Change(2000, Timeout.Infinite);
            }
            finally
            {
                _timer.Change(10, Timeout.Infinite);
            }
        }
    }
}
