using System;
using System.Threading.Tasks;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Notifications;

namespace HA4IoT.Hardware
{
    public class InterruptMonitor
    {
        private readonly IBinaryInput _pin;
        private readonly INotificationHandler _notificationHandler;

        public InterruptMonitor(IBinaryInput pin, INotificationHandler notificationHandler)
        {
            if (pin == null) throw new ArgumentNullException(nameof(pin));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            _pin = pin;
            _notificationHandler = notificationHandler;
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
                    _notificationHandler.Error("Error while polling interrupt pin '" + _pin + "'. " + ex.Message);

                    // Ensure that a persistent error whill flood the trace.
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
