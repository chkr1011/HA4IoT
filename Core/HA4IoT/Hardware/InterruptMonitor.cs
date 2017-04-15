using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware
{
    public class InterruptMonitor
    {
        private readonly List<Action> _callbacks = new List<Action>();

        private readonly IBinaryInput _binaryInput;
        private readonly ILogger _log;
        private readonly Timer _timer;

        private bool _isDetected;

        public InterruptMonitor(IBinaryInput binaryInput, ILogService logService)
        {
            _binaryInput = binaryInput ?? throw new ArgumentNullException(nameof(binaryInput));
            _log = logService?.CreatePublisher(nameof(InterruptMonitor)) ?? throw new ArgumentNullException(nameof(logService));

            // The server-based Timer is designed for use with worker threads in a multithreaded environment.
            // Server timers can move among threads to handle the raised Elapsed event, resulting in more accuracy
            // than Windows timers in raising the event on time.
            _timer = new Timer(Poll, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void AddCallback(Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            lock (_callbacks)
            {
                _callbacks.Add(callback);
            }
        }

        public void Start()
        {
            _timer.Change(0, Timeout.Infinite);
        }

        private void Poll(object state)
        {
            try
            {
                var interruptState = _binaryInput.Read();
                if (interruptState == BinaryState.Low && !_isDetected)
                {
                    _isDetected = true;
                    _log.Info("Detected interrupt at pin '" + _binaryInput + "'.");

                    TryExecuteCallbacksAsync();
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

                _log.Error(ex, "Error while polling interrupt pin '" + _binaryInput + "'.");
            }
            finally
            {
                _timer.Change(10, Timeout.Infinite);
            }
        }

        private Task TryExecuteCallbacksAsync()
        {
            List<Action> callbacks;
            lock (_callbacks)
            {
                callbacks = new List<Action>(_callbacks);
            }

            return Task.Run(() =>
            {
                foreach (var callback in callbacks)
                {
                    TryExecuteCallback(callback);
                }
            });
        }

        private void TryExecuteCallback(Action callback)
        {
            try
            {
                callback();
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error while executing interrupt callback.");
            }
        }
    }
}
