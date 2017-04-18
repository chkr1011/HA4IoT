using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.I2C.I2CPortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools.Devices
{
    public class HSREL5 : CCToolsDeviceBase, IBinaryOutputController
    {
        public HSREL5(string id, I2CSlaveAddress i2CAddress, II2CBusService i2CBusService, IDeviceMessageBrokerService deviceMessageBrokerService, ILogger log)
            : base(id, new PCF8574Driver(i2CAddress, i2CBusService), deviceMessageBrokerService, log)
        {
            // Ensure that all relays are off by default. The first 5 ports are hardware inverted! The other ports are not inverted but the
            // connected relays are inverted.
            SetState(new byte[] { 0xFF });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0 || number > 7) throw new ArgumentOutOfRangeException(nameof(number));

            var port = GetPort(number);
            if (number <= 4)
            {
                // The 4 relays have an hardware inverter. Invert here again to ensure that high means "on".
                return ((IBinaryOutput)port).WithInvertedState();
            }

            return port;
        }

        public IBinaryOutput this[HSREL5Pin pin] => GetOutput((int) pin);
    }
}