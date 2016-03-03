using System;
using System.Threading;
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

        public async Task PollAsync()
        {
            while (true)
            {
                try
                {
                    if (_pin.Read() == BinaryState.Low)
                    {
                        InterruptDetected?.Invoke(this, EventArgs.Empty);
                    }

                    await Task.Delay(10);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error while polling interrupt pin '" + _pin + "'");

                    // Ensure that a persistent error will not flood the trace.
                    await Task.Delay(2000);
                }
            }
        }

        public Task StartPollingAsync()
        {
            return Task.Factory.StartNew(async () => await PollAsync(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
