using System.Collections.Generic;
using Windows.Devices.Gpio;

namespace HA4IoT.Hardware.Pi2
{
    public class Pi2PortController : IBinaryOutputController, IBinaryInputController
    {
        private readonly GpioController _gpioController = GpioController.GetDefault();
        private readonly Dictionary<int, Pi2Port> _openPorts = new Dictionary<int, Pi2Port>();

        public IBinaryInput GetInput(int number)
        {
            return OpenPort(number, GpioPinDriveMode.Input);
        }

        public IBinaryOutput GetOutput(int number)
        {
            return OpenPort(number, GpioPinDriveMode.Output);
        }

        private Pi2Port OpenPort(int number, GpioPinDriveMode mode)
        {
            Pi2Port port;
            if (!_openPorts.TryGetValue(number, out port))
            {
                GpioPin pin = _gpioController.OpenPin(number, GpioSharingMode.Exclusive);
                pin.SetDriveMode(mode);

                port = new Pi2Port(pin);
                _openPorts.Add(number, port);
            }

            return port;
        }

        public void PollOpenInputPorts()
        {
            foreach (Pi2Port port in _openPorts.Values)
            {
                if (port.Pin.GetDriveMode() == GpioPinDriveMode.Input)
                {
                    ((IBinaryInput)port).Read();
                }
            }
        }
    }
}
