using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.I2C.I2CPortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools.Devices
{
    public class HSPE16OutputOnly : CCToolsDeviceBase, IBinaryOutputController
    {
        public HSPE16OutputOnly(string id, I2CSlaveAddress address, II2CBusService i2cBusService, IDeviceMessageBrokerService deviceMessageBrokerService, ILogger log)
            : base(id, new MAX7311Driver(address, i2cBusService), deviceMessageBrokerService, log)
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
