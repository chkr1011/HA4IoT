using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware
{
    public class InterruptMonitor
    {
        private readonly IBinaryInput _pin;
        private readonly ILogger _logger;

        public InterruptMonitor(IBinaryInput pin, ILogger logger)
        {
            if (pin == null) throw new ArgumentNullException(nameof(pin));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _pin = pin;
            _logger = logger;
        }

        public event EventHandler InterruptDetected;

        public void Poll()
        {
            if (_pin.Read() == BinaryState.Low)
            {
                InterruptDetected?.Invoke(this, EventArgs.Empty);
            }
        }

        public void PollForever()
        {
            while (true)
            {
                try
                {
                    Poll();
                    Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error while polling interrupt pin '" + _pin + "'");

                    // Ensure that a persistent error will not flood the trace.
                    Task.Delay(TimeSpan.FromSeconds(2)).Wait();
                }
            }
        }

        public void StartPollingTaskAsync()
        {
            Task.Factory.StartNew(PollForever, TaskCreationOptions.LongRunning);
        }
    }
}
