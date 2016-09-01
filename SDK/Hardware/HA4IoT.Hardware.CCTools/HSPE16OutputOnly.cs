using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.PortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSPE16OutputOnly : CCToolsBoardBase, IBinaryOutputController
    {
        public HSPE16OutputOnly(DeviceId id, I2CSlaveAddress address, II2CBusService i2cBus)
            : base(id, new MAX7311Driver(address, i2cBus))
        {
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            return GetPort(number);
        }

        public IBinaryOutput this[HSPE16Pin pin] => GetOutput((int)pin);
    }
}
