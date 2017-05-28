using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Hardware.I2C.I2CPortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools.Devices
{
    public class HSREL8 : CCToolsDeviceBase
    {
        public HSREL8(string id, I2CSlaveAddress i2CAddress, II2CBusService i2CBusService, ILogger log)
            : base(id, new MAX7311Driver(i2CAddress, i2CBusService), log)
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