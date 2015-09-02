using System;
using Windows.Devices.Gpio;

namespace CK.HomeAutomation.Hardware
{
    public class InterruptMonitor
    {
        private readonly GpioPin _pin;

        public InterruptMonitor(GpioPin pin, Core.HomeAutomationTimer timer)
        {
            if (pin == null) throw new ArgumentNullException(nameof(pin));
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _pin = pin;
            _pin.SetDriveMode(GpioPinDriveMode.Input);

            timer.Tick += CheckInterrupt;
        }

        public event EventHandler InterruptDetected;

        private void CheckInterrupt(object sender, Core.TimerTickEventArgs e)
        {
            if (_pin.Read() == GpioPinValue.Low)
            {
                InterruptDetected?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
