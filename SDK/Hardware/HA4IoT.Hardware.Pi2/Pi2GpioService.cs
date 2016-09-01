using System;
using System.Collections.Generic;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.Pi2
{
    public class Pi2GpioService : ServiceBase, IPi2GpioService
    {
        private readonly GpioController _gpioController = GpioController.GetDefault();
        private readonly Dictionary<int, Pi2Gpio> _openPorts = new Dictionary<int, Pi2Gpio>();

        public Pi2GpioService(ITimerService timerService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));

            timerService.Tick += (s, e) => PollOpenInputPorts();
        }

        public IBinaryInput GetInput(int number)
        {
            return OpenPort(number, GpioPinDriveMode.Input);
        }

        public IBinaryOutput GetOutput(int number)
        {
            return OpenPort(number, GpioPinDriveMode.Output);
        }

        private void PollOpenInputPorts()
        {
            foreach (Pi2Gpio port in _openPorts.Values)
            {
                if (port.Pin.GetDriveMode() == GpioPinDriveMode.Input)
                {
                    ((IBinaryInput)port).Read();
                }
            }
        }

        private Pi2Gpio OpenPort(int number, GpioPinDriveMode mode)
        {
            Pi2Gpio port;
            if (!_openPorts.TryGetValue(number, out port))
            {
                GpioPin pin = _gpioController.OpenPin(number, GpioSharingMode.Exclusive);
                pin.SetDriveMode(mode);

                port = new Pi2Gpio(pin);
                _openPorts.Add(number, port);
            }

            return port;
        }
    }
}
