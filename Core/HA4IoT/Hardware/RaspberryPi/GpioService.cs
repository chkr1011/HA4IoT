using System.Collections.Generic;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.RaspberryPi
{
    public class GpioService : ServiceBase, IGpioService
    {
        private readonly GpioController _gpioController = GpioController.GetDefault();
        private readonly Dictionary<int, GpioPort> _openPorts = new Dictionary<int, GpioPort>();

        public IBinaryInput GetInput(int number)
        {
            return OpenPort(number, GpioPinDriveMode.Input);
        }

        public IBinaryOutput GetOutput(int number)
        {
            return OpenPort(number, GpioPinDriveMode.Output);
        }

        private GpioPort OpenPort(int number, GpioPinDriveMode mode)
        {
            GpioPort port;

            lock (_openPorts)
            {   
                if (_openPorts.TryGetValue(number, out port))
                {
                    return port;
                }

                var pin = _gpioController.OpenPin(number, GpioSharingMode.Exclusive);
                pin.SetDriveMode(mode);

                port = new GpioPort(pin);
                _openPorts.Add(number, port);
            }

            return port;
        }
    }
}
