using System.Collections.Generic;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.Pi2
{
    public class Pi2PortController : IBinaryOutputController, IBinaryInputController
    {
        private readonly GpioController _gpioController = GpioController.GetDefault();
        private readonly Dictionary<int, Pi2Port> _openPorts = new Dictionary<int, Pi2Port>();

        public DeviceId Id { get; } = new DeviceId("Pi2.GPIOs");

        public IBinaryInput GetInput(int number)
        {
            return OpenPort(number, GpioPinDriveMode.Input);
        }

        public IBinaryOutput GetOutput(int number)
        {
            return OpenPort(number, GpioPinDriveMode.Output);
        }

        public void HandleApiCommand(IApiContext apiContext)
        {
        }

        public void HandleApiRequest(IApiContext apiContext)
        {
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
    }
}
