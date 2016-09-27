using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.PortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSREL5 : CCToolsBoardBase, IBinaryOutputController
    {
        public HSREL5(DeviceId id, I2CSlaveAddress i2CAddress, II2CBusService bus)
            : base(id, new PCF8574Driver(i2CAddress, bus))
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
                return port.WithInvertedState();
            }

            return port;
        }

        public IBinaryOutput this[HSREL5Pin pin] => GetOutput((int) pin);
    }
}