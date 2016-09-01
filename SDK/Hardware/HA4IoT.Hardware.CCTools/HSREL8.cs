using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.PortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSREL8 : CCToolsBoardBase, IBinaryOutputController
    {
        public HSREL8(DeviceId id, I2CSlaveAddress i2CAddress, II2CBusService i2CBus)
            : base(id, new MAX7311Driver(i2CAddress, i2CBus))
        {
            SetState(new byte[] { 0x00, 255 });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0 || number > 15) throw new ArgumentOutOfRangeException(nameof(number));

            return GetPort(number);
        }

        public IBinaryOutput this[HSREL8Pin pin] => GetOutput((int) pin);
    }
}