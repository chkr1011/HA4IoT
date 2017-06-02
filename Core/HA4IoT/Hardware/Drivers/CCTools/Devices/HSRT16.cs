using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Hardware.Drivers.I2CPortExpanderDrivers;

namespace HA4IoT.Hardware.Drivers.CCTools.Devices
{
    public class HSRT16 : CCToolsDeviceBase
    {
        public HSRT16(string id, I2CSlaveAddress address, II2CBusService i2CBusService, ILogger log)
            : base(id, new MAX7311Driver(address, i2CBusService), log)
        {
            SetState(new byte[] { 0x00, 0x00 });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0 || number > 15) throw new ArgumentOutOfRangeException(nameof(number));

            return GetPort(number);
        }

        public IBinaryOutput this[HSRT16Pin pin] => GetOutput((int) pin);
    }
}