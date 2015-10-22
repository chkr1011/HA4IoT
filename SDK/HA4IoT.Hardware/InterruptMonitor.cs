using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware
{
    public class InterruptMonitor
    {
        private readonly IBinaryInput _pin;

        public InterruptMonitor(IBinaryInput pin)
        {
            if (pin == null) throw new ArgumentNullException(nameof(pin));

            _pin = pin;
        }

        public event EventHandler InterruptDetected;

        public void Poll()
        {
            if (_pin.Read() == BinaryState.Low)
            {
                InterruptDetected?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
