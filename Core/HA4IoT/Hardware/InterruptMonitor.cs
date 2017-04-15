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

        private bool _isDetected;
        
        public InterruptMonitor(IBinaryInput pin, ILogService logService)
        {
            _pin = pin ?? throw new ArgumentNullException(nameof(pin));
            _log = logService?.CreatePublisher(nameof(InterruptMonitor)) ?? throw new ArgumentNullException(nameof(logService));

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
                var interruptState = _pin.Read();
                if (interruptState == BinaryState.Low && !_isDetected)
                {
                    InterruptDetected?.Invoke(this, EventArgs.Empty);
                }
                else if (interruptState == BinaryState.High)
                {
                    _isDetected = false;
                }
            }
            catch (Exception ex)
            {
                // Ensure that a persistent error will not flood the trace.
                _timer.Change(2000, Timeout.Infinite);

                _log.Error(ex, "Error while polling interrupt pin '" + _pin + "'");
            }
            finally
            {
                _timer.Change(10, Timeout.Infinite);
            }
        }
    }
}
